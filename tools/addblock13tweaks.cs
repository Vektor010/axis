using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock13Tool
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

            // 1. visibility-disable-widgets
            newTweaks.Add(new TweakItem {
                id = "visibility-disable-widgets",
                name_ru = "Отключить службу 'Виджеты' (AllowNewsAndInterests)",
                description_ru = "Полностью отключает фоновую службу новостей и виджетов Windows 11 (DSH), освобождая память и ресурсы процессора.",
                group = "Панель задач Windows 11",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1295, line_to = 1298 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Dsh",
                        name = "AllowNewsAndInterests",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 2. visibility-disable-widgets-lockscreen
            newTweaks.Add(new TweakItem {
                id = "visibility-disable-widgets-lockscreen",
                name_ru = "Отключить виджеты на экране блокировки (DisableWidgetsOnLockScreen)",
                description_ru = "Блокирует показ погоды, финансов и новостных карточек на экране блокировки Windows.",
                group = "Экран входа и блокировки",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1295, line_to = 1299 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Dsh",
                        name = "DisableWidgetsOnLockScreen",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 3. visibility-disable-widgets-board
            newTweaks.Add(new TweakItem {
                id = "visibility-disable-widgets-board",
                name_ru = "Отключить доску виджетов (DisableWidgetsBoard)",
                description_ru = "Отключает боковую выдвижную панель мини-приложений и ленты новостей MSN.",
                group = "Панель задач Windows 11",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1295, line_to = 1300 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Dsh",
                        name = "DisableWidgetsBoard",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 4. visibility-hide-people-bar
            newTweaks.Add(new TweakItem {
                id = "visibility-hide-people-bar",
                name_ru = "Убрать значок 'Люди' с панели задач (HidePeopleBar)",
                description_ru = "Скрывает устаревший значок контактов 'Люди' (People Bar), освобождая место на панели задач.",
                group = "Панель задач",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1295, line_to = 1301 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "HidePeopleBar",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 5. visibility-hide-task-view-button
            newTweaks.Add(new TweakItem {
                id = "visibility-hide-task-view-button",
                name_ru = "Скрыть кнопку 'Просмотр задач' (HideTaskViewButton)",
                description_ru = "Убирает кнопку виртуальных рабочих столов и переключения задач из системной панели задач.",
                group = "Панель задач",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1295, line_to = 1302 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "HideTaskViewButton",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 6. power-remove-sleep-option
            newTweaks.Add(new TweakItem {
                id = "power-remove-sleep-option",
                name_ru = "Скрыть режим 'Сон' из меню выключения (ShowSleepOption)",
                description_ru = "Убирает пункт спящего режима из меню кнопки 'Пуск' и диалога выключения Windows, предотвращая случайный переход в сон.",
                group = "Управление питанием и выключение",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "power/desc.md", line_from = 2396, line_to = 2400 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "ShowSleepOption",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 7. power-remove-hibernate-option
            newTweaks.Add(new TweakItem {
                id = "power-remove-hibernate-option",
                name_ru = "Скрыть режим 'Гибернация' из меню выключения (ShowHibernateOption)",
                description_ru = "Убирает режим гибернации из списков выключения питания в меню 'Пуск'.",
                group = "Управление питанием и выключение",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "power/desc.md", line_from = 2396, line_to = 2401 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "ShowHibernateOption",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 8. visibility-disable-acrylic-logon-bg
            newTweaks.Add(new TweakItem {
                id = "visibility-disable-acrylic-logon-bg",
                name_ru = "Отключить акриловое размытие фона входа (DisableAcrylicBackgroundOnLogon)",
                description_ru = "Убирает тяжелый графический эффект размытия (Fluent Acrylic) на экране ввода пароля, ускоряя отрисовку.",
                group = "Экран входа и персонализация",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1765, line_to = 1773 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\System",
                        name = "DisableAcrylicBackgroundOnLogon",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 9. visibility-prevent-lockscreen-slideshow
            newTweaks.Add(new TweakItem {
                id = "visibility-prevent-lockscreen-slideshow",
                name_ru = "Запретить слайд-шоу на экране блокировки (NoLockScreenSlideshow)",
                description_ru = "Отключает циклическую смену обоев на экране блокировки, экономя фоновые ресурсы диска и процессора.",
                group = "Экран входа и персонализация",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1765, line_to = 1770 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Personalization",
                        name = "NoLockScreenSlideshow",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 10. visibility-prevent-lockscreen-camera
            newTweaks.Add(new TweakItem {
                id = "visibility-prevent-lockscreen-camera",
                name_ru = "Отключить доступ к камере на экране блокировки (NoLockScreenCamera)",
                description_ru = "Запрещает запуск приложения 'Камера' до разблокировки устройства пользователем.",
                group = "Экран входа и персонализация",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 1765, line_to = 1771 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Personalization",
                        name = "NoLockScreenCamera",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 11. visibility-no-recent-docs-history
            newTweaks.Add(new TweakItem {
                id = "visibility-no-recent-docs-history",
                name_ru = "Не сохранять журнал недавних документов (NoRecentDocsHistory)",
                description_ru = "Запрещает операционной системе вести список недавно открывавшихся файлов и программ в проводнике и меню 'Пуск'.",
                group = "Проводник и конфиденциальность",
                category = "visibility",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "visibility/desc.md", line_from = 334, line_to = 338 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "NoRecentDocsHistory",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 12. security-prevent-device-metadata
            newTweaks.Add(new TweakItem {
                id = "security-prevent-device-metadata",
                name_ru = "Запретить фоновую загрузку метаданных устройств (PreventDeviceMetadataFromNetwork)",
                description_ru = "Предотвращает скачивание сторонних значков, метаданных и сопутствующего ПО оборудования из сети Интернет.",
                group = "Установка оборудования и драйверов",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 483, line_to = 487 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Device Metadata",
                        name = "PreventDeviceMetadataFromNetwork",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 13. security-disable-root-auto-update
            newTweaks.Add(new TweakItem {
                id = "security-disable-root-auto-update",
                name_ru = "Отключить автоматическое обновление корневых сертификатов (DisableRootAutoUpdate)",
                description_ru = "Предотвращает непредвиденные фоновые сетевые обращения службы Certificate Manager к внешним узлам верификации.",
                group = "Сертификаты и безопасность",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 483, line_to = 488 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\SystemCertificates\AuthRoot",
                        name = "DisableRootAutoUpdate",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 14. security-driver-search-order
            newTweaks.Add(new TweakItem {
                id = "security-driver-search-order",
                name_ru = "Не искать драйверы в Центре обновления Windows (SearchOrderConfig)",
                description_ru = "Ограничивает поиск драйверов устройств локальным хранилищем системы, не обращаясь к серверам Windows Update.",
                group = "Установка оборудования и драйверов",
                category = "security",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 483, line_to = 486 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\DriverSearching",
                        name = "SearchOrderConfig",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 15. security-removable-disks-deny-write
            newTweaks.Add(new TweakItem {
                id = "security-removable-disks-deny-write",
                name_ru = "Запретить запись на съемные диски (Deny_Write)",
                description_ru = "Переводит все подключаемые USB-флешки и съемные накопители в режим 'Только для чтения', защищая от утечек информации и заражения червями.",
                group = "Защита съемных носителей",
                category = "security",
                type = "toggle",
                risk = "medium",
                presets = new List<string> { },
                needs_review = false,
                source = new SourceObj { file = "security/desc.md", line_from = 1174, line_to = 1177 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\RemovableStorageDevices\{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}",
                        name = "Deny_Write",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 16. system-notifications-no-use-store
            newTweaks.Add(new TweakItem {
                id = "system-notifications-no-use-store",
                name_ru = "Убрать 'Поиск приложения в Store' при открытии файлов (NoUseStoreOpenWith)",
                description_ru = "Убирает навязчивое предложение открыть Windows Store при попытке запустить файл с неизвестным расширением.",
                group = "Диалог открытия файлов",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8405 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "NoUseStoreOpenWith",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 17. system-notifications-disable-lockscreen
            newTweaks.Add(new TweakItem {
                id = "system-notifications-disable-lockscreen",
                name_ru = "Отключить уведомления приложений на экране блокировки (DisableLockScreenAppNotifications)",
                description_ru = "Блокирует показ всплывающих уведомлений от программ при заблокированном компьютере, повышая приватность.",
                group = "Уведомления и оповещения",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8406 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\System",
                        name = "DisableLockScreenAppNotifications",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 18. system-notifications-disable-enhanced
            newTweaks.Add(new TweakItem {
                id = "system-notifications-disable-enhanced",
                name_ru = "Отключить информационные уведомления антивируса (DisableEnhancedNotifications)",
                description_ru = "Отключает второстепенные сводки Defender о завершении обычных сканирований, не затрагивая критические угрозы.",
                group = "Уведомления и оповещения",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8411 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows Defender\Reporting",
                        name = "DisableEnhancedNotifications",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 19. system-notifications-no-tile
            newTweaks.Add(new TweakItem {
                id = "system-notifications-no-tile",
                name_ru = "Отключить push-уведомления для живых плиток (NoTileApplicationNotification)",
                description_ru = "Блокирует сетевую передачу push-обновлений для живых плиток в меню 'Пуск', уменьшая сетевой фоновый трафик.",
                group = "Уведомления и оповещения",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8414 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                        name = "NoTileApplicationNotification",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 20. system-notifications-no-tray-cleanup
            newTweaks.Add(new TweakItem {
                id = "system-notifications-no-tray-cleanup",
                name_ru = "Всегда отображать все значки в трее без скрытия (NoAutoTrayNotify)",
                description_ru = "Отключает автоскрытие значков в системном трее, показывая все активные фоновые значки на виду.",
                group = "Системный трей",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8422 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "NoAutoTrayNotify",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 21. system-notifications-turn-off-balloons
            newTweaks.Add(new TweakItem {
                id = "system-notifications-turn-off-balloons",
                name_ru = "Отключить классические всплывающие подсказки-баллоны (TaskbarNoNotification)",
                description_ru = "Отключает классические всплывающие сообщения над областью уведомлений панели задач.",
                group = "Уведомления и оповещения",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8423 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                        name = "TaskbarNoNotification",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 22. system-notifications-no-feature-ads
            newTweaks.Add(new TweakItem {
                id = "system-notifications-no-feature-ads",
                name_ru = "Отключить рекламные уведомления о новых функциях (NoBalloonFeatureAdvertisements)",
                description_ru = "Блокирует рекламные подсказки Windows, предлагающие попробовать новые функции ОС.",
                group = "Уведомления и оповещения",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8424 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Policies\Microsoft\Windows\Explorer",
                        name = "NoBalloonFeatureAdvertisements",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 23. system-search-disable-removable-drive-indexing
            newTweaks.Add(new TweakItem {
                id = "system-search-disable-removable-drive-indexing",
                name_ru = "Запретить индексацию файлов на съемных дисках (DisableRemovableDriveIndexing)",
                description_ru = "Запрещает службе Windows Search индексировать содержимое внешних жестких дисков и флешек, предотвращая замедление работы.",
                group = "Индексация и поиск",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 9150, line_to = 9160 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                        name = "DisableRemovableDriveIndexing",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 24. peripheral-printers-disable-webpnp-download
            newTweaks.Add(new TweakItem {
                id = "peripheral-printers-disable-webpnp-download",
                name_ru = "Запретить скачивание драйверов принтеров по HTTP (DisableWebPnPDownload)",
                description_ru = "Блокирует автоматическую загрузку драйверов печати через Интернет по протоколу WebPnP.",
                group = "Принтеры и печать",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1828, line_to = 1834 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows NT\Printers",
                        name = "DisableWebPnPDownload",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 25. peripheral-printers-legacy-default-mode
            newTweaks.Add(new TweakItem {
                id = "peripheral-printers-legacy-default-mode",
                name_ru = "Отключить автосмену принтера по умолчанию (LegacyDefaultPrinterMode)",
                description_ru = "Запрещает Windows автоматически переназначать принтер по умолчанию на последнее использовавшееся устройство.",
                group = "Принтеры и печать",
                category = "peripheral",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "peripheral/desc.md", line_from = 1828, line_to = 1836 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\Microsoft\Windows NT\CurrentVersion\Windows",
                        name = "LegacyDefaultPrinterMode",
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
