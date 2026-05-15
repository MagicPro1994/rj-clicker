using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Delivery;

namespace RjClicker.Core.Tests.Sessions;

public sealed class ClickDispatcherTests
{
    private sealed class FakeForegroundClickService : IForegroundClickService
    {
        public int ExecuteClickCalls { get; private set; }

        public Task ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType)
        {
            ExecuteClickCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBackgroundClickService : IBackgroundClickService
    {
        public int ExecuteClickCalls { get; private set; }

        public bool ReturnSuccess { get; set; } = true;

        public string? WarningMessage { get; set; }

        public Task<BackgroundClickResult> ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType)
        {
            ExecuteClickCalls++;
            return Task.FromResult(new BackgroundClickResult(ReturnSuccess, WarningMessage));
        }
    }

    private static RuntimeConfig CreateConfig(
        DeliveryMode deliveryMode,
        MouseButton button = MouseButton.Left,
        PressType pressType = PressType.Single,
        int intervalMs = 100)
    {
        var targets = new[] { PointTarget.Absolute(100, 100) };
        return new RuntimeConfig(
            button,
            pressType,
            intervalMs,
            ClickMode.Simultaneous,
            deliveryMode,
            useCounter: false,
            maxClicks: null,
            targets: targets);
    }

    [Fact]
    public async Task DispatchAsync_WithForegroundMode_CallsForegroundService()
    {
        // Arrange
        var foregroundService = new FakeForegroundClickService();
        var backgroundService = new FakeBackgroundClickService();
        var dispatcher = new ClickDispatcher(foregroundService, backgroundService);
        var config = CreateConfig(DeliveryMode.Foreground);

        // Act
        await dispatcher.DispatchAsync(config, CancellationToken.None);

        // Assert
        foregroundService.ExecuteClickCalls.Should().Be(1);
        backgroundService.ExecuteClickCalls.Should().Be(0);
    }

    [Fact]
    public async Task DispatchAsync_WithBackgroundMode_CallsBackgroundService()
    {
        // Arrange
        var foregroundService = new FakeForegroundClickService();
        var backgroundService = new FakeBackgroundClickService();
        var dispatcher = new ClickDispatcher(foregroundService, backgroundService);
        var config = CreateConfig(DeliveryMode.Background);

        // Act
        await dispatcher.DispatchAsync(config, CancellationToken.None);

        // Assert
        foregroundService.ExecuteClickCalls.Should().Be(0);
        backgroundService.ExecuteClickCalls.Should().Be(1);
    }

    [Fact]
    public async Task DispatchAsync_WithBackgroundModeAndSucceededResult_ContinuedClick()
    {
        // Arrange
        var foregroundService = new FakeForegroundClickService();
        var backgroundService = new FakeBackgroundClickService { ReturnSuccess = true };
        var dispatcher = new ClickDispatcher(foregroundService, backgroundService);
        var config = CreateConfig(DeliveryMode.Background);

        // Act
        await dispatcher.DispatchAsync(config, CancellationToken.None);

        // Assert - should not throw
        backgroundService.ExecuteClickCalls.Should().Be(1);
    }

    [Fact]
    public async Task DispatchAsync_WithBackgroundModeAndFailedResult_ContinuesWithoutThrow()
    {
        // Arrange
        var foregroundService = new FakeForegroundClickService();
        var backgroundService = new FakeBackgroundClickService
        {
            ReturnSuccess = false,
            WarningMessage = "Window not found"
        };
        var dispatcher = new ClickDispatcher(foregroundService, backgroundService);
        var config = CreateConfig(DeliveryMode.Background);

        // Act & Assert - should not throw even when succeeded=false
        await dispatcher.DispatchAsync(config, CancellationToken.None);
        backgroundService.ExecuteClickCalls.Should().Be(1);
    }

    [Fact]
    public async Task DispatchAsync_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        var foregroundService = new FakeForegroundClickService();
        var backgroundService = new FakeBackgroundClickService();
        var dispatcher = new ClickDispatcher(foregroundService, backgroundService);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => dispatcher.DispatchAsync(null!, CancellationToken.None));
    }
}
