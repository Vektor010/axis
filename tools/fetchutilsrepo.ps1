try {
    $api = Invoke-RestMethod -Uri 'https://api.github.com/repos/nohuto/utils/contents' -UseBasicParsing -Headers @{ "User-Agent" = "PowerShell" }
    $api | Select-Object name, path, type | Format-Table -AutoSize
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
