using System.Windows;

namespace RjClicker.App.Core.Services;

public interface IWindowBindingService
{
    Task<nint> GetWindowHandleAsync(string windowTitle);
    Task<Rect> GetWindowBoundsAsync(nint windowHandle);
}
