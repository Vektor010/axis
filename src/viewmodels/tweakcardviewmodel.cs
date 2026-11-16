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
        private int _sourceIndex;

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
                if (this._id != null && this._id.Contains("dns"))
                {
                    return (this._status == TweakStatus.Enabled) ? "Сбросить снова" : "Сбросить";
                }
                return (this._status == TweakStatus.Enabled) ? "Очистить снова" : "Очистить";
            }
        }

        public string DetailsActionButtonText
        {
            get
            {
                if (this.IsActionTweak)
                {
                    return "Выполнить очистку";
                }
                return "Откатить твик";
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
        public ICommand RevertTweakCommand { get; private set; }
        public ICommand ExecuteActionCommand { get; private set; }

        public TweakCardViewModel(TweakItem model, TweakEngine engine, Action onStateChanged)
        {
            this._model = model;
            this._engine = engine;
            this._onStateChanged = onStateChanged;
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
            this.RevertTweakCommand = new RelayCommand(this.OnRevertTweak);
            this.ExecuteActionCommand = new RelayCommand(this.OnExecuteAction);
            this.RefreshFromEngine();
        }

        public void ToggleExpand()
        {
            this.IsExpanded = !this.IsExpanded;
        }

        private void OnToggleExpand(object obj)
        {
            this.ToggleExpand();
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

        public void RefreshFromEngine()
        {
            if (this._engine != null && this._model != null)
            {
                bool applied = this._engine.Check(this._model);
                this.Status = applied ? TweakStatus.Enabled : TweakStatus.Disabled;
                this.StatusMessage = applied ? "Значение активно в системе" : "Исходное значение в системе";
            }
            else
            {
                this.Status = TweakStatus.Disabled;
                this.StatusMessage = "";
            }
            this.OnPropertyChanged("CanRevert");
            this.OnPropertyChanged("RevertToolTip");
            this.OnPropertyChanged("HasSnapshotRecord");
        }

        public void RefreshStatus()
        {
            if (this._engine != null && this._model != null)
            {
                TweakStatus status = this._engine.CheckStatus(this._model);
                this.Status = status;
                this._isChecked = (status == TweakStatus.Enabled);
                this.OnPropertyChanged("IsChecked");
                this.OnPropertyChanged("Status");
            }
            this.OnPropertyChanged("CanRevert");
            this.OnPropertyChanged("RevertToolTip");
            this.OnPropertyChanged("HasSnapshotRecord");
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

        [System.Runtime.InteropServices.DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        private static extern int DnsFlushResolverCache();

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        private async void OnExecuteAction(object obj)
        {
            if (this._isBusy) return;
            this.IsBusy = true;
            this.Status = TweakStatus.Executing;
            this.StatusMessage = "Выполняется очистка...";

            try
            {
                await Task.Delay(120);

                this.PerformActualCleanup();

                bool ok = false;
                if (this._engine != null && this._model != null)
                {
                    ok = this._engine.Apply(this._model);
                }
                else
                {
                    ok = true;
                }

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
            }

            if (this._onStateChanged != null)
            {
                this._onStateChanged();
            }
        }

        private void PerformActualCleanup()
        {
            if (this._id == null) return;

            try
            {
                if (this._id.Contains("dns-cache"))
                {
                    try { DnsFlushResolverCache(); } catch { }
                    try
                    {
                        var psi = new ProcessStartInfo("ipconfig", "/flushdns") { CreateNoWindow = true, UseShellExecute = false };
                        Process.Start(psi);
                    }
                    catch { }
                }
                else if (this._id.Contains("recycle-bin"))
                {
                    try { SHEmptyRecycleBin(IntPtr.Zero, null, 7); } catch { }
                }
                else if (this._id.Contains("temporary-files"))
                {
                    string tempPath = System.IO.Path.GetTempPath();
                    CleanDirectorySafe(tempPath);
                }
                else if (this._id.Contains("temporary-internet-files"))
                {
                    string inetCache = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Windows\\INetCache");
                    CleanDirectorySafe(inetCache);
                }
                else if (this._id.Contains("directx-shader-cache"))
                {
                    string d3d = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "D3DSCache");
                    CleanDirectorySafe(d3d);
                    string nvidia = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA\\DXCache");
                    CleanDirectorySafe(nvidia);
                }
                else if (this._id.Contains("bsod-dumps"))
                {
                    string minidump = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Minidump");
                    CleanDirectorySafe(minidump);
                }
            }
            catch { }
        }

        private static void CleanDirectorySafe(string dir)
        {
            try
            {
                if (!System.IO.Directory.Exists(dir)) return;
                var dirInfo = new System.IO.DirectoryInfo(dir);
                foreach (var file in dirInfo.GetFiles())
                {
                    try { file.Delete(); } catch { }
                }
                foreach (var sub in dirInfo.GetDirectories())
                {
                    try { sub.Delete(true); } catch { }
                }
            }
            catch { }
        }
    }
}
