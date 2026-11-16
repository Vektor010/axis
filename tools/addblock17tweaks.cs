using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock17Tool
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
                // 1. privacy-app-access-location
                new TweakItem
                {
                    id = "privacy-app-access-location",
                    name_ru = "Запретить приложениям доступ к местоположению",
                    description_ru = "Блокирует UWP-приложениям и службам доступ к GPS-координатам и геолокации устройства.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1070, line_to = 1072 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessLocation",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. privacy-app-access-microphone
                new TweakItem
                {
                    id = "privacy-app-access-microphone",
                    name_ru = "Запретить приложениям доступ к микрофону",
                    description_ru = "Запрещает установленным UWP-программам захватывать звук со встроенных и внешних микрофонов.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1072, line_to = 1074 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessMicrophone",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. privacy-app-access-voice-activation
                new TweakItem
                {
                    id = "privacy-app-access-voice-activation",
                    name_ru = "Запретить голосовую активацию приложений",
                    description_ru = "Блокирует фоновое прослушивание ключевых фраз голосовой активации для вызова приложений.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1082, line_to = 1084 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsActivateWithVoice",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. privacy-app-access-voice-above-lock
                new TweakItem
                {
                    id = "privacy-app-access-voice-above-lock",
                    name_ru = "Запретить голосовую активацию при заблокированном экране",
                    description_ru = "Запрещает активацию голосовых помощников и приложений голосом при заблокированном компьютере.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1083, line_to = 1085 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsActivateWithVoiceAboveLock",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. privacy-app-access-spatial-perception
                new TweakItem
                {
                    id = "privacy-app-access-spatial-perception",
                    name_ru = "Запретить отслеживание движений пользователя в фоне",
                    description_ru = "Блокирует фоновым приложениям доступ к пространственному восприятию движений и жестов пользователя.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1084, line_to = 1086 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessBackgroundSpatialPerception",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. privacy-app-access-user-info
                new TweakItem
                {
                    id = "privacy-app-access-user-info",
                    name_ru = "Запретить совместное использование сведений профиля",
                    description_ru = "Блокирует передачу имени пользователя, аватара и домена приложениям Windows.",
                    group = "Конфиденциальность учетных записей",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1085, line_to = 1087 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "AllowUserInfoAccess",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-app-access-cellular-data
                new TweakItem
                {
                    id = "privacy-app-access-cellular-data",
                    name_ru = "Запретить приложениям доступ к сотовым данным (Cellular Data)",
                    description_ru = "Блокирует неконтролируемое использование сотового модема и мобильного трафика UWP-приложениями.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1086, line_to = 1088 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WwanSvc\CellularDataAccess",
                            name = "LetAppsAccessCellularData",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. privacy-biometrics-anti-spoofing
                new TweakItem
                {
                    id = "privacy-biometrics-anti-spoofing",
                    name_ru = "Включить усиленную защиту от спуфинга лица (Enhanced Anti-Spoofing)",
                    description_ru = "Обязывает алгоритмы Windows Hello использовать датчики глубины и инфракрасную подсветку для защиты от фотоподделок.",
                    group = "Биометрия и безопасность",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1006, line_to = 1008 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures",
                            name = "EnhancedAntiSpoofing",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. privacy-terminal-encrypted-ticket
                new TweakItem
                {
                    id = "privacy-terminal-encrypted-ticket",
                    name_ru = "Разрешать только зашифрованные тикеты удаленной помощи",
                    description_ru = "Требует использование шифрованных билетов подключений Remote Assistance стандарта Vista и выше.",
                    group = "Удаленный доступ и терминалы",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1033, line_to = 1035 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\policies\Microsoft\Windows NT\Terminal Services",
                            name = "CreateEncryptedOnlyTickets",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. security-uac-enable-uia-desktop-toggle
                new TweakItem
                {
                    id = "security-uac-enable-uia-desktop-toggle",
                    name_ru = "Разрешить запросы UIA без безопасного рабочего стола",
                    description_ru = "Позволяет программам специальных возможностей и удаленной помощи отображать запросы UAC на обычном рабочем столе.",
                    group = "Контроль учетных записей (UAC)",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 531, line_to = 539 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "EnableUIADesktopToggle",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 11. security-uac-enable-secure-uia-paths
                new TweakItem
                {
                    id = "security-uac-enable-secure-uia-paths",
                    name_ru = "Требовать запуск UIAccess только из безопасных папок",
                    description_ru = "Повышает привилегии UIAccess только для приложений, расположенных в защищенных системных каталогах Program Files и System32.",
                    group = "Контроль учетных записей (UAC)",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 581, line_to = 589 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "EnableSecureUIAPaths",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 12. security-uac-enable-virtualization
                new TweakItem
                {
                    id = "security-uac-enable-virtualization",
                    name_ru = "Отключить виртуализацию записи в реестр и файлы (Luafv)",
                    description_ru = "Отключает драйвер виртуализации Luafv, предотвращая скрытое дублирование файлов в папки VirtualStore.",
                    group = "Контроль учетных записей (UAC)",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 608, line_to = 616 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "EnableVirtualization",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 13. security-deviceguard-require-platform-security
                new TweakItem
                {
                    id = "security-deviceguard-require-platform-security",
                    name_ru = "Базовая аппаратная безопасность Device Guard (Secure Boot)",
                    description_ru = "Требует только безопасную загрузку Secure Boot без обязательной изоляции памяти DMA для работы VBS.",
                    group = "Аппаратная безопасность и целостность",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1025, line_to = 1028 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard",
                            name = "RequirePlatformSecurityFeatures",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. security-deviceguard-hvci-mat-required
                new TweakItem
                {
                    id = "security-deviceguard-hvci-mat-required",
                    name_ru = "Отключить требование таблицы UEFI MAT для HVCI",
                    description_ru = "Разрешает работу целостности памяти без строгой поддержки таблицы UEFI Memory Attributes Table на старых материнских платах.",
                    group = "Аппаратная безопасность и целостность",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1025, line_to = 1028 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard",
                            name = "HVCIMATRequired",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. security-deviceguard-machine-identity-isolation
                new TweakItem
                {
                    id = "security-deviceguard-machine-identity-isolation",
                    name_ru = "Отключить изоляцию идентификатора машины VBS",
                    description_ru = "Отключает фоновую изоляцию ключей идентификатора машины в виртуальном доверенном пространстве VBS.",
                    group = "Аппаратная безопасность и целостность",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1025, line_to = 1028 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard",
                            name = "MachineIdentityIsolation",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. system-fs-ntfs-allow-extended-char-8dot3
                new TweakItem
                {
                    id = "system-fs-ntfs-allow-extended-char-8dot3",
                    name_ru = "Ограничить короткие имена 8.3 базовым набором ASCII",
                    description_ru = "Запрещает использование расширенных диакритических символов при формировании коротких имен 8.3 в NTFS.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8515, line_to = 8517 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "NtfsAllowExtendedCharacter8dot3Rename",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 17. system-fs-ntfs-disable-compression-limit
                new TweakItem
                {
                    id = "system-fs-ntfs-disable-compression-limit",
                    name_ru = "Отключить предел сжатия фрагментированных файлов NTFS",
                    description_ru = "Принудительно обеспечивает сжатие сильно фрагментированных файлов без автоматического переключения в несжатый режим.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8519, line_to = 8521 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "NtfsDisableCompressionLimit",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 18. system-fs-ntfs-encrypt-paging-file
                new TweakItem
                {
                    id = "system-fs-ntfs-encrypt-paging-file",
                    name_ru = "Отключить шифрование файла подкачки NTFS",
                    description_ru = "Хранит файл подкачки pagefile.sys без шифрования, исключая процессорные накладные расходы при свопе памяти.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8523, line_to = 8525 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "NtfsEncryptPagingFile",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 19. system-fs-ntfs-memory-usage
                new TweakItem
                {
                    id = "system-fs-ntfs-memory-usage",
                    name_ru = "Увеличить кэш памяти NTFS (NtfsMemoryUsage 2)",
                    description_ru = "Выделяет увеличенный объем выгружаемого и невыгружаемого пула под внутренние структуры NTFS для ускорения работы с файлами.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8524, line_to = 8526 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "NtfsMemoryUsage",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 20. system-fs-ntfs-mft-zone-reservation
                new TweakItem
                {
                    id = "system-fs-ntfs-mft-zone-reservation",
                    name_ru = "Оптимальное резервирование зоны MFT (400 МБ)",
                    description_ru = "Увеличивает начальную зону Master File Table до 400 МБ, снижая фрагментацию таблицы файлов на дисках с тысячами мелких файлов.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8525, line_to = 8527 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "NtfsMftZoneReservation",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 21. system-fs-refs-disable-last-access-update
                new TweakItem
                {
                    id = "system-fs-refs-disable-last-access-update",
                    name_ru = "Отключить обновление времени последнего доступа для ReFS",
                    description_ru = "Предотвращает запись временной метки последнего обращения при чтении каталогов и файлов на томах ReFS.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8526, line_to = 8528 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "RefsDisableLastAccessUpdate",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 22. system-fs-win31-file-system
                new TweakItem
                {
                    id = "system-fs-win31-file-system",
                    name_ru = "Отключить устаревший режим Windows 3.1 для FAT",
                    description_ru = "Обеспечивает современное поведение FAT с поддержкой длинных имен файлов и точных временных меток.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8528, line_to = 8530 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "Win31FileSystem",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 23. peripheral-touchpad-right-click-zone
                new TweakItem
                {
                    id = "peripheral-touchpad-right-click-zone",
                    name_ru = "Включить зону правого клика в углу тачпада",
                    description_ru = "Активирует аппаратную нижнюю правую зону сенсорной панели для вызова контекстного меню кликом одним пальцем.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1440, line_to = 1442 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                            name = "RightClickZoneEnabled",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 24. peripheral-touchpad-scroll-direction
                new TweakItem
                {
                    id = "peripheral-touchpad-scroll-direction",
                    name_ru = "Классическое направление прокрутки тачпада (ScrollDirection 0)",
                    description_ru = "Устанавливает стандартное прямое направление прокрутки жестом двух пальцев (движение вниз опускает страницу).",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1442, line_to = 1444 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                            name = "ScrollDirection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 25. peripheral-pen-sys-use-hw-timestamp
                new TweakItem
                {
                    id = "peripheral-pen-sys-use-hw-timestamp",
                    name_ru = "Аппаратные временные метки пера (UseHWTimeStamp)",
                    description_ru = "Использует аппаратные метки времени контроллера дигитайзера для минимизации джиттера и задержки штриха.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1459, line_to = 1461 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Wisp\Pen\SysEventParameters",
                            name = "UseHWTimeStamp",
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
