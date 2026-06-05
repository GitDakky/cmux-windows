using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cmux.Core.Config;
using Cmux.Views;

namespace Cmux.Services;

public static class AppThemeService
{
    public static void Apply(CmuxSettings settings)
    {
        var app = Application.Current;
        if (app == null)
            return;

        var themeName = AppThemes.Normalize(settings.AppThemeName);
        var path = AppThemes.GetResourcePath(themeName);
        var dictionary = new ResourceDictionary
        {
            Source = new Uri($"/Cmux;component/{path}", UriKind.Relative),
        };

        var merged = app.Resources.MergedDictionaries;
        var replaced = false;
        for (var i = 0; i < merged.Count; i++)
        {
            var source = merged[i].Source?.OriginalString ?? "";
            if (source.Contains("Themes/", StringComparison.OrdinalIgnoreCase))
            {
                merged[i] = dictionary;
                replaced = true;
                break;
            }
        }

        if (!replaced)
            merged.Insert(0, dictionary);

        var darkChrome = AppThemes.UsesDarkWindowChrome(themeName);
        foreach (Window window in app.Windows)
        {
            ApplyUiScaleToWindow(window, settings.UiFontScale);
            if (!WindowAppearance.TrySetImmersiveDarkMode(window, darkChrome))
                WindowAppearance.Apply(window, darkChrome);
        }
    }

    public static void ApplyUiScaleToWindow(Window window, double scale)
    {
        scale = Math.Clamp(scale, 1.0, 2.0);
        if (window.FindName("UiScaleRoot") is not FrameworkElement root)
            return;

        if (Math.Abs(scale - 1.0) < 0.001)
        {
            root.LayoutTransform = null;
            return;
        }

        root.LayoutTransform = new ScaleTransform(scale, scale);
    }
}
