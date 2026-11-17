# Contributing to AXIS

Thank you for your interest in contributing to AXIS! We welcome code contributions, tweak proposals, documentation improvements, and bug fixes.

## Engineering Standards & Architecture

AXIS is built as an ultra-compact, high-performance native Windows optimization suite with zero external package dependencies.

### 1. Language & Compiler Constraints
- **Framework**: .NET Framework 4.6.2 (WPF / XAML x64).
- **C# Language Version**: C# 5.0 compatible.
- **Rules**:
  - Do not use C# 6.0+ syntax (e.g. string interpolation `$"..."`, null-conditional operator `?.`, read-only auto-properties `{ get; } = val;`, `nameof(...)`).
  - Use `string.Format(...)`, explicit `null` checks, and full properties with backing fields.
  - Never introduce external NuGet packages without prior architectural RFC. The standalone executable size must remain **strictly under 5.0 MB** (current footprint: ~1.22 MB).

### 2. Tweak Submission Guidelines
When contributing a new Windows tweak to `data/tweaks.json`:
1. **Schema Compliance**: Every tweak must conform to `data/tweaks.schema.json`.
2. **Revert Requirement**: You must define an accurate revert specification:
   - `mode: "set"` with an explicit default value.
   - `mode: "delete"` if the value does not exist on fresh Windows installations.
3. **Hardware / Compatibility**: Specify `min_build` (e.g., 22000 for Windows 11).
4. **Verification**: Include technical documentation references in the `source` field.

### 3. Local Verification Process
Before submitting a Pull Request, run the local continuous verification suite:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

The build script will:
1. Compile `dist/Axis.exe` and `dist/Tests.exe` using native MSBuild.
2. Verify PowerShell AST syntax on all 52 scripts.
3. Validate all 856 tweaks in `data/tweaks.json`.
4. Verify SHA-256 integrity of the embedded resource.
5. Execute all 7,760 automated tests across 13 test suites.

All checks must pass (`7760 PASS / 0 FAIL`, 0 AST errors) before submitting your PR.
