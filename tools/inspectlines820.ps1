$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
820..840 | ForEach-Object { "$($_): $($c[$_ - 1])" }
