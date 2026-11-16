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
	
	target1 := `Content="Применить рекомендуемые"`
	if !strings.Contains(s, target1) {
		fmt.Println("target1 not found!")
	} else {
		s = strings.Replace(s, target1, `Content="{Binding ApplyRecommendedText}"`, 1)
		fmt.Println("target1 replaced!")
	}

	target2 := `<ScrollViewer Grid.Row="1"`
	if !strings.Contains(s, target2) {
		fmt.Println("target2 not found!")
	} else {
		s = strings.Replace(s, target2, `<ScrollViewer Grid.Row="2"`, 1)
		fmt.Println("target2 replaced!")
	}

	err = ioutil.WriteFile(`C:\Users\User\Documents\win-tweaker\MainWindow.xaml`, []byte(s), 0644)
	if err != nil {
		panic(err)
	}
	fmt.Println("Done updating MainWindow.xaml")
}
