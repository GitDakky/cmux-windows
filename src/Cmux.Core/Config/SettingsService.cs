using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cmux.Core.Services;

namespace Cmux.Core.Config;

/// <summary>
/// Manages reading, writing, and caching of <see cref="CmuxSettings"/>.
/// Settings are stored at <c>%USERPROFILE%\.cmux-windows\config.json</c> when present or after save;
/// legacy path <c>%LOCALAPPDATA%\cmux\settings.json</c> is used for load when the user file does not exist.
/// </summary>
public static class SettingsService
{
    /// <summary>Override for unit tests.</summary>
    internal static string? SettingsPathOverride { get; set; }

    internal static void ResetForTests() => _current = null;

    private static readonly string UserSettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cmux-windows");

    private static readonly string UserSettingsPath =
        Path.Combine(UserSettingsDir, "config.json");

    private static readonly string LegacySettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cmux");

    private static readonly string LegacySettingsPath =
        Path.Combine(LegacySettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static CmuxSettings? _current;

    /// <summary>Path used for the next load (user config if it exists, else legacy).</summary>
    public static string ActiveSettingsPath => SettingsPathOverride ?? ResolveLoadPath();

    /// <summary>Path where <see cref="Save"/> writes.</summary>
    public static string SaveSettingsPath => SettingsPathOverride ?? UserSettingsPath;

    /// <summary>
    /// The current in-memory settings instance (loaded on first access).
    /// </summary>
    public static CmuxSettings Current => _current ??= Load();

    /// <summary>
    /// Raised after settings have been modified and persisted.
    /// </summary>
    public static event Action? SettingsChanged;

    private static string ResolveLoadPath()
    {
        if (File.Exists(UserSettingsPath))
            return UserSettingsPath;
        return LegacySettingsPath;
    }

    /// <summary>
    /// Reads settings from disk. Returns a fresh default instance on any failure.
    /// </summary>
    public static CmuxSettings Load()
    {
        try
        {
            var path = ActiveSettingsPath;
            if (!File.Exists(path))
            {
                var defaults = new CmuxSettings();
                ApplyDefaults(defaults);
                return defaults;
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<CmuxSettings>(json, JsonOptions) ?? new CmuxSettings();
            ApplyDefaults(settings);
            return settings;
        }
        catch
        {
            var defaults = new CmuxSettings();
            ApplyDefaults(defaults);
            return defaults;
        }
    }

    /// <summary>
    /// Persists the given settings to disk atomically (write to .tmp, then move).
    /// </summary>
    public static void Save(CmuxSettings? settings = null)
    {
        settings ??= Current;

        try
        {
            var path = SaveSettingsPath;
            var dir = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(dir);

            var tmpPath = path + ".tmp";
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(tmpPath, json);
            File.Move(tmpPath, path, overwrite: true);
        }
        catch (Exception ex)
        {
            LogSettingsFailure("Save", ex);
        }
    }

    private static void LogSettingsFailure(string operation, Exception ex)
    {
        Debug.WriteLine($"[SettingsService] {operation} failed: {ex.Message}");

        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cmux-windows");
            Directory.CreateDirectory(logDir);
            var logPath = Path.Combine(logDir, "settings.log");
            File.AppendAllText(logPath, $"{DateTime.UtcNow:o} {operation} failed: {ex}{Environment.NewLine}");
        }
        catch
        {
            // Last resort — avoid throwing from logging.
        }
    }

    /// <summary>
    /// Resets settings to defaults and persists the result.
    /// </summary>
    public static CmuxSettings Reset()
    {
        _current = new CmuxSettings();
        ApplyDefaults(_current);
        Save(_current);
        return _current;
    }

    /// <summary>
    /// Raises the <see cref="SettingsChanged"/> event.
    /// Call after modifying <see cref="Current"/> properties.
    /// </summary>
    public static void NotifyChanged() => SettingsChanged?.Invoke();

    /// <summary>
    /// Reloads settings from disk into <see cref="Current"/>.
    /// </summary>
    public static void Reload()
    {
        _current = Load();
        SettingsChanged?.Invoke();
    }

    internal static void ApplyDefaults(CmuxSettings settings)
    {
        settings.Notifications ??= new NotificationSettings();
        settings.Agent ??= new AgentSettings();
        settings.Browser ??= new BrowserSettings();
        settings.Updates ??= new UpdateSettings();

        settings.AppThemeName = AppThemes.Normalize(settings.AppThemeName);
        settings.UiFontScale = Math.Clamp(settings.UiFontScale <= 0 ? 1.0 : settings.UiFontScale, 1.0, 2.0);

        if (settings.Browser.Profiles.Count == 0)
        {
            settings.Browser.Profiles.Add(BrowserProfileService.CreateDefaultIsolatedProfile());
            settings.Browser.Profiles.Add(BrowserProfileService.CreateDefaultPersistentProfile());
            settings.Browser.Profiles.Add(BrowserProfileService.CreateDefaultExternalProfile());
        }

        if (string.IsNullOrWhiteSpace(settings.Browser.DefaultProfileId))
        {
            settings.Browser.DefaultProfileId = settings.Browser.Profiles
                .FirstOrDefault(p => p.IsDefault)?.Id
                ?? settings.Browser.Profiles[0].Id;
        }

        if (string.IsNullOrWhiteSpace(settings.DefaultShell))
        {
            var shells = ShellDetector.DetectShells();
            if (shells.Count > 0)
                settings.DefaultShell = shells[0].Path;
        }

        if (settings.ShellProfiles.Count == 0)
        {
            var shells = ShellDetector.DetectShells();
            for (int i = 0; i < shells.Count; i++)
            {
                var sh = shells[i];
                settings.ShellProfiles.Add(new ShellProfile
                {
                    Name = sh.Name,
                    Command = sh.Path,
                    IsDefault = i == 0,
                });
            }
        }
    }
}
