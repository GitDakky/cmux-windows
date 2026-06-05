using System.Windows;
using System.Windows.Controls;
using Cmux.ViewModels;

namespace Cmux.Controls;

/// <summary>
/// Hosts either a terminal split layout or an embedded browser for the active surface.
/// </summary>
public class SurfaceContentHost : ContentControl
{
    private readonly SplitPaneContainer _splitPane = new();
    private BrowserControl? _browser;
    private SurfaceViewModel? _surface;

    public event Action? SearchRequested;

    public SplitPaneContainer SplitPaneContainer => _splitPane;

    public BrowserControl? BrowserControl => _browser;

    public SurfaceContentHost()
    {
        Background = System.Windows.Media.Brushes.Transparent;
        Content = _splitPane;
        _splitPane.SearchRequested += () => SearchRequested?.Invoke();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SurfaceViewModel oldSurface)
            oldSurface.DetachBrowserControl();

        _surface = e.NewValue as SurfaceViewModel;
        if (_surface == null)
        {
            Content = null;
            return;
        }

        if (_surface.IsBrowser)
        {
            _browser = new BrowserControl();
            Content = _browser;
            _surface.AttachBrowserControl(_browser);
            return;
        }

        _splitPane.DataContext = _surface;
        Content = _splitPane;
    }
}
