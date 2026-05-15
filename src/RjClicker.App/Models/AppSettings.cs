namespace RjClicker.App.Models;

public sealed class AppSettings
{
    public string MouseButton { get; set; } = "Left";
    public string PressType { get; set; } = "Single";
    public string ClickMode { get; set; } = "Simultaneous";
    public string DeliveryMode { get; set; } = "Foreground";
    public bool UseCounter { get; set; } = false;
    public int MaxClicks { get; set; } = 100;
    public int HoursPart { get; set; } = 0;
    public int MinutesPart { get; set; } = 0;
    public int SecondsPart { get; set; } = 0;
    public int TenthsPart { get; set; } = 0;
    public int HundredthsPart { get; set; } = 1;
    public int ThousandthsPart { get; set; } = 0;
    public bool UseSmartClick { get; set; } = false;
    public bool FreezePointer { get; set; } = false;
    public bool KeepOnTop { get; set; } = false;
    public string StartStopModifiers { get; set; } = "None";
    public string StartStopKey { get; set; } = "F3";
    public string RecordModifiers { get; set; } = "None";
    public string RecordKey { get; set; } = "F4";
    public List<AppPointTarget> Points { get; set; } = [];
}

public sealed class AppPointTarget
{
    public string TargetType { get; set; } = "Absolute";
    public int X { get; set; }
    public int Y { get; set; }
    public string? TargetWindowId { get; set; }
}
