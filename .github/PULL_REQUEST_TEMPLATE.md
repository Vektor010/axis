## 🚀 Pull Request: AXIS

### Description
Provide a concise explanation of the changes introduced in this PR.

### Related Issue
Fixes #(issue number)

### Type of Change
- [ ] 🐛 Bug fix (non-breaking change fixing an issue)
- [ ] ⚡ New tweak / tweak definition update
- [ ] 🎨 UI / UX refinement or typography fix
- [ ] 🏎️ Performance improvement
- [ ] 📖 Documentation update

### Mandatory Quality Checklist
- [ ] All code conforms to C# 5.0 (.NET Framework 4.6.2 MSBuild).
- [ ] `powershell -ExecutionPolicy Bypass -File .\build.ps1` runs locally and passes **100% of all 7760 tests** (0 failed).
- [ ] All PowerShell scripts pass AST syntax verification (`verify-all.ps1`).
- [ ] Final standalone binary size remains strictly **< 5.0 MB** without external NuGet bloat.
- [ ] If adding/modifying tweaks: `state.json` snapshot revert and `min_build` compatibility are defined.
- [ ] Tested on Windows 10/11 environment.
