using Cmux.Core.Models;
using Cmux.Core.Services;
using FluentAssertions;
using Xunit;

namespace Cmux.Tests;

public class NotificationServiceTests
{
    [Fact]
    public void MarkPaneAsRead_ClearsUnreadForPaneOnly()
    {
        var service = new NotificationService();

        service.AddNotification("ws1", "surf1", "pane-a", "T", null, "A", NotificationSource.Osc9);
        service.AddNotification("ws1", "surf1", "pane-b", "T", null, "B", NotificationSource.Osc9);

        service.MarkPaneAsRead("ws1", "surf1", "pane-a");

        service.HasUnreadForPane("ws1", "surf1", "pane-a").Should().BeFalse();
        service.HasUnreadForPane("ws1", "surf1", "pane-b").Should().BeTrue();
        service.GetUnreadCountForSurface("ws1", "surf1").Should().Be(1);
    }

    [Fact]
    public void GetUnreadCountForSurface_CountsAllUnreadOnSurface()
    {
        var service = new NotificationService();

        service.AddNotification("ws1", "surf1", "p1", "T", null, "1", NotificationSource.Cli);
        service.AddNotification("ws1", "surf1", "p2", "T", null, "2", NotificationSource.Cli);
        service.AddNotification("ws1", "surf2", "p1", "T", null, "3", NotificationSource.Cli);

        service.GetUnreadCountForSurface("ws1", "surf1").Should().Be(2);
        service.GetUnreadCountForSurface("ws1", "surf2").Should().Be(1);
    }
}
