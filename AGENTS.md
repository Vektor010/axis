# AGENTS.md — Universal AI Contributor & Autonomous Agent Guide

This document is the single source of truth for AI coding agents (Claude Code, Cursor, Windsurf, Gemini, GitHub Copilot, ChatGPT) and human engineers working on **AXIS**.

---

## 1. Project Mission & North Star

AXIS is an ultra-compact (~1.2 MB), zero-overhead, native Windows 10/11 optimization suite designed for competitive esports players and system engineers.

**Guiding Principles**:
1. **Never break user systems**: Every modification must have an isolated, deterministic rollback mechanism in `state.json`.
2. **Hardware honesty**: UI switches must reflect the live registry/WinAPI value in Windows, never an assumed or mocked state.
3. **Zero bloat**: Strictly zero external NuGet dependencies. The binary size must stay strictly below 5.0 MB.
4. **Build once, verify all**: Every change must pass 100% of all 7,760 unit and integration tests via `.\build.ps1`.

---

## 2. Hard Technical Constraints (Must Follow)

### 2.1 Language & Compiler Rules
- **Target Runtime**: .NET Framework 4.6.2 (WPF / XAML x64).
- **C# Language Level**: **C# 5.0 compatible**.
- **PROHIBITED SYNTAX** (will break MSBuild v4.0 on clean Windows environments):
  - ❌ String interpolation: `$"text {val}"` ➔ Use `string.Format("text {0}", val)`.
  - ❌ Null-conditional operator: `obj?.Prop` ➔ Use `if (obj != null) { ... }`.
  - ❌ Expression-bodied members: `public int X => 5;` ➔ Use `public int X { get { return 5; } }`.
  - ❌ Auto-property initializers: `public int X { get; set; } = 1;` ➔ Initialize in constructor.
  - ❌ `nameof(...)` operator ➔ Use string literals like `"PropertyName"`.
  - ❌ Pattern matching `is Type t` ➔ Use `as` and null checks.

### 2.2 WPF Resource Rules
- Any new `Brush` added to theme palettes must be explicitly frozen via `brush.Freeze()`.
- Unfrozen brushes cause visual tree notification churn and memory leaks during theme transitions.

---

## 3. Essential Commands

| Action | Command | Expected Output |
| :--- | :--- | :--- |
| **Full Build & Test** | `powershell -ExecutionPolicy Bypass -File .\build.ps1` | `7760 PASS / 0 FAIL`, 0 AST errors, `dist\Axis.exe` generated |
| **Run Tests Only** | `dist\Tests.exe` | 13 test suites PASS |
| **Verify Code & Scripts**| `powershell -ExecutionPolicy Bypass -File .\verify-all.ps1` | AST checks, schema validation, SHA-256 parity |
| **Headless Screenshot** | `dist\Axis.exe --mock --screenshot test.png --tab overview` | Generates 1180x800 offscreen render |

---

## 4. Key Codebase Directories

```
win-tweaker/
├── MainWindow.xaml / .cs           # Primary WPF shell, sidebar navigation, Bento views
├── App.xaml / .cs                  # Entry point, frozen ThemePalette, CLI argument parsing
├── tweaker.csproj                  # MSBuild configuration for Axis.exe
├── build.ps1                       # Continuous compilation and verification harness
├── verify-all.ps1                  # 4-stage strict verification pipeline
├── data/
│   ├── tweaks.json                 # Core database of 856 Windows optimizations
│   └── tweaks.schema.json          # JSON Schema validating every tweak definition
├── src/
│   ├── engine/
│   │   ├── tweakengine.cs          # Core engine executing WinAPI/registry mutations
│   │   ├── statemanager.cs         # Isolated snapshot management (state.json)
│   │   ├── journalmanager.cs       # Structured audit trail (journal.jsonl)
│   │   ├── systeminfoservice.cs    # WMI & hardware telemetry provider
│   │   ├── gamemodewnfservice.cs   # Windows Notification Facility (WNF) Game Mode tracker
│   │   ├── gpuhwschservice.cs      # WDDM 3.0 Hardware Flip Queue & HAGS detector
│   │   └── mmcsstaskservice.cs     # MMCSS multimedia scheduling priority service
│   ├── models/                     # TweakItem, TweakStatus, RegistryActionItem
│   └── viewmodels/
│       ├── mainviewmodel.cs        # Primary application state, search, categories, restore points
│       └── tweakcardviewmodel.cs   # Tweak card state, live system value querying, inspector
└── tests/
    └── program.cs                  # Standalone test runner (13 suites, 7,760 assertions)
```

---

## 5. Agent Verification Checklist Before Completing Work

1. Did you run `powershell -ExecutionPolicy Bypass -File .\build.ps1`?
2. Did all 7,760 tests pass?
3. Did `dist\Axis.exe` compile with a file size under 5.0 MB?
4. Are all modified `.ps1` scripts valid under PowerShell AST?
5. Did you update `PROJECT_STATE.md` with your exact changes?
