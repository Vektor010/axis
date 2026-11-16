package main

import (
	"bytes"
	"fmt"
	"io/ioutil"
	"strings"
)

func main() {
	content, err := ioutil.ReadFile("App.xaml.cs")
	if err != nil {
		panic(err)
	}

	text := string(content)

	oldBlock := `                string tabToSelect = null;
                string searchQuery = null;
                bool expandFirstCard = false;`
	oldBlockCRLF := strings.Replace(oldBlock, "\n", "\r\n", -1)

	newBlock := `                string tabToSelect = null;
                string searchQuery = null;
                string categoryId = null;
                int? filterSegment = null;
                bool expandFirstCard = false;`
	newBlockCRLF := strings.Replace(newBlock, "\n", "\r\n", -1)

	if strings.Contains(text, oldBlockCRLF) {
		text = strings.Replace(text, oldBlockCRLF, newBlockCRLF, 1)
	} else if strings.Contains(text, oldBlock) {
		text = strings.Replace(text, oldBlock, newBlock, 1)
	} else {
		panic("oldBlock not found")
	}

	oldArgs := `                        if (args[i] == "--tab" && i + 1 < args.Length)
                        {
                            tabToSelect = args[i + 1];
                        }`
	oldArgsCRLF := strings.Replace(oldArgs, "\n", "\r\n", -1)

	newArgs := `                        if (args[i] == "--tab" && i + 1 < args.Length)
                        {
                            tabToSelect = args[i + 1];
                        }
                        else if (args[i] == "--category" && i + 1 < args.Length)
                        {
                            categoryId = args[i + 1];
                        }
                        else if (args[i] == "--filter-segment" && i + 1 < args.Length)
                        {
                            int fs;
                            if (int.TryParse(args[i + 1], out fs)) filterSegment = fs;
                        }`
	newArgsCRLF := strings.Replace(newArgs, "\n", "\r\n", -1)

	if strings.Contains(text, oldArgsCRLF) {
		text = strings.Replace(text, oldArgsCRLF, newArgsCRLF, 1)
	} else if strings.Contains(text, oldArgs) {
		text = strings.Replace(text, oldArgs, newArgs, 1)
	} else {
		panic("oldArgs not found")
	}

	oldVm := `                    if (expandFirstCard && vm.FilteredTweaks.Count > 0)
                    {
                        vm.FilteredTweaks[0].IsExpanded = true;
                    }`
	oldVmCRLF := strings.Replace(oldVm, "\n", "\r\n", -1)

	newVm := `                    if (!string.IsNullOrEmpty(categoryId))
                    {
                        for (int c = 0; c < vm.Categories.Count; c++)
                        {
                            if (string.Equals(vm.Categories[c].Id, categoryId, StringComparison.OrdinalIgnoreCase))
                            {
                                vm.SelectedCategory = vm.Categories[c];
                                break;
                            }
                        }
                    }

                    if (filterSegment.HasValue)
                    {
                        vm.FilterSegment = filterSegment.Value;
                    }

                    if (expandFirstCard && vm.FilteredTweaks.Count > 0)
                    {
                        vm.FilteredTweaks[0].IsExpanded = true;
                    }`
	newVmCRLF := strings.Replace(newVm, "\n", "\r\n", -1)

	if strings.Contains(text, oldVmCRLF) {
		text = strings.Replace(text, oldVmCRLF, newVmCRLF, 1)
	} else if strings.Contains(text, oldVm) {
		text = strings.Replace(text, oldVm, newVm, 1)
	} else {
		panic("oldVm not found")
	}

	res := []byte(text)
	if !bytes.HasPrefix(res, []byte{0xEF, 0xBB, 0xBF}) {
		res = append([]byte{0xEF, 0xBB, 0xBF}, res...)
	}

	if err := ioutil.WriteFile("App.xaml.cs", res, 0644); err != nil {
		panic(err)
	}

	fmt.Println("App.xaml.cs updated successfully")
}
