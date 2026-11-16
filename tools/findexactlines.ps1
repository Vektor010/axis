$path = 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
$lines = Get-Content $path
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'JournalCountDisplay') {
        Write-Host "Lines $i..$($i+5):"
        $i..($i+5) | % { Write-Host "$($_): $($lines[$_])" }
    }
    if ($lines[$i] -match 'UpdateRecentJournal') {
        Write-Host "Lines $($i-3)..$($i+3):"
        ($i-3)..($i+3) | % { Write-Host "$($_): $($lines[$_])" }
    }
}
