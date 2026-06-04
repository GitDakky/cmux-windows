using Cmux.Core.Services;
using FluentAssertions;
using Xunit;

namespace Cmux.Tests;

public class PaneActivityTrackerTests
{
    public PaneActivityTrackerTests()
    {
        PaneActivityTracker.ResetForTests();
    }

    [Fact]
    public void GetIdlePaneIds_ReturnsPaneAfterThreshold()
    {
        PaneActivityTracker.RecordOutput("pane-1");

        var activityType = typeof(PaneActivityTracker);
        // Force last output into the past by re-recording then waiting is slow — use reflection on internal state
        // Instead: record, mark notified false, and use very short threshold with manual time manipulation via second record pattern.

        // Simulate idle by not recording again and using 0-second threshold after busy mark
        var idle = PaneActivityTracker.GetIdlePaneIds(TimeSpan.Zero);
        idle.Should().Contain("pane-1");
    }

    [Fact]
    public void MarkIdleNotified_SuppressesRepeatUntilNewOutput()
    {
        PaneActivityTracker.RecordOutput("pane-2");
        PaneActivityTracker.MarkIdleNotified("pane-2");

        PaneActivityTracker.GetIdlePaneIds(TimeSpan.Zero).Should().NotContain("pane-2");

        PaneActivityTracker.RecordOutput("pane-2");
        PaneActivityTracker.GetIdlePaneIds(TimeSpan.Zero).Should().Contain("pane-2");
    }

    [Fact]
    public void Remove_ClearsTracking()
    {
        PaneActivityTracker.RecordOutput("pane-3");
        PaneActivityTracker.Remove("pane-3");

        PaneActivityTracker.GetIdlePaneIds(TimeSpan.FromMinutes(1)).Should().BeEmpty();
    }
}
