$raw = [System.IO.File]::ReadAllText('C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs')
$idx = $raw.IndexOf('_systemInfoService = new SystemInfoService();')
Write-Host "Index: $idx"
if ($idx -ge 0) {
    $slice = $raw.Substring($idx - 30, 90)
    Write-Host "Slice: [$slice]"
}
