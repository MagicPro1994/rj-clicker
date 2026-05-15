using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Delivery;

public interface IForegroundClickService
{
    Task ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType);
}