using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock12Tool
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

            // 1. privacy-disable-onesettings-downloads
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-onesettings-downloads",
                name_ru = "Отключить загрузку настроек OneSettings (DisableOneSettingsDownloads)",
                description_ru = "Запрещает Windows периодически связываться со службой OneSettings для фонового скачивания динамических конфигураций и правил телеметрии.",
                group = "Телеметрия и фоновые службы",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1431, line_to = 1434 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\DataCollection",
                        name = "DisableOneSettingsDownloads",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 2. privacy-disable-find-my-device
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-find-my-device",
                name_ru = "Отключить службу 'Поиск устройства' (AllowFindMyDevice)",
                description_ru = "Отключает фоновую геолокацию и отслеживание устройства Find My Device, предотвращая сбор координат компьютером.",
                group = "Конфиденциальность и геолокация",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1454, line_to = 1457 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\FindMyDevice",
                        name = "AllowFindMyDevice",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 3. privacy-disable-font-providers
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-font-providers",
                name_ru = "Отключить онлайн-поставщиков шрифтов (EnableFontProviders)",
                description_ru = "Запрещает Windows периодически опрашивать онлайн-каталоги шрифтов и загружать внешние начертания через Интернет.",
                group = "Облачные службы Windows",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1373, line_to = 1376 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\System",
                        name = "EnableFontProviders",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 4. privacy-disable-thumbnail-cache-network
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-thumbnail-cache-network",
                name_ru = "Отключить кэширование эскизов в thumbs.db по сети (DisableThumbsDBOnNetworkFolders)",
                description_ru = "Запрещает проводнику создавать скрытые файлы thumbs.db в сетевых каталогах, снижая сетевой трафик и риск утечки данных.",
                group = "Проводник и кэширование",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1385, line_to = 1388 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "DisableThumbsDBOnNetworkFolders",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 5. privacy-disable-thumbnail-cache-local
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-thumbnail-cache-local",
                name_ru = "Отключить создание постоянного кэша эскизов (NoThumbnailCache)",
                description_ru = "Отключает фоновое накопление постоянного кэша миниатюр файлов, устраняя сохранение артефактов просмотренных изображений.",
                group = "Проводник и кэширование",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1385, line_to = 1389 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "NoThumbnailCache",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 6. privacy-disable-app-compat-engine
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-app-compat-engine",
                name_ru = "Отключить движок совместимости приложений (DisableEngine)",
                description_ru = "Отключает фоновый механизм Application Compatibility Engine, уменьшая задержки запуска и фоновую телеметрию приложений.",
                group = "Совместимость приложений",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1411, line_to = 1415 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\AppCompat",
                        name = "DisableEngine",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 7. privacy-disable-program-compat-assistant
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-program-compat-assistant",
                name_ru = "Отключить помощник по совместимости программ (DisablePCA)",
                description_ru = "Отключает службу Program Compatibility Assistant (PCA), устраняя всплывающие окна и проверки совместимости после закрытия программ.",
                group = "Совместимость приложений",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1411, line_to = 1416 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\AppCompat",
                        name = "DisablePCA",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 8. privacy-disable-compat-ui
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-compat-ui",
                name_ru = "Отключить интерфейс обнаружения проблем совместимости (DisablePcaUI)",
                description_ru = "Блокирует показ диалоговых окон о несовместимости программ и драйверов, устраняя прерывания рабочего процесса.",
                group = "Совместимость приложений",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1411, line_to = 1421 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\AppCompat",
                        name = "DisablePcaUI",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 9. privacy-disable-install-tracing
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-install-tracing",
                name_ru = "Отключить трассировку инсталляций (DisableInstallTracing)",
                description_ru = "Отключает сбор данных инвентаризации при установке программного обеспечения, экономя ресурсы процессора и накопителя.",
                group = "Совместимость приложений",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1411, line_to = 1417 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\AppCompat",
                        name = "DisableInstallTracing",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 10. privacy-disable-file-history
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-file-history",
                name_ru = "Отключить службу 'История файлов' (DisableFileHistory)",
                description_ru = "Отключает фоновое создание резервных копий и отслеживание журнала изменений файлов через NTFS Change Journal.",
                group = "Фоновые службы и бэкапы",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1197, line_to = 1200 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\FileHistory",
                        name = "Disabled",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 11. privacy-disable-search-history
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-search-history",
                name_ru = "Отключить сохранение истории поиска в проводнике (DisableSearchHistory)",
                description_ru = "Запрещает проводнику сохранять поисковые запросы в строке поиска и отображать предыдущие варианты поиска.",
                group = "Проводник и поиск",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1184, line_to = 1190 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Explorer",
                        name = "DisableSearchHistory",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 12. privacy-disable-publish-user-activities
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-publish-user-activities",
                name_ru = "Запретить публикацию действий пользователя (PublishUserActivities)",
                description_ru = "Блокирует локальную фиксацию и запись хронологии действий пользователя в журнал активности ОС.",
                group = "Журнал действий и история",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1184, line_to = 1188 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\System",
                        name = "PublishUserActivities",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 13. privacy-disable-upload-user-activities
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-upload-user-activities",
                name_ru = "Запретить отправку действий пользователя в облако (UploadUserActivities)",
                description_ru = "Предотвращает передачу истории запусков программ и действий пользователя на облачные сервера Microsoft.",
                group = "Журнал действий и история",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1184, line_to = 1189 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\System",
                        name = "UploadUserActivities",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 14. security-disable-remote-assistance
            newTweaks.Add(new TweakItem {
                id = "security-disable-remote-assistance",
                name_ru = "Отключить удаленного помощника (fAllowToGetHelp)",
                description_ru = "Запрещает создание сеансов удаленной помощи (Remote Assistance), закрывая уязвимые порты и каналы доступа к системе.",
                group = "Удаленный доступ и сеть",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1029, line_to = 1032 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\policies\Microsoft\Windows NT\Terminal Services",
                        name = "fAllowToGetHelp",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 15. security-restrict-unauthenticated-rpc
            newTweaks.Add(new TweakItem {
                id = "security-restrict-unauthenticated-rpc",
                name_ru = "Ограничить неаутентифицированные RPC-клиенты (RestrictRemoteClients)",
                description_ru = "Требует обязательной аутентификации для входящих RPC-вызовов, предотвращая атаки на отказ в обслуживании и повышение привилегий.",
                group = "Сетевая безопасность и RPC",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1029, line_to = 1036 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows NT\Rpc",
                        name = "RestrictRemoteClients",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 16. security-disallow-projection-to-pc
            newTweaks.Add(new TweakItem {
                id = "security-disallow-projection-to-pc",
                name_ru = "Запретить проецирование экрана на этот ПК (AllowProjectionToPC)",
                description_ru = "Блокирует беспроводное подключение других устройств в качестве экрана через службу Wireless Display / Miracast.",
                group = "Беспроводные подключения",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1029, line_to = 1037 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Connect",
                        name = "AllowProjectionToPC",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 17. security-disable-mdm-registration
            newTweaks.Add(new TweakItem {
                id = "security-disable-mdm-registration",
                name_ru = "Отключить регистрацию в MDM (DisableRegistration)",
                description_ru = "Запрещает регистрацию компьютера в системах удаленного управления мобильными устройствами (Mobile Device Management).",
                group = "Управление устройствами MDM",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1211, line_to = 1214 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\CurrentVersion\MDM",
                        name = "DisableRegistration",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 18. system-turn-off-aero-shake
            newTweaks.Add(new TweakItem {
                id = "system-turn-off-aero-shake",
                name_ru = "Отключить сворачивание окон встряхиванием мыши (NoWindowMinimizingShortcuts)",
                description_ru = "Отключает жест Aero Shake, который случайно сворачивает все фоновые окна при резком движении мышью за заголовок.",
                group = "Управление окнами",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8459, line_to = 8462 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "NoWindowMinimizingShortcuts",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 19. system-prevent-index-on-battery
            newTweaks.Add(new TweakItem {
                id = "system-prevent-index-on-battery",
                name_ru = "Приостановить индексацию при работе от батареи (PreventIndexOnBattery)",
                description_ru = "Экономит заряд аккумулятора и снижает фоновую нагрузку на накопитель, отключая индексацию поиска при питании от батареи.",
                group = "Индексация и поиск",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9154 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                        name = "PreventIndexOnBattery",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 20. system-disable-web-search-in-start
            newTweaks.Add(new TweakItem {
                id = "system-disable-web-search-in-start",
                name_ru = "Отключить результаты поиска в Интернете (ConnectedSearchUseWeb)",
                description_ru = "Убирает веб-результаты и подсказки Bing из строки поиска Windows, делая поиск мгновенным и локальным.",
                group = "Индексация и поиск",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9156 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                        name = "ConnectedSearchUseWeb",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 21. system-turn-off-user-tracking
            newTweaks.Add(new TweakItem {
                id = "system-turn-off-user-tracking",
                name_ru = "Отключить отслеживание запуска программ (NoInstrumentation)",
                description_ru = "Отключает встроенное профилирование частоты запуска программ в меню 'Пуск', снижая фоновые дисковые операции.",
                group = "Меню Пуск и отслеживание",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9164 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "NoInstrumentation",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 22. system-disable-notification-center
            newTweaks.Add(new TweakItem {
                id = "system-disable-notification-center",
                name_ru = "Отключить Центр уведомлений (DisableNotificationCenter)",
                description_ru = "Полностью отключает боковую панель Центра уведомлений и Центр действий, снижая потребление оперативной памяти.",
                group = "Уведомления и оповещения",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8407 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "DisableNotificationCenter",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 23. system-turn-off-toast-notifications
            newTweaks.Add(new TweakItem {
                id = "system-turn-off-toast-notifications",
                name_ru = "Отключить всплывающие уведомления (NoToastApplicationNotification)",
                description_ru = "Отключает всплывающие toast-уведомления от приложений Windows, предотвращая свертывание и прерывания полноэкранных игр.",
                group = "Уведомления и оповещения",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8413, line_to = 8416 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                        name = "NoToastApplicationNotification",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 24. peripheral-printers-disable-http
            newTweaks.Add(new TweakItem {
                id = "peripheral-printers-disable-http",
                name_ru = "Отключить печать по протоколу HTTP (DisableHTTPPrinting)",
                description_ru = "Запрещает отправку заданий печати через веб-протокол HTTP/IPP, закрывая потенциальный вектор атак по локальной сети.",
                group = "Принтеры и печать",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1829, line_to = 1832 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows NT\Printers",
                        name = "DisableHTTPPrinting",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 25. peripheral-printers-no-add-printer
            newTweaks.Add(new TweakItem {
                id = "peripheral-printers-no-add-printer",
                name_ru = "Запретить автоматическое добавление сетевых принтеров (NoAddPrinter)",
                description_ru = "Блокирует автоматическое обнаружение и установку сторонних принтеров из локальной сети без ведома пользователя.",
                group = "Принтеры и печать",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1829, line_to = 1835 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "NoAddPrinter",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
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
