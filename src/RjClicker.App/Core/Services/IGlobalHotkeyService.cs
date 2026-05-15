using System.Windows.Input;

namespace RjClicker.App.Core.Services;

public interface IGlobalHotkeyService
{
    Task RegisterAsync(int hotkeyId, ModifierKeys modifiers, Key key, Func<Task> onPressed);
    Task UnregisterAsync(int hotkeyId);
}
