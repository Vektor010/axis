using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Validator33
{
    class Program
    {
        class CandidateSpec
        {
            public string Id;
            public string NameRu;
            public string DescRu;
            public string Category;
            public string Group;
            public string SourceRel;
            public int LineFrom;
            public int LineTo;
            public string Hive;
            public string Path;
            public string Name;
            public string Type;
            public object Apply;
            public string RevertMode;
            public object RevertValue;
        }

        static void Main(string[] args)
        {
            string rootDir = @"C:\Users\User\Documents\win-tweaker";
            string tweaksJsonPath = Path.Combine(rootDir, @"data\tweaks.json");
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var currentTweaks = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJsonPath, Encoding.UTF8));

            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> existingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var t in currentTweaks)
            {
                existingIds.Add(t["id"] as string);
                var regList = t["registry"] as System.Collections.ArrayList;
                if (regList != null)
                {
                    foreach (Dictionary<string, object> r in regList)
                    {
                        string p = r.ContainsKey("path") ? r["path"] as string : "";
                        string n = r.ContainsKey("name") ? r["name"] as string : "";
                        existingPaths.Add(p + "\\" + n);
                    }
                }
            }

            var candidates = GetCandidates();
            Console.WriteLine("Кандидатов на проверку: " + candidates.Count);

            bool allPass = true;
            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];
                Console.WriteLine(string.Format("[{0}] Проверка: {1} ({2})", i + 1, c.Id, c.Name));

                // 1. Проверка уникальности ID
                if (existingIds.Contains(c.Id))
                {
                    Console.WriteLine("  [FAIL] Дубликат ID: " + c.Id);
                    allPass = false;
                }

                // 2. Проверка коллизий пути
                string fullKey = c.Path + "\\" + c.Name;
                if (existingPaths.Contains(fullKey))
                {
                    Console.WriteLine("  [FAIL] Коллизия ключа реестра: " + fullKey);
                    allPass = false;
                }

                // 3. Проверка типа Apply
                if (c.Type == "REG_DWORD" && !(c.Apply is int))
                {
                    Console.WriteLine("  [FAIL] REG_DWORD Apply должен быть int: " + c.Apply);
                    allPass = false;
                }
                if (c.Type == "REG_SZ" && !(c.Apply is string))
                {
                    Console.WriteLine("  [FAIL] REG_SZ Apply должен быть string: " + c.Apply);
                    allPass = false;
                }

                // 4. Проверка Revert
                if (c.RevertMode == "set")
                {
                    if (c.RevertValue == null)
                    {
                        Console.WriteLine("  [FAIL] mode=set требует revert.value!");
                        allPass = false;
                    }
                    if (c.Apply.Equals(c.RevertValue))
                    {
                        Console.WriteLine("  [FAIL] apply == revert.value: " + c.Apply);
                        allPass = false;
                    }
                }

                // 5. Проверка существования файла источника и диапазона строк
                string fullSourcePath = Path.Combine(rootDir, @"data\source\win-config", c.SourceRel.Replace('/', '\\'));
                if (!File.Exists(fullSourcePath))
                {
                    Console.WriteLine("  [FAIL] Источник не найден: " + fullSourcePath);
                    allPass = false;
                }
                else
                {
                    var lines = File.ReadAllLines(fullSourcePath);
                    if (c.LineFrom < 1 || c.LineTo > lines.Length || c.LineFrom > c.LineTo)
                    {
                        Console.WriteLine(string.Format("  [FAIL] Неверный диапазон строк: {0}..{1} (всего {2})", c.LineFrom, c.LineTo, lines.Length));
                        allPass = false;
                    }
                    else
                    {
                        bool foundName = false;
                        for (int lineIdx = c.LineFrom - 1; lineIdx < c.LineTo; lineIdx++)
                        {
                            if (lines[lineIdx].Contains(c.Name))
                            {
                                foundName = true;
                                break;
                            }
                        }
                        if (!foundName)
                        {
                            Console.WriteLine(string.Format("  [FAIL] Имя параметра '{0}' не найдено в строках {1}..{2} файла {3}", c.Name, c.LineFrom, c.LineTo, c.SourceRel));
                            allPass = false;
                        }
                    }
                }
            }

            if (allPass)
            {
                Console.WriteLine("\n[SUCCESS] ВСЕ 25 КАНДИДАТОВ УСПЕШНО ПРОШЛИ ПРЕДВАРИТЕЛЬНУЮ ВАЛИДАЦИЮ БЛОКА 33!");
            }
            else
            {
                Console.WriteLine("\n[ERROR] ОБНАРУЖЕНЫ ОШИБКИ В КАНДИДАТАХ БЛОКА 33!");
            }
        }

        static List<CandidateSpec> GetCandidates()
        {
            var list = new List<CandidateSpec>();

            // 1. SMB1
            list.Add(new CandidateSpec {
                Id = "network-smb-disable-smb1",
                NameRu = "Отключить устаревший протокол SMBv1 (SMB1)",
                DescRu = "Отключает небезопасный и уязвимый протокол общего доступа к файлам SMB 1.0 на сервере LanmanServer, снижая поверхность атаки и сетевые накладные расходы.",
                Category = "network",
                Group = "SMB",
                SourceRel = "network/desc.md",
                LineFrom = 405,
                LineTo = 420,
                Hive = "HKLM",
                Path = @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                Name = "SMB1",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "set",
                RevertValue = 1
            });

            // 2. AutoShareWks
            list.Add(new CandidateSpec {
                Id = "network-smb-disable-autoshare-wks",
                NameRu = "Отключить скрытые административные ресурсы рабочей станции (AutoShareWks)",
                DescRu = "Блокирует автоматическое создание скрытых административных общих ресурсов (C$, ADMIN$) при загрузке системы, предотвращая несанкционированное сканирование сети.",
                Category = "network",
                Group = "SMB",
                SourceRel = "network/desc.md",
                LineFrom = 505,
                LineTo = 515,
                Hive = "HKLM",
                Path = @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                Name = "AutoShareWks",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 3. AutoShareServer
            list.Add(new CandidateSpec {
                Id = "network-smb-disable-autoshare-server",
                NameRu = "Отключить скрытые административные ресурсы сервера (AutoShareServer)",
                DescRu = "Запрещает автоматическое развертывание скрытых шар администратора на уровне службы LanmanServer.",
                Category = "network",
                Group = "SMB",
                SourceRel = "network/desc.md",
                LineFrom = 505,
                LineTo = 515,
                Hive = "HKLM",
                Path = @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                Name = "AutoShareServer",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 4. RequireSecuritySignature Workstation
            list.Add(new CandidateSpec {
                Id = "network-smb-disable-workstation-security-signature",
                NameRu = "Отключить обязательную подпись пакетов SMB клиента (RequireSecuritySignature)",
                DescRu = "Отключает обязательную цифровую подпись SMB-пакетов на клиенте LanmanWorkstation, устраняя накладные расходы процессора на вычисление хэшей в локальной доверенной сети.",
                Category = "network",
                Group = "SMB",
                SourceRel = "network/desc.md",
                LineFrom = 460,
                LineTo = 480,
                Hive = "HKLM",
                Path = @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters",
                Name = "RequireSecuritySignature",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "set",
                RevertValue = 1
            });

            // 5. RequireSecuritySignature Server
            list.Add(new CandidateSpec {
                Id = "network-smb-disable-server-security-signature",
                NameRu = "Отключить обязательную подпись пакетов SMB сервера (RequireSecuritySignature)",
                DescRu = "Отключает принудительную валидацию цифровых подписей сетевых пакетов на сервере LanmanServer для максимальной скорости передачи данных.",
                Category = "network",
                Group = "SMB",
                SourceRel = "network/desc.md",
                LineFrom = 460,
                LineTo = 480,
                Hive = "HKLM",
                Path = @"System\CurrentControlSet\Services\LanmanServer\Parameters",
                Name = "RequireSecuritySignature",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "set",
                RevertValue = 1
            });

            // 6. *IPChecksumOffloadIPv4
            list.Add(new CandidateSpec {
                Id = "network-offload-ip-checksum-ipv4",
                NameRu = "Аппаратный расчет контрольных сумм IP IPv4 (*IPChecksumOffloadIPv4)",
                DescRu = "Включает двустороннюю (Tx/Rx) аппаратную разгрузку контрольных сумм IPv4 на сетевом адаптере, освобождая ядра ЦП от побайтовой проверки заголовков.",
                Category = "network",
                Group = "Сетевой адаптер",
                SourceRel = "network/desc.md",
                LineFrom = 296,
                LineTo = 330,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*IPChecksumOffloadIPv4",
                Type = "REG_SZ",
                Apply = "3",
                RevertMode = "set",
                RevertValue = "0"
            });

            // 7. *TCPChecksumOffloadIPv6
            list.Add(new CandidateSpec {
                Id = "network-offload-tcp-checksum-ipv6",
                NameRu = "Аппаратный расчет контрольных сумм TCP IPv6 (*TCPChecksumOffloadIPv6)",
                DescRu = "Включает двунаправленную аппаратную проверку контрольных сумм TCP для пакетов IPv6 в сетевом чипе.",
                Category = "network",
                Group = "Сетевой адаптер",
                SourceRel = "network/desc.md",
                LineFrom = 296,
                LineTo = 330,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*TCPChecksumOffloadIPv6",
                Type = "REG_SZ",
                Apply = "3",
                RevertMode = "set",
                RevertValue = "0"
            });

            // 8. *UDPChecksumOffloadIPv6
            list.Add(new CandidateSpec {
                Id = "network-offload-udp-checksum-ipv6",
                NameRu = "Аппаратный расчет контрольных сумм UDP IPv6 (*UDPChecksumOffloadIPv6)",
                DescRu = "Включает аппаратный оффлоад расчета контрольных сумм UDP IPv6 на сетевом адаптере для игровых и медиа-потоков.",
                Category = "network",
                Group = "Сетевой адаптер",
                SourceRel = "network/desc.md",
                LineFrom = 296,
                LineTo = 330,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*UDPChecksumOffloadIPv6",
                Type = "REG_SZ",
                Apply = "3",
                RevertMode = "set",
                RevertValue = "0"
            });

            // 9. *TCPUDPChecksumOffloadIPv4
            list.Add(new CandidateSpec {
                Id = "network-offload-tcp-udp-checksum-ipv4",
                NameRu = "Комбинированный расчет сумм TCP/UDP IPv4 (*TCPUDPChecksumOffloadIPv4)",
                DescRu = "Активирует единый аппаратный конвейер вычисления контрольных сумм TCP и UDP для трафика IPv4.",
                Category = "network",
                Group = "Сетевой адаптер",
                SourceRel = "network/desc.md",
                LineFrom = 296,
                LineTo = 330,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*TCPUDPChecksumOffloadIPv4",
                Type = "REG_SZ",
                Apply = "3",
                RevertMode = "set",
                RevertValue = "0"
            });

            // 10. *TCPUDPChecksumOffloadIPv6
            list.Add(new CandidateSpec {
                Id = "network-offload-tcp-udp-checksum-ipv6",
                NameRu = "Комбинированный расчет сумм TCP/UDP IPv6 (*TCPUDPChecksumOffloadIPv6)",
                DescRu = "Активирует аппаратный конвейер проверки контрольных сумм TCP/UDP для кадров IPv6 на сетевой карте.",
                Category = "network",
                Group = "Сетевой адаптер",
                SourceRel = "network/desc.md",
                LineFrom = 296,
                LineTo = 330,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*TCPUDPChecksumOffloadIPv6",
                Type = "REG_SZ",
                Apply = "3",
                RevertMode = "set",
                RevertValue = "0"
            });

            // 11. *LsoV1IPv4
            list.Add(new CandidateSpec {
                Id = "network-offload-lso-v1-ipv4",
                NameRu = "Большая сегментация TCP LSOv1 IPv4 (*LsoV1IPv4)",
                DescRu = "Включает Large Send Offload v1 для стека TCP/IPv4, передавая нарезку крупных TCP-пакетов аппаратному контроллеру сетевой карты.",
                Category = "network",
                Group = "Сетевой адаптер",
                SourceRel = "network/desc.md",
                LineFrom = 296,
                LineTo = 330,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*LsoV1IPv4",
                Type = "REG_SZ",
                Apply = "1",
                RevertMode = "set",
                RevertValue = "0"
            });

            // 12. EnablePrimitiveReordering
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-primitive-reordering",
                NameRu = "Оптимизация порядка отрисовки примитивов DWM (EnablePrimitiveReordering)",
                DescRu = "Включает внутреннюю переупорядоченность примитивов рендеринга для минимизации переключений состояний пайплайна видеокарты.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2760,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnablePrimitiveReordering",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 13. EnableCommonSuperSets
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-common-supersets",
                NameRu = "Объединение общих супермножеств геометрии DWM (EnableCommonSuperSets)",
                DescRu = "Активирует объединение пересекающихся прямоугольников и контуров геометрии окон для ускорения вычисления видимых регионов.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2760,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnableCommonSuperSets",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 14. EnableCpuClipping
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-cpu-clipping",
                NameRu = "Аппаратное отсечение невидимых областей окон (EnableCpuClipping)",
                DescRu = "Включает предварительное отсечение перекрытых регионов окон на ЦП перед отправкой команд отрисовки в GPU (Clipping).",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2760,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnableCpuClipping",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 15. EnableDDisplayScanoutCaching
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-ddisplay-scanout-caching",
                NameRu = "Кэширование развертки дисплея DWM (EnableDDisplayScanoutCaching)",
                DescRu = "Активирует кэширование параметров вывода сканирования дисплея DirectDisplay, снижая число обращений к видеодрайверу.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2760,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnableDDisplayScanoutCaching",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 16. EnableFrontBufferRenderChecks
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-front-buffer-render-checks",
                NameRu = "Контроль рендеринга переднего буфера DWM (EnableFrontBufferRenderChecks)",
                DescRu = "Включает защиту от одновременного рендеринга в отображаемый буфер Direct3D, предотвращая тиринг и артефакты развертки.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2760,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnableFrontBufferRenderChecks",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 17. EnableMegaRects
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-mega-rects",
                NameRu = "Объединение областей перерисовки в мега-прямоугольники (EnableMegaRects)",
                DescRu = "Объединяет множество мелких грязных регионов окна (Dirty Regions) в единые мега-прямоугольники для пакетной отрисовки.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2760,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnableMegaRects",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 18. OptimizeForDirtyExpressions
            list.Add(new CandidateSpec {
                Id = "system-dwm-optimize-dirty-expressions",
                NameRu = "Оптимизация выражений анимации DWM (OptimizeForDirtyExpressions)",
                DescRu = "Оптимизирует вычисление интерполяционных выражений анимаций Composition, рассчитывая значения только для изменившихся узлов дерева.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2760,
                LineTo = 2775,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "OptimizeForDirtyExpressions",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 19. EnableDrawToBackbuffer
            list.Add(new CandidateSpec {
                Id = "system-dwm-scene-enable-draw-to-backbuffer",
                NameRu = "Прямой рендеринг сцены в задний буфер (Scene EnableDrawToBackbuffer)",
                DescRu = "Форсирует отрисовку компонентов сцены DWM непосредственно в задний буфер без создания промежуточных копий в памяти.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2875,
                LineTo = 2900,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                Name = "EnableDrawToBackbuffer",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 20. EnableBloom
            list.Add(new CandidateSpec {
                Id = "system-dwm-scene-enable-bloom",
                NameRu = "Отключить свечение элементов сцены DWM (Scene EnableBloom)",
                DescRu = "Отключает шейдерный эффект свечения (Bloom) в подсистеме композиции сцены рабочего стола, экономя такты шейдеров GPU.",
                Category = "system",
                Group = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2875,
                LineTo = 2900,
                Hive = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                Name = "EnableBloom",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 21. debuglevel
            list.Add(new CandidateSpec {
                Id = "peripheral-usb-disable-debug-level",
                NameRu = "Отключить отладочный уровень драйвера USB (debuglevel)",
                DescRu = "Устанавливает уровень отладки стека usbport.sys в 0, полностью исключая генерацию детальных диагностических сообщений ядра.",
                Category = "peripheral",
                Group = "USB",
                SourceRel = "peripheral/desc.md",
                LineFrom = 743,
                LineTo = 755,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Services\usb",
                Name = "debuglevel",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 22. debuglogmask
            list.Add(new CandidateSpec {
                Id = "peripheral-usb-disable-debug-log-mask",
                NameRu = "Отключить битовую маску логирования USB (debuglogmask)",
                DescRu = "Сбрасывает маску трассировки событий USB-контроллера (0 вместо 0xFFFFFFFE), предотвращая создание записей в системных логах.",
                Category = "peripheral",
                Group = "USB",
                SourceRel = "peripheral/desc.md",
                LineFrom = 743,
                LineTo = 755,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Services\usb",
                Name = "debuglogmask",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 23. debugcatc
            list.Add(new CandidateSpec {
                Id = "peripheral-usb-disable-catc-trace",
                NameRu = "Отключить аппаратную трассировку CATC USB (debugcatc)",
                DescRu = "Отключает внутреннюю интеграцию со специальными анализаторами протокола CATC в драйвере USB.",
                Category = "peripheral",
                Group = "USB",
                SourceRel = "peripheral/desc.md",
                LineFrom = 743,
                LineTo = 755,
                Hive = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Services\usb",
                Name = "debugcatc",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 24. DisableSpatialAudioPerEndpoint
            list.Add(new CandidateSpec {
                Id = "peripheral-audio-disable-spatial-endpoint",
                NameRu = "Отключить пространственный звук на конечных устройствах (DisableSpatialAudioPerEndpoint)",
                DescRu = "Устанавливает флаг PKEY_Endpoint_SpatialNotAllowed в подсистеме Windows Audio, полностью отключая обработку пространственного звука для конечных аудиовыходов.",
                Category = "peripheral",
                Group = "Аудио",
                SourceRel = "peripheral/desc.md",
                LineFrom = 450,
                LineTo = 470,
                Hive = "HKLM",
                Path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                Name = "DisableSpatialAudioPerEndpoint",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 25. DisableSpatialAudioVssFeature
            list.Add(new CandidateSpec {
                Id = "peripheral-audio-disable-spatial-vss",
                NameRu = "Отключить виртуализацию объемного звука VSS (DisableSpatialAudioVssFeature)",
                DescRu = "Отключает внутренний компонент виртуального объемного звука Virtual Surround Sound (VSS) в Windows Audio Service.",
                Category = "peripheral",
                Group = "Аудио",
                SourceRel = "peripheral/desc.md",
                LineFrom = 450,
                LineTo = 470,
                Hive = "HKLM",
                Path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                Name = "DisableSpatialAudioVssFeature",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            return list;
        }
    }
}
