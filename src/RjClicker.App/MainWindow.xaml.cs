using Microsoft.Extensions.DependencyInjection;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Delivery;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.Points;
using RjClicker.App.Infrastructure.Windows;
using RjClicker.App.ViewModels;
using System.Windows;

namespace RjClicker.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ServiceProvider _serviceProvider;

    public MainWindow()
    {
        InitializeComponent();

        _serviceProvider = BuildServiceProvider();
        DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _serviceProvider.Dispose();
    }

    private static ServiceProvider BuildServiceProvider()
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
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }
}