# App Guide: Brave Browser Policies & Background Services (Noverse)

Source: https://noverse.dev/docs/app-guides/brave-desktop/ & https://github.com/nohuto/app-guides/blob/main/brave-desktop.md

Group Policies (`HKLM\SOFTWARE\Policies\BraveSoftware\Brave`):
- `BackgroundModeEnabled` = `0`
- `MetricsReportingEnabled` = `0`
- `BraveRewardsDisabled` = `1`
- `BraveWalletDisabled` = `1`
- `BraveVPNDisabled` = `1`
- `BraveAIChatEnabled` = `0`

Update Services (`HKLM\SYSTEM\CurrentControlSet\Services\brave`, `BraveElevationService`, `bravem`):
- `Start` = `4` (Disabled) when updater removal tweak is enabled
