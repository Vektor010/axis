$res = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/utils/commits" -Headers @{ "User-Agent" = "WinTweaker" }
foreach ($c in ($res | Select-Object -First 10)) {
    Write-Host "$($c.sha.Substring(0,7)) | $($c.commit.author.date) | $($c.commit.message)"
}
