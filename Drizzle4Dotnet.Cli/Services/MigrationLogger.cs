namespace Drizzle4Dotnet.Cli.Services;

/// <summary>
/// File-based logger for migration execution details.
/// Writes timestamped log entries to a local log file with action categories and messages.
/// Log format: [yyyy-MM-dd HH:mm:ss] ACTION: message
/// Implements IAsyncDisposable for proper resource cleanup.
/// </summary>
public class MigrationLogger : IAsyncDisposable
{
    private readonly string _logFilePath;
    private readonly StreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of MigrationLogger that writes to the specified directory.
    /// The log file is named with a timestamp for uniqueness (e.g., "20250101-120000.log").
    /// </summary>
    /// <param name="logDir">Directory path where log files will be stored. Created if it does not exist.</param>
    public MigrationLogger(string logDir)
    {
        // Ensure log directory exists
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var fileName = $"{timestamp}.log";
        _logFilePath = Path.Combine(logDir, fileName);

        _writer = new StreamWriter(_logFilePath, append: false, encoding: System.Text.Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    /// <summary>
    /// Gets the full path to the current log file.
    /// </summary>
    public string LogFilePath => _logFilePath;

    /// <summary>
    /// Writes a log entry with the current timestamp and action category.
    /// Format: [yyyy-MM-dd HH:mm:ss] ACTION: message
    /// </summary>
    /// <param name="action">Action category name (e.g., "PROVIDER", "SUCCESS", "ERROR").</param>
    /// <param name="message">Log message content.</param>
    public void Log(string action, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _writer.WriteLine($"[{timestamp}] {action}: {message}");
    }

    /// <summary>
    /// Writes an error log entry with exception details, including inner exception and stack trace.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    public void LogError(Exception ex)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _writer.WriteLine($"[{timestamp}] ERROR: {ex.Message}");
        if (ex.InnerException != null)
        {
            _writer.WriteLine($"[{timestamp}] ERROR_INNER: {ex.InnerException.Message}");
        }
        _writer.WriteLine($"[{timestamp}] ERROR_DETAIL: {ex}");
        _writer.WriteLine($"[{timestamp}] ERROR_STACKTRACE: {ex.StackTrace}");
    }

    /// <summary>
    /// Writes an error log entry with SQL context.
    /// Includes information about which SQL line caused the error and the full SQL script for debugging.
    /// </summary>
    /// <param name="sqlContent">The full SQL content that was being executed.</param>
    /// <param name="exception">The exception that occurred.</param>
    public void LogSqlError(string sqlContent, Exception exception)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        _writer.WriteLine($"[{timestamp}] ERROR: {exception.Message}");

        if (exception.InnerException != null)
        {
            _writer.WriteLine($"[{timestamp}] ERROR_INNER: {exception.InnerException.Message}");
        }

        // Try to identify which SQL line caused the error
        var sqlLines = sqlContent.Split('\n', StringSplitOptions.None);
        var errorLineInfo = FindErrorLine(sqlLines, exception);
        if (errorLineInfo != null)
        {
            _writer.WriteLine($"[{timestamp}] ERROR_LINE: {errorLineInfo.Value.LineNumber}: {errorLineInfo.Value.LineContent.Trim()}");
        }

        // Log the full SQL for context
        _writer.WriteLine($"[{timestamp}] SQL_CONTENT: Full SQL script:");
        for (var i = 0; i < sqlLines.Length; i++)
        {
            var marker = (errorLineInfo != null && i == errorLineInfo.Value.Index) ? " >>>" : "";
            _writer.WriteLine($"   {i + 1,4}: {sqlLines[i]}{marker}");
        }

        _writer.WriteLine($"[{timestamp}] ERROR_DETAIL: {exception}");
    }

    /// <summary>
    /// Attempts to identify which line in the SQL caused the error by analyzing
    /// the exception message for line number or position references.
    /// Supports PostgreSQL (LINE N:), generic (at line N), and position-based formats.
    /// </summary>
    /// <param name="sqlLines">The SQL content split into lines.</param>
    /// <param name="exception">The exception to analyze.</param>
    /// <returns>A tuple with the 1-based line number, 0-based index, and line content, or null if not found.</returns>
    private static (int LineNumber, int Index, string LineContent)? FindErrorLine(string[] sqlLines, Exception exception)
    {
        // Strategy 1: Look for line number patterns in exception message
        // Many databases report errors like "LINE 1: ..." or "at line 42" or "position: 123"
        var message = exception.Message;
        var innerMessage = exception.InnerException?.Message ?? "";

        // Check for PostgreSQL style: "LINE N: ..."
        var lineMatch = System.Text.RegularExpressions.Regex.Match(message + "\n" + innerMessage,
            @"LINE\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (lineMatch.Success && int.TryParse(lineMatch.Groups[1].Value, out var pgLine))
        {
            var idx = pgLine - 1; // 0-based index
            if (idx >= 0 && idx < sqlLines.Length)
                return (pgLine, idx, sqlLines[idx]);
        }

        // Strategy 2: Look for "at line N" patterns
        lineMatch = System.Text.RegularExpressions.Regex.Match(message + "\n" + innerMessage,
            @"at\s+line\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (lineMatch.Success && int.TryParse(lineMatch.Groups[1].Value, out var atLine))
        {
            var idx = atLine - 1;
            if (idx >= 0 && idx < sqlLines.Length)
                return (atLine, idx, sqlLines[idx]);
        }

        // Strategy 3: Look for "line N:" pattern (some error formats)
        lineMatch = System.Text.RegularExpressions.Regex.Match(message + "\n" + innerMessage,
            @"line\s+(\d+):", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (lineMatch.Success && int.TryParse(lineMatch.Groups[1].Value, out var colonLine))
        {
            var idx = colonLine - 1;
            if (idx >= 0 && idx < sqlLines.Length)
                return (colonLine, idx, sqlLines[idx]);
        }

        // Strategy 4: Look for position numbers (character position in SQL)
        var posMatch = System.Text.RegularExpressions.Regex.Match(message + "\n" + innerMessage,
            @"position\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (posMatch.Success && int.TryParse(posMatch.Groups[1].Value, out var position))
        {
            // Calculate which line the position falls on
            var charCount = 0;
            for (var i = 0; i < sqlLines.Length; i++)
            {
                charCount += sqlLines[i].Length + 1; // +1 for newline
                if (charCount > position)
                {
                    var lineNum = i + 1;
                    return (lineNum, i, sqlLines[i]);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Writes a separator line of dashes (80 chars) for readability.
    /// </summary>
    public void LogSeparator()
    {
        _writer.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Disposes the logger asynchronously, flushing all pending writes and
    /// writing a final END log entry before closing the file.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Log("END", "Migration log closed.");
        await _writer.FlushAsync();
        await _writer.DisposeAsync();
    }
}
