namespace RjClicker.App.Core.Models;

public sealed record RuntimeConfig
{
    private static readonly IReadOnlyList<PointTarget> EmptyTargets = Array.Empty<PointTarget>();

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
        Targets = CreateTargetsSnapshot(targets);
    }

    public MouseButton MouseButton { get; init; }

    public PressType PressType { get; init; }

    public int TotalIntervalMilliseconds { get; init; }

    public ClickMode ClickMode { get; init; }

    public DeliveryMode DeliveryMode { get; init; }

    public bool UseCounter { get; init; }

    public int? MaxClicks { get; init; }

    public IReadOnlyList<PointTarget> Targets { get; init; }

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