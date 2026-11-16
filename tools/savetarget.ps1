$raw = [System.IO.File]::ReadAllText('C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs', [System.Text.Encoding]::UTF8)
$idx = $raw.IndexOf('JournalCountDisplay')
$target = $raw.Substring($idx - 14, 130)
Write-Host "Target length: $($target.Length)"
$target | Set-Content 'C:\Users\User\Documents\win-tweaker\tools\target.txt' -NoNewline -Encoding UTF8
