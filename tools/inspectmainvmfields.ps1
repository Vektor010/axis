$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'

Write-Host "=== Status Pills Fields ==="
for ($i=0; $i -lt $c.Count; $i++) {
    if ($c[$i] -match 'Status Pills') {
        for ($j=$i; $j -le $i+40; $j++) {
            Write-Host "$($j): $($c[$j])"
        }
        break
    }
}
