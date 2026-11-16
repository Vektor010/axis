$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
Write-Host "=== MainViewModel Constructor ==="
for ($i=0; $i -lt $c.Count; $i++) {
    if ($c[$i] -match 'public MainViewModel\(') {
        for ($j=$i; $j -le $i+50; $j++) {
            Write-Host "$($j): $($c[$j])"
        }
        break
    }
}
