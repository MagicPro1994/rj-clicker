using System.Windows;

namespace RjClicker.App.Core.Services;

/// <summary>
/// Win32 point capture service stub.
/// Will be fully implemented in Task 8 with SetWindowsHookEx mouse event hook PInvoke calls.
/// </summary>
public sealed class Win32PointCaptureService : IPointCaptureService
{
    public async Task<Point?> CapturePointAsync(CancellationToken cancellationToken)
    {
        // Stub: wait for cancellation or return null
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected: cancellation token was cancelled
        }

        return null;
    }
}
