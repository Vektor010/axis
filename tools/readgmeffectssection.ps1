$sys = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md'
4410..4460 | ForEach-Object { "$($_): $($sys[$_ - 1])" }
