namespace RjClicker.App.Core.Models;

public sealed record RuntimeConfig(
    MouseButton MouseButton,
    PressType PressType,
    int TotalIntervalMilliseconds,
    ClickMode ClickMode,
    DeliveryMode DeliveryMode,
    bool UseCounter,
    int? MaxClicks,
    IReadOnlyList<PointTarget> Targets);