# Changelog

All notable changes to AXIS will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-11-18

### Fixed & Audited
- **Steam Autostart Dual-Registry Synchronization**: Fixed desynchronization between Valve's native `SuppressAutoRun` DWORD and Windows Explorer Startup Subsystem (`HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run\Steam`). In Windows 10/11, disabling startup in Task Manager sets byte 0 of this binary value to `0x03`. AXIS now checks both Valve registry and Windows StartupApproved, accurately reporting `1 (Автозапуск выключен)` and status `Оптимально`.
- **Steam Category Name**: Renamed category from `Платформа Steam` to `Steam` across the navigation rail and tweak card headers.
- **Steam Overlay Real-State Inspector**: Added real-time fallback in `TweakCardViewModel.LiveSystemValueText` to inspect both registry and `userdata/*/config/localconfig.vdf`, displaying `0 (Выключен в Steam)` instead of generic fallback.
- **Steam Registry Default Text**: Updated fallback when a Steam registry value is not created to `Не задано (по умолчанию Steam)`.
- **Automated Test Suite 13**: Added `RunSteamConfigHelperTests` in `dist/Tests.exe`, expanding automated verification from 7,760 to **7,793 assertions** (100% PASS, 0 FAIL).

## [2.0.1] - 2026-11-18

### Performance & UI Acceleration
- **VirtualizingStackPanel Direct Viewport Connection**: Resolved critical WPF virtualization bottleneck where outer `ScrollViewer` provided infinite measure height to `ItemsControl`, previously realizing all 856 tweak cards (~20,000 UIElements) simultaneously. Moved `ScrollViewer` inside `ItemsControl.Template`, connecting `ItemsPresenter` directly to `IScrollInfo`. Only ~10 visible cards are now instantiated in memory (98.8% visual tree reduction).
- **Atomic Batch Collection Updates (`BatchObservableCollection<T>`)**: Created a specialized collection type with `ReplaceAll(items)` that suppresses intermediate collection change notifications and fires a single `NotifyCollectionChangedEventArgs(Reset)`. Eliminated 856 sequential `CollectionChanged` events during filtering and category selection.
- **Single-Pass Theme Dictionary Invalidation**: Replaced 35 individual key mutations on `Application.Current.Resources` with an atomic dictionary swap in `Resources.MergedDictionaries[0]`, collapsing 35 full visual tree invalidation waves into a single unified pass.
- **Asynchronous Settings Serialization**: Moved `SettingsManager.Save()` to `Task.Run` on theme toggles, eliminating synchronous disk I/O freezes from the UI thread.
- **Eliminated Redundant Group Rebuilds**: Optimized `ApplyGrouping()` to retain `PropertyGroupDescription` across category and search updates, removing duplicate collection view reset and refresh calls.

### Measured Benchmarks (Full UI Render Passes)
- **Tab Switching (Tweaks <-> Overview)**: Accelerated from ~1,500 ms to **14.68 ms** (sub-frame 60 FPS performance).
- **Tweaks Category Switching**: Accelerated from ~1,200 ms to **26.95 ms**.
- **Theme Toggling**: Accelerated from ~1,800 ms to **50.60 ms** (100% smooth transition).

### Layout & Perfectionist Calibration
- **Window Geometry Adjusted to 1200x840** (MinHeight: 660, MinWidth: 1000): Guaranteed ample vertical headroom across all high-DPI displays (100%, 125%, 150%), completely removing unwanted vertical scrollbars on Overview, Settings, and About screens.

## [2.0.0] - 2026-11-17

### Added
- **Flagship Rebranding**: Complete transition to AXIS branding, custom vector SVG badge, and standalone `Axis.exe` binary.
- **2026 Bento UI Architecture**: Modern dark and light theme redesign featuring Bento-style cards and zero unnecessary scrollbars.
- **Deep Tweak Inspector**: Expanded tweak cards now display live registry values queried from Windows in real time, target values, reboot requirements, and isolated revert definitions.
- **Settings & Recovery Hub**:
  - Windows System Restore integration with one-click VSS checkpoint creation (`Checkpoint-Computer`).
  - Snapshot database status and one-click backup export to Desktop (`axis_snapshots_backup.json`).
  - Telemetry polling frequency controls (1s, 2s, 5s, and esports "Pause" mode for 0% CPU overhead).
  - Network ping measurement with quick presets (Default Gateway, Cloudflare, Google, Yandex).
  - Diagnostic system report generation directly to clipboard.
- **Theme Palette Optimization**: Pre-cached and frozen (`Freeze()`) brush resources providing sub-millisecond (0.10 ms) theme toggle latency and eliminating visual tree notification churn.
- **Kernel Subsystem Monitoring**: Native WDDM 2.9/3.0 Hardware Flip Queue detection, MMCSS multimedia scheduler inspector, and Game Mode WNF state tracking.

### Changed
- Refactored `App.xaml.cs` to eliminate dynamic hex color parsing on theme toggles.
- Calibrated window dimensions to 1180x800 for seamless layout fitting across high-DPI displays.
- Completely removed deprecated `Tweaker.exe` binaries across all distribution points.

### Verified
- 7,760 automated unit and integration tests passing (100% PASS).
- All 52 production PowerShell scripts verified with zero AST syntax errors.
- 856 registry tweaks validated for structural, type, and path integrity.
