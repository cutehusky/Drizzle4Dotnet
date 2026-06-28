using Drizzle4Dotnet.Cli.Models;
using Drizzle4Dotnet.Cli.Services;

namespace Drizzle4Dotnet.Cli.Commands;

/// <summary>
/// Handles the 'status' command: displays the migration journal and current state.
/// </summary>
public static class StatusCommand
{
    public static int Execute(StatusOptions options)
    {
        try
        {
            var outputDir = Path.GetFullPath(options.OutputDir);
            var journalPath = Path.Combine(outputDir, "migration-journal.json");

            if (!File.Exists(journalPath))
            {
                Console.WriteLine($"📋 Migration Status");
                Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine($"  No migration journal found at:");
                Console.WriteLine($"  {journalPath}");
                Console.WriteLine();
                Console.WriteLine($"  Use 'drizzle4net generate' to create your first migration.");
                Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return 0;
            }

            var journal = MigrationJournal.Load(journalPath);

            Console.WriteLine($"📋 Migration Status");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Provider:     {journal.Provider}");
            Console.WriteLine($"  Migrations:   {journal.Migrations.Count} total");
            Console.WriteLine($"  Journal:      {journalPath}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            if (journal.Migrations.Count == 0)
            {
                Console.WriteLine("  (no migrations recorded)");
            }
            else
            {
                for (var i = 0; i < journal.Migrations.Count; i++)
                {
                    var entry = journal.Migrations[i];
                    var status = IsMigrationApplied(outputDir, entry) ? "✅" : "⬜";
                    Console.WriteLine();
                    Console.WriteLine($"  {status} #{i + 1}: {entry.Name}");
                    Console.WriteLine($"      Generated:  {entry.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
                    Console.WriteLine($"      SQL File:   {entry.SqlFileName}");
                    Console.WriteLine($"      Snapshot:   {entry.SnapshotFileName}");
                    Console.WriteLine($"      Checksum:   {entry.Checksum[..Math.Min(16, entry.Checksum.Length)]}...");
                    Console.WriteLine($"      Desc:       {entry.Description}");
                }
            }

            // List snapshot files
            var snapshotFiles = Directory.GetFiles(outputDir, "snapshot-*.json")
                .OrderBy(f => f)
                .ToArray();

            Console.WriteLine();
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"  Snapshot files: {snapshotFiles.Length}");
            foreach (var file in snapshotFiles)
            {
                Console.WriteLine($"    {Path.GetFileName(file)}");
            }
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            if (options.Verbose)
                Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    /// <summary>
    /// Checks if a migration has been applied by verifying the SQL file and snapshot exist.
    /// </summary>
    private static bool IsMigrationApplied(string outputDir, MigrationJournalEntry entry)
    {
        var sqlPath = Path.Combine(outputDir, entry.SqlFileName);
        var snapshotPath = Path.Combine(outputDir, entry.SnapshotFileName);
        return File.Exists(sqlPath) && File.Exists(snapshotPath);
    }
}

/// <summary>
/// Options for the 'status' command.
/// </summary>
public class StatusOptions
{
    /// <summary>Output directory for migration files.</summary>
    public string OutputDir { get; set; } = "./Migrations/{provider}";

    /// <summary>Database provider.</summary>
    public string Provider { get; set; } = "pgsql";

    /// <summary>Enable verbose output.</summary>
    public bool Verbose { get; set; }
}
