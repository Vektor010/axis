$c = Get-Content 'C:\Users\User\Documents\win-tweaker\MainWindow.xaml'
893..940 | ForEach-Object { "$($_): $($c[$_ - 1])" }
