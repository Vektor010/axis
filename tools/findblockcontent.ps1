$c = Get-Content 'C:\Users\User\Documents\win-tweaker\Tests\Program.cs'
for ($i=0; $i -lt $c.Count; $i++) {
    if ($c[$i] -match 'blockContent\.IndexOf') {
        Write-Host "Line $i : $($c[$i])"
        for ($j=$i-10; $j -le $i+15; $j++) {
            Write-Host "$($j): $($c[$j])"
        }
    }
}
