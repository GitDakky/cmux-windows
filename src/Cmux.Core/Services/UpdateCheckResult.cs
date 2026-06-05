namespace Cmux.Core.Services;

public sealed class UpdateCheckResult
{
    public bool UpdateAvailable { get; init; }
    public string CurrentVersion { get; init; } = "";
    public string? LatestVersion { get; init; }
    public string? ReleaseName { get; init; }
    public string? ReleaseNotes { get; init; }
    public string? DownloadUrl { get; init; }
    public string? AssetName { get; init; }
    public string? Error { get; init; }

    public static UpdateCheckResult UpToDate(string currentVersion) => new()
    {
        UpdateAvailable = false,
        CurrentVersion = currentVersion,
    };

    public static UpdateCheckResult Failed(string currentVersion, string error) => new()
    {
        UpdateAvailable = false,
        CurrentVersion = currentVersion,
        Error = error,
    };
}
