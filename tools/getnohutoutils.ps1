$res = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/utils/contents" -Headers @{ "User-Agent" = "WinTweaker" }
foreach ($item in $res) {
    Write-Host "$($item.type): $($item.name)"
}
