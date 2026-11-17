# Architecture Decision Records (ADRs)

This document records the foundational architectural decisions made in AXIS.

---

## ADR-001: Native C# / WPF over Electron or Web-Based Frameworks

- **Status**: Accepted
- **Context**: Many modern system tools use Electron or webview frameworks. However, system optimization tools targeting esports players must not introduce background memory bloat or high idle CPU overhead.
- **Decision**: AXIS is built as a pure native C# WPF application targeting the native Desktop Window Manager (DWM) with hardware acceleration.
- **Consequences**:
  - Memory consumption is low (~40 MB working set).
  - Executable size is tiny (~1.22 MB).
  - Direct P/Invoke and WinAPI access without IPC or node runtime overhead.

---

## ADR-002: Target .NET Framework 4.6.2 and C# 5.0 Compatibility

- **Status**: Accepted
- **Context**: Modern .NET (such as .NET 8/9) requires either separate runtime installation or bundled runtimes that balloon executable size to 60–100+ MB.
- **Decision**: Target .NET Framework 4.6.2, which is pre-installed out of the box on every Windows 10 (1607+) and Windows 11 machine worldwide.
- **Consequences**:
  - Zero prerequisite installation for end users.
  - Strict code constraints: must use C# 5.0 syntax to ensure builds succeed with native MSBuild (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe`).

---

## ADR-003: Isolated Snapshot Architecture (`state.json`)

- **Status**: Accepted
- **Context**: Traditional batch scripts and tweakers destructively overwrite registry keys without recording initial states, preventing reliable rollbacks.
- **Decision**: Implement a two-phase commit model managed by `StateManager`. Before applying any tweak, the original registry value and type are stored in an atomic `state.json` file.
- **Consequences**:
  - Every modification can be reversed to its exact prior value in a single click.
  - Absence of a key on fresh Windows installations is preserved via `"delete"` revert modes.

---

## ADR-004: Pre-Cached Frozen Brushes for Theme Rendering

- **Status**: Accepted
- **Context**: Dynamic allocation of unfrozen `SolidColorBrush` instances during theme toggles caused visual tree event listener churn and temporary memory spikes (+234 MB across 30 cycles).
- **Decision**: Pre-allocate all theme brush definitions statically at startup and freeze them using `.Freeze()`.
- **Consequences**:
  - Theme toggles execute with 0.10 ms median latency.
  - Brushes become thread-safe immutable constants with zero WPF notification overhead.
