using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Drizzle4Dotnet.Cli.Models;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Schema.Migration;
using Drizzle4Dotnet.Core.Schema.Migration.Query;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.PgSql;
using Drizzle4Dotnet.MySql;
using Drizzle4Dotnet.Mssql;
using Drizzle4Dotnet.Sqlite;
using Drizzle4Dotnet.Oracle;

namespace Drizzle4Dotnet.Cli.Services;

/// <summary>
/// Supported database providers for migration generation.
/// </summary>
public enum DatabaseProvider
{
    PgSql,
    MySql,
    Mssql,
    Sqlite,
    Oracle
}

/// <summary>
/// Result of a migration generation operation.
/// </summary>
public record MigrationGenerationResult(
    string MigrationName,
    string SqlScript,
    string SqlFilePath,
    string DownSqlScript,
    string DownSqlFilePath,
    string SnapshotJson,
    string SnapshotFilePath,
    string Checksum,
    string Description
);

/// <summary>
/// Core service for generating SQL migration scripts and journal JSON
/// from ORM table schema classes.
/// </summary>
public class MigrationGenerator
{
    // Static guard to prevent duplicate reflection debug output
    private static readonly HashSet<string> ReflectionDebugPrinted = new();

    private readonly DatabaseProvider _provider;
    private readonly Type _dialectType;

    
    public static DatabaseProvider ParseCliOptionProvider(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "pgsql" or "postgres" or "postgresql" or "npgsql" => DatabaseProvider.PgSql,
            "mysql" or "mariadb" => DatabaseProvider.MySql,
            "mssql" or "sqlserver" or "sql-server" => DatabaseProvider.Mssql,
            "sqlite" or "sqlite3" => DatabaseProvider.Sqlite,
            "oracle" => DatabaseProvider.Oracle,
            _ => throw new ArgumentException(
                $"Unknown provider '{provider}'. Supported providers: pgsql, mysql, mssql, sqlite, oracle")
        };
    }
    
    private static readonly Dictionary<DatabaseProvider, Type> DialectMap = new()
    {
        [DatabaseProvider.PgSql] = typeof(PgSqlSqlDialectImpl),
        [DatabaseProvider.MySql] = typeof(MySqlSqlDialectImpl),
        [DatabaseProvider.Mssql] = typeof(MssqlSqlDialectImpl),
        [DatabaseProvider.Sqlite] = typeof(SqliteSqlDialectImpl),
        [DatabaseProvider.Oracle] = typeof(OracleSqlDialectImpl),
    };

    private static readonly Dictionary<DatabaseProvider, string> ProviderNames = new()
    {
        [DatabaseProvider.PgSql] = "pgsql",
        [DatabaseProvider.MySql] = "mysql",
        [DatabaseProvider.Mssql] = "mssql",
        [DatabaseProvider.Sqlite] = "sqlite",
        [DatabaseProvider.Oracle] = "oracle",
    };

    public MigrationGenerator(DatabaseProvider provider)
    {
        _provider = provider;
        _dialectType = DialectMap[provider];
    }

    /// <summary>
    /// Gets the provider name string (e.g., "pgsql", "mysql").
    /// </summary>
    public string ProviderName => ProviderNames[_provider];

    /// <summary>
    /// Generates a migration: creates the SQL script, snapshot JSON, and updates the journal.
    /// </summary>
    /// <param name="migrationName">Name for this migration (e.g., "v1.0.0", "add_users_table").</param>
    /// <param name="tableTypes">Fully qualified type names of ORM table classes.</param>
    /// <param name="assemblyPath">Optional path to the assembly containing the table types.
    /// If null, the executing assembly is used.</param>
    /// <param name="outputDir">Directory where migration files will be written.</param>
    /// <param name="snapshotPath">Optional path to the current snapshot JSON. If provided, will diff against it.</param>
    public MigrationGenerationResult GenerateMigration(
        string migrationName,
        IReadOnlyList<string> tableTypes,
        string? assemblyPath,
        string outputDir,
        string? snapshotPath = null)
    {
        // Ensure output directory exists
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        // Load table types and create target snapshot
        var targetSnapshot = CreateSnapshotFromTypes(migrationName, tableTypes, assemblyPath);

        // Load current snapshot if provided
        SchemaSnapshot? currentSnapshot = null;
        if (snapshotPath != null && File.Exists(snapshotPath))
        {
            var json = File.ReadAllText(snapshotPath);
            currentSnapshot = SchemaSnapshot.Deserialize(json);
        }
        else
        {
            // Try to find latest snapshot in output directory
            var snapshotFiles = Directory.GetFiles(outputDir, "*-snapshot-*.json")
                .OrderByDescending(f => f)
                .ToArray();

            if (snapshotFiles.Length > 0)
            {
                var json = File.ReadAllText(snapshotFiles[0]);
                currentSnapshot = SchemaSnapshot.Deserialize(json);
            }
        }

        // Generate migration plan by comparing current and target snapshots
        // If no current snapshot, Compare(null) treats all tables as added (first migration)
        var diff = currentSnapshot != null ? currentSnapshot.Compare(targetSnapshot) : targetSnapshot.CreateNew();
        var hasChanges = diff?.HasChanges ?? false;
        var description = diff != null ? DescribeDiff(diff) : "No schema changes detected.";
        var plan = diff?.ToMigrationPlan(migrationName) ?? new MigrationPlan(migrationName, new List<MigrationStep>());

        // If no changes detected and we have a previous snapshot, don't generate files
        if (!hasChanges && currentSnapshot != null)
        {
            return new MigrationGenerationResult(
                migrationName,
                "-- No schema changes detected.",
                "",
                "-- No schema changes detected.",
                "",
                targetSnapshot.Serialize(),
                "",
                ComputeChecksum(""),
                "No schema changes detected."
            );
        }

        // Load or create journal
        var journalPath = Path.Combine(outputDir, "migration-journal.json");
        var journal = MigrationJournal.Load(journalPath);
        journal.Provider = ProviderName;

        // Generate SQL script (up)
        var sqlScript = GenerateSqlScript(plan);
        var checksum = ComputeChecksum(sqlScript);

        // Generate down SQL script (rollback) by reversing the diff
        var downSqlScript = GenerateDownSqlScript(currentSnapshot, targetSnapshot, migrationName);

        // Generate safe filename with incremental ID
        var safeName = SanitizeFileName(migrationName);
        var incrementalId = GetNextIncrementalId(journal);
        var sqlFileName = $"{incrementalId}-{safeName}-up.sql";
        var downSqlFileName = $"{incrementalId}-{safeName}-down.sql";
        var snapshotFileName = $"{incrementalId}-snapshot-{safeName}.json";

        var sqlFilePath = Path.Combine(outputDir, sqlFileName);
        var downSqlFilePath = Path.Combine(outputDir, downSqlFileName);
        var snapshotFilePath = Path.Combine(outputDir, snapshotFileName);

        // Write SQL files
        File.WriteAllText(sqlFilePath, sqlScript);
        File.WriteAllText(downSqlFilePath, downSqlScript);

        // Write snapshot file
        var snapshotJson = targetSnapshot.Serialize();
        File.WriteAllText(snapshotFilePath, snapshotJson);

        // Always push a new entry to the journal array (each generation gets a distinct entry)
        journal.Migrations.Add(new MigrationJournalEntry
        {
            Name = migrationName,
            GeneratedAt = DateTime.UtcNow,
            Checksum = checksum,
            SqlFileName = sqlFileName,
            DownSqlFileName = downSqlFileName,
            SnapshotFileName = snapshotFileName,
            Description = description,
            IncrementalId = incrementalId
        });

        journal.Save(journalPath);

        return new MigrationGenerationResult(
            migrationName,
            sqlScript,
            sqlFilePath,
            downSqlScript,
            downSqlFilePath,
            snapshotJson,
            snapshotFilePath,
            checksum,
            description
        );
    }

    /// <summary>
    /// Generates only a schema snapshot from table types (without creating migration SQL).
    /// </summary>
    public SchemaSnapshot GenerateSnapshot(
        string snapshotName,
        IReadOnlyList<string> tableTypes,
        string? assemblyPath = null)
    {
        return CreateSnapshotFromTypes(snapshotName, tableTypes, assemblyPath);
    }

    /// <summary>
    /// Computes a SHA256 checksum of the given text.
    /// </summary>
    public static string ComputeChecksum(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Sanitizes a string for use as a filename.
    /// </summary>
    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "migration" : sanitized;
    }

    /// <summary>
    /// Computes the next incremental ID based on existing migration journal entries.
    /// IDs are zero-padded to 4 digits (e.g., "0001", "0002", "0010").
    /// </summary>
    private static string GetNextIncrementalId(MigrationJournal journal)
    {
        if (journal.Migrations.Count == 0)
            return "0001";

        // Parse existing incremental IDs to find the highest number
        var maxId = 0;
        foreach (var entry in journal.Migrations)
        {
            if (!string.IsNullOrEmpty(entry.IncrementalId) &&
                int.TryParse(entry.IncrementalId, out var parsed) &&
                parsed > maxId)
            {
                maxId = parsed;
            }
        }

        // Also scan SQL filenames for existing incremental IDs (for backward compatibility with old journal entries)
        var sqlFilePattern = new System.Text.RegularExpressions.Regex(@"^(\d+)-");
        foreach (var entry in journal.Migrations)
        {
            if (!string.IsNullOrEmpty(entry.SqlFileName))
            {
                var match = sqlFilePattern.Match(entry.SqlFileName);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var parsed) && parsed > maxId)
                {
                    maxId = parsed;
                }
            }
        }

        return (maxId + 1).ToString("D4");
    }

    /// <summary>
    /// Generates the full SQL script for a migration plan.
    /// Builds the SQL using the dialect-specific SqlBuilder.
    /// </summary>
    private string GenerateSqlScript(MigrationPlan plan)
    {
        var sqlBuilderType = typeof(SqlBuilder<>).MakeGenericType(_dialectType);

        var parts = new List<string>();
        parts.Add($"-- Migration: {plan.Name}");
        parts.Add($"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        parts.Add("");

        foreach (var step in plan.Steps)
        {
            parts.Add($"-- {step.Description}");

            // Create SqlBuilder<TDialect> instance and call ISql.BuildSql(ISqlBuilder)
            var builder = (ISqlBuilder)Activator.CreateInstance(sqlBuilderType)
                ?? throw new InvalidOperationException("Failed to create SqlBuilder instance.");

            step.Sql.BuildSql(builder);

            // Call Build() via reflection to get the (string, Dictionary) tuple
            var buildMethod = sqlBuilderType.GetMethod("Build", Type.EmptyTypes)
                ?? throw new InvalidOperationException("SqlBuilder.Build() method not found.");

            var result = buildMethod.Invoke(builder, null)
                ?? throw new InvalidOperationException("SqlBuilder.Build() returned null.");

            var sql = ((ValueTuple<string, Dictionary<string, object?>>)result).Item1;
            parts.Add(sql);
            parts.Add("");
        }

        return string.Join("\n", parts);
    }

    /// <summary>
    /// Generates the down (rollback) SQL script by comparing snapshots in reverse order.
    /// The down SQL reverses the migration: from target snapshot back to current snapshot.
    /// </summary>
    private string GenerateDownSqlScript(SchemaSnapshot? currentSnapshot, SchemaSnapshot targetSnapshot, string migrationName)
    {
        SchemaDiff? reverseDiff;

        if (currentSnapshot == null)
        {
            // First migration (no prior state): rollback means dropping all created tables
            reverseDiff = targetSnapshot.CreateRollback();
        }
        else
        {
            // Generate the reverse diff: target → current (undo the migration)
            reverseDiff = targetSnapshot.Compare(currentSnapshot);
        }

        if (!reverseDiff.HasChanges)
        {
            return $"-- Down migration: {migrationName}\n" +
                   $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                   "-- No rollback steps required.\n";
        }

        var reversePlan = reverseDiff.ToMigrationPlan($"{migrationName}_down");
        return GenerateSqlScript(reversePlan);
    }
    
    private SchemaSnapshot CreateSnapshotFromTypes(
        string snapshotName,
        IReadOnlyList<string> tableTypes,
        string? assemblyPath)
    {
        var defs = ExtractTableDefinitions(tableTypes, assemblyPath);
        return SchemaSnapshot.FromTableDefinitions(snapshotName, defs.Values.ToList());
    }

    /// <summary>
    /// Extracts TableDefinitions from the given type names.
    /// If no types are specified, auto-discovers all table types in the loaded assemblies
    /// that match the current dialect.
    /// Uses OrmSchemaExporter to extract schema via reflection.
    /// </summary>
    private Dictionary<string, TableDefinition> ExtractTableDefinitions(
        IReadOnlyList<string> tableTypes,
        string? assemblyPath)
    {
        var result = new Dictionary<string, TableDefinition>();

        // If no types specified, auto-discover all table types matching the current dialect
        var resolvedTypes = tableTypes.Count > 0
            ? tableTypes.ToList()
            : AutoDiscoverTableTypes(assemblyPath);

        if (resolvedTypes.Count == 0)
            return result;

        // Load the assembly and its dependencies from the same directory
        var loadedAssemblies = new List<Assembly>();
        
        if (!string.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath))
        {
            var mainAssembly = Assembly.LoadFrom(assemblyPath);
            loadedAssemblies.Add(mainAssembly);

            // Also load other assemblies in the same directory (dependencies)
            var dir = Path.GetDirectoryName(Path.GetFullPath(assemblyPath))!;
            foreach (var dll in Directory.GetFiles(dir, "*.dll"))
            {
                try
                {
                    var asmName = AssemblyName.GetAssemblyName(dll);
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == asmName.Name))
                        continue;
                    var asm = Assembly.LoadFrom(dll);
                    loadedAssemblies.Add(asm);
                }
                catch
                {
                    // Skip assemblies that can't be loaded
                }
            }
        }
        else
        {
            loadedAssemblies.Add(Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly());
        }

        // Include already loaded assemblies
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!loadedAssemblies.Any(a => a == asm))
                loadedAssemblies.Add(asm);
        }

        // Get OrmSchemaExporter type for reflection
        var exporterType = typeof(OrmSchemaExporter);

        foreach (var typeName in resolvedTypes)
        {
            Type? type = null;
            
            foreach (var asm in loadedAssemblies)
            {
                type = asm.GetType(typeName);
                if (type != null) break;
            }

            if (type == null)
            {
                var searchedAssemblies = string.Join(", ", loadedAssemblies.Select(a => a.GetName().Name));
                throw new InvalidOperationException(
                    $"Type '{typeName}' not found in any loaded assembly. " +
                    $"Searched assemblies: {searchedAssemblies}. " +
                    $"Ensure the assembly is referenced and the type name is correct.");
            }

            // DEBUG: Print reflection details when verbose
            PrintReflectionDebug(type, _dialectType);

            // Build the closed generic method: OrmSchemaExporter.GetTableDefinition<TTable, TDialect>()
            var method = exporterType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "GetTableDefinition" && m.IsGenericMethodDefinition &&
                                     m.GetGenericArguments().Length == 2);

            if (method == null)
                throw new InvalidOperationException(
                    "OrmSchemaExporter.GetTableDefinition<TTable, TDialect>() not found.");

            var genericMethod = method.MakeGenericMethod(type, _dialectType);
            var tableDef = genericMethod.Invoke(null, null) as TableDefinition;

            if (tableDef == null)
                throw new InvalidOperationException(
                    $"Failed to extract TableDefinition from '{typeName}'.");

            var key = $"{tableDef.SchemaName}.{tableDef.TableName}";
            result[key] = tableDef;
        }

        return result;
    }

    /// <summary>
    /// Auto-discovers all table types in the loaded assemblies that match the current dialect.
    /// Scans types with the [Table] attribute where TableAttribute.Dialect == _dialectType.
    /// Skips types with [Alias] attribute (table aliases are not real tables).
    /// </summary>
    private List<string> AutoDiscoverTableTypes(string? assemblyPath)
    {
        var discovered = new List<string>();

        // Collect all loaded assemblies (same logic as in ExtractTableDefinitions)
        var loadedAssemblies = new List<Assembly>();

        if (!string.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath))
        {
            var mainAssembly = Assembly.LoadFrom(assemblyPath);
            loadedAssemblies.Add(mainAssembly);

            var dir = Path.GetDirectoryName(Path.GetFullPath(assemblyPath))!;
            foreach (var dll in Directory.GetFiles(dir, "*.dll"))
            {
                try
                {
                    var asmName = AssemblyName.GetAssemblyName(dll);
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == asmName.Name))
                        continue;
                    var asm = Assembly.LoadFrom(dll);
                    loadedAssemblies.Add(asm);
                }
                catch
                {
                    // Skip assemblies that can't be loaded
                }
            }
        }
        else
        {
            loadedAssemblies.Add(Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly());
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!loadedAssemblies.Any(a => a == asm))
                loadedAssemblies.Add(asm);
        }

        var tableAttrType = typeof(Drizzle4Dotnet.Core.Schema.Tables.TableAttribute);
        var aliasAttrType = typeof(Drizzle4Dotnet.Core.Schema.Tables.AliasAttribute);

        foreach (var asm in loadedAssemblies)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type == null || !type.IsClass || type.IsAbstract)
                    continue;

                // Skip types with [Alias] attribute (not real tables)
                if (type.GetCustomAttribute(aliasAttrType) != null)
                    continue;

                // Check for [Table] attribute with matching dialect
                var tableAttr = type.GetCustomAttribute(tableAttrType) as Drizzle4Dotnet.Core.Schema.Tables.TableAttribute;
                if (tableAttr == null)
                    continue;

                // Check if the table's dialect matches the current provider
                if (tableAttr.Dialect != _dialectType)
                    continue;

                discovered.Add(type.FullName!);
            }
        }

        if (discovered.Count > 0)
        {
            Console.WriteLine($"  Auto-discovered: {discovered.Count} table type(s)");
            foreach (var t in discovered)
                Console.WriteLine($"    - {t}");
        }
        else
        {
            Console.WriteLine($"  Auto-discovered: 0 table type(s) for {ProviderName}");
        }

        return discovered;
    }

    /// <summary>
    /// Creates a human-readable description of the schema diff.
    /// </summary>
    private static string DescribeDiff(SchemaDiff diff)
    {
        var parts = new List<string>();

        foreach (var change in diff.TableChanges)
        {
            switch (change.ChangeType)
            {
                case TableChangeType.Added:
                    var addParts = new List<string> { $"Create table {change.SchemaName}.{change.TableName}" };
                    if (change.NewTable?.Indexes.Count > 0)
                    {
                        foreach (var idx in change.NewTable.Indexes)
                            addParts.Add($"index {idx.IndexName}({string.Join(",", idx.Columns)})");
                    }
                    if (change.NewTable?.TableConstraints.Count > 0)
                    {
                        foreach (var c in change.NewTable.TableConstraints)
                        {
                            var name = SchemaSnapshot.GetConstraintName(c);
                            addParts.Add($"constraint {(name ?? c.GetType().Name.Replace("Constraint", "").ToLower())}");
                        }
                    }
                    parts.Add(string.Join(", ", addParts));
                    break;

                case TableChangeType.Removed:
                    parts.Add($"Drop table {change.SchemaName}.{change.TableName}");
                    break;

                case TableChangeType.Modified:
                    var modParts = new List<string>
                    {
                        $"Alter table {change.SchemaName}.{change.TableName}"
                    };

                    // Column changes
                    foreach (var col in change.ColumnChanges)
                        modParts.Add($"{col.ChangeType} {col.ColumnName}");

                    // Added constraints
                    foreach (var c in change.AddedConstraints)
                    {
                        var name = SchemaSnapshot.GetConstraintName(c);
                        modParts.Add($"add constraint {(name ?? c.GetType().Name.Replace("Constraint", "").ToLower())}");
                    }

                    // Removed constraints
                    foreach (var c in change.RemovedConstraints)
                    {
                        var name = SchemaSnapshot.GetConstraintName(c);
                        modParts.Add($"drop constraint {(name ?? c.GetType().Name.Replace("Constraint", "").ToLower())}");
                    }

                    // Added indexes
                    foreach (var idx in change.AddedIndexes)
                        modParts.Add($"add index {idx.IndexName}({string.Join(",", idx.Columns)})");

                    // Removed indexes
                    foreach (var idx in change.RemovedIndexes)
                        modParts.Add($"drop index {idx.IndexName}");

                    parts.Add(string.Join(", ", modParts));
                    break;
            }
        }

        return string.Join("; ", parts);
    }

    /// <summary>
    /// Prints detailed reflection debug information for a table type.
    /// Shows assembly info, interfaces, static properties, column types, custom attributes.
    /// </summary>
    private static void PrintReflectionDebug(Type tableType, Type dialectType)
    {
        // Only print reflection debug once per type per process run
        var key = $"{tableType.FullName}@{dialectType.FullName}";
        if (!ReflectionDebugPrinted.Add(key))
            return;

        Console.WriteLine();
        Console.WriteLine($"  🔬 Reflection Debug: {tableType.FullName}");
        Console.WriteLine($"     Assembly:     {tableType.Assembly.GetName().Name}");
        Console.WriteLine($"     Dialect:      {dialectType.Name}");
        Console.WriteLine();

        // Table info
        var tableNameProp = tableType.GetProperty("TableName", BindingFlags.Public | BindingFlags.Static);
        var schemaNameProp = tableType.GetProperty("SchemaName", BindingFlags.Public | BindingFlags.Static);
        var tableName = tableNameProp?.GetValue(null)?.ToString() ?? tableType.Name;
        var schemaName = schemaNameProp?.GetValue(null)?.ToString() ?? "public";
        Console.WriteLine($"     Table:        {schemaName}.{tableName}");
        Console.WriteLine($"     Type:         {tableType.FullName}");
        Console.WriteLine();

        // Custom attributes on the table class
        var tableAttrs = tableType.GetCustomAttributes(false);
        var tableCustomAttrs = tableAttrs
            .Where(a => a.GetType().Namespace != null
                && !a.GetType().Namespace.StartsWith("System.")
                && !a.GetType().Namespace.StartsWith("Microsoft.")
                && a.GetType().Name is not "CompilerGeneratedAttribute"
                and not "NullableContextAttribute"
                and not "NullableAttribute")
            .ToList();

        // Separate indexes, constraints, and other attributes
        var indexAttrs = tableCustomAttrs
            .Where(a => a is IndexAttribute).Cast<IndexAttribute>().ToList();
        var constraintAttrs = tableCustomAttrs
            .Where(a => a is ForeignKeyConstraintAttribute
                     or UniqueConstraintAttribute
                     or PrimaryKeyTableConstraintAttribute
                     or CheckTableConstraintAttribute).ToList();
        var otherAttrs = tableCustomAttrs
            .Except(indexAttrs).Except(constraintAttrs).ToList();

        // Indexes
        Console.WriteLine($"     Indexes:       {indexAttrs.Count}");
        foreach (var idx in indexAttrs)
        {
            var details = $"\"{idx.IndexName}\" ON ({string.Join(", ", idx.Columns)})";
            if (idx.IsUnique) details += " UNIQUE";
            if (idx.IndexType != null) details += $" USING {idx.IndexType}";
            if (idx.Where != null) details += $" WHERE {idx.Where}";
            Console.WriteLine($"       [Index] {details}");
        }

        // Constraints
        Console.WriteLine($"     Constraints:   {constraintAttrs.Count}");
        foreach (var attr in constraintAttrs)
        {
            switch (attr)
            {
                case ForeignKeyConstraintAttribute fk:
                    var fkName = fk.ConstraintName != null ? $"\"{fk.ConstraintName}\" " : "";
                    Console.WriteLine($"       [ForeignKey] {fkName}({string.Join(", ", fk.Columns)}) → {fk.ForeignTable.Name}({string.Join(", ", fk.ForeignColumns)})");
                    break;
                case UniqueConstraintAttribute uc:
                    Console.WriteLine($"       [Unique] ({string.Join(", ", uc.Columns)})");
                    break;
                case PrimaryKeyTableConstraintAttribute pk:
                    Console.WriteLine($"       [PrimaryKey] ({string.Join(", ", pk.Columns)})");
                    break;
                case CheckTableConstraintAttribute cc:
                    Console.WriteLine($"       [Check] {cc.Expression}");
                    break;
            }
        }

        // Other custom attributes
        if (otherAttrs.Count > 0)
        {
            Console.WriteLine($"     Table Attributes: {otherAttrs.Count}");
            foreach (var attr in otherAttrs)
            {
                var attrName = attr.GetType().Name;
                var attrDetail = DescribeAttribute(attr);
                Console.WriteLine($"       [{attrName}]{attrDetail}");
            }
        }
        else
        {
            Console.WriteLine($"     Table Attributes: none");
        }

        // Interfaces
        var interfaces = tableType.GetInterfaces();
        Console.WriteLine($"     Interfaces:   {string.Join(", ", interfaces.Select(i => i.Name))}");
        Console.WriteLine();

        // Static properties (potential columns)
        var props = tableType.GetProperties(BindingFlags.Public | BindingFlags.Static);
        var columnProps = props.Where(p => IsColumnType(p.PropertyType)).ToList();

        Console.WriteLine($"     Static Properties: {props.Length} total, {columnProps.Count} column(s)");
        Console.WriteLine();

        foreach (var prop in props)
        {
            var propType = prop.PropertyType;
            var isColumn = columnProps.Contains(prop);

            Console.WriteLine($"     {(isColumn ? "📌" : "  ")} {prop.Name} : {GetTypeDisplayName(propType)}");

            // Print column-specific info
            if (isColumn)
            {
                // Column identifier
                var instance = prop.GetValue(null);
                if (instance != null)
                {
                    var identifierProp = propType.GetProperty("Identifier", BindingFlags.Public | BindingFlags.Instance);
                    var identifier = identifierProp?.GetValue(instance)?.ToString();
                    Console.WriteLine($"         Identifier:  {identifier ?? "(null)"}");
                }

                // CLR type from generic argument
                var clrType = GetColumnClrType(propType);
                Console.WriteLine($"         CLR Type:    {GetTypeDisplayName(clrType)}");

                // Custom attributes on the property
                var customAttrs = prop.GetCustomAttributes(false);
                if (customAttrs.Length > 0)
                {
                    Console.WriteLine($"         Attributes:  {customAttrs.Length}");
                    foreach (var attr in customAttrs)
                    {
                        var attrName = attr.GetType().Name;
                        var attrDetail = DescribeAttribute(attr);
                        Console.WriteLine($"           [{attrName}]{attrDetail}");
                    }
                }
                else
                {
                    Console.WriteLine($"         Attributes:  none");
                }

                // SqlTypeAttribute check
                var sqlTypeAttr = prop.GetCustomAttributes(false)
                    .FirstOrDefault(a => a.GetType().IsSubclassOf(typeof(SqlTypeAttribute)) 
                                        || a.GetType() == typeof(SqlTypeAttribute));
                
                if (sqlTypeAttr != null)
                {
                    var stAttr = (SqlTypeAttribute)sqlTypeAttr;
                    var sqlType = stAttr.ToSqlDataType();
                    Console.WriteLine($"         SQL Type:    {sqlType.Sql} (from [{sqlTypeAttr.GetType().Name}])");
                }
                else
                {
                    // Will use CLR-to-SQL mapping
                    var mappedSql = MapClrToSqlType(clrType, dialectType);
                    Console.WriteLine($"         SQL Type:    {mappedSql} (mapped from CLR)");
                }

                // Nullability
                var isNullable = IsClrNullableType(clrType);
                Console.WriteLine($"         Nullable:    {isNullable}");
            }

            // Show non-column properties for context
            if (!isColumn)
            {
                Console.WriteLine($"         Value:       {prop.GetValue(null) ?? "(null)"}");
            }
        }

        Console.WriteLine();
    }

    private static bool IsColumnType(Type type)
    {
        if (!type.IsGenericType) return false;
        var checkType = type;
        while (checkType != null)
        {
            if (checkType.IsGenericType && checkType.GetGenericTypeDefinition() == typeof(DbColumn<,,>))
                return true;
            checkType = checkType.BaseType;
        }
        return false;
    }

    private static Type GetColumnClrType(Type columnType)
    {
        var checkType = columnType;
        while (checkType != null)
        {
            if (checkType.IsGenericType && checkType.GetGenericTypeDefinition() == typeof(DbColumn<,,>))
                return checkType.GetGenericArguments()[0];
            checkType = checkType.BaseType;
        }
        return typeof(string);
    }

    private static bool IsClrNullableType(Type type)
    {
        if (!type.IsValueType) return true;
        return Nullable.GetUnderlyingType(type) != null;
    }

    private static string GetTypeDisplayName(Type type)
    {
        if (!type.IsGenericType) return type.Name;
        var name = type.Name.Split('`')[0];
        var args = string.Join(", ", type.GetGenericArguments().Select(GetTypeDisplayName));
        return $"{name}<{args}>";
    }

    private static string DescribeAttribute(object attr)
    {
        try
        {
            var type = attr.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name is not "TypeId" && p.CanRead)
                .ToList();

            if (props.Count == 0) return "";

            var parts = new List<string>();
            foreach (var prop in props)
            {
                var val = prop.GetValue(attr);
                if (val != null && prop.Name is not "TypeId")
                {
                    parts.Add($"{prop.Name}={val}");
                }
            }

            return parts.Count > 0 ? $" ({string.Join(", ", parts)})" : "";
        }
        catch
        {
            return "";
        }
    }

    private static string MapClrToSqlType(Type clrType, Type dialectType)
    {
        try
        {
            var typeMapProp = dialectType.GetProperty("ClrToSqlTypeMap", BindingFlags.Public | BindingFlags.Static);
            if (typeMapProp == null) return "?";

            var typeMap = typeMapProp.GetValue(null) as Dictionary<Type, ISqlDataType>;
            if (typeMap == null) return "?";

            // Handle nullable
            var underlyingType = Nullable.GetUnderlyingType(clrType);
            var checkType = underlyingType ?? clrType;

            if (typeMap.TryGetValue(checkType, out var sqlType))
                return sqlType.Sql;

            foreach (var (key, value) in typeMap)
            {
                if (key.IsAssignableFrom(checkType))
                    return value.Sql;
            }

            return "TEXT (default)";
        }
        catch
        {
            return "?";
        }
    }
}
