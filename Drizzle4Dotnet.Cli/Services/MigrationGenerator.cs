using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Drizzle4Dotnet.Cli.Models;
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
    private readonly DatabaseProvider _provider;
    private readonly Type _dialectType;

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
            var snapshotFiles = Directory.GetFiles(outputDir, "snapshot-*.json")
                .OrderByDescending(f => f)
                .ToArray();

            if (snapshotFiles.Length > 0)
            {
                var json = File.ReadAllText(snapshotFiles[0]);
                currentSnapshot = SchemaSnapshot.Deserialize(json);
            }
        }

        // Generate migration plan
        MigrationPlan plan;
        string description;

        if (currentSnapshot != null)
        {
            var diff = currentSnapshot.Compare(targetSnapshot);
            description = DescribeDiff(diff);
            plan = diff.ToMigrationPlan(migrationName);
        }
        else
        {
            // First migration - create all tables
            var tableDefs = ExtractTableDefinitions(tableTypes, assemblyPath);
            var steps = tableDefs.Values.Select(t =>
            {
                var createQuery = new CreateTableQuery(t);
                return new MigrationStep(
                    MigrationStepType.CreateTable,
                    $"Create table {t.SchemaName}.{t.TableName}",
                    createQuery
                );
            }).ToList();

            plan = new MigrationPlan(migrationName, steps);
            description = string.Join("; ", steps.Select(s => s.Description));
        }

        // Generate SQL script
        var sqlScript = GenerateSqlScript(plan);
        var checksum = ComputeChecksum(sqlScript);

        // Generate safe filename
        var safeName = SanitizeFileName(migrationName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var sqlFileName = $"{timestamp}_{safeName}.sql";
        var snapshotFileName = $"snapshot-{safeName}.json";

        var sqlFilePath = Path.Combine(outputDir, sqlFileName);
        var snapshotFilePath = Path.Combine(outputDir, snapshotFileName);

        // Write SQL file
        File.WriteAllText(sqlFilePath, sqlScript);

        // Write snapshot file
        var snapshotJson = targetSnapshot.Serialize();
        File.WriteAllText(snapshotFilePath, snapshotJson);

        // Update journal
        var journalPath = Path.Combine(outputDir, "migration-journal.json");
        var journal = MigrationJournal.Load(journalPath);
        journal.Provider = ProviderName;

        // Check if migration already exists in journal
        if (journal.Migrations.Any(m => m.Name == migrationName))
        {
            // Update existing entry
            var entry = journal.Migrations.First(m => m.Name == migrationName);
            entry.GeneratedAt = DateTime.UtcNow;
            entry.Checksum = checksum;
            entry.SqlFileName = sqlFileName;
            entry.SnapshotFileName = snapshotFileName;
            entry.Description = description;
        }
        else
        {
            journal.Migrations.Add(new MigrationJournalEntry
            {
                Name = migrationName,
                GeneratedAt = DateTime.UtcNow,
                Checksum = checksum,
                SqlFileName = sqlFileName,
                SnapshotFileName = snapshotFileName,
                Description = description
            });
        }

        journal.Save(journalPath);

        return new MigrationGenerationResult(
            migrationName,
            sqlScript,
            sqlFilePath,
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
    /// Creates a SchemaSnapshot from the given table type names.
    /// </summary>
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
    /// Uses OrmSchemaExporter to extract schema via reflection.
    /// </summary>
    private Dictionary<string, TableDefinition> ExtractTableDefinitions(
        IReadOnlyList<string> tableTypes,
        string? assemblyPath)
    {
        var result = new Dictionary<string, TableDefinition>();

        if (tableTypes.Count == 0)
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

        foreach (var typeName in tableTypes)
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
                    parts.Add($"Create table {change.SchemaName}.{change.TableName}");
                    break;
                case TableChangeType.Removed:
                    parts.Add($"Drop table {change.SchemaName}.{change.TableName}");
                    break;
                case TableChangeType.Modified:
                    var colChanges = new List<string>();
                    foreach (var col in change.ColumnChanges)
                    {
                        colChanges.Add($"{col.ChangeType} {col.ColumnName}");
                    }
                    parts.Add($"Alter table {change.SchemaName}.{change.TableName} ({string.Join(", ", colChanges)})");
                    break;
            }
        }

        return string.Join("; ", parts);
    }
}
