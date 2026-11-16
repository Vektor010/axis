$raw = [System.IO.File]::ReadAllText('C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs')
$idx = $raw.IndexOf('JournalCountDisplay')
Write-Host "Index: $idx"
if ($idx -ge 0) {
    $slice = $raw.Substring($idx - 20, 140)
    Write-Host "Slice: [$slice]"
}
