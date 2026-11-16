package main

import (
	"fmt"
	"io/ioutil"
	"strings"
)

func main() {
	// 1. Fix MainWindow.xaml
	mwPath := `C:\Users\User\Documents\win-tweaker\MainWindow.xaml`
	b, err := ioutil.ReadFile(mwPath)
	if err != nil {
		panic(err)
	}
	s := string(b)
	s = strings.ReplaceAll(s, "{DynamicResource TextMutedBrush}", "{DynamicResource TextSecondary}")
	err = ioutil.WriteFile(mwPath, []byte(s), 0644)
	if err != nil {
		panic(err)
	}
	fmt.Println("Replaced all TextMutedBrush in MainWindow.xaml with TextSecondary")

	// 2. Add fallback brush in App.xaml
	appPath := `C:\Users\User\Documents\win-tweaker\App.xaml`
	ab, err := ioutil.ReadFile(appPath)
	if err != nil {
		panic(err)
	}
	as := string(ab)
	if !strings.Contains(as, "x:Key=\"TextMutedBrush\"") {
		target := `<Application.Resources>`
		repl := `<Application.Resources>
        <SolidColorBrush x:Key="TextMutedBrush" Color="#A1A1AA" />`
		as = strings.Replace(as, target, repl, 1)
		err = ioutil.WriteFile(appPath, []byte(as), 0644)
		if err != nil {
			panic(err)
		}
		fmt.Println("Added TextMutedBrush fallback to App.xaml")
	}
}
