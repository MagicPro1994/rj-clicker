# Windows WPF AutoClick App Design

Date: 2026-05-13
Status: Approved for planning
Platform: Windows desktop only
UI baseline: Similar layout and controls to the provided AutoClick screenshot

## 1. Goals

Build a Windows desktop auto clicker with UI and capabilities equivalent to the reference app, plus an additional key feature: clicking multiple places in one run cycle.

Primary goals:
- Provide full feature parity with the screenshot controls and behavior
- Support multi-point clicking with two runtime modes
- Provide reliable global hotkeys and precise timing
- Keep architecture maintainable and testable

## 2. Scope

### In scope for v1
- Settings controls matching screenshot-level feature set:
  - Mouse button selection: left, right
  - Press type: single, double
  - Hotkeys: start/stop hotkey and record hotkey
  - Counter and stop-on-count behavior
  - Interval controls (segmented): hours, minutes, seconds, 1/10s, 1/100s, 1/1000s
  - Total interval input in milliseconds (ms)
  - Smart click, freeze pointer, keep on top, hide/show auto click options
- Point targeting:
  - Record points mode (capture from user clicks)
  - Manual point editing
  - Screen coordinates and window-relative coordinates
- Multi-point clicking modes:
  - Simultaneous mode
  - Rapid sequence mode
- Click delivery modes:
  - Foreground injection mode (default, reliable)
  - Background message mode (best effort for inactive/background windows)
- Single persistent configuration for v1

### Out of scope for v1
- Multiple named profiles (deferred to v2)
- Cross-platform support

## 3. Non-Functional Requirements

- Low-latency input loop suitable for desktop utility use
- Global hotkeys must work while app is unfocused
- Clear runtime status and error messages
- Deterministic, testable core logic separated from UI

## 4. Architecture

Use a layered architecture with clear separation of concerns.

### 4.1 Presentation layer (WPF)
- MainWindow and ViewModels
- Binding for all settings and runtime status
- No direct Win32 calls in UI components

### 4.2 Application layer
- Build and validate RuntimeConfig from UI state
- Session orchestration: start, stop, pause, and teardown
- Translate user intent into runtime commands

### 4.3 Infrastructure layer
- GlobalHotkeyService: register, unregister, dispatch callbacks
- InputInjectionService: foreground click dispatch
- BackgroundMessageClickService: background click dispatch (best effort)
- PointCaptureService: capture target coordinates
- WindowBindingService: resolve target window and map window-relative points
- SettingsStore: persist and load single v1 configuration

### 4.4 Runtime engine
- ClickScheduler for timing loop and cancellation
- ClickDispatcher for per-cycle click delivery
- Mode selection:
  - Simultaneous: dispatch all configured points in one cycle batch
  - Sequence: dispatch configured points in fast configured order per cycle

## 5. Data Model

Core models:
- RuntimeConfig
  - MouseButton
  - PressType
  - Interval (canonical total ms)
  - Counter settings
  - Click mode (simultaneous or sequence)
  - Delivery mode (foreground or background)
  - Hotkey settings
  - Target definitions
- PointTarget
  - TargetType: Absolute or WindowRelative
  - X, Y
  - Optional TargetWindowId
- SessionState
  - Idle, Running, Paused, Error, Stopped

Timing model:
- Canonical interval value is total milliseconds
- Segmented time controls and total-ms field remain synchronized both ways

## 6. UI and Interaction Design

The main window mirrors the reference grouping:
- Settings group
- Counter group
- Auto click hotkey group
- Click interval group
- Smart click and recording group
- Runtime action section

Additional v1 elements:
- Total Interval (ms) input field
- Point list panel (add, edit, remove, reorder)
- Click mode selector (simultaneous or sequence)
- Delivery mode selector (foreground or background best effort)
- Explicit runtime status area for warnings and errors

## 7. Data Flow

Start flow:
1. User edits settings
2. User starts via button or start hotkey
3. RuntimeConfigBuilder validates all fields
4. SessionController initializes services and scheduler
5. Scheduler tick resolves targets and dispatches clicks
6. Counter and stop conditions update
7. Session stops on user command, stop hotkey, counter completion, or fatal error

Record points flow:
1. User triggers record mode/hotkey
2. PointCaptureService captures click locations
3. New points are appended to target list
4. User may convert/edit point type (absolute or window-relative)

Background mode flow:
1. Session starts with selected target window
2. BackgroundMessageClickService sends window messages per target point
3. If target rejects/ignores messages, app reports warning and suggests foreground mode

## 8. Error Handling and Safety

Validation and startup:
- Reject invalid interval values and hotkey collisions
- Require at least one valid target point before start
- Require valid target window when window-relative mode is selected

Runtime safety:
- Only one active session at a time
- Always-available stop mechanism
- Graceful cancellation and deterministic teardown
- Stop session when bound target window handle becomes invalid

User messaging:
- Show clear, actionable errors in status panel
- Label background mode clearly as best effort

## 9. Testing Strategy

Unit tests:
- Interval conversion and synchronization (segmented to total ms and reverse)
- RuntimeConfig validation rules
- Session state transitions
- Counter decrement and stop behavior
- Mode branching (simultaneous vs sequence)

Integration tests (with test doubles/mocks):
- Start/stop lifecycle with mocked hotkey and click services
- Target resolution behavior as windows move/resize
- Background mode warning behavior when delivery fails

Manual verification checklist:
- Hotkeys work while app not focused
- UI parity controls behave as expected
- Multi-point simultaneous and sequence modes hit expected targets
- Window-relative points remain accurate after window movement
- Foreground and background delivery mode behavior is correctly surfaced

## 10. Risks and Mitigations

- Background clicking inconsistency across target apps
  - Mitigation: foreground mode default, explicit best-effort labeling, runtime warning
- High-frequency timing jitter at very small intervals
  - Mitigation: dedicated scheduler thread and high-resolution timing strategy
- Hotkey conflicts with other software
  - Mitigation: startup validation and clear conflict feedback

## 11. Delivery Plan Boundary

This document defines design only.
Implementation planning is the next phase and will be produced separately via writing-plans skill.
