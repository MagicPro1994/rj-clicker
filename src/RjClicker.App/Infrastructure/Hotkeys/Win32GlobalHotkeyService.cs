using System.Windows.Input;
using RjClicker.App.Infrastructure.PInvoke;

namespace RjClicker.App.Infrastructure.Hotkeys;

public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService
{
    private readonly Dictionary<int, nint> _registeredWindowHandles = [];
    private readonly Dictionary<int, Func<Task>> _registeredCallbacks = [];
    private readonly IWin32Api _win32Api;

    public Win32GlobalHotkeyService()
        : this(new Win32Api())
    {
    }

    public Win32GlobalHotkeyService(IWin32Api win32Api)
    {
        _win32Api = win32Api ?? throw new ArgumentNullException(nameof(win32Api));
    }

    public Task RegisterAsync(nint windowHandle, int hotkeyId, ModifierKeys modifiers, Key key, Func<Task> onPressed)
    {
        ArgumentNullException.ThrowIfNull(onPressed);

        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("Window handle must be non-zero", nameof(windowHandle));
        }

        var virtualKeyCode = (uint)KeyInterop.VirtualKeyFromKey(key);
        var modifierMask = (uint)modifiers;

        var isRegistered = _win32Api.RegisterHotKey(windowHandle, hotkeyId, modifierMask, virtualKeyCode);
        if (isRegistered)
        {
            _registeredWindowHandles[hotkeyId] = windowHandle;
            _registeredCallbacks[hotkeyId] = onPressed;
        }

        return Task.CompletedTask;
    }

    public Task UnregisterAsync(int hotkeyId)
    {
        _registeredWindowHandles.TryGetValue(hotkeyId, out var targetWindowHandle);
        _win32Api.UnregisterHotKey(targetWindowHandle, hotkeyId);
        _registeredWindowHandles.Remove(hotkeyId);
        _registeredCallbacks.Remove(hotkeyId);

        return Task.CompletedTask;
    }

    public void HandleHotkeyPressed(int hotkeyId)
    {
        if (!_registeredCallbacks.TryGetValue(hotkeyId, out var callback))
        {
            return;
        }

        _ = callback();
    }
}
