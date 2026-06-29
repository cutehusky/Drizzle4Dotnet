namespace Drizzle4Dotnet.Cli.Services;

/// <summary>
/// Shared CLI option parsing utilities to reduce repetitive argument parsing code
/// across all commands. Provides common option keys, aliases, and validation helpers.
/// Includes standardized banner printing, error formatting, and centralized option descriptions
/// used across generate, snapshot, apply, status, and debug commands.
/// </summary>
public static class CliOptionParser
{
    /// <summary>
    /// Attempts to consume a named option from the argument list at the given index.
    /// If the option takes a value (hasValue=true), advances the index and returns it.
    /// Returns (consumed: true, value: "...") on success.
    /// </summary>
    public static (bool Consumed, string? Value) TryConsume(
        string[] args, ref int i,
        string longName, string shortAlias,
        bool hasValue = true)
    {
        var arg = args[i].ToLowerInvariant();
        if (arg == longName || arg == shortAlias)
        {
            if (hasValue)
            {
                if (++i < args.Length)
                    return (true, args[i]);
                return (false, null); // caller should add error
            }
            return (true, "true");
        }
        return (false, null);
    }

    /// <summary>
    /// Parses a comma-separated list of type names from a CLI argument value.
    /// Returns an empty list if the value is null or empty.
    /// </summary>
    public static List<string> ParseTypeList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }

    /// <summary>
    /// Prints a standardized error block for invalid arguments.
    /// </summary>
    public static void PrintErrors(List<string> errors, string usageLine)
    {
        Console.Error.WriteLine("Error: Invalid arguments:");
        foreach (var error in errors)
            Console.Error.WriteLine($"  - {error}");
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Usage: drizzle4net {usageLine}");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Use 'drizzle4net --help' for more information.");
    }

    /// <summary>
    /// Prints a standardized command banner with a title and key-value info lines.
    /// </summary>
    public static void PrintBanner(string title, params (string Key, string Value)[] info)
    {
        var line = new string('─', 50);
        Console.WriteLine($"  {title}");
        Console.WriteLine($"  {line}");
        foreach (var (key, value) in info)
        {
            Console.WriteLine($"  {key,-14} {value}");
        }
        Console.WriteLine($"  {line}");
    }

    /// <summary>
    /// Prints a standardized log banner using MigrationLogger (mirrors PrintBanner format).
    /// </summary>
    public static void LogBanner(MigrationLogger log, string title, params (string Key, string Value)[] info)
    {
        log.Log("BANNER", title);
        foreach (var (key, value) in info)
            log.Log(key.ToUpperInvariant(), value);
        log.LogSeparator();
    }

    /// <summary>
    /// Prints a standardized separator line.
    /// </summary>
    public static void PrintSeparator()
    {
        Console.WriteLine($"  {new string('─', 50)}");
    }

    /// <summary>
    /// Common option descriptors used across multiple commands.
    /// </summary>
    public static class Descriptions
    {
        public const string Provider = "Database provider (pgsql, mysql, mssql, sqlite, oracle)";
        public const string Types = "Comma-separated fully qualified ORM table type names; if omitted, auto-discovers all matching table types";
        public const string Assembly = "Path to pre-built assembly DLL containing table types";
        public const string Project = "Path to .csproj file; builds automatically and resolves assembly path";
        public const string Verbose = "Enable verbose output with detailed debug information";
        public const string Output = "Output directory path; use {provider} placeholder for provider name";
        public const string OutputFile = "Output file path for the snapshot JSON";
        public const string Connection = "Database connection string";
        public const string MigrationSchema = "Schema name for the migration tracking table (default: public)";
        public const string MigrationTable = "Table name for the migration tracking table (default: __Migrations)";
        public const string Name = "Migration or snapshot name; if omitted, a random name is auto-generated";
        public const string Snapshot = "Path to existing snapshot JSON file for diff comparison";
    }
}
