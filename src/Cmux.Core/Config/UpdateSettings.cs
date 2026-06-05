namespace Cmux.Core.Config;

public class UpdateSettings
{
    /// <summary>Check GitHub Releases shortly after startup.</summary>
    public bool CheckOnStartup { get; set; } = true;

    /// <summary>Minimum hours between automatic checks (0 = every startup only).</summary>
    public int CheckIntervalHours { get; set; } = 24;

    /// <summary>Latest release version the user dismissed with "Later".</summary>
    public string? DismissedVersion { get; set; }

    public DateTime? LastCheckUtc { get; set; }
}
