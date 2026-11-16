try {
    $code = Invoke-RestMethod -Uri 'https://raw.githubusercontent.com/nohuto/utils/main/gm_effects/gm_effects.c' -UseBasicParsing -Headers @{ "User-Agent" = "PowerShell" }
    $code | Set-Content 'C:\Users\User\Documents\win-tweaker\tools\gm_effects.c'
    Write-Host "Downloaded gm_effects.c, length: $($code.Length)"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
