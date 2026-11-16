using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\MainWindow.xaml";
        string content = File.ReadAllText(path, Encoding.UTF8);

        string target = "<!-- 2.4 Bottom Two Cards:";
        string card = @"<!-- 2.3c GPU Scheduling & Flip Queue (Noverse query_hwsch) Diagnostic Card -->
                        <Border Background=""{DynamicResource CardBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""12"" Padding=""14,12"" Margin=""0,0,0,12"">
                            <StackPanel>
                                <Grid Margin=""0,0,0,8"">
                                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                                        <TextBlock Text=""&#xE7F4;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets"" FontSize=""13"" Foreground=""{DynamicResource AccentBrush}"" VerticalAlignment=""Center"" Margin=""0,0,6,0"" />
                                        <TextBlock Text=""ПЛАНИРОВАНИЕ GPU &amp; ОЧЕРЕДЬ КАДРОВ (NOVERSE QUERY_HWSCH)"" Style=""{StaticResource CardHeaderStyle}"" />
                                    </StackPanel>
                                    <Border HorizontalAlignment=""Right"" CornerRadius=""6"" Padding=""8,2"">
                                        <Border.Style>
                                            <Style TargetType=""Border"">
                                                <Setter Property=""Background"" Value=""{DynamicResource BadgeBg}"" />
                                                <Style.Triggers>
                                                    <DataTrigger Binding=""{Binding IsHagsActive}"" Value=""True"">
                                                        <Setter Property=""Background"" Value=""#253526"" />
                                                    </DataTrigger>
                                                </Style.Triggers>
                                            </Style>
                                        </Border.Style>
                                        <TextBlock FontSize=""10.5"" FontWeight=""SemiBold"">
                                            <TextBlock.Style>
                                                <Style TargetType=""TextBlock"">
                                                    <Setter Property=""Text"" Value=""HAGS: ВЫКЛЮЧЕН"" />
                                                    <Setter Property=""Foreground"" Value=""{DynamicResource TextSecondary}"" />
                                                    <Style.Triggers>
                                                        <DataTrigger Binding=""{Binding IsHagsActive}"" Value=""True"">
                                                            <Setter Property=""Text"" Value=""HAGS: АКТИВЕН"" />
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

                                    <!-- Column 1: Видеоадаптер -->
                                    <Border Grid.Column=""0"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""0,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Активный GPU"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding GpuSpec}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 2: HAGS (WDDM 2.9 HWSCH) -->
                                    <Border Grid.Column=""1"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Аппаратное планирование (HWSCH)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding GpuHagsDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 3: Hardware Flip Queue (WDDM 3.0) -->
                                    <Border Grid.Column=""2"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Очередь кадров (HWFLIPQUEUE)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,4,0"" />
                                            <TextBlock Text=""{Binding GpuFlipQueueDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 4: Сводка D3DKMT -->
                                    <Border Grid.Column=""3"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,0,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Статус D3DKMT"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding GpuHwschSummary}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>
                                </Grid>
                            </StackPanel>
                        </Border>

                        <!-- 2.4 Bottom Two Cards:";

        if (content.Contains(target) && !content.Contains("NOVERSE QUERY_HWSCH"))
        {
            content = content.Replace(target, card);
            File.WriteAllText(path, content, Encoding.UTF8);
            Console.WriteLine("MainWindow.xaml patched with GPU HWSCH card!");
        }
        else
        {
            Console.WriteLine("Target not found or already patched.");
        }
    }
}
