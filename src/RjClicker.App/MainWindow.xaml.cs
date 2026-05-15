using Microsoft.Extensions.DependencyInjection;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Delivery;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.Points;
using RjClicker.App.Infrastructure.Windows;
using RjClicker.App.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace RjClicker.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ServiceProvider _serviceProvider;
    private readonly MainViewModel _mainViewModel;

    public MainWindow()
    {
        InitializeComponent();

        _serviceProvider = BuildServiceProvider();
        _mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        _mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        DataContext = _mainViewModel;

        ApplyWindowStateFromViewModel();
    }

    protected override void OnClosed(EventArgs e)
    {
        _mainViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
        base.OnClosed(e);
        _serviceProvider.Dispose();
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.KeepOnTop))
        {
            Topmost = _mainViewModel.KeepOnTop;
            return;
        }

        if (e.PropertyName == nameof(MainViewModel.IsWindowHidden))
        {
            ApplyWindowVisibilityState();
        }
    }

    private void ApplyWindowStateFromViewModel()
    {
        Topmost = _mainViewModel.KeepOnTop;
        ApplyWindowVisibilityState();
    }

    private void ApplyWindowVisibilityState()
    {
        if (_mainViewModel.IsWindowHidden)
        {
            WindowState = WindowState.Minimized;
            return;
        }

        WindowState = WindowState.Normal;
        Activate();
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