$url = "https://raw.githubusercontent.com/nohuto/utils/main/query_hwsch/query_hwsch.c"
$code = Invoke-RestMethod -Uri $url -Headers @{ "User-Agent" = "WinTweaker" }
Write-Host $code
