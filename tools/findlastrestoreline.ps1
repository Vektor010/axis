$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
for ($i=0; $i -lt $c.Count; $i++) {
    if ($c[$i] -match 'LastRestorePointDisplay') {
        Write-Host "Line $($i + 1): $($c[$i])"
    }
}
