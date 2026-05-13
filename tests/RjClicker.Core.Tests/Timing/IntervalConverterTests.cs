using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Timing;

namespace RjClicker.Core.Tests.Timing;

public sealed class IntervalConverterTests
{
    [Fact]
    public void ToMilliseconds_ShouldConvertPartsToTotalMilliseconds()
    {
        var parts = new IntervalParts(0, 0, 1, 2, 3, 4);

        var milliseconds = IntervalConverter.ToMilliseconds(parts);

        milliseconds.Should().Be(1234);
    }

    [Fact]
    public void FromMilliseconds_ShouldConvertToIntervalParts()
    {
        var parts = IntervalConverter.FromMilliseconds(1234);

        parts.Hours.Should().Be(0);
        parts.Minutes.Should().Be(0);
        parts.Seconds.Should().Be(1);
        parts.Tenths.Should().Be(2);
        parts.Hundredths.Should().Be(3);
        parts.Thousandths.Should().Be(4);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0, 0, 0)]
    [InlineData(0, -1, 0, 0, 0, 0)]
    [InlineData(0, 0, -1, 0, 0, 0)]
    [InlineData(0, 0, 0, -1, 0, 0)]
    [InlineData(0, 0, 0, 0, -1, 0)]
    [InlineData(0, 0, 0, 0, 0, -1)]
    public void ToMilliseconds_ShouldThrow_WhenAnyPartIsNegative(
        int hours,
        int minutes,
        int seconds,
        int tenths,
        int hundredths,
        int thousandths)
    {
        var parts = new IntervalParts(hours, minutes, seconds, tenths, hundredths, thousandths);

        var act = () => IntervalConverter.ToMilliseconds(parts);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
