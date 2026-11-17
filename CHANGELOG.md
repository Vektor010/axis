# Changelog

All notable changes to AXIS will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
