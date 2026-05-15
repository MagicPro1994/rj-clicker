using System.Windows.Input;

namespace RjClicker.App.Infrastructure.Hotkeys;

public interface IGlobalHotkeyService
{
    Task RegisterAsync(int hotkeyId, ModifierKeys modifiers, Key key, Func<Task> onPressed);
    Task UnregisterAsync(int hotkeyId);
}
