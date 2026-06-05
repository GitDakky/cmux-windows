using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cmux.Core.Config;
using Cmux.Core.Services;
using Cmux.Models;
using Microsoft.Web.WebView2.Core;

namespace Cmux.Controls;

public partial class BrowserControl : UserControl
{
    private static readonly ConcurrentDictionary<string, Task<CoreWebView2Environment>> EnvironmentCache = new();

    private BrowserProfile? _profile;
    private string _currentUrl = "about:blank";
    private bool _isExternalMode;

    public event Action? CloseRequested;
    public event Action<string>? UrlChanged;

    public BrowserControl()
    {
        InitializeComponent();
    }

    public bool IsEmbeddedReady => !_isExternalMode && WebView.CoreWebView2 != null;

    public async Task InitializeEmbeddedAsync(BrowserLaunchContext context)
    {
        _isExternalMode = false;
        ExternalPanel.Visibility = Visibility.Collapsed;
        WebView.Visibility = Visibility.Visible;

        _profile = context.Profile;
        _currentUrl = BrowserProfileService.NormalizeUrl(context.StartUrl);

        var userDataFolder = BrowserProfileService.GetUserDataFolder(
            context.Profile,
            context.WorkspaceId,
            context.SurfaceId);
        Directory.CreateDirectory(userDataFolder);

        var environment = await GetOrCreateEnvironmentAsync(userDataFolder, context.Profile);
        await WebView.EnsureCoreWebView2Async(environment);

        WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;

        Navigate(_currentUrl);
    }

    public void ShowExternalMode(BrowserProfile profile, string url)
    {
        _isExternalMode = true;
        _profile = profile;
        _currentUrl = BrowserProfileService.NormalizeUrl(url);

        WebView.Visibility = Visibility.Collapsed;
        ExternalPanel.Visibility = Visibility.Visible;
        ExternalTitleText.Text = profile.Name;
        ExternalDetailText.Text = $"Opened {_currentUrl} in your external browser. Embedded WebView2 is disabled for this profile.";
        AddressBar.Text = _currentUrl;
    }

    public void Navigate(string url)
    {
        url = BrowserProfileService.NormalizeUrl(url);
        _currentUrl = url;
        AddressBar.Text = url;

        if (_isExternalMode)
        {
            if (_profile != null)
                BrowserProfileService.TryLaunchExternal(_profile, url, out _);
            return;
        }

        try
        {
            WebView.CoreWebView2?.Navigate(url);
        }
        catch
        {
            // Invalid URL
        }
    }

    public async Task<string> EvaluateJavaScript(string script)
    {
        if (_isExternalMode || WebView.CoreWebView2 == null)
            return "";

        return await WebView.CoreWebView2.ExecuteScriptAsync(script);
    }

    public async Task<string> GetAccessibilitySnapshot()
    {
        const string script = """
            (function() {
                function walk(node) {
                    const result = {
                        role: node.getAttribute('role') || node.tagName.toLowerCase(),
                        name: node.getAttribute('aria-label') || node.textContent?.substring(0, 100) || '',
                        children: []
                    };
                    for (const child of node.children) {
                        result.children.push(walk(child));
                    }
                    return result;
                }
                return JSON.stringify(walk(document.body));
            })()
            """;
        return await EvaluateJavaScript(script);
    }

    public async Task ClickElement(string selector)
    {
        var escapedSelector = selector.Replace("'", "\\'");
        await EvaluateJavaScript($"document.querySelector('{escapedSelector}')?.click()");
    }

    public async Task FillElement(string selector, string value)
    {
        var escapedSelector = selector.Replace("'", "\\'");
        var escapedValue = value.Replace("'", "\\'");
        await EvaluateJavaScript($"""
            (() => {{
                const el = document.querySelector('{escapedSelector}');
                if (el) {{
                    el.value = '{escapedValue}';
                    el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                    el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                }}
            }})()
            """);
    }

    public string GetCurrentUrl()
    {
        if (_isExternalMode)
            return _currentUrl;

        return WebView.CoreWebView2?.Source ?? _currentUrl;
    }

    public void RelaunchExternal()
    {
        if (_profile == null)
            return;

        BrowserProfileService.TryLaunchExternal(_profile, _currentUrl, out _);
    }

    private static Task<CoreWebView2Environment> GetOrCreateEnvironmentAsync(
        string userDataFolder,
        BrowserProfile profile)
    {
        var cacheKey = userDataFolder;
        if (!string.IsNullOrWhiteSpace(profile.BrowserExecutablePath))
            cacheKey += "|" + profile.BrowserExecutablePath.Trim();

        return EnvironmentCache.GetOrAdd(cacheKey, _ => CreateEnvironmentAsync(userDataFolder, profile));
    }

    private static async Task<CoreWebView2Environment> CreateEnvironmentAsync(
        string userDataFolder,
        BrowserProfile profile)
    {
        string? browserExecutableFolder = null;
        if (!string.IsNullOrWhiteSpace(profile.BrowserExecutablePath))
        {
            var exePath = Environment.ExpandEnvironmentVariables(profile.BrowserExecutablePath.Trim());
            if (File.Exists(exePath))
                browserExecutableFolder = Path.GetDirectoryName(exePath);
        }

        return await CoreWebView2Environment.CreateAsync(browserExecutableFolder, userDataFolder);
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CoreWebView2?.CanGoBack == true)
            WebView.CoreWebView2.GoBack();
    }

    private void Forward_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CoreWebView2?.CanGoForward == true)
            WebView.CoreWebView2.GoForward();
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        if (_isExternalMode)
        {
            RelaunchExternal();
            return;
        }

        WebView.CoreWebView2?.Reload();
    }

    private void CloseBrowser_Click(object sender, RoutedEventArgs e) => CloseRequested?.Invoke();

    private void ExternalOpen_Click(object sender, RoutedEventArgs e) => RelaunchExternal();

    private void AddressBar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        Navigate(AddressBar.Text);
        e.Handled = true;
    }

    private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        UpdateUrlFromWebView();
    }

    private void WebView_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        UpdateUrlFromWebView();
    }

    private void UpdateUrlFromWebView()
    {
        var url = WebView.CoreWebView2?.Source;
        if (string.IsNullOrWhiteSpace(url))
            return;

        _currentUrl = url;
        AddressBar.Text = url;
        UrlChanged?.Invoke(url);
    }
}
