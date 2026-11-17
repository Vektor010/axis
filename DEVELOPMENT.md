# Developer Guide & Build Setup

This guide provides instructions for setting up a local development environment, compiling AXIS from source, and executing the automated verification suite.

---

## 1. Prerequisites

- **Operating System**: Windows 10 (Build 1809+) or Windows 11 x64.
- **Build Tools**: Microsoft MSBuild (shipped with the .NET Framework 4.6.2+ Developer Pack, Visual Studio 2017/2019/2022, or standard Windows Build Tools).
- **PowerShell**: Windows PowerShell 5.1 or PowerShell 7+.

---

## 2. Clone the Repository

```powershell
git clone https://github.com/Vektor010/axis.git
cd axis
```

---

## 3. Building the Project

AXIS uses an automated PowerShell build script that locates MSBuild, compiles the flagship binary, and runs the test suite:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

### Build Artifacts
Upon successful compilation, binaries are placed in `dist/`:
- `dist/Axis.exe`: The primary standalone application (~1.22 MB).
- `dist/Tests.exe`: Standalone console test runner (~123 KB).

---

## 4. Running Tests

### Execute Full Automated Test Suite
```cmd
dist\Tests.exe
```

The test runner runs 13 test suites covering:
1. `tweaks.schema.json` validation.
2. Parity with upstream source references.
3. In-memory `MockRegistryStore` operations.
4. `TweakEngine` pre-check, apply, and rollback behavior.
5. System state snapshot isolation.
6. `RegistryWatcher` notification events.
7. `RestorePointManager` VSS boundaries.
8. Game Mode WNF subsystem integration.
9. WDDM 3.0 Hardware Flip Queue and HAGS detection.
10. MMCSS thread prioritization (`avrt.dll`).
11. Full end-to-end Apply -> Verify -> Revert cycle for all 856 tweaks.

### Code Verification Pipeline
```powershell
powershell -ExecutionPolicy Bypass -File .\verify-all.ps1
```

This verifies:
- PowerShell AST syntax across all 52 scripts.
- Structural schema audit across all 856 tweaks.
- Binary size constraint enforcement (< 5.0 MB).
- SHA-256 byte-for-byte parity of the embedded `tweaks.json` resource.

---

## 5. Offline & Headless Testing

You can run AXIS headlessly to generate PNG screen captures without opening a window:

```powershell
# Capture Overview screen
.\dist\Axis.exe --mock --screenshot docs/images/overview_test.png --tab overview

# Capture Tweaks screen with first card expanded
.\dist\Axis.exe --mock --screenshot docs/images/tweaks_test.png --tab tweaks --expand-card
```
