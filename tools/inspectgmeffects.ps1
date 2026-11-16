$sys = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md'

Write-Host "=== Searching for gm_effects in system/desc.md ==="
for ($i = 0; $i -lt $sys.Count; $i++) {
    if ($sys[$i] -match 'gm_effects') {
        Write-Host "Line $($i + 1): $($sys[$i])"
    }
}
