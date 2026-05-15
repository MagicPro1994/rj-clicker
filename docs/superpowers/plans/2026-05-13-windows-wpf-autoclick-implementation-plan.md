# Windows WPF AutoClick Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Windows-only WPF autoclick app with screenshot-parity controls, multi-point click modes, global hotkeys, and foreground/background click delivery.

**Architecture:** Use a layered structure with WPF presentation, application orchestration, and infrastructure adapters over Win32 APIs. Keep runtime behavior in deterministic services with interfaces so core logic is unit-testable. Run click scheduling and dispatch off the UI thread with cancellation-based lifecycle control.

**Tech Stack:** .NET 8, C#, WPF, xUnit, FluentAssertions, Microsoft.Extensions.DependencyInjection, Win32 interop (PInvoke)

---

## Scope Check

The approved design is one coherent subsystem (single desktop app), so one implementation plan is appropriate.

## File Structure

### Solution and projects
- Create: `src/RjClicker.sln`
- Create: `src/RjClicker.App/RjClicker.App.csproj`
- Create: `tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj`
- Create: `tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj`

### Application project structure
- Create: `src/RjClicker.App/App.xaml`
- Create: `src/RjClicker.App/App.xaml.cs`
- Create: `src/RjClicker.App/MainWindow.xaml`
- Create: `src/RjClicker.App/MainWindow.xaml.cs`
- Create: `src/RjClicker.App/Composition/ServiceRegistration.cs`

### Core models and logic
- Create: `src/RjClicker.App/Core/Models/RuntimeConfig.cs`
- Create: `src/RjClicker.App/Core/Models/PointTarget.cs`
- Create: `src/RjClicker.App/Core/Models/Enums.cs`
- Create: `src/RjClicker.App/Core/Timing/IntervalConverter.cs`
- Create: `src/RjClicker.App/Core/Validation/RuntimeConfigValidator.cs`
- Create: `src/RjClicker.App/Core/Sessions/ClickSessionController.cs`
- Create: `src/RjClicker.App/Core/Sessions/ClickScheduler.cs`
- Create: `src/RjClicker.App/Core/Sessions/ClickDispatcher.cs`

### Infrastructure adapters
- Create: `src/RjClicker.App/Infrastructure/Hotkeys/IGlobalHotkeyService.cs`
- Create: `src/RjClicker.App/Infrastructure/Hotkeys/Win32GlobalHotkeyService.cs`
- Create: `src/RjClicker.App/Infrastructure/Input/IForegroundClickService.cs`
- Create: `src/RjClicker.App/Infrastructure/Input/Win32ForegroundClickService.cs`
- Create: `src/RjClicker.App/Infrastructure/Input/IBackgroundClickService.cs`
- Create: `src/RjClicker.App/Infrastructure/Input/Win32BackgroundClickService.cs`
- Create: `src/RjClicker.App/Infrastructure/Points/IPointCaptureService.cs`
- Create: `src/RjClicker.App/Infrastructure/Points/PointCaptureService.cs`
- Create: `src/RjClicker.App/Infrastructure/Windows/IWindowBindingService.cs`
- Create: `src/RjClicker.App/Infrastructure/Windows/WindowBindingService.cs`
- Create: `src/RjClicker.App/Infrastructure/Settings/ISettingsStore.cs`
- Create: `src/RjClicker.App/Infrastructure/Settings/JsonSettingsStore.cs`

### View models
- Create: `src/RjClicker.App/Presentation/ViewModels/MainViewModel.cs`
- Create: `src/RjClicker.App/Presentation/ViewModels/PointTargetViewModel.cs`
- Create: `src/RjClicker.App/Presentation/Commands/RelayCommand.cs`

### Tests
- Create: `tests/RjClicker.Core.Tests/Timing/IntervalConverterTests.cs`
- Create: `tests/RjClicker.Core.Tests/Validation/RuntimeConfigValidatorTests.cs`
- Create: `tests/RjClicker.Core.Tests/Sessions/ClickSchedulerTests.cs`
- Create: `tests/RjClicker.Core.Tests/Sessions/ClickSessionControllerTests.cs`
- Create: `tests/RjClicker.Integration.Tests/SessionLifecycleTests.cs`
- Create: `tests/RjClicker.Integration.Tests/BackgroundModeBehaviorTests.cs`

---

### Task 1: Bootstrap solution and test harness

**Files:**
- Create: `src/RjClicker.sln`
- Create: `src/RjClicker.App/RjClicker.App.csproj`
- Create: `tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj`
- Create: `tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj`

- [ ] **Step 1: Create solution and projects**

Run: `dotnet new sln -n RjClicker -o src`
Run: `dotnet new wpf -n RjClicker.App -o src/RjClicker.App --framework net8.0-windows`
Run: `dotnet new xunit -n RjClicker.Core.Tests -o tests/RjClicker.Core.Tests`
Run: `dotnet new xunit -n RjClicker.Integration.Tests -o tests/RjClicker.Integration.Tests`

Expected: commands succeed and project files are created

- [ ] **Step 2: Wire project references and test packages**

Run: `dotnet sln src/RjClicker.sln add src/RjClicker.App/RjClicker.App.csproj`
Run: `dotnet sln src/RjClicker.sln add tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj`
Run: `dotnet sln src/RjClicker.sln add tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj`
Run: `dotnet add tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj reference src/RjClicker.App/RjClicker.App.csproj`
Run: `dotnet add tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj reference src/RjClicker.App/RjClicker.App.csproj`
Run: `dotnet add tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj package FluentAssertions`
Run: `dotnet add tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj package FluentAssertions`

Expected: references and packages added without errors

- [ ] **Step 3: Run baseline tests**

Run: `dotnet test src/RjClicker.sln -v minimal`
Expected: PASS for template tests

- [ ] **Step 4: Commit bootstrap**

Run: `git add src tests`
Run: `git commit -m "chore: bootstrap wpf solution and test projects"`

Expected: one commit created

### Task 2: Implement interval conversion with TDD

**Files:**
- Create: `src/RjClicker.App/Core/Timing/IntervalConverter.cs`
- Create: `src/RjClicker.App/Core/Models/IntervalParts.cs`
- Test: `tests/RjClicker.Core.Tests/Timing/IntervalConverterTests.cs`

- [ ] **Step 1: Write failing interval conversion tests**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Timing;
using Xunit;

namespace RjClicker.Core.Tests.Timing;

public sealed class IntervalConverterTests
{
    [Fact]
    public void ToMilliseconds_ShouldConvertPartsCorrectly()
    {
        var parts = new IntervalParts(hours: 0, minutes: 0, seconds: 1, tenths: 2, hundredths: 3, thousandths: 4);

        var totalMs = IntervalConverter.ToMilliseconds(parts);

        totalMs.Should().Be(1234);
    }

    [Fact]
    public void FromMilliseconds_ShouldConvertBackToParts()
    {
        var parts = IntervalConverter.FromMilliseconds(1234);

        parts.Hours.Should().Be(0);
        parts.Minutes.Should().Be(0);
        parts.Seconds.Should().Be(1);
        parts.Tenths.Should().Be(2);
        parts.Hundredths.Should().Be(3);
        parts.Thousandths.Should().Be(4);
    }

    [Fact]
    public void ToMilliseconds_ShouldThrowForNegativePart()
    {
        var parts = new IntervalParts(hours: 0, minutes: 0, seconds: -1, tenths: 0, hundredths: 0, thousandths: 0);

        var action = () => IntervalConverter.ToMilliseconds(parts);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
```

- [ ] **Step 2: Run the focused test and confirm it fails**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~IntervalConverterTests -v minimal`
Expected: FAIL with missing types

- [ ] **Step 3: Implement minimal converter and model**

```csharp
namespace RjClicker.App.Core.Models;

public readonly record struct IntervalParts(
    int Hours,
    int Minutes,
    int Seconds,
    int Tenths,
    int Hundredths,
    int Thousandths);
```

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Timing;

public static class IntervalConverter
{
    public static int ToMilliseconds(IntervalParts parts)
    {
        ValidateNonNegative(parts);

        checked
        {
            return (parts.Hours * 3_600_000)
                + (parts.Minutes * 60_000)
                + (parts.Seconds * 1_000)
                + (parts.Tenths * 100)
                + (parts.Hundredths * 10)
                + parts.Thousandths;
        }
    }

    public static IntervalParts FromMilliseconds(int totalMilliseconds)
    {
        if (totalMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalMilliseconds));
        }

        var remaining = totalMilliseconds;
        var hours = remaining / 3_600_000;
        remaining %= 3_600_000;

        var minutes = remaining / 60_000;
        remaining %= 60_000;

        var seconds = remaining / 1_000;
        remaining %= 1_000;

        var tenths = remaining / 100;
        remaining %= 100;

        var hundredths = remaining / 10;
        remaining %= 10;

        var thousandths = remaining;

        return new IntervalParts(hours, minutes, seconds, tenths, hundredths, thousandths);
    }

    private static void ValidateNonNegative(IntervalParts parts)
    {
        if (parts.Hours < 0 || parts.Minutes < 0 || parts.Seconds < 0 ||
            parts.Tenths < 0 || parts.Hundredths < 0 || parts.Thousandths < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parts));
        }
    }
}
```

- [ ] **Step 4: Run tests to verify pass**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~IntervalConverterTests -v minimal`
Expected: PASS

- [ ] **Step 5: Commit**

Run: `git add src/RjClicker.App/Core/Timing src/RjClicker.App/Core/Models tests/RjClicker.Core.Tests/Timing`
Run: `git commit -m "feat(core): add interval conversion model and tests"`

Expected: one commit created

### Task 3: Implement runtime configuration and validation with TDD

**Files:**
- Create: `src/RjClicker.App/Core/Models/Enums.cs`
- Create: `src/RjClicker.App/Core/Models/PointTarget.cs`
- Create: `src/RjClicker.App/Core/Models/RuntimeConfig.cs`
- Create: `src/RjClicker.App/Core/Validation/RuntimeConfigValidator.cs`
- Test: `tests/RjClicker.Core.Tests/Validation/RuntimeConfigValidatorTests.cs`

- [ ] **Step 1: Write failing validator tests**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Validation;
using Xunit;

namespace RjClicker.Core.Tests.Validation;

public sealed class RuntimeConfigValidatorTests
{
    [Fact]
    public void Validate_ShouldReject_WhenNoTargets()
    {
        var config = RuntimeConfigFactory.Create(targets: Array.Empty<PointTarget>());

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least one target"));
    }

    [Fact]
    public void Validate_ShouldReject_WhenIntervalBelowOneMillisecond()
    {
        var config = RuntimeConfigFactory.Create(totalIntervalMilliseconds: 0);

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Interval"));
    }

    [Fact]
    public void Validate_ShouldPass_ForValidConfig()
    {
        var config = RuntimeConfigFactory.Create();

        var result = RuntimeConfigValidator.Validate(config);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Add runtime config test helper**

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.Core.Tests.Validation;

internal static class RuntimeConfigFactory
{
    public static RuntimeConfig Create(
        int totalIntervalMilliseconds = 100,
        IReadOnlyList<PointTarget>? targets = null)
    {
        return new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: false,
            maxClicks: null,
            targets ?? new[] { PointTarget.Absolute(100, 200) });
    }
}
```

- [ ] **Step 3: Run focused tests and confirm failure**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~RuntimeConfigValidatorTests -v minimal`
Expected: FAIL with missing production types

- [ ] **Step 4: Implement minimal models and validator**

```csharp
namespace RjClicker.App.Core.Models;

public enum MouseButton { Left, Right }
public enum PressType { Single, Double }
public enum ClickMode { Simultaneous, Sequence }
public enum DeliveryMode { Foreground, Background }
public enum TargetType { Absolute, WindowRelative }
```

```csharp
namespace RjClicker.App.Core.Models;

public sealed record PointTarget(TargetType TargetType, int X, int Y, nint? TargetWindowId)
{
    public static PointTarget Absolute(int x, int y) => new(TargetType.Absolute, x, y, null);

    public static PointTarget WindowRelative(int x, int y, nint targetWindowId) =>
        new(TargetType.WindowRelative, x, y, targetWindowId);
}
```

```csharp
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
```

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Validation;

public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);

public static class RuntimeConfigValidator
{
    public static ValidationResult Validate(RuntimeConfig config)
    {
        var errors = new List<string>();

        if (config.TotalIntervalMilliseconds < 1)
        {
            errors.Add("Interval must be at least 1 ms");
        }

        if (config.Targets.Count == 0)
        {
            errors.Add("Config must include at least one target");
        }

        if (config.UseCounter && (!config.MaxClicks.HasValue || config.MaxClicks.Value < 1))
        {
            errors.Add("Counter requires MaxClicks >= 1");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }
}
```

- [ ] **Step 5: Run tests to verify pass**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~RuntimeConfigValidatorTests -v minimal`
Expected: PASS

- [ ] **Step 6: Commit**

Run: `git add src/RjClicker.App/Core/Models src/RjClicker.App/Core/Validation tests/RjClicker.Core.Tests/Validation`
Run: `git commit -m "feat(core): add runtime config validation"`

Expected: one commit created

### Task 4: Add session scheduler and dispatcher with TDD

**Files:**
- Create: `src/RjClicker.App/Core/Sessions/ClickScheduler.cs`
- Create: `src/RjClicker.App/Core/Sessions/ClickDispatcher.cs`
- Create: `src/RjClicker.App/Core/Sessions/ClickSessionController.cs`
- Test: `tests/RjClicker.Core.Tests/Sessions/ClickSchedulerTests.cs`
- Test: `tests/RjClicker.Core.Tests/Sessions/ClickSessionControllerTests.cs`

- [ ] **Step 1: Write failing scheduler tests**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Sessions;
using Xunit;

namespace RjClicker.Core.Tests.Sessions;

public sealed class ClickSchedulerTests
{
    [Fact]
    public async Task RunAsync_ShouldInvokeTickHandler_UntilCancelled()
    {
        var ticks = 0;
        using var cts = new CancellationTokenSource();

        var scheduler = new ClickScheduler();
        var runTask = scheduler.RunAsync(
            intervalMilliseconds: 5,
            onTick: () =>
            {
                ticks++;
                if (ticks == 3)
                {
                    cts.Cancel();
                }
            },
            cts.Token);

        await runTask;

        ticks.Should().BeGreaterOrEqualTo(3);
    }
}
```

- [ ] **Step 2: Run focused tests and confirm failure**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~ClickSchedulerTests -v minimal`
Expected: FAIL with missing scheduler

- [ ] **Step 3: Implement scheduler**

```csharp
namespace RjClicker.App.Core.Sessions;

public sealed class ClickScheduler
{
    public async Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
    {
        if (intervalMilliseconds < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalMilliseconds));
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            onTick();
            await Task.Delay(intervalMilliseconds, cancellationToken);
        }
    }
}
```

- [ ] **Step 4: Write failing session controller tests**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using Xunit;

namespace RjClicker.Core.Tests.Sessions;

public sealed class ClickSessionControllerTests
{
    [Fact]
    public async Task StartAsync_ShouldStopAtCounterLimit()
    {
        var dispatcher = new FakeDispatcher();
        var scheduler = new FakeScheduler(maxTicks: 10);
        var controller = new ClickSessionController(dispatcher, scheduler);
        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            1,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: true,
            maxClicks: 2,
            new[] { PointTarget.Absolute(1, 1) });

        await controller.StartAsync(config, CancellationToken.None);

        dispatcher.DispatchCalls.Should().Be(2);
        controller.IsRunning.Should().BeFalse();
    }
}
```

- [ ] **Step 5: Add test fakes for scheduler/dispatcher**

```csharp
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;

namespace RjClicker.Core.Tests.Sessions;

internal sealed class FakeDispatcher : IClickDispatcher
{
    public int DispatchCalls { get; private set; }

    public Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        DispatchCalls++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeScheduler : IClickScheduler
{
    private readonly int _maxTicks;

    public FakeScheduler(int maxTicks)
    {
        _maxTicks = maxTicks;
    }

    public Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
    {
        for (var i = 0; i < _maxTicks; i++)
        {
            onTick();
        }

        return Task.CompletedTask;
    }
}
```

- [ ] **Step 6: Implement dispatcher interface, scheduler interface, and session controller**

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Sessions;

public interface IClickScheduler
{
    Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken);
}

public interface IClickDispatcher
{
    Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken);
}
```

```csharp
namespace RjClicker.App.Core.Sessions;

public sealed class ClickScheduler : IClickScheduler
{
    public async Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
    {
        if (intervalMilliseconds < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalMilliseconds));
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            onTick();
            await Task.Delay(intervalMilliseconds, cancellationToken);
        }
    }
}
```

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Sessions;

public sealed class ClickSessionController
{
    private readonly IClickDispatcher _dispatcher;
    private readonly IClickScheduler _scheduler;

    public ClickSessionController(IClickDispatcher dispatcher, IClickScheduler scheduler)
    {
        _dispatcher = dispatcher;
        _scheduler = scheduler;
    }

    public bool IsRunning { get; private set; }

    public async Task StartAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Session already running");
        }

        IsRunning = true;
        var clicks = 0;

        await _scheduler.RunAsync(
            config.TotalIntervalMilliseconds,
            onTick: () =>
            {
                if (config.UseCounter && config.MaxClicks.HasValue && clicks >= config.MaxClicks.Value)
                {
                    IsRunning = false;
                    return;
                }

                _dispatcher.DispatchAsync(config, cancellationToken).GetAwaiter().GetResult();
                clicks++;

                if (config.UseCounter && config.MaxClicks.HasValue && clicks >= config.MaxClicks.Value)
                {
                    IsRunning = false;
                }
            },
            cancellationToken);

        IsRunning = false;
    }
}
```

- [ ] **Step 7: Run focused session tests**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~Sessions -v minimal`
Expected: PASS

- [ ] **Step 8: Commit**

Run: `git add src/RjClicker.App/Core/Sessions tests/RjClicker.Core.Tests/Sessions`
Run: `git commit -m "feat(core): add click scheduler and session controller"`

Expected: one commit created

### Task 5: Implement click delivery services and mode behavior with TDD

**Files:**
- Create: `src/RjClicker.App/Infrastructure/Input/IForegroundClickService.cs`
- Create: `src/RjClicker.App/Infrastructure/Input/IBackgroundClickService.cs`
- Create: `src/RjClicker.App/Infrastructure/Input/Win32ForegroundClickService.cs`
- Create: `src/RjClicker.App/Infrastructure/Input/Win32BackgroundClickService.cs`
- Modify: `src/RjClicker.App/Core/Sessions/ClickDispatcher.cs`
- Test: `tests/RjClicker.Core.Tests/Sessions/ClickDispatcherTests.cs`

- [ ] **Step 1: Write failing dispatcher mode tests**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using Xunit;

namespace RjClicker.Core.Tests.Sessions;

public sealed class ClickDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_ShouldUseForeground_ForForegroundMode()
    {
        var fg = new FakeForegroundClickService();
        var bg = new FakeBackgroundClickService();
        var dispatcher = new ClickDispatcher(fg, bg);
        var config = RuntimeConfigFactory.Create(deliveryMode: DeliveryMode.Foreground);

        await dispatcher.DispatchAsync(config, CancellationToken.None);

        fg.Calls.Should().Be(1);
        bg.Calls.Should().Be(0);
    }

    [Fact]
    public async Task DispatchAsync_ShouldUseBackground_ForBackgroundMode()
    {
        var fg = new FakeForegroundClickService();
        var bg = new FakeBackgroundClickService();
        var dispatcher = new ClickDispatcher(fg, bg);
        var config = RuntimeConfigFactory.Create(deliveryMode: DeliveryMode.Background);

        await dispatcher.DispatchAsync(config, CancellationToken.None);

        fg.Calls.Should().Be(0);
        bg.Calls.Should().Be(1);
    }
}
```

- [ ] **Step 2: Add test fakes for delivery services**

```csharp
using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.Input;

namespace RjClicker.Core.Tests.Sessions;

internal sealed class FakeForegroundClickService : IForegroundClickService
{
    public int Calls { get; private set; }

    public Task ClickAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeBackgroundClickService : IBackgroundClickService
{
    public int Calls { get; private set; }

    public Task<BackgroundClickResult> ClickAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(BackgroundClickResult.Success());
    }
}
```

- [ ] **Step 3: Run focused tests and confirm failure**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~ClickDispatcherTests -v minimal`
Expected: FAIL with missing dispatcher/service contracts

- [ ] **Step 4: Implement service interfaces and dispatcher**

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Input;

public interface IForegroundClickService
{
    Task ClickAsync(RuntimeConfig config, CancellationToken cancellationToken);
}

public interface IBackgroundClickService
{
    Task<BackgroundClickResult> ClickAsync(RuntimeConfig config, CancellationToken cancellationToken);
}

public sealed record BackgroundClickResult(bool Succeeded, string? Warning)
{
    public static BackgroundClickResult Success() => new(true, null);
    public static BackgroundClickResult WarningResult(string warning) => new(false, warning);
}
```

```csharp
using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.Input;

namespace RjClicker.App.Core.Sessions;

public sealed class ClickDispatcher : IClickDispatcher
{
    private readonly IForegroundClickService _foregroundClickService;
    private readonly IBackgroundClickService _backgroundClickService;

    public ClickDispatcher(
        IForegroundClickService foregroundClickService,
        IBackgroundClickService backgroundClickService)
    {
        _foregroundClickService = foregroundClickService;
        _backgroundClickService = backgroundClickService;
    }

    public async Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        if (config.DeliveryMode == DeliveryMode.Foreground)
        {
            await _foregroundClickService.ClickAsync(config, cancellationToken);
            return;
        }

        _ = await _backgroundClickService.ClickAsync(config, cancellationToken);
    }
}
```

- [ ] **Step 5: Run tests to verify pass**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~ClickDispatcherTests -v minimal`
Expected: PASS

- [ ] **Step 6: Implement Win32 service shells for later completion**

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Input;

public sealed class Win32ForegroundClickService : IForegroundClickService
{
    public Task ClickAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        // Implement SendInput dispatch in Task 8
        return Task.CompletedTask;
    }
}
```

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Input;

public sealed class Win32BackgroundClickService : IBackgroundClickService
{
    public Task<BackgroundClickResult> ClickAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        // Implement PostMessage dispatch in Task 8
        return Task.FromResult(BackgroundClickResult.Success());
    }
}
```

- [ ] **Step 7: Commit**

Run: `git add src/RjClicker.App/Core/Sessions src/RjClicker.App/Infrastructure/Input tests/RjClicker.Core.Tests/Sessions`
Run: `git commit -m "feat(core): add click delivery mode dispatch contracts"`

Expected: one commit created

### Task 6: Implement hotkeys, window binding, and point capture with TDD-first contracts

**Files:**
- Create: `src/RjClicker.App/Infrastructure/Hotkeys/IGlobalHotkeyService.cs`
- Create: `src/RjClicker.App/Infrastructure/Windows/IWindowBindingService.cs`
- Create: `src/RjClicker.App/Infrastructure/Points/IPointCaptureService.cs`
- Create: `src/RjClicker.App/Infrastructure/Hotkeys/Win32GlobalHotkeyService.cs`
- Create: `src/RjClicker.App/Infrastructure/Windows/WindowBindingService.cs`
- Create: `src/RjClicker.App/Infrastructure/Points/PointCaptureService.cs`
- Test: `tests/RjClicker.Integration.Tests/SessionLifecycleTests.cs`

- [ ] **Step 1: Write failing lifecycle integration test with fakes**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using Xunit;

namespace RjClicker.Integration.Tests;

public sealed class SessionLifecycleTests
{
    [Fact]
    public async Task StartStop_ShouldRegisterAndUnregisterHotkeys()
    {
        var hotkeys = new FakeHotkeyService();
        var dispatcher = new FakeDispatcher();
        var scheduler = new FakeScheduler(maxTicks: 1);
        var controller = new ClickSessionController(dispatcher, scheduler, hotkeys);
        var config = RuntimeConfigFactory.Create();

        await controller.StartAsync(config, CancellationToken.None);
        await controller.StopAsync();

        hotkeys.RegisterCalls.Should().BeGreaterOrEqualTo(1);
        hotkeys.UnregisterCalls.Should().BeGreaterOrEqualTo(1);
    }
}
```

- [ ] **Step 2: Run focused integration test and confirm failure**

Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~SessionLifecycleTests -v minimal`
Expected: FAIL with constructor mismatch/missing hotkey service

- [ ] **Step 3: Define infrastructure contracts and update controller constructor**

```csharp
namespace RjClicker.App.Infrastructure.Hotkeys;

public interface IGlobalHotkeyService
{
    Task RegisterAsync(CancellationToken cancellationToken);
    Task UnregisterAsync(CancellationToken cancellationToken);
}
```

```csharp
namespace RjClicker.App.Infrastructure.Windows;

public interface IWindowBindingService
{
    bool TryResolveWindow(nint windowId, out WindowBounds bounds);
}

public readonly record struct WindowBounds(int Left, int Top, int Width, int Height);
```

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Points;

public interface IPointCaptureService
{
    Task<IReadOnlyList<PointTarget>> CaptureAsync(CancellationToken cancellationToken);
}
```

```csharp
using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.Hotkeys;

namespace RjClicker.App.Core.Sessions;

public sealed class ClickSessionController
{
    private readonly IClickDispatcher _dispatcher;
    private readonly IClickScheduler _scheduler;
    private readonly IGlobalHotkeyService _hotkeyService;

    public ClickSessionController(
        IClickDispatcher dispatcher,
        IClickScheduler scheduler,
        IGlobalHotkeyService hotkeyService)
    {
        _dispatcher = dispatcher;
        _scheduler = scheduler;
        _hotkeyService = hotkeyService;
    }

    public bool IsRunning { get; private set; }

    public async Task StartAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        await _hotkeyService.RegisterAsync(cancellationToken);
        IsRunning = true;

        await _scheduler.RunAsync(
            config.TotalIntervalMilliseconds,
            onTick: () => _dispatcher.DispatchAsync(config, cancellationToken).GetAwaiter().GetResult(),
            cancellationToken);

        IsRunning = false;
    }

    public async Task StopAsync()
    {
        IsRunning = false;
        await _hotkeyService.UnregisterAsync(CancellationToken.None);
    }
}
```

- [ ] **Step 4: Add minimal Win32 class shells and fake implementation in tests**

```csharp
using RjClicker.App.Infrastructure.Hotkeys;

namespace RjClicker.App.Infrastructure.Hotkeys;

public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService
{
    public Task RegisterAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

```csharp
using RjClicker.App.Infrastructure.Windows;

namespace RjClicker.App.Infrastructure.Windows;

public sealed class WindowBindingService : IWindowBindingService
{
    public bool TryResolveWindow(nint windowId, out WindowBounds bounds)
    {
        bounds = default;
        return false;
    }
}
```

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Points;

public sealed class PointCaptureService : IPointCaptureService
{
    public Task<IReadOnlyList<PointTarget>> CaptureAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<PointTarget>>(Array.Empty<PointTarget>());
    }
}
```

- [ ] **Step 5: Run integration test to verify pass**

Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~SessionLifecycleTests -v minimal`
Expected: PASS

- [ ] **Step 6: Commit**

Run: `git add src/RjClicker.App/Infrastructure src/RjClicker.App/Core/Sessions tests/RjClicker.Integration.Tests`
Run: `git commit -m "feat(app): add lifecycle contracts for hotkeys windows and points"`

Expected: one commit created

### Task 7: Build WPF UI parity and ViewModel behavior with TDD for ViewModel

**Files:**
- Create: `src/RjClicker.App/Presentation/ViewModels/MainViewModel.cs`
- Create: `src/RjClicker.App/Presentation/ViewModels/PointTargetViewModel.cs`
- Create: `src/RjClicker.App/Presentation/Commands/RelayCommand.cs`
- Modify: `src/RjClicker.App/MainWindow.xaml`
- Test: `tests/RjClicker.Core.Tests/Presentation/MainViewModelTests.cs`

- [ ] **Step 1: Write failing ViewModel tests for timing sync and mode toggles**

```csharp
using FluentAssertions;
using RjClicker.App.Presentation.ViewModels;
using Xunit;

namespace RjClicker.Core.Tests.Presentation;

public sealed class MainViewModelTests
{
    [Fact]
    public void TotalIntervalMilliseconds_Setter_ShouldSyncSegmentedFields()
    {
        var vm = new MainViewModel();

        vm.TotalIntervalMilliseconds = 1234;

        vm.Seconds.Should().Be(1);
        vm.Tenths.Should().Be(2);
        vm.Hundredths.Should().Be(3);
        vm.Thousandths.Should().Be(4);
    }

    [Fact]
    public void SegmentedFields_Change_ShouldSyncTotalMilliseconds()
    {
        var vm = new MainViewModel
        {
            Seconds = 1,
            Tenths = 2,
            Hundredths = 3,
            Thousandths = 4
        };

        vm.RecalculateTotalInterval();

        vm.TotalIntervalMilliseconds.Should().Be(1234);
    }
}
```

- [ ] **Step 2: Run focused ViewModel tests and confirm failure**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~MainViewModelTests -v minimal`
Expected: FAIL with missing view model

- [ ] **Step 3: Implement minimal MainViewModel and command support**

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Timing;

namespace RjClicker.App.Presentation.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private int _hours;
    private int _minutes;
    private int _seconds;
    private int _tenths;
    private int _hundredths;
    private int _thousandths;
    private int _totalIntervalMilliseconds = 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Hours { get => _hours; set => SetField(ref _hours, value); }
    public int Minutes { get => _minutes; set => SetField(ref _minutes, value); }
    public int Seconds { get => _seconds; set => SetField(ref _seconds, value); }
    public int Tenths { get => _tenths; set => SetField(ref _tenths, value); }
    public int Hundredths { get => _hundredths; set => SetField(ref _hundredths, value); }
    public int Thousandths { get => _thousandths; set => SetField(ref _thousandths, value); }

    public int TotalIntervalMilliseconds
    {
        get => _totalIntervalMilliseconds;
        set
        {
            if (SetField(ref _totalIntervalMilliseconds, value))
            {
                var parts = IntervalConverter.FromMilliseconds(Math.Max(value, 1));
                Hours = parts.Hours;
                Minutes = parts.Minutes;
                Seconds = parts.Seconds;
                Tenths = parts.Tenths;
                Hundredths = parts.Hundredths;
                Thousandths = parts.Thousandths;
            }
        }
    }

    public void RecalculateTotalInterval()
    {
        var parts = new IntervalParts(Hours, Minutes, Seconds, Tenths, Hundredths, Thousandths);
        TotalIntervalMilliseconds = IntervalConverter.ToMilliseconds(parts);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
```

```csharp
using System.Windows.Input;

namespace RjClicker.App.Presentation.Commands;

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

- [ ] **Step 4: Update MainWindow XAML with parity controls and new v1 fields**

```xml
<Window x:Class="RjClicker.App.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="RJ Clicker" Height="520" Width="720">
    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <GroupBox Header="Settings" Grid.Row="0" Margin="0,0,0,8">
            <StackPanel Orientation="Horizontal" Margin="8">
                <TextBlock Text="Mouse" VerticalAlignment="Center" Margin="0,0,8,0"/>
                <ComboBox Width="100"/>
                <TextBlock Text="Press" VerticalAlignment="Center" Margin="12,0,8,0"/>
                <ComboBox Width="100"/>
                <TextBlock Text="Mode" VerticalAlignment="Center" Margin="12,0,8,0"/>
                <ComboBox Width="140"/>
                <TextBlock Text="Delivery" VerticalAlignment="Center" Margin="12,0,8,0"/>
                <ComboBox Width="180"/>
            </StackPanel>
        </GroupBox>

        <GroupBox Header="Click Interval" Grid.Row="1" Margin="0,0,0,8">
            <StackPanel Orientation="Horizontal" Margin="8">
                <TextBlock Text="H"/><TextBox Width="40" Text="{Binding Hours}"/>
                <TextBlock Text="M" Margin="8,0,0,0"/><TextBox Width="40" Text="{Binding Minutes}"/>
                <TextBlock Text="S" Margin="8,0,0,0"/><TextBox Width="40" Text="{Binding Seconds}"/>
                <TextBlock Text="1/10" Margin="8,0,0,0"/><TextBox Width="40" Text="{Binding Tenths}"/>
                <TextBlock Text="1/100" Margin="8,0,0,0"/><TextBox Width="40" Text="{Binding Hundredths}"/>
                <TextBlock Text="1/1000" Margin="8,0,0,0"/><TextBox Width="40" Text="{Binding Thousandths}"/>
                <TextBlock Text="Total ms" Margin="12,0,0,0"/>
                <TextBox Width="80" Text="{Binding TotalIntervalMilliseconds}"/>
            </StackPanel>
        </GroupBox>

        <GroupBox Header="Points" Grid.Row="2" Margin="0,0,0,8">
            <Grid Margin="8">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                <ListBox Grid.Column="0"/>
                <StackPanel Grid.Column="1" Margin="12,0,0,0">
                    <Button Content="Record" Margin="0,0,0,8"/>
                    <Button Content="Add" Margin="0,0,0,8"/>
                    <Button Content="Remove" Margin="0,0,0,8"/>
                    <Button Content="Move Up" Margin="0,0,0,8"/>
                    <Button Content="Move Down"/>
                </StackPanel>
            </Grid>
        </GroupBox>

        <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="Start" Width="100" Margin="0,0,8,0"/>
            <Button Content="Stop" Width="100"/>
        </StackPanel>
    </Grid>
</Window>
```

- [ ] **Step 5: Run ViewModel tests to verify pass**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~MainViewModelTests -v minimal`
Expected: PASS

- [ ] **Step 6: Commit**

Run: `git add src/RjClicker.App/Presentation src/RjClicker.App/MainWindow.xaml tests/RjClicker.Core.Tests/Presentation`
Run: `git commit -m "feat(ui): add wpf parity layout and timing sync behavior"`

Expected: one commit created

### Task 8: Complete Win32 implementations and background warning behavior

**Files:**
- Modify: `src/RjClicker.App/Infrastructure/Input/Win32ForegroundClickService.cs`
- Modify: `src/RjClicker.App/Infrastructure/Input/Win32BackgroundClickService.cs`
- Modify: `src/RjClicker.App/Infrastructure/Hotkeys/Win32GlobalHotkeyService.cs`
- Modify: `src/RjClicker.App/Infrastructure/Windows/WindowBindingService.cs`
- Test: `tests/RjClicker.Integration.Tests/BackgroundModeBehaviorTests.cs`

- [ ] **Step 1: Write failing background warning integration test**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using Xunit;

namespace RjClicker.Integration.Tests;

public sealed class BackgroundModeBehaviorTests
{
    [Fact]
    public async Task DispatchAsync_ShouldReturnWarning_WhenBackgroundDeliveryFails()
    {
        var fg = new FakeForegroundClickService();
        var bg = new FailingBackgroundClickService();
        var dispatcher = new ClickDispatcher(fg, bg);
        var config = RuntimeConfigFactory.Create(deliveryMode: DeliveryMode.Background);

        var result = await dispatcher.DispatchWithResultAsync(config, CancellationToken.None);

        result.Warning.Should().NotBeNullOrWhiteSpace();
        result.Warning.Should().Contain("foreground");
    }
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~BackgroundModeBehaviorTests -v minimal`
Expected: FAIL with missing result-returning API

- [ ] **Step 3: Extend dispatcher API and implement warning propagation**

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Sessions;

public sealed record DispatchResult(string? Warning)
{
    public static DispatchResult Success() => new(null);
    public static DispatchResult WarningResult(string warning) => new(warning);
}
```

```csharp
using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.Input;

namespace RjClicker.App.Core.Sessions;

public sealed class ClickDispatcher : IClickDispatcher
{
    private readonly IForegroundClickService _foregroundClickService;
    private readonly IBackgroundClickService _backgroundClickService;

    public ClickDispatcher(IForegroundClickService foregroundClickService, IBackgroundClickService backgroundClickService)
    {
        _foregroundClickService = foregroundClickService;
        _backgroundClickService = backgroundClickService;
    }

    public async Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        _ = await DispatchWithResultAsync(config, cancellationToken);
    }

    public async Task<DispatchResult> DispatchWithResultAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        if (config.DeliveryMode == DeliveryMode.Foreground)
        {
            await _foregroundClickService.ClickAsync(config, cancellationToken);
            return DispatchResult.Success();
        }

        var result = await _backgroundClickService.ClickAsync(config, cancellationToken);
        if (result.Succeeded)
        {
            return DispatchResult.Success();
        }

        return DispatchResult.WarningResult(result.Warning ?? "Background click failed, switch to foreground mode");
    }
}
```

- [ ] **Step 4: Implement Win32 PInvoke click and hotkey behavior**

```csharp
using System.Runtime.InteropServices;
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Input;

public sealed class Win32ForegroundClickService : IForegroundClickService
{
    public Task ClickAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        foreach (var target in config.Targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendInputClick(target.X, target.Y, config.MouseButton, config.PressType);
        }

        return Task.CompletedTask;
    }

    private static void SendInputClick(int x, int y, MouseButton button, PressType pressType)
    {
        SetCursorPos(x, y);

        var (downFlag, upFlag) = button switch
        {
            MouseButton.Left => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP),
            MouseButton.Right => (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP),
            _ => throw new ArgumentOutOfRangeException(nameof(button))
        };

        var repetitions = pressType == PressType.Double ? 2 : 1;
        for (var i = 0; i < repetitions; i++)
        {
            mouse_event(downFlag, 0, 0, 0, UIntPtr.Zero);
            mouse_event(upFlag, 0, 0, 0, UIntPtr.Zero);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
}
```

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Input;

public sealed class Win32BackgroundClickService : IBackgroundClickService
{
    public Task<BackgroundClickResult> ClickAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        foreach (var target in config.Targets)
        {
            if (target.TargetWindowId is null)
            {
                return Task.FromResult(BackgroundClickResult.WarningResult("Missing target window for background click"));
            }

            var lParam = MakeLParam(target.X, target.Y);
            var repetitions = config.PressType == PressType.Double ? 2 : 1;

            for (var i = 0; i < repetitions; i++)
            {
                var downMessage = config.MouseButton == MouseButton.Left ? WM_LBUTTONDOWN : WM_RBUTTONDOWN;
                var upMessage = config.MouseButton == MouseButton.Left ? WM_LBUTTONUP : WM_RBUTTONUP;

                _ = PostMessage(target.TargetWindowId.Value, downMessage, IntPtr.Zero, lParam);
                _ = PostMessage(target.TargetWindowId.Value, upMessage, IntPtr.Zero, lParam);
            }
        }

        return Task.FromResult(BackgroundClickResult.Success());
    }

    private static IntPtr MakeLParam(int x, int y)
    {
        var value = (y << 16) | (x & 0xFFFF);
        return new IntPtr(value);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(nint hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_LBUTTONDOWN = 0x0201;
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_RBUTTONDOWN = 0x0204;
    private const uint WM_RBUTTONUP = 0x0205;
}
```

```csharp
namespace RjClicker.App.Infrastructure.Hotkeys;

public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService
{
    private readonly nint _windowHandle;
    private const int StartStopHotkeyId = 1001;

    public Win32GlobalHotkeyService(nint windowHandle)
    {
        _windowHandle = windowHandle;
    }

    public Task RegisterAsync(CancellationToken cancellationToken)
    {
        var registered = RegisterHotKey(_windowHandle, StartStopHotkeyId, MOD_NOREPEAT, VK_F6);
        if (!registered)
        {
            throw new InvalidOperationException("Failed to register global hotkey F6");
        }

        return Task.CompletedTask;
    }

    public Task UnregisterAsync(CancellationToken cancellationToken)
    {
        _ = UnregisterHotKey(_windowHandle, StartStopHotkeyId);
        return Task.CompletedTask;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    private const uint MOD_NOREPEAT = 0x4000;
    private const uint VK_F6 = 0x75;
}
```

- [ ] **Step 5: Run integration tests and full test suite**

Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj -v minimal`
Expected: PASS

Run: `dotnet test src/RjClicker.sln -v minimal`
Expected: PASS

- [ ] **Step 6: Commit**

Run: `git add src/RjClicker.App/Infrastructure src/RjClicker.App/Core/Sessions tests/RjClicker.Integration.Tests`
Run: `git commit -m "feat(infra): add win32 delivery hotkeys and background warnings"`

Expected: one commit created

### Task 9: Wire dependency injection, persistence, and final UX behaviors

**Files:**
- Create: `src/RjClicker.App/Composition/ServiceRegistration.cs`
- Create: `src/RjClicker.App/Infrastructure/Settings/ISettingsStore.cs`
- Create: `src/RjClicker.App/Infrastructure/Settings/JsonSettingsStore.cs`
- Modify: `src/RjClicker.App/App.xaml.cs`
- Modify: `src/RjClicker.App/MainWindow.xaml.cs`

- [ ] **Step 1: Write failing integration test for settings persistence**

```csharp
using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.Settings;
using Xunit;

namespace RjClicker.Integration.Tests;

public sealed class SettingsStoreTests
{
    [Fact]
    public async Task SaveThenLoad_ShouldRoundTripRuntimeConfig()
    {
        var path = Path.GetTempFileName();
        var store = new JsonSettingsStore(path);
        var config = RuntimeConfigFactory.Create(totalIntervalMilliseconds: 777);

        await store.SaveAsync(config, CancellationToken.None);
        var loaded = await store.LoadAsync(CancellationToken.None);

        loaded.TotalIntervalMilliseconds.Should().Be(777);
    }
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~SettingsStoreTests -v minimal`
Expected: FAIL with missing settings store

- [ ] **Step 3: Implement settings store and DI registration**

```csharp
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Settings;

public interface ISettingsStore
{
    Task SaveAsync(RuntimeConfig config, CancellationToken cancellationToken);
    Task<RuntimeConfig> LoadAsync(CancellationToken cancellationToken);
}
```

```csharp
using System.Text.Json;
using RjClicker.App.Core.Models;

namespace RjClicker.App.Infrastructure.Settings;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly string _filePath;

    public JsonSettingsStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task SaveAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(config);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    public async Task<RuntimeConfig> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new RuntimeConfig(
                MouseButton.Left,
                PressType.Single,
                100,
                ClickMode.Simultaneous,
                DeliveryMode.Foreground,
                useCounter: false,
                maxClicks: null,
                new[] { PointTarget.Absolute(100, 100) });
        }

        var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
        return JsonSerializer.Deserialize<RuntimeConfig>(json)!;
    }
}
```

```csharp
using Microsoft.Extensions.DependencyInjection;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.Input;
using RjClicker.App.Infrastructure.Settings;
using RjClicker.App.Infrastructure.Windows;
using RjClicker.App.Presentation.ViewModels;

namespace RjClicker.App.Composition;

public static class ServiceRegistration
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IForegroundClickService, Win32ForegroundClickService>();
        services.AddSingleton<IBackgroundClickService, Win32BackgroundClickService>();
        services.AddSingleton<IGlobalHotkeyService, Win32GlobalHotkeyService>();
        services.AddSingleton<IWindowBindingService, WindowBindingService>();
        services.AddSingleton<IClickScheduler, ClickScheduler>();
        services.AddSingleton<IClickDispatcher, ClickDispatcher>();
        services.AddSingleton<ClickSessionController>();
        services.AddSingleton<ISettingsStore>(_ => new JsonSettingsStore("rjclicker.settings.json"));
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
```

- [ ] **Step 4: Wire app startup and main window data context**

```csharp
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RjClicker.App.Composition;
using RjClicker.App.Presentation.ViewModels;

namespace RjClicker.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = ServiceRegistration.Build();
        var vm = services.GetRequiredService<MainViewModel>();

        var window = new MainWindow
        {
            DataContext = vm
        };

        window.Show();
    }
}
```

- [ ] **Step 5: Run tests and smoke-run app**

Run: `dotnet test src/RjClicker.sln -v minimal`
Expected: PASS

Run: `dotnet run --project src/RjClicker.App/RjClicker.App.csproj`
Expected: app launches with configured controls

- [ ] **Step 6: Commit**

Run: `git add src/RjClicker.App tests/RjClicker.Integration.Tests`
Run: `git commit -m "feat(app): wire di settings and startup composition"`

Expected: one commit created

### Task 10: Verification and release-readiness pass

**Files:**
- Create: `docs/superpowers/specs/windows-wpf-autoclick-manual-test-checklist.md`

- [ ] **Step 1: Run full verification commands**

Run: `dotnet test src/RjClicker.sln -v minimal`
Expected: PASS all tests

Run: `dotnet build src/RjClicker.sln -c Release -v minimal`
Expected: BUILD SUCCEEDED

- [ ] **Step 2: Execute manual test checklist**

Use this checklist:
- Start/stop hotkeys work while unfocused
- Segmented interval and total ms remain synchronized
- Simultaneous mode clicks all points in one cycle
- Sequence mode clicks points in expected order
- Window-relative points remain correct when window moves
- Background mode shows warning when target app rejects messages
- Keep on top and hide/show behavior works correctly

Expected: all checklist items pass

- [ ] **Step 3: Document verification outcome**

Create `docs/superpowers/specs/windows-wpf-autoclick-manual-test-checklist.md` with:
- Test date/time
- Environment details
- Pass/fail outcome for each checklist item in Step 2
- Any noted caveats for background mode behavior

- [ ] **Step 4: Commit**

Run: `git add docs/superpowers/specs/windows-wpf-autoclick-manual-test-checklist.md`
Run: `git commit -m "test: record verification checklist results"`

Expected: one commit created

---

## Self-Review

### 1. Spec coverage
- Screenshot-parity settings controls: covered in Task 7 and Task 9
- Multi-point clicking with simultaneous and sequence modes: covered in Task 4 and Task 5
- Global hotkeys: covered in Task 6 and Task 8
- Interval segmented controls plus total ms input: covered in Task 2 and Task 7
- Record points and window-relative targeting: covered in Task 6 and Task 7
- Background inactive-window best-effort mode with warning: covered in Task 5 and Task 8
- Single configuration persistence: covered in Task 9
- Testing strategy and manual validation: covered in Task 1 to Task 10

No uncovered requirements found.

### 2. Placeholder scan
- No TBD/TODO markers found
- No vague "implement later" phrasing remains

### 3. Type consistency
- `RuntimeConfig`, `PointTarget`, `ClickScheduler`, and `ClickSessionController` names are consistent across tasks.
- `DeliveryMode.Background` warning behavior is consistently represented via `BackgroundClickResult` and `DispatchResult`.

No type mismatches found.
