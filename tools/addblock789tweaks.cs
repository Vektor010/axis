using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            string raw = File.ReadAllText(jsonPath, Encoding.UTF8);
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
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
                // 1. system-disable-paging-executive
                new TweakDef
                {
                    id = "system-disable-paging-executive",
                    name = "Запретить сброс ядра в файл подкачки (DisablePagingExecutive)",
                    desc = "Удерживает исполняемый код ntoskrnl и драйверы ядра строго в физической оперативной памяти (DisablePagingExecutive = 1), полностью устраняя задержки при системных вызовах.",
                    group = "Управление памятью и кэшем",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    nameVal = "DisablePagingExecutive",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "system/desc.md",
                    from = 3543,
                    to = 3545,
                    needsReview = true
                },

                // 2. system-disable-page-combining
                new TweakDef
                {
                    id = "system-disable-page-combining",
                    name = "Отключить дедупликацию памяти (DisablePageCombining)",
                    desc = "Отключает фоновое сканирование одинаковых страниц оперативной памяти процессором (DisablePageCombining = 1), снижая непроизводительную нагрузку на ядра CPU.",
                    group = "Управление памятью и кэшем",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    nameVal = "DisablePageCombining",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "system/desc.md",
                    from = 3542,
                    to = 3544,
                    needsReview = true
                },

                // 3. system-disable-cache-telemetry
                new TweakDef
                {
                    id = "system-disable-cache-telemetry",
                    name = "Отключить телеметрию кэша ФС (DisableCacheTelemetry)",
                    desc = "Отключает сбор внутренней диагностической телеметрии кэша системных файлов (CcDisableTelemetryRegKeyAtInit = 1), уменьшая фоновую активность накопителя.",
                    group = "Управление памятью и кэшем",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    nameVal = "DisableCacheTelemetry",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "system/desc.md",
                    from = 3541,
                    to = 3543,
                    needsReview = true
                },

                // 4. system-bcd-bootux-disabled
                new TweakDef
                {
                    id = "system-bcd-bootux-disabled",
                    name = "Отключить графический экран загрузки (bootuxdisabled)",
                    desc = "Отключает экран приветствия и полосу загрузки при старте операционной системы (bootuxdisabled = true), ускоряя инициализацию Windows на быстрых SSD.",
                    group = "Загрузка и BCD",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"BCD00000000\Objects\{default}\Elements\1600006C",
                    nameVal = "Element",
                    type = "REG_BINARY",
                    apply = "01",
                    revMode = "set",
                    revVal = "00",
                    srcFile = "system/desc.md",
                    from = 7805,
                    to = 7807,
                    needsReview = true
                },

                // 5. system-bcd-bootmenupolicy-legacy
                new TweakDef
                {
                    id = "system-bcd-bootmenupolicy-legacy",
                    name = "Классическое меню загрузки F8 (bootmenupolicy = Legacy)",
                    desc = "Возвращает классический текстовый загрузчик Windows с вызовом безопасного режима по клавише F8 без перехода в тяжелый графический интерфейс WinRE.",
                    group = "Загрузка и BCD",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"BCD00000000\Objects\{default}\Elements\25000008",
                    nameVal = "Element",
                    type = "REG_BINARY",
                    apply = "0000000000000000",
                    revMode = "set",
                    revVal = "0100000000000000",
                    srcFile = "system/desc.md",
                    from = 7852,
                    to = 7854,
                    needsReview = true
                },

                // 6. system-bcd-highestmode
                new TweakDef
                {
                    id = "system-bcd-highestmode",
                    name = "Максимальное графическое разрешение UEFI (highestmode)",
                    desc = "Принудительно устанавливает наивысшее доступное экранное разрешение видеоадаптера на этапе ранней загрузки UEFI (highestmode = true).",
                    group = "Загрузка и BCD",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"BCD00000000\Objects\{default}\Elements\16000054",
                    nameVal = "Element",
                    type = "REG_BINARY",
                    apply = "01",
                    revMode = "set",
                    revVal = "00",
                    srcFile = "system/desc.md",
                    from = 7809,
                    to = 7811,
                    needsReview = true
                },

                // 7. system-bcd-disable-loading-circle
                new TweakDef
                {
                    id = "system-bcd-disable-loading-circle",
                    name = "Отключить вращающийся круг загрузки (Custom 16000069)",
                    desc = "Отключает циклическую анимацию точек при загрузке ядра Windows (custom:16000069 = true), исключая микрозадержки видеодрайвера при инициализации.",
                    group = "Загрузка и BCD",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"BCD00000000\Objects\{default}\Elements\16000069",
                    nameVal = "Element",
                    type = "REG_BINARY",
                    apply = "01",
                    revMode = "set",
                    revVal = "00",
                    srcFile = "system/desc.md",
                    from = 7806,
                    to = 7808,
                    needsReview = true
                },

                // 8. system-bcd-disable-boot-logo
                new TweakDef
                {
                    id = "system-bcd-disable-boot-logo",
                    name = "Отключить логотип Windows при старте (Custom 16000067)",
                    desc = "Отключает отображение системного логотипа при инициализации ядра (custom:16000067 = true), делая запуск бесшовным и быстрым.",
                    group = "Загрузка и BCD",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"BCD00000000\Objects\{default}\Elements\16000067",
                    nameVal = "Element",
                    type = "REG_BINARY",
                    apply = "01",
                    revMode = "set",
                    revVal = "00",
                    srcFile = "system/desc.md",
                    from = 7807,
                    to = 7809,
                    needsReview = true
                },

                // 9. system-timer-watchdog-disable
                new TweakDef
                {
                    id = "system-timer-watchdog-disable",
                    name = "Отключить аппаратный сторожевой таймер (SYSTEMWATCHDOGPOLICY=DISABLED)",
                    desc = "Отключает прерывания сторожевого таймера HAL (HalpTimerWatchdogDisable = 1), предотвращая регулярные всплески задержек DPC в соревновательных играх.",
                    group = "Системные таймеры и тактовая частота",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel",
                    nameVal = "SYSTEMWATCHDOGPOLICY",
                    type = "REG_SZ",
                    apply = "DISABLED",
                    revMode = "delete",
                    revVal = null,
                    srcFile = "system/desc.md",
                    from = 8068,
                    to = 8072,
                    needsReview = true
                },

                // 10. system-timer-force-tsc
                new TweakDef
                {
                    id = "system-timer-force-tsc",
                    name = "Использовать быстрый процессорный счетчик TSC (USEPLATFORMCLOCK=No)",
                    desc = "Запрещает принудительное чтение медленного внешнего таймера HPET системной платы, переключая систему на прямой аппаратный счетчик тактов TSC.",
                    group = "Системные таймеры и тактовая частота",
                    category = "system",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel",
                    nameVal = "USEPLATFORMCLOCK",
                    type = "REG_SZ",
                    apply = "No",
                    revMode = "delete",
                    revVal = null,
                    srcFile = "system/desc.md",
                    from = 8087,
                    to = 8090,
                    needsReview = true
                },

                // 11. affinities-msi-mode-gpu
                new TweakDef
                {
                    id = "affinities-msi-mode-gpu",
                    name = "Включить режим MSI (Message Signaled Interrupts) для видеокарты",
                    desc = "Переводит графический процессор в режим векторных прерываний MSI (MSISupported = 1), устраняя конфликты общих линий IRQ и стабилизируя фреймтайм.",
                    group = "Режим MSI и прерывания",
                    category = "affinities",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties",
                    nameVal = "MSISupported",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "power/desc.md",
                    from = 1154,
                    to = 1157,
                    needsReview = true
                },

                // 12. affinities-gpu-device-priority
                new TweakDef
                {
                    id = "affinities-gpu-device-priority",
                    name = "Высокий приоритет прерываний видеокарты (DevicePriority = High)",
                    desc = "Устанавливает приоритет обработки прерываний видеокарты на высокий уровень (DevicePriority = 3 / High), сокращая инпут-лаг при рендеринге кадров.",
                    group = "Режим MSI и прерывания",
                    category = "affinities",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy",
                    nameVal = "DevicePriority",
                    type = "REG_DWORD",
                    apply = 3,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "power/desc.md",
                    from = 1164,
                    to = 1168,
                    needsReview = true
                },

                // 13. affinities-nic-device-priority
                new TweakDef
                {
                    id = "affinities-nic-device-priority",
                    name = "Высокий приоритет прерываний сетевой карты (NIC DevicePriority)",
                    desc = "Задает высокий приоритет прерываний для сетевого адаптера (DevicePriority = 3 / High), ускоряя цикл обработки сетевых пакетов на уровне ядра.",
                    group = "Режим MSI и прерывания",
                    category = "affinities",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy",
                    nameVal = "DevicePriority",
                    type = "REG_DWORD",
                    apply = 3,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "power/desc.md",
                    from = 1164,
                    to = 1168,
                    needsReview = true
                },

                // 14. affinities-device-policy-spread
                new TweakDef
                {
                    id = "affinities-device-policy-spread",
                    name = "Многоядерное распределение MSI-X (DevicePolicy = Spread)",
                    desc = "Включает равномерное распределение векторов прерываний контроллеров по всем доступным ядрам CPU (DevicePolicy = 5), исключая троттлинг Core 0.",
                    group = "Политика распределения прерываний",
                    category = "affinities",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy",
                    nameVal = "DevicePolicy",
                    type = "REG_DWORD",
                    apply = 5,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "power/desc.md",
                    from = 1164,
                    to = 1168,
                    needsReview = true
                },

                // 15. misc-disable-onedrive-file-sync
                new TweakDef
                {
                    id = "misc-disable-onedrive-file-sync",
                    name = "Запретить использование OneDrive для хранения файлов",
                    desc = "Полностью отключает интеграцию облака OneDrive в Windows (DisableFileSyncNGSC = 1), предотвращая фоновую синхронизацию и дисковую нагрузку.",
                    group = "OneDrive и облачные службы",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"Software\Policies\Microsoft\Windows\OneDrive",
                    nameVal = "DisableFileSyncNGSC",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 356,
                    to = 358,
                    needsReview = true
                },

                // 16. misc-disable-onedrive-network-traffic
                new TweakDef
                {
                    id = "misc-disable-onedrive-network-traffic",
                    name = "Запретить сетевой трафик OneDrive до входа",
                    desc = "Блокирует отправку предварительных сетевых запросов службой OneDrive до авторизации пользователя (PreventNetworkTrafficPreUserSignIn = 1).",
                    group = "OneDrive и облачные службы",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SOFTWARE\Microsoft\OneDrive",
                    nameVal = "PreventNetworkTrafficPreUserSignIn",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 357,
                    to = 359,
                    needsReview = true
                },

                // 17. misc-7zip-cascaded-menu
                new TweakDef
                {
                    id = "misc-7zip-cascaded-menu",
                    name = "Каскадное контекстное меню 7-Zip (CascadedMenu)",
                    desc = "Объединяет все операции архиватора 7-Zip в компактное каскадное подменю Проводника, очищая контекстное меню от визуального мусора.",
                    group = "Архиваторы и контекстное меню",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKCU",
                    path = @"Software\7-Zip\Options",
                    nameVal = "CascadedMenu",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 304,
                    to = 306,
                    needsReview = true
                },

                // 18. misc-7zip-eliminate-root-folder-duplication
                new TweakDef
                {
                    id = "misc-7zip-eliminate-root-folder-duplication",
                    name = "Устранять дублирование корневых папок при распаковке 7-Zip",
                    desc = "Предотвращает создание вложенных одноименных папок при извлечении архивов 7-Zip (ElimDupExtract = 1), наводя порядок в файлах.",
                    group = "Архиваторы и контекстное меню",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKCU",
                    path = @"Software\7-Zip\Options",
                    nameVal = "ElimDupExtract",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 304,
                    to = 306,
                    needsReview = true
                },

                // 19. misc-7zip-menu-icons
                new TweakDef
                {
                    id = "misc-7zip-menu-icons",
                    name = "Отображать значки 7-Zip в контекстном меню",
                    desc = "Включает наглядные миниатюрные иконки рядом с пунктами 7-Zip в контекстном меню Проводника (MenuIcons = 1).",
                    group = "Архиваторы и контекстное меню",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKCU",
                    path = @"Software\7-Zip\Options",
                    nameVal = "MenuIcons",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 304,
                    to = 306,
                    needsReview = true
                },

                // 20. misc-startallback-noxaml-menus
                new TweakDef
                {
                    id = "misc-startallback-noxaml-menus",
                    name = "Отключить медленные WinUI/XAML меню в StartAllBack (NoXAMLMenus)",
                    desc = "Отключает тяжелый рендеринг XAML в контекстных меню Проводника и панели задач, возвращая мгновенный Win32 отклик.",
                    group = "Интерфейсные утилиты (StartAllBack)",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKCU",
                    path = @"Software\StartIsBack",
                    nameVal = "NoXAMLMenus",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 63,
                    to = 65,
                    needsReview = true
                },

                // 21. misc-startallback-undead-control-panel
                new TweakDef
                {
                    id = "misc-startallback-undead-control-panel",
                    name = "Классическая Панель управления в StartAllBack (UndeadControlPanel)",
                    desc = "Сохраняет доступ к классической панели управления Windows без редиректа в громоздкое UWP-приложение Параметры.",
                    group = "Интерфейсные утилиты (StartAllBack)",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKCU",
                    path = @"Software\StartIsBack",
                    nameVal = "UndeadControlPanel",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 63,
                    to = 65,
                    needsReview = true
                },

                // 22. misc-startallback-navbar-glass
                new TweakDef
                {
                    id = "misc-startallback-navbar-glass",
                    name = "Эффект Mica/Acrylic для навигационной панели (NavBarGlass)",
                    desc = "Включает аппаратный эффект прозрачности Mica/Glass для адресной строки и панели управления Проводника (NavBarGlass = 1).",
                    group = "Интерфейсные утилиты (StartAllBack)",
                    category = "misc",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKCU",
                    path = @"Software\StartIsBack",
                    nameVal = "NavBarGlass",
                    type = "REG_DWORD",
                    apply = 1,
                    revMode = "set",
                    revVal = 0,
                    srcFile = "misc/desc.md",
                    from = 63,
                    to = 65,
                    needsReview = true
                },

                // 23. affinities-gpu-core-affinity
                new TweakDef
                {
                    id = "affinities-gpu-core-affinity",
                    name = "Маска привязки прерываний GPU к ядрам (AssignmentSetOverride)",
                    desc = "Направляет прерывания видеокарты на выделенные физические ядра CPU (AssignmentSetOverride), исключая конкуренцию DPC/ISR с игровыми потоками.",
                    group = "Политика распределения прерываний",
                    category = "affinities",
                    risk = "low",
                    preset = "recommended",
                    hive = "HKLM",
                    path = @"SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy",
                    nameVal = "AssignmentSetOverride",
                    type = "REG_BINARY",
                    apply = "0000000000000300",
                    revMode = "delete",
                    revVal = null,
                    srcFile = "affinities/desc.md",
                    from = 26,
                    to = 28,
                    needsReview = true
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
                t["presets"] = string.IsNullOrEmpty(d.preset) ? new List<string>() : new List<string> { d.preset };
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
            File.WriteAllText(jsonPath, updatedJson, Encoding.UTF8);
            Console.WriteLine("Done! Added {0} tweaks. Total tweaks: {1}", addedCount, tweaks.Count);
        }
    }
}
