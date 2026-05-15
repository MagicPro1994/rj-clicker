# Windows WPF AutoClick Manual Test Checklist

Date: 2026-05-15
Purpose: Manual verification checklist for the WPF AutoClick release build

## Preconditions

- [ ] Confirm a Release build exists at `src/RjClicker.App/bin/Release/net8.0-windows/`
- [ ] Launch the Release build instead of a Debug binary
- [ ] Start with a clean app session and note the default state before changing settings

## Click Modes

- [ ] Select `Left` button and verify clicks are delivered as left-clicks only
- [ ] Select `Right` button and verify clicks are delivered as right-clicks only
- [ ] Select `Both` buttons and verify both button actions are delivered for each cycle
- [ ] Select `Single` press type and verify one press is emitted per configured button
- [ ] Select `Double` press type and verify a double press is emitted per configured button
- [ ] Verify each button mode works with both `Single` and `Double` press types

## Delivery Modes

- [ ] Start a session in `Foreground` delivery mode and verify clicks target the active foreground window
- [ ] Switch to `Background` delivery mode and verify clicks are attempted against the selected background target
- [ ] Verify switching between `Foreground` and `Background` updates the UI state without requiring an app restart
- [ ] Verify the app surfaces any best-effort warning or status messaging for background delivery failures

## Interval Timing

- [ ] Enter an interval using the segmented parts fields (hours, minutes, seconds, milliseconds) and verify the total milliseconds field updates immediately
- [ ] Enter a total interval directly in the milliseconds field and verify the segmented parts fields update immediately
- [ ] Verify bidirectional sync remains correct after several edits in both directions
- [ ] Verify zero, small, and larger interval values are accepted or rejected according to validation rules
- [ ] Verify starting a session uses the currently displayed interval value, not a stale previous value

## Counter

- [ ] Disable the counter and verify the session continues until stopped manually
- [ ] Enable the counter with a small maximum click count and verify the session stops at the expected count
- [ ] Verify the displayed click count increments correctly while running
- [ ] Verify the session stops exactly at the configured max clicks without an extra click
- [ ] Verify counter settings can be changed while idle and persist for the next run

## Point Targets

- [ ] Add a new point target manually and verify it appears in the points list
- [ ] Remove a point target and verify it no longer appears or participates in clicks
- [ ] Reorder point targets upward and verify the list order updates correctly
- [ ] Reorder point targets downward and verify the list order updates correctly
- [ ] Clear all point targets and verify the list becomes empty
- [ ] Create an `Absolute` point target and verify clicks land at the expected screen coordinates
- [ ] Create a `WindowRelative` point target and verify clicks land relative to the target window
- [ ] Configure multiple points in simultaneous mode and verify all points are clicked in one cycle
- [ ] Configure multiple points in sequence mode and verify points are clicked in list order over the cycle

## Hotkeys

- [ ] Verify `F12` starts a session by default
- [ ] Verify `F12` stops a running session by default
- [ ] Verify `R` starts point-recording mode by default
- [ ] Override the start/stop hotkey with a custom key combination and verify the new combo works
- [ ] Override the record hotkey with a custom key combination and verify the new combo works
- [ ] Verify custom hotkeys still work when the app window is not focused
- [ ] Verify invalid or conflicting hotkey combinations are rejected with clear feedback

## Window Capture

- [ ] Start point recording mode and verify the mouse hook captures the next intended point
- [ ] Verify captured coordinates match the clicked position on screen
- [ ] Verify repeated recordings append additional points rather than overwriting unrelated entries
- [ ] Verify recording a window-relative point captures coordinates relative to the selected target window

## Window Options

- [ ] Enable `Keep on Top` and verify the app stays above other normal windows
- [ ] Disable `Keep on Top` and verify normal window z-order behavior returns
- [ ] Enable `Smart Click` and verify the related click behavior/status changes as intended
- [ ] Enable `Freeze Pointer` and verify the mouse pointer remains fixed as intended during clicking
- [ ] Disable each option again and verify the option state and runtime behavior revert correctly

## Hide and Show

- [ ] Hide the window using the available hide action and verify the app remains running
- [ ] Restore the window via tray interaction or hotkey and verify the full UI state is preserved
- [ ] Verify a hidden window can still be controlled with the configured start/stop hotkey

## Settings Persistence

- [ ] Configure representative settings across click mode, delivery mode, interval, counter, hotkeys, and points
- [ ] Close the app normally
- [ ] Reopen the app and verify all previously changed settings are restored
- [ ] Verify the full points list is restored with order, coordinates, and target types intact
- [ ] Verify persistence also restores toggle options such as `Keep on Top`, `Smart Click`, and `Freeze Pointer`

## Session Control

- [ ] Start a session with the Start button and verify the app enters the running state
- [ ] Stop the session with the Stop button and verify the app returns to the idle state
- [ ] Start a session with the configured hotkey and verify the app enters the running state
- [ ] Stop the session with the configured hotkey and verify the app returns to the idle state
- [ ] Verify the `IsRunning` state is reflected correctly in button enabled state, status text, or other runtime indicators
- [ ] Verify repeated start and stop cycles do not leave the app in a stuck or inconsistent state

## Release Build Validation

- [ ] Launch the Release build and verify the app starts without debugger attachment or debug-only prompts
- [ ] Verify the app functions normally in Release configuration across a short smoke test
- [ ] Confirm no obvious debug artifacts are visible, such as debug banners, placeholder text, or diagnostic popups
- [ ] Confirm the Release binary path and version under test are recorded for the validation session

## Sign-off Notes

- [ ] Record tester name, date, and environment details
- [ ] Record any failures, warnings, or behavior gaps discovered during manual testing
- [ ] Record whether the Release build is acceptable for distribution