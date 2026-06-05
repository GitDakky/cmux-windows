using Cmux.Core.Config;

namespace Cmux.Tests;

public class AppThemesTests
{
    [Theory]
    [InlineData(null, "Default Dark")]
    [InlineData("", "Default Dark")]
    [InlineData("light", "Light")]
    [InlineData("HIGH CONTRAST", "High Contrast")]
    [InlineData("Unknown", "Default Dark")]
    public void Normalize_returns_known_theme(string? input, string expected)
    {
        Assert.Equal(expected, AppThemes.Normalize(input));
    }

    [Theory]
    [InlineData("Light", "Themes/LightTheme.xaml")]
    [InlineData("High Contrast", "Themes/HighContrastTheme.xaml")]
    [InlineData("Cyberpunk", "Themes/CyberpunkTheme.xaml")]
    [InlineData("Default Dark", "Themes/DarkTheme.xaml")]
    public void GetResourcePath_maps_theme_to_dictionary(string theme, string path)
    {
        Assert.Equal(path, AppThemes.GetResourcePath(theme));
    }

    [Theory]
    [InlineData("Light", "Light")]
    [InlineData("High Contrast", "High Contrast")]
    [InlineData("Cyberpunk", "Cyberpunk")]
    [InlineData("Default Dark", null)]
    [InlineData("Dracula", null)]
    public void LinkedTerminalTheme_pairs_app_and_terminal_presets(string appTheme, string? terminal)
    {
        Assert.Equal(terminal, AppThemes.LinkedTerminalTheme(appTheme));
    }

    [Theory]
    [InlineData("Light", false)]
    [InlineData("Default Dark", true)]
    [InlineData("High Contrast", true)]
    [InlineData("Cyberpunk", true)]
    public void UsesDarkWindowChrome_only_false_for_light(string theme, bool dark)
    {
        Assert.Equal(dark, AppThemes.UsesDarkWindowChrome(theme));
    }

    [Fact]
    public void TerminalThemes_includes_accessibility_presets()
    {
        foreach (var name in new[] { "Light", "High Contrast", "Cyberpunk" })
        {
            Assert.Contains(name, TerminalThemes.Names);
            Assert.Equal(name, TerminalThemes.Get(name).Name);
        }
    }
}
