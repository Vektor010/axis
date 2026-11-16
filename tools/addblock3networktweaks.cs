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

            var defs = new List<TweakDef>
            {
                // ==================== NETWORK ====================
                new TweakDef {
                    id = "network-disable-lltdio",
                    name = "Отключить протокол топологии LLTD (EnableLLTDIO)",
                    desc = "Отключает драйвер сопоставителя топологии канального уровня (LLTDIO), исключая лишние фоновые сетевые запросы.",
                    group = "Сетевые протоколы", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\LLTD", nameVal = "EnableLLTDIO", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "network/desc.md", from = 755, to = 762, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-rspndr",
                    name = "Отключить сетевой ответчик Responder (EnableRspndr)",
                    desc = "Запрещает отправку системных пакетов идентификации устройства в локальную сеть по протоколу Responder.",
                    group = "Сетевые протоколы", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\LLTD", nameVal = "EnableRspndr", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "network/desc.md", from = 755, to = 762, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-mdns",
                    name = "Отключить многоадресный DNS (EnableMDNS)",
                    desc = "Отключает широковещательные mDNS-запросы (порт 5353) для поиска локальных устройств, снижая сетевой шум.",
                    group = "DNS и разрешение имен", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows NT\DNSClient", nameVal = "EnableMDNS", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "network/desc.md", from = 1075, to = 1080, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-multicast",
                    name = "Отключить многоадресное разрешение имен LLMNR (EnableMulticast)",
                    desc = "Отключает протокол Link-Local Multicast Name Resolution (порт 5355), уязвимый к перехвату и подмене DNS.",
                    group = "DNS и разрешение имен", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows NT\DNSClient", nameVal = "EnableMulticast", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "network/desc.md", from = 1080, to = 1085, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-smart-name-resolution",
                    name = "Отключить параллельное разрешение имен (DisableSmartNameResolution)",
                    desc = "Предотвращает одновременную отправку DNS-запросов на все сетевые интерфейсы (Smart Multi-Homed), предотвращая утечки DNS.",
                    group = "DNS и разрешение имен", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows NT\DNSClient", nameVal = "DisableSmartNameResolution", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "network/desc.md", from = 1076, to = 1082, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-netbios",
                    name = "Отключить NetBIOS через TCP/IP (EnableNetbios)",
                    desc = "Отключает устаревший широковещательный протокол NetBIOS (порты 137-139), повышая сетевую безопасность и отклик.",
                    group = "Сетевые протоколы", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows NT\DNSClient", nameVal = "EnableNetbios", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "network/desc.md", from = 1077, to = 1083, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-active-probing",
                    name = "Отключить активные сетевые зонды NCSI (NoActiveProbe)",
                    desc = "Отключает периодическую отправку HTTP-запросов к серверам проверки подключения msftconnecttest.com.",
                    group = "Сетевая телеметрия NCSI", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator", nameVal = "NoActiveProbe", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "network/desc.md", from = 990, to = 996, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-passive-polling",
                    name = "Отключить пассивный опрос подключения (DisablePassivePolling)",
                    desc = "Отключает 15-секундный фоновый анализ hop count сетевых пакетов службой индикатора состояния сети.",
                    group = "Сетевая телеметрия NCSI", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator", nameVal = "DisablePassivePolling", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "network/desc.md", from = 992, to = 998, needsReview = true
                },
                new TweakDef {
                    id = "network-disable-ipv6-components",
                    name = "Отключить компоненты IPv6 (DisabledComponents)",
                    desc = "Отключает неиспользуемые компоненты протокола IPv6, предотвращая скрытые таймауты сетевого стека.",
                    group = "Сетевой стек", category = "network", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters", nameVal = "DisabledComponents", type = "REG_DWORD",
                    apply = 255, revMode = "set", revVal = 0, srcFile = "network/desc.md", from = 1095, to = 1105, needsReview = true
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
