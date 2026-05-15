using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.PInvoke;

namespace RjClicker.App.Infrastructure.Delivery;

public sealed class Win32BackgroundClickService : IBackgroundClickService
{
    private readonly IWin32Api _win32Api;

    public Win32BackgroundClickService()
        : this(new Win32Api())
    {
    }

    public Win32BackgroundClickService(IWin32Api win32Api)
    {
        _win32Api = win32Api ?? throw new ArgumentNullException(nameof(win32Api));
    }

    public Task<BackgroundClickResult> ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType)
    {
        ArgumentNullException.ThrowIfNull(target);

        var targetWindowHandle = ResolveTargetWindowHandle(target);
        if (targetWindowHandle == nint.Zero)
        {
            return Task.FromResult(new BackgroundClickResult(false, "Target window not found"));
        }

        var clickCycles = pressType == PressType.Double ? 2 : 1;
        var (downMessage, upMessage, downWParam) = GetMessages(button);
        var lParam = BuildLParam(target.X, target.Y);

        for (var cycle = 0; cycle < clickCycles; cycle++)
        {
            var downSent = _win32Api.PostMessage(targetWindowHandle, downMessage, downWParam, lParam);
            var upSent = _win32Api.PostMessage(targetWindowHandle, upMessage, 0, lParam);
            if (!downSent || !upSent)
            {
                return Task.FromResult(new BackgroundClickResult(false, "PostMessage failed for one or more click messages"));
            }
        }

        return Task.FromResult(new BackgroundClickResult(true));
    }

    private static nint ResolveTargetWindowHandle(PointTarget target)
    {
        if (target.TargetWindowId.HasValue && target.TargetWindowId.Value != nint.Zero)
        {
            return target.TargetWindowId.Value;
        }

        return nint.Zero;
    }

    private static (uint DownMessage, uint UpMessage, nuint DownWParam) GetMessages(MouseButton button)
    {
        return button == MouseButton.Right
            ? (NativeConstants.WmRButtonDown, NativeConstants.WmRButtonUp, NativeConstants.MkRButton)
            : (NativeConstants.WmLButtonDown, NativeConstants.WmLButtonUp, NativeConstants.MkLButton);
    }

    private static nint BuildLParam(int x, int y)
    {
        var packed = (y << 16) | (x & 0xFFFF);
        return new nint(packed);
    }
}