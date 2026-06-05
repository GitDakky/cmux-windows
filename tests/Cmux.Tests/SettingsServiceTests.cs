using System.Text.Json;
using Cmux.Core.Config;
using FluentAssertions;
using Xunit;

namespace Cmux.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsNotificationSettings()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"cmux-settings-{Guid.NewGuid():N}");
        var path = Path.Combine(dir, "config.json");
        SettingsService.SettingsPathOverride = path;

        try
        {
            var settings = new CmuxSettings
            {
                FontFamily = "Consolas",
                FontSize = 12,
                Notifications = new NotificationSettings
                {
                    EnableToastNotifications = false,
                    ToastOnlyWhenUnfocused = false,
                    EnableTaskbarFlash = true,
                    FlashOnlyWhenUnfocused = false,
                },
            };

            SettingsService.Save(settings);
            SettingsService.Reload();

            var loaded = SettingsService.Current;
            loaded.FontFamily.Should().Be("Consolas");
            loaded.Notifications.EnableToastNotifications.Should().BeFalse();
            loaded.Notifications.ToastOnlyWhenUnfocused.Should().BeFalse();
            loaded.Notifications.EnableTaskbarFlash.Should().BeTrue();
            loaded.Notifications.FlashOnlyWhenUnfocused.Should().BeFalse();
        }
        finally
        {
            SettingsService.SettingsPathOverride = null;
            SettingsService.ResetForTests();
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaultsWithNotifications()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"cmux-settings-{Guid.NewGuid():N}");
        var path = Path.Combine(dir, "config.json");
        SettingsService.SettingsPathOverride = path;

        try
        {
            SettingsService.Reload();
            SettingsService.Current.Notifications.Should().NotBeNull();
            SettingsService.Current.Notifications.EnableToastNotifications.Should().BeTrue();
            SettingsService.Current.Notifications.EnableTaskbarFlash.Should().BeTrue();
            SettingsService.Current.Browser.Should().NotBeNull();
            SettingsService.Current.Browser!.Profiles.Should().NotBeEmpty();
        }
        finally
        {
            SettingsService.SettingsPathOverride = null;
            SettingsService.ResetForTests();
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Deserialize_LegacyJsonWithoutNotifications_UsesDefaults()
    {
        var json = """
            {
              "fontFamily": "Cascadia Code",
              "fontSize": 14
            }
            """;

        var settings = JsonSerializer.Deserialize<CmuxSettings>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        settings.Should().NotBeNull();
        SettingsService.ApplyDefaults(settings!);
        settings!.Notifications.EnableTaskbarFlash.Should().BeTrue();
    }
}
