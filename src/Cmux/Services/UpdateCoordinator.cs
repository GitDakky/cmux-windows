using System.Reflection;
using System.Windows;
using Cmux.Core.Config;
using Cmux.Core.Services;
using Cmux.Views;

namespace Cmux.Services;

/// <summary>
/// Background update checks and user prompts.
/// </summary>
public static class UpdateCoordinator
{
    private static readonly AppUpdateService UpdateService = new();
    private static bool _promptVisible;

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 9);

    public static async Task CheckOnStartupAsync(Window owner)
    {
        var settings = SettingsService.Current.Updates ?? new UpdateSettings();
        if (!settings.CheckOnStartup)
            return;

        if (!ShouldCheckNow(settings))
            return;

        await Task.Delay(TimeSpan.FromSeconds(8));
        if (!owner.IsLoaded)
            return;

        await CheckAndPromptAsync(owner, manual: false);
    }

    public static async Task CheckAndPromptAsync(Window owner, bool manual = true)
    {
        var settings = SettingsService.Current;
        settings.Updates ??= new UpdateSettings();

        var result = await GitHubUpdateChecker.CheckForUpdateAsync(
            CurrentVersion,
            manual ? null : settings.Updates.DismissedVersion);

        settings.Updates.LastCheckUtc = DateTime.UtcNow;
        SettingsService.Save(settings);

        if (!string.IsNullOrWhiteSpace(result.Error) && manual)
        {
            MessageBox.Show(
                owner,
                $"Could not check for updates:\n{result.Error}",
                "cmux updates",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (!result.UpdateAvailable)
        {
            if (manual)
            {
                MessageBox.Show(
                    owner,
                    $"You are running the latest release (v{result.CurrentVersion}).",
                    "cmux updates",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            return;
        }

        if (_promptVisible)
            return;

        _promptVisible = true;
        try
        {
            var dialog = new UpdateAvailableWindow(result)
            {
                Owner = owner,
            };

            if (dialog.ShowDialog() != true || !dialog.InstallRequested)
            {
                if (dialog.RemindLater)
                {
                    settings.Updates.DismissedVersion = result.LatestVersion;
                    SettingsService.Save(settings);
                }

                return;
            }

            await InstallUpdateAsync(owner, result);
        }
        finally
        {
            _promptVisible = false;
        }
    }

    private static bool ShouldCheckNow(UpdateSettings settings)
    {
        if (settings.CheckIntervalHours <= 0)
            return true;

        if (settings.LastCheckUtc is not DateTime lastCheck)
            return true;

        return DateTime.UtcNow - lastCheck >= TimeSpan.FromHours(settings.CheckIntervalHours);
    }

    private static async Task InstallUpdateAsync(Window owner, UpdateCheckResult update)
    {
        var progressWindow = new UpdateProgressWindow(update)
        {
            Owner = owner,
        };
        progressWindow.Show();

        try
        {
            var progress = new Progress<(long received, long? total)>(value =>
            {
                progressWindow.ReportDownloadProgress(value.received, value.total);
            });

            await UpdateService.DownloadAsync(update, progress);
            progressWindow.SetStatus("Extracting update…");
            await Task.Run(UpdateService.ExtractDownload);

            progressWindow.Close();

            var restart = MessageBox.Show(
                owner,
                $"cmux {update.LatestVersion} is ready to install.\n\n" +
                "The app will close briefly while files are replaced, then restart automatically.",
                "Restart to apply update",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information,
                MessageBoxResult.OK);

            if (restart != MessageBoxResult.OK)
                return;

            var layout = AppInstallLayout.Detect();
            var pid = Environment.ProcessId;
            UpdateService.LaunchUpdaterAndShutdown(layout, pid);
        }
        catch (Exception ex)
        {
            progressWindow.Close();
            MessageBox.Show(
                owner,
                $"Update failed:\n{ex.Message}",
                "cmux updates",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
