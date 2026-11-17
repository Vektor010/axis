# PROJECT_STATE.md — Current Operational State

**Last Updated**: 2026-11-17  
**Version**: v2.0.0 (Flagship Release)  
**Binary Output**: `dist/Axis.exe` (1.22 MB, standalone x64)

---

## 1. Summary of What Was Implemented

1. **Complete UI/UX Overhaul to Bento 2026**:
   - Modernized all 6 primary navigation screens (Overview, Tweaks, Journal, History, Settings, About).
   - Calibrated layout geometry to 1180x800, eliminating vertical scrollbars across 5 of 6 screens.
   - Designed a 2-column Bento Settings Hub with Windows System Restore (VSS) integration, local `state.json` backup export, esports telemetry polling toggles (1s, 2s, 5s, Pause), and network ping presets.
2. **Deep Tweak Inspector**:
   - Added live Windows registry query cards displaying the active hardware value, target value, optimization compliance badge, reboot requirement, and explicit Apply / Revert buttons.
3. **Theme Engine & Memory Optimization (DEF-03 Resolution)**:
   - Converted dynamic color allocations in `App.xaml.cs` to a static, pre-cached palette of frozen (`.Freeze()`) brushes.
   - Benchmark confirmed **0.10 ms median latency** and eliminated visual tree listener leaks.
4. **Complete Elimination of Legacy Tweaker.exe**:
   - Removed `Tweaker.exe` from Desktop, dist folder, optimization workspace, and GitHub release assets.
   - Updated `build.ps1` and `verify-all.ps1` to target `Axis.exe` exclusively.
5. **Universal Open Source Repository Architecture**:
   - Created `AGENTS.md`, `ARCHITECTURE.md`, `PROJECT_STATE.md`, `ROADMAP.md`, `DECISIONS.md`, `KNOWN_ISSUES.md`, `DEVELOPMENT.md`, `SECURITY.md`, `CONTRIBUTING.md`, `CHANGELOG.md`, `README.md`, `README.ru.md`, issue templates, and PR template.

---

## 2. Files Modified and Added

### Modified
- `src/viewmodels/tweakcardviewmodel.cs`: Added `LiveSystemValueText`, `RecommendedValueText`, `OptimizationStatusText`, `RebootRequirementText`, `ApplyTweakCommand`.
- `MainWindow.xaml`: Implemented 2-column Bento Settings Hub and expanded Tweak Inspector.
- `App.xaml.cs`: Optimized `ApplyTheme` with pre-cached frozen brush palette.
- `build.ps1`: Cleaned up legacy Tweaker.exe output and build step.
- `verify-all.ps1`: Updated to verify `Axis.exe` exclusively.
- `README.md`: Flagship English documentation.
- `README.ru.md`: Comprehensive Russian documentation.

### Added
- `AGENTS.md`
- `ARCHITECTURE.md`
- `PROJECT_STATE.md`
- `ROADMAP.md`
- `DECISIONS.md`
- `KNOWN_ISSUES.md`
- `DEVELOPMENT.md`
- `SECURITY.md`
- `CONTRIBUTING.md`
- `CHANGELOG.md`
- `.github/ISSUE_TEMPLATE/` (bug_report.yml, feature_request.yml, tweak_request.yml, config.yml)
- `.github/PULL_REQUEST_TEMPLATE.md`
- `docs/ci-verify.yml.template`
- `tools/ThemeBenchmark.cs`

---

## 3. Test Verification Results

| Suite / Check | Assertions | Result | Status |
| :--- | :---: | :---: | :---: |
| **PowerShell AST Syntax** | 52 scripts | 52 PASS / 0 FAIL | **PASS** |
| **Tweak Definition Audit** | 856 tweaks | 856 PASS / 0 FAIL | **PASS** |
| **Embedded tweaks.json Parity** | SHA-256 | Exact Match | **PASS** |
| **Executable Size Limit** | 1.22 MB (< 5.0 MB) | 1,249.5 KB | **PASS** |
| **Automated Unit & Integration Tests** | 13 test suites | **7,760 PASS / 0 FAIL** | **PASS** |
| **Theme Toggle Latency Benchmark** | 30 cycles | Median: 0.10 ms, P95: 2.67 ms | **PASS** |

---

## 4. Current Limitations & Known Issues

1. **DEF-04 (Hardware PCI Templates)**: 5 tweaks in `affinities-*` use template paths `Enum\PCI\...` and require custom hardware instance IDs (isolated via `needs_review: true`).
2. **DEF-05 (260 Tweaks with needs_review)**: 260 tweaks lack expanded technical descriptions in original source documentation (clear warning badge displayed in inspector).
3. **UAC Requirement**: Write operations require administrative privileges.

---

## 5. Next Recommended Milestone

- **Phase 3**: Implement dynamic PCI bus enumeration service to auto-detect active discrete GPU and NIC instance IDs, replacing `...` in `affinities-*` tweaks dynamically at runtime.

---

## 6. Exact Build & Run Commands

```powershell
# Clean build and automated 7,760 tests run
powershell -ExecutionPolicy Bypass -File .\build.ps1

# Direct launch
.\dist\Axis.exe

# Safe in-memory simulation mode (mock store, zero registry changes)
.\dist\Axis.exe --mock
```
