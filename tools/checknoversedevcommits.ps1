$commits = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/noverse.dev/commits" -Headers @{ "User-Agent" = "WinTweaker" }
foreach ($c in ($commits | Select-Object -First 5)) {
    Write-Host "$($c.sha.Substring(0,7)) | $($c.commit.author.date) | $($c.commit.message)"
}
