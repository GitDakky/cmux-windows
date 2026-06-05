using Cmux.Core.Config;

namespace Cmux.Models;

public sealed class BrowserLaunchContext
{
    public required BrowserProfile Profile { get; init; }
    public required string WorkspaceId { get; init; }
    public required string SurfaceId { get; init; }
    public string StartUrl { get; init; } = "about:blank";
}
