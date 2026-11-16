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
        string card = @"<!-- 2.3b Game Mode & WNF (Noverse gm_effects) Diagnostic Card -->
                        <Border Background=""{DynamicResource CardBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""12"" Padding=""14,12"" Margin=""0,0,0,12"">
                            <StackPanel>
                                <Grid Margin=""0,0,0,8"">
                                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                                        <TextBlock Text=""&#xE7FC;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets"" FontSize=""13"" Foreground=""{DynamicResource AccentBrush}"" VerticalAlignment=""Center"" Margin=""0,0,6,0"" />
                                        <TextBlock Text=""ИГРОВОЙ РЕЖИМ &amp; WNF (NOVERSE GM_EFFECTS)"" Style=""{StaticResource CardHeaderStyle}"" />
                                    </StackPanel>
                                    <Border HorizontalAlignment=""Right"" CornerRadius=""6"" Padding=""8,2"">
                                        <Border.Style>
                                            <Style TargetType=""Border"">
                                                <Setter Property=""Background"" Value=""{DynamicResource BadgeBg}"" />
                                                <Style.Triggers>
                                                    <DataTrigger Binding=""{Binding IsGameModeRunning}"" Value=""True"">
                                                        <Setter Property=""Background"" Value=""#253526"" />
                                                    </DataTrigger>
                                                </Style.Triggers>
                                            </Style>
                                        </Border.Style>
                                        <TextBlock FontSize=""10.5"" FontWeight=""SemiBold"">
                                            <TextBlock.Style>
                                                <Style TargetType=""TextBlock"">
                                                    <Setter Property=""Text"" Value=""ОЖИДАНИЕ ИГРЫ"" />
                                                    <Setter Property=""Foreground"" Value=""{DynamicResource TextSecondary}"" />
                                                    <Style.Triggers>
                                                        <DataTrigger Binding=""{Binding IsGameModeRunning}"" Value=""True"">
                                                            <Setter Property=""Text"" Value=""ИГРА АКТИВНА"" />
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

                                    <!-- Column 1: Зарегистрированная игра -->
                                    <Border Grid.Column=""0"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""0,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Активная игра (PID)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding GameModeActiveGameDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 2: WNF Профиль питания -->
                                    <Border Grid.Column=""1"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""WNF Питание (LowVal)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding GameModePowerProfileDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 3: CPU Sets -->
                                    <Border Grid.Column=""2"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,4,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Выделенные CPU Sets"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding GameModeCpuSetsDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>

                                    <!-- Column 4: Окно в фокусе -->
                                    <Border Grid.Column=""3"" Background=""{DynamicResource BadgeBg}"" BorderBrush=""{DynamicResource CardBorder}"" BorderThickness=""1"" CornerRadius=""8"" Padding=""10,6"" Margin=""4,0,0,0"">
                                        <StackPanel>
                                            <TextBlock Text=""Фокус (Foreground)"" FontSize=""10"" Foreground=""{DynamicResource TextSecondary}"" Margin=""0,0,0,2"" />
                                            <TextBlock Text=""{Binding GameModeForegroundDisplay}"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""{DynamicResource TextPrimary}"" TextTrimming=""CharacterEllipsis"" />
                                        </StackPanel>
                                    </Border>
                                </Grid>
                            </StackPanel>
                        </Border>

                        <!-- 2.4 Bottom Two Cards:";

        if (content.Contains(target) && !content.Contains("NOVERSE GM_EFFECTS"))
        {
            content = content.Replace(target, card);
            File.WriteAllText(path, content, Encoding.UTF8);
            Console.WriteLine("MainWindow.xaml patched with Game Mode Card!");
        }
        else
        {
            Console.WriteLine("Target not found or already patched.");
        }
    }
}
