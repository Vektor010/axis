using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock15Tool
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
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(jsonPath, Encoding.UTF8);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<TweakItem>>(raw);

            Console.WriteLine("Текущее число твиков: " + list.Count);

            var newTweaks = new List<TweakItem>
            {
                // 1. privacy-appcompat-disable-api-sampling
                new TweakItem
                {
                    id = "privacy-appcompat-disable-api-sampling",
                    name_ru = "Отключить сэмплирование API приложений (API Sampling)",
                    description_ru = "Отключает фоновый замер вызовов API инвентаризатором совместимости приложений для снижения накладных расходов.",
                    group = "Телеметрия и диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1416, line_to = 1418 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppCompat",
                            name = "DisableAPISamping",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. privacy-appcompat-disable-app-footprint
                new TweakItem
                {
                    id = "privacy-appcompat-disable-app-footprint",
                    name_ru = "Отключить инвентаризацию следов приложений (Application Footprint)",
                    description_ru = "Блокирует службу инвентаризации устройств и приложений от сканирования каталогов установленного ПО.",
                    group = "Телеметрия и диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1417, line_to = 1419 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppCompat",
                            name = "DisableApplicationFootprint",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. privacy-appcompat-disable-win32-backup
                new TweakItem
                {
                    id = "privacy-appcompat-disable-win32-backup",
                    name_ru = "Отключить проверку совместимости архивных приложений Win32",
                    description_ru = "Отключает периодическое фоновое сканирование совместимости для резервируемых приложений Win32.",
                    group = "Телеметрия и диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1418, line_to = 1420 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppCompat",
                            name = "DisableWin32AppBackup",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. privacy-disable-onesettings-auditing
                new TweakItem
                {
                    id = "privacy-disable-onesettings-auditing",
                    name_ru = "Отключить аудит службы OneSettings",
                    description_ru = "Отключает журналирование и аудит взаимодействия компонентов Windows с облачной службой конфигурации OneSettings.",
                    group = "Телеметрия и диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1431, line_to = 1434 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "EnableOneSettingsAuditing",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. privacy-video-playback-allow-low-resolution
                new TweakItem
                {
                    id = "privacy-video-playback-allow-low-resolution",
                    name_ru = "Воспроизводить видео в низком разрешении для экономии сети",
                    description_ru = "Разрешает встроенным видеоплеерам воспроизводить сетевые видеопотоки в меньшем разрешении для снижения расхода трафика.",
                    group = "Конфиденциальность и данные",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1578, line_to = 1580 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\VideoSettings",
                            name = "AllowLowResolution",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 6. privacy-video-playback-disable-auto-enhance
                new TweakItem
                {
                    id = "privacy-video-playback-disable-auto-enhance",
                    name_ru = "Отключить автоулучшение видео при воспроизведении",
                    description_ru = "Отключает фоновую цифровую постобработку видеопотока, снижая нагрузку на видеокарту и батарею устройства.",
                    group = "Конфиденциальность и данные",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1581, line_to = 1583 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\VideoSettings",
                            name = "EnableAutoEnhanceDuringPlayback",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. security-sudo-disable-normal-mode
                new TweakItem
                {
                    id = "security-sudo-disable-normal-mode",
                    name_ru = "Отключить команду Sudo в Windows",
                    description_ru = "Запрещает выполнение команды sudo в командной строке и консоли во избежание несанкционированного повышения привилегий.",
                    group = "Контроль учетных записей и доступ",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 26100,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1288, line_to = 1290 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Sudo",
                            name = "Enabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. security-camera-osd-indicator
                new TweakItem
                {
                    id = "security-camera-osd-indicator",
                    name_ru = "Включить программный экранный индикатор веб-камеры (OSD)",
                    description_ru = "Отображает системное экранное уведомление при обращении к веб-камере на устройствах без физического светодиода активности.",
                    group = "Безопасность устройств",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1304, line_to = 1307 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Microsoft\OEM\Device\Capture",
                            name = "NoPhysicalCameraLED",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 9. security-wu-exclude-drivers-in-quality-updates
                new TweakItem
                {
                    id = "security-wu-exclude-drivers-in-quality-updates",
                    name_ru = "Исключить драйверы из накопительных обновлений Windows",
                    description_ru = "Гарантирует, что обновления качества Windows Update не перезапишут установленные графические, аудио и системные драйверы.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 499, line_to = 501 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "ExcludeWUDriversInQualityUpdate",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. security-wu-disable-pause-updates-ux
                new TweakItem
                {
                    id = "security-wu-disable-pause-updates-ux",
                    name_ru = "Скрыть кнопку приостановки обновлений в интерфейсе",
                    description_ru = "Удаляет доступ к кнопке приостановки обновлений в диалоговом окне параметров Центра обновления Windows.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 501, line_to = 503 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "SetDisablePauseUXAccess",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. security-wu-disable-internet-locations
                new TweakItem
                {
                    id = "security-wu-disable-internet-locations",
                    name_ru = "Запретить подключение Windows Update к узлам в интернете",
                    description_ru = "Блокирует отправку запросов Центра обновления Windows на общедоступные интернет-серверы обновлений.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 495, line_to = 497 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "DoNotConnectToWindowsUpdateInternetLocations",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. security-wu-allow-servicing-enterprise-features
                new TweakItem
                {
                    id = "security-wu-allow-servicing-enterprise-features",
                    name_ru = "Включить новые корпоративные функции Windows (Servicing Features)",
                    description_ru = "Активирует новые возможности и сервисные улучшения операционной системы, по умолчанию отключенные в сборке.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 502, line_to = 504 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "AllowTemporaryEnterpriseFeatureControl",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. security-passport-dynamic-lock-factors
                new TweakItem
                {
                    id = "security-passport-dynamic-lock-factors",
                    name_ru = "Настроить факторы динамической блокировки (Dynamic Lock)",
                    description_ru = "Активирует групповую политику динамической блокировки ПК с проверкой внешних факторов присутствия пользователя.",
                    group = "Безопасность входа и учетных записей",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1267, line_to = 1270 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\PassportForWork\DynamicLock",
                            name = "DynamicLock",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. system-notifications-lock-screen-toast-off
                new TweakItem
                {
                    id = "system-notifications-lock-screen-toast-off",
                    name_ru = "Отключить всплывающие уведомления на экране блокировки",
                    description_ru = "Запрещает отображение всплывающих уведомлений приложений на экране блокировки для сохранения конфиденциальности.",
                    group = "Уведомления и фокус внимания",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8415, line_to = 8417 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                            name = "NoToastApplicationNotificationOnLockScreen",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. system-notifications-turn-off-cloud
                new TweakItem
                {
                    id = "system-notifications-turn-off-cloud",
                    name_ru = "Отключить сетевое использование службы push-уведомлений (WPN)",
                    description_ru = "Запрещает службе Windows Push Notifications подключаться к облачным серверам Microsoft для доставки уведомлений приложений.",
                    group = "Уведомления и фокус внимания",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8416, line_to = 8418 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                            name = "NoCloudApplicationNotification",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. system-notifications-disallow-mirroring
                new TweakItem
                {
                    id = "system-notifications-disallow-mirroring",
                    name_ru = "Отключить дублирование уведомлений между устройствами",
                    description_ru = "Блокирует синхронизацию и зеркалирование системных уведомлений с другими сопряженными смартфонами и ПК.",
                    group = "Уведомления и фокус внимания",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8417, line_to = 8419 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                            name = "DisallowNotificationMirroring",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. system-notifications-always-show-bell-icon
                new TweakItem
                {
                    id = "system-notifications-always-show-bell-icon",
                    name_ru = "Всегда отображать значок колокольчика центра уведомлений",
                    description_ru = "Принудительно закрепляет и всегда отображает значок центра уведомлений на панели задач.",
                    group = "Уведомления и фокус внимания",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8418, line_to = 8420 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "AlwaysShowNotificationIcon",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. system-notifications-enable-legacy-balloons
                new TweakItem
                {
                    id = "system-notifications-enable-legacy-balloons",
                    name_ru = "Отображать подсказки трея в классическом стиле (Balloon Tips)",
                    description_ru = "Возвращает классические всплывающие подсказки-баллоны в трее вместо современных тостов.",
                    group = "Уведомления и фокус внимания",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8419, line_to = 8421 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "EnableLegacyBalloonNotifications",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. system-notifications-expanded-toast-action-center
                new TweakItem
                {
                    id = "system-notifications-expanded-toast-action-center",
                    name_ru = "Развернутый вид уведомлений в Центре уведомлений",
                    description_ru = "Показывает всплывающие уведомления в Центре уведомлений сразу в развернутом виде без необходимости нажатия «Подробнее».",
                    group = "Уведомления и фокус внимания",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8413, line_to = 8415 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                            name = "EnableExpandedToastNotifications",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. network-lltd-allow-io-public-net
                new TweakItem
                {
                    id = "network-lltd-allow-io-public-net",
                    name_ru = "Запретить сопоставитель LLTDIO в общедоступных сетях",
                    description_ru = "Запрещает работу протокола Link-Layer Topology Discovery Mapper в открытых и общедоступных Wi-Fi сетях.",
                    group = "Сетевые протоколы и обнаружение",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 758, line_to = 761 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LLTD",
                            name = "AllowLLTDIOOnPublicNet",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. network-lltd-prohibit-io-private-net
                new TweakItem
                {
                    id = "network-lltd-prohibit-io-private-net",
                    name_ru = "Отключить сопоставитель LLTDIO в частных сетях",
                    description_ru = "Блокирует широковещательный опрос сетевой топологии протоколом LLTDIO в домашних и рабочих сетях для снижения сетевого шума.",
                    group = "Сетевые протоколы и обнаружение",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 758, line_to = 761 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LLTD",
                            name = "ProhibitLLTDIOOnPrivateNet",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. network-lltd-prohibit-rspndr-private-net
                new TweakItem
                {
                    id = "network-lltd-prohibit-rspndr-private-net",
                    name_ru = "Отключить ответчик RSPNDR в частных сетях",
                    description_ru = "Блокирует ответы сетевого адаптера на запросы обнаружения топологии RSPNDR в частных локальных сетях.",
                    group = "Сетевые протоколы и обнаружение",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 760, line_to = 762 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LLTD",
                            name = "ProhibitRspndrOnPrivateNet",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. peripheral-touchpad-aap-threshold-low
                new TweakItem
                {
                    id = "peripheral-touchpad-aap-threshold-low",
                    name_ru = "Максимальная чувствительность тачпада (AAP Threshold 0)",
                    description_ru = "Устанавливает порог Accidental Activation Prevention в 0, обеспечивая мгновенный отклик курсора без задержки при наборе текста.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1431, line_to = 1433 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                            name = "AAPThreshold",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 2 }
                        }
                    }
                },

                // 24. peripheral-touchpad-feedback-intensity-max
                new TweakItem
                {
                    id = "peripheral-touchpad-feedback-intensity-max",
                    name_ru = "Максимальная тактильная отдача тачпада (100% Haptic Feedback)",
                    description_ru = "Устанавливает интенсивность виброотклика сенсорной панели на максимальное значение 100% для четкого физического клика.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1433, line_to = 1435 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                            name = "FeedbackIntensity",
                            type = "REG_DWORD",
                            apply = 100,
                            revert = new RevertObj { mode = "set", value = 50 }
                        }
                    }
                },

                // 25. peripheral-touchpad-honor-mouse-accel
                new TweakItem
                {
                    id = "peripheral-touchpad-honor-mouse-accel",
                    name_ru = "Применять настройки акселерации мыши к тачпаду",
                    description_ru = "Синхронизирует кривую ускорения курсора сенсорной панели с параметрами аппаратной мыши из Панели управления.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1444, line_to = 1446 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                            name = "HonorMouseAccelSetting",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                }
            };

            Console.WriteLine("Добавляем новых твиков: " + newTweaks.Count);
            list.AddRange(newTweaks);
            Console.WriteLine("Новое суммарное число твиков: " + list.Count);

            string serialized = serializer.Serialize(list);
            string formatted = FormatJson(serialized);

            File.WriteAllText(jsonPath, formatted, new UTF8Encoding(false));
            Console.WriteLine("Файл tweaks.json успешно сохранен в кодировке UTF-8 без BOM!");
        }

        static string FormatJson(string json)
        {
            var sb = new StringBuilder();
            bool quoted = false;
            int indent = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
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
