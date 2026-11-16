using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace WinTweaker.Engine
{
    // =========================================================================
    // Интерфейс хранилища реестра для абстракции и моков (C# 5.0)
    // =========================================================================
    public interface IRegistryStore
    {
        bool KeyExists(string hive, string subkey);
        bool ValueExists(string hive, string subkey, string valueName);
        object GetValue(string hive, string subkey, string valueName);
        RegistryValueKind GetValueKind(string hive, string subkey, string valueName);
        void SetValue(string hive, string subkey, string valueName, object value, RegistryValueKind kind);
        void DeleteValue(string hive, string subkey, string valueName);
        void CreateKey(string hive, string subkey);
    }

    // =========================================================================
    // Реальная реализация с 64-битным RegistryView и без раскрытия REG_EXPAND_SZ
    // =========================================================================
    public class RealRegistryStore : IRegistryStore
    {
        private RegistryKey GetBaseKey(string hive, bool writable)
        {
            RegistryHive h = RegistryHive.CurrentUser;
            string u = hive.ToUpperInvariant();
            if (u == "HKLM" || u == "HKEY_LOCAL_MACHINE")
            {
                h = RegistryHive.LocalMachine;
            }
            else if (u == "HKCU" || u == "HKEY_CURRENT_USER")
            {
                h = RegistryHive.CurrentUser;
            }
            else if (u == "HKCR" || u == "HKEY_CLASSES_ROOT")
            {
                h = RegistryHive.ClassesRoot;
            }

            return RegistryKey.OpenBaseKey(h, RegistryView.Registry64);
        }

        public bool KeyExists(string hive, string subkey)
        {
            using (RegistryKey root = this.GetBaseKey(hive, false))
            {
                using (RegistryKey sub = root.OpenSubKey(subkey, false))
                {
                    return sub != null;
                }
            }
        }

        public bool ValueExists(string hive, string subkey, string valueName)
        {
            using (RegistryKey root = this.GetBaseKey(hive, false))
            {
                using (RegistryKey sub = root.OpenSubKey(subkey, false))
                {
                    if (sub == null) return false;
                    string[] names = sub.GetValueNames();
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (string.Equals(names[i], valueName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        public object GetValue(string hive, string subkey, string valueName)
        {
            using (RegistryKey root = this.GetBaseKey(hive, false))
            {
                using (RegistryKey sub = root.OpenSubKey(subkey, false))
                {
                    if (sub == null) return null;
                    // DoNotExpandEnvironmentNames запрещает разворачивание REG_EXPAND_SZ
                    return sub.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                }
            }
        }

        public RegistryValueKind GetValueKind(string hive, string subkey, string valueName)
        {
            using (RegistryKey root = this.GetBaseKey(hive, false))
            {
                using (RegistryKey sub = root.OpenSubKey(subkey, false))
                {
                    if (sub == null) return RegistryValueKind.Unknown;
                    try
                    {
                        return sub.GetValueKind(valueName);
                    }
                    catch
                    {
                        return RegistryValueKind.Unknown;
                    }
                }
            }
        }

        public void SetValue(string hive, string subkey, string valueName, object value, RegistryValueKind kind)
        {
            using (RegistryKey root = this.GetBaseKey(hive, true))
            {
                using (RegistryKey sub = root.CreateSubKey(subkey, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (sub == null)
                    {
                        throw new InvalidOperationException("Не удалось открыть или создать раздел реестра: " + subkey);
                    }
                    sub.SetValue(valueName, value, kind);
                }
            }
        }

        public void DeleteValue(string hive, string subkey, string valueName)
        {
            using (RegistryKey root = this.GetBaseKey(hive, true))
            {
                using (RegistryKey sub = root.OpenSubKey(subkey, true))
                {
                    if (sub != null)
                    {
                        sub.DeleteValue(valueName, false);
                    }
                }
            }
        }

        public void CreateKey(string hive, string subkey)
        {
            using (RegistryKey root = this.GetBaseKey(hive, true))
            {
                using (RegistryKey sub = root.CreateSubKey(subkey))
                {
                }
            }
        }
    }

    // =========================================================================
    // Оверлей в памяти (MemoryOverlayRegistryStore) для тестового режима и моков
    // Читает реальный реестр, но все записи и удаления держит в оперативной памяти!
    // =========================================================================
    public class MemoryOverlayRegistryStore : IRegistryStore
    {
        private readonly IRegistryStore _underlying;
        private readonly Dictionary<string, object> _overlayValues;
        private readonly Dictionary<string, RegistryValueKind> _overlayKinds;
        private readonly HashSet<string> _deletedValues;
        private readonly HashSet<string> _createdKeys;

        public MemoryOverlayRegistryStore(IRegistryStore underlying)
        {
            this._underlying = underlying;
            this._overlayValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this._overlayKinds = new Dictionary<string, RegistryValueKind>(StringComparer.OrdinalIgnoreCase);
            this._deletedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._createdKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private string MakeKey(string hive, string subkey, string valueName)
        {
            return (hive + "\\" + subkey + "\\" + valueName).ToLowerInvariant();
        }

        private string MakeSubkey(string hive, string subkey)
        {
            return (hive + "\\" + subkey).ToLowerInvariant();
        }

        public bool KeyExists(string hive, string subkey)
        {
            string sk = this.MakeSubkey(hive, subkey);
            if (this._createdKeys.Contains(sk)) return true;
            if (this._underlying != null)
            {
                return this._underlying.KeyExists(hive, subkey);
            }
            return false;
        }

        public bool ValueExists(string hive, string subkey, string valueName)
        {
            string key = this.MakeKey(hive, subkey, valueName);
            if (this._deletedValues.Contains(key)) return false;
            if (this._overlayValues.ContainsKey(key)) return true;
            if (this._underlying != null)
            {
                return this._underlying.ValueExists(hive, subkey, valueName);
            }
            return false;
        }

        public object GetValue(string hive, string subkey, string valueName)
        {
            string key = this.MakeKey(hive, subkey, valueName);
            if (this._deletedValues.Contains(key)) return null;
            if (this._overlayValues.ContainsKey(key))
            {
                return this._overlayValues[key];
            }
            if (this._underlying != null)
            {
                return this._underlying.GetValue(hive, subkey, valueName);
            }
            return null;
        }

        public RegistryValueKind GetValueKind(string hive, string subkey, string valueName)
        {
            string key = this.MakeKey(hive, subkey, valueName);
            if (this._deletedValues.Contains(key)) return RegistryValueKind.Unknown;
            if (this._overlayKinds.ContainsKey(key))
            {
                return this._overlayKinds[key];
            }
            if (this._underlying != null)
            {
                return this._underlying.GetValueKind(hive, subkey, valueName);
            }
            return RegistryValueKind.Unknown;
        }

        public void SetValue(string hive, string subkey, string valueName, object value, RegistryValueKind kind)
        {
            string key = this.MakeKey(hive, subkey, valueName);
            this._deletedValues.Remove(key);
            this._overlayValues[key] = value;
            this._overlayKinds[key] = kind;
            this.CreateKey(hive, subkey);
        }

        public void DeleteValue(string hive, string subkey, string valueName)
        {
            string key = this.MakeKey(hive, subkey, valueName);
            this._overlayValues.Remove(key);
            this._overlayKinds.Remove(key);
            this._deletedValues.Add(key);
        }

        public void CreateKey(string hive, string subkey)
        {
            string sk = this.MakeSubkey(hive, subkey);
            this._createdKeys.Add(sk);
        }
    }
}
