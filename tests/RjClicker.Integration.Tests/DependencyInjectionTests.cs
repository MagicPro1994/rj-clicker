using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RjClicker.App;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Delivery;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.Points;
using RjClicker.App.Infrastructure.Windows;
using RjClicker.App.Models;
using RjClicker.App.Services;
using RjClicker.App.ViewModels;
using System.IO;

namespace RjClicker.Integration.Tests;

public sealed class DependencyInjectionTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public DependencyInjectionTests()
    {
        _serviceProvider = ServiceRegistration.BuildServiceProvider();
    }

    public void Dispose()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [Fact]
    public void ServiceRegistration_ShouldRegisterAllRequiredServices()
    {
        _serviceProvider.GetRequiredService<IForegroundClickService>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<IBackgroundClickService>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<IClickDispatcher>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<IClickScheduler>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<IGlobalHotkeyService>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<IPointCaptureService>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<IWindowBindingService>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<ClickSessionController>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<ISettingsStore>().Should().NotBeNull();
        _serviceProvider.GetRequiredService<MainViewModel>().Should().NotBeNull();
    }

    [Fact]
    public void ServiceRegistration_ShouldReuseMainViewModelInstance()
    {
        var first = _serviceProvider.GetRequiredService<MainViewModel>();
        var second = _serviceProvider.GetRequiredService<MainViewModel>();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void ServiceRegistration_ShouldCreateMainViewModelWithAllDependencies()
    {
        var action = () => _serviceProvider.GetRequiredService<MainViewModel>();

        action.Should().NotThrow();
    }

    [Fact]
    public async Task SettingsStore_ShouldRoundTripSettings()
    {
        var path = Path.Combine(Path.GetTempPath(), $"rjclicker-test-{Guid.NewGuid()}.json");
        var store = new JsonSettingsStore(path);

        try
        {
            var original = new AppSettings
            {
                MouseButton = "Right",
                PressType = "Double",
                ClickMode = "Sequence",
                DeliveryMode = "Background",
                UseCounter = true,
                MaxClicks = 42,
                HoursPart = 1,
                MinutesPart = 2,
                SecondsPart = 3,
                TenthsPart = 4,
                HundredthsPart = 5,
                ThousandthsPart = 6,
                UseSmartClick = true,
                FreezePointer = true,
                KeepOnTop = true,
                StartStopModifiers = "Control",
                StartStopKey = "F5",
                RecordModifiers = "Control+Alt",
                RecordKey = "R",
                Points =
                [
                    new AppPointTarget { TargetType = "Absolute", X = 100, Y = 200 },
                    new AppPointTarget { TargetType = "WindowRelative", X = 10, Y = 20, TargetWindowId = "12345" },
                ],
            };

            await store.SaveAsync(original);
            var loaded = await store.LoadAsync();

            loaded.Should().NotBeNull();
            loaded!.MouseButton.Should().Be("Right");
            loaded.PressType.Should().Be("Double");
            loaded.ClickMode.Should().Be("Sequence");
            loaded.DeliveryMode.Should().Be("Background");
            loaded.UseCounter.Should().BeTrue();
            loaded.MaxClicks.Should().Be(42);
            loaded.HoursPart.Should().Be(1);
            loaded.MinutesPart.Should().Be(2);
            loaded.SecondsPart.Should().Be(3);
            loaded.TenthsPart.Should().Be(4);
            loaded.HundredthsPart.Should().Be(5);
            loaded.ThousandthsPart.Should().Be(6);
            loaded.UseSmartClick.Should().BeTrue();
            loaded.FreezePointer.Should().BeTrue();
            loaded.KeepOnTop.Should().BeTrue();
            loaded.StartStopModifiers.Should().Be("Control");
            loaded.StartStopKey.Should().Be("F5");
            loaded.RecordModifiers.Should().Be("Control+Alt");
            loaded.RecordKey.Should().Be("R");
            loaded.Points.Should().HaveCount(2);
            loaded.Points[0].X.Should().Be(100);
            loaded.Points[1].TargetWindowId.Should().Be("12345");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task SettingsStore_ShouldReturnNullWhenSettingsFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), $"rjclicker-nonexistent-{Guid.NewGuid()}.json");
        var store = new JsonSettingsStore(path);

        var result = await store.LoadAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task SettingsStore_ShouldCreateDirectoryIfMissing()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"rjclicker-dir-{Guid.NewGuid()}");
        var path = Path.Combine(directory, "nested", "settings.json");
        var store = new JsonSettingsStore(path);

        try
        {
            await store.SaveAsync(new AppSettings());

            File.Exists(path).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SettingsStore_DefaultConstructor_ShouldUseBaseDirectorySettingsFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "settings.json");
        File.Delete(path);
        var store = new JsonSettingsStore();

        try
        {
            await store.SaveAsync(new AppSettings { MouseButton = "Right" });

            File.Exists(path).Should().BeTrue();
            (await File.ReadAllTextAsync(path)).Should().Contain("Right");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task SettingsStore_ShouldLogDeserializeFailures()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"rjclicker-log-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var settingsPath = Path.Combine(tempDir, "settings.json");
            var logPath = Path.Combine(tempDir, "rjclicker.log");

            await File.WriteAllTextAsync(settingsPath, "not valid json");

            var logger = new FileAppLogger(logPath);
            var store = new JsonSettingsStore(settingsPath, logger);

            var result = await store.LoadAsync();

            result.Should().BeNull();
            (await File.ReadAllTextAsync(logPath)).Should().Contain("Failed to deserialize settings");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void AppSettings_ShouldHaveDefaultValues()
    {
        var settings = new AppSettings();

        settings.MouseButton.Should().Be("Left");
        settings.PressType.Should().Be("Single");
        settings.ClickMode.Should().Be("Simultaneous");
        settings.DeliveryMode.Should().Be("Foreground");
        settings.UseCounter.Should().BeFalse();
        settings.MaxClicks.Should().Be(100);
        settings.HoursPart.Should().Be(0);
        settings.MinutesPart.Should().Be(0);
        settings.SecondsPart.Should().Be(0);
        settings.TenthsPart.Should().Be(0);
        settings.HundredthsPart.Should().Be(1);
        settings.ThousandthsPart.Should().Be(0);
        settings.UseSmartClick.Should().BeFalse();
        settings.FreezePointer.Should().BeFalse();
        settings.KeepOnTop.Should().BeFalse();
        settings.StartStopModifiers.Should().Be("None");
        settings.StartStopKey.Should().Be("F3");
        settings.RecordModifiers.Should().Be("None");
        settings.RecordKey.Should().Be("F4");
        settings.Points.Should().BeEmpty();
    }
}
