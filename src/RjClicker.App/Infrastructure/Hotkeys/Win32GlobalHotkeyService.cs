using System.Windows.Input;
using RjClicker.App.Infrastructure.PInvoke;

namespace RjClicker.App.Infrastructure.Hotkeys;

/// <summary>
/// Win32 global hotkey service stub.
/// Will be fully implemented in Task 8 with RegisterHotKey/UnregisterHotKey PInvoke calls.
/// </summary>
public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService
{
    private readonly Dictionary<int, nint> _registeredWindowHandles = [];
    private readonly IWin32Api _win32Api;

    public Win32GlobalHotkeyService()
        : this(new Win32Api())
    {
    }

    public Win32GlobalHotkeyService(IWin32Api win32Api)
    {
        _win32Api = win32Api ?? throw new ArgumentNullException(nameof(win32Api));
    }

    public Task RegisterAsync(int hotkeyId, ModifierKeys modifiers, Key key, Func<Task> onPressed)
    {
        ArgumentNullException.ThrowIfNull(onPressed);

        var targetWindowHandle = _win32Api.GetForegroundWindow();
        var virtualKeyCode = (uint)KeyInterop.VirtualKeyFromKey(key);
        var modifierMask = (uint)modifiers;

        var isRegistered = _win32Api.RegisterHotKey(targetWindowHandle, hotkeyId, modifierMask, virtualKeyCode);
        if (isRegistered)
        {
            _registeredWindowHandles[hotkeyId] = targetWindowHandle;
        }

        return Task.CompletedTask;
    }

    public Task UnregisterAsync(int hotkeyId)
    {
        _registeredWindowHandles.TryGetValue(hotkeyId, out var targetWindowHandle);
        _win32Api.UnregisterHotKey(targetWindowHandle, hotkeyId);
        _registeredWindowHandles.Remove(hotkeyId);

        return Task.CompletedTask;
    }
}
