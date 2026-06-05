using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cmux.Core.Services;

/// <summary>
/// Checks GitHub Releases for a newer cmux-windows build.
/// </summary>
public static class GitHubUpdateChecker
{
    public const string RepoOwner = "GitDakky";
    public const string RepoName = "cmux-windows";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<UpdateCheckResult> CheckForUpdateAsync(
        Version currentVersion,
        string? dismissedVersion = null,
        bool includePrereleases = false,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        var currentText = currentVersion.ToString(3).TrimEnd('.', '0');
        if (currentText.EndsWith('.'))
            currentText = currentText.TrimEnd('.');

        var ownsClient = httpClient == null;
        httpClient ??= CreateHttpClient();

        try
        {
            var release = await FetchLatestReleaseAsync(httpClient, includePrereleases, cancellationToken);
            if (release == null)
                return UpdateCheckResult.Failed(currentText, "No GitHub release found.");

            var latestText = ApplicationVersion.NormalizeTag(release.TagName);
            if (!ApplicationVersion.IsNewer(latestText, currentText))
                return UpdateCheckResult.UpToDate(currentText);

            if (!string.IsNullOrWhiteSpace(dismissedVersion) &&
                string.Equals(ApplicationVersion.NormalizeTag(dismissedVersion), latestText, StringComparison.OrdinalIgnoreCase))
            {
                return UpdateCheckResult.UpToDate(currentText);
            }

            var asset = FindWindowsAsset(release.Assets);
            if (asset == null)
            {
                return UpdateCheckResult.Failed(
                    currentText,
                    "Latest release does not include a cmux-windows-v*-win-x64.zip asset.");
            }

            if (!IsTrustedDownloadUrl(asset.BrowserDownloadUrl))
            {
                return UpdateCheckResult.Failed(currentText, "Release download URL is not from GitHub.");
            }

            return new UpdateCheckResult
            {
                UpdateAvailable = true,
                CurrentVersion = currentText,
                LatestVersion = latestText,
                ReleaseName = release.Name,
                ReleaseNotes = TrimReleaseNotes(release.Body),
                DownloadUrl = asset.BrowserDownloadUrl,
                AssetName = asset.Name,
            };
        }
        catch (Exception ex)
        {
            return UpdateCheckResult.Failed(currentText, ex.Message);
        }
        finally
        {
            if (ownsClient)
                httpClient.Dispose();
        }
    }

    internal static GitHubReleaseAsset? FindWindowsAsset(IEnumerable<GitHubReleaseAsset> assets)
    {
        return assets.FirstOrDefault(a =>
            a.Name.StartsWith("cmux-windows-v", StringComparison.OrdinalIgnoreCase) &&
            a.Name.EndsWith("-win-x64.zip", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsTrustedDownloadUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("objects.githubusercontent.com", StringComparison.OrdinalIgnoreCase);
    }

    internal static string TrimReleaseNotes(string? body, int maxLength = 1200)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "See the release notes on GitHub for details.";

        body = body.Trim();
        return body.Length <= maxLength ? body : body[..maxLength].TrimEnd() + "…";
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20),
        };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("cmux-windows", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static async Task<GitHubReleaseResponse?> FetchLatestReleaseAsync(
        HttpClient httpClient,
        bool includePrereleases,
        CancellationToken cancellationToken)
    {
        if (!includePrereleases)
        {
            var latestUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            return await GetReleaseAsync(httpClient, latestUrl, cancellationToken);
        }

        var listUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases?per_page=10";
        using var response = await httpClient.GetAsync(listUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var releases = await JsonSerializer.DeserializeAsync<List<GitHubReleaseResponse>>(stream, JsonOptions, cancellationToken);
        return releases?.FirstOrDefault(r => !r.Draft);
    }

    private static async Task<GitHubReleaseResponse?> GetReleaseAsync(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<GitHubReleaseResponse>(stream, JsonOptions, cancellationToken);
    }

    internal sealed class GitHubReleaseResponse
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("body")]
        public string Body { get; set; } = "";

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubReleaseAsset> Assets { get; set; } = [];
    }

    internal sealed class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";
    }
}
