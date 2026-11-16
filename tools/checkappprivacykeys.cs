using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

class Program
{
    static void Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
        var ser = new JavaScriptSerializer();
        ser.MaxJsonLength = int.MaxValue;
        var tweaks = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(path, Encoding.UTF8));

        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in tweaks)
        {
            if (t.ContainsKey("registry") && t["registry"] is System.Collections.ArrayList)
            {
                var list = t["registry"] as System.Collections.ArrayList;
                foreach (Dictionary<string, object> r in list)
                {
                    usedNames.Add(r["name"].ToString());
                }
            }
        }

        string[] checkKeys = new string[]
        {
            "LetAppsAccessAccountInfo",
            "LetAppsAccessCalendar",
            "LetAppsAccessCallHistory",
            "LetAppsAccessCamera",
            "LetAppsAccessContacts",
            "LetAppsAccessEmail",
            "LetAppsAccessSystemAIModels",
            "LetAppsAccessGraphicsCaptureProgrammatic",
            "LetAppsAccessGraphicsCaptureWithoutBorder",
            "LetAppsAccessHumanPresence",
            "LetAppsAccessMessaging",
            "LetAppsAccessMotion",
            "LetAppsAccessNotifications",
            "LetAppsAccessPhone",
            "LetAppsAccessRadios",
            "LetAppsSyncWithDevices",
            "LetAppsAccessTasks",
            "LetAppsAccessTrustedDevices",
            "LetAppsGetDiagnosticInfo",
            "LetAppsAccessGazeInput",
            "LetAppsActivateWithVoice",
            "LetAppsActivateWithVoiceAboveLock",
            "LetAppsAccessBackgroundSpatialPerception",
            "AllowUserInfoAccess",
            "LetAppsAccessCellularData",
            "DisableAIDataAnalysis",
            "DisableClickToDo",
            "AllowRecallEnablement",
            "HideRecommendedSection",
            "HideRecommendedPersonalizedSites",
            "DisableSensors",
            "DisableLocation",
            "DisableLocationScripting",
            "DisableWindowsLocationProvider",
            "AllowProjectionToPC",
            "RequirePinForPairing",
            "RestrictRemoteClients",
            "LoggingEnabled",
            "CreateEncryptedOnlyTickets",
            "fAllowToGetHelp",
            "EnhancedAntiSpoofing",
            "NoAutorun",
            "NoDriveTypeAutoRun",
            "NoAutoplayfornonVolume",
            "DontSetAutoplayCheckbox",
            "DisableWebPnPDownload",
            "NoNotificationCenter",
            "NoToastApplicationNotification",
            "ShowOrHideMostUsedApps",
            "NoStartMenuMFUprogramsList",
            "HideRecentlyAddedApps",
            "DisallowAnimations",
            "HideTaskViewButton",
            "MultiTaskingAltTabFilter"
        };

        foreach (var k in checkKeys)
        {
            if (usedNames.Contains(k))
            {
                Console.WriteLine("USED: " + k);
            }
            else
            {
                Console.WriteLine("FREE: " + k);
            }
        }
    }
}
