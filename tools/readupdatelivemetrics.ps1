$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
1315..1365 | ForEach-Object { "$($_): $($c[$_ - 1])" }
