using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Cmux.Services;

/// <summary>
/// Flashes the cmux window on the taskbar when an agent needs attention.
/// </summary>
public static class TaskbarFlashHelper
{
    [DllImport("user32.dll")]
    private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

    public static void Flash(Window? window)
    {
        if (window == null)
            return;

        try
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle != IntPtr.Zero)
                FlashWindow(handle, true);
        }
        catch
        {
            // Non-critical on headless or unusual hosts
        }
    }
}
