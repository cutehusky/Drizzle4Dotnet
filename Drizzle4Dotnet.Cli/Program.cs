using Drizzle4Dotnet.Cli.Commands;
using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli;

/// <summary>
/// Drizzle4Dotnet CLI - SQL migration script and journal JSON generator.
/// </summary>
/// <remarks>
/// Usage:
///   drizzle4net generate --provider pgsql --name v1 --types "SharedDemo.PgSql.UsersTable,SharedDemo.PgSql.DepartmentsTable"
///   drizzle4net generate --provider pgsql --name v1                                    (auto-discovers all table types)
///   drizzle4net generate --provider pgsql                                              (auto-name + auto-types)
///   drizzle4net generate --provider mysql --name v1 --types "SharedDemo.MySql.UsersTable" --assembly ./path/to/assembly.dll
///   drizzle4net snapshot --provider pgsql --name v1 --types "SharedDemo.PgSql.UsersTable" --output ./snapshot.json
///   drizzle4net snapshot --provider pgsql                                              (auto-name + auto-types)
///   drizzle4net apply --provider pgsql --connection "Host=localhost;Database=mydb"
///   drizzle4net generate --help
/// </remarks>
public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }

        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "generate":
            case "gen":
            case "g":
                return HandleGenerate(args.Skip(1).ToArray());

            case "snapshot":
            case "snap":
            case "s":
                return HandleSnapshot(args.Skip(1).ToArray());

            case "status":
            case "st":
                return HandleStatus(args.Skip(1).ToArray());

            case "apply":
            case "a":
                return HandleApply(args.Skip(1).ToArray());

            case "debug":
            case "d":
                return HandleDebug(args.Skip(1).ToArray());

            case "--help":
            case "-h":
            case "help":
                PrintUsage();
                return 0;

            case "--version":
            case "-v":
            case "version":
                PrintVersion();
                return 0;

            default:
                Console.Error.WriteLine($"Unknown command: '{command}'");
                Console.Error.WriteLine("Use 'drizzle4net --help' to see available commands.");
                return 1;
        }
    }

    private static int HandleGenerate(string[] args)
    {
        var options = new GenerateOptions();
        var errors = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--provider":
                case "-p":
                    if (++i < args.Length) options.Provider = args[i];
                    else errors.Add("--provider requires a value");
                    break;

                case "--name":
                case "-n":
                    if (++i < args.Length) options.MigrationName = args[i];
                    else errors.Add("--name requires a value");
                    break;

                case "--types":
                case "-t":
                    if (++i < args.Length)
                    {
                        options.TableTypes = args[i]
                            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }
                    else errors.Add("--types requires a value");
                    break;

                case "--output":
                case "-o":
                    if (++i < args.Length) options.OutputDir = args[i];
                    else errors.Add("--output requires a value");
                    break;

                case "--snapshot":
                case "-s":
                    if (++i < args.Length) options.SnapshotPath = args[i];
                    else errors.Add("--snapshot requires a value");
                    break;

                case "--assembly":
                case "-a":
                    if (++i < args.Length) options.AssemblyPath = args[i];
                    else errors.Add("--assembly requires a value");
                    break;

                case "--project":
                case "--proj":
                    if (++i < args.Length) options.ProjectPath = args[i];
                    else errors.Add("--project requires a value");
                    break;

                case "--verbose":
                case "-V":
                    options.Verbose = true;
                    break;

                default:
                    errors.Add($"Unknown option: '{args[i]}'");
                    break;
            }
        }

        // Resolve assembly path from project or assembly option
        if (!ResolveAssemblyPath(options, errors))
            return 1;

        // Validate required options
        if (string.IsNullOrWhiteSpace(options.MigrationName))
        {
            // Generate a random migration name when --name is omitted
            options.MigrationName = GenerateRandomMigrationName();
            Console.WriteLine($"  Name:         {options.MigrationName} (auto-generated)");
        }

        if (errors.Count > 0)
        {
            Console.Error.WriteLine("❌ Invalid arguments:");
            foreach (var error in errors)
                Console.Error.WriteLine($"  - {error}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: drizzle4net generate --provider pgsql [options]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine("  --provider, -p    Database provider (pgsql, mysql, mssql, sqlite, oracle)");
            Console.Error.WriteLine("  --name, -n        Migration name (e.g., 'v1.0.0', 'add_users_table')");
            Console.Error.WriteLine("                    (if omitted, a random name is auto-generated)");
            Console.Error.WriteLine("  --types, -t       Comma-separated fully qualified ORM table type names");
            Console.Error.WriteLine("                    (if omitted, auto-discovers all table types for the provider)");
            Console.Error.WriteLine("  --output, -o      Output directory (default: ./Migrations/{provider})");
            Console.Error.WriteLine("  --snapshot, -s    Path to current snapshot JSON file");
            Console.Error.WriteLine("  --project, --proj Path to .csproj file (builds automatically)");
            Console.Error.WriteLine("  --assembly, -a    Path to pre-built assembly DLL");
            Console.Error.WriteLine("  --verbose, -V     Enable verbose output");
            return 1;
        }

        return GenerateCommand.Execute(options);
    }

    private static int HandleSnapshot(string[] args)
    {
        var options = new SnapshotOptions();
        var errors = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--provider":
                case "-p":
                    if (++i < args.Length) options.Provider = args[i];
                    else errors.Add("--provider requires a value");
                    break;

                case "--name":
                case "-n":
                    if (++i < args.Length) options.SnapshotName = args[i];
                    else errors.Add("--name requires a value");
                    break;

                case "--types":
                case "-t":
                    if (++i < args.Length)
                    {
                        options.TableTypes = args[i]
                            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }
                    else errors.Add("--types requires a value");
                    break;

                case "--output":
                case "-o":
                    if (++i < args.Length) options.OutputFile = args[i];
                    else errors.Add("--output requires a value");
                    break;

                case "--assembly":
                case "-a":
                    if (++i < args.Length) options.AssemblyPath = args[i];
                    else errors.Add("--assembly requires a value");
                    break;

                case "--project":
                case "--proj":
                    if (++i < args.Length) options.ProjectPath = args[i];
                    else errors.Add("--project requires a value");
                    break;

                case "--verbose":
                case "-V":
                    options.Verbose = true;
                    break;

                default:
                    errors.Add($"Unknown option: '{args[i]}'");
                    break;
            }
        }

        // Resolve assembly path
        if (!ResolveAssemblyPath(options, errors))
            return 1;

        // Validate required options
        if (string.IsNullOrWhiteSpace(options.SnapshotName))
        {
            // Generate a random snapshot name when --name is omitted
            options.SnapshotName = GenerateRandomMigrationName();
            Console.WriteLine($"  Name:         {options.SnapshotName} (auto-generated)");
        }

        if (options.TableTypes.Count == 0 && errors.Count == 0)
        {
            Console.WriteLine($"  Auto-discover: On (no --types specified, scanning for {options.Provider} table types)");
        }

        if (errors.Count > 0)
        {
            Console.Error.WriteLine("❌ Invalid arguments:");
            foreach (var error in errors)
                Console.Error.WriteLine($"  - {error}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: drizzle4net snapshot --provider pgsql [options]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine("  --provider, -p    Database provider (pgsql, mysql, mssql, sqlite, oracle)");
            Console.Error.WriteLine("  --name, -n        Snapshot name (e.g., 'v1.0.0')");
            Console.Error.WriteLine("                    (if omitted, a random name is auto-generated)");
            Console.Error.WriteLine("  --types, -t       Comma-separated fully qualified ORM table type names");
            Console.Error.WriteLine("                    (if omitted, auto-discovers all table types for the provider)");
            Console.Error.WriteLine("  --output, -o      Output file path (default: ./snapshot.json)");
            Console.Error.WriteLine("  --project, --proj Path to .csproj file (builds automatically)");
            Console.Error.WriteLine("  --assembly, -a    Path to pre-built assembly DLL");
            Console.Error.WriteLine("  --verbose, -V     Enable verbose output");
            return 1;
        }

        return SnapshotCommand.Execute(options);
    }

    /// <summary>
    /// Resolves the assembly path from either --project (builds the project) or --assembly (direct path).
    /// Modifies the options object in place, setting AssemblyPath if --project was provided.
    /// Returns false if there was a resolution error.
    /// </summary>
    private static bool ResolveAssemblyPath<T>(T options, List<string> errors) where T : class
    {
        // Try to get ProjectPath and AssemblyPath via reflection (works for all options types)
        var projProp = typeof(T).GetProperty("ProjectPath");
        var asmProp = typeof(T).GetProperty("AssemblyPath");

        var projectPath = projProp?.GetValue(options) as string;
        var assemblyPath = asmProp?.GetValue(options) as string;

        // If both provided, project takes precedence
        if (!string.IsNullOrWhiteSpace(projectPath))
        {
            try
            {
                var builtDll = ProjectBuilder.Build(projectPath);
                Console.WriteLine($"  Built DLL:    {builtDll}");
                asmProp?.SetValue(options, builtDll);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to build project '{projectPath}': {ex.Message}");
                return false;
            }
        }
        else if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            // Neither provided - try auto-discover in current directory
            var cwd = Directory.GetCurrentDirectory();
            var csprojFiles = Directory.GetFiles(cwd, "*.csproj");
            if (csprojFiles.Length == 1)
            {
                try
                {
                    var builtDll = ProjectBuilder.Build(csprojFiles[0]);
                    Console.WriteLine($"  Auto-discovered project: {Path.GetFileName(csprojFiles[0])}");
                    Console.WriteLine($"  Built DLL:    {builtDll}");
                    asmProp?.SetValue(options, builtDll);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to auto-build project: {ex.Message}");
                    return false;
                }
            }
            else
            {
                errors.Add("Either --project <path.csproj> or --assembly <path.dll> is required. " +
                          (csprojFiles.Length > 1
                              ? $"Multiple .csproj files found in current directory. Specify one with --project."
                              : ""));
            }
        }

        return true;
    }

    private static int HandleStatus(string[] args)
    {
        var options = new StatusOptions();
        var errors = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--output":
                case "-o":
                    if (++i < args.Length) options.OutputDir = args[i];
                    else errors.Add("--output requires a value");
                    break;

                case "--provider":
                case "-p":
                    if (++i < args.Length) options.Provider = args[i];
                    else errors.Add("--provider requires a value");
                    break;

                case "--verbose":
                case "-V":
                    options.Verbose = true;
                    break;

                default:
                    errors.Add($"Unknown option: '{args[i]}'");
                    break;
            }
        }

        // Resolve {provider} placeholder
        options.OutputDir = options.OutputDir.Replace("{provider}", options.Provider);

        if (errors.Count > 0)
        {
            Console.Error.WriteLine("❌ Invalid arguments:");
            foreach (var error in errors)
                Console.Error.WriteLine($"  - {error}");
            return 1;
        }

        return StatusCommand.Execute(options);
    }

    private static int HandleDebug(string[] args)
    {
        var options = new DebugOptions();
        var errors = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--provider":
                case "-p":
                    if (++i < args.Length) options.Provider = args[i];
                    else errors.Add("--provider requires a value");
                    break;

                case "--types":
                case "-t":
                    if (++i < args.Length)
                    {
                        options.TableTypes = args[i]
                            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }
                    else errors.Add("--types requires a value");
                    break;

                case "--assembly":
                case "-a":
                    if (++i < args.Length) options.AssemblyPath = args[i];
                    else errors.Add("--assembly requires a value");
                    break;

                case "--project":
                case "--proj":
                    if (++i < args.Length) options.ProjectPath = args[i];
                    else errors.Add("--project requires a value");
                    break;

                case "--verbose":
                case "-V":
                    options.Verbose = true;
                    break;

                default:
                    errors.Add($"Unknown option: '{args[i]}'");
                    break;
            }
        }

        // Resolve assembly path
        if (!ResolveAssemblyPath(options, errors))
            return 1;

        if (errors.Count > 0)
        {
            Console.Error.WriteLine("❌ Invalid arguments:");
            foreach (var error in errors)
                Console.Error.WriteLine($"  - {error}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: drizzle4net debug --provider pgsql [options]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine("  --provider, -p    Database provider (pgsql, mysql, mssql, sqlite, oracle)");
            Console.Error.WriteLine("  --types, -t       Comma-separated fully qualified ORM table type names");
            Console.Error.WriteLine("                    (if omitted, auto-discovers all table types for the provider)");
            Console.Error.WriteLine("  --project, --proj Path to .csproj file (builds automatically)");
            Console.Error.WriteLine("  --assembly, -a    Path to pre-built assembly DLL");
            Console.Error.WriteLine("  --verbose, -V     Enable verbose output");
            return 1;
        }

        return DebugCommand.Execute(options);
    }

    private static int HandleApply(string[] args)
    {
        var options = new ApplyOptions();
        var errors = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--provider":
                case "-p":
                    if (++i < args.Length) options.Provider = args[i];
                    else errors.Add("--provider requires a value");
                    break;

                case "--connection":
                case "-c":
                    if (++i < args.Length) options.ConnectionString = args[i];
                    else errors.Add("--connection requires a value");
                    break;

                case "--output":
                case "-o":
                    if (++i < args.Length) options.OutputDir = args[i];
                    else errors.Add("--output requires a value");
                    break;

                case "--migration-schema":
                    if (++i < args.Length) options.MigrationSchema = args[i];
                    else errors.Add("--migration-schema requires a value");
                    break;

                case "--migration-table":
                    if (++i < args.Length) options.MigrationTable = args[i];
                    else errors.Add("--migration-table requires a value");
                    break;

                default:
                    errors.Add($"Unknown option: '{args[i]}'");
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            errors.Add("--connection (-c) is required (database connection string)");

        if (errors.Count > 0)
        {
            Console.Error.WriteLine("❌ Invalid arguments:");
            foreach (var error in errors)
                Console.Error.WriteLine($"  - {error}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: drizzle4net apply --provider pgsql --connection \"Host=...\" [options]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine("  --provider, -p       Database provider (pgsql, mysql, mssql, sqlite, oracle)");
            Console.Error.WriteLine("  --connection, -c     Database connection string");
            Console.Error.WriteLine("  --output, -o         Migrations directory (default: ./Migrations/{provider})");
            Console.Error.WriteLine("  --migration-schema   Schema for migration tracking table (default: public)");
            Console.Error.WriteLine("  --migration-table    Table name for migration tracking (default: __Migrations)");
            return 1;
        }

        // Execute synchronously — wait for the async task
        return ApplyCommand.Execute(options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Generates a random migration name using adjective_noun[ noun] format,
    /// inspired by Drizzle ORM's naming convention.
    /// Examples: "mature_champions", "fuzzy_hitman", "busy_professor_monster"
    /// </summary>
    private static string GenerateRandomMigrationName()
    {
        var random = Random.Shared;
        var adj = _adjectives[random.Next(_adjectives.Length)];
        var noun1 = _nouns[random.Next(_nouns.Length)];
        var noun2 = _nouns[random.Next(_nouns.Length)];

        // Sometimes 2 words (adj_noun), sometimes 3 words (adj_noun_noun)
        return random.Next(2) == 0
            ? $"{adj}_{noun1}"
            : $"{adj}_{noun1}_{noun2}";
    }

    private static readonly string[] _adjectives =
    [
        "aged", "ancient", "autumn", "billowing", "bitter", "black", "blue", "bold",
        "broad", "broken", "calm", "cold", "cool", "crimson", "curly", "damp",
        "dark", "dawn", "delicate", "divine", "dry", "empty", "falling", "fancy",
        "flat", "floral", "fragrant", "frosty", "fuzzy", "gentle", "green", "growing",
        "hidden", "holy", "icy", "jolly", "late", "lingering", "little", "lively",
        "long", "lucky", "misty", "mature", "morning", "muddy", "mute", "nameless",
        "noisy", "odd", "old", "orange", "patient", "plain", "polished", "proud",
        "purple", "quiet", "rapid", "raspy", "red", "restless", "rough", "round",
        "royal", "shiny", "shrill", "shy", "silent", "small", "snowy", "solemn",
        "spring", "square", "steep", "still", "summer", "super", "sweet", "thawing",
        "tight", "tiny", "twilight", "wandering", "warm", "weathered", "white", "wild",
        "winter", "wispy", "young"
    ];

    private static readonly string[] _nouns =
    [
        "waterfall", "river", "breeze", "moon", "rain", "wind", "sea", "morning",
        "snow", "lake", "sunset", "pine", "shadow", "leaf", "dawn", "glitter",
        "forest", "hill", "cloud", "meadow", "sun", "glade", "bird", "brook",
        "butterfly", "bush", "campfire", "canyon", "cave", "coast", "creek", "desert",
        "diamond", "dust", "feather", "fire", "flower", "fog", "frog", "frost",
        "garden", "gem", "grass", "haze", "island", "lagoon", "light", "mountain",
        "mushroom", "oak", "ocean", "peak", "petal", "pond", "rainbow", "reed",
        "rift", "rock", "sand", "sapphire", "savanna", "seed", "sky", "spring",
        "star", "storm", "sunlight", "swamp", "thorn", "thunder", "trail", "valley",
        "violet", "water", "wave", "wildflower", "wood", "castle", "king", "queen",
        "knight", "wizard", "dragon", "phoenix", "tiger", "lion", "eagle", "hawk",
        "falcon", "wolf", "bear", "deer", "fox", "rabbit", "horse", "panda",
        "koala", "dolphin", "whale", "shark", "turtle", "snake", "spider", "scorpion",
        "raven", "crow", "swan", "owl", "heron", "crane", "robin", "finch",
        "captain", "professor", "champion", "hitman", "midlands", "monster"
    ];

    private static void PrintUsage()
    {
        Console.WriteLine("Drizzle4Dotnet CLI - SQL Migration Generator");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  drizzle4net generate [options]    Generate a new SQL migration script");
        Console.WriteLine("  drizzle4net snapshot [options]    Generate a schema snapshot JSON");
        Console.WriteLine("  drizzle4net apply [options]       Apply pending migrations to database");
        Console.WriteLine("  drizzle4net status [options]      Show migration journal status");
        Console.WriteLine("  drizzle4net debug [options]       Print detailed schema information");
        Console.WriteLine("  drizzle4net --help                Show this help message");
        Console.WriteLine("  drizzle4net --version             Show version information");
        Console.WriteLine();
        Console.WriteLine("Generate Command:");
        Console.WriteLine("  Generates a SQL migration script by diffing ORM table schema classes");
        Console.WriteLine("  against a previous snapshot. Creates .sql file, snapshot JSON, and");
        Console.WriteLine("  updates the migration-journal.json file.");
        Console.WriteLine();
        Console.WriteLine("  drizzle4net generate --provider pgsql --name v1 --types \"Namespace.TableA,Namespace.TableB\"");
        Console.WriteLine("  drizzle4net generate --provider pgsql --name v1                (auto-discovers all table types)");
        Console.WriteLine("  drizzle4net generate --provider pgsql                          (auto-name + auto-types)");
        Console.WriteLine();
        Console.WriteLine("Snapshot Command:");
        Console.WriteLine("  Creates a snapshot JSON from ORM table types without generating SQL.");
        Console.WriteLine();
        Console.WriteLine("  drizzle4net snapshot --provider pgsql --name v1 --types \"Namespace.TableA\" --output ./schema.json");
        Console.WriteLine("  drizzle4net snapshot --provider pgsql --name v1                  (auto-discovers all table types)");
        Console.WriteLine("  drizzle4net snapshot --provider pgsql                            (auto-name + auto-types)");
        Console.WriteLine();
        Console.WriteLine("Apply Command:");
        Console.WriteLine("  Applies pending SQL migration scripts from the journal to a database.");
        Console.WriteLine("  Uses MigrationManager to track applied migrations in a configurable table.");
        Console.WriteLine("  Validates checksums from journal.json against SQL file content.");
        Console.WriteLine();
        Console.WriteLine("  drizzle4net apply --provider pgsql --connection \"Host=localhost;Database=mydb\"");
        Console.WriteLine("  drizzle4net apply --provider pgsql --connection \"...\" --migration-schema myapp --migration-table __SchemaMigrations");
        Console.WriteLine();
        Console.WriteLine("Status Command:");
        Console.WriteLine("  Shows the migration journal with all tracked migrations and snapshots.");
        Console.WriteLine();
        Console.WriteLine("  drizzle4net status --output ./Migrations/pgsql");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --provider, -p    Database provider: pgsql, mysql, mssql, sqlite, oracle");
        Console.WriteLine("  --name, -n        Migration or snapshot name");
        Console.WriteLine("                    (if omitted, a random name is auto-generated)");
        Console.WriteLine("  --types, -t       Comma-separated fully qualified ORM table type names");
        Console.WriteLine("                    (if omitted, auto-discovers all table types for the provider)");
        Console.WriteLine("  --output, -o      Output directory or file path");
        Console.WriteLine("  --snapshot, -s    Path to current snapshot JSON (for generate)");
        Console.WriteLine("  --assembly, -a    Path to assembly containing table types");
        Console.WriteLine("  --connection, -c  Database connection string (for apply)");
        Console.WriteLine("  --migration-schema Schema for migration tracking table (default: public)");
        Console.WriteLine("  --migration-table  Table name for migration tracking (default: __Migrations)");
        Console.WriteLine("  --verbose, -V     Enable verbose output");
    }

    private static void PrintVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version;
        Console.WriteLine($"Drizzle4Dotnet CLI version {version?.ToString() ?? "1.0.0"}");
    }
}
