using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Services;

public interface IBackgroundClickService
{
    Task<BackgroundClickResult> ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType);
}
