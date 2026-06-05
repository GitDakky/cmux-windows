using System.Windows;
using Cmux.Core.Services;

namespace Cmux.Views;

public partial class UpdateProgressWindow : Window
{
    public UpdateProgressWindow(UpdateCheckResult update)
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        StatusText.Text = $"Downloading cmux v{update.LatestVersion}…";
    }

    public void ReportDownloadProgress(long received, long? total)
    {
        if (total is > 0)
        {
            var percent = received * 100.0 / total.Value;
            DownloadProgress.IsIndeterminate = false;
            DownloadProgress.Value = percent;
            ProgressText.Text = $"{percent:0}% ({FormatBytes(received)} / {FormatBytes(total.Value)})";
            return;
        }

        DownloadProgress.IsIndeterminate = true;
        ProgressText.Text = FormatBytes(received);
    }

    public void SetStatus(string status)
    {
        StatusText.Text = status;
        DownloadProgress.IsIndeterminate = true;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        double value = bytes;
        string[] units = ["KB", "MB", "GB"];
        foreach (var unit in units)
        {
            value /= 1024;
            if (value < 1024)
                return $"{value:0.#} {unit}";
        }

        return $"{value:0.#} TB";
    }
}
