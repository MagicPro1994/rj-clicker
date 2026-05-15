using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Core.Timing;
using RjClicker.App.Core.Validation;
using RjClicker.App.Infrastructure.Points;
using RjClicker.App.Infrastructure.Windows;
using RjClicker.App.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Key = System.Windows.Input.Key;
using ModifierKeys = System.Windows.Input.ModifierKeys;
using ICommand = System.Windows.Input.ICommand;

namespace RjClicker.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ClickSessionController _clickSessionController;
    private readonly IPointCaptureService _pointCaptureService;
    private readonly IWindowBindingService _windowBindingService;
    private bool _isSyncingInterval;

    private MouseButton _selectedMouseButton;
    private PressType _selectedPressType;
    private ClickMode _selectedClickMode;
    private DeliveryMode _selectedDeliveryMode;
    private bool _useCounter;
    private int _maxClicks;
    private int _hoursPart;
    private int _minutesPart;
    private int _secondsPart;
    private int _tenthsPart;
    private int _hundredthsPart;
    private int _thousandthsPart;
    private int _totalIntervalMs;
    private int _clickCount;
    private string _currentStatus;
    private ModifierKeys _startStopModifiers;
    private Key _startStopKey;
    private ModifierKeys _recordModifiers;
    private Key _recordKey;
    private bool _useSmartClick;
    private bool _freezePointer;
    private bool _keepOnTop;
    private bool _isWindowHidden;
    private bool _hideOnStart;
    private bool _showOnStop;

    public MainViewModel(
        ClickSessionController clickSessionController,
        IPointCaptureService pointCaptureService,
        IWindowBindingService windowBindingService)
    {
        _clickSessionController = clickSessionController ?? throw new ArgumentNullException(nameof(clickSessionController));
        _pointCaptureService = pointCaptureService ?? throw new ArgumentNullException(nameof(pointCaptureService));
        _windowBindingService = windowBindingService ?? throw new ArgumentNullException(nameof(windowBindingService));

        MouseButtons = Enum.GetValues<MouseButton>();
        PressTypes = Enum.GetValues<PressType>();
        ClickModes = Enum.GetValues<ClickMode>();
        DeliveryModes = Enum.GetValues<DeliveryMode>();
        TargetTypes = Enum.GetValues<TargetType>();
        HotkeyModifiers =
        [
            ModifierKeys.None,
            ModifierKeys.Control,
            ModifierKeys.Shift,
            ModifierKeys.Alt,
            ModifierKeys.Windows,
        ];
        HotkeyKeys = BuildSupportedHotkeyKeys();

        _selectedMouseButton = MouseButtons[0];
        _selectedPressType = PressTypes[0];
        _selectedClickMode = ClickModes[0];
        _selectedDeliveryMode = DeliveryModes[0];
        _useCounter = false;
        _maxClicks = 10;
        _totalIntervalMs = 100;
        _currentStatus = "Ready";
        _startStopModifiers = ModifierKeys.None;
        _startStopKey = Key.F3;
        _recordModifiers = ModifierKeys.None;
        _recordKey = Key.F4;

        PointTargets = new ObservableCollection<PointTargetViewModel>();
        PointTargets.CollectionChanged += OnPointTargetsCollectionChanged;

        SyncPartsFromTotal(_totalIntervalMs);

        StartClickSession = new RelayCommand(_ => StartClickSessionAsync(), _ => !IsRunning);
        StopClickSession = new RelayCommand(_ => StopClickSessionAsync(), _ => IsRunning);
        ToggleClickSession = new RelayCommand(_ => ToggleClickSessionAsync());
        RecordPoint = new RelayCommand(_ => RecordPointAsync(), _ => !IsRunning);
        AddPoint = new RelayCommand(_ => AddPointAsync(), _ => !IsRunning);
        RemovePoint = new RelayCommand(RemovePointAsync, _ => !IsRunning);
        ClearPoints = new RelayCommand(_ => ClearPointsAsync(), _ => !IsRunning && PointTargets.Count > 0);
        MovePointUp = new RelayCommand(MovePointUpAsync, parameter => !IsRunning && CanMovePoint(parameter, isMoveUp: true));
        MovePointDown = new RelayCommand(MovePointDownAsync, parameter => !IsRunning && CanMovePoint(parameter, isMoveUp: false));

    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<MouseButton> MouseButtons { get; }

    public IReadOnlyList<PressType> PressTypes { get; }

    public IReadOnlyList<ClickMode> ClickModes { get; }

    public IReadOnlyList<DeliveryMode> DeliveryModes { get; }

    public IReadOnlyList<TargetType> TargetTypes { get; }

    public IReadOnlyList<ModifierKeys> HotkeyModifiers { get; }

    public IReadOnlyList<Key> HotkeyKeys { get; }

    public ObservableCollection<PointTargetViewModel> PointTargets { get; }

    public MouseButton SelectedMouseButton
    {
        get => _selectedMouseButton;
        set => SetField(ref _selectedMouseButton, value);
    }

    public PressType SelectedPressType
    {
        get => _selectedPressType;
        set => SetField(ref _selectedPressType, value);
    }

    public ClickMode SelectedClickMode
    {
        get => _selectedClickMode;
        set => SetField(ref _selectedClickMode, value);
    }

    public DeliveryMode SelectedDeliveryMode
    {
        get => _selectedDeliveryMode;
        set => SetField(ref _selectedDeliveryMode, value);
    }

    public bool UseCounter
    {
        get => _useCounter;
        set => SetField(ref _useCounter, value);
    }

    public int MaxClicks
    {
        get => _maxClicks;
        set => SetField(ref _maxClicks, value);
    }

    public int HoursPart
    {
        get => _hoursPart;
        set
        {
            if (SetField(ref _hoursPart, value))
            {
                SyncTotalFromParts();
            }
        }
    }

    public int MinutesPart
    {
        get => _minutesPart;
        set
        {
            if (SetField(ref _minutesPart, value))
            {
                SyncTotalFromParts();
            }
        }
    }

    public int SecondsPart
    {
        get => _secondsPart;
        set
        {
            if (SetField(ref _secondsPart, value))
            {
                SyncTotalFromParts();
            }
        }
    }

    public int TenthsPart
    {
        get => _tenthsPart;
        set
        {
            if (SetField(ref _tenthsPart, value))
            {
                SyncTotalFromParts();
            }
        }
    }

    public int HundredthsPart
    {
        get => _hundredthsPart;
        set
        {
            if (SetField(ref _hundredthsPart, value))
            {
                SyncTotalFromParts();
            }
        }
    }

    public int ThousandthsPart
    {
        get => _thousandthsPart;
        set
        {
            if (SetField(ref _thousandthsPart, value))
            {
                SyncTotalFromParts();
            }
        }
    }

    public int TotalIntervalMs
    {
        get => _totalIntervalMs;
        set
        {
            var normalizedValue = Math.Max(0, value);
            if (!SetField(ref _totalIntervalMs, normalizedValue))
            {
                return;
            }

            if (_isSyncingInterval)
            {
                return;
            }

            SyncPartsFromTotal(normalizedValue);
        }
    }

    public ModifierKeys StartStopModifiers
    {
        get => _startStopModifiers;
        set
        {
            if (SetField(ref _startStopModifiers, value))
            {
                OnPropertyChanged(nameof(StartStopHotkeyDisplay));
            }
        }
    }

    public Key StartStopKey
    {
        get => _startStopKey;
        set
        {
            if (SetField(ref _startStopKey, value))
            {
                OnPropertyChanged(nameof(StartStopHotkeyDisplay));
            }
        }
    }

    public ModifierKeys RecordModifiers
    {
        get => _recordModifiers;
        set
        {
            if (SetField(ref _recordModifiers, value))
            {
                OnPropertyChanged(nameof(RecordHotkeyDisplay));
            }
        }
    }

    public Key RecordKey
    {
        get => _recordKey;
        set
        {
            if (SetField(ref _recordKey, value))
            {
                OnPropertyChanged(nameof(RecordHotkeyDisplay));
            }
        }
    }

    public bool UseSmartClick
    {
        get => _useSmartClick;
        set => SetField(ref _useSmartClick, value);
    }

    public bool FreezePointer
    {
        get => _freezePointer;
        set => SetField(ref _freezePointer, value);
    }

    public bool KeepOnTop
    {
        get => _keepOnTop;
        set => SetField(ref _keepOnTop, value);
    }

    public bool IsWindowHidden
    {
        get => _isWindowHidden;
        private set => SetField(ref _isWindowHidden, value);
    }

    public bool HideOnStart
    {
        get => _hideOnStart;
        set => SetField(ref _hideOnStart, value);
    }

    public bool ShowOnStop
    {
        get => _showOnStop;
        set => SetField(ref _showOnStop, value);
    }

    public string StartStopHotkeyDisplay => BuildHotkeyDisplay(StartStopModifiers, StartStopKey);

    public string RecordHotkeyDisplay => BuildHotkeyDisplay(RecordModifiers, RecordKey);

    public ICommand StartClickSession { get; }

    public ICommand StopClickSession { get; }

    public ICommand ToggleClickSession { get; }

    public ICommand RecordPoint { get; }

    public ICommand AddPoint { get; }

    public ICommand RemovePoint { get; }

    public ICommand ClearPoints { get; }

    public ICommand MovePointUp { get; }

    public ICommand MovePointDown { get; }



    public bool IsRunning => _clickSessionController.IsRunning;

    public int ClickCount
    {
        get => _clickCount;
        private set => SetField(ref _clickCount, value);
    }

    public string CurrentStatus
    {
        get => _currentStatus;
        private set => SetField(ref _currentStatus, value);
    }

    private Task AddPointAsync()
    {
        AddPointInternal(TargetType.Absolute, x: 0, y: 0, windowId: null);
        return Task.CompletedTask;
    }

    private Task RemovePointAsync(object? parameter)
    {
        if (parameter is not PointTargetViewModel target)
        {
            return Task.CompletedTask;
        }

        PointTargets.Remove(target);
        return Task.CompletedTask;
    }

    private Task ClearPointsAsync()
    {
        PointTargets.Clear();
        return Task.CompletedTask;
    }

    private Task MovePointUpAsync(object? parameter)
    {
        MovePoint(parameter, -1);
        return Task.CompletedTask;
    }

    private Task MovePointDownAsync(object? parameter)
    {
        MovePoint(parameter, 1);
        return Task.CompletedTask;
    }

    private void MovePoint(object? parameter, int offset)
    {
        if (parameter is not PointTargetViewModel target)
        {
            return;
        }

        var currentIndex = PointTargets.IndexOf(target);
        if (currentIndex < 0)
        {
            return;
        }

        var destinationIndex = currentIndex + offset;
        if (destinationIndex < 0 || destinationIndex >= PointTargets.Count)
        {
            return;
        }

        PointTargets.Move(currentIndex, destinationIndex);
    }

    private async Task RecordPointAsync()
    {
        var capturedPoint = await _pointCaptureService.CapturePointAsync(CancellationToken.None);
        if (!capturedPoint.HasValue)
        {
            CurrentStatus = "Point capture cancelled";
            return;
        }

        var capturedX = (int)Math.Round(capturedPoint.Value.X);
        var capturedY = (int)Math.Round(capturedPoint.Value.Y);

        if (SelectedDeliveryMode == DeliveryMode.Background)
        {
            var foregroundWindowHandle = await _windowBindingService.GetForegroundWindowHandleAsync();
            if (foregroundWindowHandle != nint.Zero)
            {
                var windowBounds = await _windowBindingService.GetWindowBoundsAsync(foregroundWindowHandle);
                if (!windowBounds.IsEmpty)
                {
                    AddPointInternal(
                        TargetType.WindowRelative,
                        x: capturedX - (int)Math.Round(windowBounds.Left),
                        y: capturedY - (int)Math.Round(windowBounds.Top),
                        windowId: foregroundWindowHandle);

                    CurrentStatus = $"Recorded background point: {capturedX}, {capturedY}";
                    return;
                }
            }
        }

        AddPointInternal(
            TargetType.Absolute,
            x: capturedX,
            y: capturedY,
            windowId: null);

        CurrentStatus = $"Recorded point: {capturedX}, {capturedY}";
    }

    private async Task StartClickSessionAsync()
    {
        var runtimeConfig = BuildRuntimeConfig();
        var validationResult = RuntimeConfigValidator.Validate(runtimeConfig);
        if (!validationResult.IsValid)
        {
            CurrentStatus = validationResult.Errors[0];
            return;
        }

        ClickCount = 0;
        CurrentStatus = BuildRunningStatus();

        if (HideOnStart)
        {
            IsWindowHidden = true;
        }

        var sessionTask = _clickSessionController.StartAsync(runtimeConfig, CancellationToken.None);
        NotifySessionStateChanged();

        try
        {
            await sessionTask;
            CurrentStatus = "Stopped";
        }
        catch (Exception exception)
        {
            CurrentStatus = $"Error: {exception.Message}";
        }
        finally
        {
            if (ShowOnStop)
            {
                IsWindowHidden = false;
            }

            NotifySessionStateChanged();
        }
    }

    private Task StopClickSessionAsync()
    {
        _clickSessionController.Stop();
        CurrentStatus = "Stopped";
        NotifySessionStateChanged();
        return Task.CompletedTask;
    }

    public async Task ToggleClickSessionAsync()
    {
        if (IsRunning)
        {
            await StopClickSessionAsync();
            return;
        }

        _ = StartClickSessionAsync();
    }

    private string BuildRunningStatus()
    {
        if (UseCounter)
        {
            return $"Running: {ClickCount}/{MaxClicks} clicks";
        }

        return "Running";
    }

    private RuntimeConfig BuildRuntimeConfig()
    {
        var targets = new PointTarget[PointTargets.Count];
        for (var index = 0; index < PointTargets.Count; index++)
        {
            var target = PointTargets[index];
            targets[index] = target.TargetType == TargetType.Absolute
                ? PointTarget.Absolute(target.X, target.Y)
                : PointTarget.WindowRelative(target.X, target.Y, target.WindowId ?? nint.Zero);
        }

        return new RuntimeConfig(
            mouseButton: SelectedMouseButton,
            pressType: SelectedPressType,
            totalIntervalMilliseconds: TotalIntervalMs,
            clickMode: SelectedClickMode,
            deliveryMode: SelectedDeliveryMode,
            useCounter: UseCounter,
            maxClicks: UseCounter ? MaxClicks : null,
            targets: targets)
        {
            StartStopModifiers = StartStopModifiers,
            StartStopKey = StartStopKey,
            RecordModifiers = RecordModifiers,
            RecordKey = RecordKey,
            UseSmartClick = UseSmartClick,
            FreezePointer = FreezePointer,
        };
    }

    private void AddPointInternal(TargetType targetType, int x, int y, nint? windowId)
    {
        var pointViewModel = new PointTargetViewModel(targetType, x, y, windowId, OnPointDeleteRequested);
        PointTargets.Add(pointViewModel);
    }

    private void OnPointDeleteRequested(PointTargetViewModel target)
    {
        PointTargets.Remove(target);
    }

    private void OnPointTargetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ((RelayCommand)ClearPoints).RaiseCanExecuteChanged();
        ((RelayCommand)RemovePoint).RaiseCanExecuteChanged();
        ((RelayCommand)MovePointUp).RaiseCanExecuteChanged();
        ((RelayCommand)MovePointDown).RaiseCanExecuteChanged();
    }

    private void SyncTotalFromParts()
    {
        if (_isSyncingInterval)
        {
            return;
        }

        _isSyncingInterval = true;

        try
        {
            var hours = NormalizeNonNegativeIntervalPart(ref _hoursPart, nameof(HoursPart));
            var minutes = NormalizeNonNegativeIntervalPart(ref _minutesPart, nameof(MinutesPart));
            var seconds = NormalizeNonNegativeIntervalPart(ref _secondsPart, nameof(SecondsPart));
            var tenths = NormalizeNonNegativeIntervalPart(ref _tenthsPart, nameof(TenthsPart));
            var hundredths = NormalizeNonNegativeIntervalPart(ref _hundredthsPart, nameof(HundredthsPart));
            var thousandths = NormalizeNonNegativeIntervalPart(ref _thousandthsPart, nameof(ThousandthsPart));

            TotalIntervalMs = IntervalConverter.ToMilliseconds(new IntervalParts(
                hours,
                minutes,
                seconds,
                tenths,
                hundredths,
                thousandths));
        }
        finally
        {
            _isSyncingInterval = false;
        }
    }

    private int NormalizeNonNegativeIntervalPart(ref int partField, string propertyName)
    {
        if (partField >= 0)
        {
            return partField;
        }

        partField = 0;
        OnPropertyChanged(propertyName);
        return 0;
    }

    private void SyncPartsFromTotal(int milliseconds)
    {
        _isSyncingInterval = true;

        try
        {
            var parts = IntervalConverter.FromMilliseconds(Math.Max(0, milliseconds));
            _hoursPart = parts.Hours;
            _minutesPart = parts.Minutes;
            _secondsPart = parts.Seconds;
            _tenthsPart = parts.Tenths;
            _hundredthsPart = parts.Hundredths;
            _thousandthsPart = parts.Thousandths;

            OnPropertyChanged(nameof(HoursPart));
            OnPropertyChanged(nameof(MinutesPart));
            OnPropertyChanged(nameof(SecondsPart));
            OnPropertyChanged(nameof(TenthsPart));
            OnPropertyChanged(nameof(HundredthsPart));
            OnPropertyChanged(nameof(ThousandthsPart));
        }
        finally
        {
            _isSyncingInterval = false;
        }
    }

    private void NotifySessionStateChanged()
    {
        OnPropertyChanged(nameof(IsRunning));
        ((RelayCommand)StartClickSession).RaiseCanExecuteChanged();
        ((RelayCommand)StopClickSession).RaiseCanExecuteChanged();
        ((RelayCommand)AddPoint).RaiseCanExecuteChanged();
        ((RelayCommand)RecordPoint).RaiseCanExecuteChanged();
        ((RelayCommand)RemovePoint).RaiseCanExecuteChanged();
        ((RelayCommand)ClearPoints).RaiseCanExecuteChanged();
        ((RelayCommand)MovePointUp).RaiseCanExecuteChanged();
        ((RelayCommand)MovePointDown).RaiseCanExecuteChanged();
    }

    private bool CanMovePoint(object? parameter, bool isMoveUp)
    {
        if (parameter is not PointTargetViewModel target)
        {
            return false;
        }

        var index = PointTargets.IndexOf(target);
        if (index < 0)
        {
            return false;
        }

        return isMoveUp ? index > 0 : index < PointTargets.Count - 1;
    }

    private static IReadOnlyList<Key> BuildSupportedHotkeyKeys()
    {
        var keys = new List<Key>();

        for (var key = Key.F1; key <= Key.F12; key++)
        {
            keys.Add(key);
        }

        for (var key = Key.A; key <= Key.Z; key++)
        {
            keys.Add(key);
        }

        for (var key = Key.D0; key <= Key.D9; key++)
        {
            keys.Add(key);
        }

        return keys;
    }

    private static string BuildHotkeyDisplay(ModifierKeys modifiers, Key key)
    {
        return modifiers == ModifierKeys.None
            ? key.ToString()
            : $"{modifiers}+{key}";
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}