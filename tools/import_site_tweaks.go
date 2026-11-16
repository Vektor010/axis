package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
)

func main() {
	fpath := `C:\Users\User\Documents\win-tweaker\data\tweaks.json`
	data, err := ioutil.ReadFile(fpath)
	if err != nil {
		panic(err)
	}

	var rawList []map[string]interface{}
	err = json.Unmarshal(data, &rawList)
	if err != nil {
		panic(err)
	}

	existing := make(map[string]bool)
	for _, item := range rawList {
		id, _ := item["id"].(string)
		existing[id] = true
	}

	newTweaks := []map[string]interface{}{
		{
			"id":             "security-defender-realtime",
			"name_ru":        "Отключить постоянную защиту Defender в реальном времени",
			"description_ru": "Отключает фоновый антивирусный мониторинг файловой системы в реальном времени (DisableRealtimeMonitoring).",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "high",
			"presets":        []string{},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection",
					"name":  "DisableRealtimeMonitoring",
					"type":  "REG_DWORD",
					"apply": 1,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 0,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-defender-behavior-monitoring",
			"name_ru":        "Отключить мониторинг поведения процессов Defender",
			"description_ru": "Отключает эвристический анализ подозрительной активности процессов (DisableBehaviorMonitoring).",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "high",
			"presets":        []string{},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection",
					"name":  "DisableBehaviorMonitoring",
					"type":  "REG_DWORD",
					"apply": 1,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 0,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-defender-ioav-protection",
			"name_ru":        "Отключить сканирование загружаемых файлов и вложений",
			"description_ru": "Отключает проверку скачиваемых из браузера файлов через интерфейс IOAV (DisableIOAVProtection).",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "medium",
			"presets":        []string{},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection",
					"name":  "DisableIOAVProtection",
					"type":  "REG_DWORD",
					"apply": 1,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 0,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-defender-on-access-protection",
			"name_ru":        "Отключить проверку файлов при обращении (On-Access)",
			"description_ru": "Запрещает постоянный перехват файловых дескрипторов при запуске программ (DisableOnAccessProtection).",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "high",
			"presets":        []string{},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection",
					"name":  "DisableOnAccessProtection",
					"type":  "REG_DWORD",
					"apply": 1,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 0,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-defender-pua-protection",
			"name_ru":        "Отключить блокировку нежелательного ПО (PUA Protection)",
			"description_ru": "Отключает автоматическую блокировку потенциально нежелательных утилит и программ (PUAProtection = 0).",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows Defender",
					"name":  "PUAProtection",
					"type":  "REG_DWORD",
					"apply": 0,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 1,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-smartscreen-shell",
			"name_ru":        "Отключить фильтр SmartScreen в Windows и Проводнике",
			"description_ru": "Устраняет задержки запуска скачанных .exe файлов и диалоговые окна 'Система защитила ваш ПК'.",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows\\System",
					"name":  "EnableSmartScreen",
					"type":  "REG_DWORD",
					"apply": 0,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 1,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-defender-cloud-block",
			"name_ru":        "Отключить облачную блокировку Defender (Block at First Sight)",
			"description_ru": "Запрещает отправку метаданных новых файлов в облако Microsoft перед их запуском (DisableBlockAtFirstSeen).",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet",
					"name":  "DisableBlockAtFirstSeen",
					"type":  "REG_DWORD",
					"apply": 1,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 0,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-defender-submit-samples",
			"name_ru":        "Запретить отправку файлов-образцов в Microsoft",
			"description_ru": "Устанавливает режим 'Никогда не отправлять образцы' (SubmitSamplesConsent = 2), защищая приватные файлы от выгрузки на серверы Microsoft.",
			"category":       "security",
			"group":          "Защитник Windows (Defender)",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet",
					"name":  "SubmitSamplesConsent",
					"type":  "REG_DWORD",
					"apply": 2,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 1,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 274,
				"line_to":   310,
			},
		},
		{
			"id":             "security-vbs-hvci-disable",
			"name_ru":        "Отключить Virtualization-Based Security (VBS / Core Isolation / HVCI)",
			"description_ru": "Отключает изоляцию ядра внутри гипервизора Hyper-V, повышая 1% Low FPS и устраняя задержки EPT/SLAT.",
			"category":       "security",
			"group":          "Виртуализация и ядро",
			"type":           "toggle",
			"risk":           "medium",
			"presets":        []string{},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SYSTEM\\CurrentControlSet\\Control\\DeviceGuard",
					"name":  "EnableVirtualizationBasedSecurity",
					"type":  "REG_DWORD",
					"apply": 0,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 1,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 986,
				"line_to":   1025,
			},
		},
		{
			"id":             "security-wpbt-disable",
			"name_ru":        "Блокировка BIOS-инжектора WPBT (Windows Platform Binary Table)",
			"description_ru": "Запрещает внедрение мусорных OEM-программ и служб материнской платы (Armoury Crate, Dragon Center) через ACPI.",
			"category":       "security",
			"group":          "Виртуализация и ядро",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SYSTEM\\CurrentControlSet\\Control\\Session Manager",
					"name":  "DisableWpbtExecution",
					"type":  "REG_DWORD",
					"apply": 1,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 0,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 853,
				"line_to":   930,
			},
		},
		{
			"id":             "security-tdr-delay",
			"name_ru":        "Увеличение таймаута сброса видеодрайвера (TDR Delay = 8 сек)",
			"description_ru": "Предотвращает вылеты тяжелых игр при долгой компиляции шейдеров видеокартой.",
			"category":       "security",
			"group":          "Стабильность GPU",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers",
					"name":  "TdrDelay",
					"type":  "REG_DWORD",
					"apply": 8,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 2,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 1177,
				"line_to":   1230,
			},
		},
		{
			"id":             "security-uac-prompt-no-dimming",
			"name_ru":        "Отключить затемнение экрана при запросе UAC (PromptOnSecureDesktop)",
			"description_ru": "Диалоговое окно контроля учетных записей открывается мгновенно без блокировки графических буферов DWM.",
			"category":       "security",
			"group":          "Контроль учетных записей (UAC)",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System",
					"name":  "PromptOnSecureDesktop",
					"type":  "REG_DWORD",
					"apply": 0,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 1,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "security/desc.md",
				"line_from": 598,
				"line_to":   615,
			},
		},
		{
			"id":             "system-delivery-optimization-p2p",
			"name_ru":        "Отключить P2P-раздачу обновлений Windows (Delivery Optimization)",
			"description_ru": "Запрещает фоновую загрузку и раздачу фрагментов обновлений соседям по сети (устраняет скачки пинга в играх).",
			"category":       "system",
			"group":          "Центр обновления (Windows Update)",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SOFTWARE\\Policies\\Microsoft\\Windows\\DeliveryOptimization",
					"name":  "DODownloadMode",
					"type":  "REG_DWORD",
					"apply": 0,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 1,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "system/desc.md",
				"line_from": 170,
				"line_to":   200,
			},
		},
		{
			"id":             "system-disable-paging-executive",
			"name_ru":        "Запретить сброс компонентов ядра в файл подкачки (DisablePagingExecutive)",
			"description_ru": "Блокирует драйверы и компоненты ядра в физической оперативной памяти (для систем с 16+ ГБ RAM).",
			"category":       "system",
			"group":          "Память и ядро",
			"type":           "toggle",
			"risk":           "low",
			"presets":        []string{"recommended"},
			"needs_review":   false,
			"registry": []map[string]interface{}{
				{
					"hive":  "HKLM",
					"path":  "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management",
					"name":  "DisablePagingExecutive",
					"type":  "REG_DWORD",
					"apply": 1,
					"revert": map[string]interface{}{
						"mode":  "set",
						"value": 0,
					},
				},
			},
			"source": map[string]interface{}{
				"file":      "system/desc.md",
				"line_from": 3316,
				"line_to":   3350,
			},
		},
	}

	added := 0
	for _, tw := range newTweaks {
		id, _ := tw["id"].(string)
		if !existing[id] {
			rawList = append(rawList, tw)
			existing[id] = true
			added++
		}
	}

	outData, err := json.MarshalIndent(rawList, "", "  ")
	if err != nil {
		panic(err)
	}

	err = ioutil.WriteFile(fpath, outData, 0644)
	if err != nil {
		panic(err)
	}

	fmt.Printf("Successfully imported %d new tweaks into %s (Total: %d)\n", added, fpath, len(rawList))
}
