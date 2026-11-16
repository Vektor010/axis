$tweaks = Get-Content C:\Users\User\Documents\win-tweaker\data\tweaks.json | ConvertFrom-Json

$idsToCheck = @(
  'system-disable-bsod-autoreboot', 'system-auto-end-tasks', 'system-hung-app-timeout', 'system-wait-to-kill-service-timeout',
  'system-disable-web-search-bing', 'system-disable-search-highlights',
  'security-tdr-delay-increase', 'security-tdr-ddi-delay-increase', 'security-disable-wpbt-execution',
  'security-disable-system-restore', 'security-disable-password-reveal',
  'network-nic-disable-vmq', 'network-nic-rss-vmq-preference', 'network-nic-disable-sriov',
  'network-nic-disable-fec', 'network-disable-ics-sharing',
  'peripheral-disable-autoplay-handlers', 'peripheral-disable-drive-type-autorun', 'peripheral-disable-startup-sound',
  'peripheral-disable-lock-on-disconnect',
  'visibility-menu-show-delay-zero', 'visibility-taskbar-end-task', 'visibility-no-lock-screen',
  'visibility-disable-first-logon-animation', 'visibility-alt-tab-no-browser-tabs'
)

$namesToCheck = @(
  'AutoReboot', 'AutoEndTasks', 'HungAppTimeout', 'WaitToKillServiceTimeout',
  'DisableWebSearch', 'EnableDynamicContentInWSB',
  'TdrDelay', 'TdrDdiDelay', 'DisableWpbtExecution',
  'DisableSR', 'DisablePasswordReveal',
  '*VMQ', '*RssOrVmqPreference', '*Sriov',
  'FecMode', 'NC_ShowSharedAccessUI',
  'DisableAutoplay', 'NoDriveTypeAutoRun', 'DisableStartupSound',
  'Language Hotkey',
  'MenuShowDelay', 'TaskbarEndTask', 'NoLockScreen',
  'EnableFirstLogonAnimation', 'MultiTaskingAltTabFilter'
)

$idCollisions = 0
$regCollisions = 0

foreach ($t in $tweaks) {
  if ($idsToCheck -contains $t.id) {
    Write-Host "ID Collision: $($t.id)"
    $idCollisions++
  }
  foreach ($r in $t.registry) {
    if ($namesToCheck -contains $r.name) {
      Write-Host "Reg Collision: $($t.id) -> $($r.hive)\$($r.path)\$($r.name)"
      $regCollisions++
    }
  }
}

Write-Host "ID Collisions: $idCollisions, Reg Collisions: $regCollisions"
