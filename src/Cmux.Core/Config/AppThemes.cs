namespace Cmux.Core.Config;

/// <summary>
/// Application chrome colour themes (WPF resources), distinct from terminal ANSI palettes.
/// </summary>
public static class AppThemes
{
    public const string DefaultDark = "Default Dark";
    public const string Light = "Light";
    public const string HighContrast = "High Contrast";
    public const string Cyberpunk = "Cyberpunk";

    public static IReadOnlyList<string> Names { get; } =
    [
        DefaultDark,
        Light,
        HighContrast,
        Cyberpunk,
    ];

    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DefaultDark;

        return Names.FirstOrDefault(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase))
            ?? DefaultDark;
    }

    public static string GetResourcePath(string? name)
    {
        return Normalize(name) switch
        {
            Light => "Themes/LightTheme.xaml",
            HighContrast => "Themes/HighContrastTheme.xaml",
            Cyberpunk => "Themes/CyberpunkTheme.xaml",
            _ => "Themes/DarkTheme.xaml",
        };
    }

    /// <summary>Matching terminal palette name when the app theme should drive terminal colours.</summary>
    public static string? LinkedTerminalTheme(string? appThemeName)
    {
        return Normalize(appThemeName) switch
        {
            Light => Light,
            HighContrast => HighContrast,
            Cyberpunk => Cyberpunk,
            _ => null,
        };
    }

    public static bool UsesDarkWindowChrome(string? appThemeName) =>
        !string.Equals(Normalize(appThemeName), Light, StringComparison.Ordinal);
}
