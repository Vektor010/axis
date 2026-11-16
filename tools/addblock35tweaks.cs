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

        var block35 = new List<Dictionary<string, object>>();

        // 1-8: DWM Compositor & Engine (system/desc.md)
        block35.Add(CreateTweak(
            "system-dwm-parallel-mode-policy",
            "Политика параллельного режима DWM (ParallelModePolicy)",
            "Задает политику параллельного выполнения потоков планировщика DWM: 2 — принудительный параллельный режим для минимизации задержек кадра, 1 — стандартный режим.",
            "system", "DWM Engine", "low",
            "system/desc.md", 2788, 2788,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "ParallelModePolicy", "REG_DWORD", 2));

        block35.Add(CreateTweak(
            "system-dwm-resample-in-linear-space",
            "Ресемплинг DWM в линейном пространстве (ResampleInLinearSpace)",
            "Принудительно переводит алгоритмы ресемплинга и масштабирования DWM в линейное цветовое пространство (формат пикселей 91), повышая четкость и точность цветопередачи при скейлинге.",
            "system", "DWM Optimization", "low",
            "system/desc.md", 2789, 2789,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "ResampleInLinearSpace", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dwm-resample-mode-override",
            "Переопределение режима ресемплинга DWM (ResampleModeOverride)",
            "Переопределяет внутренний шейдер масштабирования оконного менеджера DWM: 1 — высококачественный фильтр Lanczos, 2 — XBR, 0 — стандартный запрашиваемый режим.",
            "system", "DWM Optimization", "low",
            "system/desc.md", 2790, 2790,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "ResampleModeOverride", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dwm-custom-refresh-rate-mode",
            "Кастомный режим частоты обновления DWM (CustomRefreshRateMode)",
            "Активирует кастомную обработку частоты обновления дисплеев в композиторе DWM для устранения микростаттеров при несовпадении частот нескольких мониторов.",
            "system", "DWM Optimization", "low",
            "system/desc.md", 2778, 2778,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "CustomRefreshRateMode", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dwm-render-thread-timeout",
            "Таймаут потока рендеринга DWM (RenderThreadTimeoutMilliseconds)",
            "Порог времени ожидания (в миллисекундах, по умолчанию 5000 мс) цикла планировщика композитора DWM до регистрации сбоя зависания потока отрисовки.",
            "system", "DWM Engine", "low",
            "system/desc.md", 2766, 2766,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "RenderThreadTimeoutMilliseconds", "REG_DWORD", 10000));

        block35.Add(CreateTweak(
            "system-dwm-gamma-blend-fp16",
            "Гамма-блендинг DWM с точностью FP16 (GammaBlendWithFP16)",
            "Включает вычисления гамма-смешивания композитора DWM с половинной точностью с плавающей запятой (FP16), исключая артефакты постеризации и градиентных полос.",
            "system", "DWM Optimization", "low",
            "system/desc.md", 2750, 2750,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "GammaBlendWithFP16", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dwm-uniform-space-dpi-mode",
            "Единое координатное пространство DPI в DWM (UniformSpaceDpiMode)",
            "Переводит координатную сетку композитора DWM в единое масштабированное пространство, устраняя размытие и рывки при перетаскивании окон между экранами с разным DPI.",
            "system", "DWM Engine", "low",
            "system/desc.md", 2772, 2772,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "UniformSpaceDpiMode", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dwm-use-hw-draw-list-warp",
            "Аппаратные списки отрисовки на WARP (UseHWDrawListEntriesOnWARP)",
            "Разрешает программному растеризатору WARP использовать аппаратные оптимизации списков команд отрисовки DWM DrawList.",
            "system", "DWM Engine", "low",
            "system/desc.md", 2773, 2773,
            "HKLM", @"SOFTWARE\Microsoft\Windows DWM", "UseHWDrawListEntriesOnWARP", "REG_DWORD", 1));

        // 9-16: DXG Kernel & WDDM (system/desc.md)
        block35.Add(CreateTweak(
            "system-dxg-enable-decode-mpo",
            "Аппаратное декодирование Multi-Plane Overlay (EnableDecodeMPO)",
            "Включает прямую передачу декодированных видеопотоков на аппаратные оверлейные плоскости GPU (MPO) в ядре dxgkrnl.sys, минуя промежуточный рендеринг.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5265, 5265,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "EnableDecodeMPO", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dxg-enable-fbr-validation",
            "Валидация Front Buffer Rendering (EnableFbrValidation)",
            "Управляет проверкой корректности прямого рендеринга в передний буфер (FBR) в dxgkrnl.sys, предотвращая повреждение содержимого экрана при низколатентном выводе.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5266, 5266,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "EnableFbrValidation", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dxg-enable-offer-reclaim",
            "Освобождение памяти драйвером при простое (EnableOfferReclaimOnDriver)",
            "Разрешает видеодрайверу автоматически возвращать неиспользуемые видеобуферы (Offer/Reclaim) диспетчеру памяти dxgmms2 для минимизации расхода видеопамяти (VRAM).",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5268, 5268,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "EnableOfferReclaimOnDriver", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dxg-enable-panel-fitter",
            "Аппаратное масштабирование Panel Fitter (EnablePanelFitterSupport)",
            "Включает поддержку встроенного в видеокарту скалера дисплея Panel Fitter для быстрого масштабирования неродных игровых разрешений без задержек ввода.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5269, 5269,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "EnablePanelFitterSupport", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dxg-force-32bit-fences",
            "Принудительные 32-битные синхронизаторы GPU (Force32BitFences)",
            "Принудительно переводит очереди синхронизации (GPU Fences) в 32-битный формат для совместимости со специализированными графическими движками и снижения накладных расходов памяти.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5272, 5272,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "Force32BitFences", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dxg-force-dxgmms2",
            "Принудительный диспетчер памяти dxgmms2 (ForceEnableDxgMms2)",
            "Принудительно активирует современный диспетчер видеопамяти dxgmms2.sys, гарантируя поддержку виртуальной адресации GPU VA и современного планирования WDDM 2.x+.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5276, 5276,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "ForceEnableDxgMms2", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dxg-disable-secondary-iflip",
            "Отключение вторичного IFlip на доп. экранах (DisableSecondaryIFlipSupport)",
            "Отключает механизм независимого флипа (IFlip) для дополнительных мониторов, предотвращая микростаттеры и рассинхронизацию частоты кадров на основном дисплее.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5262, 5262,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableSecondaryIFlipSupport", "REG_DWORD", 1));

        block35.Add(CreateTweak(
            "system-dxg-disable-bad-driver-hw-protection",
            "Обход блокировки аппаратной защиты драйверов (DisableBadDriverCheckForHwProtection)",
            "Отключает эвристическую проверку нестабильных драйверов в dxgkrnl.sys, предотвращая автоматическое принудительное отключение аппаратных функций GPU.",
            "system", "GPU Scheduling", "low",
            "system/desc.md", 5253, 5253,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableBadDriverCheckForHwProtection", "REG_DWORD", 1));

        // 17-21: Network Adapter Offloads (network/desc.md)
        block35.Add(CreateTweak(
            "network-adapter-rsc-mode",
            "Режим агрегации сегментов RSC (RscMode)",
            "Задает режим аппаратного объединения входящих сегментов TCP (Receive Segment Coalescing) на адаптере: 1 — стандартный режим агрегации фрагментов для разгрузки CPU.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 315, 315,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "RscMode", "REG_SZ", "1"));

        block35.Add(CreateTweak(
            "network-adapter-encapsulated-vxlan",
            "Разгрузка инкапсулированных пакетов VXLAN (*EncapsulatedPacketTaskOffloadVxlan)",
            "Включает аппаратную разгрузку задач контрольных сумм и сегментации сетевых пакетов протокола VXLAN на сетевой карте.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 319, 319,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "*EncapsulatedPacketTaskOffloadVxlan", "REG_SZ", "1"));

        block35.Add(CreateTweak(
            "network-adapter-tcp-connection-offload-ipv6",
            "Разгрузка соединений TCP Chimney IPv6 (*TCPConnectionOffloadIPv6)",
            "Разрешает сетевому адаптеру аппаратно обрабатывать соединения протокола IPv6 (TCP Chimney Offload) непосредственно процессором сетевой карты.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 327, 327,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "*TCPConnectionOffloadIPv6", "REG_SZ", "1"));

        block35.Add(CreateTweak(
            "network-adapter-ipsec-offload-v2",
            "Аппаратная разгрузка IPsec Offload V2 (*IPsecOffloadV2)",
            "Включает разгрузку криптографических операций IPsec v2 (AH и ESP заголовки, значение 3) в специализированный сопроцессор сетевого адаптера.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 323, 323,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "*IPsecOffloadV2", "REG_SZ", "3"));

        block35.Add(CreateTweak(
            "network-adapter-sa-offload-capacity",
            "Разгрузка ассоциаций безопасности SA (SaOffloadCapacityEnabled)",
            "Активирует разгрузку базы ассоциаций безопасности (Security Associations, SA) для защищенных сетевых протоколов в память сетевого контроллера.",
            "network", "Network HW Offloads", "low",
            "network/desc.md", 333, 333,
            "HKLM", @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX", "SaOffloadCapacityEnabled", "REG_SZ", "1"));

        // 22-25: 7-Zip, Hung Screen & StartAllBack (misc/desc.md & system/desc.md)
        block35.Add(CreateTweak(
            "misc-7zip-write-zone-id-extract",
            "Отключение потока Zone.Identifier в 7-Zip (WriteZoneIdExtract)",
            "Отключает принудительную запись метки Zone.Identifier (Mark of the Web) в извлекаемые файлы, устраняя лишние блокировки SmartScreen при открытии распакованных утилит.",
            "misc", "7-Zip Options", "low",
            "misc/desc.md", 318, 318,
            "HKCU", @"Software\7-Zip\Options", "WriteZoneIdExtract", "REG_DWORD", 0));

        block35.Add(CreateTweak(
            "system-hung-screen-wait-to-kill-timeout",
            "Таймаут завершения консольных процессов (WaitToKillTimeout)",
            "Задает время (в миллисекундах, 2000 мс вместо 5000 мс), которое подсистема CSRSS ожидает завершения консольных обработчиков перед выводом диалога зависшей программы при завершении работы.",
            "system", "System Responsiveness", "low",
            "system/desc.md", 9355, 9355,
            "HKCU", @"Control Panel\Desktop", "WaitToKillTimeout", "REG_SZ", "2000"));

        block35.Add(CreateTweak(
            "misc-sab-disable-new-app-notification",
            "Отключение подсветки новых программ в меню Пуск (Start_NotifyNewApps)",
            "Отключает подсветку и всплывающие оповещения о недавно установленных приложениях в меню Пуск оболочки StartAllBack.",
            "misc", "StartAllBack Options", "low",
            "misc/desc.md", 78, 78,
            "HKCU", @"Software\StartIsBack", "Start_NotifyNewApps", "REG_DWORD", 0));

        block35.Add(CreateTweak(
            "misc-sab-hide-user-frame",
            "Скрытие рамки аватара пользователя в меню Пуск (HideUserFrame)",
            "Скрывает рамку вокруг изображения профиля пользователя в меню Пуск оболочки StartAllBack для минималистичного интерфейса.",
            "misc", "StartAllBack Options", "low",
            "misc/desc.md", 87, 87,
            "HKCU", @"Software\StartIsBack", "HideUserFrame", "REG_DWORD", 1));

        Console.WriteLine("Сформировано новых твиков для Блока 35: {0}", block35.Count);
        list.AddRange(block35);
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
        Console.WriteLine("Файл data/tweaks.json успешно обновлен до 850 твиков!");
    }
}
