namespace Cmux.Core.Services;

/// <summary>
/// Tracks terminal output activity per pane for idle/attention heuristics.
/// </summary>
public static class PaneActivityTracker
{
    private sealed class PaneActivity
    {
        public DateTime LastOutputUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastBusyUtc { get; set; }
        public bool IdleNotified;
    }

    private static readonly Dictionary<string, PaneActivity> _panes = new();
    private static readonly object _lock = new();

    public static void RecordOutput(string paneId)
    {
        if (string.IsNullOrEmpty(paneId)) return;

        lock (_lock)
        {
            var activity = GetOrCreate(paneId);
            var now = DateTime.UtcNow;
            activity.LastOutputUtc = now;
            activity.LastBusyUtc = now;
            activity.IdleNotified = false;
        }
    }

    public static void MarkIdleNotified(string paneId)
    {
        if (string.IsNullOrEmpty(paneId)) return;

        lock (_lock)
        {
            if (_panes.TryGetValue(paneId, out var activity))
                activity.IdleNotified = true;
        }
    }

    public static void Remove(string paneId)
    {
        if (string.IsNullOrEmpty(paneId)) return;

        lock (_lock)
            _panes.Remove(paneId);
    }

    /// <summary>
    /// Returns pane IDs that have been idle longer than <paramref name="threshold"/> since last output,
    /// had prior activity, and have not yet received an idle notification for this idle episode.
    /// </summary>
    public static IReadOnlyList<string> GetIdlePaneIds(TimeSpan threshold)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var result = new List<string>();

            foreach (var (paneId, activity) in _panes)
            {
                if (activity.IdleNotified)
                    continue;

                if (activity.LastBusyUtc == default)
                    continue;

                if (now - activity.LastOutputUtc >= threshold)
                    result.Add(paneId);
            }

            return result;
        }
    }

    internal static void ResetForTests()
    {
        lock (_lock)
            _panes.Clear();
    }

    private static PaneActivity GetOrCreate(string paneId)
    {
        if (!_panes.TryGetValue(paneId, out var activity))
        {
            activity = new PaneActivity();
            _panes[paneId] = activity;
        }

        return activity;
    }
}
