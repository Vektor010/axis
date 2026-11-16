$c = Get-Content 'C:\Users\User\Documents\win-tweaker\Tests\Program.cs'
$matches = $c | Select-String -Pattern 'MainViewModel|MainWindow|Dashboard|Specs|Metrics'
Write-Host "Matches found: $($matches.Count)"
$matches | ForEach-Object { Write-Host $_.Line }
