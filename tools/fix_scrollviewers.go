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

	// Revert sidebar scrollviewer to Grid.Row="1"
	sidebarBad := "<!-- 2. Navigation Items List -->\r\n                <ScrollViewer Grid.Row=\"2\""
	sidebarGood := "<!-- 2. Navigation Items List -->\r\n                <ScrollViewer Grid.Row=\"1\""
	if strings.Contains(s, sidebarBad) {
		s = strings.Replace(s, sidebarBad, sidebarGood, 1)
		fmt.Println("Fixed sidebar scrollviewer!")
	} else {
		// try with LF
		sidebarBadLF := strings.ReplaceAll(sidebarBad, "\r\n", "\n")
		sidebarGoodLF := strings.ReplaceAll(sidebarGood, "\r\n", "\n")
		if strings.Contains(s, sidebarBadLF) {
			s = strings.Replace(s, sidebarBadLF, sidebarGoodLF, 1)
			fmt.Println("Fixed sidebar scrollviewer with LF!")
		}
	}

	// Ensure tweaks scrollviewer is Grid.Row="2"
	tweaksOld := "<!-- Grouped & Virtualized Tweak Cards List -->\r\n                    <ScrollViewer Grid.Row=\"1\""
	tweaksNew := "<!-- Grouped & Virtualized Tweak Cards List -->\r\n                    <ScrollViewer Grid.Row=\"2\""
	if strings.Contains(s, tweaksOld) {
		s = strings.Replace(s, tweaksOld, tweaksNew, 1)
		fmt.Println("Fixed tweaks scrollviewer!")
	} else {
		tweaksOldLF := strings.ReplaceAll(tweaksOld, "\r\n", "\n")
		tweaksNewLF := strings.ReplaceAll(tweaksNew, "\r\n", "\n")
		if strings.Contains(s, tweaksOldLF) {
			s = strings.Replace(s, tweaksOldLF, tweaksNewLF, 1)
			fmt.Println("Fixed tweaks scrollviewer with LF!")
		}
	}

	err = ioutil.WriteFile(`C:\Users\User\Documents\win-tweaker\MainWindow.xaml`, []byte(s), 0644)
	if err != nil {
		panic(err)
	}
	fmt.Println("Done fixing scrollviewers.")
}
