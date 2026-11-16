using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock24Tool
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
                // 1. network-lltd-allow-lltdio-on-domain
                new TweakItem
                {
                    id = "network-lltd-allow-lltdio-on-domain",
                    name_ru = "Запретить работу драйвера сопоставителя топологии LLTDIO в домене",
                    description_ru = "Отключает драйвер сопоставителя Link-Layer Topology Discovery (LLTDIO) при подключении к доменной сети.",
                    group = "Топология сети (LLTD)",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 759, line_to = 761 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LLTD",
                            name = "AllowLLTDIOOnDomain",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. network-lltd-allow-rspndr-on-domain
                new TweakItem
                {
                    id = "network-lltd-allow-rspndr-on-domain",
                    name_ru = "Запретить работу драйвера ответчика топологии RSPNDR в домене",
                    description_ru = "Отключает драйвер ответчика Link-Layer Topology Discovery (RSPNDR) при подключении к доменной сети.",
                    group = "Топология сети (LLTD)",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 760, line_to = 762 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LLTD",
                            name = "AllowRspndrOnDomain",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. network-lltd-allow-rspndr-on-public-net
                new TweakItem
                {
                    id = "network-lltd-allow-rspndr-on-public-net",
                    name_ru = "Запретить работу драйвера ответчика топологии RSPNDR в публичных сетях",
                    description_ru = "Предотвращает обнаружение компьютера другими устройствами через протокол RSPNDR в общедоступных сетях.",
                    group = "Топология сети (LLTD)",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 760, line_to = 762 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LLTD",
                            name = "AllowRspndrOnPublicNet",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. peripheral-printers-disable-http-printing
                new TweakItem
                {
                    id = "peripheral-printers-disable-http-printing",
                    name_ru = "Отключить печать по сетевому протоколу HTTP",
                    description_ru = "Запрещает отправку заданий на печать на сетевые принтеры по протоколу HTTP/HTTPS для снижения сетевой активности.",
                    group = "Печать и принтеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1829, line_to = 1831 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows NT\Printers",
                            name = "DisableHTTPPrinting",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. privacy-handwriting-prevent-error-reports
                new TweakItem
                {
                    id = "privacy-handwriting-prevent-error-reports",
                    name_ru = "Запретить отправку отчетов об ошибках рукописного ввода",
                    description_ru = "Блокирует автоматическую передачу дампов и фрагментов рукописного текста при возникновении сбоев пера.",
                    group = "Рукописный ввод",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 125, line_to = 127 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\HandwritingErrorReports",
                            name = "PreventHandwritingErrorReports",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. privacy-handwriting-prevent-data-sharing
                new TweakItem
                {
                    id = "privacy-handwriting-prevent-data-sharing",
                    name_ru = "Запретить передачу образцов рукописного текста в Microsoft",
                    description_ru = "Запрещает отправку введенных от руки символов и рисунков для улучшения распознавания рукописного ввода.",
                    group = "Рукописный ввод",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 125, line_to = 127 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\HandwritingErrorReports",
                            name = "PreventHandwritingDataSharing",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-sqm-disable-ceip
                new TweakItem
                {
                    id = "privacy-sqm-disable-ceip",
                    name_ru = "Запретить участие в программе улучшения качества ПО (CEIP Internet Explorer)",
                    description_ru = "Отключает сбор данных об использовании и телеметрию компонентов Internet Explorer / WebBrowser.",
                    group = "Телеметрия Internet Explorer",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 133, line_to = 135 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Internet Explorer\SQM",
                            name = "DisableCustomerImprovementProgram",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. privacy-wer-auto-approve-os-dumps
                new TweakItem
                {
                    id = "privacy-wer-auto-approve-os-dumps",
                    name_ru = "Запретить автоматическую отправку дампов памяти при системных сбоях",
                    description_ru = "Блокирует автоматическую фоновую передачу дампов оперативной памяти службы Windows Error Reporting.",
                    group = "Отчеты об ошибках (WER)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 335, line_to = 337 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting",
                            name = "AutoApproveOSDumps",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. privacy-explorer-hide-recommended-section
                new TweakItem
                {
                    id = "privacy-explorer-hide-recommended-section",
                    name_ru = "Скрыть раздел Рекомендуем в меню Пуск",
                    description_ru = "Убирает раздел рекомендуемых файлов и приложений из меню Пуск Windows 11.",
                    group = "Меню Пуск",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 450, line_to = 452 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "HideRecommendedSection",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. privacy-explorer-hide-recommended-personalized-sites
                new TweakItem
                {
                    id = "privacy-explorer-hide-recommended-personalized-sites",
                    name_ru = "Скрыть персональные рекомендации сайтов в меню Пуск",
                    description_ru = "Отключает отображение ссылок на рекомендуемые веб-сайты на основе истории активности пользователя в меню Пуск.",
                    group = "Меню Пуск",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 451, line_to = 453 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "HideRecommendedPersonalizedSites",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. privacy-ai-disable-recall-snapshots
                new TweakItem
                {
                    id = "privacy-ai-disable-recall-snapshots",
                    name_ru = "Запретить сохранение снимков экрана для службы Windows Recall",
                    description_ru = "Полностью блокирует регулярное создание и анализ локальных скриншотов рабочего стола AI-службой Recall.",
                    group = "Windows Recall & Copilot",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 845, line_to = 847 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI",
                            name = "DisableAIDataAnalysis",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. privacy-ai-allow-recall-enablement
                new TweakItem
                {
                    id = "privacy-ai-allow-recall-enablement",
                    name_ru = "Запретить принудительное включение функций Windows Recall",
                    description_ru = "Запрещает системе автоматически активировать функции Recall при установке обновлений или входе.",
                    group = "Windows Recall & Copilot",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 846, line_to = 848 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI",
                            name = "AllowRecallEnablement",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. privacy-ai-disable-click-to-do
                new TweakItem
                {
                    id = "privacy-ai-disable-click-to-do",
                    name_ru = "Отключить интерактивный контекстный AI-инструмент Click to Do",
                    description_ru = "Отключает фоновый экранный парсер и контекстные подсказки AI-модели Click to Do в Windows.",
                    group = "Windows Recall & Copilot",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 847, line_to = 849 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI",
                            name = "DisableClickToDo",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-biometrics-enabled
                new TweakItem
                {
                    id = "privacy-biometrics-enabled",
                    name_ru = "Глобально запретить использование биометрических служб Windows",
                    description_ru = "Отключает работу службы биометрии Windows Biometric Service для сканеров отпечатков и распознавания лиц.",
                    group = "Биометрия и Windows Hello",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1003, line_to = 1005 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Biometrics",
                            name = "Enabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-biometrics-credential-provider-enabled
                new TweakItem
                {
                    id = "privacy-biometrics-credential-provider-enabled",
                    name_ru = "Запретить вход в Windows по биометрии (Windows Hello)",
                    description_ru = "Блокирует использование биометрических данных при авторизации на экране входа Windows.",
                    group = "Биометрия и Windows Hello",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1004, line_to = 1006 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider",
                            name = "Enabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. privacy-settingsync-disable-user-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-user-override",
                    name_ru = "Запретить пользователю включать синхронизацию параметров",
                    description_ru = "Блокирует переключатель синхронизации системных параметров в интерфейсе приложения Параметры.",
                    group = "Синхронизация параметров",
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
                            name = "DisableSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. privacy-settingsync-disable-credentials-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-credentials-override",
                    name_ru = "Запретить пользователю включать синхронизацию паролей",
                    description_ru = "Блокирует возможность включения облачной синхронизации сохраненных паролей и учетных данных.",
                    group = "Синхронизация параметров",
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
                            name = "DisableCredentialsSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-settingsync-disable-personalization-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-personalization-override",
                    name_ru = "Запретить пользователю включать синхронизацию персонализации",
                    description_ru = "Блокирует возможность включения облачной синхронизации темы, фона и цветов интерфейса.",
                    group = "Синхронизация параметров",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1167, line_to = 1169 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisablePersonalizationSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-netcache-enabled
                new TweakItem
                {
                    id = "privacy-netcache-enabled",
                    name_ru = "Полностью отключить подсистему автономных файлов (NetCache Offline Files)",
                    description_ru = "Глобально выключает компонент Offline Files через системную политику для высвобождения сетевых ресурсов.",
                    group = "Автономные файлы (NetCache)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1332, line_to = 1334 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\NetCache",
                            name = "Enabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. security-smartscreen-edge-phishing-filter-v9
                new TweakItem
                {
                    id = "security-smartscreen-edge-phishing-filter-v9",
                    name_ru = "Отключить фильтр SmartScreen в Microsoft Edge (v9)",
                    description_ru = "Отключает телеметрию и отправку посещаемых адресов веб-страниц фильтром SmartScreen браузера Edge.",
                    group = "Фильтр SmartScreen",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 278, line_to = 280 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\MicrosoftEdge\PhishingFilter",
                            name = "EnabledV9",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. security-smartscreen-edge-enabled
                new TweakItem
                {
                    id = "security-smartscreen-edge-enabled",
                    name_ru = "Отключить фильтр SmartScreen на уровне браузерного ядра Edge",
                    description_ru = "Отключает проверку загружаемых файлов и онлайн-репутации на уровне базовой политики Edge.",
                    group = "Фильтр SmartScreen",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 280, line_to = 282 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Edge",
                            name = "SmartScreenEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. system-start-disable-account-notifications
                new TweakItem
                {
                    id = "system-start-disable-account-notifications",
                    name_ru = "Отключить уведомления учетной записи в меню Пуск",
                    description_ru = "Отключает оранжевые значки и рекламные уведомления учетной записи Microsoft в профиле меню Пуск.",
                    group = "Меню Пуск",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8404 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\AccountNotifications",
                            name = "DisableAccountNotifications",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. system-push-notifications-no-toast
                new TweakItem
                {
                    id = "system-push-notifications-no-toast",
                    name_ru = "Отключить всплывающие тост-уведомления приложений",
                    description_ru = "Подавляет всплывающие прямоугольные тост-уведомления от приложений в правом нижнем углу экрана.",
                    group = "Push-уведомления (WPN)",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8414, line_to = 8416 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                            name = "NoToastApplicationNotification",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. system-start-show-hide-most-used-apps
                new TweakItem
                {
                    id = "system-start-show-hide-most-used-apps",
                    name_ru = "Скрыть список Часто используемые приложения в меню Пуск",
                    description_ru = "Запрещает отображение списка часто используемых программ в верхней части меню Пуск.",
                    group = "Меню Пуск",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9160, line_to = 9162 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "ShowOrHideMostUsedApps",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. visibility-explorer-no-recent-docs-history
                new TweakItem
                {
                    id = "visibility-explorer-no-recent-docs-history",
                    name_ru = "Не сохранять историю недавно открывавшихся документов",
                    description_ru = "Отключает автоматическое отслеживание и сохранение списка недавних документов в Проводнике.",
                    group = "Проводник и рабочий стол",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 336, line_to = 338 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "NoRecentDocsHistory",
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

            Console.WriteLine("Новое число твиков после добавления Блока 24: " + list.Count);

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
