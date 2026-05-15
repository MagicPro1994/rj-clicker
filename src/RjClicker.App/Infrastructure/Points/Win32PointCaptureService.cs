using System.Windows;
using RjClicker.App.Infrastructure.PInvoke;

namespace RjClicker.App.Infrastructure.Points;

/// <summary>
/// Win32 point capture service stub.
/// Will be fully implemented in Task 8 with SetWindowsHookEx mouse event hook PInvoke calls.
/// </summary>
public sealed class Win32PointCaptureService : IPointCaptureService
{
    private readonly IWin32Api _win32Api;

    public Win32PointCaptureService()
        : this(new Win32Api())
    {
    }

    public Win32PointCaptureService(IWin32Api win32Api)
    {
        _win32Api = win32Api ?? throw new ArgumentNullException(nameof(win32Api));
    }

    public async Task<Point?> CapturePointAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        var completion = new TaskCompletionSource<Point?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var hookHandle = nint.Zero;
        var unhooked = 0;

        void Unhook()
        {
            if (Interlocked.Exchange(ref unhooked, 1) == 1)
            {
                return;
            }

            if (hookHandle != nint.Zero)
            {
                _win32Api.UnhookWindowsHookEx(hookHandle);
            }
        }

        NativeMethods.HookProc? hookProc = null;
        hookProc = (code, wParam, lParam) =>
        {
            if (code >= 0
                && IsMouseButtonDownMessage(wParam)
                && _win32Api.TryReadMouseHookStruct(lParam, out var hookStruct))
            {
                completion.TrySetResult(new Point(hookStruct.Pt.X, hookStruct.Pt.Y));
                Unhook();
                return nint.Zero;
            }

            return _win32Api.CallNextHookEx(hookHandle, code, wParam, lParam);
        };

        var moduleHandle = _win32Api.GetModuleHandle(null);
        hookHandle = _win32Api.SetWindowsHookEx(NativeConstants.WhMouseLl, hookProc, moduleHandle, 0);
        if (hookHandle == nint.Zero)
        {
            return null;
        }

        using var registration = cancellationToken.Register(() =>
        {
            completion.TrySetResult(null);
            Unhook();
        });

        var capturedPoint = await completion.Task.ConfigureAwait(false);
        Unhook();

        return capturedPoint;
    }

    private static bool IsMouseButtonDownMessage(nuint message)
    {
        return message == NativeConstants.WmLButtonDown || message == NativeConstants.WmRButtonDown;
    }
}
