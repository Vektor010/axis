try {
    $code = Invoke-RestMethod -Uri 'https://raw.githubusercontent.com/nohuto/utils/main/gm_effects/main.cpp' -UseBasicParsing
    Write-Host "Found main.cpp, length: $($code.Length)"
    $lines = $code -split "`n"
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match 'WNF') {
            Write-Host "$($i + 1): $($lines[$i])"
        }
    }
    # save full file to inspection scratch
    $code | Set-Content "C:\Users\User\Documents\win-tweaker\tools\gm_effects_main.cpp"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
