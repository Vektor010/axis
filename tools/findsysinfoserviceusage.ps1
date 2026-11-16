$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
Write-Host "=== Search _systemInfoService ==="
for ($i=0; $i -lt $c.Count; $i++) {
    if ($c[$i] -match '_systemInfoService') {
        Write-Host "$($i): $($c[$i])"
    }
}
