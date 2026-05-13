namespace RjClicker.App.Core.Models;

public sealed record PointTarget(TargetType TargetType, int X, int Y, nint? TargetWindowId)
{
    public static PointTarget Absolute(int x, int y)
    {
        return new PointTarget(TargetType.Absolute, x, y, null);
    }

    public static PointTarget WindowRelative(int x, int y, nint targetWindowId)
    {
        return new PointTarget(TargetType.WindowRelative, x, y, targetWindowId);
    }
}