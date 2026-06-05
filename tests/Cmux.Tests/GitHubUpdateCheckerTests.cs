using Cmux.Core.Services;
using FluentAssertions;
using Xunit;
using static Cmux.Core.Services.GitHubUpdateChecker;

namespace Cmux.Tests;

public class GitHubUpdateCheckerTests
{
    [Fact]
    public void FindWindowsAsset_MatchesReleaseZip()
    {
        var assets = new List<GitHubUpdateChecker.GitHubReleaseAsset>
        {
            new() { Name = "notes.txt", BrowserDownloadUrl = "https://github.com/x/y" },
            new() { Name = "cmux-windows-v1.0.10-win-x64.zip", BrowserDownloadUrl = "https://github.com/GitDakky/cmux-windows/releases/download/v1.0.10/cmux-windows-v1.0.10-win-x64.zip" },
        };

        FindWindowsAsset(assets)!.Name.Should().Be("cmux-windows-v1.0.10-win-x64.zip");
    }

    [Theory]
    [InlineData("https://github.com/GitDakky/cmux-windows/releases/download/v1.0.10/cmux-windows-v1.0.10-win-x64.zip", true)]
    [InlineData("https://objects.githubusercontent.com/github-production-release-asset-123", true)]
    [InlineData("https://evil.example/update.zip", false)]
    public void IsTrustedDownloadUrl_ValidatesHost(string url, bool expected)
    {
        IsTrustedDownloadUrl(url).Should().Be(expected);
    }

    [Theory]
    [InlineData("1.0.10", "1.0.9", true)]
    [InlineData("v1.1.0", "1.0.9", true)]
    [InlineData("1.0.9", "1.0.9", false)]
    [InlineData("1.0.8", "1.0.9", false)]
    public void ApplicationVersion_IsNewer(string latest, string current, bool expected)
    {
        ApplicationVersion.IsNewer(latest, current).Should().Be(expected);
    }

    [Fact]
    public void TrimReleaseNotes_TruncatesLongText()
    {
        var body = new string('a', 2000);
        TrimReleaseNotes(body, 100).Should().HaveLength(101);
        TrimReleaseNotes(body, 100).Should().EndWith("…");
    }
}
