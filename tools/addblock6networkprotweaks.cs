using System;
using System.Collections.Generic;
using System.IO;
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
            string raw = File.ReadAllText(jsonPath);
            var serializer = new JavaScriptSerializer();
            var tweaks = serializer.Deserialize<List<Dictionary<string, object>>>(raw);

            var existingIds = new HashSet<string>();
            foreach (var t in tweaks)
            {
                if (t.ContainsKey("id") && t["id"] != null)
                {
                    existingIds.Add(t["id"].ToString());
                }
            }

            string nicClassPath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX";

            var defs = new List<TweakDef>
            {
                new TweakDef {
                    id = "network-tcp-task-offload",
                    name = "Аппаратная разгрузка сетевых задач (DisableTaskOffload)",
                    desc = "Активирует аппаратную разгрузку сетевых вычислений на контроллер сетевой карты (Task Offload), снижая нагрузку на процессор.",
                    group = "Аппаратная разгрузка NIC", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"System\CurrentControlSet\Services\TCPIP\Parameters", nameVal = "DisableTaskOffload", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "network/desc.md", from = 288, to = 292, needsReview = true
                },
                new TweakDef {
                    id = "network-ipsec-offload",
                    name = "Аппаратная разгрузка IPsec (EnabledOffload)",
                    desc = "Включает аппаратную обработку криптографических вычислений IPsec сетевым адаптером.",
                    group = "Аппаратная разгрузка NIC", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"System\CurrentControlSet\Services\Ipsec", nameVal = "EnabledOffload", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "network/desc.md", from = 291, to = 294, needsReview = true
                },
                new TweakDef {
                    id = "network-tcp-checksum-offload",
                    name = "Разгрузка контрольных сумм TCP (TCPChecksumOffloadIPv4)",
                    desc = "Переносит проверку и подсчет контрольных сумм пакетов TCP IPv4 на аппаратный чип сетевой карты.",
                    group = "Аппаратная разгрузка NIC", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*TCPChecksumOffloadIPv4", type = "REG_SZ",
                    apply = "3", revMode = "set", revVal = "0", srcFile = "network/desc.md", from = 296, to = 305, needsReview = true
                },
                new TweakDef {
                    id = "network-udp-checksum-offload",
                    name = "Разгрузка контрольных сумм UDP (UDPChecksumOffloadIPv4)",
                    desc = "Переносит проверку контрольных сумм UDP-пакетов (голосовая связь, онлайн-игры) на аппаратный сетевой процессор.",
                    group = "Аппаратная разгрузка NIC", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*UDPChecksumOffloadIPv4", type = "REG_SZ",
                    apply = "3", revMode = "set", revVal = "0", srcFile = "network/desc.md", from = 297, to = 305, needsReview = true
                },
                new TweakDef {
                    id = "network-lso-v2",
                    name = "Аппаратная сегментация пакетов (LsoV2IPv4)",
                    desc = "Включает Large Send Offload v2 (LSOv2) для сегментации исходящих TCP-пакетов сетевой картой без участия CPU.",
                    group = "Аппаратная разгрузка NIC", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*LsoV2IPv4", type = "REG_SZ",
                    apply = "1", revMode = "set", revVal = "0", srcFile = "network/desc.md", from = 304, to = 308, needsReview = true
                },
                new TweakDef {
                    id = "network-rsc-ipv4",
                    name = "Коалесценция входящих сегментов (RscIPv4)",
                    desc = "Включает Receive Segment Coalescing (RSC) для объединения фрагментов входящих пакетов на уровне контроллера.",
                    group = "Аппаратная разгрузка NIC", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*RscIPv4", type = "REG_SZ",
                    apply = "1", revMode = "set", revVal = "0", srcFile = "network/desc.md", from = 309, to = 313, needsReview = true
                },
                new TweakDef {
                    id = "network-ndis-poll-mode",
                    name = "Опрос интерфейса NDIS Poll Mode (NdisPoll)",
                    desc = "Активирует современную модель опроса сетевого адаптера NDIS Poll Mode взамен медленных очередей отложенных вызовов DPC.",
                    group = "Сетевой стек и ядро", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*NdisPoll", type = "REG_SZ",
                    apply = "1", revMode = "set", revVal = "0", srcFile = "network/desc.md", from = 775, to = 785, needsReview = true
                },
                new TweakDef {
                    id = "network-congestion-provider-bbr",
                    name = "Контроль перегрузок TCP BBR2/CUBIC (CongestionProvider)",
                    desc = "Устанавливает передовой алгоритм контроля сетевых перегрузок BBR2/CUBIC для минимальной задержки RTT и низкого пинга в играх.",
                    group = "Сетевой стек и ядро", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", nameVal = "CongestionProvider", type = "REG_SZ",
                    apply = "BBR2", revMode = "set", revVal = "Default", srcFile = "network/desc.md", from = 860, to = 873, needsReview = true
                },
                new TweakDef {
                    id = "network-transmit-buffers",
                    name = "Максимальные буферы передачи (TransmitBuffers)",
                    desc = "Увеличивает число дескрипторов отправки пакетов до 1024, предотвращая потери пакетов при резких сетевых скачках.",
                    group = "Буферы и очереди", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*TransmitBuffers", type = "REG_SZ",
                    apply = "1024", revMode = "set", revVal = "512", srcFile = "network/desc.md", from = 1250, to = 1260, needsReview = true
                },
                new TweakDef {
                    id = "network-receive-buffers",
                    name = "Максимальные буферы приема (ReceiveBuffers)",
                    desc = "Увеличивает размер пула приемных буферов до 1024 для стабильного приема пакетов без очередей в ОЗУ.",
                    group = "Буферы и очереди", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*ReceiveBuffers", type = "REG_SZ",
                    apply = "1024", revMode = "set", revVal = "256", srcFile = "network/desc.md", from = 1260, to = 1270, needsReview = true
                },
                new TweakDef {
                    id = "network-interrupt-moderation-low",
                    name = "Отключить задержку прерываний сетевой карты (ITR)",
                    desc = "Отключает троттлинг прерываний сетевого адаптера (ITR = 0 / Off), гарантируя мгновенную обработку сетевых пакетов ядрами CPU.",
                    group = "Задержки и прерывания", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "ITR", type = "REG_SZ",
                    apply = "0", revMode = "set", revVal = "65535", srcFile = "network/desc.md", from = 1298, to = 1312, needsReview = true
                },
                new TweakDef {
                    id = "network-num-rss-queues",
                    name = "Многоядерное масштабирование приема RSS (NumRssQueues)",
                    desc = "Настраивает 4 независимые аппаратные очереди Receive-Side Scaling для распределения сетевого трафика по физическим ядрам процессора.",
                    group = "Задержки и прерывания", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*NumRssQueues", type = "REG_SZ",
                    apply = "4", revMode = "set", revVal = "1", srcFile = "network/desc.md", from = 1374, to = 1385, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-flow-control",
                    name = "Отключить управление потоком (FlowControl)",
                    desc = "Отключает паузы сетевой карты (Flow Control = 0 / Disabled), устраняя статтеры и задержки отклика в многопользовательских играх.",
                    group = "Задержки и прерывания", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = nicClassPath, nameVal = "*FlowControl", type = "REG_SZ",
                    apply = "0", revMode = "set", revVal = "3", srcFile = "network/desc.md", from = 1515, to = 1525, needsReview = true
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
                t["presets"] = new List<string> { d.preset };
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
            File.WriteAllText(jsonPath, updatedJson, System.Text.Encoding.UTF8);
            Console.WriteLine("Done! Added {0} tweaks. Total tweaks: {1}", addedCount, tweaks.Count);
        }
    }
}
