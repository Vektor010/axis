using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace WinTweaker.Tools
{
    class TweakDef
    {
        public string id;
        public string name;
        public string desc;
        public string group;
        public string category;
        public string risk;
        public string preset;
        public string hive;
        public string path;
        public string nameVal;
        public string type;
        public object apply;
        public string revMode;
        public object revVal;
        public string srcFile;
        public int from;
        public int to;
        public bool needsReview;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(jsonPath);
            var serializer = new JavaScriptSerializer();
            var tweaks = serializer.Deserialize<List<Dictionary<string, object>>>(raw);

            var existingIds = new HashSet<string>();
            foreach (var t in tweaks)
            {
                if (t.ContainsKey("id") && t["id"] != null)
                {
                    existingIds.Add(t["id"].ToString());
                }
            }

            var defs = new List<TweakDef>
            {
                // ==================== PRIVACY & AI ====================
                new TweakDef {
                    id = "privacy-disable-telemetry",
                    name = "Отключить сбор диагностических данных (AllowTelemetry)",
                    desc = "Устанавливает базовый уровень сбора телеметрии Windows (Security / 0), запрещая отправку расширенных данных.",
                    group = "Телеметрия и диагностика", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\DataCollection", nameVal = "AllowTelemetry", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 3, srcFile = "privacy/desc.md", from = 112, to = 118, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-advertising-id",
                    name = "Отключить рекламный идентификатор (DisabledByGroupPolicy)",
                    desc = "Запрещает приложениям использовать индивидуальный рекламный идентификатор пользователя для показа целевой рекламы.",
                    group = "Реклама и идентификаторы", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AdvertisingInfo", nameVal = "DisabledByGroupPolicy", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 122, to = 127, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-app-telemetry",
                    name = "Отключить телеметрию приложений (AITEnable)",
                    desc = "Отключает службу отслеживания сбоев совместимости и телеметрии используемых программ (Application Impact Telemetry).",
                    group = "Телеметрия и диагностика", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppCompat", nameVal = "AITEnable", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "privacy/desc.md", from = 110, to = 115, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-inventory",
                    name = "Отключить инвентаризацию программ (DisableInventory)",
                    desc = "Запрещает сканирование и отправку списка установленных на компьютере приложений и драйверов в Microsoft.",
                    group = "Телеметрия и диагностика", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppCompat", nameVal = "DisableInventory", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 111, to = 116, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-feedback-notifications",
                    name = "Отключить всплывающие запросы отзывов (DoNotShowFeedbackNotifications)",
                    desc = "Запрещает Windows прерывать работу показом всплывающих окон с вопросами об оценке функций ОС.",
                    group = "Уведомления и предложения", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\DataCollection", nameVal = "DoNotShowFeedbackNotifications", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 129, to = 135, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-ceip-sqm",
                    name = "Отключить программу улучшения качества ПО (CEIPEnable)",
                    desc = "Отключает участие системы в программе Customer Experience Improvement Program (SQM / CEIP).",
                    group = "Телеметрия и диагностика", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\SQMClient\Windows", nameVal = "CEIPEnable", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "privacy/desc.md", from = 130, to = 136, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-wer",
                    name = "Отключить отчеты об ошибках (Disabled)",
                    desc = "Отключает создание и передачу дампов падений приложений службой Windows Error Reporting на серверы Microsoft.",
                    group = "Отчеты об ошибках (WER)", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", nameVal = "Disabled", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 326, to = 332, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-wer-logging",
                    name = "Отключить журнал службы отчетов об ошибках (LoggingDisabled)",
                    desc = "Предотвращает постоянную запись детальных логов сбоев процессов службой WER на диск.",
                    group = "Отчеты об ошибках (WER)", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", nameVal = "LoggingDisabled", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 327, to = 333, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-copilot",
                    name = "Отключить Windows Copilot (TurnOffWindowsCopilot)",
                    desc = "Полностью отключает боковую панель и фоновые процессы ИИ-ассистента Microsoft Copilot.",
                    group = "Искусственный интеллект", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", nameVal = "TurnOffWindowsCopilot", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 820, to = 826, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-recall",
                    name = "Запретить анализ активности и скриншоты Recall (DisableAIDataAnalysis)",
                    desc = "Запрещает встроенному ИИ Windows делать фоновые снимки экрана и анализировать журнал действий пользователя.",
                    group = "Искусственный интеллект", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", nameVal = "DisableAIDataAnalysis", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 850, to = 858, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-click-to-do",
                    name = "Отключить распознавание экрана Click to Do (DisableClickToDo)",
                    desc = "Отключает локальный фоновый захват и контекстный OCR-анализ экрана функцией Click to Do.",
                    group = "Искусственный интеллект", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", nameVal = "DisableClickToDo", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 852, to = 860, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-consumer-features",
                    name = "Запретить фоновую установку рекламных приложений (DisableWindowsConsumerFeatures)",
                    desc = "Блокирует автоматическую загрузку промо-приложений и рекламных игр из Microsoft Store при первом входе.",
                    group = "Уведомления и предложения", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\CloudContent", nameVal = "DisableWindowsConsumerFeatures", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 442, to = 448, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-windows-tips",
                    name = "Отключить подсказки и советы Windows (DisableSoftLanding)",
                    desc = "Отключает встроенные всплывающие подсказки Windows Tips по использованию стандартных приложений.",
                    group = "Уведомления и предложения", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\CloudContent", nameVal = "DisableSoftLanding", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "privacy/desc.md", from = 444, to = 450, needsReview = true
                },
                new TweakDef {
                    id = "privacy-disable-gamedvr-app-capture",
                    name = "Отключить фоновую запись клипов GameDVR (AppCaptureEnabled)",
                    desc = "Отключает фоновый захват игрового процесса в оперативной памяти (Xbox GameDVR), устраняя просадки FPS.",
                    group = "Игровые службы Xbox", category = "privacy", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\GameDVR", nameVal = "AppCaptureEnabled", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "privacy/desc.md", from = 870, to = 885, needsReview = true
                },

                // ==================== SECURITY ====================
                new TweakDef {
                    id = "security-disable-vbs",
                    name = "Отключить изоляцию ядра VBS (EnableVirtualizationBasedSecurity)",
                    desc = "Отключает безопасность на основе виртуализации (VBS), устраняя накладные расходы гипервизора и повышая FPS в играх.",
                    group = "Виртуализация и ядро", category = "security", risk = "medium", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", nameVal = "EnableVirtualizationBasedSecurity", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "security/desc.md", from = 1020, to = 1028, needsReview = true
                },
                new TweakDef {
                    id = "security-disable-hvci",
                    name = "Отключить целостность памяти HVCI (HypervisorEnforcedCodeIntegrity)",
                    desc = "Отключает проверку целостности кода в изолированном пространстве гипервизора, снижая задержки прерываний DPC.",
                    group = "Виртуализация и ядро", category = "security", risk = "medium", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", nameVal = "HypervisorEnforcedCodeIntegrity", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "security/desc.md", from = 1020, to = 1028, needsReview = true
                },
                new TweakDef {
                    id = "security-disable-p2p-updates",
                    name = "Отключить P2P-раздачу обновлений (DODownloadMode)",
                    desc = "Запрещает оптимизации доставки Delivery Optimization отдавать обновления другим компьютерам в локальной сети и интернете.",
                    group = "Центр обновления", category = "security", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", nameVal = "DODownloadMode", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "security/desc.md", from = 1093, to = 1100, needsReview = true
                },
                new TweakDef {
                    id = "security-disable-driver-searching",
                    name = "Запретить поиск драйверов через Windows Update (DontSearchWindowsUpdate)",
                    desc = "Предотвращает автоматическую перезапись установленных оптимизированных видео- и аудиодрайверов центром обновления.",
                    group = "Драйверы и обновления", category = "security", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\DriverSearching", nameVal = "DontSearchWindowsUpdate", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "security/desc.md", from = 490, to = 498, needsReview = true
                },
                new TweakDef {
                    id = "security-disable-downloads-blocking",
                    name = "Отключить метку блокировки скачанных файлов (SaveZoneInformation)",
                    desc = "Отключает принудительное добавление потока Zone.Identifier (Mark of the Web) к файлам, убирая предупреждения о блокировке.",
                    group = "Проводник и вложения", category = "security", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Attachments", nameVal = "SaveZoneInformation", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 2, srcFile = "security/desc.md", from = 1145, to = 1152, needsReview = true
                },
                new TweakDef {
                    id = "security-uac-dimming-desktop",
                    name = "Отключить затемнение экрана при запросе прав UAC (PromptOnSecureDesktop)",
                    desc = "Переводит диалоговые окна контроля учетных записей на обычный рабочий стол без мерцания и задержки переключения защищенного стола.",
                    group = "Контроль учетных записей (UAC)", category = "security", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", nameVal = "PromptOnSecureDesktop", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "security/desc.md", from = 597, to = 608, needsReview = true
                },
                new TweakDef {
                    id = "security-uac-admin-approval",
                    name = "Режим подтверждения встроенного администратора (FilterAdministratorToken)",
                    desc = "Запускает приложения под встроенной учетной записью администратора с полными правами без блокирующих запросов.",
                    group = "Контроль учетных записей (UAC)", category = "security", risk = "medium", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", nameVal = "FilterAdministratorToken", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 0, srcFile = "security/desc.md", from = 518, to = 530, needsReview = true
                },
                new TweakDef {
                    id = "security-defender-disable-realtime",
                    name = "Отключить защиту реального времени Defender (DisableRealtimeMonitoring)",
                    desc = "Отключает непрерывное сканирование каждого открываемого или создаваемого файла антивирусом Windows Defender.",
                    group = "Windows Defender", category = "security", risk = "medium", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", nameVal = "DisableRealtimeMonitoring", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "security/desc.md", from = 295, to = 305, needsReview = true
                },
                new TweakDef {
                    id = "security-defender-disable-spynet",
                    name = "Отключить облачную защиту Defender (SpynetReporting)",
                    desc = "Отключает отправку сведений о подозрительных файлах в облачную службу Microsoft MAPS/SpyNet.",
                    group = "Windows Defender", category = "security", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Spynet", nameVal = "SpynetReporting", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 2, srcFile = "security/desc.md", from = 300, to = 310, needsReview = true
                },
                new TweakDef {
                    id = "security-defender-disable-smartscreen",
                    name = "Отключить SmartScreen для системы (EnableSmartScreen)",
                    desc = "Отключает сетевую проверку запускаемых программ через фильтр репутации Windows Defender SmartScreen.",
                    group = "Windows Defender", category = "security", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\System", nameVal = "EnableSmartScreen", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "security/desc.md", from = 280, to = 290, needsReview = true
                }
            };

            int addedCount = 0;
            foreach (var d in defs)
            {
                if (existingIds.Contains(d.id))
                {
                    Console.WriteLine("Skip existing: " + d.id);
                    continue;
                }

                var t = new Dictionary<string, object>();
                t["id"] = d.id;
                t["name_ru"] = d.name;
                t["description_ru"] = d.desc;
                t["group"] = d.group;
                t["category"] = d.category;
                t["type"] = "toggle";
                t["risk"] = d.risk;
                t["presets"] = new List<string> { d.preset };
                t["min_build"] = null;
                t["needs_review"] = d.needsReview;

                var src = new Dictionary<string, object>();
                src["file"] = d.srcFile;
                src["line_from"] = d.from;
                src["line_to"] = d.to;
                t["source"] = src;

                var regItem = new Dictionary<string, object>();
                regItem["hive"] = d.hive;
                regItem["path"] = d.path;
                regItem["name"] = d.nameVal;
                regItem["type"] = d.type;

                regItem["apply"] = d.apply;

                var revObj = new Dictionary<string, object>();
                revObj["mode"] = d.revMode;
                revObj["value"] = d.revVal;
                regItem["revert"] = revObj;

                t["registry"] = new List<Dictionary<string, object>> { regItem };

                tweaks.Add(t);
                existingIds.Add(d.id);
                addedCount++;
                Console.WriteLine("Added tweak: " + d.id);
            }

            string updatedJson = serializer.Serialize(tweaks);
            File.WriteAllText(jsonPath, updatedJson, System.Text.Encoding.UTF8);
            Console.WriteLine("Done! Added {0} tweaks. Total tweaks: {1}", addedCount, tweaks.Count);
        }
    }
}
