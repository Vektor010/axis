using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock33
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootDir = @"C:\Users\User\Documents\win-tweaker";
            string tweaksJsonPath = Path.Combine(rootDir, @"data\tweaks.json");
            string backupPath = Path.Combine(rootDir, @"data\tweaks.json.bak33");

            File.Copy(tweaksJsonPath, backupPath, true);
            Console.WriteLine("Создан бэкап: " + backupPath);

            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var list = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJsonPath, Encoding.UTF8));
            Console.WriteLine("Исходное число твиков: " + list.Count);

            var newTweaks = CreateBlock33Tweaks();
            Console.WriteLine("Новых твиков для добавления: " + newTweaks.Count);

            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in list)
            {
                ids.Add(t["id"] as string);
            }

            foreach (var nt in newTweaks)
            {
                string id = nt["id"] as string;
                if (ids.Contains(id))
                {
                    throw new Exception("Дубликат ID обнаружен: " + id);
                }
                ids.Add(id);
                list.Add(nt);
            }

            Console.WriteLine("Итоговое число твиков: " + list.Count);

            string jsonOutput = ser.Serialize(list);

            StringBuilder sb = new StringBuilder();
            int indent = 0;
            bool inQuote = false;
            for (int i = 0; i < jsonOutput.Length; i++)
            {
                char ch = jsonOutput[i];
                if (ch == '"' && (i == 0 || jsonOutput[i - 1] != '\\'))
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
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        sb.Append("\r\n");
                        indent--;
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent * 2));
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

            File.WriteAllText(tweaksJsonPath, sb.ToString(), new UTF8Encoding(false));
            Console.WriteLine("Успешно записано в tweaks.json (UTF-8 без BOM)");
        }

        static List<Dictionary<string, object>> CreateBlock33Tweaks()
        {
            var res = new List<Dictionary<string, object>>();

            // 1
            res.Add(CreateTweak(
                "network-smb-disable-smb1",
                "Отключить устаревший протокол SMBv1 (SMB1)",
                "Отключает небезопасный и уязвимый протокол общего доступа к файлам SMB 1.0 на сервере LanmanServer, снижая поверхность атаки и сетевые накладные расходы.",
                "network",
                "SMB",
                new string[] { "network", "smb", "security", "protocol", "noverse" },
                new string[] { "security", "recommended" },
                "network/desc.md",
                405,
                420,
                "HKLM",
                @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                "SMB1",
                "REG_DWORD",
                0,
                "set",
                1
            ));

            // 2
            res.Add(CreateTweak(
                "network-smb-disable-autoshare-wks",
                "Отключить скрытые административные ресурсы рабочей станции (AutoShareWks)",
                "Блокирует автоматическое создание скрытых административных общих ресурсов (C$, ADMIN$) при загрузке системы, предотвращая несанкционированное сканирование сети.",
                "network",
                "SMB",
                new string[] { "network", "smb", "shares", "privacy", "noverse" },
                new string[] { "security" },
                "network/desc.md",
                505,
                515,
                "HKLM",
                @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                "AutoShareWks",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 3
            res.Add(CreateTweak(
                "network-smb-disable-autoshare-server",
                "Отключить скрытые административные ресурсы сервера (AutoShareServer)",
                "Запрещает автоматическое развертывание скрытых шар администратора на уровне службы LanmanServer.",
                "network",
                "SMB",
                new string[] { "network", "smb", "server", "shares", "noverse" },
                new string[] { "security" },
                "network/desc.md",
                505,
                515,
                "HKLM",
                @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                "AutoShareServer",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 4
            res.Add(CreateTweak(
                "network-smb-disable-workstation-security-signature",
                "Отключить обязательную подпись пакетов SMB клиента (RequireSecuritySignature)",
                "Отключает обязательную цифровую подпись SMB-пакетов на клиенте LanmanWorkstation, устраняя накладные расходы процессора на вычисление хэшей в локальной доверенной сети.",
                "network",
                "SMB",
                new string[] { "network", "smb", "signing", "throughput", "noverse" },
                new string[] { "latency" },
                "network/desc.md",
                460,
                480,
                "HKLM",
                @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters",
                "RequireSecuritySignature",
                "REG_DWORD",
                0,
                "set",
                1
            ));

            // 5
            res.Add(CreateTweak(
                "network-smb-disable-server-security-signature",
                "Отключить обязательную подпись пакетов SMB сервера (RequireSecuritySignature)",
                "Отключает принудительную валидацию цифровых подписей сетевых пакетов на сервере LanmanServer для максимальной скорости передачи данных.",
                "network",
                "SMB",
                new string[] { "network", "smb", "server", "signing", "noverse" },
                new string[] { "latency" },
                "network/desc.md",
                460,
                480,
                "HKLM",
                @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                "RequireSecuritySignature",
                "REG_DWORD",
                0,
                "set",
                1
            ));

            // 6
            res.Add(CreateTweak(
                "network-offload-ip-checksum-ipv4",
                "Аппаратный расчет контрольных сумм IP IPv4 (*IPChecksumOffloadIPv4)",
                "Включает двустороннюю (Tx/Rx) аппаратную разгрузку контрольных сумм IPv4 на сетевом адаптере, освобождая ядра ЦП от побайтовой проверки заголовков.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "offload", "checksum", "ipv4", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                296,
                330,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*IPChecksumOffloadIPv4",
                "REG_SZ",
                "3",
                "set",
                "0"
            ));

            // 7
            res.Add(CreateTweak(
                "network-offload-tcp-checksum-ipv6",
                "Аппаратный расчет контрольных сумм TCP IPv6 (*TCPChecksumOffloadIPv6)",
                "Включает двунаправленную аппаратную проверку контрольных сумм TCP для пакетов IPv6 в сетевом чипе.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "offload", "tcp", "ipv6", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                296,
                330,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*TCPChecksumOffloadIPv6",
                "REG_SZ",
                "3",
                "set",
                "0"
            ));

            // 8
            res.Add(CreateTweak(
                "network-offload-udp-checksum-ipv6",
                "Аппаратный расчет контрольных сумм UDP IPv6 (*UDPChecksumOffloadIPv6)",
                "Включает аппаратный оффлоад расчета контрольных сумм UDP IPv6 на сетевом адаптере для игровых и медиа-потоков.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "offload", "udp", "ipv6", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                296,
                330,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*UDPChecksumOffloadIPv6",
                "REG_SZ",
                "3",
                "set",
                "0"
            ));

            // 9
            res.Add(CreateTweak(
                "network-offload-tcp-udp-checksum-ipv4",
                "Комбинированный расчет сумм TCP/UDP IPv4 (*TCPUDPChecksumOffloadIPv4)",
                "Активирует единый аппаратный конвейер вычисления контрольных сумм TCP и UDP для трафика IPv4.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "offload", "tcp-udp", "ipv4", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                296,
                330,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*TCPUDPChecksumOffloadIPv4",
                "REG_SZ",
                "3",
                "set",
                "0"
            ));

            // 10
            res.Add(CreateTweak(
                "network-offload-tcp-udp-checksum-ipv6",
                "Комбинированный расчет сумм TCP/UDP IPv6 (*TCPUDPChecksumOffloadIPv6)",
                "Активирует аппаратный конвейер проверки контрольных сумм TCP/UDP для кадров IPv6 на сетевой карте.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "offload", "tcp-udp", "ipv6", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                296,
                330,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*TCPUDPChecksumOffloadIPv6",
                "REG_SZ",
                "3",
                "set",
                "0"
            ));

            // 11
            res.Add(CreateTweak(
                "network-offload-lso-v1-ipv4",
                "Большая сегментация TCP LSOv1 IPv4 (*LsoV1IPv4)",
                "Включает Large Send Offload v1 для стека TCP/IPv4, передавая нарезку крупных TCP-пакетов аппаратному контроллеру сетевой карты.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "lso", "offload", "ipv4", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                296,
                330,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*LsoV1IPv4",
                "REG_SZ",
                "1",
                "set",
                "0"
            ));

            // 12
            res.Add(CreateTweak(
                "system-dwm-enable-primitive-reordering",
                "Оптимизация порядка отрисовки примитивов DWM (EnablePrimitiveReordering)",
                "Включает внутреннюю переупорядоченность примитивов рендеринга для минимизации переключений состояний пайплайна видеокарты.",
                "system",
                "DWM",
                new string[] { "dwm", "primitives", "rendering", "gpu", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2760,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnablePrimitiveReordering",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 13
            res.Add(CreateTweak(
                "system-dwm-enable-common-supersets",
                "Объединение общих супермножеств геометрии DWM (EnableCommonSuperSets)",
                "Активирует объединение пересекающихся прямоугольников и контуров геометрии окон для ускорения вычисления видимых регионов.",
                "system",
                "DWM",
                new string[] { "dwm", "supersets", "geometry", "ui", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2760,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnableCommonSuperSets",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 14
            res.Add(CreateTweak(
                "system-dwm-enable-cpu-clipping",
                "Аппаратное отсечение невидимых областей окон (EnableCpuClipping)",
                "Включает предварительное отсечение перекрытых регионов окон на ЦП перед отправкой команд отрисовки в GPU (Clipping).",
                "system",
                "DWM",
                new string[] { "dwm", "clipping", "cpu", "occlusion", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2760,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnableCpuClipping",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 15
            res.Add(CreateTweak(
                "system-dwm-enable-ddisplay-scanout-caching",
                "Кэширование развертки дисплея DWM (EnableDDisplayScanoutCaching)",
                "Активирует кэширование параметров вывода сканирования дисплея DirectDisplay, снижая число обращений к видеодрайверу.",
                "system",
                "DWM",
                new string[] { "dwm", "scanout", "caching", "display", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2760,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnableDDisplayScanoutCaching",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 16
            res.Add(CreateTweak(
                "system-dwm-enable-front-buffer-render-checks",
                "Контроль рендеринга переднего буфера DWM (EnableFrontBufferRenderChecks)",
                "Включает защиту от одновременного рендеринга в отображаемый буфер Direct3D, предотвращая тиринг и артефакты развертки.",
                "system",
                "DWM",
                new string[] { "dwm", "front-buffer", "render", "sync", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2760,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnableFrontBufferRenderChecks",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 17
            res.Add(CreateTweak(
                "system-dwm-enable-mega-rects",
                "Объединение областей перерисовки в мега-прямоугольники (EnableMegaRects)",
                "Объединяет множество мелких грязных регионов окна (Dirty Regions) в единые мега-прямоугольники для пакетной отрисовки.",
                "system",
                "DWM",
                new string[] { "dwm", "megarects", "batch", "rendering", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2760,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnableMegaRects",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 18
            res.Add(CreateTweak(
                "system-dwm-optimize-dirty-expressions",
                "Оптимизация выражений анимации DWM (OptimizeForDirtyExpressions)",
                "Оптимизирует вычисление интерполяционных выражений анимаций Composition, рассчитывая значения только для изменившихся узлов дерева.",
                "system",
                "DWM",
                new string[] { "dwm", "expressions", "animations", "ui", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2760,
                2775,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "OptimizeForDirtyExpressions",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 19
            res.Add(CreateTweak(
                "system-dwm-scene-enable-draw-to-backbuffer",
                "Прямой рендеринг сцены в задний буфер (Scene EnableDrawToBackbuffer)",
                "Форсирует отрисовку компонентов сцены DWM непосредственно в задний буфер без создания промежуточных копий в памяти.",
                "system",
                "DWM",
                new string[] { "dwm", "backbuffer", "scene", "rendering", "noverse" },
                new string[] { "gaming" },
                "system/desc.md",
                2875,
                2900,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                "EnableDrawToBackbuffer",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 20
            res.Add(CreateTweak(
                "system-dwm-scene-enable-bloom",
                "Отключить свечение элементов сцены DWM (Scene EnableBloom)",
                "Отключает шейдерный эффект свечения (Bloom) в подсистеме композиции сцены рабочего стола, экономя такты шейдеров GPU.",
                "system",
                "DWM",
                new string[] { "dwm", "bloom", "shaders", "gpu", "noverse" },
                new string[] { "gaming" },
                "system/desc.md",
                2875,
                2900,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                "EnableBloom",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 21
            res.Add(CreateTweak(
                "peripheral-usb-disable-debug-level",
                "Отключить отладочный уровень драйвера USB (debuglevel)",
                "Устанавливает уровень отладки стека usbport.sys в 0, полностью исключая генерацию детальных диагностических сообщений ядра.",
                "peripheral",
                "USB",
                new string[] { "usb", "debug", "logging", "driver", "noverse" },
                new string[] { "latency" },
                "peripheral/desc.md",
                743,
                755,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Services\usb",
                "debuglevel",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 22
            res.Add(CreateTweak(
                "peripheral-usb-disable-debug-log-mask",
                "Отключить битовую маску логирования USB (debuglogmask)",
                "Сбрасывает маску трассировки событий USB-контроллера (0 вместо 0xFFFFFFFE), предотвращая создание записей в системных логах.",
                "peripheral",
                "USB",
                new string[] { "usb", "mask", "logging", "driver", "noverse" },
                new string[] { "latency" },
                "peripheral/desc.md",
                743,
                755,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Services\usb",
                "debuglogmask",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 23
            res.Add(CreateTweak(
                "peripheral-usb-disable-catc-trace",
                "Отключить аппаратную трассировку CATC USB (debugcatc)",
                "Отключает внутреннюю интеграцию со специальными анализаторами протокола CATC в драйвере USB.",
                "peripheral",
                "USB",
                new string[] { "usb", "catc", "trace", "hardware", "noverse" },
                new string[] { "latency" },
                "peripheral/desc.md",
                743,
                755,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Services\usb",
                "debugcatc",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 24
            res.Add(CreateTweak(
                "peripheral-audio-disable-spatial-endpoint",
                "Отключить пространственный звук на конечных устройствах (DisableSpatialAudioPerEndpoint)",
                "Устанавливает флаг PKEY_Endpoint_SpatialNotAllowed в подсистеме Windows Audio, полностью отключая обработку пространственного звука для конечных аудиовыходов.",
                "peripheral",
                "Аудио",
                new string[] { "audio", "spatial", "endpoint", "latency", "noverse" },
                new string[] { "latency", "gaming" },
                "peripheral/desc.md",
                450,
                470,
                "HKLM",
                @"Software\Microsoft\Windows\CurrentVersion\Audio",
                "DisableSpatialAudioPerEndpoint",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 25
            res.Add(CreateTweak(
                "peripheral-audio-disable-spatial-vss",
                "Отключить виртуализацию объемного звука VSS (DisableSpatialAudioVssFeature)",
                "Отключает внутренний компонент виртуального объемного звука Virtual Surround Sound (VSS) в Windows Audio Service.",
                "peripheral",
                "Аудио",
                new string[] { "audio", "vss", "virtualization", "surround", "noverse" },
                new string[] { "latency", "gaming" },
                "peripheral/desc.md",
                450,
                470,
                "HKLM",
                @"Software\Microsoft\Windows\CurrentVersion\Audio",
                "DisableSpatialAudioVssFeature",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            return res;
        }

        static Dictionary<string, object> CreateTweak(
            string id,
            string nameRu,
            string descRu,
            string category,
            string group,
            string[] tags,
            string[] presets,
            string sourceFile,
            int lineFrom,
            int lineTo,
            string hive,
            string path,
            string name,
            string type,
            object applyVal,
            string revertMode,
            object revertVal)
        {
            var tweak = new Dictionary<string, object>();
            tweak["id"] = id;
            tweak["name_ru"] = nameRu;
            tweak["description_ru"] = descRu;
            tweak["category"] = category;
            tweak["group"] = group;
            tweak["type"] = "toggle";
            tweak["risk"] = "low";
            tweak["tags"] = new List<string>(tags);
            tweak["presets"] = new List<string>(presets);
            tweak["min_build"] = null;
            tweak["needs_review"] = true;

            var source = new Dictionary<string, object>();
            source["file"] = sourceFile;
            source["line_from"] = lineFrom;
            source["line_to"] = lineTo;
            tweak["source"] = source;

            var regList = new List<Dictionary<string, object>>();
            var regItem = new Dictionary<string, object>();
            regItem["hive"] = hive;
            regItem["path"] = path;
            regItem["name"] = name;
            regItem["type"] = type;
            regItem["apply"] = applyVal;

            var revert = new Dictionary<string, object>();
            revert["mode"] = revertMode;
            if (revertMode == "set")
            {
                revert["value"] = revertVal;
            }
            regItem["revert"] = revert;

            regList.Add(regItem);
            tweak["registry"] = regList;

            return tweak;
        }
    }
}
