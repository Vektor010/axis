# App Guide: Logitech G HUB Toggle (Noverse)

Source: https://noverse.dev/docs/app-guides/lghub/ & https://github.com/nohuto/app-guides/blob/main/assets/LGHUB-Toggle.ps1

Disables background G HUB services, virtual input drivers, and autostart when not configuring peripherals:
- `LGHUBUpdaterService` (`Start = 4`, stopped)
- `logi_joy_bus_enum` (`Start = 4`)
- `logi_joy_vir_hid` (`Start = 4`)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` -> `LGHUB` removed
