using System.Diagnostics;
using System.Xml.Linq;

namespace Drizzle4Dotnet.Cli.Services;

/// <summary>
/// Builds a .NET project from a .csproj file using dotnet build with MSBuild property overrides.
/// Allows disabling AOT/trimming for better reflection-based schema scanning.
/// </summary>
public static class ProjectBuilder
{
    /// <summary>
    /// Default MSBuild properties to override for safe reflection-based scanning.
    /// Disables AOT and trimming which would break reflection-based schema extraction.
    /// </summary>
    private static readonly Dictionary<string, string> DefaultProperties = new()
    {
        ["PublishAot"] = "false",
        ["PublishTrimmed"] = "false",
        ["PublishSingleFile"] = "false",
        ["PublishReadyToRun"] = "false",
    };

    /// <summary>
    /// Builds the specified project and returns the path to the output assembly DLL.
    /// Uses MSBuild property overrides to disable AOT/trimming for safe reflection scanning.
    /// </summary>
    /// <param name="projectPath">Path to the .csproj file.</param>
    /// <param name="configuration">Build configuration (Debug, Release). Default: Debug.</param>
    /// <param name="outputPath">Custom output path. If null, uses default project output directory.</param>
    /// <param name="extraProperties">Additional MSBuild properties to set.</param>
    /// <returns>The path to the built assembly DLL.</returns>
    public static string Build(
        string projectPath,
        string configuration = "Debug",
        string? outputPath = null,
        Dictionary<string, string>? extraProperties = null)
    {
        if (!File.Exists(projectPath))
            throw new FileNotFoundException($"Project file not found: {projectPath}");

        projectPath = Path.GetFullPath(projectPath);
        var projectDir = Path.GetDirectoryName(projectPath)!;

        // Resolve output path
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = Path.Combine(projectDir, "bin", configuration,
                ParseTargetFramework(projectPath), "cli-migration-tmp");
        }

        var outputDir = Path.GetFullPath(outputPath);

        // Build MSBuild properties
        var properties = new Dictionary<string, string>(DefaultProperties);
        if (extraProperties != null)
        {
            foreach (var kv in extraProperties)
                properties[kv.Key] = kv.Value;
        }
        properties["Configuration"] = configuration;
        properties["OutputPath"] = outputDir;

        // Convert to MSBuild property arguments: -p:Key=Value
        var msbuildProps = string.Join(" ",
            properties.Select(kv => $"-p:{kv.Key}={kv.Value}"));

        Console.WriteLine($"  Building: {Path.GetFileName(projectPath)}");
        Console.WriteLine($"  Output:   {outputDir}");

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\" {msbuildProps} --verbosity q -nologo",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.Error.WriteLine(stdout);
            Console.Error.WriteLine(stderr);
            throw new InvalidOperationException(
                $"Build failed for project '{Path.GetFileName(projectPath)}'. " +
                $"Exit code: {process.ExitCode}");
        }

        if (!Directory.Exists(outputDir))
            throw new DirectoryNotFoundException(
                $"Build output directory not found: {outputDir}. " +
                $"Build may have failed or output path is incorrect.");

        // Find the DLL in the output directory
        var assemblyName = Path.GetFileNameWithoutExtension(projectPath);
        var dllPath = Path.Combine(outputDir, $"{assemblyName}.dll");

        if (File.Exists(dllPath))
            return dllPath;

        // Fallback: search for any DLL matching the assembly name
        var found = Directory.GetFiles(outputDir, $"{assemblyName}.*")
            .Where(f => f.EndsWith(".dll") || f.EndsWith(".exe"))
            .ToArray();

        if (found.Length > 0)
            return found[0];

        throw new FileNotFoundException(
            $"No output assembly found for project '{assemblyName}' in {outputDir}. " +
            $"Expected: {dllPath}");
    }

    /// <summary>
    /// Parses a .csproj file to extract the TargetFramework.
    /// </summary>
    private static string ParseTargetFramework(string projectPath)
    {
        var doc = XDocument.Load(projectPath);
        var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

        var tfm = doc.Descendants(ns + "TargetFramework").FirstOrDefault()?.Value
                  ?? doc.Descendants(ns + "TargetFrameworks").FirstOrDefault()?.Value?.Split(';')[0];

        if (string.IsNullOrEmpty(tfm))
            throw new InvalidOperationException(
                $"Could not determine TargetFramework from '{projectPath}'. " +
                "Ensure the .csproj has <TargetFramework> or <TargetFrameworks> set.");

        return tfm;
    }
}
