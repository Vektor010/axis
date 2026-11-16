$items = @(
    @{ file="security/desc.md"; from=1111; to=1115; path="Software\Policies\Microsoft\Windows NT\SystemRestore"; name="DisableConfig" },
    @{ file="security/desc.md"; from=1161; to=1165; path="Software\Policies\Microsoft\Windows\System"; name="BlockDomainPicturePassword" },
    @{ file="security/desc.md"; from=1174; to=1177; path="Software\Policies\Microsoft\Windows\RemovableStorageDevices\{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}"; name="Deny_Write" },
    @{ file="peripheral/desc.md"; from=1389; to=1395; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="NoAutorun" },
    @{ file="peripheral/desc.md"; from=1389; to=1395; path="Software\Policies\Microsoft\Windows\Explorer"; name="NoAutoplayfornonVolume" },
    @{ file="peripheral/desc.md"; from=1389; to=1395; path="SOFTWARE\Policies\Microsoft\Windows\Explorer"; name="DontSetAutoplayCheckbox" },
    @{ file="peripheral/desc.md"; from=1544; to=1547; path="SOFTWARE\Policies\Microsoft\TabletPC"; name="TurnOffTouchInput" },
    @{ file="peripheral/desc.md"; from=1413; to=1415; path="Software\Microsoft\Wisp\Touch"; name="PanningDisabled" },
    @{ file="peripheral/desc.md"; from=1413; to=1421; path="Software\Microsoft\Wisp\Touch"; name="TouchGate" },
    @{ file="peripheral/desc.md"; from=1449; to=1457; path="Software\Microsoft\Wisp\Pen\SysEventParameters"; name="FlickMode" },
    @{ file="peripheral/desc.md"; from=1449; to=1460; path="Software\Microsoft\Wisp\Pen\SysEventParameters"; name="SampleTime" },
    @{ file="power/desc.md"; from=2370; to=2373; path="Software\Policies\Microsoft\Windows\System"; name="HiberbootEnabled" },
    @{ file="power/desc.md"; from=2396; to=2401; path="Software\Policies\Microsoft\Windows\Explorer"; name="ShowLockOption" },
    @{ file="power/desc.md"; from=2396; to=2401; path="Software\Policies\Microsoft\Windows\Explorer"; name="ShowSleepOption" },
    @{ file="power/desc.md"; from=2396; to=2401; path="Software\Policies\Microsoft\Windows\Explorer"; name="ShowHibernateOption" },
    @{ file="power/desc.md"; from=2406; to=2411; path="System\CurrentControlSet\Control\Power"; name="UserBatteryDischargeEstimator" },
    @{ file="power/desc.md"; from=2406; to=2411; path="System\CurrentControlSet\Control\Power"; name="EnergyEstimationEnabled" },
    @{ file="visibility/desc.md"; from=1359; to=1368; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="NoDispAppearancePage" },
    @{ file="visibility/desc.md"; from=1359; to=1368; path="Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop"; name="NoChangingWallPaper" },
    @{ file="visibility/desc.md"; from=1359; to=1368; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="NoThemesTab" },
    @{ file="visibility/desc.md"; from=1385; to=1388; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="UseDefaultTile" },
    @{ file="system/desc.md"; from=9406; to=9409; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="VerboseStatus" },
    @{ file="system/desc.md"; from=9456; to=9459; path="Software\Policies\Microsoft\Windows\Appx"; name="AllowAutomaticAppArchiving" },
    @{ file="system/desc.md"; from=9150; to=9166; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="PreventRemoteQueries" },
    @{ file="system/desc.md"; from=9150; to=9166; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="PreventIndexOnBattery" }
)

$failed = 0
foreach ($it in $items) {
    $filePath = "C:\Users\User\Documents\win-tweaker\data\source\win-config\" + $it.file
    $lines = Get-Content $filePath
    $sub = $lines[($it.from - 1)..($it.to - 1)] -join "`n"
    
    $hasNameOrPath = ($sub.IndexOf($it.name, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) -or ($sub.IndexOf($it.path, [System.StringComparison]::OrdinalIgnoreCase) -ge 0)
    $hasType = $sub.IndexOf("Type:", [System.StringComparison]::OrdinalIgnoreCase) -ge 0

    if (-not $hasNameOrPath) {
        Write-Host "FAIL: $($it.name) not found in $($it.file):$($it.from)..$($it.to)" -ForegroundColor Red
        $failed++
    }
    if ($hasType) {
        Write-Host "WARNING Type: found in $($it.name)" -ForegroundColor Yellow
        $failed++
    }
    if ($hasNameOrPath -and (-not $hasType)) {
        Write-Host "PASS: $($it.name)" -ForegroundColor Green
    }
}

Write-Host "Finished. Failed: $failed"
