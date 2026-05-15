using System.Windows;
using RjClicker.App.Infrastructure.PInvoke;

namespace RjClicker.App.Infrastructure.Windows;

/// <summary>
/// Win32 window binding service stub.
/// Will be fully implemented in Task 8 with GetWindowRect/FindWindow/GetForegroundWindow PInvoke calls.
/// </summary>
public sealed class Win32WindowBindingService : IWindowBindingService
{
    private readonly IWin32Api _win32Api;

    public Win32WindowBindingService()
        : this(new Win32Api())
    {
    }

    public Win32WindowBindingService(IWin32Api win32Api)
    {
        _win32Api = win32Api ?? throw new ArgumentNullException(nameof(win32Api));
    }

    public Task<nint> GetWindowHandleAsync(string windowTitle)
    {
        ArgumentNullException.ThrowIfNull(windowTitle);
        if (string.IsNullOrWhiteSpace(windowTitle))
        {
            return Task.FromResult(nint.Zero);
        }

        return Task.FromResult(_win32Api.FindWindow(null, windowTitle));
    }

    public Task<Rect> GetWindowBoundsAsync(nint windowHandle)
    {
        if (windowHandle == nint.Zero)
        {
            return Task.FromResult(Rect.Empty);
        }

        var hasBounds = _win32Api.GetWindowRect(windowHandle, out var rect);
        if (!hasBounds)
        {
            return Task.FromResult(Rect.Empty);
        }

        var width = Math.Max(0, rect.Right - rect.Left);
        var height = Math.Max(0, rect.Bottom - rect.Top);
        return Task.FromResult(new Rect(rect.Left, rect.Top, width, height));
    }
}
