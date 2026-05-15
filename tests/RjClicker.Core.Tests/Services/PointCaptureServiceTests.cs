using FluentAssertions;
using RjClicker.App.Infrastructure.Points;
using System.Windows;

namespace RjClicker.Core.Tests.Services;

public sealed class PointCaptureServiceTests
{
    [Fact]
    public async Task CapturePointAsync_ShouldReturnNull_WhenCancelled()
    {
        var service = new Win32PointCaptureService();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(10));

        var result = await service.CapturePointAsync(cts.Token);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CapturePointAsync_ShouldNotThrow_WithValidCancellationToken()
    {
        var service = new Win32PointCaptureService();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var captureAction = async () => await service.CapturePointAsync(cts.Token);
        await captureAction.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CapturePointAsync_ShouldReturnNullOrValidPoint_WhenCalled()
    {
        var service = new Win32PointCaptureService();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var result = await service.CapturePointAsync(cts.Token);

        (result == null || (result.Value.X >= 0 && result.Value.Y >= 0)).Should().BeTrue();
    }

    [Fact]
    public async Task CapturePointAsync_ShouldReturnNull_WhenCancellationTokenAlreadyCancelled()
    {
        var service = new Win32PointCaptureService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.CapturePointAsync(cts.Token);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CapturePointAsync_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        var service = new Win32PointCaptureService();

        for (int i = 0; i < 3; i++)
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(10));

            var captureAction = async () => await service.CapturePointAsync(cts.Token);
            await captureAction.Should().NotThrowAsync();
        }
    }
}
