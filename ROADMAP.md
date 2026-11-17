# AXIS Project Roadmap (2026)

## Version 2.0.0 (Released — Q4 2026) ✅
- [x] Full rebranding to **AXIS** with custom SVG vector identity.
- [x] Modern 2026 Bento UI with calibrated 1180x800 geometry and zero extraneous scrollbars.
- [x] Deep Tweak Inspector displaying real-time live Windows registry values.
- [x] Complete Settings Hub: Windows System Restore (VSS) checkpoints, snapshot database export, telemetry polling controls (1s, 2s, 5s, Pause), and ping presets.
- [x] Frozen brush theme engine achieving 0.10 ms toggle response.
- [x] 100% test coverage across 7,760 assertions and 52 AST-verified scripts.

---

## Version 2.1.0 (Q1 2027) 🎯
- [ ] **Hardware PCI Auto-Discovery**: Runtime PCI device scanner to dynamically resolve GPU (`VEN_10DE...`, `VEN_1002...`) and NIC hardware paths for `affinities-*` interrupt priority tweaks.
- [ ] **Custom Profile Import/Export**: Save and load community esports presets (`.axisprofile` JSON format).
- [ ] **Expanded Documentation**: Complete technical annotations for all 260 `needs_review` parameters.

---

## Version 2.2.0 (Q2 2027) 🔮
- [ ] **Per-Game Optimization Engine**: Automatic activation of specific tweak sets when designated games (CS2, Valorant, Fortnite) launch via WNF process detection.
- [ ] **High-Resolution Timer Service**: Native query and adjustment of Windows global timer resolution (0.5ms precision) with DWM frame synchronization.
