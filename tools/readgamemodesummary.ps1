$sys = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md'
5070..5235 | ForEach-Object { "$($_): $($sys[$_ - 1])" }
