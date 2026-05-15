using RjClicker.App.ViewModels;
using RjClicker.App.Infrastructure.Hotkeys;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace RjClicker.App;

public partial class MainWindow : Window
{
    private const int StartStopHotkeyId = 1;
    private const int WmHotkey = 0x0312;

    private readonly MainViewModel _mainViewModel;
    private readonly IGlobalHotkeyService _hotkeyService;
    private HwndSource? _source;

    public MainWindow(MainViewModel viewModel, IGlobalHotkeyService hotkeyService)
    {
        InitializeComponent();

        _mainViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
        _mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        DataContext = _mainViewModel;

        ApplyWindowStateFromViewModel();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _source = (HwndSource?)PresentationSource.FromVisual(this);
        _source?.AddHook(WndProc);

        _ = RefreshStartStopHotkeyRegistrationAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        _ = _hotkeyService.UnregisterAsync(StartStopHotkeyId);
        _source?.RemoveHook(WndProc);
        _mainViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
        base.OnClosed(e);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg != WmHotkey)
        {
            return nint.Zero;
        }

        _hotkeyService.HandleHotkeyPressed((int)wParam);
        handled = true;
        return nint.Zero;
    }

    private Task OnStartStopHotkeyPressedAsync()
    {
        return _mainViewModel.ToggleClickSessionAsync();
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.StartStopModifiers)
            || e.PropertyName == nameof(MainViewModel.StartStopKey))
        {
            _ = RefreshStartStopHotkeyRegistrationAsync();
            return;
        }

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

    private async Task RefreshStartStopHotkeyRegistrationAsync()
    {
        if (_source is null)
        {
            return;
        }

        await _hotkeyService.UnregisterAsync(StartStopHotkeyId);
        await _hotkeyService.RegisterAsync(
            _source.Handle,
            StartStopHotkeyId,
            _mainViewModel.StartStopModifiers,
            _mainViewModel.StartStopKey,
            OnStartStopHotkeyPressedAsync);
    }

    private void OnPointTargetsCellPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var cell = (DataGridCell)sender;
        var row = FindVisualParent<DataGridRow>(cell);
        if (row is { IsSelected: false })
        {
            row.IsSelected = true;
        }
    }

    private static T? FindVisualParent<T>(DependencyObject? element) where T : DependencyObject
    {
        while (element != null)
        {
            if (element is T match) return match;
            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }
}