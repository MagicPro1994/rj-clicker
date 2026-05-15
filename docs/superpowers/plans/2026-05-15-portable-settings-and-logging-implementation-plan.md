# Portable Settings And Logging Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the app portable by storing `settings.json` and `rjclicker.log` next to the running executable, while logging both settings persistence failures and unexpected app-level exceptions.

**Architecture:** Keep persistence in `JsonSettingsStore`, add a minimal file-based logger abstraction for append-only error logging, and route WPF/app-domain/task exceptions through a small app-level forwarding helper. The change stays localized to existing startup/composition code and focused service files so behavior is testable without broad UI changes.

**Tech Stack:** .NET 8, C#, WPF, xUnit, FluentAssertions, Microsoft.Extensions.DependencyInjection, System.Text.Json

---

## Scope Check

The approved spec is one coherent change set: portable settings plus portable error logging. One implementation plan is appropriate.

## File Structure

### Create
- `src/RjClicker.App/Services/IAppLogger.cs`
- `src/RjClicker.App/Services/FileAppLogger.cs`
- `src/RjClicker.App/Services/AppExceptionLogger.cs`
- `tests/RjClicker.Core.Tests/Services/FileAppLoggerTests.cs`
- `tests/RjClicker.Core.Tests/Services/AppExceptionLoggerTests.cs`

### Modify
- `src/RjClicker.App/Services/JsonSettingsStore.cs`
- `src/RjClicker.App/ServiceRegistration.cs`
- `src/RjClicker.App/App.xaml.cs`
- `tests/RjClicker.Integration.Tests/DependencyInjectionTests.cs`

---

### Task 1: Add portable file logger with TDD

**Files:**
- Create: `src/RjClicker.App/Services/IAppLogger.cs`
- Create: `src/RjClicker.App/Services/FileAppLogger.cs`
- Test: `tests/RjClicker.Core.Tests/Services/FileAppLoggerTests.cs`

- [ ] **Step 1: Write the failing logger tests**

```csharp
using FluentAssertions;
using RjClicker.App.Services;
using System.IO;

namespace RjClicker.Core.Tests.Services;

public sealed class FileAppLoggerTests
{
    [Fact]
    public async Task LogErrorAsync_ShouldAppendMessageAndExceptionToConfiguredFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"rjclicker-log-{Guid.NewGuid()}.log");
        var logger = new FileAppLogger(path);
        var exception = new InvalidOperationException("boom");

        try
        {
            await logger.LogErrorAsync("JsonSettingsStore", "Failed to save settings", exception);

            var text = await File.ReadAllTextAsync(path);
            text.Should().Contain("ERROR [JsonSettingsStore] Failed to save settings");
            text.Should().Contain("InvalidOperationException");
            text.Should().Contain("boom");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task DefaultConstructor_ShouldWriteToBaseDirectoryLogFile()
    {
        var expectedPath = Path.Combine(AppContext.BaseDirectory, "rjclicker.log");
        var logger = new FileAppLogger();

        await logger.LogErrorAsync("App", "probe", null);

        File.Exists(expectedPath).Should().BeTrue();
        File.ReadAllText(expectedPath).Should().Contain("ERROR [App] probe");

        File.Delete(expectedPath);
    }

    [Fact]
    public async Task LogErrorAsync_ShouldSuppressIoFailures()
    {
        var logger = new FileAppLogger("Z:\\path-that-should-not-exist\\rjclicker.log");

        var action = async () => await logger.LogErrorAsync("App", "probe", new IOException("nope"));

        await action.Should().NotThrowAsync();
    }
}
```

- [ ] **Step 2: Run the focused logger tests and confirm they fail**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~FileAppLoggerTests -v minimal`
Expected: FAIL with missing `FileAppLogger`

- [ ] **Step 3: Implement the minimal logger abstraction and file logger**

```csharp
namespace RjClicker.App.Services;

public interface IAppLogger
{
    Task LogErrorAsync(string source, string message, Exception? exception = null);
}
```

```csharp
using System.Globalization;
using System.Text;

namespace RjClicker.App.Services;

public sealed class FileAppLogger : IAppLogger
{
    private static readonly string DefaultLogPath = Path.Combine(AppContext.BaseDirectory, "rjclicker.log");
    private readonly string _logPath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public FileAppLogger() : this(DefaultLogPath)
    {
    }

    public FileAppLogger(string logPath)
    {
        _logPath = logPath ?? throw new ArgumentNullException(nameof(logPath));
    }

    public async Task LogErrorAsync(string source, string message, Exception? exception = null)
    {
        try
        {
            var directory = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var builder = new StringBuilder();
            builder.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
            builder.Append(" ERROR [");
            builder.Append(source);
            builder.Append("] ");
            builder.AppendLine(message);

            if (exception is not null)
            {
                builder.AppendLine(exception.ToString());
            }

            await _writeLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await File.AppendAllTextAsync(_logPath, builder.ToString()).ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }
        catch
        {
            // Logging is best-effort only.
        }
    }
}
```

- [ ] **Step 4: Run the focused logger tests and confirm they pass**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~FileAppLoggerTests -v minimal`
Expected: PASS

- [ ] **Step 5: Commit the logger slice**

Run: `git add src/RjClicker.App/Services/IAppLogger.cs src/RjClicker.App/Services/FileAppLogger.cs tests/RjClicker.Core.Tests/Services/FileAppLoggerTests.cs`
Run: `git commit -m "feat(logging): add portable file logger"`
Expected: one commit created

### Task 2: Move settings to the executable directory and log store failures with TDD

**Files:**
- Modify: `src/RjClicker.App/Services/JsonSettingsStore.cs`
- Modify: `tests/RjClicker.Integration.Tests/DependencyInjectionTests.cs`

- [ ] **Step 1: Write failing portable-settings and logging tests**

Add these tests to `tests/RjClicker.Integration.Tests/DependencyInjectionTests.cs`:

```csharp
[Fact]
public async Task SettingsStore_DefaultConstructor_ShouldUseBaseDirectorySettingsFile()
{
    var path = Path.Combine(AppContext.BaseDirectory, "settings.json");
    File.Delete(path);
    var store = new JsonSettingsStore();

    await store.SaveAsync(new AppSettings { MouseButton = "Right" });

    File.Exists(path).Should().BeTrue();
    (await File.ReadAllTextAsync(path)).Should().Contain("Right");

    File.Delete(path);
}

[Fact]
public async Task SettingsStore_ShouldLogDeserializeFailures()
{
    var directory = Path.Combine(Path.GetTempPath(), $"rjclicker-settings-{Guid.NewGuid()}");
    Directory.CreateDirectory(directory);
    var settingsPath = Path.Combine(directory, "settings.json");
    var logPath = Path.Combine(directory, "rjclicker.log");
    await File.WriteAllTextAsync(settingsPath, "not valid json");
    var logger = new FileAppLogger(logPath);
    var store = new JsonSettingsStore(settingsPath, logger);

    var result = await store.LoadAsync();

    result.Should().BeNull();
    (await File.ReadAllTextAsync(logPath)).Should().Contain("Failed to deserialize settings");

    Directory.Delete(directory, recursive: true);
}
```

- [ ] **Step 2: Run the focused settings tests and confirm they fail**

Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~SettingsStore_DefaultConstructor_ShouldUseBaseDirectorySettingsFile|FullyQualifiedName~SettingsStore_ShouldLogDeserializeFailures -v minimal`
Expected: FAIL because the default path still points to `%APPDATA%` and `JsonSettingsStore` does not accept a logger

- [ ] **Step 3: Implement the portable default path and logger-backed settings store**

Update `src/RjClicker.App/Services/JsonSettingsStore.cs` to this shape:

```csharp
using RjClicker.App.Models;
using System.Text.Json;

namespace RjClicker.App.Services;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly string DefaultSettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _settingsPath;
    private readonly IAppLogger _logger;

    public JsonSettingsStore() : this(DefaultSettingsPath, new FileAppLogger())
    {
    }

    public JsonSettingsStore(string path) : this(path, new FileAppLogger())
    {
    }

    public JsonSettingsStore(string path, IAppLogger logger)
    {
        _settingsPath = path ?? throw new ArgumentNullException(nameof(path));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveAsync(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, SerializerOptions);
            await File.WriteAllTextAsync(_settingsPath, json).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to save settings", ex).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to save settings", ex).ConfigureAwait(false);
        }
    }

    public async Task<AppSettings?> LoadAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch (IOException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to load settings", ex).ConfigureAwait(false);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to load settings", ex).ConfigureAwait(false);
            return null;
        }
        catch (JsonException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to deserialize settings", ex).ConfigureAwait(false);
            return null;
        }
    }
}
```

- [ ] **Step 4: Run the focused settings tests and the existing round-trip tests**

Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~SettingsStore -v minimal`
Expected: PASS

- [ ] **Step 5: Commit the portable settings slice**

Run: `git add src/RjClicker.App/Services/JsonSettingsStore.cs tests/RjClicker.Integration.Tests/DependencyInjectionTests.cs`
Run: `git commit -m "fix(settings): store config beside the executable"`
Expected: one commit created

### Task 3: Register the logger and forward app-level exceptions with TDD

**Files:**
- Create: `src/RjClicker.App/Services/AppExceptionLogger.cs`
- Modify: `src/RjClicker.App/ServiceRegistration.cs`
- Modify: `src/RjClicker.App/App.xaml.cs`
- Test: `tests/RjClicker.Core.Tests/Services/AppExceptionLoggerTests.cs`
- Modify: `tests/RjClicker.Integration.Tests/DependencyInjectionTests.cs`

- [ ] **Step 1: Write the failing exception-forwarding and DI tests**

Create `tests/RjClicker.Core.Tests/Services/AppExceptionLoggerTests.cs`:

```csharp
using FluentAssertions;
using RjClicker.App.Services;

namespace RjClicker.Core.Tests.Services;

public sealed class AppExceptionLoggerTests
{
    [Fact]
    public async Task LogDispatcherUnhandledExceptionAsync_ShouldWriteThroughLogger()
    {
        var logger = new SpyAppLogger();
        var service = new AppExceptionLogger(logger);
        var exception = new InvalidOperationException("dispatcher failed");

        await service.LogDispatcherUnhandledExceptionAsync(exception);

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Source.Should().Be("App");
        logger.Entries[0].Message.Should().Be("Unhandled dispatcher exception");
        logger.Entries[0].Exception.Should().Be(exception);
    }

    private sealed class SpyAppLogger : IAppLogger
    {
        public List<(string Source, string Message, Exception? Exception)> Entries { get; } = [];

        public Task LogErrorAsync(string source, string message, Exception? exception = null)
        {
            Entries.Add((source, message, exception));
            return Task.CompletedTask;
        }
    }
}
```

Add this DI assertion to `tests/RjClicker.Integration.Tests/DependencyInjectionTests.cs`:

```csharp
_serviceProvider.GetRequiredService<IAppLogger>().Should().NotBeNull();
_serviceProvider.GetRequiredService<AppExceptionLogger>().Should().NotBeNull();
```

- [ ] **Step 2: Run the focused exception and DI tests and confirm they fail**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~AppExceptionLoggerTests -v minimal`
Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~ServiceRegistration_ShouldRegisterAllRequiredServices -v minimal`
Expected: FAIL with missing types/registrations

- [ ] **Step 3: Implement exception forwarding and wire it into startup composition**

Create `src/RjClicker.App/Services/AppExceptionLogger.cs`:

```csharp
namespace RjClicker.App.Services;

public sealed class AppExceptionLogger
{
    private readonly IAppLogger _logger;

    public AppExceptionLogger(IAppLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task LogDispatcherUnhandledExceptionAsync(Exception exception)
    {
        return _logger.LogErrorAsync("App", "Unhandled dispatcher exception", exception);
    }

    public Task LogUnhandledExceptionAsync(Exception exception)
    {
        return _logger.LogErrorAsync("App", "Unhandled app domain exception", exception);
    }

    public Task LogUnobservedTaskExceptionAsync(Exception exception)
    {
        return _logger.LogErrorAsync("App", "Unobserved task exception", exception);
    }
}
```

Update `src/RjClicker.App/ServiceRegistration.cs` registrations:

```csharp
services.AddSingleton<IAppLogger, FileAppLogger>();
services.AddSingleton<AppExceptionLogger>();
services.AddSingleton<ISettingsStore>(serviceProvider =>
    new JsonSettingsStore(
        Path.Combine(AppContext.BaseDirectory, "settings.json"),
        serviceProvider.GetRequiredService<IAppLogger>()));
```

Update `src/RjClicker.App/App.xaml.cs` startup and shutdown logic to subscribe and unsubscribe handlers:

```csharp
private AppExceptionLogger? _appExceptionLogger;

protected override async void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    _serviceProvider = ServiceRegistration.BuildServiceProvider();
    _appExceptionLogger = _serviceProvider.GetRequiredService<AppExceptionLogger>();
    DispatcherUnhandledException += OnDispatcherUnhandledException;
    AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

    // existing startup logic continues here
}

protected override async void OnExit(ExitEventArgs e)
{
    DispatcherUnhandledException -= OnDispatcherUnhandledException;
    AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;
    TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

    // existing save/dispose logic continues here
}

private async void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
{
    if (_appExceptionLogger is not null)
    {
        await _appExceptionLogger.LogDispatcherUnhandledExceptionAsync(e.Exception);
    }
}

private async void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
{
    if (_appExceptionLogger is not null && e.ExceptionObject is Exception exception)
    {
        await _appExceptionLogger.LogUnhandledExceptionAsync(exception);
    }
}

private async void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
{
    if (_appExceptionLogger is not null)
    {
        await _appExceptionLogger.LogUnobservedTaskExceptionAsync(e.Exception);
    }
}
```

- [ ] **Step 4: Run the focused exception and DI tests and confirm they pass**

Run: `dotnet test tests/RjClicker.Core.Tests/RjClicker.Core.Tests.csproj --filter FullyQualifiedName~AppExceptionLoggerTests -v minimal`
Run: `dotnet test tests/RjClicker.Integration.Tests/RjClicker.Integration.Tests.csproj --filter FullyQualifiedName~ServiceRegistration_ShouldRegisterAllRequiredServices -v minimal`
Expected: PASS

- [ ] **Step 5: Commit the exception-forwarding slice**

Run: `git add src/RjClicker.App/Services/AppExceptionLogger.cs src/RjClicker.App/ServiceRegistration.cs src/RjClicker.App/App.xaml.cs tests/RjClicker.Core.Tests/Services/AppExceptionLoggerTests.cs tests/RjClicker.Integration.Tests/DependencyInjectionTests.cs`
Run: `git commit -m "feat(logging): capture app-level exceptions"`
Expected: one commit created

### Task 4: Run final verification and manual portability checks

**Files:**
- Modify: `docs/superpowers/specs/2026-05-15-portable-settings-and-logging-design.md` only if implementation reveals a design correction
- Verify: `src/RjClicker.App/Services/JsonSettingsStore.cs`
- Verify: `src/RjClicker.App/Services/FileAppLogger.cs`
- Verify: `src/RjClicker.App/App.xaml.cs`

- [ ] **Step 1: Run the full automated test suite**

Run: `dotnet test src/RjClicker.sln --configuration Debug --no-restore --logger "console;verbosity=minimal"`
Expected: PASS with all tests green

- [ ] **Step 2: Run a publish build for portable verification**

Run: `dotnet publish src/RjClicker.App/RjClicker.App.csproj --configuration Release --no-restore --self-contained --runtime win-x64 --output publish/self-contained`
Expected: PASS and published files available under `publish/self-contained`

- [ ] **Step 3: Manually verify portable files from debug output**

Check for:
- `src/RjClicker.App/bin/Debug/net8.0-windows/settings.json`
- `src/RjClicker.App/bin/Debug/net8.0-windows/rjclicker.log`

Expected: both files appear after exercising settings save and a controlled logged failure

- [ ] **Step 4: Manually verify portable files from published output**

Check for:
- `publish/self-contained/settings.json`
- `publish/self-contained/rjclicker.log`

Expected: both files appear next to the published executable after exercising the same flows

- [ ] **Step 5: Commit final verification-safe updates if needed**

Run: `git add src tests docs`
Run: `git commit -m "test(logging): verify portable settings and error logging"`
Expected: commit only if verification required code or test adjustments

---

## Self-Review

### Spec coverage
- Portable settings path: covered in Task 2
- Portable log path: covered in Task 1
- Settings save/load/deserialization logging: covered in Task 2
- App-level exception capture: covered in Task 3
- Best-effort logging and no fallback path: covered in Tasks 1 through 3 and verified in Task 4

### Placeholder scan
- No `TODO`, `TBD`, or implied "write tests later" steps remain
- Each code-bearing task includes explicit code or explicit test content
- Each validation step includes concrete commands and expected outcomes

### Type consistency
- Logger abstraction name is consistently `IAppLogger`
- Logger implementation name is consistently `FileAppLogger`
- App-level forwarding helper name is consistently `AppExceptionLogger`
- Settings store constructor plan consistently uses `JsonSettingsStore(string path, IAppLogger logger)`

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-15-portable-settings-and-logging-implementation-plan.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

Which approach?
