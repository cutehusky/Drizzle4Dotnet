using System.Text.Json;

namespace Drizzle4Dotnet.Cli.Models;

/// <summary>
/// Represents the journal entry for a single migration.
/// Stores metadata in the migration-journal.json file including
/// the migration name, SQL filenames, checksum, snapshot reference, and description.
/// </summary>
public class MigrationJournalEntry
{
    /// <summary>The unique migration name or version identifier.</summary>
    public string Name { get; set; } = "";

    /// <summary>Timestamp (UTC) when the migration was generated.</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>SHA256 checksum of the up SQL migration content, used for integrity verification.</summary>
    public string Checksum { get; set; } = "";

    /// <summary>Filename of the up SQL migration script.</summary>
    public string SqlFileName { get; set; } = "";

    /// <summary>Filename of the snapshot JSON file capturing the schema state after this migration.</summary>
    public string SnapshotFileName { get; set; } = "";

    /// <summary>Description of the schema changes this migration makes.</summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Sequential incremental ID for this migration (e.g., "0001", "0002").
    /// Used in file naming to ensure uniqueness and ordering of migrations.
    /// </summary>
    public string IncrementalId { get; set; } = "";

    /// <summary>Filename of the rollback (down) SQL migration script.</summary>
    public string DownSqlFileName { get; set; } = "";

    /// <summary>
    /// Validates that all required fields (Name, IncrementalId, Checksum, SqlFileName,
    /// DownSqlFileName, SnapshotFileName) have non-empty values.
    /// Throws an InvalidOperationException if any required field is missing.
    /// </summary>
    public void Validate()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(Name)) missing.Add("Name");
        if (string.IsNullOrWhiteSpace(IncrementalId)) missing.Add("IncrementalId");
        if (string.IsNullOrWhiteSpace(Checksum)) missing.Add("Checksum");
        if (string.IsNullOrWhiteSpace(SqlFileName)) missing.Add("SqlFileName");
        if (string.IsNullOrWhiteSpace(DownSqlFileName)) missing.Add("DownSqlFileName");
        if (string.IsNullOrWhiteSpace(SnapshotFileName)) missing.Add("SnapshotFileName");

        if (missing.Count > 0)
        {
            var label = string.IsNullOrWhiteSpace(Name) ? "(unnamed)" : Name;
            throw new InvalidOperationException(
                $"Migration journal entry '{label}' is missing required fields: {string.Join(", ", missing)}. " +
                "The journal file may be corrupted or from an incompatible version.");
        }
    }
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
    /// Serializes the journal to a JSON string with indentation for readability.
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Deserializes a journal from a JSON string and validates all entries.
    /// Throws if any required field is missing.
    /// </summary>
    public static MigrationJournal Deserialize(string json)
    {
        var journal = JsonSerializer.Deserialize<MigrationJournal>(json)
            ?? throw new InvalidOperationException("Failed to deserialize migration journal");
        journal.Validate();
        return journal;
    }

    /// <summary>
    /// Validates the journal by checking that the Provider is set and all migration entries are valid.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Provider))
            throw new InvalidOperationException("Migration journal is missing required field 'Provider'");

        foreach (var entry in Migrations)
            entry.Validate();
    }

    /// <summary>
    /// Loads the journal from a file path, or returns a new empty journal if the file does not exist.
    /// Validates all entries after loading.
    /// </summary>
    public static MigrationJournal Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new MigrationJournal();

        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }

    /// <summary>
    /// Saves the journal to a file path as indented JSON. Creates the directory if it does not exist.
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