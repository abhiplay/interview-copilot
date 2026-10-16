namespace InterviewCopilot.Infrastructure.Persistence;

/// <summary>Bound from "Storage". Empty DataDirectory means the OS user-data folder.</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";
    public const string AppFolderName = "InterviewCopilot";
    public const string DatabaseFileName = "interview-copilot.db";

    public string? DataDirectory { get; set; }

    /// <summary>
    /// %APPDATA%\InterviewCopilot on Windows, ~/Library/Application Support/InterviewCopilot on macOS,
    /// ~/.config/InterviewCopilot elsewhere.
    /// </summary>
    public string ResolveDataDirectory()
    {
        if (!string.IsNullOrWhiteSpace(DataDirectory))
        {
            return DataDirectory;
        }

        // On macOS .NET maps LocalApplicationData to ~/Library/Application Support.
        var root = OperatingSystem.IsMacOS()
            ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(root, AppFolderName);
    }

    public string ResolveDatabasePath() => Path.Combine(ResolveDataDirectory(), DatabaseFileName);

    public string ResolveModelsDirectory() => Path.Combine(ResolveDataDirectory(), "models");
}
