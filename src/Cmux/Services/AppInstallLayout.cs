using System.IO;

namespace Cmux.Services;

/// <summary>
/// Detects where cmux is installed relative to cmuxw.exe.
/// Supports the recommended layout (installRoot/app + installRoot/cli) and flat installs.
/// </summary>
public sealed class AppInstallLayout
{
    public required string InstallRoot { get; init; }
    public required string AppDirectory { get; init; }
    public required string CliDirectory { get; init; }
    public required string MainExecutablePath { get; init; }

    public static AppInstallLayout Detect()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
            exePath = Path.Combine(AppContext.BaseDirectory, "cmuxw.exe");

        exePath = Path.GetFullPath(exePath);
        var appDirectory = Path.GetDirectoryName(exePath)
            ?? throw new InvalidOperationException("Could not determine application directory.");

        if (Path.GetFileName(appDirectory).Equals("app", StringComparison.OrdinalIgnoreCase))
        {
            var installRoot = Path.GetDirectoryName(appDirectory)
                ?? appDirectory;
            return new AppInstallLayout
            {
                InstallRoot = installRoot,
                AppDirectory = appDirectory,
                CliDirectory = Path.Combine(installRoot, "cli"),
                MainExecutablePath = exePath,
            };
        }

        return new AppInstallLayout
        {
            InstallRoot = appDirectory,
            AppDirectory = appDirectory,
            CliDirectory = appDirectory,
            MainExecutablePath = exePath,
        };
    }
}
