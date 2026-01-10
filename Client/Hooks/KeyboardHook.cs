namespace Client.Hooks;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

public class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    
    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    
    private LowLevelKeyboardProc _proc;
    private IntPtr _hookId = IntPtr.Zero;
    
    public KeyboardHook()
    {
        _proc = HookCallback;
    }
    
    public void Install()
    {
        _hookId = SetHook(_proc);
    }
    
    public void Uninstall()
    {
        UnhookWindowsHookEx(_hookId);
    }
    
    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule? curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule?.ModuleName ?? ""), 0);
        }
    }
    
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            
            // Block Windows Key (91, 92)
            if (vkCode == 91 || vkCode == 92)
                return (IntPtr)1;
            
            // Block Alt+F4
            if (vkCode == (int)Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt)
                return (IntPtr)1;
            
            // Block Ctrl+Shift+Esc (Task Manager)
            if (vkCode == (int)Key.Escape && 
                (Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
                (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                return (IntPtr)1;
        }
        
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }
    
    public void Dispose()
    {
        Uninstall();
    }
}