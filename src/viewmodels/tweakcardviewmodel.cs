using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinTweaker.Common;
using WinTweaker.Engine;
using WinTweaker.Models;

namespace WinTweaker.ViewModels
{
    public class TweakCardViewModel : ObservableObject
    {
        private readonly TweakItem _model;
        private readonly TweakEngine _engine;
        private readonly Action _onStateChanged;

        private string _id;
        private string _name;
        private string _category;
        private string _group;
        private string _description;
        private TweakRisk _risk;
        private bool _requiresReboot;
        private TweakStatus _status;
        private bool _isChecked;
        private bool _isBusy;
        private string _statusMessage;
        private string _sourceLocation;
        private bool _isExpanded;
        private bool _isSelectedForBatch;
        private int _sourceIndex;
        private readonly Action _onSelectionChanged;

        public int SourceIndex
        {
            get { return this._sourceIndex; }
            set { this._sourceIndex = value; this.OnPropertyChanged("SourceIndex"); }
        }

        public TweakItem Model
        {
            get { return this._model; }
        }

        public string Id
        {
            get { return this._id; }
            set { this._id = value; this.OnPropertyChanged("Id"); }
        }

        public string Name
        {
            get { return this._name; }
            set { this._name = value; this.OnPropertyChanged("Name"); }
        }

        public string Category
        {
            get { return this._category; }
            set { this._category = value; this.OnPropertyChanged("Category"); }
        }

        public string Group
        {
            get { return this._group; }
            set { this._group = value; this.OnPropertyChanged("Group"); }
        }

        public string Description
        {
            get { return this._description; }
            set
            {
                this._description = value;
                this.OnPropertyChanged("Description");
                this.OnPropertyChanged("HasDescription");
                this.OnPropertyChanged("ExpandedDescription");
            }
        }

        public bool HasDescription
        {
            get { return !string.IsNullOrEmpty(this._description); }
        }

        public string ExpandedDescription
        {
            get
            {
                if (string.IsNullOrEmpty(this._description))
                {
                    return "Описание в источнике отсутствует";
                }
                return this._description;
            }
        }

        public string SourceLocation
        {
            get { return this._sourceLocation; }
            set { this._sourceLocation = value; this.OnPropertyChanged("SourceLocation"); }
        }

        public bool IsExpanded
        {
            get { return this._isExpanded; }
            set { this._isExpanded = value; this.OnPropertyChanged("IsExpanded"); }
        }

        public bool IsSelectedForBatch
        {
            get { return this._isSelectedForBatch; }
            set
            {
                if (this._isSelectedForBatch != value)
                {
                    this._isSelectedForBatch = value;
                    this.OnPropertyChanged("IsSelectedForBatch");
                    if (this._onSelectionChanged != null) this._onSelectionChanged();
                }
            }
        }

        public string CategoryDisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this._category)) return "Система";
                switch (this._category.ToLowerInvariant())
                {
                    case "security": return "Безопасность";
                    case "visibility": return "Проводник";
                    case "system": return "Система";
                    case "privacy": return "Приватность";
                    case "network": return "Сеть";
                    case "nvidia": return "NVIDIA";
                    case "steam": return "Steam";
                    case "peripheral": return "Периферия";
                    case "power": return "Питание";
                    case "cleanup": return "Очистка";
                    case "affinities": return "Аффинити";
                    case "policies": return "Политики";
                    case "misc": return "Дополнительно";
                    default: return this._category;
                }
            }
        }

        public bool NeedsReview
        {
            get { return this._model != null && this._model.needs_review; }
        }

        public bool IsSnapshotMode
        {
            get
            {
                if (this._model == null || this._model.registry == null) return false;
                for (int i = 0; i < this._model.registry.Count; i++)
                {
                    var r = this._model.registry[i];
                    if (r.revert == null || string.IsNullOrEmpty(r.revert.mode) || string.Equals(r.revert.mode, "snapshot", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool HasSnapshotRecord
        {
            get
            {
                if (this._engine != null && this._engine.StateManager != null && this._model != null)
                {
                    return this._engine.StateManager.HasSnapshot(this._model.id);
                }
                return false;
            }
        }

        public bool CanRevert
        {
            get
            {
                if (this._isBusy) return false;
                if (this.IsSnapshotMode && !this.HasSnapshotRecord)
                {
                    return false;
                }
                return true;
            }
        }

        public string RevertToolTip
        {
            get
            {
                if (this.IsSnapshotMode && !this.HasSnapshotRecord)
                {
                    return "Нет сохранённого исходного значения";
                }
                return "Откатить твик к исходному состоянию";
            }
        }

        public string RegistryPath
        {
            get
            {
                if (this._model != null && this._model.registry != null && this._model.registry.Count > 0)
                {
                    RegistryActionItem act = this._model.registry[0];
                    return string.Format("{0}\\{1}", act.hive, act.path);
                }
                return "Нет данных";
            }
        }

        public string RegistryName
        {
            get
            {
                if (this._model != null && this._model.registry != null && this._model.registry.Count > 0)
                {
                    return this._model.registry[0].name;
                }
                return "Нет данных";
            }
        }

        public bool HasRegistryName
        {
            get
            {
                return !string.IsNullOrEmpty(this.RegistryName) && this.RegistryName != "Нет данных";
            }
        }

        public bool IsActionTweak
        {
            get
            {
                return (this._model != null && string.Equals(this._model.type, "action", StringComparison.OrdinalIgnoreCase))
                    || string.Equals(this._category, "cleanup", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsToggleTweak
        {
            get { return !this.IsActionTweak; }
        }

        public string ActionButtonText
        {
            get
            {
                return (this._status == TweakStatus.Enabled) ? "Применить" : "Применить";
            }
        }

        public string DetailsActionButtonText
        {
            get
            {
                if (this.IsActionTweak)
                {
                    return "Применить";
                }
                return "Откатить";
            }
        }

        public bool IsRecommended
        {
            get
            {
                if (this._model != null && this._model.presets != null)
                {
                    for (int i = 0; i < this._model.presets.Count; i++)
                    {
                        if (string.Equals(this._model.presets[i], "recommended", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public string RegistryApplyText
        {
            get
            {
                if (this.IsActionTweak)
                {
                    return "Выполнение очистки системы";
                }
                if (this._model != null && this._model.registry != null && this._model.registry.Count > 0)
                {
                    RegistryActionItem act = this._model.registry[0];
                    string block = !string.IsNullOrEmpty(act.apply_block) ? act.apply_block : "Включено";
                    return string.Format("{0} (значение: {1})", block, act.apply);
                }
                return "Нет данных";
            }
        }

        public string RegistryRevertText
        {
            get
            {
                if (this.IsActionTweak)
                {
                    return "Разовая операция (без отката)";
                }
                if (this._model != null && this._model.registry != null && this._model.registry.Count > 0)
                {
                    RegistryActionItem act = this._model.registry[0];
                    if (act.revert != null)
                    {
                        if (act.revert.mode == "set")
                        {
                            return string.Format("Фиксированное (значение: {0})", act.revert.value);
                        }
                        if (act.revert.mode == "delete")
                        {
                            return "Удаление параметра";
                        }
                        return "Восстановление из снимка системы";
                    }
                }
                return "Восстановление из снимка";
            }
        }

        public string LiveSystemValueText
        {
            get
            {
                if (this.IsActionTweak)
                {
                    return (this._status == TweakStatus.Enabled) ? "Очищено" : "Требуется запуск";
                }

                // Специальное отображение реального статуса автозапуска Steam
                if (string.Equals(this._id, "steam-disable-autorun", StringComparison.OrdinalIgnoreCase))
                {
                    bool isDis = SteamConfigHelper.IsSteamAutoRunDisabled();
                    return isDis ? "1 (Автозапуск выключен)" : "0 (Автозапуск включен)";
                }

                // Специальное отображение статуса внутриигрового оверлея Steam (реестр или localconfig.vdf)
                if (string.Equals(this._id, "steam-disable-game-overlay", StringComparison.OrdinalIgnoreCase))
                {
                    if (this._engine != null && this._engine.Store != null && this._engine.Store.ValueExists("HKCU", @"Software\Valve\Steam", "EnableGameOverlay"))
                    {
                        object val = this._engine.Store.GetValue("HKCU", @"Software\Valve\Steam", "EnableGameOverlay");
                        if (val != null) return val.ToString();
                    }
                    int? vdfVal = SteamConfigHelper.GetGameOverlaySetting();
                    if (vdfVal.HasValue)
                    {
                        return vdfVal.Value == 0 ? "0 (Выключен в Steam)" : "1 (Включен в Steam)";
                    }
                    return "Не задано (по умолчанию Steam)";
                }

                if (this._engine != null && this._model != null)
                {
                    return SpecialTweakExecutor.GetLiveDisplayValue(this._model, this._engine.Store, this._engine.IsRealMode);
                }
                return (this._status == TweakStatus.Enabled) ? "1 (Активно)" : "0 (Выключено)";
            }
        }

        public string RecommendedValueText
        {
            get
            {
                if (this.IsActionTweak) return "Разовое действие";
                if (this._model != null && this._model.registry != null && this._model.registry.Count > 0)
                {
                    var act = this._model.registry[0];
                    return act.apply != null ? act.apply.ToString() : "1";
                }
                return "1";
            }
        }

        public string OptimizationStatusText
        {
            get
            {
                if (this.IsActionTweak)
                {
                    return (this._status == TweakStatus.Enabled) ? "Очищено" : "Готово к запуску";
                }
                switch (this._status)
                {
                    case TweakStatus.Enabled: return "Оптимально";
                    case TweakStatus.Disabled: return "Требуется оптимизация";
                    case TweakStatus.PartiallyEnabled: return "Частично";
                    case TweakStatus.Unavailable: return "Не поддерживается";
                    case TweakStatus.Executing: return "Применение...";
                    default: return "Неизвестно";
                }
            }
        }

        public string OptimizationStatusColor
        {
            get
            {
                if (this._status == TweakStatus.Enabled) return "#22C55E";
                if (this._status == TweakStatus.PartiallyEnabled) return "#F59E0B";
                if (this._status == TweakStatus.Unavailable) return "#64748B";
                return "#3B82F6";
            }
        }

        public string RebootRequirementText
        {
            get
            {
                if (this._requiresReboot) return "Требуется перезагрузка ПК";
                if (this.Category == "visibility" || this.Category == "appearance") return "Мгновенно (или перезапуск Проводника)";
                return "Не требуется (мгновенно)";
            }
        }

        public string CompatibilityText
        {
            get
            {
                if (this._model != null && this._model.min_build.HasValue)
                {
                    if (this._model.min_build.Value >= 22000)
                    {
                        return string.Format("Windows 11 (сборка {0}+)", this._model.min_build.Value);
                    }
                    return string.Format("Windows 10/11 (сборка {0}+)", this._model.min_build.Value);
                }
                return "Windows 10 / Windows 11 (любая сборка)";
            }
        }

        public string RollbackCapabilityText
        {
            get
            {
                if (this.IsActionTweak) return "Разовая операция (очистка временных файлов)";
                if (this.HasSnapshotRecord) return "Снимок сохранен • Мгновенный откат доступен";
                return "Поддерживается откат к значениям по умолчанию";
            }
        }

        public TweakRisk Risk
        {
            get { return this._risk; }
            set
            {
                this._risk = value;
                this.OnPropertyChanged("Risk");
                this.OnPropertyChanged("RiskText");
                this.OnPropertyChanged("RiskColor");
                this.OnPropertyChanged("ShowRiskBadge");
            }
        }

        public bool ShowRiskBadge
        {
            get { return this._risk == TweakRisk.Medium || this._risk == TweakRisk.High; }
        }

        public string RiskText
        {
            get
            {
                switch (this._risk)
                {
                    case TweakRisk.High: return "Высокий риск";
                    case TweakRisk.Medium: return "Средний риск";
                    default: return "Безопасно";
                }
            }
        }

        public string RiskColor
        {
            get
            {
                switch (this._risk)
                {
                    case TweakRisk.High: return "#EF4444";
                    case TweakRisk.Medium: return "#F59E0B";
                    default: return "#22C55E";
                }
            }
        }

        public bool RequiresReboot
        {
            get { return this._requiresReboot; }
            set { this._requiresReboot = value; this.OnPropertyChanged("RequiresReboot"); }
        }

        public TweakStatus Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                this._isChecked = (value == TweakStatus.Enabled);
                this._isBusy = (value == TweakStatus.Executing);
                this.OnPropertyChanged("Status");
                this.OnPropertyChanged("IsChecked");
                this.OnPropertyChanged("IsBusy");
                this.OnPropertyChanged("StatusBadgeText");
                this.OnPropertyChanged("StatusBadgeColor");
            }
        }

        public bool IsChecked
        {
            get { return this._isChecked; }
            set
            {
                if (this._isChecked != value && !this._isBusy)
                {
                    this.ExecuteToggleAsync(value);
                }
            }
        }

        public bool IsBusy
        {
            get { return this._isBusy; }
            set { this._isBusy = value; this.OnPropertyChanged("IsBusy"); }
        }

        public string StatusMessage
        {
            get { return this._statusMessage; }
            set { this._statusMessage = value; this.OnPropertyChanged("StatusMessage"); }
        }

        public string StatusBadgeText
        {
            get
            {
                if (this.IsActionTweak)
                {
                    switch (this._status)
                    {
                        case TweakStatus.Enabled: return "Очищено";
                        case TweakStatus.Disabled: return "Ожидает очистки";
                        case TweakStatus.Executing: return "Очистка...";
                        default: return "Ожидает очистки";
                    }
                }

                switch (this._status)
                {
                    case TweakStatus.Enabled: return "Включено";
                    case TweakStatus.Disabled: return "Выключено";
                    case TweakStatus.Executing: return "Применение...";
                    case TweakStatus.PartiallyEnabled: return "Частично";
                    default: return "Неизвестно";
                }
            }
        }

        public string StatusBadgeColor
        {
            get
            {
                switch (this._status)
                {
                    case TweakStatus.Enabled: return "#22C55E";
                    case TweakStatus.Disabled: return "#8B8BA3";
                    case TweakStatus.Executing: return "#F59E0B";
                    default: return "#8B8BA3";
                }
            }
        }

        public ICommand ToggleCommand { get; private set; }
        public ICommand ToggleExpandCommand { get; private set; }
        public ICommand OpenDetailsCommand { get; private set; }
        public ICommand RevertTweakCommand { get; private set; }
        public ICommand ApplyTweakCommand { get; private set; }
        public ICommand ExecuteActionCommand { get; private set; }

        public TweakCardViewModel(TweakItem model, TweakEngine engine, Action onStateChanged, Action onSelectionChanged = null)
        {
            this._model = model;
            this._engine = engine;
            this._onStateChanged = onStateChanged;
            this._onSelectionChanged = onSelectionChanged;
            this._isExpanded = false;

            if (model != null)
            {
                this._id = model.id;
                this._name = model.name_ru;
                this._category = model.category;
                this._group = string.IsNullOrEmpty(model.group) ? "Основное" : model.group;
                this._description = model.description_ru != null ? model.description_ru.Trim() : "";

                if (model.source != null)
                {
                    this._sourceLocation = string.Format("{0}:{1}-{2}", model.source.file, model.source.line_from, model.source.line_to);
                }

                if (string.Equals(model.risk, "high", StringComparison.OrdinalIgnoreCase))
                {
                    this._risk = TweakRisk.High;
                }
                else if (string.Equals(model.risk, "medium", StringComparison.OrdinalIgnoreCase))
                {
                    this._risk = TweakRisk.Medium;
                }
                else
                {
                    this._risk = TweakRisk.Low;
                }
            }

            this.ToggleCommand = new RelayCommand(this.OnToggleClicked);
            this.ToggleExpandCommand = new RelayCommand(this.OnToggleExpand);
            this.OpenDetailsCommand = new RelayCommand(this.OnOpenDetails);
            this.RevertTweakCommand = new RelayCommand(this.OnRevertTweak);
            this.ApplyTweakCommand = new RelayCommand(this.OnApplyTweak);
            this.ExecuteActionCommand = new RelayCommand(this.OnExecuteAction);
            this.RefreshFromEngine(false);
        }

        private void OnOpenDetails(object obj)
        {
            WinTweaker.UI.AxisDialogWindow.ShowTweakDetails(this);
        }

        public void ToggleExpand()
        {
            this.IsExpanded = !this.IsExpanded;
        }

        private void OnToggleExpand(object obj)
        {
            this.ToggleExpand();
        }

        private void OnApplyTweak(object obj)
        {
            if (this._isBusy) return;
            if (!this._isChecked)
            {
                this.IsChecked = true;
            }
            else
            {
                this.ExecuteToggleAsync(true);
            }
        }

        private void OnRevertTweak(object obj)
        {
            if (this._isBusy) return;
            if (this._engine != null && this._model != null)
            {
                this.IsBusy = true;
                this.Status = TweakStatus.Executing;
                this.StatusMessage = "Откат к исходному состоянию...";

                try
                {
                    bool ok = this._engine.Revert(this._model);
                    if (ok)
                    {
                        this.RefreshFromEngine();
                        this.StatusMessage = "Успешно возвращено";
                    }
                    else
                    {
                        this.RefreshFromEngine();
                        this.StatusMessage = "Ошибка отката твика";
                    }
                }
                catch (Exception ex)
                {
                    this.RefreshFromEngine();
                    this.StatusMessage = "Исключение: " + ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }

                if (this._onStateChanged != null)
                {
                    this._onStateChanged();
                }
            }
        }

        public void RefreshFromEngine(bool notifyProperties = true)
        {
            TweakStatus resolved = TweakStatus.Disabled;
            if (this._engine != null && this._model != null)
            {
                resolved = this._engine.CheckStatus(this._model);
            }

            this.ApplyResolvedStatus(resolved, notifyProperties);
        }

        public void ApplyResolvedStatus(TweakStatus status, bool notifyProperties = true)
        {
            this._status = status;
            this._isChecked = (status == TweakStatus.Enabled);
            this._isBusy = (status == TweakStatus.Executing);
            this._statusMessage = (status == TweakStatus.Enabled)
                ? "Значение активно в системе"
                : (status == TweakStatus.Unavailable ? "Недоступно на данном ПК" : "Исходное значение в системе");

            if (!notifyProperties) return;

            this.OnPropertyChanged("Status");
            this.OnPropertyChanged("IsChecked");
            this.OnPropertyChanged("IsBusy");
            this.OnPropertyChanged("StatusMessage");
            this.OnPropertyChanged("StatusBadgeText");
            this.OnPropertyChanged("StatusBadgeColor");
            this.OnPropertyChanged("CanRevert");
            this.OnPropertyChanged("RevertToolTip");
            this.OnPropertyChanged("HasSnapshotRecord");
            this.OnPropertyChanged("LiveSystemValueText");
            this.OnPropertyChanged("RecommendedValueText");
            this.OnPropertyChanged("OptimizationStatusText");
            this.OnPropertyChanged("OptimizationStatusColor");
            this.OnPropertyChanged("RebootRequirementText");
            this.OnPropertyChanged("CompatibilityText");
            this.OnPropertyChanged("RollbackCapabilityText");
        }

        public void RefreshStatus()
        {
            TweakStatus status = (this._engine != null && this._model != null)
                ? this._engine.CheckStatus(this._model)
                : TweakStatus.Disabled;
            this.ApplyResolvedStatus(status, true);
        }

        private void OnToggleClicked(object obj)
        {
            if (this.IsBusy) return;
            this.IsChecked = !this.IsChecked;
        }

        private async void ExecuteToggleAsync(bool targetState)
        {
            if (this._isBusy) return;

            this.Status = TweakStatus.Executing;
            this.StatusMessage = targetState ? "Применяется твик..." : "Откат к значениям...";

            bool success = false;
            try
            {
                await Task.Delay(150); // Плавная задержка переключения

                if (this._engine != null && this._model != null)
                {
                    if (targetState)
                    {
                        success = this._engine.Apply(this._model);
                    }
                    else
                    {
                        success = this._engine.Revert(this._model);
                    }
                }
                else
                {
                    success = true;
                }

                if (success)
                {
                    this.Status = targetState ? TweakStatus.Enabled : TweakStatus.Disabled;
                    this.StatusMessage = targetState ? "Успешно применено" : "Успешно возвращено";
                }
                else
                {
                    this.Status = (!targetState) ? TweakStatus.Enabled : TweakStatus.Disabled;
                    this.StatusMessage = "Ошибка применения операции";
                }
            }
            catch (Exception ex)
            {
                this.Status = (!targetState) ? TweakStatus.Enabled : TweakStatus.Disabled;
                this.StatusMessage = "Исключение: " + ex.Message;
            }

            if (this._onStateChanged != null)
            {
                this._onStateChanged();
            }
        }

        private async void OnExecuteAction(object obj)
        {
            if (this._isBusy) return;
            this.IsBusy = true;
            this.Status = TweakStatus.Executing;
            this.StatusMessage = "Выполняется очистка...";

            try
            {
                bool ok = await Task.Run(() =>
                {
                    if (this._engine != null && this._model != null)
                    {
                        return this._engine.Apply(this._model);
                    }
                    return true;
                });

                if (ok)
                {
                    this.Status = TweakStatus.Enabled;
                    this.StatusMessage = "Очистка успешно выполнена";
                }
                else
                {
                    this.Status = TweakStatus.Disabled;
                    this.StatusMessage = "Ошибка выполнения очистки";
                }
            }
            catch (Exception ex)
            {
                this.Status = TweakStatus.Disabled;
                this.StatusMessage = "Исключение: " + ex.Message;
            }
            finally
            {
                this.IsBusy = false;
                this.OnPropertyChanged("ActionButtonText");
                this.OnPropertyChanged("StatusBadgeText");
                this.OnPropertyChanged("LiveSystemValueText");
                this.OnPropertyChanged("OptimizationStatusText");
                this.OnPropertyChanged("OptimizationStatusColor");
            }

            if (this._onStateChanged != null)
            {
                this._onStateChanged();
            }
        }
    }
}
