using FluentAssertions;
using RjClicker.App.Infrastructure.Hotkeys;
using System.Windows.Input;

namespace RjClicker.Core.Tests.Services;

public sealed class HotkeyServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldRegisterHotkey_WhenCalled()
    {
        var service = new Win32GlobalHotkeyService();
        var hotkeyId = 1;
        var modifiers = ModifierKeys.Control;
        var key = Key.A;
        var pressed = false;

        await service.RegisterAsync(
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
        var service = new Win32GlobalHotkeyService();
        var hotkeyId = 2;

        await service.RegisterAsync(
            hotkeyId,
            ModifierKeys.Control,
            Key.B,
            async () => await Task.CompletedTask);

        var unregisterAction = async () => await service.UnregisterAsync(hotkeyId);
        await unregisterAction.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccessfully_WhenMultipleHotkeysRegistered()
    {
        var service = new Win32GlobalHotkeyService();

        var registerAction = async () =>
        {
            await service.RegisterAsync(1, ModifierKeys.Control, Key.A, async () => await Task.CompletedTask);
            await service.RegisterAsync(2, ModifierKeys.Alt, Key.B, async () => await Task.CompletedTask);
            await service.RegisterAsync(3, ModifierKeys.Shift, Key.C, async () => await Task.CompletedTask);
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
}
