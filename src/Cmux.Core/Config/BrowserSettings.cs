namespace Cmux.Core.Config;

/// <summary>
/// How an embedded or external browser profile behaves.
/// </summary>
public enum BrowserProfileKind
{
    /// <summary>WebView2 with an isolated user-data folder (default: per workspace/surface).</summary>
    EmbeddedIsolated = 0,

    /// <summary>WebView2 with a shared persistent user-data folder.</summary>
    EmbeddedPersistent = 1,

    /// <summary>Launch the configured executable (or system default) outside cmux.</summary>
    External = 2,
}

public class BrowserSettings
{
    public string? DefaultProfileId { get; set; }
    public List<BrowserProfile> Profiles { get; set; } = [];
}

/// <summary>
/// Named browser profile for embedded WebView2 or external launch.
/// </summary>
public class BrowserProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Isolated";
    public BrowserProfileKind Kind { get; set; } = BrowserProfileKind.EmbeddedIsolated;

    /// <summary>Optional override for WebView2 user data or external profile directory.</summary>
    public string? UserDataFolder { get; set; }

    /// <summary>Optional fixed WebView2 runtime folder or external browser executable.</summary>
    public string? BrowserExecutablePath { get; set; }

    public string? AdditionalArguments { get; set; }
    public string StartUrl { get; set; } = "about:blank";

    /// <summary>When true (default for isolated), append workspace/surface ids to the profile path.</summary>
    public bool ScopeToSurface { get; set; } = true;

    public bool IsDefault { get; set; }
}
