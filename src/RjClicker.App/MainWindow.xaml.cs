using RjClicker.App.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace RjClicker.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _mainViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        DataContext = _mainViewModel;

        ApplyWindowStateFromViewModel();
    }

    protected override void OnClosed(EventArgs e)
    {
        _mainViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
        base.OnClosed(e);
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

}