using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

class Program {
    static Dictionary<string, object> CreateTweak(
        string id,
        string nameRu,
        string descRu,
        string category,
        string group,
        string risk,
        string sourceFile,
        int lineFrom,
        int lineTo,
        string hive,
        string path,
        string name,
        string type,
        object applyVal,
        string revertMode = "delete",
        object revertVal = null)
    {
        var tweak = new Dictionary<string, object>();
        tweak["id"] = id;
        tweak["name_ru"] = nameRu;
        tweak["description_ru"] = descRu;
        tweak["category"] = category;
        tweak["group"] = group;
        tweak["type"] = "toggle";
        tweak["risk"] = risk;
        tweak["presets"] = new object[] { };
        tweak["needs_review"] = true;

        var source = new Dictionary<string, object>();
        source["file"] = sourceFile;
        source["line_from"] = lineFrom;
        source["line_to"] = lineTo;
        tweak["source"] = source;

        var regList = new List<object>();
        var reg = new Dictionary<string, object>();
        reg["hive"] = hive;
        reg["path"] = path;
        reg["name"] = name;
        reg["type"] = type;
        reg["apply"] = applyVal;
        reg["apply_block"] = "Enabled";

        var revert = new Dictionary<string, object>();
        revert["mode"] = revertMode;
        if (revertVal != null) revert["value"] = revertVal;
        reg["revert"] = revert;

        regList.Add(reg);
        tweak["registry"] = regList;

        return tweak;
    }

    static void Main() {
        string tweaksPath = @"data\tweaks.json";
        var ser = new JavaScriptSerializer();
        ser.MaxJsonLength = int.MaxValue;
        var list = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksPath, Encoding.UTF8));

        Console.WriteLine("Начальное количество твиков: {0}", list.Count);

        var block34 = new List<Dictionary<string, object>>();

        // 1-6: Security / TDR Tweaks (security/desc.md)
        block34.Add(CreateTweak(
            "system-tdr-level",
            "Уровень восстановления видеодрайвера (TdrLevel)",
            "Определяет поведение подсистемы TDR (Timeout Detection and Recovery) при зависании GPU: 3 — восстановление видеоадаптера по таймауту (по умолчанию), 0 — отключение обнаружения зависаний.",
            "security", "Security", "low",
            "security/desc.md", 1194, 1199,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrLevel", "REG_DWORD", 0));

        block34.Add(CreateTweak(
            "system-tdr-debug-mode",
            "Режим отладки TDR (TdrDebugMode)",
            "Определяет поведение отладки при возникновении таймаута видеодрайвера: 2 — восстановление без прерывания в отладчик ядра (по умолчанию), 1 — игнорирование таймаутов, 0 — прерывание в отладчик ядра.",
            "security", "Security", "low",
            "security/desc.md", 1207, 1212,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDebugMode", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-tdr-limit-time",
            "Временное окно счетчика TDR (TdrLimitTime)",
            "Задает интервал времени (в секундах, по умолчанию 60 с), в течение которого допускается предельное число сбоев видеодрайвера (TdrLimitCount) до вызова BSOD VIDEO_TDR_FAILURE.",
            "security", "Security", "low",
            "security/desc.md", 1214, 1216,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrLimitTime", "REG_DWORD", 120));

        block34.Add(CreateTweak(
            "system-tdr-limit-count",
            "Лимит сбоев видеодрайвера TDR (TdrLimitCount)",
            "Задает максимальное число попыток восстановления видеодрайвера (код 0x117, по умолчанию 5) за интервал TdrLimitTime, после превышения которого система производит аварийную остановку (BSOD).",
            "security", "Security", "low",
            "security/desc.md", 1217, 1219,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrLimitCount", "REG_DWORD", 10));

        block34.Add(CreateTweak(
            "system-tdr-dod-present-delay",
            "Таймаут Present драйвера дисплея KMDOD (TdrDodPresentDelay)",
            "Задает допустимое время (в секундах, по умолчанию 2 с) для завершения асинхронной операции Present драйвером дисплея KMDOD до инициации процедуры восстановления TDR.",
            "security", "Security", "low",
            "security/desc.md", 1225, 1227,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDodPresentDelay", "REG_DWORD", 5));

        block34.Add(CreateTweak(
            "system-tdr-dod-vsync-delay",
            "Таймаут ожидания VSync драйвером KMDOD (TdrDodVSyncDelay)",
            "Задает допустимое время (в секундах, по умолчанию 2 с) сторожевого таймера VSync до инициации процедуры восстановления TDR в драйвере KMDOD.",
            "security", "Security", "low",
            "security/desc.md", 1228, 1230,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDodVSyncDelay", "REG_DWORD", 5));

        // 7-17: DXG Kernel / GPU Scheduling (system/desc.md)
        block34.Add(CreateTweak(
            "system-dxg-force-direct-flip",
            "Принудительный DirectFlip в подсистеме DXG (ForceDirectFlip)",
            "Принудительно устанавливает возможность DirectFlip (SupportDirectFlip в D3DKMDT_PREEMPTION_CAPS) для обхода промежуточных буферов композиции DWM и снижения задержки вывода кадров.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5274, 5275,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "ForceDirectFlip", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-force-variable-refresh",
            "Принудительная переменная частота обновления DXG (ForceVariableRefresh)",
            "Принудительно включает поддержку переменной частоты обновления экрана (VRR/G-Sync/FreeSync) на уровне ядра dxgkrnl.sys для полноэкранных и оконных графических приложений.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5284, 5284,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "ForceVariableRefresh", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-known-process-boost-mode",
            "Ускорение известных графических процессов (KnownProcessBoostMode)",
            "Активирует режим автоматического приоритетного ускорения графических очередей для распознанных игровых и графических процессов в планировщике dxgkrnl.sys (Windows 11 25H2).",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5292, 5292,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "KnownProcessBoostMode", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-small-quantum-mode",
            "Режим уменьшенных квантов времени GPU (SmallQuantumMode)",
            "Настраивает кванты времени диспетчера VidSch для контекстов GPU: 0 — отключено (максимальная пропускная способность), 1 — авто, 2 — принудительно (минимизация задержки переключения задач).",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5293, 5295,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "SmallQuantumMode", "REG_DWORD", 2));

        block34.Add(CreateTweak(
            "system-dxg-high-priority-completion-mode",
            "Высокоприоритетное завершение пакетов GPU (HighPriorityCompletionMode)",
            "Управляет высокоприоритетной обработкой пакетов завершения команд GPU в dxgmms2.sys (Windows 11 25H2), ускоряя передачу сигналов синхронизации кадров прикладному потоку.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5296, 5296,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HighPriorityCompletionMode", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-gpu-priority-change-mode",
            "Динамическая смена приоритетов GPU (GpuPriorityChangeMode)",
            "Включает динамическую смену приоритетов очередей GPU на основе контекста выполнения (Windows 11 25H2), гарантируя максимальный приоритет активному игровому процессу на переднем плане.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5297, 5297,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "GpuPriorityChangeMode", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-runtime-power-management",
            "Управление питанием GPU во время выполнения (EnableRuntimePowerManagement)",
            "Управляет рантайм-энергосбережением графического адаптера (Runtime D3 power management в dxgkrnl.sys). Отключение переводит адаптер в постоянное состояние максимальной готовности.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5331, 5331,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "EnableRuntimePowerManagement", "REG_DWORD", 0));

        block34.Add(CreateTweak(
            "system-dxg-hwsch-override-blocklist",
            "Игнорирование черного списка оборудования HAGS (HwSchOverrideBlockList)",
            "Принудительно разрешает аппаратное планирование GPU (HAGS / WDDM GPU Scheduling) даже на видеокартах или версиях драйверов, внесенных Microsoft в список блокировки совместимости.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5387, 5387,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchOverrideBlockList", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-hwsch-treat-experimental-as-stable",
            "Активация экспериментальных режимов HAGS (HwSchTreatExperimentalAsStable)",
            "Предписывает подсистеме dxgkrnl.sys обрабатывать экспериментальные и бета-функции аппаратного планирования графики HAGS как стабильные, активируя их для текущего дисплея.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5388, 5388,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchTreatExperimentalAsStable", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-experimental-refresh-rates",
            "Экспериментальные частоты обновления дисплея (EnableExperimentalRefreshRates)",
            "Разрешает диспетчеру VIDPN_MGR в dxgkrnl.sys применять нестандартные и сверхвысокие частоты развертки монитора (Windows 11 25H2), игнорируя консервативные фильтры драйвера.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5391, 5391,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "EnableExperimentalRefreshRates", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "system-dxg-miracast-force-disable",
            "Отключение беспроводной проекции Miracast на уровне ядра (MiracastForceDisable)",
            "Принудительно отключает стек беспроводного дисплея Miracast (DpiMiracastGetForcedMode) в dxgkrnl.sys, предотвращая выделение фоновых ресурсов под RTSP-стриминг экрана.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5406, 5409,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "MiracastForceDisable", "REG_DWORD", 1));

        // 18-21: Network SMB Server & Client (network/desc.md)
        block34.Add(CreateTweak(
            "network-smb-server-encrypt-data",
            "Принудительное шифрование трафика SMB-сервера (EncryptData)",
            "Требует обязательного сквозного шифрования (AES-128/256-GCM) всех сетевых пакетов SMB при обращении к общим ресурсам компьютера, защищая передаваемые файлы от перехвата в локальной сети.",
            "network", "Network Services", "low",
            "network/desc.md", 516, 519,
            "HKLM", @"System\CurrentControlSet\Services\LanmanServer\Parameters", "EncryptData", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "network-smb-server-reject-unencrypted",
            "Запрет незашифрованного доступа к SMB-серверу (RejectUnencryptedAccess)",
            "Автоматически отклоняет попытки подключения клиентов SMB, не поддерживающих или не использующих шифрование данных, обеспечивая строгую безопасность сетевых ресурсов.",
            "network", "Network Services", "low",
            "network/desc.md", 520, 522,
            "HKLM", @"System\CurrentControlSet\Services\LanmanServer\Parameters", "RejectUnencryptedAccess", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "network-smb-workstation-quic",
            "Поддержка SMB over QUIC для клиента (EnableSMBQUIC)",
            "Включает транспорт SMB over QUIC (UDP порт 443 с шифрованием TLS 1.3) для клиентской службы LanmanWorkstation, обеспечивая высокоскоростное и безопасное соединение с файловыми серверами без VPN.",
            "network", "Network Services", "low",
            "network/desc.md", 538, 541,
            "HKLM", @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters", "EnableSMBQUIC", "REG_DWORD", 1));

        block34.Add(CreateTweak(
            "network-smb-server-quic",
            "Поддержка SMB over QUIC для сервера (EnableSMBQUIC)",
            "Включает транспорт SMB over QUIC для службы LanmanServer, позволяя защищенным клиентам передавать файлы через современный протокол QUIC поверх UDP без накладных расходов традиционного TCP.",
            "network", "Network Services", "low",
            "network/desc.md", 542, 544,
            "HKLM", @"System\CurrentControlSet\Services\LanmanServer\Parameters", "EnableSMBQUIC", "REG_DWORD", 1));

        // 22-25: Network HW Offloads (network/desc.md)
        block34.Add(CreateTweak(
            "network-adapter-force-rsc",
            "Принудительное включение Receive Segment Coalescing (ForceRscEnabled)",
            "Принудительно активирует механизм объединения входящих TCP-сегментов (RSC) на уровне сетевого адаптера, объединяя фрагменты пакетов до передачи их в стек ядра для снижения загрузки процессора.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 314, 314,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "ForceRscEnabled", "REG_SZ", "1"));

        block34.Add(CreateTweak(
            "network-adapter-encapsulated-packet-task-offload",
            "Аппаратная разгрузка инкапсулированных пакетов (*EncapsulatedPacketTaskOffload)",
            "Включает разгрузку контрольных сумм и сегментации для сетевых пакетов, инкапсулированных в туннели (NVGRE / VXLAN), позволяя адаптеру обрабатывать их аппаратно без задержек.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 317, 317,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "*EncapsulatedPacketTaskOffload", "REG_SZ", "1"));

        block34.Add(CreateTweak(
            "network-adapter-encapsulated-nvgre",
            "Разгрузка туннельных пакетов NVGRE (*EncapsulatedPacketTaskOffloadNvgre)",
            "Включает аппаратную обработку контрольных сумм и LSO для виртуализованных пакетов протокола NVGRE на сетевом адаптере.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 318, 318,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "*EncapsulatedPacketTaskOffloadNvgre", "REG_SZ", "1"));

        block34.Add(CreateTweak(
            "network-adapter-tcp-connection-offload-ipv4",
            "Разгрузка соединений TCP Chimney IPv4 (*TCPConnectionOffloadIPv4)",
            "Разрешает сетевому адаптеру разгружать обработку активных TCP-соединений IPv4 (TCP Chimney Offload) из программного стека операционной системы в процессор сетевой карты.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 326, 326,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "*TCPConnectionOffloadIPv4", "REG_SZ", "1"));

        Console.WriteLine("Сформировано новых твиков: {0}", block34.Count);
        list.AddRange(block34);
        Console.WriteLine("Новое общее количество твиков: {0}", list.Count);

        string jsonOutput = ser.Serialize(list);
        StringBuilder sb = new StringBuilder();
        int indent = 0;
        bool inQuote = false;
        for (int i = 0; i < jsonOutput.Length; i++) {
            char ch = jsonOutput[i];
            if (ch == '"' && (i == 0 || jsonOutput[i - 1] != '\\')) {
                inQuote = !inQuote;
                sb.Append(ch);
            } else if (!inQuote) {
                if (ch == '{' || ch == '[') {
                    sb.Append(ch);
                    sb.Append("\r\n");
                    indent++;
                    sb.Append(new string(' ', indent * 2));
                } else if (ch == '}' || ch == ']') {
                    sb.Append("\r\n");
                    indent--;
                    sb.Append(new string(' ', indent * 2));
                    sb.Append(ch);
                } else if (ch == ',') {
                    sb.Append(ch);
                    sb.Append("\r\n");
                    sb.Append(new string(' ', indent * 2));
                } else if (ch == ':') {
                    sb.Append(": ");
                } else {
                    sb.Append(ch);
                }
            } else {
                sb.Append(ch);
            }
        }

        File.WriteAllText(tweaksPath, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine("Файл data/tweaks.json успешно обновлен до 825 твиков!");
    }
}
