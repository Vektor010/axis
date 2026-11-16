$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
100..120 | ForEach-Object { "$($_): $($c[$_ - 1])" }
