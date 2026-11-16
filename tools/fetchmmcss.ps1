$url = "https://raw.githubusercontent.com/nohuto/utils/main/mmcss_task/mmcss_task.c"
$code = Invoke-RestMethod -Uri $url -Headers @{ "User-Agent" = "WinTweaker" }
Write-Host $code
