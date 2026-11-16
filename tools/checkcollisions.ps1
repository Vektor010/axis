$tweaks = Get-Content C:\Users\User\Documents\win-tweaker\data\tweaks.json | ConvertFrom-Json
$namesToCheck = @(
  'PowerThrottlingOff', 'EnabledActions', 'EnableDsNetRefresh', 'AlwaysComputeQosHints',
  '*DeviceSleepOnDisconnect', '*EnableDynamicPowerGating', 'DMACoalescing', '*EEE', '*SelectiveSuspend', 'PerformanceIdleTime',
  'ForegroundPriorityBoost', 'AdjustWorkerThreadPriority', 'DisableOverlays', 'OverlayTestMode', 'DisableIndependentFlip', 'DisableAdvancedDirectFlip',
  'HideXGpuTrayIcon', 'ContextUIPolicy', 'ShowDlssIndicator', 'OptInOrOutPreference',
  'LetAppsRunInBackground', 'AutoDownloadAndUpdateMapData', 'EnableCdp', 'EnableActivityFeed', 'AllowCortana'
)
$found = 0
foreach ($t in $tweaks) {
  foreach ($r in $t.registry) {
    if ($namesToCheck -contains $r.name) {
      Write-Host "Conflict found: $($t.id) -> $($r.hive)\$($r.path)\$($r.name)"
      $found++
    }
  }
}
Write-Host "Total conflicts: $found"
