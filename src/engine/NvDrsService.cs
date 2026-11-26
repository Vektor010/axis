using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WinTweaker.Engine
{
    /// <summary>
    /// Native NVIDIA Driver Settings (DRS) interface via nvapi64.dll.
    /// Applies and reverts real Noverse NV-Minimal Base Profile settings without external tools.
    /// </summary>
    public static class NvDrsService
    {
        private const int NVAPI_OK = 0;
        private const int NVAPI_SETTING_NOT_FOUND = -162;

        // Verified x64 sizeof(NVDRS_SETTING_V1) on NVIDIA Windows drivers:
        // version(4) + settingName[2048 wchar=4096] + settingId(4) + settingType(4) +
        // settingLocation(4) + isCurrentPredefined(4) + isPredefinedValid(4) +
        // predefinedValue(4100) + currentValue(4100) = 12320 bytes.
        private const int NVDRS_SETTING_SIZE = 12320;
        private const uint NVDRS_SETTING_VER = (1U << 16) | (uint)NVDRS_SETTING_SIZE;
        private const int OFFSET_SETTING_ID = 4 + 4096;
        private const int OFFSET_SETTING_TYPE = OFFSET_SETTING_ID + 4;
        private const int OFFSET_CUR_VALUE = NVDRS_SETTING_SIZE - 4100;

        private const uint QID_INITIALIZE = 0x0150E828;
        private const uint QID_UNLOAD = 0xD22BDD7E;
        private const uint QID_DRS_CREATE_SESSION = 0x0694D52E;
        private const uint QID_DRS_DESTROY_SESSION = 0xDAD9CFF8;
        private const uint QID_DRS_LOAD_SETTINGS = 0x375DBD6B;
        private const uint QID_DRS_SAVE_SETTINGS = 0xFCBC7E14;
        private const uint QID_DRS_GET_BASE_PROFILE = 0xDA8466A0;
        private const uint QID_DRS_GET_SETTING = 0x73BF8338;
        private const uint QID_DRS_SET_SETTING = 0x577DD202;
        private const uint QID_DRS_DELETE_PROFILE_SETTING = 0xE4A26362;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr nvapi_QueryInterface(uint id);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvApiSimpleDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvApiCreateSessionDelegate(out IntPtr phSession);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvApiSessionDelegate(IntPtr hSession);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvApiGetBaseProfileDelegate(IntPtr hSession, out IntPtr phProfile);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvApiProfileSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId, byte[] pSetting);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvApiSetSettingDelegate(IntPtr hSession, IntPtr hProfile, byte[] pSetting);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvApiDeleteSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId);

        public struct DrsEntry
        {
            public uint SettingId;
            public uint Value;
            public string Name;
        }

        // Core competitive Base Profile settings from Noverse NV-Minimal.nip (all key documented 3D settings)
        public static readonly DrsEntry[] MinimalProfileEntries = new DrsEntry[]
        {
            new DrsEntry { SettingId = 1675263U, Value = 1U, Name = "Shader Cache = On" },
            new DrsEntry { SettingId = 11306135U, Value = 0xFFFFFFFFU, Name = "Shader disk cache max size = Unlimited" },
            new DrsEntry { SettingId = 1686376U, Value = 0U, Name = "Texture filtering - Negative LOD bias = Allow" },
            new DrsEntry { SettingId = 3066610U, Value = 0U, Name = "Texture filtering - Trilinear opt = Off" },
            new DrsEntry { SettingId = 8703344U, Value = 1U, Name = "Texture filtering - Anisotropic filter opt = On" },
            new DrsEntry { SettingId = 15151633U, Value = 1U, Name = "Texture filtering - Anisotropic sample opt = On" },
            new DrsEntry { SettingId = 13510289U, Value = 20U, Name = "Texture filtering - Quality = High performance" },
            new DrsEntry { SettingId = 270426537U, Value = 1U, Name = "Anisotropic filtering setting = Off/App" },
            new DrsEntry { SettingId = 282245910U, Value = 1U, Name = "Anisotropic filtering mode = User-defined/Off" },
            new DrsEntry { SettingId = 8102046U, Value = 1U, Name = "Maximum pre-rendered frames = 1" },
            new DrsEntry { SettingId = 546199011U, Value = 1U, Name = "Maximum frames allowed = 1" },
            new DrsEntry { SettingId = 274197361U, Value = 1U, Name = "Power management = Prefer maximum performance" },
            new DrsEntry { SettingId = 11041231U, Value = 138504007U, Name = "Vertical Sync = Force off" },
            new DrsEntry { SettingId = 5912412U, Value = 2525368439U, Name = "Vertical Sync Tear Control = Adaptive off" },
            new DrsEntry { SettingId = 6600001U, Value = 1U, Name = "Preferred refresh rate = Highest available" },
            new DrsEntry { SettingId = 277041152U, Value = 0U, Name = "Low Latency Mode (FRL) = Off" },
            new DrsEntry { SettingId = 277041154U, Value = 0U, Name = "Frame Rate Limiter = Off" },
            new DrsEntry { SettingId = 277041157U, Value = 0U, Name = "Background App Max Frame Rate = Off" },
            new DrsEntry { SettingId = 276089202U, Value = 0U, Name = "FXAA = Off" },
            new DrsEntry { SettingId = 6714153U, Value = 0U, Name = "Ambient Occlusion = Off" },
            new DrsEntry { SettingId = 276757595U, Value = 1U, Name = "Antialiasing - Mode = Off" },
            new DrsEntry { SettingId = 294973784U, Value = 0U, Name = "G-SYNC Global = Off" },
            new DrsEntry { SettingId = 278196567U, Value = 0U, Name = "VRR global feature = Off" },
            new DrsEntry { SettingId = 549528094U, Value = 0U, Name = "Threaded optimization = Off" },
            new DrsEntry { SettingId = 553505273U, Value = 0U, Name = "Triple buffering = Off" },
            new DrsEntry { SettingId = 276158834U, Value = 0U, Name = "Enable Ansel = Off" }
        };

        private static T GetDelegate<T>(uint id) where T : class
        {
            IntPtr addr = nvapi_QueryInterface(id);
            if (addr == IntPtr.Zero) return null;
            return Marshal.GetDelegateForFunctionPointer(addr, typeof(T)) as T;
        }

        public static bool IsAvailable()
        {
            if (IntPtr.Size != 8) return false;
            try
            {
                IntPtr mod = LoadLibrary("nvapi64.dll");
                if (mod == IntPtr.Zero) return false;
                NvApiSimpleDelegate init = GetDelegate<NvApiSimpleDelegate>(QID_INITIALIZE);
                if (init == null) return false;
                return init() == NVAPI_OK;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryQueryMinimalProfile(out int matchedCount, out int totalCount, out string summary)
        {
            matchedCount = 0;
            totalCount = MinimalProfileEntries.Length;
            summary = "Недоступно (нет NVIDIA GPU)";

            if (!IsAvailable()) return false;

            IntPtr hSession = IntPtr.Zero;
            try
            {
                var createSess = GetDelegate<NvApiCreateSessionDelegate>(QID_DRS_CREATE_SESSION);
                var destroySess = GetDelegate<NvApiSessionDelegate>(QID_DRS_DESTROY_SESSION);
                var loadSettings = GetDelegate<NvApiSessionDelegate>(QID_DRS_LOAD_SETTINGS);
                var getBase = GetDelegate<NvApiGetBaseProfileDelegate>(QID_DRS_GET_BASE_PROFILE);
                var getSetting = GetDelegate<NvApiProfileSettingDelegate>(QID_DRS_GET_SETTING);
                var unload = GetDelegate<NvApiSimpleDelegate>(QID_UNLOAD);

                if (createSess == null || loadSettings == null || getBase == null || getSetting == null)
                    return false;

                if (createSess(out hSession) != NVAPI_OK) return false;
                if (loadSettings(hSession) != NVAPI_OK)
                {
                    if (destroySess != null) destroySess(hSession);
                    return false;
                }

                IntPtr hProfile;
                if (getBase(hSession, out hProfile) != NVAPI_OK || hProfile == IntPtr.Zero)
                {
                    if (destroySess != null) destroySess(hSession);
                    return false;
                }

                for (int i = 0; i < MinimalProfileEntries.Length; i++)
                {
                    DrsEntry entry = MinimalProfileEntries[i];
                    byte[] buf = new byte[NVDRS_SETTING_SIZE];
                    WriteUInt32(buf, 0, NVDRS_SETTING_VER);

                    int rc = getSetting(hSession, hProfile, entry.SettingId, buf);
                    if (rc == NVAPI_OK)
                    {
                        uint currentVal = ReadUInt32(buf, OFFSET_CUR_VALUE);
                        if (currentVal == entry.Value)
                        {
                            matchedCount++;
                        }
                    }
                }

                if (destroySess != null && hSession != IntPtr.Zero) destroySess(hSession);
                if (unload != null) try { unload(); } catch { }

                summary = string.Format("DRS: {0}/{1} параметров", matchedCount, totalCount);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ApplyMinimalProfile(out string snapshotSerialized, out string detail)
        {
            snapshotSerialized = "";
            detail = "nvapi64.dll недоступна";
            if (!IsAvailable()) return false;

            IntPtr hSession = IntPtr.Zero;
            var createSess = GetDelegate<NvApiCreateSessionDelegate>(QID_DRS_CREATE_SESSION);
            var destroySess = GetDelegate<NvApiSessionDelegate>(QID_DRS_DESTROY_SESSION);
            var loadSettings = GetDelegate<NvApiSessionDelegate>(QID_DRS_LOAD_SETTINGS);
            var saveSettings = GetDelegate<NvApiSessionDelegate>(QID_DRS_SAVE_SETTINGS);
            var getBase = GetDelegate<NvApiGetBaseProfileDelegate>(QID_DRS_GET_BASE_PROFILE);
            var getSetting = GetDelegate<NvApiProfileSettingDelegate>(QID_DRS_GET_SETTING);
            var setSetting = GetDelegate<NvApiSetSettingDelegate>(QID_DRS_SET_SETTING);

            if (createSess == null || loadSettings == null || saveSettings == null || getBase == null || getSetting == null || setSetting == null)
                return false;

            try
            {
                if (createSess(out hSession) != NVAPI_OK) return false;
                if (loadSettings(hSession) != NVAPI_OK) return false;

                IntPtr hProfile;
                if (getBase(hSession, out hProfile) != NVAPI_OK || hProfile == IntPtr.Zero) return false;

                StringBuilder snap = new StringBuilder();
                int setCount = 0;

                for (int i = 0; i < MinimalProfileEntries.Length; i++)
                {
                    DrsEntry entry = MinimalProfileEntries[i];
                    byte[] queryBuf = new byte[NVDRS_SETTING_SIZE];
                    WriteUInt32(queryBuf, 0, NVDRS_SETTING_VER);

                    int qRc = getSetting(hSession, hProfile, entry.SettingId, queryBuf);
                    if (qRc == NVAPI_OK)
                    {
                        uint prev = ReadUInt32(queryBuf, OFFSET_CUR_VALUE);
                        snap.AppendFormat("{0}={1};", entry.SettingId, prev);
                    }
                    else
                    {
                        snap.AppendFormat("{0}=ABSENT;", entry.SettingId);
                    }

                    byte[] setBuf = new byte[NVDRS_SETTING_SIZE];
                    WriteUInt32(setBuf, 0, NVDRS_SETTING_VER);
                    WriteUInt32(setBuf, OFFSET_SETTING_ID, entry.SettingId);
                    WriteUInt32(setBuf, OFFSET_SETTING_TYPE, 0U); // NVDRS_DWORD_TYPE
                    WriteUInt32(setBuf, OFFSET_CUR_VALUE, entry.Value);

                    if (setSetting(hSession, hProfile, setBuf) == NVAPI_OK)
                    {
                        setCount++;
                    }
                }

                int saveRc = saveSettings(hSession);
                snapshotSerialized = snap.ToString();
                detail = string.Format("Применено {0}/{1} DRS-настроек Base Profile (save={2})", setCount, MinimalProfileEntries.Length, saveRc);
                return saveRc == NVAPI_OK;
            }
            finally
            {
                if (hSession != IntPtr.Zero && destroySess != null)
                {
                    try { destroySess(hSession); } catch { }
                }
            }
        }

        public static bool RevertMinimalProfile(string snapshotSerialized, out string detail)
        {
            detail = "nvapi64.dll недоступна";
            if (!IsAvailable()) return false;

            IntPtr hSession = IntPtr.Zero;
            var createSess = GetDelegate<NvApiCreateSessionDelegate>(QID_DRS_CREATE_SESSION);
            var destroySess = GetDelegate<NvApiSessionDelegate>(QID_DRS_DESTROY_SESSION);
            var loadSettings = GetDelegate<NvApiSessionDelegate>(QID_DRS_LOAD_SETTINGS);
            var saveSettings = GetDelegate<NvApiSessionDelegate>(QID_DRS_SAVE_SETTINGS);
            var getBase = GetDelegate<NvApiGetBaseProfileDelegate>(QID_DRS_GET_BASE_PROFILE);
            var setSetting = GetDelegate<NvApiSetSettingDelegate>(QID_DRS_SET_SETTING);
            var delSetting = GetDelegate<NvApiDeleteSettingDelegate>(QID_DRS_DELETE_PROFILE_SETTING);

            if (createSess == null || loadSettings == null || saveSettings == null || getBase == null)
                return false;

            try
            {
                if (createSess(out hSession) != NVAPI_OK) return false;
                if (loadSettings(hSession) != NVAPI_OK) return false;

                IntPtr hProfile;
                if (getBase(hSession, out hProfile) != NVAPI_OK || hProfile == IntPtr.Zero) return false;

                Dictionary<uint, string> snapMap = new Dictionary<uint, string>();
                if (!string.IsNullOrEmpty(snapshotSerialized))
                {
                    string[] pairs = snapshotSerialized.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < pairs.Length; i++)
                    {
                        string[] kv = pairs[i].Split('=');
                        uint sid;
                        if (kv.Length == 2 && uint.TryParse(kv[0], out sid))
                        {
                            snapMap[sid] = kv[1];
                        }
                    }
                }

                for (int i = 0; i < MinimalProfileEntries.Length; i++)
                {
                    uint sid = MinimalProfileEntries[i].SettingId;
                    string prev;
                    uint prevVal;
                    if (snapMap.TryGetValue(sid, out prev) && prev != "ABSENT" && uint.TryParse(prev, out prevVal) && setSetting != null)
                    {
                        byte[] setBuf = new byte[NVDRS_SETTING_SIZE];
                        WriteUInt32(setBuf, 0, NVDRS_SETTING_VER);
                        WriteUInt32(setBuf, OFFSET_SETTING_ID, sid);
                        WriteUInt32(setBuf, OFFSET_SETTING_TYPE, 0U);
                        WriteUInt32(setBuf, OFFSET_CUR_VALUE, prevVal);
                        setSetting(hSession, hProfile, setBuf);
                    }
                    else if (delSetting != null)
                    {
                        delSetting(hSession, hProfile, sid);
                    }
                }

                int saveRc = saveSettings(hSession);
                detail = string.Format("DRS Base Profile восстановлен (save={0})", saveRc);
                return saveRc == NVAPI_OK;
            }
            finally
            {
                if (hSession != IntPtr.Zero && destroySess != null)
                {
                    try { destroySess(hSession); } catch { }
                }
            }
        }

        private static void WriteUInt32(byte[] buf, int offset, uint val)
        {
            buf[offset] = (byte)(val & 0xFF);
            buf[offset + 1] = (byte)((val >> 8) & 0xFF);
            buf[offset + 2] = (byte)((val >> 16) & 0xFF);
            buf[offset + 3] = (byte)((val >> 24) & 0xFF);
        }

        private static uint ReadUInt32(byte[] buf, int offset)
        {
            return (uint)buf[offset]
                | ((uint)buf[offset + 1] << 8)
                | ((uint)buf[offset + 2] << 16)
                | ((uint)buf[offset + 3] << 24);
        }
    }
}
