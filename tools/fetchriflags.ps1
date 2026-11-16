$url = "https://raw.githubusercontent.com/nohuto/utils/main/ri_flagtest/ri_flagtest.c"
$code = Invoke-RestMethod -Uri $url -Headers @{ "User-Agent" = "WinTweaker" }
Write-Host $code
