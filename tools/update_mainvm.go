package main

import (
	"bytes"
	"fmt"
	"io/ioutil"
	"strings"
)

func main() {
	content, err := ioutil.ReadFile("src/ViewModels/MainViewModel.cs")
	if err != nil {
		panic(err)
	}

	text := string(content)

	// 1. In SelectedCategory setter:
	oldSelCat := `                    this.OnPropertyChanged("SelectedCategory");
                    this.ApplyFilter();`
	oldSelCatCRLF := strings.Replace(oldSelCat, "\n", "\r\n", -1)

	newSelCat := `                    this.OnPropertyChanged("SelectedCategory");
                    this.ApplyFilter();
                    this.UpdateSummary();`
	newSelCatCRLF := strings.Replace(newSelCat, "\n", "\r\n", -1)

	if !strings.Contains(text, oldSelCatCRLF) && !strings.Contains(text, oldSelCat) {
		panic("oldSelCat not found")
	}

	if strings.Contains(text, oldSelCatCRLF) {
		text = strings.Replace(text, oldSelCatCRLF, newSelCatCRLF, 1)
	} else {
		text = strings.Replace(text, oldSelCat, newSelCat, 1)
	}

	// 2. In UpdateSummary():
	oldSummary := `            this.TotalTweaksCount = total;
            this.EnabledTweaksCount = enabled;
            this.DisabledTweaksCount = disabled;
            this.NeedsReviewCount = review;

            this.SegmentAllText = string.Format("Все ({0})", total);
            this.SegmentEnabledText = string.Format("Включённые ({0})", enabled);
            this.SegmentDisabledText = string.Format("Выключенные ({0})", disabled);
            this.SegmentNeedsReviewText = string.Format("Требуют проверки ({0})", review);`
	oldSummaryCRLF := strings.Replace(oldSummary, "\n", "\r\n", -1)

	newSummary := `            this.TotalTweaksCount = total;
            this.EnabledTweaksCount = enabled;
            this.DisabledTweaksCount = disabled;
            this.NeedsReviewCount = review;

            // Локальные счётчики для сегментов выбранной категории
            string catId = (this._selectedCategory != null) ? this._selectedCategory.Id : "all";
            int catTotal = 0;
            int catEnabled = 0;
            int catDisabled = 0;
            int catReview = 0;

            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._allTweaks[i];
                if (catId != "all" && !string.Equals(tw.Category, catId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                catTotal++;
                if (tw.Status == TweakStatus.Enabled)
                {
                    catEnabled++;
                }
                else if (tw.Status == TweakStatus.Disabled)
                {
                    catDisabled++;
                }
                if (tw.NeedsReview)
                {
                    catReview++;
                }
            }

            this.SegmentAllText = string.Format("Все ({0})", catTotal);
            this.SegmentEnabledText = string.Format("Включённые ({0})", catEnabled);
            this.SegmentDisabledText = string.Format("Выключенные ({0})", catDisabled);
            this.SegmentNeedsReviewText = string.Format("Требуют проверки ({0})", catReview);`
	newSummaryCRLF := strings.Replace(newSummary, "\n", "\r\n", -1)

	if strings.Contains(text, oldSummaryCRLF) {
		text = strings.Replace(text, oldSummaryCRLF, newSummaryCRLF, 1)
	} else if strings.Contains(text, oldSummary) {
		text = strings.Replace(text, oldSummary, newSummary, 1)
	} else {
		panic("oldSummary not found")
	}

	// 3. In OnSelectCategory:
	oldSelectCat := `            CategoryItemViewModel cat = obj as CategoryItemViewModel;
            if (cat != null)
            {
                this.SelectedCategory = cat;`
	oldSelectCatCRLF := strings.Replace(oldSelectCat, "\n", "\r\n", -1)

	newSelectCat := `            CategoryItemViewModel cat = obj as CategoryItemViewModel;
            if (cat != null)
            {
                if (this.SelectedCategory == cat)
                {
                    this.SelectedCategory = null;
                }
                else
                {
                    this.SelectedCategory = cat;
                }`
	newSelectCatCRLF := strings.Replace(newSelectCat, "\n", "\r\n", -1)

	if strings.Contains(text, oldSelectCatCRLF) {
		text = strings.Replace(text, oldSelectCatCRLF, newSelectCatCRLF, 1)
	} else if strings.Contains(text, oldSelectCat) {
		text = strings.Replace(text, oldSelectCat, newSelectCat, 1)
	} else {
		panic("oldSelectCat not found")
	}

	res := []byte(text)
	if !bytes.HasPrefix(res, []byte{0xEF, 0xBB, 0xBF}) {
		res = append([]byte{0xEF, 0xBB, 0xBF}, res...)
	}

	if err := ioutil.WriteFile("src/ViewModels/MainViewModel.cs", res, 0644); err != nil {
		panic(err)
	}

	fmt.Println("MainViewModel.cs updated successfully")
}
