using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Validation;

namespace RjClicker.Core.Tests.Validation;

public sealed class RuntimeConfigValidatorTests
{
    [Fact]
    public void Validate_ShouldThrowArgumentNullException_WhenConfigIsNull()
    {
        var act = () => RuntimeConfigValidator.Validate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_ShouldPass_WhenForegroundModeHasNoTargets()
    {
        var config = RuntimeConfigFactory.Create(targets: Array.Empty<PointTarget>());

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReject_WhenBackgroundModeHasNoTargets()
    {
        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            100,
            ClickMode.Simultaneous,
            DeliveryMode.Background,
            false,
            null,
            null!);

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Background mode requires at least one target"));
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

    [Fact]
    public void ValidationResult_ShouldCopyAndProtectErrorsFromMutation()
    {
        var sourceErrors = new List<string> { "first" };
        var result = new ValidationResult(false, sourceErrors);

        sourceErrors.Add("second");

        result.Errors.Should().ContainSingle().Which.Should().Be("first");
        var mutateErrors = () => ((IList<string>)result.Errors).Add("third");
        mutateErrors.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void RuntimeConfig_ShouldNotBeAffectedByExternalTargetsMutation()
    {
        var sourceTargets = new List<PointTarget> { PointTarget.Absolute(100, 200) };
        var config = RuntimeConfigFactory.Create(targets: sourceTargets);

        sourceTargets.Add(PointTarget.Absolute(300, 400));

        config.Targets.Should().HaveCount(1);
    }

    [Fact]
    public void RuntimeConfig_ShouldUseEmptyTargets_WhenConstructedWithNullTargets()
    {
        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            100,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            false,
            null,
            null);

        config.Targets.Should().NotBeNull();
        config.Targets.Should().BeEmpty();
    }

    [Fact]
    public void RuntimeConfig_ShouldSnapshotTargets_WhenReassignedViaWithExpression()
    {
        var sourceTargets = new List<PointTarget> { PointTarget.Absolute(100, 200) };
        var config = RuntimeConfigFactory.Create();
        var reassignedConfig = config with { Targets = sourceTargets };

        sourceTargets.Add(PointTarget.Absolute(300, 400));

        reassignedConfig.Targets.Should().HaveCount(1);
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