using System.Text.Json;

namespace Drizzle4Dotnet.Cli.Models;

/// <summary>
/// Represents the journal entry for a single migration.
/// This is the metadata stored in the journal JSON file.
/// </summary>
public class MigrationJournalEntry
{
    /// <summary>The unique migration name/version.</summary>
    public string Name { get; set; } = "";

    /// <summary>Timestamp when the migration was generated.</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>SHA256 checksum of the migration SQL content.</summary>
    public string Checksum { get; set; } = "";

    /// <summary>The filename of the SQL migration script.</summary>
    public string SqlFileName { get; set; } = "";

    /// <summary>The filename of the snapshot JSON (state after this migration).</summary>
    public string SnapshotFileName { get; set; } = "";

    /// <summary>Description of what this migration does.</summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Sequential incremental ID for this migration (e.g., "0001", "0002").
    /// Used in file naming to ensure uniqueness and ordering.
    /// </summary>
    public string IncrementalId { get; set; } = "";
}

/// <summary>
/// The full journal file that tracks all generated migrations.
/// This is persisted as <c>migration-journal.json</c> in the migrations output directory.
/// </summary>
public class MigrationJournal
{
    /// <summary>The dialect/provider this journal is for.</summary>
    public string Provider { get; set; } = "";

    /// <summary>The list of migration entries, in order.</summary>
    public List<MigrationJournalEntry> Migrations { get; set; } = new();

    /// <summary>
    /// Serializes the journal to a JSON string.
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Deserializes a journal from a JSON string.
    /// </summary>
    public static MigrationJournal Deserialize(string json)
    {
        return JsonSerializer.Deserialize<MigrationJournal>(json)
            ?? throw new InvalidOperationException("Failed to deserialize migration journal");
    }

    /// <summary>
    /// Loads the journal from a file path, or returns a new empty journal if the file doesn't exist.
    /// </summary>
    public static MigrationJournal Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new MigrationJournal();

        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }

    /// <summary>
    /// Saves the journal to a file path.
    /// </summary>
    public void Save(string filePath)
    {
        var json = Serialize();
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(filePath, json);
    }
}
