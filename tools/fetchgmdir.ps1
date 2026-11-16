try {
    $api = Invoke-RestMethod -Uri 'https://api.github.com/repos/nohuto/utils/contents/gm_effects' -UseBasicParsing -Headers @{ "User-Agent" = "PowerShell" }
    $api | Select-Object name, download_url | Format-Table -AutoSize
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
