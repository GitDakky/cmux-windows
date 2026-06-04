namespace Cmux.Core.Config;

/// <summary>
/// User preferences for agent attention signals (toast, taskbar flash).
/// </summary>
public class NotificationSettings
{
    /// <summary>Show Windows toast notifications for agent/terminal alerts.</summary>
    public bool EnableToastNotifications { get; set; } = true;

    /// <summary>Only show toasts when the cmux window is not focused.</summary>
    public bool ToastOnlyWhenUnfocused { get; set; } = true;

    /// <summary>Flash the taskbar button when a notification arrives.</summary>
    public bool EnableTaskbarFlash { get; set; } = true;

    /// <summary>Only flash the taskbar when the cmux window is not focused.</summary>
    public bool FlashOnlyWhenUnfocused { get; set; } = true;
}
