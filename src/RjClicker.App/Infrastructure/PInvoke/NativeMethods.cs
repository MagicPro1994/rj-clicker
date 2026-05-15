using System.Runtime.InteropServices;

namespace RjClicker.App.Infrastructure.PInvoke;

public interface IWin32Api
{
    bool SetCursorPos(int x, int y);

    uint SendInput(uint inputCount, NativeMethods.INPUT[] inputs, int inputSize);

    bool PostMessage(nint windowHandle, uint message, nuint wParam, nint lParam);

    bool RegisterHotKey(nint windowHandle, int hotkeyId, uint modifiers, uint virtualKeyCode);

    bool UnregisterHotKey(nint windowHandle, int hotkeyId);

    bool GetWindowRect(nint windowHandle, out NativeMethods.RECT rect);

    nint GetForegroundWindow();

    nint FindWindow(string? className, string? windowTitle);

    nint SetWindowsHookEx(int hookType, NativeMethods.HookProc hookProcedure, nint moduleHandle, uint threadId);

    bool UnhookWindowsHookEx(nint hookHandle);

    nint CallNextHookEx(nint hookHandle, int code, nuint wParam, nint lParam);

    nint GetModuleHandle(string? moduleName);

    bool TryReadMouseHookStruct(nint lParam, out NativeMethods.MSLLHOOKSTRUCT hookStruct);
}

public sealed class Win32Api : IWin32Api
{
    public bool SetCursorPos(int x, int y)
    {
        return NativeMethods.SetCursorPos(x, y);
    }

    public uint SendInput(uint inputCount, NativeMethods.INPUT[] inputs, int inputSize)
    {
        return NativeMethods.SendInput(inputCount, inputs, inputSize);
    }

    public bool PostMessage(nint windowHandle, uint message, nuint wParam, nint lParam)
    {
        return NativeMethods.PostMessage(windowHandle, message, wParam, lParam);
    }

    public bool RegisterHotKey(nint windowHandle, int hotkeyId, uint modifiers, uint virtualKeyCode)
    {
        return NativeMethods.RegisterHotKey(windowHandle, hotkeyId, modifiers, virtualKeyCode);
    }

    public bool UnregisterHotKey(nint windowHandle, int hotkeyId)
    {
        return NativeMethods.UnregisterHotKey(windowHandle, hotkeyId);
    }

    public bool GetWindowRect(nint windowHandle, out NativeMethods.RECT rect)
    {
        return NativeMethods.GetWindowRect(windowHandle, out rect);
    }

    public nint GetForegroundWindow()
    {
        return NativeMethods.GetForegroundWindow();
    }

    public nint FindWindow(string? className, string? windowTitle)
    {
        return NativeMethods.FindWindow(className, windowTitle);
    }

    public nint SetWindowsHookEx(int hookType, NativeMethods.HookProc hookProcedure, nint moduleHandle, uint threadId)
    {
        return NativeMethods.SetWindowsHookEx(hookType, hookProcedure, moduleHandle, threadId);
    }

    public bool UnhookWindowsHookEx(nint hookHandle)
    {
        return NativeMethods.UnhookWindowsHookEx(hookHandle);
    }

    public nint CallNextHookEx(nint hookHandle, int code, nuint wParam, nint lParam)
    {
        return NativeMethods.CallNextHookEx(hookHandle, code, wParam, lParam);
    }

    public nint GetModuleHandle(string? moduleName)
    {
        return NativeMethods.GetModuleHandle(moduleName);
    }

    public bool TryReadMouseHookStruct(nint lParam, out NativeMethods.MSLLHOOKSTRUCT hookStruct)
    {
        try
        {
            hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            return true;
        }
        catch (ArgumentException)
        {
            hookStruct = default;
            return false;
        }
    }
}

public static class NativeMethods
{
    public delegate nint HookProc(int code, nuint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint Type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public nuint DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT Pt;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint DwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(nint hWnd, uint msg, nuint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(nint hWnd, out RECT rect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CallNextHookEx(nint hhk, int nCode, nuint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint GetModuleHandle(string? lpModuleName);
}