$repos = Invoke-RestMethod -Uri "https://api.github.com/users/nohuto/repos?sort=updated" -Headers @{ "User-Agent" = "WinTweaker" }
foreach ($r in $repos) {
    Write-Host "$($r.name) | Updated: $($r.updated_at) | Pushed: $($r.pushed_at)"
}
