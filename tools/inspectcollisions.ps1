$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json

$colliding = @(
    "Deny_Write",
    "NoAutoplayfornonVolume",
    "TurnOffTouchInput",
    "ShowSleepOption",
    "ShowHibernateOption",
    "EnergyEstimationEnabled",
    "VerboseStatus",
    "PreventRemoteQueries",
    "PreventIndexOnBattery"
)

foreach ($c in $colliding) {
    $found = $tweaks | Where-Object { 
        $t = $_
        $hasKey = $false
        foreach ($r in $t.registry) {
            if ($r.name -ieq $c) { $hasKey = $true }
        }
        $hasKey
    }
    Write-Host "$c is in: $($found.id)"
}
