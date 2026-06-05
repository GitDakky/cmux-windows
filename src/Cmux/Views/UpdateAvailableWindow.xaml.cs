using System.Windows;
using Cmux.Core.Services;

namespace Cmux.Views;

public partial class UpdateAvailableWindow : Window
{
    public bool InstallRequested { get; private set; }
    public bool RemindLater { get; private set; }

    public UpdateAvailableWindow(UpdateCheckResult update)
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        VersionText.Text = $"v{update.CurrentVersion}  →  v{update.LatestVersion}";
        ReleaseNotesText.Text = update.ReleaseNotes ?? "See GitHub for release notes.";
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        InstallRequested = true;
        DialogResult = true;
        Close();
    }

    private void Later_Click(object sender, RoutedEventArgs e)
    {
        RemindLater = true;
        DialogResult = false;
        Close();
    }
}
