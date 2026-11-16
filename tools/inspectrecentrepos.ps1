$repos = @("dsc-verify", "thread-sim", "win32-ui", "regkit")
foreach ($r in $repos) {
    Write-Host "=== Repo: $r ==="
    $c = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/$r/commits" -Headers @{ "User-Agent" = "WinTweaker" }
    foreach ($item in ($c | Select-Object -First 3)) {
        Write-Host "$($item.sha.Substring(0,7)) | $($item.commit.author.date) | $($item.commit.message)"
    }
}
