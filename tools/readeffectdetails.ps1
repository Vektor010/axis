$sys = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md'
4836..4950 | ForEach-Object { "$($_): $($sys[$_ - 1])" }
