package main

import (
	"fmt"
	"io/ioutil"
	"strings"
)

func main() {
	b, err := ioutil.ReadFile(`C:\Users\User\Documents\win-tweaker\MainWindow.xaml`)
	if err != nil {
		panic(err)
	}
	s := string(b)

	// 1. Add RowDefinition for Active Section Header Banner
	gridTarget := `<Grid Visibility="{Binding IsTweaksSelected, Converter={StaticResource BoolToVis}}">
                    <Grid.RowDefinitions>
                        <!-- Screen Toolbar (Segments, Summary, Batch Buttons) -->
                        <RowDefinition Height="Auto" />
                        <!-- Grouped & Virtualized List -->
                        <RowDefinition Height="*" />
                    </Grid.RowDefinitions>`

	gridRepl := `<Grid Visibility="{Binding IsTweaksSelected, Converter={StaticResource BoolToVis}}">
                    <Grid.RowDefinitions>
                        <!-- Active Section Header Banner -->
                        <RowDefinition Height="Auto" />
                        <!-- Screen Toolbar (Segments, Summary, Batch Buttons) -->
                        <RowDefinition Height="Auto" />
                        <!-- Grouped & Virtualized List -->
                        <RowDefinition Height="*" />
                    </Grid.RowDefinitions>

                    <!-- Active Section Header Banner -->
                    <Border Grid.Row="0"
                            Background="{DynamicResource CardBg}"
                            BorderBrush="{DynamicResource CardBorder}"
                            BorderThickness="1"
                            CornerRadius="10"
                            Padding="14,10"
                            Margin="0,0,0,8">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*" />
                                <ColumnDefinition Width="Auto" />
                            </Grid.ColumnDefinitions>

                            <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                                <Border Width="32" Height="32" CornerRadius="8" Background="{DynamicResource BadgeBg}" Margin="0,0,12,0" VerticalAlignment="Center">
                                    <TextBlock Text="{Binding CurrentSectionGlyph}"
                                               FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets"
                                               FontSize="15"
                                               Foreground="{DynamicResource AccentBrush}"
                                               HorizontalAlignment="Center"
                                               VerticalAlignment="Center" />
                                </Border>
                                <StackPanel VerticalAlignment="Center">
                                    <TextBlock Text="{Binding CurrentSectionTitle}"
                                               FontSize="15"
                                               FontWeight="Bold"
                                               Foreground="{DynamicResource TextPrimary}" />
                                    <TextBlock Text="{Binding CurrentSectionStatsText}"
                                               FontSize="11.5"
                                               Foreground="{DynamicResource TextMutedBrush}"
                                               Margin="0,2,0,0" />
                                </StackPanel>
                            </StackPanel>
                        </Grid>
                    </Border>`

	// Normalizing CRLF
	s = replaceNormalized(s, gridTarget, gridRepl, "Grid RowDefinitions + Header Banner")

	// 2. Change Tweaks Toolbar from Grid.Row="0" to Grid.Row="1"
	toolbarTarget := `<!-- Tweaks Toolbar -->
                    <Border Grid.Row="0"`
	toolbarRepl := `<!-- Tweaks Toolbar -->
                    <Border Grid.Row="1"`
	s = replaceNormalized(s, toolbarTarget, toolbarRepl, "Tweaks Toolbar Row=1")

	// 3. Change Recommended button text to binding
	btnTarget := `Content="Применить рекомендуемые"`
	btnRepl := `Content="{Binding ApplyRecommendedText}"`
	s = replaceNormalized(s, btnTarget, btnRepl, "ApplyRecommendedText binding")

	// 4. Change Tweaks ScrollViewer to Grid.Row="2"
	scrollTarget := `<!-- Grouped & Virtualized Tweak Cards List -->
                    <ScrollViewer Grid.Row="1"`
	scrollRepl := `<!-- Grouped & Virtualized Tweak Cards List -->
                    <ScrollViewer Grid.Row="2"`
	s = replaceNormalized(s, scrollTarget, scrollRepl, "Tweaks ScrollViewer Row=2")

	// 5. Replace Tweak Card Header Layout
	cardTarget := `                                                <!-- Left: Title, Badges, and 1-line Description -->
                                                <StackPanel Grid.Column="0" Margin="0,0,12,0">
                                                    <WrapPanel Orientation="Horizontal" VerticalAlignment="Center">
                                                        <TextBlock Text="{Binding Name}"
                                                                   FontSize="13.5"
                                                                   FontWeight="SemiBold"
                                                                   Foreground="{DynamicResource TextPrimary}"
                                                                   VerticalAlignment="Center"
                                                                   Margin="0,0,8,0" />

                                                        <!-- Risk Badge (Shown ONLY for Medium and High) -->
                                                        <Border CornerRadius="6"
                                                                Background="{DynamicResource BadgeBg}"
                                                                Padding="6,2"
                                                                VerticalAlignment="Center"
                                                                Margin="0,0,6,0"
                                                                Visibility="{Binding ShowRiskBadge, Converter={StaticResource BoolToVis}}">
                                                            <TextBlock Text="{Binding RiskText}"
                                                                       FontSize="10"
                                                                       FontWeight="SemiBold"
                                                                       Foreground="{Binding RiskColor}" />
                                                        </Border>

                                                        <!-- Small amber icon with tooltip on compact row -->
                                                        <TextBlock Text="&#xE7BA;"
                                                                   FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets"
                                                                   FontSize="12"
                                                                   Foreground="{DynamicResource WarningBrush}"
                                                                   ToolTip="Требует проверки"
                                                                   VerticalAlignment="Center"
                                                                   Margin="0,0,6,0"
                                                                   Visibility="{Binding NeedsReview, Converter={StaticResource BoolToVis}}" />
                                                    </WrapPanel>

                                                    <!-- 1-line Description (Hidden if empty) -->
                                                    <TextBlock Text="{Binding Description}"
                                                               FontSize="12"
                                                               Foreground="{DynamicResource TextSecondary}"
                                                               TextTrimming="CharacterEllipsis"
                                                               Margin="0,4,0,0"
                                                               Visibility="{Binding HasDescription, Converter={StaticResource BoolToVis}}" />
                                                </StackPanel>

                                                <!-- Right: Status Badge & Live Switch & Expand Chevron -->
                                                <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
                                                    <TextBlock Text="{Binding StatusBadgeText}"
                                                               FontSize="11.5"
                                                               Foreground="{DynamicResource TextSecondary}"
                                                               VerticalAlignment="Center"
                                                               Margin="0,0,10,0" />`

	cardRepl := `                                                <!-- Left: Badges, Title, and 1-line Description -->
                                                <StackPanel Grid.Column="0" Margin="0,0,12,0">
                                                    <!-- Badges Row: Group, Key, Recommended, Risk, Warning -->
                                                    <WrapPanel Orientation="Horizontal" VerticalAlignment="Center" Margin="0,0,0,3">
                                                        <!-- Group Badge -->
                                                        <Border CornerRadius="4"
                                                                Background="{DynamicResource BadgeBg}"
                                                                Padding="6,2"
                                                                VerticalAlignment="Center"
                                                                Margin="0,0,6,0">
                                                            <TextBlock Text="{Binding Group}"
                                                                       FontSize="10"
                                                                       FontWeight="SemiBold"
                                                                       Foreground="{DynamicResource AccentBrush}" />
                                                        </Border>

                                                        <!-- Parameter Key Badge -->
                                                        <Border CornerRadius="4"
                                                                Background="{DynamicResource SearchBg}"
                                                                BorderBrush="{DynamicResource CardBorder}"
                                                                BorderThickness="1"
                                                                Padding="5,1"
                                                                VerticalAlignment="Center"
                                                                Margin="0,0,6,0"
                                                                Visibility="{Binding HasRegistryName, Converter={StaticResource BoolToVis}}">
                                                            <TextBlock Text="{Binding RegistryName}"
                                                                       FontFamily="Consolas, Cascadia Code, Courier New"
                                                                       FontSize="10.5"
                                                                       Foreground="{DynamicResource TextSecondary}" />
                                                        </Border>

                                                        <!-- Recommended Badge -->
                                                        <Border CornerRadius="4"
                                                                Background="#183620"
                                                                BorderBrush="#22C55E"
                                                                BorderThickness="1"
                                                                Padding="6,2"
                                                                VerticalAlignment="Center"
                                                                Margin="0,0,6,0"
                                                                Visibility="{Binding IsRecommended, Converter={StaticResource BoolToVis}}">
                                                            <TextBlock Text="★ Рекомендуется"
                                                                       FontSize="10"
                                                                       FontWeight="Bold"
                                                                       Foreground="#4ADE80" />
                                                        </Border>

                                                        <!-- Risk Badge (Shown ONLY for Medium and High) -->
                                                        <Border CornerRadius="4"
                                                                Background="{DynamicResource BadgeBg}"
                                                                Padding="6,2"
                                                                VerticalAlignment="Center"
                                                                Margin="0,0,6,0"
                                                                Visibility="{Binding ShowRiskBadge, Converter={StaticResource BoolToVis}}">
                                                            <TextBlock Text="{Binding RiskText}"
                                                                       FontSize="10"
                                                                       FontWeight="SemiBold"
                                                                       Foreground="{Binding RiskColor}" />
                                                        </Border>

                                                        <!-- Small amber icon with tooltip on compact row -->
                                                        <TextBlock Text="&#xE7BA;"
                                                                   FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets"
                                                                   FontSize="12"
                                                                   Foreground="{DynamicResource WarningBrush}"
                                                                   ToolTip="Требует проверки"
                                                                   VerticalAlignment="Center"
                                                                   Margin="0,0,6,0"
                                                                   Visibility="{Binding NeedsReview, Converter={StaticResource BoolToVis}}" />
                                                    </WrapPanel>

                                                    <!-- Tweak Title -->
                                                    <TextBlock Text="{Binding Name}"
                                                               FontSize="13.5"
                                                               FontWeight="SemiBold"
                                                               Foreground="{DynamicResource TextPrimary}"
                                                               Margin="0,2,0,0" />

                                                    <!-- 1-line Description (Hidden if empty) -->
                                                    <TextBlock Text="{Binding Description}"
                                                               FontSize="12"
                                                               Foreground="{DynamicResource TextSecondary}"
                                                               TextTrimming="CharacterEllipsis"
                                                               Margin="0,3,0,0"
                                                               Visibility="{Binding HasDescription, Converter={StaticResource BoolToVis}}" />
                                                </StackPanel>

                                                <!-- Right: Status Badge & Live Switch & Expand Chevron -->
                                                <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
                                                    <TextBlock Text="{Binding StatusBadgeText}"
                                                               FontSize="11.5"
                                                               FontWeight="SemiBold"
                                                               Foreground="{Binding StatusBadgeColor}"
                                                               VerticalAlignment="Center"
                                                               Margin="0,0,10,0" />`
	s = replaceNormalized(s, cardTarget, cardRepl, "Card Header Badges & Colored Status")

	err = ioutil.WriteFile(`C:\Users\User\Documents\win-tweaker\MainWindow.xaml`, []byte(s), 0644)
	if err != nil {
		panic(err)
	}
	fmt.Println("All UI updates applied successfully!")
}

func replaceNormalized(content, target, replacement, label string) string {
	if strings.Contains(content, target) {
		fmt.Printf("[%s] Replaced exact\n", label)
		return strings.Replace(content, target, replacement, 1)
	}
	targetCRLF := strings.ReplaceAll(target, "\r\n", "\n")
	targetCRLF = strings.ReplaceAll(targetCRLF, "\n", "\r\n")
	replCRLF := strings.ReplaceAll(replacement, "\r\n", "\n")
	replCRLF = strings.ReplaceAll(replCRLF, "\n", "\r\n")
	if strings.Contains(content, targetCRLF) {
		fmt.Printf("[%s] Replaced CRLF\n", label)
		return strings.Replace(content, targetCRLF, replCRLF, 1)
	}
	targetLF := strings.ReplaceAll(target, "\r\n", "\n")
	replLF := strings.ReplaceAll(replacement, "\r\n", "\n")
	if strings.Contains(content, targetLF) {
		fmt.Printf("[%s] Replaced LF\n", label)
		return strings.Replace(content, targetLF, replLF, 1)
	}
	fmt.Printf("[%s] ERROR: Target not found!\n", label)
	return content
}
