# Changelog

All notable changes to AXIS will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.1.0] - 2026-11-26

### Clear Contextual Batch Buttons, OS Protection Guard & Smarter Recommended Preset
- **Unambiguous Batch Action Buttons**:
  - **«Применить рекомендуемые (N)»**: clearly indicates the curated safe Noverse/esports preset (`N` unapplied of `216`).
  - **«Применить все (M)» / «Применить раздел (M)» / «Применить выбранные (M)»**: dynamically follows user intent—applies explicitly checked cards when any are selected, or all remaining eligible tweaks in the currently open tab/category (`Все`, `Внимание`, `Сеть`, `Steam`, etc.) when no checkboxes are ticked.
  - **«Откатить все (K)»**: shows active snapshot-backed rollback count cleanly.
- **High-Risk & OS Protection Guard (`AxisDialogWindow`)**: When applying a whole tab/section containing `high`-risk or security-sensitive tweaks (Defender, Firewall, UAC, VBS/HVCI, SmartScreen, BitLocker, etc.), `AxisDialogWindow.ShowBatchConfirmWithGuard` defaults to applying safe tweaks first while giving the user an explicit opt-in checkbox to include OS-protection tweaks too.
- **Curated `216` Recommended Preset (`data/tweaks.json` + `tweakengine.cs`)**: Removed 24 surprising/controversial items (`DisablePagingExecutive`, `NtfsDisableEncryption`, full toast/notification shutdowns, legacy boot menu, sleep/hibernate removal, lockscreen removal, root cert / PowerShell security relaxations) from `recommended`, removed stale NVIDIA batch block for safe NVIDIA tweaks, and promoted 7 proven NVIDIA/Steam optimizations (HAGS, NV telemetry/tasks, Noverse minimal NV profile, Steam smooth scroll/GPU webviews/gameplay downloads).

## [3.0.0] - 2026-11-26

### Global Audit, Unified Dark-Blue Design System, Real Telemetry & Batch Engine
- **Unified Black-Graphite & Blue UI + Custom Modal Dialogs (`AxisDialogWindow`)**: Rebuilt palette in obsidian/graphite with Cobalt Blue (`#3B82F6` / `#2563EB`) accents across all views and replaced all 12 legacy Win32 `MessageBox.Show` calls with custom WPF dark-blue confirmation, preview, result report, error, and Tweak Inspector («Подробнее») dialogs.
- **Tweak Cards & Recommended Catalog Audit (`233` low-risk recommended of `897`)**: Excluded `security-disable-vbs` and `security-disable-hvci` from automatic `recommended` preset (moving them to `gaming`), enriched all 25 short descriptions, added dedicated **«Рекомендуемые»** sidebar category and toolbar segment filter, batch checkboxes, and clean card actions (**«Применить»**, **«Откатить»**, **«Подробнее»**).
- **Real Hardware Dashboard Telemetry (`SystemInfoService` + NVML `nvmlDeviceGetUtilizationRates`)**: Removed synthetic chart startup series, added live GPU Load (%), ACPI CPU Temp (honest `"Датчик недоступен"` when absent), Disk Activity (%) & Space, Network RX/TX throughput, OS Uptime (`GetTickCount64`), active Process Count (all handles strictly disposed), and last-updated timestamp.
- **Centralized Mass Operations**: Implemented **«Применить выбранные»**, **«Применить все»** (safe recommended), and **«Откатить все»** (snapshot-backed only) with conflict detection, OS build compatibility checks, admin rights warning, itemized previews, and structured reports.
- **Memory & Thread Leak Fixes**: Implemented `IDisposable` across `MainViewModel` and `SystemInfoService`, added `Interlocked` overlap guard in `PollLiveMetrics()`, eliminated startup `NvmlTemperatureSource` leak, stopped stopping/restarting hundreds of `RegistryWatcher` handles on every search keystroke.
- **Verification**: Suite 16 added (`8,159` assertions PASS, 0 FAIL, `dist/Axis.exe` 1.38 MB).

## [2.2.0] - 2026-11-26

### Complete Noverse Docs & App Guides 1:1 Sync + Full IPv6 Disable
- **Complete IPv6 Disable (`0xFF` + `ms_tcpip6` unbind)**: Upgraded `network-disable-ipv6-components` to apply `DisabledComponents = 255` (`0xFF`) AND automatically unbind `ms_tcpip6` across network adapters (`Disable-NetAdapterBinding`) with snapshot revert to prevent real IP leaks behind VPNs.
- **22 New Curated Noverse `win-config` & `app-guides` Tweaks (897 total)**:
  - **Network & Power**: `network-disable-wpad` (`DisableWpad` + `WpadOverride`), `network-encrypted-dns-global` (`EnableAutoDoh`, `EnableDot`, `EnableDdr`), `power-nic-pnp-capabilities` (`PnPCapabilities=24` across physical NICs), `power-nic-auto-power-save` (`AutoPowerSaveModeEnabled`, `EnableGreenEthernet`, `GigaLite`), `power-disable-storage-d3-modern-standby`.
  - **Visibility & Explorer**: `visibility-taskbar-end-task`, `visibility-taskbar-align-left`, `visibility-taskbar-hide-search`, `visibility-taskbar-never-combine`, `visibility-explorer-launch-this-pc`, `visibility-explorer-disable-graph-recent`, `visibility-disable-lockscreen-spotlight`.
  - **Privacy & Storage**: `privacy-disable-sleep-study` (`SleepStudyDisabled=1` + `wevtutil` Analytic channels + `AnalyzeSystem` task), `privacy-disable-cross-device-cdp`, `privacy-disable-phone-link-resume`, `privacy-disable-data-commercial-pipeline`, `peripheral-disable-autoplay`, `peripheral-storport-disable-telemetry`.
  - **App Guides (`misc` / `peripheral`)**: C# live executors for Spotify (`prefs` + `Users\*\prefs`), VS Code / VSCodium (`settings.json` with `.axis.bak` rollback), Logitech G HUB (`LGHUB-Toggle` services + virtual HID + `Run`), SteelSeries GG / Sonar, and Brave Browser Group Policies (`BraveRewardsDisabled`, `BraveWalletDisabled`, `BraveVPNDisabled`, `BraveAIChatEnabled`).
- **Verification**: Suite 15 added (`8,153` total assertions, 100% PASS, 0 FAIL, `dist/Axis.exe` 1.34 MB).

## [2.1.0] - 2026-11-26

### Major Engine Overhaul & Noverse 2026 1:1 Alignment
- **Dynamic Hardware Enumeration (`SpecialTweakExecutor`)**: Replaced literal placeholder paths (`00XX`, `00xx`, `...`) with live enumeration of physical PCI/USB network adapters, USB/HD Audio `PowerSettings`, NVIDIA Display Class adapters (`ClassGUID {4d36e968...}`), and PCI device MSI/IRQ affinity instances (`Enum\PCI\VEN_...`) with CPU topology-aware `AssignmentSetOverride` masks.
- **Native Windows Subsystem Executors**: Added real `bcdedit.exe` execution/status inspection (`system-bcd-*`, `disabledynamictick`, `useplatformclock`, `loadoptions`), `netsh.exe` TCP BBR2 + `loopbacklargemtu=disabled` fix for Steam/localhost, `powercfg.exe` High Performance plan + hibernation control, `MMAgent` memory compression, and live `ServiceController` start/stop.
- **Native NVIDIA DRS (`nvapi64.dll` via `NvDrsService`)**: Replaced fake registry writes for `nvidia-noverse-minimal-profile` with direct NVIDIA Base Profile DRS configuration (26 settings from `NV-Minimal.nip`), added `NvTmRep*`/`NvTmMon*` scheduled task disabling, `NvDevToolsVisible`, multi-key PowerMizer, and fixed `nvcpl.ps1 -Action Create/Restore` with portable standalone fallback.
- **Full Steam `localconfig.vdf` Engine (18 tweaks)**: Added 10 Noverse Steam configuration tweaks (`BackgroundRecordMode`, `EnableStreaming`, `LibraryLowPerfMode`, `LibraryLowBandwidthMode`, `LibraryDisableCommunityContent`, `Broadcast\Permissions`, `SignIntoFriends`, `ReduceMotion`, `InGameOverlayRestoreBrowserTabs`, `NotifyAvailableGames`).
- **Complete `REG_BINARY` / `REG_QWORD` / Live Win32 Refresh & Real Cleanup**: Fixed `REG_BINARY` parsing/serialization and `.reg` `=hex:` backups, added `SystemParametersInfo` / `WM_SETTINGCHANGE` live updates, fixed `SystemResponsiveness=100`, `HiberbootEnabled` path (`Session Manager\Power`), `*InterruptModeration` on NICs, `DisabledComponents=0x20` IPv4 preference, `%ProgramData%\Tweaker\` real-mode storage, and unified all 16 real `cleanup-*` actions cleanly out of batch recommended apply.
- **CI & Automated Verification**: Added `.github/workflows/ci-build-release.yml` (`windows-latest` MSBuild + `verify-all.ps1` + GitHub Release automation) and Test Suite 14 (**7,956 assertions**, 100% PASS, 0 FAIL).

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
