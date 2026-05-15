using RjClicker.App.Core.Models;
using RjClicker.App.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RjClicker.App.ViewModels;

public sealed class PointTargetViewModel : INotifyPropertyChanged
{
    private readonly Action<PointTargetViewModel> _onDelete;
    private TargetType _targetType;
    private int _x;
    private int _y;
    private nint? _windowId;

    public PointTargetViewModel(
        TargetType targetType,
        int x,
        int y,
        nint? windowId,
        Action<PointTargetViewModel> onDelete)
    {
        _targetType = targetType;
        _x = x;
        _y = y;
        _windowId = windowId;
        _onDelete = onDelete ?? throw new ArgumentNullException(nameof(onDelete));

        Delete = new RelayCommand(_ =>
        {
            _onDelete(this);
            return Task.CompletedTask;
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public TargetType TargetType
    {
        get => _targetType;
        set
        {
            if (_targetType == value)
            {
                return;
            }

            _targetType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    public int X
    {
        get => _x;
        set
        {
            if (_x == value)
            {
                return;
            }

            _x = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    public int Y
    {
        get => _y;
        set
        {
            if (_y == value)
            {
                return;
            }

            _y = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    public nint? WindowId
    {
        get => _windowId;
        set
        {
            if (_windowId == value)
            {
                return;
            }

            _windowId = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    public string DisplayText
    {
        get
        {
            if (TargetType == TargetType.WindowRelative)
            {
                var windowSuffix = WindowId.HasValue ? $" (Window {WindowId.Value})" : string.Empty;
                return $"WindowRelative: {X},{Y}{windowSuffix}";
            }

            return $"Absolute: {X},{Y}";
        }
    }

    public ICommand Delete { get; }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}