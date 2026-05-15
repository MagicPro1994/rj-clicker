using Microsoft.Extensions.DependencyInjection;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Delivery;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.Points;
using RjClicker.App.Infrastructure.Windows;
using RjClicker.App.Services;
using RjClicker.App.ViewModels;
using System.IO;

namespace RjClicker.App;

public static class ServiceRegistration
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IForegroundClickService, Win32ForegroundClickService>();
        services.AddSingleton<IBackgroundClickService, Win32BackgroundClickService>();
        services.AddSingleton<IClickDispatcher, ClickDispatcher>();
        services.AddSingleton<IClickScheduler, ClickScheduler>();
        services.AddSingleton<IGlobalHotkeyService, Win32GlobalHotkeyService>();
        services.AddSingleton<IPointCaptureService, Win32PointCaptureService>();
        services.AddSingleton<IWindowBindingService, Win32WindowBindingService>();
        services.AddSingleton<ClickSessionController>();
        services.AddSingleton<IAppLogger, FileAppLogger>();
        services.AddSingleton<AppExceptionLogger>();
        services.AddSingleton<ISettingsStore>(serviceProvider =>
            new JsonSettingsStore(
                Path.Combine(AppContext.BaseDirectory, "settings.json"),
                serviceProvider.GetRequiredService<IAppLogger>()));
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
