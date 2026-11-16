using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock11Tool
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

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            List<TweakItem> list = serializer.Deserialize<List<TweakItem>>(json);

            Console.WriteLine("Current tweaks count: " + list.Count);

            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in list) existingIds.Add(t.id);

            List<TweakItem> newTweaks = new List<TweakItem>();

            // 1. system-disable-bsod-autoreboot
            newTweaks.Add(new TweakItem {
                id = "system-disable-bsod-autoreboot",
                name_ru = "Отключить автоматическую перезагрузку при BSoD (AutoReboot)",
                description_ru = "Запрещает автоматическую перезагрузку при сбое ядра BSoD, позволяя изучить код ошибки и упавший драйвер.",
                group = "Диагностика и загрузка",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9395, line_to = 9397 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\CrashControl",
                        name = "AutoReboot",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 2. system-auto-end-tasks
            newTweaks.Add(new TweakItem {
                id = "system-auto-end-tasks",
                name_ru = "Автоматическое закрытие зависших программ при выключении (AutoEndTasks)",
                description_ru = "Автоматически принудительно завершает зависшие процессы без показа экрана ожидания завершения программ.",
                group = "Диагностика и загрузка",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9354, line_to = 9358 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Control Panel\Desktop",
                        name = "AutoEndTasks",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "1" } },
                        revert = new RevertObj { mode = "set", value = "0" }
                    }
                }
            });

            // 3. system-hung-app-timeout
            newTweaks.Add(new TweakItem {
                id = "system-hung-app-timeout",
                name_ru = "Уменьшить таймаут зависших приложений (HungAppTimeout)",
                description_ru = "Сокращает время ожидания отклика окна перед определением процесса как зависшего с 5000 мс до 1000 мс.",
                group = "Диагностика и загрузка",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9354, line_to = 9357 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Control Panel\Desktop",
                        name = "HungAppTimeout",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "1000" } },
                        revert = new RevertObj { mode = "set", value = "5000" }
                    }
                }
            });

            // 4. system-wait-to-kill-service-timeout
            newTweaks.Add(new TweakItem {
                id = "system-wait-to-kill-service-timeout",
                name_ru = "Уменьшить таймаут ожидания служб при выключении (WaitToKillServiceTimeout)",
                description_ru = "Сокращает максимальное время ожидания остановки служб диспетчером SCM перед выключением ПК с 5000 мс до 2000 мс.",
                group = "Диагностика и загрузка",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9351, line_to = 9353 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control",
                        name = "WaitToKillServiceTimeout",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "2000" } },
                        revert = new RevertObj { mode = "set", value = "5000" }
                    }
                }
            });

            // 5. system-disable-web-search-bing
            newTweaks.Add(new TweakItem {
                id = "system-disable-web-search-bing",
                name_ru = "Отключить веб-поиск Bing в меню Пуск (DisableWebSearch)",
                description_ru = "Запрещает отправку поисковых запросов в интернет через Bing при поиске в Windows, ускоряя поиск локальных файлов.",
                group = "Поиск и индексация",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9156, line_to = 9158 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                        name = "DisableWebSearch",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 6. system-prevent-remote-queries
            newTweaks.Add(new TweakItem {
                id = "system-prevent-remote-queries",
                name_ru = "Запретить удаленный опрос поискового индекса (PreventRemoteQueries)",
                description_ru = "Запрещает сетевым клиентам обращаться к локальному поисковому индексу ПК при сетевом доступе к папкам.",
                group = "Поиск и индексация",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9151, line_to = 9153 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                        name = "PreventRemoteQueries",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 7. security-tdr-delay-increase
            newTweaks.Add(new TweakItem {
                id = "security-tdr-delay-increase",
                name_ru = "Увеличить порог таймаута видеодрайвера (TdrDelay)",
                description_ru = "Увеличивает допустимое время отклика видеокарты с 2 до 8 секунд, предотвращая ложные сбросы видеодрайвера при тяжелом рендеринге и в играх.",
                group = "Стабильность графической подсистемы",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 1201, line_to = 1203 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                        name = "TdrDelay",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 8 } },
                        revert = new RevertObj { mode = "set", value = 2 }
                    }
                }
            });

            // 8. security-tdr-ddi-delay-increase
            newTweaks.Add(new TweakItem {
                id = "security-tdr-ddi-delay-increase",
                name_ru = "Увеличить задержку выхода из потока драйвера (TdrDdiDelay)",
                description_ru = "Увеличивает допустимое время выхода потоков из видеодрайвера с 5 до 10 секунд, исключая появление синих экранов VIDEO_TDR_FAILURE (0x116).",
                group = "Стабильность графической подсистемы",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 1204, line_to = 1206 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                        name = "TdrDdiDelay",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 10 } },
                        revert = new RevertObj { mode = "set", value = 5 }
                    }
                }
            });

            // 9. security-disable-wpbt-execution
            newTweaks.Add(new TweakItem {
                id = "security-disable-wpbt-execution",
                name_ru = "Запретить выполнение OEM-руткитов WPBT (DisableWpbtExecution)",
                description_ru = "Блокирует выполнение исполняемых файлов из таблицы ACPI WPBT при старте smss.exe, предотвращая принудительную установку OEM-софта вендоров плат.",
                group = "Целостность и изоляция",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 866, line_to = 868 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Session Manager",
                        name = "DisableWpbtExecution",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 10. security-disable-system-restore
            newTweaks.Add(new TweakItem {
                id = "security-disable-system-restore",
                name_ru = "Отключить создание точек восстановления (DisableSR)",
                description_ru = "Отключает фоновое создание теневых снимков диска System Restore, снижая нагрузку на накопитель и освобождая место.",
                group = "Целостность и изоляция",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 1113, line_to = 1115 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows NT\SystemRestore",
                        name = "DisableSR",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 11. security-disable-password-reveal
            newTweaks.Add(new TweakItem {
                id = "security-disable-password-reveal",
                name_ru = "Скрыть кнопку показа пароля в полях ввода (DisablePasswordReveal)",
                description_ru = "Удаляет значок «глаз» в полях ввода паролей интерфейса Windows CredUI, исключая подглядывание введенного пароля.",
                group = "Целостность и изоляция",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 1163, line_to = 1165 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\CredUI",
                        name = "DisablePasswordReveal",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 12. network-nic-disable-vmq
            newTweaks.Add(new TweakItem {
                id = "network-nic-disable-vmq",
                name_ru = "Отключить очереди виртуальных машин сетевой карты (*VMQ)",
                description_ru = "Отключает поддержку Virtual Machine Queue (VMQ) в чипе сетевой карты, снижая задержку обработки пакетов на хосте без виртуализации.",
                group = "Аппаратная разгрузка NIC",
                category = "network",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "network/desc.md", line_from = 1579, line_to = 1581 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "*VMQ",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 13. network-nic-rss-vmq-preference
            newTweaks.Add(new TweakItem {
                id = "network-nic-rss-vmq-preference",
                name_ru = "Приоритет аппаратного RSS перед VMQ (*RssOrVmqPreference)",
                description_ru = "Указывает сетевому драйверу отдавать приоритет многопоточному масштабированию Receive Side Scaling (RSS), а не VMQ.",
                group = "Аппаратная разгрузка NIC",
                category = "network",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "network/desc.md", line_from = 1579, line_to = 1580 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "*RssOrVmqPreference",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 14. network-nic-disable-sriov
            newTweaks.Add(new TweakItem {
                id = "network-nic-disable-sriov",
                name_ru = "Отключить виртуализацию SR-IOV сетевой карты (*Sriov)",
                description_ru = "Отключает Single Root I/O Virtualization (SR-IOV), освобождая аппаратные ресурсы контроллера для прямого трафика системы.",
                group = "Аппаратная разгрузка NIC",
                category = "network",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "network/desc.md", line_from = 1625, line_to = 1627 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "*Sriov",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 15. network-nic-disable-fec
            newTweaks.Add(new TweakItem {
                id = "network-nic-disable-fec",
                name_ru = "Отключить коррекцию ошибок FEC для минимального пинга (FecMode)",
                description_ru = "Отключает Forward Error Correction на высокоскоростных сетевых линках, ликвидируя аппаратную задержку буферизации кадров.",
                group = "Аппаратная разгрузка NIC",
                category = "network",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "network/desc.md", line_from = 1686, line_to = 1689 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "FecMode",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 16. network-disable-ics-sharing
            newTweaks.Add(new TweakItem {
                id = "network-disable-ics-sharing",
                name_ru = "Отключить общий доступ к подключению к интернету (NC_ShowSharedAccessUI)",
                description_ru = "Запрещает использование службы Internet Connection Sharing (ICS), отключая ненужный шлюз и разгружая стек сетевых интерфейсов.",
                group = "Сетевые службы",
                category = "network",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "network/desc.md", line_from = 1478, line_to = 1480 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Network Connections",
                        name = "NC_ShowSharedAccessUI",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 17. peripheral-disable-non-volume-autoplay
            newTweaks.Add(new TweakItem {
                id = "peripheral-disable-non-volume-autoplay",
                name_ru = "Запретить автозапуск для нетомных устройств (NoAutoplayfornonVolume)",
                description_ru = "Блокирует автозапуск мультимедиа для внешних устройств без томов (смартфоны, MTP-камеры).",
                group = "Подключение устройств",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1392, line_to = 1394 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "NoAutoplayfornonVolume",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 18. peripheral-disable-drive-type-autorun
            newTweaks.Add(new TweakItem {
                id = "peripheral-disable-drive-type-autorun",
                name_ru = "Запретить автозапуск со всех типов дисков (NoDriveTypeAutoRun)",
                description_ru = "Устанавливает глобальную политику запрета автозапуска со всех типов приводов и накопителей (значение 255 / 0xFF).",
                group = "Подключение устройств",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1391, line_to = 1393 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "NoDriveTypeAutoRun",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 255 } },
                        revert = new RevertObj { mode = "set", value = 145 }
                    }
                }
            });

            // 19. peripheral-disable-startup-sound
            newTweaks.Add(new TweakItem {
                id = "peripheral-disable-startup-sound",
                name_ru = "Отключить звук запуска Windows (DisableStartupSound)",
                description_ru = "Отключает воспроизведение звукового сигнала при запуске и входе в Windows.",
                group = "Звуковые схемы и аудио",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1752, line_to = 1754 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                        name = "DisableStartupSound",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 20. peripheral-disable-touch-features
            newTweaks.Add(new TweakItem {
                id = "peripheral-disable-touch-features",
                name_ru = "Отключить сенсорный ввод планшета (TurnOffTouchInput)",
                description_ru = "Отключает сенсорный ввод TabletPC, снижая потребление ресурсов драйверами сенсорных панелей на ПК.",
                group = "Клавиатура и ввод",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1410, line_to = 1412 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\TabletPC",
                        name = "TurnOffTouchInput",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 21. visibility-menu-show-delay-zero
            newTweaks.Add(new TweakItem {
                id = "visibility-menu-show-delay-zero",
                name_ru = "Мгновенное открытие контекстных меню (MenuShowDelay)",
                description_ru = "Уменьшает задержку раскрытия подменю в проводнике с 400 мс до 0 мс на основе физических замеров msd_test.",
                group = "Отклик интерфейса",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 942, line_to = 944 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Control Panel\Desktop",
                        name = "MenuShowDelay",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "400" }
                    }
                }
            });

            // 22. visibility-classic-control-panel
            newTweaks.Add(new TweakItem {
                id = "visibility-classic-control-panel",
                name_ru = "Всегда открывать классическую панель управления (ForceClassicControlPanel)",
                description_ru = "Принудительно открывает классический вид всех элементов панели управления (крупные значки) вместо категорий.",
                group = "Проводник и рабочий стол",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1841, line_to = 1844 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "ForceClassicControlPanel",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 23. visibility-no-lock-screen
            newTweaks.Add(new TweakItem {
                id = "visibility-no-lock-screen",
                name_ru = "Отключить экран блокировки (NoLockScreen)",
                description_ru = "Пропускает экран блокировки с фоновыми часами и переходит сразу к окну ввода пароля или на рабочий стол.",
                group = "Экран входа и персонализация",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1765, line_to = 1767 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Personalization",
                        name = "NoLockScreen",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 24. visibility-disable-first-logon-animation
            newTweaks.Add(new TweakItem {
                id = "visibility-disable-first-logon-animation",
                name_ru = "Отключить анимацию первого входа в систему (EnableFirstLogonAnimation)",
                description_ru = "Отключает долгие анимационные экраны приветствия («Привет», «Подготовка Windows») при первом создании профиля.",
                group = "Экран входа и персонализация",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1082, line_to = 1084 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                        name = "EnableFirstLogonAnimation",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 25. visibility-alt-tab-no-browser-tabs
            newTweaks.Add(new TweakItem {
                id = "visibility-alt-tab-no-browser-tabs",
                name_ru = "Исключить вкладки браузера из Alt+Tab (MultiTaskingAltTabFilter)",
                description_ru = "Ограничивает переключатель Alt+Tab только открытыми окнами приложений, не засоряя список десятками вкладок Edge и браузеров.",
                group = "Многозадачность и окна",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1646, line_to = 1648 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                        name = "MultiTaskingAltTabFilter",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 3 } },
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
