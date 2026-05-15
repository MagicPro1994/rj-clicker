using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Hotkeys;
using System.Windows.Input;

namespace RjClicker.Integration.Tests;

/// <summary>
/// Integration tests for session lifecycle with hotkey service.
/// Verifies that hotkey service is registered and unregistered during session lifecycle.
/// </summary>
public sealed class SessionLifecycleTests
{
    [Fact]
    public async Task StartStop_ShouldRegisterAndUnregisterHotkeys_DuringSessionLifecycle()
    {
        // Arrange
        var hotkeyService = new MockHotkeyService();
        var dispatcher = new MockDispatcher();
        var scheduler = new MockScheduler();
        var controller = new ClickSessionController(dispatcher, scheduler, hotkeyService);

        var config = new RuntimeConfig(
            RjClicker.App.Core.Models.MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 10,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: true,
            maxClicks: 1,
            targets: new[] { PointTarget.Absolute(100, 100) });

        // Act
        await controller.StartAsync(config, CancellationToken.None);

        // Assert
        // Verify hotkey service was called in the correct order:
        // 1. RegisterAsync should be called before dispatcher
        hotkeyService.RegisterCalls.Should().HaveCount(1);
        hotkeyService.RegisterCalls[0].HotkeyId.Should().Be(1);
        hotkeyService.RegisterCalls[0].Modifiers.Should().Be(ModifierKeys.Control);
        hotkeyService.RegisterCalls[0].Key.Should().Be(Key.F12);

        // 2. UnregisterAsync should be called after session completes
        hotkeyService.UnregisterCalls.Should().HaveCount(1);
        hotkeyService.UnregisterCalls[0].Should().Be(1);

        // Verify the order: register was called before unregister
        hotkeyService.CallOrder.Should().Equal("RegisterAsync", "UnregisterAsync");
    }

    [Fact]
    public async Task SessionStop_ShouldUnregisterHotkeys_EvenIfDispatcherThrows()
    {
        // Arrange
        var hotkeyService = new MockHotkeyService();
        var dispatcher = new ThrowingMockDispatcher();
        var scheduler = new MockSchedulerWithException();
        var controller = new ClickSessionController(dispatcher, scheduler, hotkeyService);

        var config = new RuntimeConfig(
            RjClicker.App.Core.Models.MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 10,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: false,
            maxClicks: null,
            targets: new[] { PointTarget.Absolute(100, 100) });

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => controller.StartAsync(config, CancellationToken.None));
        
        // Even though dispatcher threw, hotkey should still be unregistered
        hotkeyService.RegisterCalls.Should().HaveCount(1);
        hotkeyService.UnregisterCalls.Should().HaveCount(1);
    }

    // Mock implementations
    private sealed class MockDispatcher : IClickDispatcher
    {
        public Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingMockDispatcher : IClickDispatcher
    {
        public Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Test exception");
        }
    }

    private sealed class MockScheduler : IClickScheduler
    {
        public Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
        {
            onTick();
            return Task.CompletedTask;
        }

        public async Task RunAsyncWithAsyncHandler(int intervalMilliseconds, Func<Task> onTick, CancellationToken cancellationToken)
        {
            await onTick();
        }
    }

    private sealed class MockSchedulerWithException : IClickScheduler
    {
        public Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Scheduler exception");
        }

        public async Task RunAsyncWithAsyncHandler(int intervalMilliseconds, Func<Task> onTick, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Scheduler exception");
        }
    }

    private sealed class MockHotkeyService : IGlobalHotkeyService
    {
        public List<RegisterCall> RegisterCalls { get; } = [];
        public List<int> UnregisterCalls { get; } = [];
        public List<string> CallOrder { get; } = [];

        public Task RegisterAsync(int hotkeyId, ModifierKeys modifiers, Key key, Func<Task> onPressed)
        {
            ArgumentNullException.ThrowIfNull(onPressed);
            RegisterCalls.Add(new RegisterCall { HotkeyId = hotkeyId, Modifiers = modifiers, Key = key });
            CallOrder.Add("RegisterAsync");
            return Task.CompletedTask;
        }

        public Task UnregisterAsync(int hotkeyId)
        {
            UnregisterCalls.Add(hotkeyId);
            CallOrder.Add("UnregisterAsync");
            return Task.CompletedTask;
        }

        public sealed class RegisterCall
        {
            public int HotkeyId { get; set; }
            public ModifierKeys Modifiers { get; set; }
            public Key Key { get; set; }
        }
    }
}
