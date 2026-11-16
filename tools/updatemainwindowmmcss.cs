using System;
using System.IO;

class Program
{
    static int Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\MainWindow.xaml";
        string content = File.ReadAllText(path);

        string target = "                        <!-- 2.4 Bottom Two Cards: Последние действия & Требуют внимания -->";
        if (!content.Contains(target))
        {
            Console.WriteLine("Error: Target marker not found in MainWindow.xaml");
            return 1;
        }

        string mmcssCard = @"                        <!-- 2.3d MMCSS & Multimedia Tasks (Noverse mmcss_task) Diagnostic Card -->
                        <Border Background=""{DynamicResource CardBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""12"" Padding=""14,12"" Margin=""0,0,0,12"">
                            <StackPanel>
                                <Grid Margin=""0,0,0,8"">
                                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                                        <TextBlock Text=""&#xE768;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets"" FontSize=""13"" Foreground=""{DynamicResource AccentBrush}"" VerticalAlignment=""Center"" Margin=""0,0,6,0"" />
                                        <TextBlock Text=""МОНИТОРИНГ MMCSS &amp; ПЛАНИРОВАНИЕ ЗАДАЧ (NOVERSE MMCSS_TASK)"" Style=""{StaticResource CardHeaderStyle}"" />
                                    </StackPanel>
                                    <Border HorizontalAlignment=""Right"" CornerRadius=""6"" Padding=""8,2"">
                                        <Border.Style>
                                            <Style TargetType=""Border"">
                                                <Setter Property=""Background"" Value=""{DynamicResource BadgeBg}"" />
                                                <Style.Triggers>
                                                    <DataTrigger Binding=""{Binding IsMmcssOptimized}"" Value=""True"">
                                                        <Setter Property=""Background"" Value=""#253526"" />
                                                    </DataTrigger>
                                                </Style.Triggers>
                                            </Style>
                                        </Border.Style>
                                        <TextBlock FontSize=""10.5"" FontWeight=""SemiBold"">
                                            <TextBlock.Style>
                                                <Style TargetType=""TextBlock"">
                                                    <Setter Property=""Text"" Value=""MMCSS: СТАНДАРТ"" />
                                                    <Setter Property=""Foreground"" Value=""{DynamicResource TextSecondary}"" />
                                                    <Style.Triggers>
                                                        <DataTrigger Binding=""{Binding IsMmcssOptimized}"" Value=""True"">
                                                            <Setter Property=""Text"" Value=""MMCSS: ОПТИМИЗИРОВАН"" />
                                                            <Setter Property=""Foreground"" Value=""#5EE07A"" />
                                                        </DataTrigger>
                                                    </Style.Triggers>
                                                </Style>
                                            </TextBlock.Style>
                                        </TextBlock>
                                    </Border>
                                </Grid>

                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width=""*"" />
                                        <ColumnDefinition Width=""*"" />
                                        <ColumnDefinition Width=""*"" />
                                        <ColumnDefinition Width=""*"" />
                                    </Grid.ColumnDefinitions>

                                    <!-- Column 1: SystemResponsiveness -->
                                    <Border Grid.Column=""0"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""0,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Отзывчивость (Responsiveness)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding MmcssSystemResponsivenessDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 2: NetworkThrottlingIndex -->
                                    <Border Grid.Column=""1"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Троттлинг сети (NDIS)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding MmcssNetworkThrottlingDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 3: Active MMCSS Tasks -->
                                    <Border Grid.Column=""2"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Задачи Avrt (mmcss_task)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding MmcssActiveTasksDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 4: CiScheduler & Summary -->
                                    <Border Grid.Column=""3"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,0,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Планировщик потоков (CiScheduler)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding MmcssStatusSummary}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>
                                </Grid>
                            </StackPanel>
                        </Border>

" + target;

        content = content.Replace(target, mmcssCard);
        File.WriteAllText(path, content);
        Console.WriteLine("Successfully added MMCSS Diagnostic Card to MainWindow.xaml!");
        return 0;
    }
}
