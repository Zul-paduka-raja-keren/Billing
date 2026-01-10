using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

public static class SystemLocker
{
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x80000;
    
    public static void DisableTaskManager()
    {
        // Registry edit (requires admin)
        try
        {
            Microsoft.Win32.Registry.CurrentUser
                .CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")
                .SetValue("DisableTaskMgr", 1);
        }
        catch { }
    }
    
    public static void EnableTaskManager()
    {
        try
        {
            Microsoft.Win32.Registry.CurrentUser
                .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true)
                ?.DeleteValue("DisableTaskMgr", false);
        }
        catch { }
    }
    
    // Block Ctrl+Alt+Del (impossible sepenuhnya, tapi bisa minimize impact)
    public static void HookKeyboard()
    {
        // Gunakan low-level keyboard hook (advanced)
        // Atau cukup handle di WPF:
        // PreviewKeyDown += (s, e) => 
        // {
        //     if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && 
        //         e.Key == Key.Delete)
        //     {
        //         e.Handled = true;
        //     }
        // };
    }
}