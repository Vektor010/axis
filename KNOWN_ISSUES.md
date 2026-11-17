# Known Issues & Technical Limitations

This document tracks known limitations, hardware-specific considerations, and their workarounds.

---

## 1. Hardware PCI Device Placeholders (`affinities-*`)
- **Affects**: 5 tweaks under the `affinities` category (`affinities-gpu-device-priority`, `affinities-nic-device-priority`, etc.).
- **Details**: The target registry path includes a wildcard placeholder:
  `HKLM\SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy`
  Because PCI device IDs vary by GPU and network card manufacturer, these tweaks cannot be applied blindly.
- **Current Status**: Isolated with `needs_review: true`. These tweaks are excluded from the automated "Apply Recommended" batch.
- **Resolution Path**: Version 2.1.0 will introduce an automatic PCI device resolution service.

---

## 2. Incomplete Documentation for Certain Parameters (`needs_review`)
- **Affects**: 260 parameters in `data/tweaks.json`.
- **Details**: These tweaks have verified registry paths and types, but lack expanded technical descriptions in the original upstream knowledge base.
- **Current Status**: Visual amber warning banner displayed in the Tweak Inspector to alert users that manual review is advised.

---

## 3. Administrative Elevation Requirement
- **Details**: Writing to `HKLM` (Local Machine) registry keys requires standard Windows administrator elevation. Running AXIS under a standard user account allows read-only inspection, but write mutations will fail gracefully with access denied notices.
- **Workaround**: Launch AXIS by right-clicking `Axis.exe` and selecting "Run as administrator".
