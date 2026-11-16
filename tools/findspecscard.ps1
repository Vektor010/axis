$c = Get-Content 'C:\Users\User\Documents\win-tweaker\MainWindow.xaml'
for ($i=0; $i -lt $c.Count; $i++) {
    if ($c[$i] -match 'System Specs Card') {
        Write-Host "Found at line $i"
        for ($j=$i; $j -le $i+60; $j++) {
            Write-Host "$($j): $($c[$j])"
        }
    }
}
