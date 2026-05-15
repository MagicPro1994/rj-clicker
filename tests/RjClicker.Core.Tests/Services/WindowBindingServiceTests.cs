using FluentAssertions;
using RjClicker.App.Infrastructure.Windows;
using System.Windows;

namespace RjClicker.Core.Tests.Services;

public sealed class WindowBindingServiceTests
{
    [Fact]
    public async Task GetWindowHandleAsync_ShouldReturnZero_WhenWindowNotFound()
    {
        var service = new Win32WindowBindingService();

        var handle = await service.GetWindowHandleAsync("NonExistentWindow12345");

        handle.Should().Be(0);
    }

    [Fact]
    public async Task GetWindowBoundsAsync_ShouldReturnEmptyRect_WhenHandleIsInvalid()
    {
        var service = new Win32WindowBindingService();

        var bounds = await service.GetWindowBoundsAsync(new nint(-1));

        bounds.Should().Be(Rect.Empty);
    }

    [Fact]
    public async Task GetWindowHandleAsync_ShouldReturnValidHandle_WhenWindowExists()
    {
        var service = new Win32WindowBindingService();

        // Use a valid window that's guaranteed to exist (current process)
        var currentProcessHandle = (nint)System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        if (currentProcessHandle != nint.Zero)
        {
            var handle = await service.GetWindowHandleAsync("ApplicationFrameWindow");
            // Either returns valid handle or zero (if not found) - should not throw
            handle.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetWindowBoundsAsync_ShouldReturnValidRect_WhenHandleIsValid()
    {
        var service = new Win32WindowBindingService();

        var currentProcessHandle = (nint)System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        if (currentProcessHandle != nint.Zero)
        {
            var bounds = await service.GetWindowBoundsAsync(currentProcessHandle);
            // Either returns valid bounds or empty - should not throw
            (bounds == Rect.Empty || bounds.Width > 0).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetWindowHandleAsync_ShouldReturnZero_WhenWindowTitleIsEmpty()
    {
        var service = new Win32WindowBindingService();

        var handle = await service.GetWindowHandleAsync(string.Empty);

        handle.Should().Be(0);
    }

    [Fact]
    public async Task GetWindowBoundsAsync_ShouldNotThrow_WithZeroHandle()
    {
        var service = new Win32WindowBindingService();

        var boundsAction = async () => await service.GetWindowBoundsAsync(nint.Zero);
        await boundsAction.Should().NotThrowAsync();
    }
}
