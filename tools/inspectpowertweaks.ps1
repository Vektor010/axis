$tw = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$res = @()
foreach ($t in $tw) {
    if ($t.category -eq 'power' -or $t.id -match 'power|shutdown') {
        $r = $t.registry[0]
        $res += [PSCustomObject]@{
            Id = $t.id
            Name = $t.name_ru
            Group = $t.group
            Apply = $r.apply
            RevertMode = $r.revert.mode
            RevertVal = $r.revert.value
        }
    }
}
$res | Format-Table -AutoSize
