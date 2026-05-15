using System.Windows;

namespace RjClicker.App.Core.Services;

/// <summary>
/// Win32 window binding service stub.
/// Will be fully implemented in Task 8 with GetWindowRect/FindWindow/GetForegroundWindow PInvoke calls.
/// </summary>
public sealed class Win32WindowBindingService : IWindowBindingService
{
    public Task<nint> GetWindowHandleAsync(string windowTitle)
    {
        ArgumentNullException.ThrowIfNull(windowTitle);
        // Stub: returns 0 (window not found) for now
        return Task.FromResult(nint.Zero);
    }

    public Task<Rect> GetWindowBoundsAsync(nint windowHandle)
    {
        // Stub: returns empty rect for now
        return Task.FromResult(Rect.Empty);
    }
}
