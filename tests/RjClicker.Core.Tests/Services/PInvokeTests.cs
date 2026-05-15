using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.Delivery;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.PInvoke;
using RjClicker.App.Infrastructure.Points;
using RjClicker.App.Infrastructure.Windows;
using System.Windows;
using System.Windows.Input;
using CoreMouseButton = RjClicker.App.Core.Models.MouseButton;

namespace RjClicker.Core.Tests.Services;

public sealed class PInvokeTests
{
    [Fact]
    public async Task ForegroundClick_ShouldSendSingleLeftDownUpSequence()
    {
        var fake = new FakeWin32Api();
        var service = new Win32ForegroundClickService(fake);

        await service.ExecuteClickAsync(PointTarget.Absolute(120, 80), CoreMouseButton.Left, PressType.Single);

        fake.CursorPositions.Should().ContainSingle().Which.Should().Be((120, 80));
        fake.SentMouseFlags.Should().ContainInOrder(
            NativeConstants.MouseEventLeftDown,
            NativeConstants.MouseEventLeftUp);
    }

    [Fact]
    public async Task ForegroundClick_ShouldSendDoubleRightDownUpSequence()
    {
        var fake = new FakeWin32Api();
        var service = new Win32ForegroundClickService(fake);

        await service.ExecuteClickAsync(PointTarget.Absolute(10, 20), CoreMouseButton.Right, PressType.Double);

        fake.SentMouseFlags.Should().ContainInOrder(
            NativeConstants.MouseEventRightDown,
            NativeConstants.MouseEventRightUp,
            NativeConstants.MouseEventRightDown,
            NativeConstants.MouseEventRightUp);
    }

    [Fact]
    public async Task BackgroundClick_ShouldReturnWarning_WhenWindowHandleMissing()
    {
        var fake = new FakeWin32Api();
        var service = new Win32BackgroundClickService(fake);

        var result = await service.ExecuteClickAsync(
            new PointTarget(TargetType.WindowRelative, 15, 30, null),
            CoreMouseButton.Left,
            PressType.Single);

        result.Succeeded.Should().BeFalse();
        result.WarningMessage.Should().NotBeNullOrWhiteSpace();
        fake.PostedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task BackgroundClick_ShouldPostMessages_WhenWindowHandleIsAvailable()
    {
        var fake = new FakeWin32Api();
        var service = new Win32BackgroundClickService(fake);

        var result = await service.ExecuteClickAsync(
            PointTarget.WindowRelative(4, 6, new nint(42)),
            CoreMouseButton.Left,
            PressType.Single);

        result.Succeeded.Should().BeTrue();
        fake.PostedMessages.Select(m => m.msg).Should().ContainInOrder(
            NativeConstants.WmLButtonDown,
            NativeConstants.WmLButtonUp);
    }

    [Fact]
    public async Task BackgroundClick_ShouldReturnFailed_WhenPostMessageFails()
    {
        var fake = new FakeWin32Api { PostMessageResult = false };
        var service = new Win32BackgroundClickService(fake);

        var result = await service.ExecuteClickAsync(
            PointTarget.WindowRelative(4, 6, new nint(42)),
            CoreMouseButton.Right,
            PressType.Single);

        result.Succeeded.Should().BeFalse();
        result.WarningMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GlobalHotkey_ShouldRegisterAndUnregisterWithNativeApi()
    {
        var fake = new FakeWin32Api { ForegroundWindow = new nint(777) };
        var service = new Win32GlobalHotkeyService(fake);

        await service.RegisterAsync(7, ModifierKeys.Control | ModifierKeys.Shift, Key.F9, () => Task.CompletedTask);
        await service.UnregisterAsync(7);

        fake.RegisterCalls.Should().ContainSingle();
        fake.RegisterCalls[0].id.Should().Be(7);
        fake.RegisterCalls[0].window.Should().Be(new nint(777));
        fake.UnregisterCalls.Should().ContainSingle(call => call.id == 7 && call.window == new nint(777));
    }

    [Fact]
    public async Task WindowBinding_ShouldResolveHandleAndBoundsFromNativeApi()
    {
        var fake = new FakeWin32Api
        {
            FindWindowResult = new nint(500),
            RectResult = new NativeMethods.RECT { Left = 10, Top = 15, Right = 40, Bottom = 55 },
            GetWindowRectResult = true,
        };
        var service = new Win32WindowBindingService(fake);

        var handle = await service.GetWindowHandleAsync("Target Window");
        var rect = await service.GetWindowBoundsAsync(handle);

        handle.Should().Be(new nint(500));
        rect.Should().Be(new Rect(10, 15, 30, 40));
    }

    [Fact]
    public async Task PointCapture_ShouldReturnPoint_WhenLowLevelMouseHookTriggers()
    {
        var fake = new FakeWin32Api();
        var service = new Win32PointCaptureService(fake);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var captureTask = service.CapturePointAsync(cts.Token);
        await WaitUntilAsync(() => fake.IsHookRegistered);

        fake.TriggerMouseHook(123, 456);
        var result = await captureTask;

        result.Should().Be(new Point(123, 456));
        fake.UnhookCalls.Should().BeGreaterThan(0);
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(5);
        }

        throw new TimeoutException("Timed out while waiting for expected test state.");
    }

    [Fact]
    public async Task PointCapture_ShouldReturnNull_WhenCancelled()
    {
        var fake = new FakeWin32Api();
        var service = new Win32PointCaptureService(fake);
        using var cts = new CancellationTokenSource();

        var captureTask = service.CapturePointAsync(cts.Token);
        cts.Cancel();

        var result = await captureTask;
        result.Should().BeNull();
        fake.UnhookCalls.Should().BeGreaterThan(0);
    }

    private sealed class FakeWin32Api : IWin32Api
    {
        private NativeMethods.HookProc? _hookProc;

        public List<(int x, int y)> CursorPositions { get; } = [];

        public List<uint> SentMouseFlags { get; } = [];

        public List<(nint handle, uint msg, nuint wParam, nint lParam)> PostedMessages { get; } = [];

        public List<(nint window, int id, uint modifiers, uint key)> RegisterCalls { get; } = [];

        public List<(nint window, int id)> UnregisterCalls { get; } = [];

        public bool PostMessageResult { get; set; } = true;

        public nint ForegroundWindow { get; set; } = nint.Zero;

        public nint FindWindowResult { get; set; } = nint.Zero;

        public bool GetWindowRectResult { get; set; }

        public NativeMethods.RECT RectResult { get; set; }

        public bool RegisterHotKeyResult { get; set; } = true;

        public bool UnregisterHotKeyResult { get; set; } = true;

        public nint HookHandle { get; set; } = new nint(99);

        public int UnhookCalls { get; private set; }

        public nint LastHookHandle { get; private set; }

        public bool IsHookRegistered => _hookProc is not null;

        public bool SetCursorPos(int x, int y)
        {
            CursorPositions.Add((x, y));
            return true;
        }

        public uint SendInput(uint inputCount, NativeMethods.INPUT[] inputs, int inputSize)
        {
            foreach (var input in inputs)
            {
                SentMouseFlags.Add(input.U.Mi.DwFlags);
            }

            return inputCount;
        }

        public bool PostMessage(nint windowHandle, uint message, nuint wParam, nint lParam)
        {
            PostedMessages.Add((windowHandle, message, wParam, lParam));
            return PostMessageResult;
        }

        public bool RegisterHotKey(nint windowHandle, int hotkeyId, uint modifiers, uint virtualKeyCode)
        {
            RegisterCalls.Add((windowHandle, hotkeyId, modifiers, virtualKeyCode));
            return RegisterHotKeyResult;
        }

        public bool UnregisterHotKey(nint windowHandle, int hotkeyId)
        {
            UnregisterCalls.Add((windowHandle, hotkeyId));
            return UnregisterHotKeyResult;
        }

        public bool GetWindowRect(nint windowHandle, out NativeMethods.RECT rect)
        {
            rect = RectResult;
            return GetWindowRectResult;
        }

        public nint GetForegroundWindow()
        {
            return ForegroundWindow;
        }

        public nint FindWindow(string? className, string? windowTitle)
        {
            return FindWindowResult;
        }

        public nint SetWindowsHookEx(int hookType, NativeMethods.HookProc hookProcedure, nint moduleHandle, uint threadId)
        {
            _hookProc = hookProcedure;
            return HookHandle;
        }

        public bool UnhookWindowsHookEx(nint hookHandle)
        {
            LastHookHandle = hookHandle;
            UnhookCalls++;
            return true;
        }

        public nint CallNextHookEx(nint hookHandle, int code, nuint wParam, nint lParam)
        {
            return nint.Zero;
        }

        public nint GetModuleHandle(string? moduleName)
        {
            return nint.Zero;
        }

        public bool TryReadMouseHookStruct(nint lParam, out NativeMethods.MSLLHOOKSTRUCT hookStruct)
        {
            hookStruct = System.Runtime.InteropServices.Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            return true;
        }

        public void TriggerMouseHook(int x, int y)
        {
            _hookProc.Should().NotBeNull();
            var hookStruct = new NativeMethods.MSLLHOOKSTRUCT
            {
                Pt = new NativeMethods.POINT { X = x, Y = y },
            };

            var memory = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MSLLHOOKSTRUCT>());
            try
            {
                System.Runtime.InteropServices.Marshal.StructureToPtr(hookStruct, memory, false);
                _hookProc!(0, NativeConstants.WmLButtonDown, memory);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(memory);
            }
        }
    }
}
