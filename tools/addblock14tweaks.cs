using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock14Tool
{
    public class RevertObj
    {
        public string mode { get; set; }
        public object value { get; set; }
    }

    public class RegItem
    {
        public string hive { get; set; }
        public string path { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public object apply { get; set; }
        public RevertObj revert { get; set; }
    }

    public class SourceObj
    {
        public string file { get; set; }
        public int line_from { get; set; }
        public int line_to { get; set; }
    }

    public class TweakItem
    {
        public string id { get; set; }
        public string name_ru { get; set; }
        public string description_ru { get; set; }
        public string group { get; set; }
        public string category { get; set; }
        public string type { get; set; }
        public string risk { get; set; }
        public List<string> presets { get; set; }
        public int? min_build { get; set; }
        public bool needs_review { get; set; }
        public SourceObj source { get; set; }
        public List<RegItem> registry { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string tweaksPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string json = File.ReadAllText(tweaksPath, Encoding.UTF8);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<TweakItem>>(json);
            Console.WriteLine("Loaded existing tweaks count: " + list.Count);

            var existingIds = new HashSet<string>();
            foreach (var t in list)
            {
                existingIds.Add(t.id);
            }

            var newTweaks = new List<TweakItem>();

            // 1. system-storage-sense-recycle-bin-threshold
            newTweaks.Add(new TweakItem {
                id = "system-storage-sense-recycle-bin-threshold",
                name_ru = "Отключить автоочистку Корзины службой Storage Sense (ConfigStorageSenseRecycleBinCleanupThreshold)",
                description_ru = "Запрещает службе контроля памяти Storage Sense автоматически удалять файлы из Корзины по истечении времени.",
                group = "Контроль памяти (Storage Sense)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8676, line_to = 8681 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\StorageSense",
                        name = "ConfigStorageSenseRecycleBinCleanupThreshold",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 30 }
                    }
                }
            });

            // 2. system-storage-sense-downloads-threshold
            newTweaks.Add(new TweakItem {
                id = "system-storage-sense-downloads-threshold",
                name_ru = "Отключить автоочистку Загрузок службой Storage Sense (ConfigStorageSenseDownloadsCleanupThreshold)",
                description_ru = "Запрещает службе контроля памяти автоматически удалять неиспользуемые файлы из папки 'Загрузки'.",
                group = "Контроль памяти (Storage Sense)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8676, line_to = 8682 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\StorageSense",
                        name = "ConfigStorageSenseDownloadsCleanupThreshold",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 30 }
                    }
                }
            });

            // 3. security-wtds-notify-password-reuse
            newTweaks.Add(new TweakItem {
                id = "security-wtds-notify-password-reuse",
                name_ru = "Отключить предупреждения о повторном использовании паролей (NotifyPasswordReuse)",
                description_ru = "Отключает фоновый перехват и анализ повторного ввода корпоративных или системных паролей службой Web Threat Defense.",
                group = "Защита от сетевых угроз (WTDS)",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 277, line_to = 285 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\WTDS\Components",
                        name = "NotifyPasswordReuse",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 4. security-wtds-notify-unsafe-app
            newTweaks.Add(new TweakItem {
                id = "security-wtds-notify-unsafe-app",
                name_ru = "Отключить предупреждения о потенциально нежелательных программах (NotifyUnsafeApp)",
                description_ru = "Блокирует показ диалоговых предупреждений службы защиты браузера при скачивании или запуске утилит.",
                group = "Защита от сетевых угроз (WTDS)",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 277, line_to = 286 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\WTDS\Components",
                        name = "NotifyUnsafeApp",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 5. network-sandbox-no-printer-redirection
            newTweaks.Add(new TweakItem {
                id = "network-sandbox-no-printer-redirection",
                name_ru = "Запретить доступ к принтерам из Windows Sandbox (AllowPrinterRedirection)",
                description_ru = "Изолирует песочницу Windows Sandbox от сетевых и локальных принтеров хост-системы.",
                group = "Песочница Windows Sandbox",
                category = "network",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "network/desc.md", line_from = 1756, line_to = 1759 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Sandbox",
                        name = "AllowPrinterRedirection",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 6. security-powershell-disable-module-logging
            newTweaks.Add(new TweakItem {
                id = "security-powershell-disable-module-logging",
                name_ru = "Отключить логирование модулей PowerShell (EnableModuleLogging)",
                description_ru = "Отключает фоновую запись событий загрузки и работы модулей PowerShell в журнал Event Viewer, снижая нагрузку на диск.",
                group = "Среда PowerShell и скрипты",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 668, line_to = 672 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging",
                        name = "EnableModuleLogging",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 7. security-powershell-disable-scriptblock-logging
            newTweaks.Add(new TweakItem {
                id = "security-powershell-disable-scriptblock-logging",
                name_ru = "Отключить логирование блоков скриптов PowerShell (EnableScriptBlockLogging)",
                description_ru = "Запрещает подробное протоколирование содержимого каждого выполняемого скрипта PowerShell в журналы аудита.",
                group = "Среда PowerShell и скрипты",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 668, line_to = 674 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging",
                        name = "EnableScriptBlockLogging",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 8. security-powershell-disable-transcription
            newTweaks.Add(new TweakItem {
                id = "security-powershell-disable-transcription",
                name_ru = "Отключить транскрипцию сеансов PowerShell (EnableTranscripting)",
                description_ru = "Отключает постоянную запись текстовой расшифровки всех вводимых и выводимых команд PowerShell в текстовые файлы.",
                group = "Среда PowerShell и скрипты",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 668, line_to = 673 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription",
                        name = "EnableTranscripting",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 9. security-edge-disable-smartscreen-filter
            newTweaks.Add(new TweakItem {
                id = "security-edge-disable-smartscreen-filter",
                name_ru = "Отключить фильтр SmartScreen в классическом браузере Edge (EnabledV9)",
                description_ru = "Отключает проверку посещаемых адресов веб-сайтов через службу SmartScreen PhishingFilter.",
                group = "Браузер Microsoft Edge",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 277, line_to = 280 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\MicrosoftEdge\PhishingFilter",
                        name = "EnabledV9",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 10. security-edge-chromium-smartscreen-off
            newTweaks.Add(new TweakItem {
                id = "security-edge-chromium-smartscreen-off",
                name_ru = "Отключить SmartScreen в браузере Edge Chromium (SmartScreenEnabled)",
                description_ru = "Запрещает браузеру Microsoft Edge отправлять хэши скачиваемых файлов и URL-адреса на серверы SmartScreen.",
                group = "Браузер Microsoft Edge",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 277, line_to = 282 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Edge",
                        name = "SmartScreenEnabled",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 11. security-disable-wtds-service
            newTweaks.Add(new TweakItem {
                id = "security-disable-wtds-service",
                name_ru = "Отключить службу Web Threat Defense (ServiceEnabled)",
                description_ru = "Полностью отключает компоненты службы защиты от сетевых угроз WTDS, снижая фоновое использование CPU.",
                group = "Защита от сетевых угроз (WTDS)",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 277, line_to = 283 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\WTDS\Components",
                        name = "ServiceEnabled",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 12. system-start-hide-recently-added-apps
            newTweaks.Add(new TweakItem {
                id = "system-start-hide-recently-added-apps",
                name_ru = "Скрыть список 'Недавно добавленные' в меню Пуск (HideRecentlyAddedApps)",
                description_ru = "Убирает раздел недавно установленных программ из верхней части меню 'Пуск', сохраняя аккуратный вид списка приложений.",
                group = "Меню Пуск",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9165 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "HideRecentlyAddedApps",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 13. system-ntfs-disable-compression
            newTweaks.Add(new TweakItem {
                id = "system-ntfs-disable-compression",
                name_ru = "Запретить сжатие томов NTFS (NtfsDisableCompression)",
                description_ru = "Отключает возможность создания новых сжатых файлов на дисках NTFS, устраняя микрозадержки при распаковке на лету.",
                group = "Файловая система NTFS",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8518, line_to = 8520 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"System\CurrentControlSet\Control\FileSystem",
                        name = "NtfsDisableCompression",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 14. system-ntfs-disable-encryption
            newTweaks.Add(new TweakItem {
                id = "system-ntfs-disable-encryption",
                name_ru = "Отключить шифрование файлов EFS на томах NTFS (NtfsDisableEncryption)",
                description_ru = "Отключает поддержку шифрования Encrypting File System (EFS), освобождая системные ресурсы драйвера NTFS.",
                group = "Файловая система NTFS",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8520, line_to = 8522 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"System\CurrentControlSet\Control\FileSystem",
                        name = "NtfsDisableEncryption",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 15. system-start-no-new-app-alert
            newTweaks.Add(new TweakItem {
                id = "system-start-no-new-app-alert",
                name_ru = "Отключить оповещение 'Установлено новое приложение' (NoNewAppAlert)",
                description_ru = "Блокирует показ уведомлений о появлении новых установленных программ при выходе на рабочий стол.",
                group = "Уведомления и оболочка",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9166 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "NoNewAppAlert",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 16. system-start-hide-most-used-apps
            newTweaks.Add(new TweakItem {
                id = "system-start-hide-most-used-apps",
                name_ru = "Скрыть список 'Часто используемые' в меню Пуск (ShowOrHideMostUsedApps)",
                description_ru = "Убирает автоматический список наиболее популярных приложений из меню 'Пуск'.",
                group = "Меню Пуск",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9162 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "ShowOrHideMostUsedApps",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 2 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 17. system-start-no-mfu-programs-list
            newTweaks.Add(new TweakItem {
                id = "system-start-no-mfu-programs-list",
                name_ru = "Отключить формирование списка частых программ MFU (NoStartMenuMFUprogramsList)",
                description_ru = "Запрещает проводнику непрерывно подсчитывать частоту запусков и сохранять список MFU в реестре.",
                group = "Меню Пуск",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9163 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "NoStartMenuMFUprogramsList",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 18. privacy-paint-disable-image-creator
            newTweaks.Add(new TweakItem {
                id = "privacy-paint-disable-image-creator",
                name_ru = "Отключить Image Creator в Paint (DisableImageCreator)",
                description_ru = "Блокирует онлайн-генератор изображений на основе ИИ в приложении Paint, предотвращая сетевые запросы к облаку Microsoft.",
                group = "Искусственный интеллект и приложения",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 824, line_to = 829 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Paint",
                        name = "DisableImageCreator",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 19. privacy-paint-disable-cocreator
            newTweaks.Add(new TweakItem {
                id = "privacy-paint-disable-cocreator",
                name_ru = "Отключить Cocreator в Paint (DisableCocreator)",
                description_ru = "Отключает функции соавторства NPU/ИИ в графическом редакторе Paint.",
                group = "Искусственный интеллект и приложения",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 824, line_to = 830 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Paint",
                        name = "DisableCocreator",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 20. privacy-paint-disable-generative-fill
            newTweaks.Add(new TweakItem {
                id = "privacy-paint-disable-generative-fill",
                name_ru = "Отключить генеративную заливку в Paint (DisableGenerativeFill)",
                description_ru = "Отключает функцию облачной генеративной заливки выделенных фрагментов в Paint.",
                group = "Искусственный интеллект и приложения",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 824, line_to = 831 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Paint",
                        name = "DisableGenerativeFill",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 21. privacy-appcompat-turn-off-switchback
            newTweaks.Add(new TweakItem {
                id = "privacy-appcompat-turn-off-switchback",
                name_ru = "Отключить движок SwitchBack AppCompat (SbEnable)",
                description_ru = "Отключает механизм SwitchBack AppCompat Engine, снижая задержки исполнения старых исполняемых файлов.",
                group = "Совместимость приложений",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1411, line_to = 1414 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\AppCompat",
                        name = "SbEnable",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 22. peripheral-touchpad-leave-on-with-mouse
            newTweaks.Add(new TweakItem {
                id = "peripheral-touchpad-leave-on-with-mouse",
                name_ru = "Отключать тачпад при подключении мыши (LeaveOnWithMouse)",
                description_ru = "Автоматически отключает сенсорную панель ноутбука Precision Touchpad при наличии подключенной мыши, предотвращая случайные касания ладонью в играх.",
                group = "Сенсорная панель Precision Touchpad",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1431, line_to = 1437 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                        name = "LeaveOnWithMouse",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 23. peripheral-touchpad-disable-taps
            newTweaks.Add(new TweakItem {
                id = "peripheral-touchpad-disable-taps",
                name_ru = "Отключить касания тачпада как клики (TapsEnabled)",
                description_ru = "Отключает реакцию тачпада на легкие касания (тап), требуя физического нажатия на кнопку для совершения клика.",
                group = "Сенсорная панель Precision Touchpad",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1431, line_to = 1439 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                        name = "TapsEnabled",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 24. peripheral-touchpad-disable-two-finger-tap
            newTweaks.Add(new TweakItem {
                id = "peripheral-touchpad-disable-two-finger-tap",
                name_ru = "Отключить правый клик двумя пальцами (TwoFingerTapEnabled)",
                description_ru = "Отключает вызов контекстного меню касанием двумя пальцами по тачпаду.",
                group = "Сенсорная панель Precision Touchpad",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1431, line_to = 1441 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                        name = "TwoFingerTapEnabled",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 25. peripheral-touchpad-disable-pinch-zoom
            newTweaks.Add(new TweakItem {
                id = "peripheral-touchpad-disable-pinch-zoom",
                name_ru = "Отключить масштабирование щипком (ZoomEnabled)",
                description_ru = "Отключает жест масштабирования двумя пальцами на тачпаде, предотвращая случайные скачки зума в веб-браузерах.",
                group = "Сенсорная панель Precision Touchpad",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1431, line_to = 1445 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                        name = "ZoomEnabled",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            int added = 0;
            foreach (var tweak in newTweaks)
            {
                if (!existingIds.Contains(tweak.id))
                {
                    list.Add(tweak);
                    existingIds.Add(tweak.id);
                    added++;
                    Console.WriteLine("Added: " + tweak.id);
                }
                else
                {
                    Console.WriteLine("Skipping already existing: " + tweak.id);
                }
            }

            Console.WriteLine("Total added: " + added);
            Console.WriteLine("New total: " + list.Count);

            string updatedJson = FormatJson(serializer.Serialize(list));
            File.WriteAllText(tweaksPath, updatedJson, new UTF8Encoding(false));
            Console.WriteLine("Updated " + tweaksPath + " successfully.");
        }

        private static string FormatJson(string json)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', ++indent * 2));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', --indent * 2));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 2));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
