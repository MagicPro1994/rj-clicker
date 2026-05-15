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

        _selectedMouseButton = MouseButtons[0];
        _selectedPressType = PressTypes[0];
        _selectedClickMode = ClickModes[0];
        _selectedDeliveryMode = DeliveryModes[0];
        _useCounter = false;
        _maxClicks = 10;
        _totalIntervalMs = 100;
        _currentStatus = "Ready";

        PointTargets = new ObservableCollection<PointTargetViewModel>();
        PointTargets.CollectionChanged += OnPointTargetsCollectionChanged;

        SyncPartsFromTotal(_totalIntervalMs);

        StartClickSession = new RelayCommand(_ => StartClickSessionAsync(), _ => !IsRunning);
        StopClickSession = new RelayCommand(_ => StopClickSessionAsync(), _ => IsRunning);
        RecordPoint = new RelayCommand(_ => RecordPointAsync(), _ => !IsRunning);
        AddPoint = new RelayCommand(_ => AddPointAsync(), _ => !IsRunning);
        RemovePoint = new RelayCommand(RemovePointAsync, _ => !IsRunning);
        ClearPoints = new RelayCommand(_ => ClearPointsAsync(), _ => !IsRunning && PointTargets.Count > 0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<MouseButton> MouseButtons { get; }

    public IReadOnlyList<PressType> PressTypes { get; }

    public IReadOnlyList<ClickMode> ClickModes { get; }

    public IReadOnlyList<DeliveryMode> DeliveryModes { get; }

    public IReadOnlyList<TargetType> TargetTypes { get; }

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

    public ICommand StartClickSession { get; }

    public ICommand StopClickSession { get; }

    public ICommand RecordPoint { get; }

    public ICommand AddPoint { get; }

    public ICommand RemovePoint { get; }

    public ICommand ClearPoints { get; }

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

    private async Task RecordPointAsync()
    {
        var capturedPoint = await _pointCaptureService.CapturePointAsync(CancellationToken.None);
        if (!capturedPoint.HasValue)
        {
            CurrentStatus = "Point capture cancelled";
            return;
        }

        AddPointInternal(
            TargetType.Absolute,
            x: (int)Math.Round(capturedPoint.Value.X),
            y: (int)Math.Round(capturedPoint.Value.Y),
            windowId: null);

        CurrentStatus = $"Recorded point: {(int)Math.Round(capturedPoint.Value.X)}, {(int)Math.Round(capturedPoint.Value.Y)}";
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
        NotifySessionStateChanged();

        try
        {
            await _clickSessionController.StartAsync(runtimeConfig, CancellationToken.None);
            CurrentStatus = "Stopped";
        }
        catch (Exception exception)
        {
            CurrentStatus = $"Error: {exception.Message}";
        }
        finally
        {
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
            targets: targets);
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