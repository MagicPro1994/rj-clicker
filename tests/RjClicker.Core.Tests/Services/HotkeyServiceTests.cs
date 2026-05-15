using FluentAssertions;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.PInvoke;
using System.Windows.Input;

namespace RjClicker.Core.Tests.Services;

public sealed class HotkeyServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldRegisterHotkey_WhenCalled()
    {
        var service = new Win32GlobalHotkeyService(new FakeWin32Api());
        var windowHandle = new nint(123);
        var hotkeyId = 1;
        var modifiers = ModifierKeys.Control;
        var key = Key.A;
        var pressed = false;

        await service.RegisterAsync(
            windowHandle,
            hotkeyId,
            modifiers,
            key,
            async () =>
            {
                pressed = true;
                await Task.CompletedTask;
            });

        pressed.Should().BeFalse(); // Not immediately invoked
    }

    [Fact]
    public async Task UnregisterAsync_ShouldUnregisterHotkey_WhenRegistered()
    {
        var service = new Win32GlobalHotkeyService(new FakeWin32Api());
        var windowHandle = new nint(123);
        var hotkeyId = 2;

        await service.RegisterAsync(
            windowHandle,
            hotkeyId,
            ModifierKeys.Control,
            Key.B,
            async () => await Task.CompletedTask);

        var unregisterAction = async () => await service.UnregisterAsync(hotkeyId);
        await unregisterAction.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleHotkeyPressed_ShouldInvokeRegisteredCallback()
    {
        var fakeWin32Api = new FakeWin32Api();
        var service = new Win32GlobalHotkeyService(fakeWin32Api);
        var windowHandle = new nint(123);
        var hotkeyId = 3;
        var pressed = false;

        await service.RegisterAsync(
            windowHandle,
            hotkeyId,
            ModifierKeys.Control,
            Key.C,
            async () =>
            {
                pressed = true;
                await Task.CompletedTask;
            });

        service.HandleHotkeyPressed(hotkeyId);

        pressed.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccessfully_WhenMultipleHotkeysRegistered()
    {
        var service = new Win32GlobalHotkeyService(new FakeWin32Api());
        var windowHandle = new nint(123);

        var registerAction = async () =>
        {
            await service.RegisterAsync(windowHandle, 1, ModifierKeys.Control, Key.A, async () => await Task.CompletedTask);
            await service.RegisterAsync(windowHandle, 2, ModifierKeys.Alt, Key.B, async () => await Task.CompletedTask);
            await service.RegisterAsync(windowHandle, 3, ModifierKeys.Shift, Key.C, async () => await Task.CompletedTask);
        };

        await registerAction.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UnregisterAsync_ShouldReturnSuccessfully_WhenHotkeyNotRegistered()
    {
        var service = new Win32GlobalHotkeyService();

        var unregisterAction = async () => await service.UnregisterAsync(9999);
        await unregisterAction.Should().NotThrowAsync();
    }

    private sealed class FakeWin32Api : IWin32Api
    {
        public bool SetCursorPos(int x, int y) => true;

        public bool GetCursorPos(out NativeMethods.POINT point)
        {
            point = new NativeMethods.POINT();
            return true;
        }

        public uint SendInput(uint inputCount, NativeMethods.INPUT[] inputs, int inputSize) => inputCount;

        public bool PostMessage(nint windowHandle, uint message, nuint wParam, nint lParam) => true;

        public bool RegisterHotKey(nint windowHandle, int hotkeyId, uint modifiers, uint virtualKeyCode) => true;

        public bool UnregisterHotKey(nint windowHandle, int hotkeyId) => true;

        public bool GetWindowRect(nint windowHandle, out NativeMethods.RECT rect)
        {
            rect = new NativeMethods.RECT();
            return true;
        }

        public nint GetForegroundWindow() => new nint(123);

        public nint FindWindow(string? className, string? windowTitle) => nint.Zero;

        public nint SetWindowsHookEx(int hookType, NativeMethods.HookProc hookProcedure, nint moduleHandle, uint threadId) => nint.Zero;

        public bool UnhookWindowsHookEx(nint hookHandle) => true;

        public nint CallNextHookEx(nint hookHandle, int code, nuint wParam, nint lParam) => nint.Zero;

        public nint GetModuleHandle(string? moduleName) => nint.Zero;

        public bool TryReadMouseHookStruct(nint lParam, out NativeMethods.MSLLHOOKSTRUCT hookStruct)
        {
            hookStruct = default;
            return false;
        }
    }
}
