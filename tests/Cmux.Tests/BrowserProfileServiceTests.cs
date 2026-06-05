using Cmux.Core.Config;
using Cmux.Core.Services;
using FluentAssertions;
using Xunit;

namespace Cmux.Tests;

public class BrowserProfileServiceTests
{
    [Fact]
    public void ResolveProfile_UsesExplicitId()
    {
        var settings = new CmuxSettings
        {
            Browser = new BrowserSettings
            {
                Profiles =
                [
                    new BrowserProfile { Id = "a", Name = "A" },
                    new BrowserProfile { Id = "b", Name = "B" },
                ],
            },
        };

        var profile = BrowserProfileService.ResolveProfile(settings, "b");
        profile.Id.Should().Be("b");
    }

    [Fact]
    public void ResolveProfile_FallsBackToDefaultProfileId()
    {
        var settings = new CmuxSettings
        {
            Browser = new BrowserSettings
            {
                DefaultProfileId = "persistent",
                Profiles =
                [
                    new BrowserProfile { Id = "isolated", Name = "Isolated", IsDefault = true },
                    new BrowserProfile { Id = "persistent", Name = "Persistent" },
                ],
            },
        };

        BrowserProfileService.ResolveProfile(settings).Id.Should().Be("persistent");
    }

    [Fact]
    public void GetUserDataFolder_IsolatedScopesByWorkspaceAndSurface()
    {
        var profile = new BrowserProfile
        {
            Id = "isolated-default",
            Kind = BrowserProfileKind.EmbeddedIsolated,
            ScopeToSurface = true,
        };

        var path = BrowserProfileService.GetUserDataFolder(profile, "ws-1", "surf-2");
        path.Should().Contain("isolated");
        path.Should().Contain("ws-1");
        path.Should().Contain("surf-2");
    }

    [Fact]
    public void GetUserDataFolder_PersistentUsesProfileId()
    {
        var profile = new BrowserProfile
        {
            Id = "project-default",
            Kind = BrowserProfileKind.EmbeddedPersistent,
        };

        var path = BrowserProfileService.GetUserDataFolder(profile, "ws-1", "surf-2");
        path.Should().Contain("persistent");
        path.Should().Contain("project-default");
        path.Should().NotContain("surf-2");
    }

    [Theory]
    [InlineData("example.com", "https://example.com")]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("about:blank", "about:blank")]
    public void NormalizeUrl_AddsSchemeWhenMissing(string input, string expected)
    {
        BrowserProfileService.NormalizeUrl(input).Should().Be(expected);
    }

    [Fact]
    public void ResolveStartUrl_PrefersSurfaceStartUrl()
    {
        var profile = new BrowserProfile { StartUrl = "https://profile.example" };
        var url = BrowserProfileService.ResolveStartUrl(
            profile,
            "https://surface.example",
            "https://last.example");

        url.Should().Be("https://surface.example");
    }
}
