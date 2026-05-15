namespace RjClicker.App.Core.Models;

using System.Windows.Input;

public sealed record RuntimeConfig
{
    private static readonly IReadOnlyList<PointTarget> EmptyTargets = Array.Empty<PointTarget>();
    private IReadOnlyList<PointTarget> _targets = EmptyTargets;

    public RuntimeConfig(
        MouseButton mouseButton,
        PressType pressType,
        int totalIntervalMilliseconds,
        ClickMode clickMode,
        DeliveryMode deliveryMode,
        bool useCounter,
        int? maxClicks,
        IReadOnlyList<PointTarget>? targets)
    {
        MouseButton = mouseButton;
        PressType = pressType;
        TotalIntervalMilliseconds = totalIntervalMilliseconds;
        ClickMode = clickMode;
        DeliveryMode = deliveryMode;
        UseCounter = useCounter;
        MaxClicks = maxClicks;
        Targets = targets ?? EmptyTargets;
    }

    public MouseButton MouseButton { get; init; }

    public PressType PressType { get; init; }

    public int TotalIntervalMilliseconds { get; init; }

    public ClickMode ClickMode { get; init; }

    public DeliveryMode DeliveryMode { get; init; }

    public bool UseCounter { get; init; }

    public int? MaxClicks { get; init; }

    public ModifierKeys StartStopModifiers { get; init; } = ModifierKeys.None;

    public Key StartStopKey { get; init; } = Key.F3;

    public ModifierKeys RecordModifiers { get; init; } = ModifierKeys.None;

    public Key RecordKey { get; init; } = Key.F4;

    public bool UseSmartClick { get; init; }

    public bool FreezePointer { get; init; }

    public IReadOnlyList<PointTarget> Targets
    {
        get => _targets;
        init => _targets = CreateTargetsSnapshot(value);
    }

    private static IReadOnlyList<PointTarget> CreateTargetsSnapshot(IReadOnlyList<PointTarget>? targets)
    {
        if (targets is null || targets.Count == 0)
        {
            return EmptyTargets;
        }

        var snapshot = new PointTarget[targets.Count];
        for (var index = 0; index < targets.Count; index++)
        {
            snapshot[index] = targets[index];
        }

        return Array.AsReadOnly(snapshot);
    }
}