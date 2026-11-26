# App Guide: SteelSeries GG / Sonar (Noverse)

Source: https://noverse.dev/docs/app-guides/steelseries/ & https://github.com/nohuto/app-guides/blob/main/steelseries.md

Disables background Sonar virtual audio services and SteelSeries GG autostart/overlays to eliminate audio DPC/ISR latency:
- `SteelSeriesSonar` (`Start = 4`, stopped)
- `SteelSeriesGGClient` (`Start = 4`, stopped)
- `SteelSeriesAudioService` (`Start = 4`, stopped)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` / `HKLM\...\Run` -> `SteelSeriesGG`
