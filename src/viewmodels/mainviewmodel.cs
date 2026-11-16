using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WinTweaker.Common;
using WinTweaker.Engine;
using WinTweaker.Models;

namespace WinTweaker.ViewModels
{
    public class NavigationItemViewModel : ObservableObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IconGlyph { get; set; }
        private int _count;
        private bool _isSelected;

        public int Count
        {
            get { return this._count; }
            set { this._count = value; this.OnPropertyChanged("Count"); this.OnPropertyChanged("HasCount"); }
        }

        public bool HasCount
        {
            get { return this._count > 0; }
        }

        public bool IsSelected
        {
            get { return this._isSelected; }
            set { this._isSelected = value; this.OnPropertyChanged("IsSelected"); }
        }

        public bool IsTweaksItem
        {
            get { return string.Equals(this.Id, "tweaks", StringComparison.OrdinalIgnoreCase); }
        }

        public NavigationItemViewModel(string id, string name, string iconGlyph, int count = 0)
        {
            this.Id = id;
            this.Name = name;
            this.IconGlyph = iconGlyph;
            this._count = count;
            this._isSelected = false;
        }
    }

    public class HistoryItemViewModel : ObservableObject
    {
        public string TweakId { get; set; }
        public string AppliedAt { get; set; }
        public string Details { get; set; }

        public HistoryItemViewModel(string tweakId, string appliedAt, string details)
        {
            this.TweakId = tweakId;
            this.AppliedAt = appliedAt;
            this.Details = details;
        }
    }

    public class SortOptionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SortOptionItem(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class MainViewModel : ObservableObject
    {
        private readonly List<TweakCardViewModel> _allTweaks;
        private readonly ObservableCollection<TweakCardViewModel> _filteredTweaks;
        private readonly ICollectionView _filteredTweaksView;
        private readonly ObservableCollection<CategoryItemViewModel> _categories;
        private readonly ObservableCollection<NavigationItemViewModel> _navItems;
        private readonly ObservableCollection<JournalEntry> _journalEntries;
        private readonly ObservableCollection<JournalEntry> _recentJournalEntries;
        private readonly ObservableCollection<HistoryItemViewModel> _historyItems;
        private readonly ObservableCollection<SortOptionItem> _sortOptions;

        private NavigationItemViewModel _selectedNavItem;
        private CategoryItemViewModel _selectedCategory;
        private SortOptionItem _selectedSortOption;
        private bool _isGrouped;
        private bool _isTweaksCategoriesExpanded;
        private double _chartWidth;
        private string _searchQuery;
        private int _filterSegment; // 0=All, 1=Enabled, 2=Disabled, 3=NeedsReview
        private bool _isDarkTheme;
        private string _summaryText;
        private string _searchSummaryText;
        private string _statusNotification;
        private bool _isRealMode;

        private double _sidebarWidth;
        private bool _isSidebarExpanded;

        private TweakEngine _engine;
        private StateManager _stateManager;
        private JournalManager _journalManager;
        private SettingsManager _settingsManager;
        private RestorePointManager _restorePointManager;
        private SystemInfoService _systemInfoService;
        private GameModeWnfService _gameModeWnfService;
        private GameModeState _gameModeState;

        private DispatcherTimer _liveTimer;
        private readonly List<double> _cpuHistory;
        private readonly List<double> _ramHistory;
        private readonly List<double> _pingHistory;

        // Metric Card properties
        private string _cpuDisplay;
        private string _cpuStatus;
        private PointCollection _cpuSparkline;

        private string _ramDisplay;
        private string _ramStatus;
        private PointCollection _ramSparkline;

        private string _pingDisplay;
        private string _pingStatus;
        private PointCollection _pingSparkline;

        private string _tempDisplay;
        private string _tempStatus;
        private PointCollection _tempSparkline;

        private string _tweaksDisplay;
        private string _tweaksStatus;
        private PointCollection _tweaksSparkline;

        // Big Chart properties
        private string _chartTitle;
        private PointCollection _cpuChartPoints;
        private PointCollection _ramChartPoints;

        // System Specs properties
        private string _cpuSpec;
        private string _motherboardSpec;
        private string _gpuSpec;
        private string _ramSpec;
        private string _disksSpec;
        private string _osSpec;

        // Status Pills
        private string _systemRestoreStatus;
        private string _lastRestorePointDisplay;

        // Settings
        private string _pingCustomHost;
        private string _settingsSaveNotification;

        public ObservableCollection<TweakCardViewModel> FilteredTweaks
        {
            get { return this._filteredTweaks; }
        }

        public ICollectionView FilteredTweaksView
        {
            get { return this._filteredTweaksView; }
        }

        public ObservableCollection<CategoryItemViewModel> Categories
        {
            get { return this._categories; }
        }

        public ObservableCollection<NavigationItemViewModel> NavItems
        {
            get { return this._navItems; }
        }

        public ObservableCollection<JournalEntry> JournalEntries
        {
            get { return this._journalEntries; }
        }

        public ObservableCollection<JournalEntry> RecentJournalEntries
        {
            get { return this._recentJournalEntries; }
        }

        public ObservableCollection<HistoryItemViewModel> HistoryItems
        {
            get { return this._historyItems; }
        }

        public NavigationItemViewModel SelectedNavItem
        {
            get { return this._selectedNavItem; }
            set
            {
                if (this._selectedNavItem != value)
                {
                    if (this._selectedNavItem != null) this._selectedNavItem.IsSelected = false;
                    this._selectedNavItem = value;
                    if (this._selectedNavItem != null) this._selectedNavItem.IsSelected = true;
                    this.OnPropertyChanged("SelectedNavItem");
                    this.OnPropertyChanged("IsOverviewSelected");
                    this.OnPropertyChanged("IsTweaksSelected");
                    this.OnPropertyChanged("IsJournalSelected");
                    this.OnPropertyChanged("IsHistorySelected");
                    this.OnPropertyChanged("IsSettingsSelected");
                    this.OnPropertyChanged("IsAboutSelected");
                    this.OnPropertyChanged("IsSubcategoriesVisible");

                    this.HandleNavigationChanged();
                }
            }
        }

        public bool IsOverviewSelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == "overview"; } }
        public bool IsTweaksSelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == "tweaks"; } }
        public bool IsJournalSelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == "journal"; } }
        public bool IsHistorySelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == "history"; } }
        public bool IsSettingsSelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == "settings"; } }
        public bool IsAboutSelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == "about"; } }

        public CategoryItemViewModel SelectedCategory
        {
            get { return this._selectedCategory; }
            set
            {
                if (this._selectedCategory != value)
                {
                    if (this._selectedCategory != null) this._selectedCategory.IsSelected = false;
                    this._selectedCategory = value;
                    if (this._selectedCategory != null) this._selectedCategory.IsSelected = true;
                    this.OnPropertyChanged("SelectedCategory");
                    this.ApplyFilter();
                    this.UpdateSummary();
                }
            }
        }

        public double SidebarWidth
        {
            get { return this._sidebarWidth; }
            set { this._sidebarWidth = value; this.OnPropertyChanged("SidebarWidth"); }
        }

        public bool IsSidebarExpanded
        {
            get { return this._isSidebarExpanded; }
            set
            {
                this._isSidebarExpanded = value;
                this.SidebarWidth = value ? 230 : 64;
                this.OnPropertyChanged("IsSidebarExpanded");
            }
        }

        public string SearchQuery
        {
            get { return this._searchQuery; }
            set
            {
                this._searchQuery = value;
                this.OnPropertyChanged("SearchQuery");
                this.OnPropertyChanged("HasSearchQuery");
                if (!string.IsNullOrEmpty(value))
                {
                    if (this._selectedNavItem == null || this._selectedNavItem.Id != "tweaks")
                    {
                        for (int n = 0; n < this._navItems.Count; n++)
                        {
                            if (this._navItems[n].Id == "tweaks")
                            {
                                this.SelectedNavItem = this._navItems[n];
                                break;
                            }
                        }
                    }

                    // При поиске сбрасываем категорию на 'Все твики', чтобы искать по всей базе
                    if (this._selectedCategory != null && this._selectedCategory.Id != "all" && this._categories.Count > 0)
                    {
                        this._selectedCategory.IsSelected = false;
                        this._selectedCategory = this._categories[0];
                        this._selectedCategory.IsSelected = true;
                        this.OnPropertyChanged("SelectedCategory");
                    }
                }
                this.ApplyFilter();
                this.OnPropertyChanged("HasNoSearchResults");
            }
        }

        public bool HasNoSearchResults
        {
            get { return this._filteredTweaks.Count == 0 && !string.IsNullOrEmpty(this._searchQuery); }
        }

        public bool HasSearchQuery
        {
            get { return !string.IsNullOrEmpty(this._searchQuery); }
        }

        public int FilterSegment
        {
            get { return this._filterSegment; }
            set
            {
                this._filterSegment = value;
                this.OnPropertyChanged("FilterSegment");
                this.OnPropertyChanged("IsFilterAll");
                this.OnPropertyChanged("IsFilterEnabled");
                this.OnPropertyChanged("IsFilterDisabled");
                this.OnPropertyChanged("IsFilterNeedsReview");
                this.ApplyFilter();
            }
        }

        public bool IsFilterAll
        {
            get { return this._filterSegment == 0; }
            set { if (value) this.FilterSegment = 0; }
        }

        public bool IsFilterEnabled
        {
            get { return this._filterSegment == 1; }
            set { if (value) this.FilterSegment = 1; }
        }

        public bool IsFilterDisabled
        {
            get { return this._filterSegment == 2; }
            set { if (value) this.FilterSegment = 2; }
        }

        public bool IsFilterNeedsReview
        {
            get { return this._filterSegment == 3; }
            set { if (value) this.FilterSegment = 3; }
        }

        public ObservableCollection<SortOptionItem> SortOptions
        {
            get { return this._sortOptions; }
        }

        public SortOptionItem SelectedSortOption
        {
            get { return this._selectedSortOption; }
            set
            {
                if (this._selectedSortOption != value)
                {
                    this._selectedSortOption = value;
                    this.OnPropertyChanged("SelectedSortOption");
                    if (this._settingsManager != null && value != null)
                    {
                        this._settingsManager.Settings.SortMode = value.Id;
                        this._settingsManager.Save();
                    }
                    this.ApplyFilter();
                }
            }
        }

        public bool IsGrouped
        {
            get { return this._isGrouped; }
            set
            {
                if (this._isGrouped != value)
                {
                    this._isGrouped = value;
                    this.OnPropertyChanged("IsGrouped");
                    if (this._settingsManager != null)
                    {
                        this._settingsManager.Settings.IsGrouped = value;
                        this._settingsManager.Save();
                    }
                    this.ApplyGrouping();
                }
            }
        }

        public bool IsTweaksCategoriesExpanded
        {
            get { return this._isTweaksCategoriesExpanded; }
            set
            {
                if (this._isTweaksCategoriesExpanded != value)
                {
                    this._isTweaksCategoriesExpanded = value;
                    this.OnPropertyChanged("IsTweaksCategoriesExpanded");
                    this.OnPropertyChanged("TweaksChevronGlyph");
                    this.OnPropertyChanged("IsSubcategoriesVisible");
                }
            }
        }

        public string TweaksChevronGlyph
        {
            get { return this._isTweaksCategoriesExpanded ? "\uE70E" : "\uE70D"; }
        }

        public bool IsSubcategoriesVisible
        {
            get { return this.IsTweaksSelected && this._isTweaksCategoriesExpanded; }
        }

        public bool HasAtLeast5Points
        {
            get { return this._cpuHistory != null && this._cpuHistory.Count >= 5; }
        }

        public bool IsClearTypeRendering
        {
            get { return this._settingsManager == null || this._settingsManager.Settings.TextRenderingMode != "Grayscale"; }
            set
            {
                if (value && this._settingsManager != null)
                {
                    this._settingsManager.Settings.TextRenderingMode = "ClearType";
                    this._settingsManager.Save();
                    this.OnPropertyChanged("IsClearTypeRendering");
                    this.OnPropertyChanged("IsGrayscaleRendering");
                    this.ApplyTextRenderingMode();
                }
            }
        }

        public bool IsGrayscaleRendering
        {
            get { return this._settingsManager != null && this._settingsManager.Settings.TextRenderingMode == "Grayscale"; }
            set
            {
                if (value && this._settingsManager != null)
                {
                    this._settingsManager.Settings.TextRenderingMode = "Grayscale";
                    this._settingsManager.Save();
                    this.OnPropertyChanged("IsClearTypeRendering");
                    this.OnPropertyChanged("IsGrayscaleRendering");
                    this.ApplyTextRenderingMode();
                }
            }
        }

        public Brush SystemRestoreStatusBrush
        {
            get
            {
                if (string.Equals(this._systemRestoreStatus, "Включена", StringComparison.OrdinalIgnoreCase))
                {
                    return Application.Current != null && Application.Current.Resources.Contains("SuccessBrush")
                        ? (Brush)Application.Current.Resources["SuccessBrush"]
                        : Brushes.Green;
                }
                if (string.Equals(this._systemRestoreStatus, "Отключена", StringComparison.OrdinalIgnoreCase))
                {
                    return Application.Current != null && Application.Current.Resources.Contains("WarningBrush")
                        ? (Brush)Application.Current.Resources["WarningBrush"]
                        : Brushes.Orange;
                }
                return Application.Current != null && Application.Current.Resources.Contains("TextSecondary")
                    ? (Brush)Application.Current.Resources["TextSecondary"]
                    : Brushes.Gray;
            }
        }

        public ICommand ToggleTweaksCategoriesCommand { get; private set; }

        public int TotalTweaksCount { get; private set; }
        public int EnabledTweaksCount { get; private set; }
        public int DisabledTweaksCount { get; private set; }
        public int NeedsReviewCount { get; private set; }

        public string SegmentAllText { get; private set; }
        public string SegmentEnabledText { get; private set; }
        public string SegmentDisabledText { get; private set; }
        public string SegmentNeedsReviewText { get; private set; }
        public string TweaksTotalText { get; private set; }

        public bool CanApplyRecommended
        {
            get
            {
                if (this._allTweaks == null || this._allTweaks.Count == 0) return false;
                for (int i = 0; i < this._allTweaks.Count; i++)
                {
                    var m = this._allTweaks[i].Model;
                    if (m != null && m.presets != null && m.presets.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public string ApplyRecommendedToolTip
        {
            get
            {
                return this.CanApplyRecommended ? "Применить все рекомендуемые твики" : "Пресеты пока не заданы";
            }
        }

        public bool IsTempAvailable
        {
            get { return !string.IsNullOrEmpty(this._tempDisplay) && this._tempDisplay != "Нет данных"; }
        }

        private bool _isCollectingData = true;
        public bool IsCollectingData
        {
            get { return this._isCollectingData; }
            set { this._isCollectingData = value; this.OnPropertyChanged("IsCollectingData"); }
        }

        private List<string> _disksList = new List<string>();
        public List<string> DisksList
        {
            get { return this._disksList; }
            set { this._disksList = value; this.OnPropertyChanged("DisksList"); }
        }

        public bool HasNoRecentJournalActions
        {
            get { return this._recentJournalEntries == null || this._recentJournalEntries.Count == 0; }
        }

        public bool HasJournalEntries
        {
            get { return this._journalEntries != null && this._journalEntries.Count > 0; }
        }

        public bool HasHistoryItems
        {
            get { return this._historyItems != null && this._historyItems.Count > 0; }
        }

        public string SummaryText
        {
            get { return this._summaryText; }
            set { this._summaryText = value; this.OnPropertyChanged("SummaryText"); }
        }

        public string SearchSummaryText
        {
            get { return this._searchSummaryText; }
            set { this._searchSummaryText = value; this.OnPropertyChanged("SearchSummaryText"); }
        }

        public string StatusNotification
        {
            get { return this._statusNotification; }
            set { this._statusNotification = value; this.OnPropertyChanged("StatusNotification"); }
        }

        public bool IsRealMode
        {
            get { return this._isRealMode; }
        }

        public bool IsTestMode
        {
            get { return !this._isRealMode; }
        }

        public string ModePillText
        {
            get { return this._isRealMode ? "Реальный режим" : "Тестовый режим"; }
        }

        public string AdminPillText
        {
            get { return IsUserAdmin() ? "Права: администратор" : "Права: обычные"; }
        }

        public string BannerText
        {
            get { return "ТЕСТОВЫЙ РЕЖИМ: реестр не меняется. Для применения запустите с --real"; }
        }

        public bool IsDarkTheme
        {
            get { return this._isDarkTheme; }
        }

        public string ThemeButtonIcon
        {
            get { return this._isDarkTheme ? "\uE706" : "\uE708"; }
        }

        public string ThemeButtonText
        {
            get { return this._isDarkTheme ? "Светлая тема" : "Тёмная тема"; }
        }

        // Overview Metric Properties
        public string CpuDisplay
        {
            get { return this._cpuDisplay; }
            set { this._cpuDisplay = value; this.OnPropertyChanged("CpuDisplay"); }
        }

        public string CpuStatus
        {
            get { return this._cpuStatus; }
            set { this._cpuStatus = value; this.OnPropertyChanged("CpuStatus"); }
        }

        public PointCollection CpuSparkline
        {
            get { return this._cpuSparkline; }
            set { this._cpuSparkline = value; this.OnPropertyChanged("CpuSparkline"); }
        }

        public string RamDisplay
        {
            get { return this._ramDisplay; }
            set { this._ramDisplay = value; this.OnPropertyChanged("RamDisplay"); }
        }

        public string RamStatus
        {
            get { return this._ramStatus; }
            set { this._ramStatus = value; this.OnPropertyChanged("RamStatus"); }
        }

        public PointCollection RamSparkline
        {
            get { return this._ramSparkline; }
            set { this._ramSparkline = value; this.OnPropertyChanged("RamSparkline"); }
        }

        public string PingDisplay
        {
            get { return this._pingDisplay; }
            set { this._pingDisplay = value; this.OnPropertyChanged("PingDisplay"); }
        }

        public string PingStatus
        {
            get { return this._pingStatus; }
            set { this._pingStatus = value; this.OnPropertyChanged("PingStatus"); }
        }

        public PointCollection PingSparkline
        {
            get { return this._pingSparkline; }
            set { this._pingSparkline = value; this.OnPropertyChanged("PingSparkline"); }
        }

        public string TempDisplay
        {
            get { return this._tempDisplay; }
            set { this._tempDisplay = value; this.OnPropertyChanged("TempDisplay"); }
        }

        public string TempStatus
        {
            get { return this._tempStatus; }
            set { this._tempStatus = value; this.OnPropertyChanged("TempStatus"); }
        }

        public PointCollection TempSparkline
        {
            get { return this._tempSparkline; }
            set { this._tempSparkline = value; this.OnPropertyChanged("TempSparkline"); }
        }

        public string TweaksDisplay
        {
            get { return this._tweaksDisplay; }
            set { this._tweaksDisplay = value; this.OnPropertyChanged("TweaksDisplay"); }
        }

        public string TweaksStatus
        {
            get { return this._tweaksStatus; }
            set { this._tweaksStatus = value; this.OnPropertyChanged("TweaksStatus"); }
        }

        public PointCollection TweaksSparkline
        {
            get { return this._tweaksSparkline; }
            set { this._tweaksSparkline = value; this.OnPropertyChanged("TweaksSparkline"); }
        }

        // Big Chart Properties
        public string ChartTitle
        {
            get { return this._chartTitle; }
            set { this._chartTitle = value; this.OnPropertyChanged("ChartTitle"); }
        }

        public PointCollection CpuChartPoints
        {
            get { return this._cpuChartPoints; }
            set { this._cpuChartPoints = value; this.OnPropertyChanged("CpuChartPoints"); }
        }

        public PointCollection RamChartPoints
        {
            get { return this._ramChartPoints; }
            set { this._ramChartPoints = value; this.OnPropertyChanged("RamChartPoints"); }
        }

        // System Specs Properties
        public string CpuSpec
        {
            get { return this._cpuSpec; }
            set { this._cpuSpec = value; this.OnPropertyChanged("CpuSpec"); }
        }

        public string MotherboardSpec
        {
            get { return this._motherboardSpec; }
            set { this._motherboardSpec = value; this.OnPropertyChanged("MotherboardSpec"); }
        }

        public string GpuSpec
        {
            get { return this._gpuSpec; }
            set { this._gpuSpec = value; this.OnPropertyChanged("GpuSpec"); }
        }

        public string RamSpec
        {
            get { return this._ramSpec; }
            set { this._ramSpec = value; this.OnPropertyChanged("RamSpec"); }
        }

        public string DisksSpec
        {
            get { return this._disksSpec; }
            set { this._disksSpec = value; this.OnPropertyChanged("DisksSpec"); }
        }

        public string OsSpec
        {
            get { return this._osSpec; }
            set { this._osSpec = value; this.OnPropertyChanged("OsSpec"); }
        }

        // Status Pills Properties
        public string SystemRestoreStatus
        {
            get { return this._systemRestoreStatus; }
            set { this._systemRestoreStatus = value; this.OnPropertyChanged("SystemRestoreStatus"); }
        }

        public string LastRestorePointDisplay
        {
            get { return this._lastRestorePointDisplay; }
            set { this._lastRestorePointDisplay = value; this.OnPropertyChanged("LastRestorePointDisplay"); }
        }

        public string EngineModeDisplay
        {
            get { return this._isRealMode ? "Реальный режим" : "Тестовый режим"; }
        }

        public string JournalCountDisplay
        {
            get { return string.Format("{0} записей", this._journalEntries.Count); }
        }

        // Game Mode & WNF Properties (Noverse gm_effects)
        public GameModeState GameModeState
        {
            get { return this._gameModeState; }
            set { this._gameModeState = value; this.OnPropertyChanged("GameModeState"); }
        }

        public string GameModeStatusSummary
        {
            get { return this._gameModeState != null ? this._gameModeState.StatusSummary : "Game Mode: Ожидание"; }
        }

        public string GameModeActiveGameDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.ActiveGameProcessName : "Отсутствует"; }
        }

        public string GameModePowerProfileDisplay
        {
            get
            {
                if (this._gameModeState == null) return "—";
                if (this._gameModeState.PowerProfileLowValue == 3) return "3 (Активен)";
                if (this._gameModeState.PowerProfileLowValue == 1) return "1 (Неактивен)";
                return this._gameModeState.PowerProfileLowValue.ToString();
            }
        }

        public string GameModeCpuSetsDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.CpuSetIdsDisplay : "—"; }
        }

        public string GameModeForegroundDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.ForegroundProcessName : "—"; }
        }

        public bool IsGameModeRunning
        {
            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }
        }

        public bool HasRecentJournalActions
        {
            get { return this._recentJournalEntries.Count > 0; }
        }

        public int AttentionCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < this._allTweaks.Count; i++)
                {
                    if (this._allTweaks[i].NeedsReview || this._allTweaks[i].Risk == TweakRisk.High)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public string AttentionSummary
        {
            get { return string.Format("{0} твиков требуют внимания", this.AttentionCount); }
        }

        // Settings Properties
        public string PingCustomHost
        {
            get { return this._pingCustomHost; }
            set { this._pingCustomHost = value; this.OnPropertyChanged("PingCustomHost"); }
        }

        public string SettingsSaveNotification
        {
            get { return this._settingsSaveNotification; }
            set { this._settingsSaveNotification = value; this.OnPropertyChanged("SettingsSaveNotification"); }
        }

        // Commands
        public ICommand ToggleSidebarCommand { get; private set; }
        public ICommand ToggleThemeCommand { get; private set; }
        public ICommand ApplyAllRecommendedCommand { get; private set; }
        public ICommand RevertAllCommand { get; private set; }
        public ICommand RefreshAllCommand { get; private set; }
        public ICommand SelectCategoryCommand { get; private set; }
        public ICommand SelectNavCommand { get; private set; }
        public ICommand NavigateAttentionCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand ClearJournalCommand { get; private set; }
        public ICommand ClearSearchCommand { get; private set; }

        public event Action<bool> ThemeChanged;

        public MainViewModel(bool isRealMode = false, bool isDarkTheme = true)
        {
            this._isRealMode = isRealMode;
            this._isDarkTheme = isDarkTheme;
            this._sidebarWidth = 230;
            this._isSidebarExpanded = true;
            this._filterSegment = 0;
            this._searchQuery = "";
            this._summaryText = "";
            this._statusNotification = "";
            this._settingsSaveNotification = "";

            this._allTweaks = new List<TweakCardViewModel>();
            this._filteredTweaks = new ObservableCollection<TweakCardViewModel>();
            this._categories = new ObservableCollection<CategoryItemViewModel>();
            this._navItems = new ObservableCollection<NavigationItemViewModel>();
            this._journalEntries = new ObservableCollection<JournalEntry>();
            this._recentJournalEntries = new ObservableCollection<JournalEntry>();
            this._historyItems = new ObservableCollection<HistoryItemViewModel>();

            this._sortOptions = new ObservableCollection<SortOptionItem>();
            this._sortOptions.Add(new SortOptionItem(0, "По источнику"));
            this._sortOptions.Add(new SortOptionItem(1, "По названию А-Я"));
            this._sortOptions.Add(new SortOptionItem(2, "Сначала включённые"));
            this._sortOptions.Add(new SortOptionItem(3, "По риску"));
            this._selectedSortOption = this._sortOptions[0];

            this._isGrouped = true;
            this._isTweaksCategoriesExpanded = true;
            this._chartWidth = 480.0;

            this._cpuHistory = new List<double>();
            this._ramHistory = new List<double>();
            this._pingHistory = new List<double>();

            // CollectionViewSource for grouped list
            this._filteredTweaksView = CollectionViewSource.GetDefaultView(this._filteredTweaks);
            this._filteredTweaksView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

            this.ToggleSidebarCommand = new RelayCommand(this.OnToggleSidebar);
            this.ToggleThemeCommand = new RelayCommand(this.OnToggleTheme);
            this.ToggleTweaksCategoriesCommand = new RelayCommand(this.OnToggleTweaksCategories);
            this.ApplyAllRecommendedCommand = new RelayCommand(this.OnApplyAllRecommended);
            this.RevertAllCommand = new RelayCommand(this.OnRevertAll);
            this.RefreshAllCommand = new RelayCommand(this.OnRefreshAll);
            this.SelectCategoryCommand = new RelayCommand(this.OnSelectCategory);
            this.SelectNavCommand = new RelayCommand(this.OnSelectNav);
            this.NavigateAttentionCommand = new RelayCommand(this.OnNavigateAttention);
            this.SaveSettingsCommand = new RelayCommand(this.OnSaveSettings);
            this.ClearJournalCommand = new RelayCommand(this.OnClearJournal);
            this.ClearSearchCommand = new RelayCommand(this.OnClearSearch);

            this.InitializeServices();
            this.InitializeNavItems();
            this.LoadTweaksData();
            this.InitializeSystemSpecs();
            this.UpdateSummary();
            this.UpdateRecentJournal();
            this.RefreshHistory();

            // Setup live update timer
            this._liveTimer = new DispatcherTimer();
            this._liveTimer.Interval = TimeSpan.FromSeconds(2);
            this._liveTimer.Tick += this.OnLiveTimerTick;

            // Start on Overview
            this.SelectedNavItem = this._navItems[0];
        }

        private void OnToggleTweaksCategories(object parameter)
        {
            this.IsTweaksCategoriesExpanded = !this.IsTweaksCategoriesExpanded;
        }

        private static bool IsUserAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void InitializeServices()
        {
            string settingsPath = this._isRealMode ? null : Path.Combine(Path.GetTempPath(), "tweaker_settings.json");
            this._settingsManager = new SettingsManager(settingsPath, isTestMode: !this._isRealMode);
            this._pingCustomHost = this._settingsManager.Settings != null ? this._settingsManager.Settings.PingCustomHost : "";

            string logPath = Path.Combine(Path.GetTempPath(), "tweaker_session.log");
            this._journalManager = new JournalManager(logPath, isTestMode: !this._isRealMode);

            string statePath = Path.Combine(Path.GetTempPath(), "tweaker_state.json");
            this._stateManager = new StateManager(statePath, isTestMode: !this._isRealMode);

            IRegistryStore realStore = new RealRegistryStore();
            IRegistryStore store;
            if (this._isRealMode)
            {
                store = realStore;
            }
            else
            {
                store = new MemoryOverlayRegistryStore(realStore);
            }

            this._restorePointManager = new RestorePointManager(
                processRunner: null,
                clock: null,
                isRestoreAvailableFunc: null,
                isRealMode: this._isRealMode,
                registryStore: store,
                stateManager: this._stateManager);

            this._engine = new TweakEngine(store, this._stateManager, this._journalManager, this._isRealMode, this._restorePointManager);
            this._systemInfoService = new SystemInfoService();
            this._gameModeWnfService = new GameModeWnfService();
            this._gameModeState = new GameModeState();
            this._gameModeWnfService = new GameModeWnfService();
            this._gameModeState = new GameModeState();
            this._gameModeWnfService = new GameModeWnfService();
            this._gameModeState = new GameModeState();

            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                int sortId = this._settingsManager.Settings.SortMode;
                if (sortId >= 0 && sortId < this._sortOptions.Count)
                {
                    this._selectedSortOption = this._sortOptions[sortId];
                }
                this._isGrouped = this._settingsManager.Settings.IsGrouped;
            }
        }

        private void InitializeNavItems()
        {
            this._navItems.Clear();
            this._navItems.Add(new NavigationItemViewModel("overview", "Обзор", "\uE80F"));
            this._navItems.Add(new NavigationItemViewModel("tweaks", "Твики", "\uE74C", 0));
            this._navItems.Add(new NavigationItemViewModel("journal", "Журнал", "\uE7C3", this._journalEntries.Count));
            this._navItems.Add(new NavigationItemViewModel("history", "История", "\uE81C", this._historyItems.Count));
            this._navItems.Add(new NavigationItemViewModel("settings", "Настройки", "\uE713"));
            this._navItems.Add(new NavigationItemViewModel("about", "О программе", "\uE946"));
        }

        private void InitializeSystemSpecs()
        {
            this.CpuDisplay = "0%";
            this.CpuStatus = string.Format("{0} потоков", Environment.ProcessorCount);
            this.RamDisplay = "0 ГБ";
            this.RamStatus = "0 ГБ";
            this.PingDisplay = "Нет данных";
            this.PingStatus = "до шлюза";
            this.TempDisplay = "Нет данных";
            this.TempStatus = "Опрос датчика...";
            this.TweaksDisplay = "0";
            this.TweaksStatus = "";
            this.TweaksTotalText = "";
            this.ChartTitle = "Загрузка системы";
            this.IsCollectingData = true;

            // Быстрый синхронный опрос спецификаций, памяти и температуры GPU
            try
            {
                SystemSpecs specs = this._systemInfoService.GetOrCollectSpecs();
                this.CpuSpec = specs.Cpu;
                this.MotherboardSpec = specs.Motherboard;
                this.GpuSpec = specs.Gpu;
                this.RamSpec = specs.TotalRam;
                this.DisksSpec = specs.Disks;
                this.DisksList = specs.DisksList;
                this.OsSpec = specs.Os;

                // Память
                SystemInfoService.MEMORYSTATUSEX mem = new SystemInfoService.MEMORYSTATUSEX();
                mem.dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(SystemInfoService.MEMORYSTATUSEX));
                if (SystemInfoService.GlobalMemoryStatusEx(ref mem))
                {
                    double totalGb = (double)mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    double availGb = (double)mem.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    double usedGb = Math.Max(0.0, totalGb - availGb);
                    double ramPercent = (double)mem.dwMemoryLoad;
                    this.RamDisplay = string.Format("{0:F1} ГБ", usedGb);
                    this.RamStatus = string.Format("Занято {0:F0}% из {1:F1} ГБ", ramPercent, totalGb);

                    if (this._ramHistory.Count == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            this._ramHistory.Add(ramPercent);
                        }
                    }
                }

                // Температура GPU (NVIDIA NVML)
                try
                {
                    var nvml = new NvmlTemperatureSource();
                    var gTemp = nvml.GetGpuTemperature();
                    if (gTemp.IsAvailable)
                    {
                        this.TempDisplay = string.Format("{0}°C", gTemp.TemperatureC);
                        this.TempStatus = "GPU (NVIDIA)";
                    }
                    else
                    {
                        this.TempDisplay = "—";
                        this.TempStatus = "Датчик недоступен";
                    }
                }
                catch
                {
                    this.TempDisplay = "—";
                    this.TempStatus = "Датчик недоступен";
                }

                // Базовые начальные точки ЦП для отображения графика с первого момента
                if (this._cpuHistory.Count == 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        this._cpuHistory.Add(4.0 + (i % 3) * 2.0);
                    }
                    this.CpuDisplay = "5%";
                    this.CpuStatus = string.Format("{0} потоков", Environment.ProcessorCount);
                }

                this.ChartTitle = string.Format("ЦП: {0} | Память: {1}", this.CpuDisplay, this.RamDisplay);
                this.IsCollectingData = false;
            }
            catch { }

            this.CpuSparkline = this.CreateSparklinePoints(this._cpuHistory);
            this.RamSparkline = this.CreateSparklinePoints(this._ramHistory);
            this.PingSparkline = this.CreateSparklinePoints(this._pingHistory);
            this.TempSparkline = this.CreateFlatSparkline();
            this.TweaksSparkline = this.CreateFlatSparkline();
            this.CpuChartPoints = this.CreateChartPoints(this._cpuHistory);
            this.RamChartPoints = this.CreateChartPoints(this._ramHistory);

            this.UpdateRestorePointStatus();

            // Асинхронное фоновое обновление пинга и точных дифференциалов
            Task.Run(() =>
            {
                SystemMetrics m = this._systemInfoService.CollectLiveMetrics(this._pingCustomHost);
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.UpdateLiveMetricsUI(m);
                }));
            });
        }

        private void UpdateRestorePointStatus()
        {
            bool? sr = this._restorePointManager.IsSystemRestoreAvailable();
            if (sr.HasValue)
            {
                this.SystemRestoreStatus = sr.Value ? "Включена" : "Отключена";
            }
            else
            {
                this.SystemRestoreStatus = "Не определено";
            }
            this.OnPropertyChanged("SystemRestoreStatusBrush");

            DateTime? lastDt = this._restorePointManager.GetLastRestorePointUtc();
            if (lastDt.HasValue)
            {
                this.LastRestorePointDisplay = lastDt.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                this.LastRestorePointDisplay = "нет";
            }
        }

        private void HandleNavigationChanged()
        {
            if (this.IsOverviewSelected)
            {
                if (this._liveTimer != null && !this._liveTimer.IsEnabled)
                {
                    this._liveTimer.Start();
                }
                this.PollLiveMetrics();
            }
            else
            {
                if (this._liveTimer != null && this._liveTimer.IsEnabled)
                {
                    this._liveTimer.Stop();
                }
            }

            if (this.IsJournalSelected)
            {
                this.RefreshJournal();
            }
            if (this.IsHistorySelected)
            {
                this.RefreshHistory();
            }
        }

        private void OnLiveTimerTick(object sender, EventArgs e)
        {
            if (!this.IsOverviewSelected)
            {
                this._liveTimer.Stop();
                return;
            }
            this.PollLiveMetrics();
        }

        private void PollLiveMetrics()
        {
            string host = this._pingCustomHost;
            Task.Run(() =>
            {
                SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.UpdateLiveMetricsUI(m);
                }));
            });
        }

        private void UpdateLiveMetricsUI(SystemMetrics m)
        {
            // 1. CPU
            this._cpuHistory.Add(m.CpuLoadPercent);
            if (this._cpuHistory.Count > 60) this._cpuHistory.RemoveAt(0);
            this.CpuDisplay = string.Format("{0:F0}%", m.CpuLoadPercent);
            this.CpuStatus = string.Format("{0} потоков", Environment.ProcessorCount);
            this.CpuSparkline = this.CreateSparklinePoints(this._cpuHistory);

            // 2. RAM
            this._ramHistory.Add(m.RamLoadPercent);
            if (this._ramHistory.Count > 60) this._ramHistory.RemoveAt(0);
            this.RamDisplay = string.Format("{0:F1} ГБ", m.RamUsedGb);
            this.RamStatus = string.Format("Занято {0:F0}% из {1:F1} ГБ", m.RamLoadPercent, m.RamTotalGb);
            this.RamSparkline = this.CreateSparklinePoints(this._ramHistory);

            // 3. Ping
            double pingVal = m.PingMs >= 0 ? (double)m.PingMs : 0.0;
            this._pingHistory.Add(pingVal);
            if (this._pingHistory.Count > 60) this._pingHistory.RemoveAt(0);
            this.PingDisplay = m.PingDisplay;
            string target = !string.IsNullOrEmpty(this._pingCustomHost) ? this._pingCustomHost : "шлюза";
            this.PingStatus = m.PingMs >= 0 ? string.Format("до {0}", target) : "Нет ответа";
            this.PingSparkline = this.CreateSparklinePoints(this._pingHistory);

            // 4. Temp
            this.TempDisplay = m.TempDisplay;
            this.TempStatus = m.TempStatus;
            this.OnPropertyChanged("IsTempAvailable");

            // 5. Big Chart
            this.ChartTitle = string.Format("ЦП: {0:F1}% | Память: {1:F1} / {2:F1} ГБ ({3:F0}%)",
                m.CpuLoadPercent, m.RamUsedGb, m.RamTotalGb, m.RamLoadPercent);
            this.CpuChartPoints = this.CreateChartPoints(this._cpuHistory);
            this.RamChartPoints = this.CreateChartPoints(this._ramHistory);
            this.IsCollectingData = this._cpuHistory.Count < 5;
            this.OnPropertyChanged("HasAtLeast5Points");
            this.OnPropertyChanged("IsCollectingData");

            // 6. Update counts
            this.UpdateSummary();
            this.UpdateRecentJournal();
            this.OnPropertyChanged("AttentionCount");
            this.OnPropertyChanged("AttentionSummary");
            this.OnPropertyChanged("JournalCountDisplay");
        }

        private PointCollection CreateSparklinePoints(List<double> history)
        {
            PointCollection pts = new PointCollection();
            double w = 90.0;
            double h = 26.0;
            int count = history.Count;
            if (count < 5) return pts;

            for (int i = 0; i < count; i++)
            {
                double x = i * (w / (count - 1));
                double val = Math.Max(0.0, Math.Min(100.0, history[i]));
                double y = h - (val / 100.0 * h);
                pts.Add(new Point(x, y));
            }
            return pts;
        }

        private PointCollection CreateChartPoints(List<double> history)
        {
            PointCollection pts = new PointCollection();
            double w = this._chartWidth > 50 ? this._chartWidth : 480.0;
            double h = 130.0;
            int count = history.Count;
            if (count < 5) return pts;

            for (int i = 0; i < count; i++)
            {
                double x = i * (w / (count - 1));
                double val = Math.Max(0.0, Math.Min(100.0, history[i]));
                double y = h - (val / 100.0 * h);
                pts.Add(new Point(x, y));
            }
            return pts;
        }

        private PointCollection CreateFlatSparkline()
        {
            PointCollection pts = new PointCollection();
            pts.Add(new Point(0, 20));
            pts.Add(new Point(90, 20));
            return pts;
        }

        private void LoadTweaksData()
        {
            string json = "";
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resNames.Length; i++)
            {
                if (resNames[i].EndsWith("tweaks.json"))
                {
                    using (Stream stream = asm.GetManifestResourceStream(resNames[i]))
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        json = reader.ReadToEnd();
                    }
                    break;
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                string diskPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "tweaks.json");
                if (File.Exists(diskPath))
                {
                    json = File.ReadAllText(diskPath, Encoding.UTF8);
                }
            }

            if (!string.IsNullOrEmpty(json))
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                List<TweakItem> items = ser.Deserialize<List<TweakItem>>(json);
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        TweakCardViewModel vm = new TweakCardViewModel(items[i], this._engine, this.OnTweakStateChanged);
                        vm.SourceIndex = i;
                        this._allTweaks.Add(vm);
                    }
                }
            }

            this.TotalTweaksCount = this._allTweaks.Count;
            this.TweaksTotalText = string.Format("из {0}", this._allTweaks.Count);
            this.TweaksStatus = string.Format("из {0}", this._allTweaks.Count);
            this.OnPropertyChanged("TotalTweaksCount");
            this.OnPropertyChanged("TweaksTotalText");
            this.OnPropertyChanged("TweaksStatus");

            this.BuildCategories();
            this.ApplyFilter();
        }

        private void BuildCategories()
        {
            this._categories.Clear();

            // Total count
            int total = this._allTweaks.Count;
            CategoryItemViewModel allCat = new CategoryItemViewModel("all", "Все твики", "\uE71D", total);
            allCat.IsSelected = true;
            this._selectedCategory = allCat;
            this._categories.Add(allCat);

            // Group by category from data
            Dictionary<string, int> catCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                string c = this._allTweaks[i].Category;
                if (!string.IsNullOrEmpty(c))
                {
                    if (catCounts.ContainsKey(c)) catCounts[c]++;
                    else catCounts[c] = 1;
                }
            }

            // Полная таблица русских названий, иконок и логического порядка для всех категорий
            Dictionary<string, Tuple<string, string, int>> catMeta = new Dictionary<string, Tuple<string, string, int>>(StringComparer.OrdinalIgnoreCase)
            {
                { "security", Tuple.Create("Безопасность", "\uE72E", 1) },
                { "visibility", Tuple.Create("Проводник", "\uE8B7", 2) },
                { "system", Tuple.Create("Система", "\uE770", 3) },
                { "privacy", Tuple.Create("Конфиденциальность", "\uE727", 4) },
                { "network", Tuple.Create("Сеть и Интернет", "\uE968", 5) },
                { "nvidia", Tuple.Create("Графика NVIDIA", "\uE7F4", 6) },
                { "steam", Tuple.Create("Платформа Steam", "\uE7FC", 7) },
                { "peripheral", Tuple.Create("Периферия и ввод", "\uE964", 8) },
                { "power", Tuple.Create("Электропитание", "\uE7E8", 9) },
                { "cleanup", Tuple.Create("Очистка системы", "\uE74D", 10) },
                { "affinities", Tuple.Create("Аффинити ядер ЦП", "\uE950", 11) },
                { "policies", Tuple.Create("Политики", "\uE7C5", 12) },
                { "misc", Tuple.Create("Дополнительно", "\uE712", 13) }
            };

            List<KeyValuePair<string, int>> orderedCats = new List<KeyValuePair<string, int>>(catCounts);
            orderedCats.Sort((a, b) =>
            {
                int orderA = catMeta.ContainsKey(a.Key) ? catMeta[a.Key].Item3 : 99;
                int orderB = catMeta.ContainsKey(b.Key) ? catMeta[b.Key].Item3 : 99;
                return orderA.CompareTo(orderB);
            });

            foreach (KeyValuePair<string, int> kv in orderedCats)
            {
                if (kv.Value > 0)
                {
                    string displayName = kv.Key;
                    string glyph = "\uE74C";
                    if (catMeta.ContainsKey(kv.Key))
                    {
                        displayName = catMeta[kv.Key].Item1;
                        glyph = catMeta[kv.Key].Item2;
                    }

                    this._categories.Add(new CategoryItemViewModel(kv.Key, displayName, glyph, kv.Value));
                }
            }

            // Update Nav Item for Tweaks
            for (int n = 0; n < this._navItems.Count; n++)
            {
                if (this._navItems[n].Id == "tweaks")
                {
                    this._navItems[n].Count = total;
                    break;
                }
            }
        }

        public void ApplyFilter()
        {
            this._filteredTweaks.Clear();
            string query = (this._searchQuery != null) ? this._searchQuery.Trim().ToLowerInvariant() : "";
            string catId = (this._selectedCategory != null) ? this._selectedCategory.Id : "all";

            List<TweakCardViewModel> matching = new List<TweakCardViewModel>();

            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._allTweaks[i];

                if (catId != "all" && !string.Equals(tw.Category, catId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // FilterSegment: 0=All, 1=Enabled, 2=Disabled, 3=NeedsReview
                if (this._filterSegment == 1 && tw.Status != TweakStatus.Enabled)
                {
                    continue;
                }
                if (this._filterSegment == 2 && tw.Status != TweakStatus.Disabled)
                {
                    continue;
                }
                if (this._filterSegment == 3 && !tw.NeedsReview)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(query))
                {
                    bool matchName = tw.Name != null && tw.Name.ToLowerInvariant().Contains(query);
                    bool matchDesc = tw.Description != null && tw.Description.ToLowerInvariant().Contains(query);
                    bool matchId = tw.Id != null && tw.Id.ToLowerInvariant().Contains(query);

                    if (!matchName && !matchDesc && !matchId)
                    {
                        continue;
                    }
                }

                matching.Add(tw);
            }

            int sortMode = (this._selectedSortOption != null) ? this._selectedSortOption.Id : 0;
            if (sortMode == 1) // По названию А-Я
            {
                var culture = new System.Globalization.CultureInfo("ru-RU");
                matching.Sort(delegate(TweakCardViewModel a, TweakCardViewModel b)
                {
                    int cmp = string.Compare(a.Name, b.Name, true, culture);
                    return cmp != 0 ? cmp : a.SourceIndex.CompareTo(b.SourceIndex);
                });
            }
            else if (sortMode == 2) // Сначала включённые
            {
                matching.Sort(delegate(TweakCardViewModel a, TweakCardViewModel b)
                {
                    int aKey = (a.Status == TweakStatus.Enabled) ? 0 : 1;
                    int bKey = (b.Status == TweakStatus.Enabled) ? 0 : 1;
                    int cmp = aKey.CompareTo(bKey);
                    return cmp != 0 ? cmp : a.SourceIndex.CompareTo(b.SourceIndex);
                });
            }
            else if (sortMode == 3) // По риску
            {
                matching.Sort(delegate(TweakCardViewModel a, TweakCardViewModel b)
                {
                    // Risk: High(2) > Medium(1) > Low(0) -> High first
                    int aRisk = (int)a.Risk;
                    int bRisk = (int)b.Risk;
                    int cmp = bRisk.CompareTo(aRisk);
                    return cmp != 0 ? cmp : a.SourceIndex.CompareTo(b.SourceIndex);
                });
            }
            else // 0: По источнику
            {
                matching.Sort(delegate(TweakCardViewModel a, TweakCardViewModel b)
                {
                    return a.SourceIndex.CompareTo(b.SourceIndex);
                });
            }

            for (int i = 0; i < matching.Count; i++)
            {
                this._filteredTweaks.Add(matching[i]);
            }

            this.ApplyGrouping();
            if (this._filteredTweaksView != null)
            {
                this._filteredTweaksView.Refresh();
            }

            this.SearchSummaryText = string.Format("Найдено: {0} твиков", this._filteredTweaks.Count);
        }

        public void ApplyGrouping()
        {
            if (this._filteredTweaksView == null) return;
            this._filteredTweaksView.GroupDescriptions.Clear();
            if (this._isGrouped)
            {
                this._filteredTweaksView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            }
        }

        public bool IsGroupExpanded(string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) return true;
            if (this._settingsManager != null && this._settingsManager.Settings != null && this._settingsManager.Settings.GroupExpandedStates != null)
            {
                if (this._settingsManager.Settings.GroupExpandedStates.ContainsKey(groupName))
                {
                    return this._settingsManager.Settings.GroupExpandedStates[groupName];
                }
            }
            // По умолчанию все группы развернуты
            return true;
        }

        public void SetGroupExpanded(string groupName, bool isExpanded)
        {
            if (string.IsNullOrEmpty(groupName)) return;
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                if (this._settingsManager.Settings.GroupExpandedStates == null)
                {
                    this._settingsManager.Settings.GroupExpandedStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                }
                this._settingsManager.Settings.GroupExpandedStates[groupName] = isExpanded;
                this._settingsManager.Save();
            }
        }

        public void ApplyTextRenderingMode()
        {
            if (Application.Current == null || Application.Current.MainWindow == null) return;
            var mode = (this._settingsManager != null && this._settingsManager.Settings.TextRenderingMode == "Grayscale")
                ? System.Windows.Media.TextRenderingMode.Grayscale
                : System.Windows.Media.TextRenderingMode.ClearType;
            System.Windows.Media.TextOptions.SetTextRenderingMode(Application.Current.MainWindow, mode);
        }

        public void SetChartDimensions(double width, double height)
        {
            this._chartWidth = width;
            if (this._cpuHistory.Count >= 5)
            {
                this.CpuChartPoints = this.CreateChartPoints(this._cpuHistory);
                this.RamChartPoints = this.CreateChartPoints(this._ramHistory);
            }
        }

        public void Populate60SecondsHistory()
        {
            this._cpuHistory.Clear();
            this._ramHistory.Clear();
            this._pingHistory.Clear();

            double[] cpuSamples = new double[] {
                12, 14, 15, 18, 22, 28, 35, 42, 50, 48,
                45, 38, 32, 25, 20, 18, 16, 15, 14, 16,
                20, 24, 30, 36, 40, 38, 34, 28, 22, 18,
                16, 15, 14, 15, 18, 22, 26, 32, 38, 44,
                46, 42, 36, 30, 24, 20, 18, 16, 15, 14,
                15, 17, 20, 25, 30, 28, 24, 20, 16, 14
            };
            double[] ramSamples = new double[] {
                24, 24, 24, 25, 25, 26, 26, 26, 27, 27,
                27, 27, 28, 28, 28, 28, 28, 27, 27, 27,
                27, 28, 28, 29, 29, 29, 29, 28, 28, 28,
                28, 28, 28, 28, 29, 29, 30, 30, 30, 30,
                30, 29, 29, 29, 29, 28, 28, 28, 28, 28,
                28, 28, 29, 29, 29, 29, 28, 28, 28, 28
            };

            for (int i = 0; i < 60; i++)
            {
                this._cpuHistory.Add(cpuSamples[i]);
                this._ramHistory.Add(ramSamples[i]);
                this._pingHistory.Add(1.2 + (i % 3) * 0.4);
            }

            this.CpuChartPoints = this.CreateChartPoints(this._cpuHistory);
            this.RamChartPoints = this.CreateChartPoints(this._ramHistory);
            this.CpuSparkline = this.CreateSparklinePoints(this._cpuHistory);
            this.RamSparkline = this.CreateSparklinePoints(this._ramHistory);
            this.PingSparkline = this.CreateSparklinePoints(this._pingHistory);
            this.IsCollectingData = false;
            this.OnPropertyChanged("HasAtLeast5Points");
            this.OnPropertyChanged("IsCollectingData");
        }

        private void UpdateSummary()
        {
            int total = this._allTweaks.Count;
            int enabled = 0;
            int disabled = 0;
            int review = 0;

            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                if (this._allTweaks[i].Status == TweakStatus.Enabled)
                {
                    enabled++;
                }
                else if (this._allTweaks[i].Status == TweakStatus.Disabled)
                {
                    disabled++;
                }
                if (this._allTweaks[i].NeedsReview)
                {
                    review++;
                }
            }

            this.TotalTweaksCount = total;
            this.EnabledTweaksCount = enabled;
            this.DisabledTweaksCount = disabled;
            this.NeedsReviewCount = review;

            this.SegmentAllText = string.Format("Все ({0})", total);
            this.SegmentEnabledText = string.Format("Включено ({0})", enabled);
            this.SegmentDisabledText = string.Format("Выключено ({0})", disabled);
            this.SegmentNeedsReviewText = string.Format("Внимание ({0})", review);

            this.SummaryText = string.Format("Активно: {0} из {1}", enabled, total);
            this.TweaksDisplay = enabled.ToString();
            this.TweaksTotalText = string.Format("из {0}", total);
            this.TweaksStatus = string.Format("из {0}", total);

            this.OnPropertyChanged("CanApplyRecommended");
            this.OnPropertyChanged("ApplyRecommendedToolTip");
            this.OnPropertyChanged("TotalTweaksCount");
            this.OnPropertyChanged("EnabledTweaksCount");
            this.OnPropertyChanged("DisabledTweaksCount");
            this.OnPropertyChanged("NeedsReviewCount");
            this.OnPropertyChanged("SegmentAllText");
            this.OnPropertyChanged("SegmentEnabledText");
            this.OnPropertyChanged("SegmentDisabledText");
            this.OnPropertyChanged("SegmentNeedsReviewText");
            this.OnPropertyChanged("TweaksTotalText");
        }

        private void UpdateRecentJournal()
        {
            this._recentJournalEntries.Clear();
            List<JournalEntry> all = this._journalManager.Entries;
            int start = Math.Max(0, all.Count - 5);
            for (int i = all.Count - 1; i >= start; i--)
            {
                this._recentJournalEntries.Add(all[i]);
            }
            this.OnPropertyChanged("HasRecentJournalActions");
            this.OnPropertyChanged("HasNoRecentJournalActions");
        }

        public void RefreshJournal()
        {
            this._journalEntries.Clear();
            List<JournalEntry> all = this._journalManager.Entries;
            for (int i = all.Count - 1; i >= 0; i--)
            {
                this._journalEntries.Add(all[i]);
            }

            for (int n = 0; n < this._navItems.Count; n++)
            {
                if (this._navItems[n].Id == "journal")
                {
                    this._navItems[n].Count = this._journalEntries.Count;
                    break;
                }
            }
            this.OnPropertyChanged("JournalCountDisplay");
            this.OnPropertyChanged("HasJournalEntries");
        }

        public void RefreshHistory()
        {
            this._historyItems.Clear();
            if (this._stateManager != null)
            {
                List<string> ids = this._stateManager.GetAppliedTweakIds();
                for (int i = 0; i < ids.Count; i++)
                {
                    TweakSnapshot snap = this._stateManager.GetSnapshot(ids[i]);
                    if (snap != null)
                    {
                        string details = string.Format("Параметров в снимке: {0}", snap.Values != null ? snap.Values.Count : 0);
                        this._historyItems.Add(new HistoryItemViewModel(snap.TweakId, snap.AppliedAt, details));
                    }
                }
            }

            for (int n = 0; n < this._navItems.Count; n++)
            {
                if (this._navItems[n].Id == "history")
                {
                    this._navItems[n].Count = this._historyItems.Count;
                    break;
                }
            }
            this.OnPropertyChanged("HasHistoryItems");
        }

        private void OnTweakStateChanged()
        {
            this.UpdateSummary();
            this.UpdateRecentJournal();
            this.RefreshJournal();
            this.RefreshHistory();
            this.UpdateRestorePointStatus();
        }

        private void OnToggleSidebar(object obj)
        {
            this.IsSidebarExpanded = !this.IsSidebarExpanded;
        }

        private void OnToggleTheme(object obj)
        {
            this._isDarkTheme = !this._isDarkTheme;
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.IsDarkTheme = this._isDarkTheme;
                this._settingsManager.Save();
            }

            if (this.ThemeChanged != null)
            {
                this.ThemeChanged(this._isDarkTheme);
            }
            this.OnPropertyChanged("ThemeButtonIcon");
            this.OnPropertyChanged("ThemeButtonText");
        }

        private void OnSelectCategory(object obj)
        {
            CategoryItemViewModel cat = obj as CategoryItemViewModel;
            if (cat != null)
            {
                this.SelectedCategory = cat;
                // Automatically switch to Tweaks screen
                for (int n = 0; n < this._navItems.Count; n++)
                {
                    if (this._navItems[n].Id == "tweaks")
                    {
                        this.SelectedNavItem = this._navItems[n];
                        break;
                    }
                }
            }
        }

        private void OnSelectNav(object obj)
        {
            NavigationItemViewModel nav = obj as NavigationItemViewModel;
            if (nav != null)
            {
                if (nav.IsTweaksItem)
                {
                    // Клик в любом месте кнопки «Твики» разворачивает/сворачивает категории
                    if (this.IsTweaksSelected)
                    {
                        this.IsTweaksCategoriesExpanded = !this.IsTweaksCategoriesExpanded;
                    }
                    else
                    {
                        this.IsTweaksCategoriesExpanded = true;
                    }
                }
                this.SelectedNavItem = nav;
            }
        }

        private void OnNavigateAttention(object obj)
        {
            // Switch to tweaks and filter by needs review
            for (int n = 0; n < this._navItems.Count; n++)
            {
                if (this._navItems[n].Id == "tweaks")
                {
                    this.SelectedNavItem = this._navItems[n];
                    break;
                }
            }
            this.FilterSegment = 3; // Требуют проверки
        }

        private void OnClearSearch(object obj)
        {
            this.SearchQuery = "";
        }

        private void OnApplyAllRecommended(object obj)
        {
            if (!this.CanApplyRecommended)
            {
                MessageBox.Show("Пресеты пока не заданы.", "Применение рекомендуемых", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "Применить все безопасные рекомендуемые твики?\n(Твики с низким риском будут активированы)",
                "Подтверждение применения",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            int appliedCount = 0;
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._allTweaks[i];
                if (tw.Risk == TweakRisk.Low && tw.Status != TweakStatus.Enabled)
                {
                    tw.IsChecked = true;
                    appliedCount++;
                }
            }
            this.StatusNotification = string.Format("Применено рекомендуемых твиков: {0}", appliedCount);
        }

        private void OnRevertAll(object obj)
        {
            MessageBoxResult confirm = MessageBox.Show(
                "Откатить все активные твики к исходному состоянию?",
                "Подтверждение отката",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            int revertedCount = 0;
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._allTweaks[i];
                if (tw.Status == TweakStatus.Enabled)
                {
                    tw.IsChecked = false;
                    revertedCount++;
                }
            }
            this.StatusNotification = string.Format("Откачено твиков: {0}", revertedCount);
        }

        private void OnRefreshAll(object obj)
        {
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                this._allTweaks[i].RefreshFromEngine();
            }
            this.UpdateSummary();
            this.StatusNotification = "Состояния твиков обновлены из системы.";
        }

        private void OnSaveSettings(object obj)
        {
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.PingCustomHost = this._pingCustomHost != null ? this._pingCustomHost.Trim() : "";
                this._settingsManager.Save();
                this.SettingsSaveNotification = "Настройки успешно сохранены.";
            }
        }

        private void OnClearJournal(object obj)
        {
            if (this._journalManager != null)
            {
                this._journalManager.Clear();
                this.RefreshJournal();
                this.UpdateRecentJournal();
                this.StatusNotification = "Журнал очищен.";
            }
        }
    }
}
