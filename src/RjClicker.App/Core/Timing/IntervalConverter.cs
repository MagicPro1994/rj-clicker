using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Timing;

public static class IntervalConverter
{
    public static int ToMilliseconds(IntervalParts parts)
    {
        ThrowIfNegative(parts.Hours, nameof(parts.Hours));
        ThrowIfNegative(parts.Minutes, nameof(parts.Minutes));
        ThrowIfNegative(parts.Seconds, nameof(parts.Seconds));
        ThrowIfNegative(parts.Tenths, nameof(parts.Tenths));
        ThrowIfNegative(parts.Hundredths, nameof(parts.Hundredths));
        ThrowIfNegative(parts.Thousandths, nameof(parts.Thousandths));

        var totalMilliseconds = checked(
            (parts.Hours * 3_600_000)
            + (parts.Minutes * 60_000)
            + (parts.Seconds * 1_000)
            + (parts.Tenths * 100)
            + (parts.Hundredths * 10)
            + parts.Thousandths);

        return totalMilliseconds;
    }

    public static IntervalParts FromMilliseconds(int milliseconds)
    {
        ThrowIfNegative(milliseconds, nameof(milliseconds));

        var remainingMilliseconds = milliseconds;

        var hours = remainingMilliseconds / 3_600_000;
        remainingMilliseconds %= 3_600_000;

        var minutes = remainingMilliseconds / 60_000;
        remainingMilliseconds %= 60_000;

        var seconds = remainingMilliseconds / 1_000;
        remainingMilliseconds %= 1_000;

        var tenths = remainingMilliseconds / 100;
        remainingMilliseconds %= 100;

        var hundredths = remainingMilliseconds / 10;
        var thousandths = remainingMilliseconds % 10;

        return new IntervalParts(hours, minutes, seconds, tenths, hundredths, thousandths);
    }

    private static void ThrowIfNegative(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Interval parts cannot be negative.");
        }
    }
}
