using System.Windows;

namespace RjClicker.App.Infrastructure.Windows;

public interface IWindowBindingService
{
    Task<nint> GetWindowHandleAsync(string windowTitle);
    Task<Rect> GetWindowBoundsAsync(nint windowHandle);
}
