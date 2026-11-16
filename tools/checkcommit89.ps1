$c = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/noverse.dev/commits/89b43dc" -Headers @{ "User-Agent" = "WinTweaker" }
foreach ($f in $c.files) {
    Write-Host "$($f.filename) (+ $($f.additions) / - $($f.deletions))"
}
