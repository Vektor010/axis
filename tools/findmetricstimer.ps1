$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'

Write-Host "=== Metrics Timer Search ==="
for ($i=0; $i -lt $c.Count; $i++) {
    if ($c[$i] -match 'CollectLiveMetrics') {
        for ($j=$i-10; $j -le $i+35; $j++) {
            Write-Host "$($j): $($c[$j])"
        }
        break
    }
}
