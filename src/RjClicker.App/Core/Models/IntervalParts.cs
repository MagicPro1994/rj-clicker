namespace RjClicker.App.Core.Models;

public readonly record struct IntervalParts(
    int Hours,
    int Minutes,
    int Seconds,
    int Tenths,
    int Hundredths,
    int Thousandths);
