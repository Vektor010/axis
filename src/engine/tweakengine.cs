using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using WinTweaker.Common;
using WinTweaker.Models;

namespace WinTweaker.Engine
{
    public class BatchApplyResult
    {
        public int AppliedCount { get; set; }
        public int ErrorCount { get; set; }
        public int SkippedCount { get; set; }
        public string RestorePointMessage { get; set; }
        public bool RestorePointCreated { get; set; }

        public BatchApplyResult()
        {
            this.RestorePointMessage = "";
        }
    }

    public class BatchRevertResult
    {
        public int RevertedCount { get; set; }
        public int ErrorCount { get; set; }
        public int SkippedCount { get; set; }
    }

    public class TweakEngine
    {
        private readonly IRegistryStore _store;
        private readonly StateManager _stateManager;
        private readonly JournalManager _journalManager;
        private readonly bool _isRealMode;
        private readonly RestorePointManager _restorePointManager;

        public IRegistryStore Store
        {
            get { return this._store; }
        }

        public StateManager StateManager
        {
            get { return this._stateManager; }
        }

        public JournalManager JournalManager
        {
            get { return this._journalManager; }
        }

        public bool IsRealMode
        {
            get { return this._isRealMode; }
        }

        public RestorePointManager RestorePointManager
        {
            get { return this._restorePointManager; }
        }

        public static bool IsEligibleForBatchApply(TweakItem tweak, int currentBuild, TweakStatus currentStatus)
        {
            if (tweak == null) return false;
            if (tweak.registry == null || tweak.registry.Count == 0) return false;

            // Исключается раздел NVIDIA и разовые cleanup/action операции
            if (string.Equals(tweak.category, "nvidia", StringComparison.OrdinalIgnoreCase)) return false;
            if (SpecialTweakExecutor.IsCleanupAction(tweak)) return false;

            // Входят только твики типа toggle
            if (!string.Equals(tweak.type, "toggle", StringComparison.OrdinalIgnoreCase)) return false;

            // Входят risk low или medium (high исключается)
            bool isLowOrMed = string.Equals(tweak.risk, "low", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(tweak.risk, "medium", StringComparison.OrdinalIgnoreCase);
            if (!isLowOrMed) return false;

            // Доступные на этой сборке (min_build)
            if (tweak.min_build.HasValue && currentBuild < tweak.min_build.Value) return false;

            // Ещё не включённые (и не недоступные)
            if (currentStatus == TweakStatus.Enabled || currentStatus == TweakStatus.Unavailable) return false;

            // Твики с apply_block исключаются
            for (int i = 0; i < tweak.registry.Count; i++)
            {
                if (!string.IsNullOrEmpty(tweak.registry[i].apply_block))
                {
                    return false;
                }
            }

            // Твики с needs_review входят
            return true;
        }

        public List<TweakItem> GetEligibleTweaksForBatchApply(IEnumerable<TweakItem> tweaks, string category, int? currentBuild = null)
        {
            List<TweakItem> result = new List<TweakItem>();
            if (tweaks == null || string.IsNullOrEmpty(category)) return result;

            if (string.Equals(category, "nvidia", StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            int build = currentBuild.HasValue ? currentBuild.Value : Environment.OSVersion.Version.Build;

            foreach (TweakItem tweak in tweaks)
            {
                if (!string.Equals(tweak.category, category, StringComparison.OrdinalIgnoreCase)) continue;

                TweakStatus status = this.CheckStatus(tweak, build);
                if (IsEligibleForBatchApply(tweak, build, status))
                {
                    result.Add(tweak);
                }
            }

            return result;
        }

        public List<TweakItem> GetEligibleTweaksForBatchRevert(IEnumerable<TweakItem> tweaks, string category)
        {
            List<TweakItem> result = new List<TweakItem>();
            if (tweaks == null || string.IsNullOrEmpty(category)) return result;

            if (string.Equals(category, "nvidia", StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            foreach (TweakItem tweak in tweaks)
            {
                if (!string.Equals(tweak.category, category, StringComparison.OrdinalIgnoreCase)) continue;

                if (this._stateManager != null && this._stateManager.HasSnapshot(tweak.id))
                {
                    result.Add(tweak);
                }
            }

            return result;
        }

        public BatchApplyResult ApplyBatch(
            IList<TweakItem> tweaks,
            string categoryName,
            Action<int, int> progressCallback = null,
            Func<bool> isCancelledFunc = null)
        {
            BatchApplyResult result = new BatchApplyResult();
            if (tweaks == null || tweaks.Count == 0) return result;

            // 1. Точка восстановления (не чаще раза в 24 ч, если недоступна - продолжить и сообщить в итоге)
            string rpMsg = "";
            if (this._restorePointManager != null)
            {
                result.RestorePointCreated = this._restorePointManager.EnsureRestorePoint(
                    string.Format("WinTweaker: раздел {0}", categoryName ?? ""),
                    this._journalManager,
                    out rpMsg);
                result.RestorePointMessage = rpMsg;
            }

            // 2. Экспорт .reg и снимок исходных значений в state.json делаются перед записью каждого твика в this.Apply(tweak)
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (isCancelledFunc != null && isCancelledFunc())
                {
                    result.SkippedCount += (tweaks.Count - i);
                    break;
                }

                if (progressCallback != null)
                {
                    progressCallback(i + 1, tweaks.Count);
                }

                TweakItem tweak = tweaks[i];
                try
                {
                    bool ok = this.Apply(tweak);
                    if (ok)
                    {
                        result.AppliedCount++;
                    }
                    else
                    {
                        result.ErrorCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    if (this._journalManager != null)
                    {
                        this._journalManager.Log("ERROR", tweak.id, "Исключение при пакетном применении: " + ex.Message);
                    }
                }
            }

            if (this._journalManager != null)
            {
                this._journalManager.Log("BATCH_APPLY", categoryName ?? "",
                    string.Format("Раздел '{0}': применено {1}, ошибок {2}, пропущено {3}",
                        categoryName ?? "", result.AppliedCount, result.ErrorCount, result.SkippedCount));
            }

            return result;
        }

        public BatchRevertResult RevertBatch(
            IList<TweakItem> allCategoryTweaks,
            string categoryName,
            Action<int, int> progressCallback = null,
            Func<bool> isCancelledFunc = null)
        {
            BatchRevertResult result = new BatchRevertResult();
            if (allCategoryTweaks == null || allCategoryTweaks.Count == 0) return result;

            List<TweakItem> withSnapshot = new List<TweakItem>();
            int skippedWithoutSnapshot = 0;

            for (int i = 0; i < allCategoryTweaks.Count; i++)
            {
                TweakItem t = allCategoryTweaks[i];
                if (this._stateManager != null && this._stateManager.HasSnapshot(t.id))
                {
                    withSnapshot.Add(t);
                }
                else
                {
                    skippedWithoutSnapshot++;
                }
            }

            result.SkippedCount = skippedWithoutSnapshot;

            for (int i = 0; i < withSnapshot.Count; i++)
            {
                if (isCancelledFunc != null && isCancelledFunc())
                {
                    result.SkippedCount += (withSnapshot.Count - i);
                    break;
                }

                if (progressCallback != null)
                {
                    progressCallback(i + 1, withSnapshot.Count);
                }

                TweakItem tweak = withSnapshot[i];
                try
                {
                    bool ok = this.Revert(tweak);
                    if (ok)
                    {
                        result.RevertedCount++;
                    }
                    else
                    {
                        result.ErrorCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    if (this._journalManager != null)
                    {
                        this._journalManager.Log("ERROR", tweak.id, "Исключение при пакетном откате: " + ex.Message);
                    }
                }
            }

            if (this._journalManager != null)
            {
                this._journalManager.Log("BATCH_REVERT", categoryName ?? "",
                    string.Format("Раздел '{0}': откачено {1}, ошибок {2}, пропущено {3}",
                        categoryName ?? "", result.RevertedCount, result.ErrorCount, result.SkippedCount));
            }

            return result;
        }

        public static string BuildBatchApplyConfirmationMessage(
            string categoryName,
            IList<TweakItem> tweaksToApply,
            bool isTestMode)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("Раздел: {0}", categoryName));
            sb.AppendLine(string.Format("Число твиков для применения: {0}", tweaksToApply != null ? tweaksToApply.Count : 0));

            int needsReviewCount = 0;
            if (tweaksToApply != null)
            {
                for (int i = 0; i < tweaksToApply.Count; i++)
                {
                    if (tweaksToApply[i].needs_review) needsReviewCount++;
                }
            }
            sb.AppendLine(string.Format("Требуют проверки: {0}", needsReviewCount));
            sb.AppendLine();
            sb.AppendLine("Первые твики к применению:");

            if (tweaksToApply != null)
            {
                int limit = Math.Min(10, tweaksToApply.Count);
                for (int i = 0; i < limit; i++)
                {
                    string name = !string.IsNullOrEmpty(tweaksToApply[i].name_ru) ? tweaksToApply[i].name_ru : tweaksToApply[i].id;
                    sb.AppendLine(string.Format("{0}. {1}", i + 1, name));
                }
                if (tweaksToApply.Count > 10)
                {
                    sb.AppendLine(string.Format("... и ещё {0} твиков", tweaksToApply.Count - 10));
                }
            }

            sb.AppendLine();
            sb.AppendLine("Откат возможен только из сохранённого исходного значения.");

            if (isTestMode)
            {
                sb.AppendLine();
                sb.AppendLine("Тестовый режим: реестр не меняется.");
            }

            return sb.ToString();
        }

        public static string BuildBatchRevertConfirmationMessage(
            string categoryName,
            int countToRevert,
            int skippedCount,
            bool isTestMode)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("Раздел: {0}", categoryName));
            sb.AppendLine(string.Format("Число твиков для отката (со снимком): {0}", countToRevert));
            if (skippedCount > 0)
            {
                sb.AppendLine(string.Format("Будет пропущено (без снимка): {0}", skippedCount));
            }
            sb.AppendLine();
            sb.AppendLine("Откатить указанные твики к сохранённым исходным значениям?");

            if (isTestMode)
            {
                sb.AppendLine();
                sb.AppendLine("Тестовый режим: реестр не меняется.");
            }

            return sb.ToString();
        }

        public TweakEngine(IRegistryStore store, StateManager stateManager, JournalManager journalManager, bool isRealMode, RestorePointManager rpm = null)
        {
            this._store = store;
            this._stateManager = stateManager;
            this._journalManager = journalManager;
            this._isRealMode = isRealMode;
            this._restorePointManager = rpm ?? new RestorePointManager(isRealMode, stateManager);
        }

        public bool Check(TweakItem tweak)
        {
            return this.CheckStatus(tweak) == TweakStatus.Enabled;
        }

        public TweakStatus CheckStatus(TweakItem tweak, int? currentBuild = null)
        {
            if (tweak == null || tweak.registry == null || tweak.registry.Count == 0)
            {
                return TweakStatus.Unknown;
            }

            int build = currentBuild.HasValue ? currentBuild.Value : Environment.OSVersion.Version.Build;
            if (tweak.min_build.HasValue && build < tweak.min_build.Value)
            {
                return TweakStatus.Unavailable;
            }

            if (SpecialTweakExecutor.IsCustomRealExecutorTweak(tweak))
            {
                TweakStatus specialStatus = SpecialTweakExecutor.CheckSpecialStatus(tweak, this._store, this._stateManager, this._isRealMode);
                if (specialStatus != TweakStatus.Unknown || SpecialTweakExecutor.IsCleanupAction(tweak))
                {
                    return specialStatus;
                }
            }

            // Специальная проверка автозапуска Steam в реальном режиме (учитывает Valve SuppressAutoRun и Windows StartupApproved)
            if (this._isRealMode && string.Equals(tweak.id, "steam-disable-autorun", StringComparison.OrdinalIgnoreCase))
            {
                return SteamConfigHelper.IsSteamAutoRunDisabled() ? TweakStatus.Enabled : TweakStatus.Disabled;
            }

            int totalTargets = 0;
            int applyMatches = 0;
            int revertMatches = 0;
            int foreignCount = 0;

            for (int i = 0; i < tweak.registry.Count; i++)
            {
                RegistryActionItem action = tweak.registry[i];
                List<string> targetPaths = SpecialTweakExecutor.ResolveTargetPaths(tweak, action, this._isRealMode);
                if (targetPaths.Count == 0)
                {
                    return TweakStatus.Unavailable;
                }

                for (int p = 0; p < targetPaths.Count; p++)
                {
                    totalTargets++;
                    string targetPath = targetPaths[p];

                    if (!this._store.ValueExists(action.hive, targetPath, action.name))
                    {
                        // Для оверлея Steam: если в реестре нет значения, проверяем реальный localconfig.vdf
                        if (string.Equals(tweak.id, "steam-disable-game-overlay", StringComparison.OrdinalIgnoreCase))
                        {
                            int? vdfVal = SteamConfigHelper.GetGameOverlaySetting();
                            if (vdfVal.HasValue)
                            {
                                if (vdfVal.Value == 0) applyMatches++;
                                else if (vdfVal.Value == 1) revertMatches++;
                                else foreignCount++;
                                continue;
                            }
                        }

                        bool isRevertAbsent = false;
                        if (action.revert != null && (action.revert.mode == "delete" || action.revert.mode == "absent"))
                        {
                            isRevertAbsent = true;
                        }
                        else if (this._stateManager != null && this._stateManager.HasSnapshot(tweak.id))
                        {
                            TweakSnapshot snap = this._stateManager.GetSnapshot(tweak.id);
                            if (snap != null && snap.Values != null)
                            {
                                for (int sv = 0; sv < snap.Values.Count; sv++)
                                {
                                    if (string.Equals(snap.Values[sv].Path, targetPath, StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(snap.Values[sv].Name, action.name, StringComparison.OrdinalIgnoreCase) &&
                                        !snap.Values[sv].Existed)
                                    {
                                        isRevertAbsent = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (isRevertAbsent)
                        {
                            revertMatches++;
                        }
                        else
                        {
                            foreignCount++;
                        }
                        continue;
                    }

                    object curVal = this._store.GetValue(action.hive, targetPath, action.name);
                    if (curVal == null)
                    {
                        foreignCount++;
                        continue;
                    }

                    object expectedApply = action.apply;
                    if (this._isRealMode && string.Equals(tweak.id, "affinities-gpu-core-affinity", StringComparison.OrdinalIgnoreCase))
                    {
                        expectedApply = SpecialTweakExecutor.ComputeOptimalGpuAffinityMask();
                    }

                    string cmpType = action.type;
                    if (string.Equals(action.name, "MinAnimate", StringComparison.OrdinalIgnoreCase) && !(curVal is int))
                    {
                        cmpType = "REG_SZ";
                    }

                    bool isApply = SpecialTweakExecutor.RegistryValuesEqual(cmpType, curVal, expectedApply);
                    bool isRevert = false;

                    if (action.revert != null && action.revert.value != null &&
                        SpecialTweakExecutor.RegistryValuesEqual(cmpType, curVal, action.revert.value))
                    {
                        isRevert = true;
                    }
                    else if (this._stateManager != null && this._stateManager.HasSnapshot(tweak.id))
                    {
                        TweakSnapshot snap = this._stateManager.GetSnapshot(tweak.id);
                        if (snap != null && snap.Values != null)
                        {
                            for (int sv = 0; sv < snap.Values.Count; sv++)
                            {
                                RegistryValueSnapshot rvs = snap.Values[sv];
                                if (string.Equals(rvs.Path, targetPath, StringComparison.OrdinalIgnoreCase) &&
                                    string.Equals(rvs.Name, action.name, StringComparison.OrdinalIgnoreCase) &&
                                    rvs.Value != null &&
                                    SpecialTweakExecutor.RegistryValuesEqual(cmpType, curVal, rvs.Value))
                                {
                                    isRevert = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (isApply) applyMatches++;
                    else if (isRevert) revertMatches++;
                    else foreignCount++;
                }
            }

            if (totalTargets == 0)
            {
                return TweakStatus.Unavailable;
            }
            if (applyMatches == totalTargets)
            {
                return TweakStatus.Enabled;
            }
            if (revertMatches == totalTargets)
            {
                return TweakStatus.Disabled;
            }
            if (foreignCount > 0)
            {
                return (applyMatches > 0 || revertMatches > 0) ? TweakStatus.PartiallyEnabled : TweakStatus.Unknown;
            }
            if (applyMatches > 0)
            {
                return TweakStatus.PartiallyEnabled;
            }

            return TweakStatus.Disabled;
        }

        public bool Apply(TweakItem tweak)
        {
            if (tweak == null || tweak.registry == null) return false;

            try
            {
                if (SpecialTweakExecutor.IsCustomRealExecutorTweak(tweak) &&
                    (this._isRealMode || SpecialTweakExecutor.IsCleanupAction(tweak)))
                {
                    if (this._restorePointManager != null && this._isRealMode && !SpecialTweakExecutor.IsCleanupAction(tweak))
                    {
                        string rpMsg;
                        this._restorePointManager.EnsureRestorePoint(tweak.name_ru ?? tweak.id, this._journalManager, out rpMsg);
                    }
                    return SpecialTweakExecutor.ApplySpecial(tweak, this._store, this._stateManager, this._journalManager, this._isRealMode);
                }

                // 0. Обеспечение точки восстановления перед применением
                if (this._restorePointManager != null)
                {
                    string rpMsg;
                    this._restorePointManager.EnsureRestorePoint(tweak.name_ru ?? tweak.id, this._journalManager, out rpMsg);
                }

                // Предварительно разрешаем все целевые пути
                List<RegistryActionItem> expandedActions = new List<RegistryActionItem>();
                List<string> expandedPaths = new List<string>();
                for (int i = 0; i < tweak.registry.Count; i++)
                {
                    RegistryActionItem action = tweak.registry[i];
                    List<string> targets = SpecialTweakExecutor.ResolveTargetPaths(tweak, action, this._isRealMode);
                    if (targets.Count == 0)
                    {
                        this._journalManager.Log("WARN", tweak.id, "Целевое устройство/ветка реестра не найдены в системе.");
                        return false;
                    }
                    for (int p = 0; p < targets.Count; p++)
                    {
                        expandedActions.Add(action);
                        expandedPaths.Add(targets[p]);
                    }
                }

                // 1. Создаем снимок исходного состояния ДО применения, если снимка еще нет
                if (!this._stateManager.HasSnapshot(tweak.id))
                {
                    TweakSnapshot snapshot = new TweakSnapshot();
                    snapshot.TweakId = tweak.id;

                    for (int i = 0; i < expandedActions.Count; i++)
                    {
                        RegistryActionItem action = expandedActions[i];
                        string targetPath = expandedPaths[i];

                        RegistryValueSnapshot valSnap = new RegistryValueSnapshot();
                        valSnap.Hive = action.hive;
                        valSnap.Path = targetPath;
                        valSnap.Name = action.name;
                        valSnap.Existed = this._store.ValueExists(action.hive, targetPath, action.name);

                        if (valSnap.Existed)
                        {
                            object rawCur = this._store.GetValue(action.hive, targetPath, action.name);
                            valSnap.Kind = this._store.GetValueKind(action.hive, targetPath, action.name).ToString();
                            byte[] rawBytes = rawCur as byte[];
                            valSnap.Value = (rawBytes != null) ? SpecialTweakExecutor.FormatHexBytes(rawBytes) : rawCur;
                        }
                        else
                        {
                            valSnap.Value = null;
                            valSnap.Kind = null;
                        }

                        snapshot.Values.Add(valSnap);

                        // Если реальный режим - экспортируем .reg файл до записи
                        if (this._isRealMode)
                        {
                            this._journalManager.ExportRegBackup(tweak.id, action.hive, targetPath, action.name, valSnap.Value, action.type);
                        }
                    }

                    this._stateManager.SaveSnapshot(tweak.id, snapshot);
                }

                // 2. Применяем значения в реестр/оверлей
                for (int i = 0; i < expandedActions.Count; i++)
                {
                    RegistryActionItem action = expandedActions[i];
                    string targetPath = expandedPaths[i];
                    string effectiveType = action.type;
                    object rawApply = action.apply;

                    if (this._isRealMode && string.Equals(action.name, "MinAnimate", StringComparison.OrdinalIgnoreCase) &&
                        targetPath.EndsWith(@"\WindowMetrics", StringComparison.OrdinalIgnoreCase))
                    {
                        effectiveType = "REG_SZ";
                        rawApply = rawApply != null ? rawApply.ToString() : "0";
                    }
                    else if (this._isRealMode && string.Equals(tweak.id, "affinities-gpu-core-affinity", StringComparison.OrdinalIgnoreCase))
                    {
                        rawApply = SpecialTweakExecutor.ComputeOptimalGpuAffinityMask();
                    }

                    RegistryValueKind kind;
                    object valToWrite = SpecialTweakExecutor.ConvertToRegistryValue(effectiveType, rawApply, out kind);

                    this._store.SetValue(action.hive, targetPath, action.name, valToWrite, kind);

                    if (this._isRealMode)
                    {
                        NativeMethods.ApplyLiveOsRefresh(action.hive, targetPath, action.name, valToWrite);
                        if (string.Equals(action.name, "Start", StringComparison.OrdinalIgnoreCase) && valToWrite is int)
                        {
                            SpecialTweakExecutor.ApplyServiceRuntimeState(targetPath, (int)valToWrite);
                        }
                    }
                }

                // Специальные действия для Steam в реальном режиме
                if (this._isRealMode && string.Equals(tweak.id, "steam-disable-game-overlay", StringComparison.OrdinalIgnoreCase))
                {
                    SteamConfigHelper.SetGameOverlaySetting(0);
                }
                if (this._isRealMode && string.Equals(tweak.id, "steam-disable-autorun", StringComparison.OrdinalIgnoreCase))
                {
                    SteamConfigHelper.SetSteamAutoRunDisabled(true);
                }

                string modeStr = this._isRealMode ? "РЕАЛЬНЫЙ РЕЖИМ" : "ТЕСТОВЫЙ РЕЖИМ (оверлей)";
                this._journalManager.Log("APPLY", tweak.id, string.Format("Твик '{0}' успешно применён ({1})", tweak.name_ru, modeStr));
                return true;
            }
            catch (Exception ex)
            {
                this._journalManager.Log("ERROR", tweak.id, "Ошибка применения: " + ex.Message);
                return false;
            }
        }

        public bool Revert(TweakItem tweak)
        {
            if (tweak == null || tweak.registry == null) return false;

            try
            {
                if (SpecialTweakExecutor.IsCustomRealExecutorTweak(tweak) &&
                    (this._isRealMode || SpecialTweakExecutor.IsCleanupAction(tweak)))
                {
                    return SpecialTweakExecutor.RevertSpecial(tweak, this._store, this._stateManager, this._journalManager, this._isRealMode);
                }

                TweakSnapshot snapshot = this._stateManager.GetSnapshot(tweak.id);

                for (int i = 0; i < tweak.registry.Count; i++)
                {
                    RegistryActionItem action = tweak.registry[i];
                    List<string> targets = SpecialTweakExecutor.ResolveTargetPaths(tweak, action, this._isRealMode);
                    string mode = "snapshot";
                    if (action.revert != null && !string.IsNullOrEmpty(action.revert.mode))
                    {
                        mode = action.revert.mode.ToLowerInvariant();
                    }

                    if (mode == "snapshot" && snapshot != null && snapshot.Values != null && snapshot.Values.Count > 0)
                    {
                        for (int s = 0; s < snapshot.Values.Count; s++)
                        {
                            RegistryValueSnapshot valSnap = snapshot.Values[s];
                            if (!string.Equals(valSnap.Name, action.name, StringComparison.OrdinalIgnoreCase)) continue;

                            if (valSnap.Existed && valSnap.Value != null)
                            {
                                string snapType = action.type;
                                if (string.Equals(valSnap.Kind, "String", StringComparison.OrdinalIgnoreCase)) snapType = "REG_SZ";
                                else if (string.Equals(valSnap.Kind, "Binary", StringComparison.OrdinalIgnoreCase)) snapType = "REG_BINARY";
                                else if (string.Equals(valSnap.Kind, "QWord", StringComparison.OrdinalIgnoreCase)) snapType = "REG_QWORD";
                                else if (string.Equals(valSnap.Kind, "ExpandString", StringComparison.OrdinalIgnoreCase)) snapType = "REG_EXPAND_SZ";
                                else if (string.Equals(valSnap.Kind, "MultiString", StringComparison.OrdinalIgnoreCase)) snapType = "REG_MULTI_SZ";

                                RegistryValueKind k;
                                object valToWrite = SpecialTweakExecutor.ConvertToRegistryValue(snapType, valSnap.Value, out k);
                                string writePath = !string.IsNullOrEmpty(valSnap.Path) ? valSnap.Path : (targets.Count > 0 ? targets[0] : action.path);
                                this._store.SetValue(valSnap.Hive ?? action.hive, writePath, action.name, valToWrite, k);
                                if (this._isRealMode)
                                {
                                    NativeMethods.ApplyLiveOsRefresh(valSnap.Hive ?? action.hive, writePath, action.name, valToWrite);
                                    if (string.Equals(action.name, "Start", StringComparison.OrdinalIgnoreCase) && valToWrite is int)
                                    {
                                        SpecialTweakExecutor.ApplyServiceRuntimeState(writePath, (int)valToWrite);
                                    }
                                }
                            }
                            else
                            {
                                string delPath = !string.IsNullOrEmpty(valSnap.Path) ? valSnap.Path : (targets.Count > 0 ? targets[0] : action.path);
                                this._store.DeleteValue(valSnap.Hive ?? action.hive, delPath, action.name);
                                if (this._isRealMode)
                                {
                                    NativeMethods.ApplyLiveOsRefresh(valSnap.Hive ?? action.hive, delPath, action.name, null);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int p = 0; p < targets.Count; p++)
                        {
                            string targetPath = targets[p];
                            if (mode == "set")
                            {
                                string effectiveType = action.type;
                                object val = action.revert != null ? action.revert.value : 0;
                                if (this._isRealMode && string.Equals(action.name, "MinAnimate", StringComparison.OrdinalIgnoreCase) &&
                                    targetPath.EndsWith(@"\WindowMetrics", StringComparison.OrdinalIgnoreCase))
                                {
                                    effectiveType = "REG_SZ";
                                    val = val != null ? val.ToString() : "1";
                                }
                                RegistryValueKind k;
                                object valToWrite = SpecialTweakExecutor.ConvertToRegistryValue(effectiveType, val, out k);
                                this._store.SetValue(action.hive, targetPath, action.name, valToWrite, k);
                                if (this._isRealMode)
                                {
                                    NativeMethods.ApplyLiveOsRefresh(action.hive, targetPath, action.name, valToWrite);
                                    if (string.Equals(action.name, "Start", StringComparison.OrdinalIgnoreCase) && valToWrite is int)
                                    {
                                        SpecialTweakExecutor.ApplyServiceRuntimeState(targetPath, (int)valToWrite);
                                    }
                                }
                            }
                            else if (mode == "delete")
                            {
                                this._store.DeleteValue(action.hive, targetPath, action.name);
                                if (this._isRealMode)
                                {
                                    NativeMethods.ApplyLiveOsRefresh(action.hive, targetPath, action.name, null);
                                }
                            }
                        }
                    }
                }

                this._stateManager.RemoveSnapshot(tweak.id);

                // Специальные действия для Steam в реальном режиме
                if (this._isRealMode && string.Equals(tweak.id, "steam-disable-game-overlay", StringComparison.OrdinalIgnoreCase))
                {
                    SteamConfigHelper.SetGameOverlaySetting(1);
                }
                if (this._isRealMode && string.Equals(tweak.id, "steam-disable-autorun", StringComparison.OrdinalIgnoreCase))
                {
                    SteamConfigHelper.SetSteamAutoRunDisabled(false);
                }

                string modeStr = this._isRealMode ? "РЕАЛЬНЫЙ РЕЖИМ" : "ТЕСТОВЫЙ РЕЖИМ (оверлей)";
                this._journalManager.Log("REVERT", tweak.id, string.Format("Твик '{0}' возвращён в исходное состояние ({1})", tweak.name_ru, modeStr));
                return true;
            }
            catch (Exception ex)
            {
                this._journalManager.Log("ERROR", tweak.id, "Ошибка отката: " + ex.Message);
                return false;
            }
        }
    }
}
