using System.Diagnostics;
using Cmux.Core.Config;

namespace Cmux.Core.Services;

/// <summary>
/// Resolves browser profile configuration and user-data paths for WebView2 sandboxes.
/// </summary>
public static class BrowserProfileService
{
    public static readonly string ProfilesRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cmux-windows",
        "browser-profiles");

    public static BrowserProfile ResolveProfile(CmuxSettings settings, string? profileId = null)
    {
        settings.Browser ??= new BrowserSettings();
        var profiles = settings.Browser.Profiles;

        if (!string.IsNullOrWhiteSpace(profileId))
        {
            var match = profiles.FirstOrDefault(p => string.Equals(p.Id, profileId, StringComparison.Ordinal));
            if (match != null)
                return match;
        }

        if (!string.IsNullOrWhiteSpace(settings.Browser.DefaultProfileId))
        {
            var byDefaultId = profiles.FirstOrDefault(p =>
                string.Equals(p.Id, settings.Browser.DefaultProfileId, StringComparison.Ordinal));
            if (byDefaultId != null)
                return byDefaultId;
        }

        var flagged = profiles.FirstOrDefault(p => p.IsDefault);
        if (flagged != null)
            return flagged;

        return profiles.FirstOrDefault() ?? CreateDefaultIsolatedProfile();
    }

    public static string GetUserDataFolder(
        BrowserProfile profile,
        string workspaceId,
        string surfaceId)
    {
        if (!string.IsNullOrWhiteSpace(profile.UserDataFolder))
        {
            var expanded = Environment.ExpandEnvironmentVariables(profile.UserDataFolder.Trim());
            if (profile.ScopeToSurface)
                return Path.Combine(expanded, SanitizePathSegment(workspaceId), SanitizePathSegment(surfaceId));
            return expanded;
        }

        return profile.Kind switch
        {
            BrowserProfileKind.EmbeddedIsolated => Path.Combine(
                ProfilesRoot,
                "isolated",
                SanitizePathSegment(workspaceId),
                SanitizePathSegment(surfaceId)),

            BrowserProfileKind.EmbeddedPersistent => Path.Combine(
                ProfilesRoot,
                "persistent",
                SanitizePathSegment(profile.Id)),

            _ => Path.Combine(ProfilesRoot, "misc", SanitizePathSegment(profile.Id)),
        };
    }

    public static string ResolveStartUrl(BrowserProfile profile, string? surfaceStartUrl, string? lastUrl)
    {
        if (!string.IsNullOrWhiteSpace(surfaceStartUrl))
            return NormalizeUrl(surfaceStartUrl);

        if (!string.IsNullOrWhiteSpace(lastUrl))
            return NormalizeUrl(lastUrl);

        if (!string.IsNullOrWhiteSpace(profile.StartUrl))
            return NormalizeUrl(profile.StartUrl);

        return "about:blank";
    }

    public static bool TryLaunchExternal(BrowserProfile profile, string url, out string? error)
    {
        error = null;
        url = NormalizeUrl(url);

        try
        {
            if (!string.IsNullOrWhiteSpace(profile.BrowserExecutablePath))
            {
                var exe = Environment.ExpandEnvironmentVariables(profile.BrowserExecutablePath.Trim());
                if (!File.Exists(exe))
                {
                    error = $"Browser executable not found: {exe}";
                    return false;
                }

                var args = BuildExternalArguments(profile, url);
                Process.Start(new ProcessStartInfo(exe, args)
                {
                    UseShellExecute = false,
                });
                return true;
            }

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static string NormalizeUrl(string url)
    {
        url = url.Trim();
        if (string.IsNullOrEmpty(url))
            return "about:blank";

        if (url.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
            return url;

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return "https://" + url;

        return url;
    }

    internal static BrowserProfile CreateDefaultIsolatedProfile() => new()
    {
        Id = "isolated-default",
        Name = "Isolated (agent sandbox)",
        Kind = BrowserProfileKind.EmbeddedIsolated,
        ScopeToSurface = true,
        StartUrl = "about:blank",
        IsDefault = true,
    };

    internal static BrowserProfile CreateDefaultPersistentProfile() => new()
    {
        Id = "project-default",
        Name = "Project (persistent)",
        Kind = BrowserProfileKind.EmbeddedPersistent,
        ScopeToSurface = false,
        StartUrl = "about:blank",
    };

    internal static BrowserProfile CreateDefaultExternalProfile() => new()
    {
        Id = "external-default",
        Name = "External browser",
        Kind = BrowserProfileKind.External,
        StartUrl = "about:blank",
    };

    private static string BuildExternalArguments(BrowserProfile profile, string url)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(profile.AdditionalArguments))
            parts.Add(profile.AdditionalArguments.Trim());

        if (!string.IsNullOrWhiteSpace(profile.UserDataFolder))
        {
            var userData = Environment.ExpandEnvironmentVariables(profile.UserDataFolder.Trim());
            parts.Add($"--user-data-dir=\"{userData}\"");
        }

        parts.Add($"\"{url}\"");
        return string.Join(' ', parts);
    }

    private static string SanitizePathSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "default";

        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        var sanitized = new string(chars).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "default" : sanitized;
    }
}
