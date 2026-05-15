# Portable Settings And Logging Design

Date: 2026-05-15
Status: Approved for planning
Platform: Windows desktop only
Scope: Portable configuration and portable error logging for the existing WPF app

## 1. Goals

Adjust the existing app so it behaves like a portable desktop utility:
- Save configuration next to the running executable instead of under `%APPDATA%`
- Write a portable log file next to the running executable
- Capture both handled operational failures and unexpected app-level exceptions in the same log
- Keep the change localized, testable, and dependency-light

## 2. Scope

### In scope
- Change the default settings file path to `Path.Combine(AppContext.BaseDirectory, "settings.json")`
- Add a file logger that writes to `Path.Combine(AppContext.BaseDirectory, "rjclicker.log")`
- Log settings save failures, settings load failures, and settings deserialization failures
- Log unexpected exceptions from WPF dispatcher, AppDomain unhandled exceptions, and unobserved task exceptions
- Preserve explicit constructor-based path overrides for tests

### Out of scope
- Log rotation, retention policies, or structured JSON logging
- User-facing log viewer UI
- Silent fallback to `%APPDATA%` when the executable directory is not writable
- Third-party logging frameworks

## 3. Non-Functional Requirements

- Behavior must stay portable: moving the app folder also moves settings and logs
- Logging must be best effort and must never crash the app during error handling
- The implementation must remain deterministic and unit-testable
- The design must avoid coupling `App.xaml.cs` or `JsonSettingsStore` to raw file-writing details

## 4. Architecture

Use a small logging abstraction plus the existing settings store abstraction.

### 4.1 Settings persistence
- `JsonSettingsStore` remains the single owner of config serialization and deserialization
- The default path changes from `%APPDATA%` to the executable directory via `AppContext.BaseDirectory`
- The constructor overload that accepts an explicit path remains unchanged for tests and future overrides

### 4.2 File logging
- Add a focused app logger service interface with one responsibility: append timestamped text entries to a file
- Default log path is `Path.Combine(AppContext.BaseDirectory, "rjclicker.log")`
- Logger API should support message-only entries and message-plus-exception entries
- The logger must catch and suppress its own I/O failures so logging does not trigger recursive failures

### 4.3 App-level exception capture
- `App.xaml.cs` subscribes to:
  - `DispatcherUnhandledException`
  - `AppDomain.CurrentDomain.UnhandledException`
  - `TaskScheduler.UnobservedTaskException`
- Each handler routes exception data into the logger
- Exception handling responsibilities stay in `App.xaml.cs`; formatting and file writing stay in the logger

## 5. Data Flow

### 5.1 Settings load flow
1. App starts
2. `App.xaml.cs` resolves `ISettingsStore`
3. `JsonSettingsStore.LoadAsync()` reads `settings.json` from `AppContext.BaseDirectory`
4. If reading or deserialization fails, the store logs the issue and returns `null`
5. App continues with default in-memory settings

### 5.2 Settings save flow
1. App exits
2. `App.xaml.cs` extracts settings from the live `MainViewModel`
3. `JsonSettingsStore.SaveAsync()` writes `settings.json` to `AppContext.BaseDirectory`
4. If writing fails, the store logs the issue and app shutdown continues

### 5.3 Unexpected error flow
1. An unhandled exception reaches a registered app-level exception event
2. `App.xaml.cs` forwards the exception to the logger
3. Logger appends a timestamped entry to `rjclicker.log`
4. Existing WPF exception semantics remain unchanged unless current code already handles them differently

## 6. Logging Format

Plain-text log entries are sufficient for this app.
Each entry should include:
- UTC timestamp in an unambiguous format
- severity label such as `ERROR`
- source area, for example `JsonSettingsStore` or `App`
- human-readable message
- exception type, message, and stack trace when present

Example shape:

```text
2026-05-15 14:22:11.123 ERROR [JsonSettingsStore] Failed to save settings
System.IO.IOException: Access denied
   at ...
```

## 7. Error Handling Strategy

Handled operational failures:
- Settings read/write/deserialization failures are logged and converted to non-fatal behavior
- The app continues with defaults on load failure and continues shutdown on save failure

Unexpected failures:
- App-level exception hooks log the error before the process exits or WPF handles the exception path
- Logging is best effort only; if the logger cannot write, it suppresses that secondary failure

Predictability rule:
- Do not silently redirect logs or settings to another directory
- If the executable folder is not writable, the expected side effect is absence of updated files, not hidden fallback behavior

## 8. Testing Strategy

Unit tests:
- Verify the default settings path resolves to `AppContext.BaseDirectory/settings.json`
- Verify the default log path resolves to `AppContext.BaseDirectory/rjclicker.log`
- Verify logger appends entries to the configured file path
- Verify logger suppresses file I/O exceptions

Integration tests:
- Verify settings round-trip still works when using the default store path behavior or an injected path
- Verify `JsonSettingsStore` logs save failures
- Verify `JsonSettingsStore` logs load and deserialization failures
- Verify app-level exception forwarding calls the logger through a testable helper or extracted handler component

Manual verification:
- Run from `bin/Debug` and confirm `settings.json` and `rjclicker.log` appear there
- Publish the app and confirm both files appear next to the published executable
- Trigger a handled settings failure and verify it is written to the log
- Trigger an unexpected exception in a controlled debug scenario and verify it is written to the log

## 9. Risks And Mitigations

- Running from a protected directory may prevent writing portable files
  - Mitigation: accept best-effort behavior and log when possible; do not introduce hidden fallback paths
- App-level exception hooks can be difficult to test directly
  - Mitigation: isolate exception-to-log forwarding behind a small helper that can be exercised in tests
- Logging from multiple error sources could interleave writes
  - Mitigation: keep logger append behavior simple and synchronized within the logger implementation

## 10. Delivery Boundary

This document defines design only.
Implementation planning is the next phase and will be produced separately.
