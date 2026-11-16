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
	lines := strings.Split(string(b), "\r\n")

	startIdx := -1
	endIdx := -1
	for i, l := range lines {
		if strings.Contains(l, "<!-- Left: Title, Badges, and 1-line Description -->") {
			startIdx = i
		}
		if startIdx >= 0 && strings.Contains(l, "<CheckBox IsChecked=\"{Binding IsChecked, Mode=TwoWay}\"") {
			endIdx = i
			break
		}
	}

	if startIdx < 0 || endIdx < 0 {
		fmt.Printf("Range not found: start=%d end=%d\n", startIdx, endIdx)
		return
	}

	fmt.Printf("Replacing lines %d to %d\n", startIdx+1, endIdx)

	replacement := []string{
		`                                                <!-- Left: Badges, Title, and 1-line Description -->`,
		`                                                <StackPanel Grid.Column="0" Margin="0,0,12,0">`,
		`                                                    <!-- Badges Row: Group, Key, Recommended, Risk, Warning -->`,
		`                                                    <WrapPanel Orientation="Horizontal" VerticalAlignment="Center" Margin="0,0,0,3">`,
		`                                                        <!-- Group Badge -->`,
		`                                                        <Border CornerRadius="4"`,
		`                                                                Background="{DynamicResource BadgeBg}"`,
		`                                                                Padding="6,2"`,
		`                                                                VerticalAlignment="Center"`,
		`                                                                Margin="0,0,6,0">`,
		`                                                            <TextBlock Text="{Binding Group}"`,
		`                                                                       FontSize="10"`,
		`                                                                       FontWeight="SemiBold"`,
		`                                                                       Foreground="{DynamicResource AccentBrush}" />`,
		`                                                        </Border>`,
		``,
		`                                                        <!-- Parameter Key Badge -->`,
		`                                                        <Border CornerRadius="4"`,
		`                                                                Background="{DynamicResource SearchBg}"`,
		`                                                                BorderBrush="{DynamicResource CardBorder}"`,
		`                                                                BorderThickness="1"`,
		`                                                                Padding="5,1"`,
		`                                                                VerticalAlignment="Center"`,
		`                                                                Margin="0,0,6,0"`,
		`                                                                Visibility="{Binding HasRegistryName, Converter={StaticResource BoolToVis}}">`,
		`                                                            <TextBlock Text="{Binding RegistryName}"`,
		`                                                                       FontFamily="Consolas, Cascadia Code, Courier New"`,
		`                                                                       FontSize="10.5"`,
		`                                                                       Foreground="{DynamicResource TextSecondary}" />`,
		`                                                        </Border>`,
		``,
		`                                                        <!-- Recommended Badge -->`,
		`                                                        <Border CornerRadius="4"`,
		`                                                                Background="#183620"`,
		`                                                                BorderBrush="#22C55E"`,
		`                                                                BorderThickness="1"`,
		`                                                                Padding="6,2"`,
		`                                                                VerticalAlignment="Center"`,
		`                                                                Margin="0,0,6,0"`,
		`                                                                Visibility="{Binding IsRecommended, Converter={StaticResource BoolToVis}}">`,
		`                                                            <TextBlock Text="★ Рекомендуется"`,
		`                                                                       FontSize="10"`,
		`                                                                       FontWeight="Bold"`,
		`                                                                       Foreground="#4ADE80" />`,
		`                                                        </Border>`,
		``,
		`                                                        <!-- Risk Badge (Shown ONLY for Medium and High) -->`,
		`                                                        <Border CornerRadius="4"`,
		`                                                                Background="{DynamicResource BadgeBg}"`,
		`                                                                Padding="6,2"`,
		`                                                                VerticalAlignment="Center"`,
		`                                                                Margin="0,0,6,0"`,
		`                                                                Visibility="{Binding ShowRiskBadge, Converter={StaticResource BoolToVis}}">`,
		`                                                            <TextBlock Text="{Binding RiskText}"`,
		`                                                                       FontSize="10"`,
		`                                                                       FontWeight="SemiBold"`,
		`                                                                       Foreground="{Binding RiskColor}" />`,
		`                                                        </Border>`,
		``,
		`                                                        <!-- Small amber icon with tooltip on compact row -->`,
		`                                                        <TextBlock Text="&#xE7BA;"`,
		`                                                                   FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets"`,
		`                                                                   FontSize="12"`,
		`                                                                   Foreground="{DynamicResource WarningBrush}"`,
		`                                                                   ToolTip="Требует проверки"`,
		`                                                                   VerticalAlignment="Center"`,
		`                                                                   Margin="0,0,6,0"`,
		`                                                                   Visibility="{Binding NeedsReview, Converter={StaticResource BoolToVis}}" />`,
		`                                                    </WrapPanel>`,
		``,
		`                                                    <!-- Tweak Title -->`,
		`                                                    <TextBlock Text="{Binding Name}"`,
		`                                                               FontSize="13.5"`,
		`                                                               FontWeight="SemiBold"`,
		`                                                               Foreground="{DynamicResource TextPrimary}"`,
		`                                                               Margin="0,2,0,0" />`,
		``,
		`                                                    <!-- 1-line Description (Hidden if empty) -->`,
		`                                                    <TextBlock Text="{Binding Description}"`,
		`                                                               FontSize="12"`,
		`                                                               Foreground="{DynamicResource TextSecondary}"`,
		`                                                               TextTrimming="CharacterEllipsis"`,
		`                                                               Margin="0,3,0,0"`,
		`                                                               Visibility="{Binding HasDescription, Converter={StaticResource BoolToVis}}" />`,
		`                                                </StackPanel>`,
		``,
		`                                                <!-- Right: Status Badge & Live Switch & Expand Chevron -->`,
		`                                                <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">`,
		`                                                    <TextBlock Text="{Binding StatusBadgeText}"`,
		`                                                               FontSize="11.5"`,
		`                                                               FontWeight="SemiBold"`,
		`                                                               Foreground="{Binding StatusBadgeColor}"`,
		`                                                               VerticalAlignment="Center"`,
		`                                                               Margin="0,0,10,0" />`,
	}

	newLines := append(lines[:startIdx], replacement...)
	newLines = append(newLines, lines[endIdx:]...)

	res := strings.Join(newLines, "\r\n")
	err = ioutil.WriteFile(`C:\Users\User\Documents\win-tweaker\MainWindow.xaml`, []byte(res), 0644)
	if err != nil {
		panic(err)
	}
	fmt.Println("Successfully replaced card template in MainWindow.xaml!")
}
