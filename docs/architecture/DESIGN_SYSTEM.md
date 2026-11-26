# AXIS Design System 2026 (Bento UI & Telemetry Standard)

> Single source of truth for contributors, UI engineers, and AI agents extending **AXIS** (`MainWindow.xaml`, `App.xaml`, `AxisDialogWindow.xaml`, `MainViewModel.cs`).

---

## 1. Visual Philosophy

AXIS follows an **Obsidian & Cobalt Engineering Bento** aesthetic inspired by modern hardware telemetry instruments:
- **Zero visual clutter**: Every label, badge, and divider must earn its pixels. Never duplicate live values across card headers and legends when already displayed cleanly in dedicated cards.
- **Pixel-exact row symmetry**: Sibling Bento cards in the same grid row MUST stretch evenly (`VerticalAlignment="Stretch"`) and use fixed-row inner `<Grid>` definitions instead of `<StackPanel>`, guaranteeing identical header baselines, metric vertical centers, and footer alignment across all DPI scales.
- **No jarring focus/outer blue borders**: Dialog windows, hero panels, Selected DataGrid rows, and cards use subtle graphite borders (`CardBorder`), reserving Cobalt Blue (`AccentBrush`) strictly for interactive emphasis, primary action CTAs, and CPU series indicators.
- **Dual-theme readiness**: Every UI surface binds exclusively via `{DynamicResource ...}` keys populated atomically by frozen `Brush` palettes in `App.xaml.cs` (`0.10 ms` switch latency, zero memory leaks). Every newly added color token MUST be added to both `BuildDarkPalette()` and `BuildLightPalette()` and frozen with `.Freeze()`.

---

## 2. Color Tokens (`App.xaml.cs`)

| Token Key | Dark Theme (Default) | Light Theme | Usage |
| :--- | :--- | :--- | :--- |
| `WindowBg` | `#07090E` | `#F1F5F9` | Root viewport canvas |
| `SidebarBg` | `#0B0F17` | `#E8EEF7` | Navigation rail background |
| `SidebarBorder` | `#1D263B` | `#D3DCEB` | Navigation rail separator |
| `CardBg` | `#111622` | `#FFFFFF` | Bento cards, panels, dialogs |
| `CardBgHover` | `#171F30` | `#F6F9FE` | Interactive hover card fill |
| `CardBorder` | `#1F2A40` | `#D8E1EF` | Default `1px` Bento / dialog stroke |
| `CardBorderHover` | `#32456B` | `#B6C6DF` | Hovered border emphasis |
| `TextPrimary` | `#ECF2FF` | `#0F172A` | Primary values, headings |
| `TextSecondary` | `#8B99B5` | `#5B6B86` | Micro-labels, subtitles, units |
| `AccentBrush` | `#3B82F6` | `#2563EB` | Primary CTA, **CPU** series, active nav |
| `AccentFill` | `#243B82F6` | `#1F2563EB` | Translucent accent pills / icon chips |
| `CyanAccentBrush` | `#38BDF8` | `#0284C7` | Kernel subsystem highlights (`HAGS`, active tweak badges) |
| `SuccessBrush` | `#22C55E` | `#16A34A` | Optimal state, **RAM** series, Network RX/TX & Ping |
| `WarningBrush` | `#F59E0B` | `#D97706` | Caution badges, **GPU (Видеокарта)** sparkline & curve |
| `DangerBrush` | `#EF4444` | `#DC2626` | Revert / risk / destructive warnings |
| `SearchBg` | `#0E131D` | `#FFFFFF` | Chart canvas backing & input fields |
| `BadgeBg` | `#161F30` | `#E7EEF9` | Neutral pill background |

---

## 3. Typography (`App.xaml`)

| Resource Key | Font Family Stack | Strict Usage Rules |
| :--- | :--- | :--- |
| `FontDisplay` | `Bahnschrift, Segoe UI Variable Display, Segoe UI` | **All** primary dashboard metric numbers across every card row (`24pt` in Row 1, `15pt` in Row 3, `14pt` in Row 4) — never mix `FontMono` and `FontDisplay` for sibling metrics in the same row. |
| `FontUI` | `Segoe UI Variable Text, Segoe UI` | Card titles (`9.5–10.5pt` `SemiBold`), descriptions, buttons, legends (`11pt`), body copy. |
| `FontMono` | `Cascadia Mono, Consolas` | Chart Y/X axis scale ticks (`8.8–9pt`), hover inspector readouts (`10pt`), status pills (`10pt` `Bold`), system RAM totals. |

---

## 4. Bento Grid Architecture (`MainWindow.xaml`)

Dashboard Overview (`1200×840`, `MinWidth=1000`, `MinHeight=660`) uses balanced rows where every card uses an explicit 3-row `<Grid>` (`VerticalAlignment="Stretch"`):

1. **Row 1 — Primary Live Cards (4 Columns, Equal Width `*`)**:
   - Inner layout: `<Grid>` with Row `20` (Title + right icon/badge `Height="20"`), Row `38` (`FontDisplay 24pt` main metric + sparkline `Width="80" Height="26"`), Row `16` (`10.5pt` secondary status or `ProgressBar`).
   - Cards: **ПРОЦЕССОР (ЦП)** (`AccentBrush`), **ВИДЕОКАРТА (GPU)** (`WarningBrush`), **ПАМЯТЬ (ОЗУ)** (`SuccessBrush`), **АКТИВНЫЕ ТВИКИ** (`CyanAccentBrush` progress bar).
2. **Row 2 — Oscilloscope Chart + System Passport (`1.25*` Left / `*` Right)**:
   - Both `<Border>` panels use `VerticalAlignment="Stretch"` so bottom borders align cleanly with zero empty gap under System specs.
3. **Row 3 — Secondary Sensors (4 Columns `*`, Unified `FontDisplay 15pt`)**:
   - Inner layout: Row `14` (`9.5pt SemiBold`), Row `24` (`FontDisplay 15pt SemiBold`), Row `15` (`10pt`).
   - Cards: **ДИСКИ (АКТИВНОСТЬ I/O)**, **СЕТЬ RX / TX**, **ЗАДЕРЖКА ПИНГА** (sparkline `Height="18"`), **АПТАЙМ / ПРОЦЕССЫ**.
4. **Row 4 — Native Kernel Subsystems (4 Columns `*`)**:
   - Inner layout: Row `14`, Row `22` (`FontDisplay 14pt SemiBold`), Row `15`.
   - Cards: **GPU HAGS (WDDM 2.9)**, **FLIP QUEUE (WDDM 3.0)**, **MMCSS • ПЛАНИРОВЩИК**, **GAME MODE (WNF)**.

---

## 5. Telemetry Oscilloscope Standard (`НАГРУЗКА (60 СЕК)`)

1. **Zero-Lag Right-to-Left Fixed-Slot Conveyor**:
   - Window fixed at `ChartSlotCount = 120` samples (`0.5s` default live cadence = `60s` window).
   - New points enter cleanly from the right edge (`Сейчас`) with constant step spacing `stepX`, never horizontally stretching or compressing historical spikes.
   - Avoid software-composited `OpacityMask` shaders on polygons; render lightweight frozen `PointCollection` polylines + `0.08–0.11` opacity area polygons for smooth 60 FPS updates.
2. **Adaptive Y-Axis Scale (`Auto · Y%` vs `0–100%`)**:
   - Steps strictly divisible by 4 (`[12, 16, 20, 40, 60, 80, 100]`) so all 4 grid quarters (`25%`, `50%`, `75%`, `100%`) yield clean integers with zero decimals.
   - Left Y-axis labels occupy `34px` gutter cleanly aligned to `ChartPlotTop = 8`, `ChartPlotBottom = 96` (`usableH = 88`, `22px` grid step), leaving `24px` bottom strip for `-60с` / `-30с` / `Сейчас` without overlapping `0%`.
3. **Strict Series Color & Localization Consistency**:
   - Legend is horizontally centered under the chart (`HorizontalAlignment="Center"`) in unified Russian:
     - **ЦП** → `AccentBrush` (Blue)
     - **ОЗУ** → `SuccessBrush` (Green)
     - **Видеокарта** → `WarningBrush` (Amber/Orange)
4. **Single-Series Hover Crosshair**:
   - `TrySampleChartAtX` snaps cursor X to sample index and identifies the **nearest vertical curve** to cursor Y, displaying only 1 highlighted dot + 1 concise badge tinted to that curve's brush (`ЦП · ...%`, `ОЗУ · ...% (... ГБ)`, or `Видеокарта · ...%`). `MouseMove` short-circuits whenever `(sampleIdx, seriesKey)` has not changed.
