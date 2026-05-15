using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.PInvoke;
using System.Runtime.InteropServices;

namespace RjClicker.App.Infrastructure.Delivery;

public sealed class Win32ForegroundClickService : IForegroundClickService
{
    private const int ClickTransitionDelayMilliseconds = 20;

    private readonly IWin32Api _win32Api;

    public Win32ForegroundClickService()
        : this(new Win32Api())
    {
    }

    public Win32ForegroundClickService(IWin32Api win32Api)
    {
        _win32Api = win32Api ?? throw new ArgumentNullException(nameof(win32Api));
    }

    public async Task ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType)
    {
        ArgumentNullException.ThrowIfNull(target);

        await ExecuteClickAtPointAsync(target.X, target.Y, button, pressType);
    }

    public async Task ExecuteClickAsync(MouseButton button, PressType pressType)
    {
        if (!_win32Api.GetCursorPos(out var cursorPosition))
        {
            throw new InvalidOperationException("Unable to determine current cursor position");
        }

        await ExecuteClickAtPointAsync(cursorPosition.X, cursorPosition.Y, button, pressType);
    }

    private async Task ExecuteClickAtPointAsync(int x, int y, MouseButton button, PressType pressType)
    {
        _win32Api.SetCursorPos(x, y);

        var (downFlag, upFlag) = GetFlags(button);
        var clickCycles = pressType == PressType.Double ? 2 : 1;

        for (var cycle = 0; cycle < clickCycles; cycle++)
        {
            SendMouseInput(downFlag);
            await Task.Delay(ClickTransitionDelayMilliseconds).ConfigureAwait(false);
            SendMouseInput(upFlag);
        }
    }

    private (uint DownFlag, uint UpFlag) GetFlags(MouseButton button)
    {
        return button == MouseButton.Right
            ? (NativeConstants.MouseEventRightDown, NativeConstants.MouseEventRightUp)
            : (NativeConstants.MouseEventLeftDown, NativeConstants.MouseEventLeftUp);
    }

    private void SendMouseInput(uint mouseFlag)
    {
        var input = new NativeMethods.INPUT
        {
            Type = NativeConstants.InputMouse,
            U = new NativeMethods.InputUnion
            {
                Mi = new NativeMethods.MOUSEINPUT
                {
                    DwFlags = mouseFlag,
                },
            },
        };

        _win32Api.SendInput(1, [input], Marshal.SizeOf<NativeMethods.INPUT>());
    }
}