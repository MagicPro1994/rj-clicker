using System.Windows.Input;

namespace RjClicker.App.Core.Services;

/// <summary>
/// Win32 global hotkey service stub.
/// Will be fully implemented in Task 8 with RegisterHotKey/UnregisterHotKey PInvoke calls.
/// </summary>
public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService
{
    public Task RegisterAsync(int hotkeyId, ModifierKeys modifiers, Key key, Func<Task> onPressed)
    {
        ArgumentNullException.ThrowIfNull(onPressed);
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(int hotkeyId)
    {
        return Task.CompletedTask;
    }
}
