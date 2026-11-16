$dirs = @("ri_flagtest", "riflags", "xhci_imod", "heap_dump", "msd_test")
foreach ($d in $dirs) {
    Write-Host "=== $d ==="
    try {
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/utils/contents/$d" -Headers @{ "User-Agent" = "WinTweaker" }
        foreach ($it in $res) {
            Write-Host "  $($it.name)"
        }
    } catch {
        Write-Host "  Error: $_"
    }
}
