using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Validation;

namespace RjClicker.Core.Tests.Validation;

public sealed class RuntimeConfigValidatorTests
{
    [Fact]
    public void Validate_ShouldReject_WhenNoTargets()
    {
        var config = RuntimeConfigFactory.Create(targets: Array.Empty<PointTarget>());

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("at least one target"));
    }

    [Fact]
    public void Validate_ShouldReject_WhenIntervalBelowOneMillisecond()
    {
        var config = RuntimeConfigFactory.Create(totalIntervalMilliseconds: 0);

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Interval"));
    }

    [Fact]
    public void Validate_ShouldPass_ForValidConfig()
    {
        var config = RuntimeConfigFactory.Create();

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReject_WhenCounterEnabledAndMaxClicksIsMissingOrInvalid()
    {
        var missingMaxClicksConfig = RuntimeConfigFactory.Create(useCounter: true, maxClicks: null);
        var zeroMaxClicksConfig = RuntimeConfigFactory.Create(useCounter: true, maxClicks: 0);

        var missingResult = RuntimeConfigValidator.Validate(missingMaxClicksConfig);
        var zeroResult = RuntimeConfigValidator.Validate(zeroMaxClicksConfig);

        missingResult.IsValid.Should().BeFalse();
        missingResult.Errors.Should().Contain(error => error.Contains("MaxClicks >= 1"));
        zeroResult.IsValid.Should().BeFalse();
        zeroResult.Errors.Should().Contain(error => error.Contains("MaxClicks >= 1"));
    }
}

internal static class RuntimeConfigFactory
{
    public static RuntimeConfig Create(
        int totalIntervalMilliseconds = 100,
        bool useCounter = false,
        int? maxClicks = null,
        IReadOnlyList<PointTarget>? targets = null)
    {
        return new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter,
            maxClicks,
            targets ?? new[] { PointTarget.Absolute(100, 200) });
    }
}