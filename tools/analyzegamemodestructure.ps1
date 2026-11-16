$sys = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md'

Write-Host "=== GAME MODE HEADINGS & STRUCTURE ==="
for ($i = 4380; $i -lt 5260; $i++) {
    if ($sys[$i] -match '^#+\s+') {
        Write-Host "Line $($i + 1): $($sys[$i])"
    }
}
