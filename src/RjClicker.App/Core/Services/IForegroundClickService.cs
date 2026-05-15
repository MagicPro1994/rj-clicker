using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Services;

public interface IForegroundClickService
{
    Task ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType);
}
