using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Delivery;

public interface IBackgroundClickService
{
    Task<BackgroundClickResult> ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType);
}