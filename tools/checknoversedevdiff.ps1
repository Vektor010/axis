$c = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/noverse.dev/commits/16de922" -Headers @{ "User-Agent" = "WinTweaker" }
Write-Host "Commit 16de922 files changed: $($c.files.Count)"
foreach ($f in $c.files) {
    Write-Host "$($f.filename) (+ $($f.additions) / - $($f.deletions))"
}
