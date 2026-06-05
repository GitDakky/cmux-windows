using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using Cmux.Core.Services;

namespace Cmux.Services;

public sealed class AppUpdateService
{
    private static readonly string UpdatesRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "cmux-windows",
        "updates");

    public string StagingDirectory => Path.Combine(UpdatesRoot, "staging");
    public string DownloadPath => Path.Combine(UpdatesRoot, "download.zip");
    public string UpdaterScriptPath => Path.Combine(UpdatesRoot, "apply-update.ps1");

    public async Task DownloadAsync(
        UpdateCheckResult update,
        IProgress<(long received, long? total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(update.DownloadUrl))
            throw new InvalidOperationException("Missing download URL.");

        if (!GitHubUpdateChecker.IsTrustedDownloadUrl(update.DownloadUrl))
            throw new InvalidOperationException("Untrusted download URL.");

        Directory.CreateDirectory(UpdatesRoot);
        if (File.Exists(DownloadPath))
            File.Delete(DownloadPath);

        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        using var response = await client.GetAsync(
            update.DownloadUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(DownloadPath);

        var buffer = new byte[81920];
        long received = 0;
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            received += read;
            progress?.Report((received, total));
        }
    }

    public void ExtractDownload()
    {
        if (!File.Exists(DownloadPath))
            throw new FileNotFoundException("Update package has not been downloaded.", DownloadPath);

        if (Directory.Exists(StagingDirectory))
            Directory.Delete(StagingDirectory, recursive: true);

        Directory.CreateDirectory(StagingDirectory);
        ZipFile.ExtractToDirectory(DownloadPath, StagingDirectory);

        var appDir = Path.Combine(StagingDirectory, "app");
        var cliDir = Path.Combine(StagingDirectory, "cli");
        if (!Directory.Exists(appDir) || !File.Exists(Path.Combine(appDir, "cmuxw.exe")))
            throw new InvalidOperationException("Update package is missing the app/ folder or cmuxw.exe.");

        if (!Directory.Exists(cliDir) || !File.Exists(Path.Combine(cliDir, "cmux.exe")))
            throw new InvalidOperationException("Update package is missing the cli/ folder or cmux.exe.");
    }

    public void LaunchUpdaterAndShutdown(AppInstallLayout layout, int parentProcessId)
    {
        Directory.CreateDirectory(UpdatesRoot);
        WriteUpdaterScript();

        var arguments =
            $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{UpdaterScriptPath}\" " +
            $"-ParentProcessId {parentProcessId} " +
            $"-StagingRoot \"{StagingDirectory}\" " +
            $"-AppDir \"{layout.AppDirectory}\" " +
            $"-CliDir \"{layout.CliDirectory}\" " +
            $"-MainExe \"{layout.MainExecutablePath}\"";

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
        });

        System.Windows.Application.Current.Shutdown();
    }

    private void WriteUpdaterScript()
    {
        Directory.CreateDirectory(UpdatesRoot);

        var script = """
            param(
              [Parameter(Mandatory=$true)][int]$ParentProcessId,
              [Parameter(Mandatory=$true)][string]$StagingRoot,
              [Parameter(Mandatory=$true)][string]$AppDir,
              [Parameter(Mandatory=$true)][string]$CliDir,
              [Parameter(Mandatory=$true)][string]$MainExe
            )

            $ErrorActionPreference = 'Stop'

            while ($true) {
              $proc = Get-Process -Id $ParentProcessId -ErrorAction SilentlyContinue
              if ($null -eq $proc) { break }
              Start-Sleep -Milliseconds 400
            }

            Start-Sleep -Milliseconds 800

            New-Item -ItemType Directory -Force -Path $AppDir | Out-Null
            New-Item -ItemType Directory -Force -Path $CliDir | Out-Null

            Copy-Item -Path (Join-Path $StagingRoot 'app\*') -Destination $AppDir -Recurse -Force
            Copy-Item -Path (Join-Path $StagingRoot 'cli\*') -Destination $CliDir -Recurse -Force

            Start-Process -FilePath $MainExe
            """;

        File.WriteAllText(UpdaterScriptPath, script);
    }
}
