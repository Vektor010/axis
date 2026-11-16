using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock22Tool
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
                // 1. security-wtds-notify-malicious
                new TweakItem
                {
                    id = "security-wtds-notify-malicious",
                    name_ru = "Уведомлять при обнаружении вредоносного содержимого WTDS",
                    description_ru = "Включает мгновенные системные оповещения службы Web Threat Defense при перехвате фишинга и вредоносных скриптов.",
                    group = "Защита от веб-угроз",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 282, line_to = 284 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WTDS\Components",
                            name = "NotifyMalicious",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. security-wtds-capture-threat-window
                new TweakItem
                {
                    id = "security-wtds-capture-threat-window",
                    name_ru = "Отключить захват снимка экрана окна с угрозой WTDS",
                    description_ru = "Запрещает службе Web Threat Defense создавать и отправлять скриншоты активных окон при обнаружении сетевой угрозы.",
                    group = "Защита от веб-угроз",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 285, line_to = 287 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WTDS\Components",
                            name = "CaptureThreatWindow",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. security-defender-disable-behavior-monitoring
                new TweakItem
                {
                    id = "security-defender-disable-behavior-monitoring",
                    name_ru = "Включить мониторинг поведения подозрительных процессов Защитника",
                    description_ru = "Активирует непрерывный эвристический анализ поведения процессов для блокировки неизвестных программ-вымогателей и эксплойтов.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 287, line_to = 289 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection",
                            name = "DisableBehaviorMonitoring",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. security-defender-disable-ioav-protection
                new TweakItem
                {
                    id = "security-defender-disable-ioav-protection",
                    name_ru = "Включить проверку всех загружаемых файлов и вложений IOAV",
                    description_ru = "Включает обязательное антивирусное сканирование всех файлов, скачиваемых через браузеры или получаемых в качестве вложений.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 288, line_to = 290 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection",
                            name = "DisableIOAVProtection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. security-defender-disable-on-access-protection
                new TweakItem
                {
                    id = "security-defender-disable-on-access-protection",
                    name_ru = "Включить постоянную защиту при обращении к файлам (On-Access Protection)",
                    description_ru = "Гарантирует работу защиты на лету при каждом открытии, копировании или запуске исполняемых файлов на диске.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 289, line_to = 291 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection",
                            name = "DisableOnAccessProtection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. security-defender-disable-scan-realtime-enable
                new TweakItem
                {
                    id = "security-defender-disable-scan-realtime-enable",
                    name_ru = "Включить экспресс-сканирование при активации защиты реального времени",
                    description_ru = "Запускает быструю фоновую проверку критических системных областей при каждом включении антивирусного модуля.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 291, line_to = 293 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection",
                            name = "DisableScanOnRealtimeEnable",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. security-defender-disable-generic-reports
                new TweakItem
                {
                    id = "security-defender-disable-generic-reports",
                    name_ru = "Отключить отправку стандартных отчетов Watson для Защитника",
                    description_ru = "Отключает фоновую отправку общих телеметрических отчетов Watson в Microsoft при событиях обнаружения угроз.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 292, line_to = 294 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Reporting",
                            name = "DisableGenericRePorts",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. security-defender-disable-email-scanning
                new TweakItem
                {
                    id = "security-defender-disable-email-scanning",
                    name_ru = "Включить сканирование входящих почтовых сообщений и баз писем",
                    description_ru = "Активирует проверку файлов баз данных почтовых клиентов и почтовых сообщений на наличие вложенных троянов и фишинга.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 294, line_to = 296 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Scan",
                            name = "DisableEmailScanning",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. security-defender-signature-disable-notification
                new TweakItem
                {
                    id = "security-defender-signature-disable-notification",
                    name_ru = "Отключить уведомления об отключении отчетов на основе сигнатур MAPS",
                    description_ru = "Подавляет всплывающие предупреждения при локальном ограничении отправки диагностических отчетов в сеть Microsoft MAPS.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 296, line_to = 298 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Signature Updates",
                            name = "SignatureDisableNotification",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. security-defender-submit-samples-consent
                new TweakItem
                {
                    id = "security-defender-submit-samples-consent",
                    name_ru = "Запретить отправку образцов подозрительных файлов в Microsoft Spynet",
                    description_ru = "Полностью запрещает автоматическую передачу пользовательских файлов на удаленный анализ в облачный центр безопасности Microsoft Spynet.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 300, line_to = 302 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Spynet",
                            name = "SubmitSamplesConsent",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. security-defender-cloud-block-level
                new TweakItem
                {
                    id = "security-defender-cloud-block-level",
                    name_ru = "Установить высокий уровень облачной блокировки угроз (MpCloudBlockLevel High)",
                    description_ru = "Переводит облачный фильтр антивируса в строгий режим блокировки сомнительных файлов до подтверждения их безопасности.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 301, line_to = 303 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\MpEngine",
                            name = "MpCloudBlockLevel",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. security-defender-bafs-extended-timeout
                new TweakItem
                {
                    id = "security-defender-bafs-extended-timeout",
                    name_ru = "Увеличить таймаут облачной проверки Block at First Sight до 50 секунд",
                    description_ru = "Увеличивает предельное время ожидания ответа облачной аналитики до 50 секунд перед разрешением запуска нового неизвестного файла.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 302, line_to = 304 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\MpEngine",
                            name = "MpBafsExtendedTimeout",
                            type = "REG_DWORD",
                            apply = 50,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. security-store-auto-download
                new TweakItem
                {
                    id = "security-store-auto-download",
                    name_ru = "Отключить автоматическое скачивание и установку обновлений Windows Store",
                    description_ru = "Запрещает Microsoft Store самостоятельно в фоновом режиме загружать обновления приложений без запроса пользователя.",
                    group = "Магазин Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 491, line_to = 493 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\WindowsStore",
                            name = "AutoDownload",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. security-wu-target-release-version
                new TweakItem
                {
                    id = "security-wu-target-release-version",
                    name_ru = "Зафиксировать целевую версию выпуска компонентов Windows Update",
                    description_ru = "Включает политику строгого удержания системы на выбранной версии сборки, предотвращая принудительные крупные апгрейды.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "advanced" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 496, line_to = 498 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "TargetReleaseVersion",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. security-wu-manage-preview-builds
                new TweakItem
                {
                    id = "security-wu-manage-preview-builds",
                    name_ru = "Запретить автоматическую установку предварительных сборок (Preview Builds)",
                    description_ru = "Блокирует участие компьютера в программах предварительной оценки и скачивание нестабильных тестовых сборок Insider.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 497, line_to = 499 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "ManagePreviewBuildsPolicyValue",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. security-wu-defer-quality-updates
                new TweakItem
                {
                    id = "security-wu-defer-quality-updates",
                    name_ru = "Включить отсрочку накопительных обновлений качества (Quality Updates)",
                    description_ru = "Активирует политику отсрочки установки регулярных накопительных патчей для исключения проблем с новыми сырыми апдейтами.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "advanced" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 498, line_to = 500 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "DeferQualityUpdates",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. security-wu-disable-ux-access
                new TweakItem
                {
                    id = "security-wu-disable-ux-access",
                    name_ru = "Скрыть доступ пользователя к интерфейсу Центра обновления Windows",
                    description_ru = "Отключает доступ к странице Центра обновления в приложении Параметры, исключая случайный запуск обновлений.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "advanced" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 500, line_to = 502 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "SetDisableUXWUAccess",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-terminal-disable-clip
                new TweakItem
                {
                    id = "privacy-terminal-disable-clip",
                    name_ru = "Запретить перенаправление буфера обмена в сеансах удаленного рабочего стола",
                    description_ru = "Блокирует автоматическую передачу содержимого буфера обмена через RDP, защищая скопированные конфиденциальные данные.",
                    group = "Удаленный рабочий стол",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 738, line_to = 740 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services",
                            name = "fDisableClip",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-copilot-hardware-key
                new TweakItem
                {
                    id = "privacy-copilot-hardware-key",
                    name_ru = "Отключить принудительное действие аппаратной клавиши Copilot",
                    description_ru = "Отключает принудительный запуск веб-ассистента Copilot при случайном нажатии физической аппаратной клавиши клавиатуры.",
                    group = "Клавиатура и горячие клавиши",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 826, line_to = 828 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\CopilotKey",
                            name = "SetCopilotHardwareKey",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. privacy-biometrics-domain-accounts
                new TweakItem
                {
                    id = "privacy-biometrics-domain-accounts",
                    name_ru = "Запретить доменным пользователям вход в систему по биометрическим данным",
                    description_ru = "Блокирует аутентификацию доменных учетных записей по отпечатку пальца или распознаванию лица через групповую политику.",
                    group = "Биометрия и безопасность",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1005, line_to = 1007 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider",
                            name = "Domain Accounts",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. privacy-terminal-allow-unsolicited
                new TweakItem
                {
                    id = "privacy-terminal-allow-unsolicited",
                    name_ru = "Запретить несанкционированные предложения удаленной помощи (Remote Assistance)",
                    description_ru = "Запрещает удаленным хостам отправлять незапрошенные сеансы подключения через службу удаленного помощника Windows.",
                    group = "Удаленный рабочий стол",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1031, line_to = 1033 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\policies\Microsoft\Windows NT\Terminal Services",
                            name = "fAllowUnsolicited",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. privacy-sync-disable-setting-sync
                new TweakItem
                {
                    id = "privacy-sync-disable-setting-sync",
                    name_ru = "Полностью отключить облачную синхронизацию всех системных настроек",
                    description_ru = "Мастер-переключатель групповой политики: полностью отключает фоновую службу синхронизации SettingSync на уровне системы.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1164, line_to = 1166 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. privacy-sync-disable-app-setting-sync
                new TweakItem
                {
                    id = "privacy-sync-disable-app-setting-sync",
                    name_ru = "Отключить облачную синхронизацию настроек приложений Windows",
                    description_ru = "Запрещает синхронизировать внутренние данные, конфигурации и пользовательские параметры программ через облако Microsoft.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1165, line_to = 1167 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableApplicationSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. privacy-sync-disable-credentials-sync
                new TweakItem
                {
                    id = "privacy-sync-disable-credentials-sync",
                    name_ru = "Отключить облачную синхронизацию паролей и сохраненных учетных данных",
                    description_ru = "Блокирует отправку паролей от веб-сайтов, сетей Wi-Fi и сертификатов в облачное хранилище учетной записи Microsoft.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1166, line_to = 1168 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableCredentialsSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. privacy-sync-disable-start-layout-sync
                new TweakItem
                {
                    id = "privacy-sync-disable-start-layout-sync",
                    name_ru = "Отключить облачную синхронизацию структуры и макета меню Пуск",
                    description_ru = "Запрещает передачу сетки закрепленных приложений, плиток и структуры меню Пуск на другие компьютеры пользователя.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1173, line_to = 1175 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableStartLayoutSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            foreach (var t in newTweaks)
            {
                list.Add(t);
            }

            Console.WriteLine("Новое число твиков после добавления Блока 22: " + list.Count);

            string jsonFormatted = FormatJson(serializer.Serialize(list));
            File.WriteAllText(jsonPath, jsonFormatted, Encoding.UTF8);

            Console.WriteLine("tweaks.json успешно сохранен!");
        }

        private static string FormatJson(string json)
        {
            var sb = new StringBuilder();
            int indent = 0;
            bool inQuote = false;

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (ch == '\"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inQuote = !inQuote;
                    sb.Append(ch);
                }
                else if (!inQuote)
                {
                    if (ch == '{' || ch == '[')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        indent += 2;
                        sb.Append(new string(' ', indent));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        sb.Append("\r\n");
                        indent -= 2;
                        if (indent < 0) indent = 0;
                        sb.Append(new string(' ', indent));
                        sb.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent));
                    }
                    else if (ch == ':')
                    {
                        sb.Append(": ");
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
}
