namespace Cmux.Core.Models;

public class Surface
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Terminal";
    public SurfaceKind Kind { get; set; } = SurfaceKind.Terminal;
    public string? BrowserProfileId { get; set; }
    public string? BrowserStartUrl { get; set; }
    public string? BrowserLastUrl { get; set; }
    public SplitNode RootSplitNode { get; set; } = SplitNode.CreateLeaf();
    public string? FocusedPaneId { get; set; }
    public Dictionary<string, string> PaneCustomNames { get; set; } = [];
    public Dictionary<string, PaneStateSnapshot> PaneSnapshots { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
