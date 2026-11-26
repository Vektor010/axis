using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

    public class MainViewModel : ObservableObject, IDisposable
    {
        private readonly List<TweakCardViewModel> _allTweaks;
        private readonly BatchObservableCollection<TweakCardViewModel> _filteredTweaks;
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
        private readonly System.Windows.Threading.Dispatcher _uiDispatcher;
        private int _isPollingMetrics;
        private volatile bool _isDisposed;
        private readonly List<double> _cpuHistory;
        private readonly List<double> _ramHistory;
        private readonly List<double> _gpuHistory;
        private readonly List<double> _pingHistory;
        private readonly List<double> _tempHistory;

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
        private const int ChartSlotCount = 120;
        private const double ChartPlotLeft = 34.0;
        private const double ChartPlotRightPad = 8.0;
        private const double ChartPlotTop = 8.0;
        private const double ChartPlotBottom = 96.0;
        private const double ChartUsableHeight = 88.0;

        private string _chartTitle;
        private bool _isChartAutoScale = true;
        private double _chartYMax = 16.0;
        private string _chartScaleButtonText = "Авто · 16%";
        private string _chartYLabel100 = "16%";
        private string _chartYLabel75 = "12%";
        private string _chartYLabel50 = "8%";
        private string _chartYLabel25 = "4%";
        private string _chartTimeStartLabel = "-60с";
        private string _chartTimeQuarterLabel = "-45с";
        private string _chartTimeMidLabel = "-30с";
        private string _chartTimeThreeQuarterLabel = "-15с";
        private string _cpuLegendText = "ЦП";
        private string _ramLegendText = "ОЗУ";
        private string _gpuLegendText = "GPU";
        private double _cpuDotLeft;
        private double _cpuDotTop;
        private double _ramDotLeft;
        private double _ramDotTop;
        private double _gpuDotLeft;
        private double _gpuDotTop;
        private bool _hasChartDots;
        private double _lastRamTotalGb;
        private PointCollection _cpuChartPoints;
        private PointCollection _ramChartPoints;
        private PointCollection _gpuChartPoints;
        private PointCollection _cpuChartAreaPoints;
        private PointCollection _ramChartAreaPoints;
        private PointCollection _gpuChartAreaPoints;

        // Extended Real Hardware Telemetry (CPU Boost/Temp, GPU Load/Clocks/Power, RAM, Disk, Net, Uptime, Processes)
        private string _cpuTempDisplay;
        private string _gpuLoadDisplay;
        private string _gpuDetailDisplay;
        private double _gpuLoadPercent;
        private string _ramPercentBadge;
        private string _diskActivityDisplay;
        private string _diskSpaceDisplay;
        private string _networkRateDisplay;
        private string _networkDetailDisplay;
        private string _uptimeDisplay;
        private string _processCountDisplay;
        private string _lastUpdatedDisplay;
        private string _lastDiagnosticSummary;
        private int _lastKnownJournalCount = -1;

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

        // Native Kernel Diagnostics (WDDM / MMCSS / WNF) & UI 2026 Helpers
        private string _hagsStatusText;
        private string _flipQueueStatusText;
        private string _mmcssRespText;
        private string _mmcssThrottlingText;
        private string _mmcssTasksText;
        private string _copySpecsFeedback;

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
                this.OnPropertyChanged("IsFilterRecommended");
                this.ApplyFilter();
                this.OnPropertyChanged("HasNoSearchResults");
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

        public bool IsFilterRecommended
        {
            get { return this._filterSegment == 4; }
            set { if (value) this.FilterSegment = 4; }
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
            get { return this._cpuHistory != null && this._cpuHistory.Count >= 2; }
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

        private string _restorePointNotification;
        public string RestorePointNotification
        {
            get { return this._restorePointNotification; }
            set { this._restorePointNotification = value; this.OnPropertyChanged("RestorePointNotification"); }
        }

        private bool _isCreatingRestorePoint;
        public bool IsCreatingRestorePoint
        {
            get { return this._isCreatingRestorePoint; }
            set { this._isCreatingRestorePoint = value; this.OnPropertyChanged("IsCreatingRestorePoint"); }
        }

        public string SnapshotsCountText
        {
            get
            {
                int count = this._stateManager != null ? this._stateManager.SnapshotCount : 0;
                return string.Format("{0} снимков параметров", count);
            }
        }

        public bool IsPolling1Sec
        {
            get { return this._settingsManager == null || this._settingsManager.Settings.PollingIntervalSeconds == 1 || this._settingsManager.Settings.PollingIntervalSeconds == 0; }
            set { if (value) this.SetPollingInterval(1); }
        }

        public bool IsPolling2Sec
        {
            get { return this._settingsManager != null && this._settingsManager.Settings.PollingIntervalSeconds == 2; }
            set { if (value) this.SetPollingInterval(2); }
        }

        public bool IsPolling5Sec
        {
            get { return this._settingsManager != null && this._settingsManager.Settings.PollingIntervalSeconds == 5; }
            set { if (value) this.SetPollingInterval(5); }
        }

        public bool IsPollingPaused
        {
            get { return this._settingsManager != null && this._settingsManager.Settings.PollingIntervalSeconds == -1; }
            set { if (value) this.SetPollingInterval(-1); }
        }

        public ICommand CreateRestorePointCommand { get; private set; }
        public ICommand OpenSystemProtectionCommand { get; private set; }
        public ICommand EnableSystemRestoreCommand { get; private set; }
        public ICommand ExportSnapshotsCommand { get; private set; }
        public ICommand CopyDiagnosticReportCommand { get; private set; }
        public ICommand SetPingPresetCommand { get; private set; }

        public ICommand ToggleTweaksCategoriesCommand { get; private set; }
        public ICommand ToggleChartScaleCommand { get; private set; }

        public int TotalTweaksCount { get; private set; }
        public int EnabledTweaksCount { get; private set; }
        public int DisabledTweaksCount { get; private set; }
        public int NeedsReviewCount { get; private set; }

        public string SegmentAllText { get; private set; }
        public string SegmentEnabledText { get; private set; }
        public string SegmentDisabledText { get; private set; }
        public string SegmentNeedsReviewText { get; private set; }
        public string SegmentRecommendedText { get; private set; }
        public string TweaksTotalText { get; private set; }
        public int RecommendedTotalCount { get; private set; }
        public int RecommendedAppliedCount { get; private set; }
        public int RecommendedRemainingCount { get; private set; }
        public int SelectedBatchCount { get; private set; }
        public int VisibleUnappliedCount { get; private set; }
        public int RevertableCount { get; private set; }
        public bool HasSelectedForBatch { get { return this.SelectedBatchCount > 0; } }
        public bool CanContextApply { get { return this.SelectedBatchCount > 0 || this.VisibleUnappliedCount > 0; } }
        public string RecommendedBatchButtonText
        {
            get
            {
                return this.RecommendedRemainingCount > 0
                    ? string.Format("Применить рекомендуемые ({0})", this.RecommendedRemainingCount)
                    : string.Format("Рекомендуемые ({0}/{0} ✓)", this.RecommendedTotalCount);
            }
        }
        public string ContextApplyButtonText
        {
            get
            {
                if (this.SelectedBatchCount > 0)
                    return string.Format("Применить выбранные ({0})", this.SelectedBatchCount);
                string catId = (this._selectedCategory != null) ? this._selectedCategory.Id : "all";
                if (string.Equals(catId, "all", StringComparison.OrdinalIgnoreCase))
                    return string.Format("Применить все ({0})", this.VisibleUnappliedCount);
                return string.Format("Применить раздел ({0})", this.VisibleUnappliedCount);
            }
        }
        public string ContextApplyToolTip
        {
            get
            {
                if (this.SelectedBatchCount > 0)
                    return "Применить отмеченные галочками твики с проверкой рисков и конфликтов";
                return "Применить все неприменённые переключатели в текущем открытом списке (с защитной проверкой рисков)";
            }
        }
        public string RevertAllButtonText
        {
            get { return this.RevertableCount > 0 ? string.Format("Откатить все ({0})", this.RevertableCount) : "Откатить все"; }
        }
        public string AppVersionDisplay { get { return "v3.4.0 • 2026"; } }
        public string DatabaseStatsDisplay { get { return string.Format("{0} твиков • 12 категорий", this.TotalTweaksCount); } }
        public string EngineSummaryFooterText { get { return string.Format("AXIS Engine v3.4 • {0} твиков, изолированные снимки state.json и прямой WinAPI без внешних зависимостей.", this.TotalTweaksCount); } }

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
            get { return this._isRealMode ? "Активный режим" : "Тестовый режим"; }
        }

        public string AdminPillText
        {
            get { return IsUserAdmin() ? "Права: администратор" : "Права: обычные"; }
        }

        public string BannerText
        {
            get { return ""; }
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

        public bool IsChartAutoScale
        {
            get { return this._isChartAutoScale; }
            set { this._isChartAutoScale = value; this.OnPropertyChanged("IsChartAutoScale"); }
        }

        public double ChartYMax
        {
            get { return this._chartYMax; }
            set { this._chartYMax = value; this.OnPropertyChanged("ChartYMax"); }
        }

        public string ChartScaleButtonText
        {
            get { return this._chartScaleButtonText; }
            set { this._chartScaleButtonText = value; this.OnPropertyChanged("ChartScaleButtonText"); }
        }

        public string ChartYLabel100
        {
            get { return this._chartYLabel100; }
            set { this._chartYLabel100 = value; this.OnPropertyChanged("ChartYLabel100"); }
        }

        public string ChartYLabel75
        {
            get { return this._chartYLabel75; }
            set { this._chartYLabel75 = value; this.OnPropertyChanged("ChartYLabel75"); }
        }

        public string ChartYLabel50
        {
            get { return this._chartYLabel50; }
            set { this._chartYLabel50 = value; this.OnPropertyChanged("ChartYLabel50"); }
        }

        public string ChartYLabel25
        {
            get { return this._chartYLabel25; }
            set { this._chartYLabel25 = value; this.OnPropertyChanged("ChartYLabel25"); }
        }

        public string ChartTimeStartLabel
        {
            get { return this._chartTimeStartLabel; }
            set { this._chartTimeStartLabel = value; this.OnPropertyChanged("ChartTimeStartLabel"); }
        }

        public string ChartTimeQuarterLabel
        {
            get { return this._chartTimeQuarterLabel; }
            set { this._chartTimeQuarterLabel = value; this.OnPropertyChanged("ChartTimeQuarterLabel"); }
        }

        public string ChartTimeMidLabel
        {
            get { return this._chartTimeMidLabel; }
            set { this._chartTimeMidLabel = value; this.OnPropertyChanged("ChartTimeMidLabel"); }
        }

        public string ChartTimeThreeQuarterLabel
        {
            get { return this._chartTimeThreeQuarterLabel; }
            set { this._chartTimeThreeQuarterLabel = value; this.OnPropertyChanged("ChartTimeThreeQuarterLabel"); }
        }

        public string CpuLegendText
        {
            get { return this._cpuLegendText; }
            set { this._cpuLegendText = value; this.OnPropertyChanged("CpuLegendText"); }
        }

        public string RamLegendText
        {
            get { return this._ramLegendText; }
            set { this._ramLegendText = value; this.OnPropertyChanged("RamLegendText"); }
        }

        public string GpuLegendText
        {
            get { return this._gpuLegendText; }
            set { this._gpuLegendText = value; this.OnPropertyChanged("GpuLegendText"); }
        }

        public double CpuDotLeft
        {
            get { return this._cpuDotLeft; }
            set { this._cpuDotLeft = value; this.OnPropertyChanged("CpuDotLeft"); }
        }

        public double CpuDotTop
        {
            get { return this._cpuDotTop; }
            set { this._cpuDotTop = value; this.OnPropertyChanged("CpuDotTop"); }
        }

        public double RamDotLeft
        {
            get { return this._ramDotLeft; }
            set { this._ramDotLeft = value; this.OnPropertyChanged("RamDotLeft"); }
        }

        public double RamDotTop
        {
            get { return this._ramDotTop; }
            set { this._ramDotTop = value; this.OnPropertyChanged("RamDotTop"); }
        }

        public double GpuDotLeft
        {
            get { return this._gpuDotLeft; }
            set { this._gpuDotLeft = value; this.OnPropertyChanged("GpuDotLeft"); }
        }

        public double GpuDotTop
        {
            get { return this._gpuDotTop; }
            set { this._gpuDotTop = value; this.OnPropertyChanged("GpuDotTop"); }
        }

        public bool HasChartDots
        {
            get { return this._hasChartDots; }
            set { this._hasChartDots = value; this.OnPropertyChanged("HasChartDots"); }
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

        public PointCollection GpuChartPoints
        {
            get { return this._gpuChartPoints; }
            set { this._gpuChartPoints = value; this.OnPropertyChanged("GpuChartPoints"); }
        }

        public PointCollection CpuChartAreaPoints
        {
            get { return this._cpuChartAreaPoints; }
            set { this._cpuChartAreaPoints = value; this.OnPropertyChanged("CpuChartAreaPoints"); }
        }

        public PointCollection RamChartAreaPoints
        {
            get { return this._ramChartAreaPoints; }
            set { this._ramChartAreaPoints = value; this.OnPropertyChanged("RamChartAreaPoints"); }
        }

        public PointCollection GpuChartAreaPoints
        {
            get { return this._gpuChartAreaPoints; }
            set { this._gpuChartAreaPoints = value; this.OnPropertyChanged("GpuChartAreaPoints"); }
        }

        public string CpuTempDisplay
        {
            get { return this._cpuTempDisplay; }
            set { this._cpuTempDisplay = value; this.OnPropertyChanged("CpuTempDisplay"); }
        }

        public string GpuLoadDisplay
        {
            get { return this._gpuLoadDisplay; }
            set { this._gpuLoadDisplay = value; this.OnPropertyChanged("GpuLoadDisplay"); }
        }

        public string GpuDetailDisplay
        {
            get { return this._gpuDetailDisplay; }
            set { this._gpuDetailDisplay = value; this.OnPropertyChanged("GpuDetailDisplay"); }
        }

        public double GpuLoadPercent
        {
            get { return this._gpuLoadPercent; }
            set { this._gpuLoadPercent = value; this.OnPropertyChanged("GpuLoadPercent"); }
        }

        public string RamPercentBadge
        {
            get { return this._ramPercentBadge; }
            set { this._ramPercentBadge = value; this.OnPropertyChanged("RamPercentBadge"); }
        }

        public string DiskActivityDisplay
        {
            get { return this._diskActivityDisplay; }
            set { this._diskActivityDisplay = value; this.OnPropertyChanged("DiskActivityDisplay"); }
        }

        public string DiskSpaceDisplay
        {
            get { return this._diskSpaceDisplay; }
            set { this._diskSpaceDisplay = value; this.OnPropertyChanged("DiskSpaceDisplay"); }
        }

        public string NetworkRateDisplay
        {
            get { return this._networkRateDisplay; }
            set { this._networkRateDisplay = value; this.OnPropertyChanged("NetworkRateDisplay"); }
        }

        public string NetworkDetailDisplay
        {
            get { return this._networkDetailDisplay; }
            set { this._networkDetailDisplay = value; this.OnPropertyChanged("NetworkDetailDisplay"); }
        }

        public string UptimeDisplay
        {
            get { return this._uptimeDisplay; }
            set { this._uptimeDisplay = value; this.OnPropertyChanged("UptimeDisplay"); }
        }

        public string ProcessCountDisplay
        {
            get { return this._processCountDisplay; }
            set { this._processCountDisplay = value; this.OnPropertyChanged("ProcessCountDisplay"); }
        }

        public string LastUpdatedDisplay
        {
            get { return this._lastUpdatedDisplay; }
            set { this._lastUpdatedDisplay = value; this.OnPropertyChanged("LastUpdatedDisplay"); }
        }

        public string LastDiagnosticSummary
        {
            get { return this._lastDiagnosticSummary; }
            set { this._lastDiagnosticSummary = value; this.OnPropertyChanged("LastDiagnosticSummary"); }
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
            get { return this._isRealMode ? "Активен" : "Тестовый режим"; }
        }

        public string JournalCountDisplay
        {
            get
            {
                int cnt = this._journalManager != null ? this._journalManager.Entries.Count : this._journalEntries.Count;
                return string.Format("{0} записей", cnt);
            }
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

        public string GameModeShortStatus
        {
            get
            {
                string raw = this.GameModeStatusSummary;
                if (string.IsNullOrEmpty(raw)) return "Ожидание";
                if (raw.StartsWith("Game Mode: ", StringComparison.OrdinalIgnoreCase))
                {
                    return raw.Substring("Game Mode: ".Length);
                }
                return raw;
            }
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

        // Native Kernel Diagnostics & Progress (2026 Bento Overview)
        public string HagsStatusText
        {
            get { return this._hagsStatusText; }
            set { this._hagsStatusText = value; this.OnPropertyChanged("HagsStatusText"); }
        }

        public string FlipQueueStatusText
        {
            get { return this._flipQueueStatusText; }
            set { this._flipQueueStatusText = value; this.OnPropertyChanged("FlipQueueStatusText"); }
        }

        public string MmcssRespText
        {
            get { return this._mmcssRespText; }
            set { this._mmcssRespText = value; this.OnPropertyChanged("MmcssRespText"); }
        }

        public string MmcssThrottlingText
        {
            get { return this._mmcssThrottlingText; }
            set { this._mmcssThrottlingText = value; this.OnPropertyChanged("MmcssThrottlingText"); }
        }

        public string MmcssTasksText
        {
            get { return this._mmcssTasksText; }
            set { this._mmcssTasksText = value; this.OnPropertyChanged("MmcssTasksText"); }
        }

        public string CopySpecsFeedback
        {
            get { return this._copySpecsFeedback; }
            set { this._copySpecsFeedback = value; this.OnPropertyChanged("CopySpecsFeedback"); }
        }

        public double OptimizationPercent
        {
            get
            {
                if (this.TotalTweaksCount <= 0) return 0.0;
                return Math.Round((100.0 * this.EnabledTweaksCount) / this.TotalTweaksCount, 1);
            }
        }

        public string OptimizationPercentText
        {
            get
            {
                return string.Format("{0:F0}%", this.OptimizationPercent);
            }
        }

        // Commands
        public ICommand ToggleSidebarCommand { get; private set; }
        public ICommand ToggleThemeCommand { get; private set; }
        public ICommand ApplySelectedCommand { get; private set; }
        public ICommand ApplyAllRecommendedCommand { get; private set; }
        public ICommand RevertAllCommand { get; private set; }
        public ICommand SelectAllVisibleCommand { get; private set; }
        public ICommand ClearSelectionCommand { get; private set; }
        public ICommand ShowRecommendedCommand { get; private set; }
        public ICommand RefreshAllCommand { get; private set; }
        public ICommand SelectCategoryCommand { get; private set; }
        public ICommand SelectNavCommand { get; private set; }
        public ICommand NavigateAttentionCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand ClearJournalCommand { get; private set; }
        public ICommand ClearSearchCommand { get; private set; }
        public ICommand CopySpecsCommand { get; private set; }
        public ICommand OpenUrlCommand { get; private set; }

        public event Action<bool> ThemeChanged;

        public MainViewModel(bool isRealMode = true, bool isDarkTheme = true)
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
            this._filteredTweaks = new BatchObservableCollection<TweakCardViewModel>();
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
            this._uiDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            this._chartWidth = 480.0;

            this._cpuHistory = new List<double>();
            this._ramHistory = new List<double>();
            this._gpuHistory = new List<double>();
            this._pingHistory = new List<double>();
            this._tempHistory = new List<double>();

            this._cpuTempDisplay = "";
            this._gpuLoadDisplay = "0%";
            this._gpuDetailDisplay = "Опрос видеокарты...";
            this._ramPercentBadge = "0%";
            this._diskActivityDisplay = "Опрос...";
            this._diskSpaceDisplay = "—";
            this._networkRateDisplay = "↓ 0 КБ/с  ↑ 0 КБ/с";
            this._networkDetailDisplay = "Активный канал";
            this._uptimeDisplay = "—";
            this._processCountDisplay = "—";
            this._lastUpdatedDisplay = "Ожидание...";
            this._lastDiagnosticSummary = "Оценка конфигурации...";

            // CollectionViewSource for grouped list
            this._filteredTweaksView = CollectionViewSource.GetDefaultView(this._filteredTweaks);
            this._filteredTweaksView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

            this.ToggleSidebarCommand = new RelayCommand(this.OnToggleSidebar);
            this.ToggleThemeCommand = new RelayCommand(this.OnToggleTheme);
            this.ToggleTweaksCategoriesCommand = new RelayCommand(this.OnToggleTweaksCategories);
            this.ToggleChartScaleCommand = new RelayCommand(this.OnToggleChartScale);
            this.ApplySelectedCommand = new RelayCommand(this.OnApplySelected);
            this.ApplyAllRecommendedCommand = new RelayCommand(this.OnApplyAllRecommended);
            this.RevertAllCommand = new RelayCommand(this.OnRevertAll);
            this.SelectAllVisibleCommand = new RelayCommand(this.OnSelectAllVisible);
            this.ClearSelectionCommand = new RelayCommand(this.OnClearSelection);
            this.ShowRecommendedCommand = new RelayCommand(this.OnShowRecommended);
            this.RefreshAllCommand = new RelayCommand(this.OnRefreshAll);
            this.SelectCategoryCommand = new RelayCommand(this.OnSelectCategory);
            this.SelectNavCommand = new RelayCommand(this.OnSelectNav);
            this.NavigateAttentionCommand = new RelayCommand(this.OnNavigateAttention);
            this.SaveSettingsCommand = new RelayCommand(this.OnSaveSettings);
            this.ClearJournalCommand = new RelayCommand(this.OnClearJournal);
            this.ClearSearchCommand = new RelayCommand(this.OnClearSearch);
            this.CopySpecsCommand = new RelayCommand(this.OnCopySpecs);
            this.OpenUrlCommand = new RelayCommand(this.OnOpenUrl);
            this.CreateRestorePointCommand = new RelayCommand(this.OnCreateRestorePoint);
            this.OpenSystemProtectionCommand = new RelayCommand(this.OnOpenSystemProtection);
            this.EnableSystemRestoreCommand = new RelayCommand(this.OnEnableSystemRestore);
            this.ExportSnapshotsCommand = new RelayCommand(this.OnExportSnapshots);
            this.CopyDiagnosticReportCommand = new RelayCommand(this.OnCopyDiagnosticReport);
            this.SetPingPresetCommand = new RelayCommand(this.OnSetPingPreset);

            this.InitializeServices();
            this.InitializeNavItems();
            this.LoadTweaksData();
            this.InitializeSystemSpecs();
            this.UpdateSummary();
            this.UpdateRecentJournal();
            this.RefreshHistory();

            // Setup live update timer (default Live 500ms high-frequency telemetry; 2s / 5s / Pause optional)
            this._liveTimer = new DispatcherTimer();
            int pollSec = (this._settingsManager != null && this._settingsManager.Settings != null) ? this._settingsManager.Settings.PollingIntervalSeconds : 1;
            if (pollSec == 0) pollSec = 1;
            if (pollSec > 0)
            {
                this._liveTimer.Interval = (pollSec == 1) ? TimeSpan.FromMilliseconds(500) : TimeSpan.FromSeconds(pollSec);
                this._liveTimer.Tick += this.OnLiveTimerTick;
                this._liveTimer.Start();
            }
            else
            {
                this._liveTimer.Interval = TimeSpan.FromMilliseconds(500);
                this._liveTimer.Tick += this.OnLiveTimerTick;
            }

            // Start on Overview
            this.SelectedNavItem = this._navItems[0];
        }

        private void OnToggleTweaksCategories(object parameter)
        {
            this.IsTweaksCategoriesExpanded = !this.IsTweaksCategoriesExpanded;
        }

        private void OnToggleChartScale(object parameter)
        {
            this.IsChartAutoScale = !this._isChartAutoScale;
            this.RefreshBigChart();
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

            string logPath = this._isRealMode ? null : Path.Combine(Path.GetTempPath(), "tweaker_session.log");
            this._journalManager = new JournalManager(logPath, isTestMode: !this._isRealMode);

            string statePath = this._isRealMode ? null : Path.Combine(Path.GetTempPath(), "tweaker_state.json");
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
            int jCount = this._journalManager != null ? this._journalManager.Entries.Count : this._journalEntries.Count;
            this._navItems.Clear();
            this._navItems.Add(new NavigationItemViewModel("overview", "Обзор", "\uE80F"));
            this._navItems.Add(new NavigationItemViewModel("tweaks", "Твики", "\uE74C", 0));
            this._navItems.Add(new NavigationItemViewModel("journal", "Журнал", "\uE7C3", jCount));
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
            this.PingDisplay = "—";
            this.PingStatus = "Измерение...";
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

                if (this._isRealMode)
                {
                    SystemMetrics instant = this._systemInfoService.CollectInstantStartupSnapshot(this._pingCustomHost);
                    this.UpdateLiveMetricsUI(instant);
                }
                else
                {
                    SystemMetrics initMetrics = this._systemInfoService.CollectLiveMetrics(this._pingCustomHost);
                    this.UpdateLiveMetricsUI(initMetrics);
                }
            }
            catch { }

            this.UpdateRestorePointStatus();
            this.RefreshNativeKernelDiagnostics();
            if (this._isRealMode)
            {
                this.PollLiveMetrics();
            }
        }

        private void RefreshNativeKernelDiagnostics()
        {
            this.HagsStatusText = "Нет данных";
            this.FlipQueueStatusText = "Нет данных";
            this.MmcssRespText = "—";
            this.MmcssThrottlingText = "—";
            this.MmcssTasksText = "—";

            try
            {
                GpuHwschService hwsch = new GpuHwschService();
                GpuHwschState gpuState = hwsch.QueryPrimaryGpuState();
                if (gpuState != null)
                {
                    this.HagsStatusText = string.IsNullOrEmpty(gpuState.HagsStatusDisplay) ? "—" : gpuState.HagsStatusDisplay;
                    this.FlipQueueStatusText = string.IsNullOrEmpty(gpuState.FlipQueueStatusDisplay) ? "—" : gpuState.FlipQueueStatusDisplay;
                }
            }
            catch { }

            try
            {
                MmcssTaskService mmcss = new MmcssTaskService();
                MmcssTaskState mState = mmcss.QueryState();
                if (mState != null)
                {
                    string rawResp = string.IsNullOrEmpty(mState.SystemResponsivenessDisplay) ? "—" : mState.SystemResponsivenessDisplay;
                    int parenIdx = rawResp.IndexOf('(');
                    if (parenIdx > 0)
                    {
                        rawResp = rawResp.Substring(0, parenIdx).Trim();
                    }
                    this.MmcssRespText = "Resp " + rawResp;
                    this.MmcssThrottlingText = string.IsNullOrEmpty(mState.NetworkThrottlingDisplay) ? "—" : mState.NetworkThrottlingDisplay;
                    this.MmcssTasksText = string.Format("{0} задач", mState.AvailableTasksCount);
                }
            }
            catch { }

            try
            {
                if (this._gameModeWnfService != null)
                {
                    this.GameModeState = this._gameModeWnfService.QueryState();
                    this.OnPropertyChanged("GameModeStatusSummary");
                    this.OnPropertyChanged("GameModeShortStatus");
                    this.OnPropertyChanged("GameModeActiveGameDisplay");
                    this.OnPropertyChanged("GameModePowerProfileDisplay");
                    this.OnPropertyChanged("IsGameModeRunning");
                }
            }
            catch { }
        }

        private void OnCopySpecs(object parameter)
        {
            try
            {
                string summary = string.Format(
                    "AXIS 2.0 — Конфигурация системы\r\n" +
                    "Процессор: {0}\r\n" +
                    "Мат. плата: {1}\r\n" +
                    "Видеокарта: {2}\r\n" +
                    "Память: {3}\r\n" +
                    "Диски: {4}\r\n" +
                    "ОС: {5}\r\n" +
                    "HAGS: {6} | Flip Queue: {7}\r\n" +
                    "MMCSS Resp: {8} | NDIS Throttling: {9}\r\n" +
                    "Активных твиков: {10} из {11} ({12})",
                    this.CpuSpec, this.MotherboardSpec, this.GpuSpec, this.RamSpec,
                    this.DisksSpec, this.OsSpec, this.HagsStatusText, this.FlipQueueStatusText,
                    this.MmcssRespText, this.MmcssThrottlingText,
                    this.EnabledTweaksCount, this.TotalTweaksCount, this.OptimizationPercentText);
                Clipboard.SetText(summary);
                this.CopySpecsFeedback = "Скопировано!";
            }
            catch
            {
                this.CopySpecsFeedback = "Ошибка буфера";
            }
        }

        private void OnOpenUrl(object parameter)
        {
            string url = parameter as string;
            if (string.IsNullOrEmpty(url)) return;
            if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
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
            if (this._isDisposed || this._systemInfoService == null) return;
            if (System.Threading.Interlocked.CompareExchange(ref this._isPollingMetrics, 1, 0) != 0)
            {
                return;
            }

            string host = this._pingCustomHost;
            var disp = this._uiDispatcher ?? (Application.Current != null ? Application.Current.Dispatcher : null);
            Task.Run(() =>
            {
                try
                {
                    if (this._isDisposed) return;
                    SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);
                    if (this._isDisposed || disp == null) return;
                    disp.BeginInvoke(new Action(() =>
                    {
                        if (!this._isDisposed)
                        {
                            this.UpdateLiveMetricsUI(m);
                        }
                    }));
                }
                catch { }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref this._isPollingMetrics, 0);
                }
            });
        }

        private void UpdateLiveMetricsUI(SystemMetrics m)
        {
            if (m == null) return;

            // 1. CPU (Utility + Boost GHz + Smart Temp if available)
            this._cpuHistory.Add(m.CpuLoadPercent);
            if (this._cpuHistory.Count > ChartSlotCount) this._cpuHistory.RemoveAt(0);
            this.CpuDisplay = (m.CpuLoadPercent < 10.0)
                ? string.Format("{0:F1}%", m.CpuLoadPercent)
                : string.Format("{0:F0}%", m.CpuLoadPercent);
            this.CpuStatus = m.IsCpuTempAvailable
                ? string.Format("{0} • Т: {1} • {2} пот.", m.CpuClockDisplay, m.CpuTempDisplay, Environment.ProcessorCount)
                : string.Format("{0} • Boost {1:F0}% • {2} пот.", m.CpuClockDisplay, m.CpuPerfPercent, Environment.ProcessorCount);
            this.CpuSparkline = this.CreateSparklinePoints(this._cpuHistory, true, 16.0, 0.0);

            // 2. RAM
            this._ramHistory.Add(m.RamLoadPercent);
            if (this._ramHistory.Count > ChartSlotCount) this._ramHistory.RemoveAt(0);
            this._lastRamTotalGb = m.RamTotalGb;
            this.RamDisplay = string.Format("{0:F1} ГБ", m.RamUsedGb);
            this.RamPercentBadge = string.Format("{0:F0}%", m.RamLoadPercent);
            this.RamStatus = string.Format("Из {0:F1} ГБ • Свободно {1:F1} ГБ", m.RamTotalGb, m.RamFreeGb);
            this.RamSparkline = this.CreateSparklinePoints(this._ramHistory, false, 100.0, 0.0);

            // 3. Ping
            double pingVal = m.PingMs >= 0 ? (double)m.PingMs : 0.0;
            this._pingHistory.Add(pingVal);
            if (this._pingHistory.Count > ChartSlotCount) this._pingHistory.RemoveAt(0);
            this.PingDisplay = m.PingDisplay;
            string target = !string.IsNullOrEmpty(this._pingCustomHost) ? this._pingCustomHost : "шлюза";
            this.PingStatus = m.PingMs >= 0 ? string.Format("до {0}", target) : "Проверка связи...";
            this.PingSparkline = this.CreateSparklinePoints(this._pingHistory, true, 10.0, 0.0);

            // 4. GPU (Temp + Load + Clocks/Power/VRAM)
            this.TempDisplay = m.TempDisplay;
            this.TempStatus = m.IsGpuLoadAvailable
                ? string.Format("{0} • Нагрузка: {1}", m.TempStatus, m.GpuLoadDisplay)
                : m.TempStatus;
            this.GpuDetailDisplay = m.GpuDetailDisplay;
            double tempVal = 0.0;
            if (!string.IsNullOrEmpty(m.TempDisplay) && m.TempDisplay != "Нет данных")
            {
                string digits = m.TempDisplay.Replace("°C", "").Trim();
                double.TryParse(digits, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempVal);
            }
            this._tempHistory.Add(tempVal);
            if (this._tempHistory.Count > ChartSlotCount) this._tempHistory.RemoveAt(0);
            this.TempSparkline = this.CreateSparklinePoints(this._tempHistory, false, 90.0, 30.0);
            this.CpuTempDisplay = m.CpuTempDisplay;
            this.GpuLoadPercent = m.GpuLoadPercent;
            this.GpuLoadDisplay = m.GpuLoadDisplay;
            this._gpuHistory.Add(m.IsGpuLoadAvailable ? m.GpuLoadPercent : 0.0);
            if (this._gpuHistory.Count > ChartSlotCount) this._gpuHistory.RemoveAt(0);
            this.OnPropertyChanged("IsTempAvailable");

            // 5. Real Hardware Sensors Bar (Disk / Network / Uptime / Processes / Last Update)
            this.DiskActivityDisplay = m.DiskActivityDisplay;
            this.DiskSpaceDisplay = m.DiskSpaceDisplay;
            this.NetworkRateDisplay = m.NetworkRateDisplay;
            this.NetworkDetailDisplay = m.NetworkDetailDisplay;
            this.UptimeDisplay = m.UptimeDisplay;
            this.ProcessCountDisplay = m.ProcessCount > 0 ? m.ProcessCount.ToString() : "—";
            this.LastUpdatedDisplay = "Обновлено: " + m.LastUpdatedDisplay;

            // 6. Big Chart (Smooth Fixed-Slot Conveyor + Auto Y-Scale + Zero Clutter)
            this.ChartTitle = "";
            this.RefreshBigChart();
            this.IsCollectingData = this._cpuHistory.Count < 1;
            this.OnPropertyChanged("HasAtLeast5Points");
            this.OnPropertyChanged("IsCollectingData");

            // 7. Журнал обновляем ТОЛЬКО при изменении числа записей (без пересчета 888 твиков каждую секунду)
            int jCount = (this._journalManager != null && this._journalManager.Entries != null) ? this._journalManager.Entries.Count : 0;
            if (jCount != this._lastKnownJournalCount)
            {
                this._lastKnownJournalCount = jCount;
                this.UpdateRecentJournal();
                this.OnPropertyChanged("JournalCountDisplay");
            }
        }

        private static double GetListMax(List<double> values)
        {
            double maxVal = 0.0;
            if (values == null) return maxVal;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] > maxVal) maxVal = values[i];
            }
            return maxVal;
        }

        private double ComputeSmartChartYMax()
        {
            if (!this._isChartAutoScale) return 100.0;
            double peak = Math.Max(GetListMax(this._cpuHistory), Math.Max(GetListMax(this._ramHistory), GetListMax(this._gpuHistory)));
            double target = peak * 1.28;
            double[] steps = new double[] { 12.0, 16.0, 20.0, 40.0, 60.0, 80.0, 100.0 };
            for (int i = 0; i < steps.Length; i++)
            {
                if (target <= steps[i]) return steps[i];
            }
            return 100.0;
        }

        private double GetSampleStepSeconds()
        {
            if (!this._isRealMode) return 1.0;
            int pollSec = (this._settingsManager != null && this._settingsManager.Settings != null) ? this._settingsManager.Settings.PollingIntervalSeconds : 1;
            if (pollSec == 0 || pollSec == 1) return 0.5;
            if (pollSec > 1) return (double)pollSec;
            return 0.5;
        }

        private int GetActiveChartSlotCapacity()
        {
            return this._isRealMode ? ChartSlotCount : 60;
        }

        public void RefreshBigChart()
        {
            double yMax = this.ComputeSmartChartYMax();
            this.ChartYMax = yMax;
            this.ChartScaleButtonText = this._isChartAutoScale
                ? string.Format("Авто · {0:F0}%", yMax)
                : "0–100%";
            this.ChartYLabel100 = string.Format("{0:F0}%", yMax);
            this.ChartYLabel75 = string.Format("{0:F0}%", yMax * 0.75);
            this.ChartYLabel50 = string.Format("{0:F0}%", yMax * 0.50);
            this.ChartYLabel25 = string.Format("{0:F0}%", yMax * 0.25);

            int slotCap = this.GetActiveChartSlotCapacity();
            double fullSpanSec = Math.Round((slotCap - 1) * this.GetSampleStepSeconds());
            this.ChartTimeStartLabel = string.Format("-{0:F0}с", fullSpanSec);
            this.ChartTimeQuarterLabel = string.Format("-{0:F0}с", Math.Round(fullSpanSec * 0.75));
            this.ChartTimeMidLabel = string.Format("-{0:F0}с", Math.Round(fullSpanSec * 0.50));
            this.ChartTimeThreeQuarterLabel = string.Format("-{0:F0}с", Math.Round(fullSpanSec * 0.25));

            this.CpuLegendText = "ЦП";
            this.RamLegendText = "ОЗУ";
            this.GpuLegendText = "GPU";

            PointCollection cpuPts = this.CreateChartPoints(this._cpuHistory, yMax);
            PointCollection ramPts = this.CreateChartPoints(this._ramHistory, yMax);
            PointCollection gpuPts = this.CreateChartPoints(this._gpuHistory, yMax);
            this.CpuChartPoints = cpuPts;
            this.RamChartPoints = ramPts;
            this.GpuChartPoints = gpuPts;
            this.CpuChartAreaPoints = CreateChartAreaPointsFromCurve(cpuPts);
            this.RamChartAreaPoints = CreateChartAreaPointsFromCurve(ramPts);
            this.GpuChartAreaPoints = CreateChartAreaPointsFromCurve(gpuPts);

            bool hasDots = cpuPts.Count >= 1 && ramPts.Count >= 1 && gpuPts.Count >= 1;
            if (hasDots)
            {
                Point cEnd = cpuPts[cpuPts.Count - 1];
                Point rEnd = ramPts[ramPts.Count - 1];
                Point gEnd = gpuPts[gpuPts.Count - 1];
                this.CpuDotLeft = cEnd.X - 3.0;
                this.CpuDotTop = cEnd.Y - 3.0;
                this.RamDotLeft = rEnd.X - 3.0;
                this.RamDotTop = rEnd.Y - 3.0;
                this.GpuDotLeft = gEnd.X - 3.0;
                this.GpuDotTop = gEnd.Y - 3.0;
            }
            this.HasChartDots = hasDots;
        }

        public bool TrySampleChartAtX(double mouseX, double mouseY, out int sampleIdx, out string seriesKey, out double snapX, out double snapY, out string readout)
        {
            sampleIdx = -1;
            seriesKey = "";
            snapX = 0.0;
            snapY = ChartPlotBottom;
            readout = "";

            int count = this._cpuHistory.Count;
            if (count < 2) return false;

            double fullW = this._chartWidth > 120.0 ? this._chartWidth : 520.0;
            double plotRight = Math.Max(ChartPlotLeft + 20.0, fullW - ChartPlotRightPad);
            double plotW = plotRight - ChartPlotLeft;
            int slotCap = Math.Max(2, this.GetActiveChartSlotCapacity());
            double stepX = plotW / (slotCap - 1);
            double firstX = plotRight - (count - 1) * stepX;

            if (mouseX < Math.Max(ChartPlotLeft - 8.0, firstX - 10.0) || mouseX > plotRight + 8.0) return false;

            int offsetFromRight = (int)Math.Round((plotRight - Math.Min(plotRight, Math.Max(firstX, mouseX))) / stepX);
            int idx = (count - 1) - offsetFromRight;
            idx = Math.Max(0, Math.Min(count - 1, idx));

            sampleIdx = idx;
            snapX = plotRight - (count - 1 - idx) * stepX;
            double yMax = this._chartYMax > 0.0 ? this._chartYMax : 100.0;

            double cpuVal = this._cpuHistory[idx];
            double ramVal = idx < this._ramHistory.Count ? this._ramHistory[idx] : 0.0;
            double gpuVal = idx < this._gpuHistory.Count ? this._gpuHistory[idx] : 0.0;

            double cpuY = ValueToChartY(cpuVal, yMax);
            double ramY = ValueToChartY(ramVal, yMax);
            double gpuY = ValueToChartY(gpuVal, yMax);

            double dCpu = Math.Abs(mouseY - cpuY);
            double dRam = Math.Abs(mouseY - ramY);
            double dGpu = Math.Abs(mouseY - gpuY);

            double agoSec = Math.Round((count - 1 - idx) * this.GetSampleStepSeconds());
            string timeTag = agoSec <= 0.5 ? "Сейчас" : string.Format("-{0:F0}с", agoSec);

            if (dRam <= dCpu && dRam <= dGpu)
            {
                seriesKey = "ram";
                snapY = ramY;
                readout = this._lastRamTotalGb > 0.1
                    ? string.Format("ОЗУ · {0:F0}% ({1:F1} ГБ) • {2}", ramVal, this._lastRamTotalGb * ramVal / 100.0, timeTag)
                    : string.Format("ОЗУ · {0:F0}% • {1}", ramVal, timeTag);
            }
            else if (dCpu <= dGpu)
            {
                seriesKey = "cpu";
                snapY = cpuY;
                readout = string.Format("ЦП · {0:F1}% • {1}", cpuVal, timeTag);
            }
            else
            {
                seriesKey = "gpu";
                snapY = gpuY;
                readout = string.Format("GPU · {0:F0}% • {1}", gpuVal, timeTag);
            }
            return true;
        }

        private static double ValueToChartY(double value, double yMax)
        {
            double clamped = Math.Max(0.0, Math.Min(yMax, value));
            return ChartPlotBottom - (clamped / yMax * ChartUsableHeight);
        }

        private PointCollection CreateSparklinePoints(List<double> history, bool autoScale, double minMaxScale, double minFloor)
        {
            PointCollection pts = new PointCollection();
            double w = 90.0;
            double h = 26.0;
            double padY = 3.0;
            double usableH = h - padY * 2.0;
            int count = history.Count;
            if (count == 0) return pts;

            double upper = minMaxScale;
            if (autoScale)
            {
                double peak = GetListMax(history) * 1.25;
                upper = Math.Max(minMaxScale, Math.Min(100.0, peak));
            }
            double span = Math.Max(1.0, upper - minFloor);

            if (count == 1)
            {
                double val0 = Math.Max(minFloor, Math.Min(upper, history[0]));
                double y0 = (h - padY) - ((val0 - minFloor) / span * usableH);
                pts.Add(new Point(0.0, y0));
                pts.Add(new Point(w, y0));
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    double x = i * (w / (count - 1));
                    double val = Math.Max(minFloor, Math.Min(upper, history[i]));
                    double y = (h - padY) - ((val - minFloor) / span * usableH);
                    pts.Add(new Point(x, y));
                }
            }
            if (pts.CanFreeze) pts.Freeze();
            return pts;
        }

        private PointCollection CreateChartPoints(List<double> history, double yMax)
        {
            PointCollection pts = new PointCollection();
            double fullW = this._chartWidth > 120.0 ? this._chartWidth : 520.0;
            double plotRight = Math.Max(ChartPlotLeft + 20.0, fullW - ChartPlotRightPad);
            double plotW = plotRight - ChartPlotLeft;
            int count = history.Count;
            if (count == 0) return pts;
            double scaleMax = yMax > 0.0 ? yMax : 100.0;
            int slotCap = Math.Max(2, this.GetActiveChartSlotCapacity());
            double stepX = plotW / (slotCap - 1);

            if (count == 1)
            {
                double y0 = ValueToChartY(history[0], scaleMax);
                pts.Add(new Point(Math.Max(ChartPlotLeft, plotRight - stepX), y0));
                pts.Add(new Point(plotRight, y0));
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    double x = plotRight - (count - 1 - i) * stepX;
                    if (x < ChartPlotLeft) x = ChartPlotLeft;
                    pts.Add(new Point(x, ValueToChartY(history[i], scaleMax)));
                }
            }
            if (pts.CanFreeze) pts.Freeze();
            return pts;
        }

        private static PointCollection CreateChartAreaPointsFromCurve(PointCollection curvePts)
        {
            if (curvePts == null || curvePts.Count < 2) return new PointCollection();
            PointCollection area = new PointCollection(curvePts.Count + 2);
            area.Add(new Point(curvePts[0].X, ChartPlotBottom));
            for (int i = 0; i < curvePts.Count; i++) area.Add(curvePts[i]);
            area.Add(new Point(curvePts[curvePts.Count - 1].X, ChartPlotBottom));
            if (area.CanFreeze) area.Freeze();
            return area;
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
                        TweakCardViewModel vm = new TweakCardViewModel(items[i], this._engine, this.OnTweakStateChanged, this.OnBatchSelectionChanged);
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

            // Total & Recommended pseudo-categories
            int total = this._allTweaks.Count;
            int recCount = 0;
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                if (this._allTweaks[i].IsRecommended) recCount++;
            }

            CategoryItemViewModel allCat = new CategoryItemViewModel("all", "Все твики", "\uE71D", total);
            allCat.IsSelected = true;
            this._selectedCategory = allCat;
            this._categories.Add(allCat);

            CategoryItemViewModel recCat = new CategoryItemViewModel("recommended", "Рекомендуемые", "\uE73E", recCount);
            this._categories.Add(recCat);

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
                { "steam", Tuple.Create("Steam", "\uE7FC", 7) },
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
            string query = (this._searchQuery != null) ? this._searchQuery.Trim().ToLowerInvariant() : "";
            string catId = (this._selectedCategory != null) ? this._selectedCategory.Id : "all";

            List<TweakCardViewModel> matching = new List<TweakCardViewModel>(this._allTweaks.Count);

            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._allTweaks[i];

                if (catId == "recommended")
                {
                    if (!tw.IsRecommended) continue;
                }
                else if (catId != "all" && !string.Equals(tw.Category, catId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // FilterSegment: 0=All, 1=Enabled, 2=Disabled, 3=NeedsReview, 4=Recommended
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
                if (this._filterSegment == 4 && !tw.IsRecommended)
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

            // High performance atomic update: ReplaceAll clears and populates without firing intermediate events,
            // raising exactly ONE single Reset notification to CollectionViewSource.
            this._filteredTweaks.ReplaceAll(matching);

            int visUnapplied = 0;
            int visEnabled = 0;
            for (int i = 0; i < matching.Count; i++)
            {
                var tw = matching[i];
                if (tw.Status == TweakStatus.Enabled)
                {
                    visEnabled++;
                }
                else if (IsEligibleForContextBatchApply(tw))
                {
                    visUnapplied++;
                }
            }
            this.VisibleUnappliedCount = visUnapplied;
            this.OnPropertyChanged("VisibleUnappliedCount");
            this.OnPropertyChanged("CanContextApply");
            this.OnPropertyChanged("ContextApplyButtonText");
            this.OnPropertyChanged("ContextApplyToolTip");

            this.SearchSummaryText = string.Format("Показано: {0} (активно: {1})", this._filteredTweaks.Count, visEnabled);
        }

        public void ApplyGrouping()
        {
            if (this._filteredTweaksView == null) return;
            if (this._isGrouped)
            {
                if (this._filteredTweaksView.GroupDescriptions.Count == 0)
                {
                    this._filteredTweaksView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                }
            }
            else
            {
                if (this._filteredTweaksView.GroupDescriptions.Count > 0)
                {
                    this._filteredTweaksView.GroupDescriptions.Clear();
                }
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
            if (this._cpuHistory.Count >= 1)
            {
                this.RefreshBigChart();
            }
        }

        public void Populate60SecondsHistory()
        {
            if (this._isRealMode) return;
            this._cpuHistory.Clear();
            this._ramHistory.Clear();
            this._gpuHistory.Clear();
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
                this._gpuHistory.Add(10.0 + (i % 6) * 2.0);
                this._pingHistory.Add(1.2 + (i % 3) * 0.4);
                this._tempHistory.Add(48.0 + (i % 4));
            }

            this.RefreshBigChart();
            this.CpuSparkline = this.CreateSparklinePoints(this._cpuHistory, true, 16.0, 0.0);
            this.RamSparkline = this.CreateSparklinePoints(this._ramHistory, false, 100.0, 0.0);
            this.PingSparkline = this.CreateSparklinePoints(this._pingHistory, true, 10.0, 0.0);
            this.TempSparkline = this.CreateSparklinePoints(this._tempHistory, false, 90.0, 30.0);
            this.IsCollectingData = false;
            this.OnPropertyChanged("HasAtLeast5Points");
            this.OnPropertyChanged("IsCollectingData");
        }

        private void OnBatchSelectionChanged()
        {
            int sel = 0;
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                if (this._allTweaks[i].IsSelectedForBatch) sel++;
            }
            this.SelectedBatchCount = sel;
            this.OnPropertyChanged("SelectedBatchCount");
            this.OnPropertyChanged("HasSelectedForBatch");
            this.OnPropertyChanged("CanContextApply");
            this.OnPropertyChanged("ContextApplyButtonText");
            this.OnPropertyChanged("ContextApplyToolTip");
        }

        private void UpdateSummary()
        {
            int total = this._allTweaks.Count;
            int enabled = 0;
            int disabled = 0;
            int review = 0;
            int recTotal = 0;
            int recApplied = 0;
            int recRemaining = 0;
            int selCount = 0;
            int revertable = 0;
            int visUnapplied = 0;
            int currentBuild = Environment.OSVersion.Version.Build;

            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                var tw = this._allTweaks[i];
                if (tw.Status == TweakStatus.Enabled)
                {
                    enabled++;
                    if (!tw.IsActionTweak && tw.HasSnapshotRecord && tw.CanRevert) revertable++;
                }
                else if (tw.Status == TweakStatus.Disabled)
                {
                    disabled++;
                }
                if (tw.NeedsReview)
                {
                    review++;
                }
                if (tw.IsSelectedForBatch)
                {
                    selCount++;
                }
                if (tw.IsRecommended)
                {
                    recTotal++;
                    if (tw.Status == TweakStatus.Enabled) recApplied++;
                    else if (tw.Risk == TweakRisk.Low && TweakEngine.IsEligibleForBatchApply(tw.Model, currentBuild, tw.Status)) recRemaining++;
                }
            }

            int visEnabled = 0;
            for (int i = 0; i < this._filteredTweaks.Count; i++)
            {
                var tw = this._filteredTweaks[i];
                if (tw.Status == TweakStatus.Enabled)
                {
                    visEnabled++;
                }
                else if (IsEligibleForContextBatchApply(tw))
                {
                    visUnapplied++;
                }
            }
            this.SearchSummaryText = string.Format("Показано: {0} (активно: {1})", this._filteredTweaks.Count, visEnabled);

            this.TotalTweaksCount = total;
            this.EnabledTweaksCount = enabled;
            this.DisabledTweaksCount = disabled;
            this.NeedsReviewCount = review;
            this.RecommendedTotalCount = recTotal;
            this.RecommendedAppliedCount = recApplied;
            this.RecommendedRemainingCount = recRemaining;
            this.SelectedBatchCount = selCount;
            this.RevertableCount = revertable;
            this.VisibleUnappliedCount = visUnapplied;

            this.SegmentAllText = string.Format("Все ({0})", total);
            this.SegmentRecommendedText = string.Format("Рекомендуемые ({0})", recTotal);
            this.SegmentEnabledText = string.Format("Включено ({0})", enabled);
            this.SegmentDisabledText = string.Format("Выключено ({0})", disabled);
            this.SegmentNeedsReviewText = string.Format("Внимание ({0})", review);

            this.LastDiagnosticSummary = string.Format(
                "Рекомендации: {0} из {1} активно (осталось применить: {2}) • Требуют внимания: {3}",
                recApplied, recTotal, recRemaining, review);

            this.SummaryText = string.Format("Активно: {0} из {1}", enabled, total);
            this.TweaksDisplay = enabled.ToString();
            this.TweaksTotalText = string.Format("из {0} • рек. {1}/{2}", total, recApplied, recTotal);
            this.TweaksStatus = string.Format("из {0}", total);

            this.OnPropertyChanged("CanApplyRecommended");
            this.OnPropertyChanged("ApplyRecommendedToolTip");
            this.OnPropertyChanged("TotalTweaksCount");
            this.OnPropertyChanged("EnabledTweaksCount");
            this.OnPropertyChanged("DisabledTweaksCount");
            this.OnPropertyChanged("NeedsReviewCount");
            this.OnPropertyChanged("RecommendedTotalCount");
            this.OnPropertyChanged("RecommendedAppliedCount");
            this.OnPropertyChanged("RecommendedRemainingCount");
            this.OnPropertyChanged("RecommendedBatchButtonText");
            this.OnPropertyChanged("SelectedBatchCount");
            this.OnPropertyChanged("HasSelectedForBatch");
            this.OnPropertyChanged("VisibleUnappliedCount");
            this.OnPropertyChanged("CanContextApply");
            this.OnPropertyChanged("ContextApplyButtonText");
            this.OnPropertyChanged("ContextApplyToolTip");
            this.OnPropertyChanged("RevertableCount");
            this.OnPropertyChanged("RevertAllButtonText");
            this.OnPropertyChanged("OptimizationPercent");
            this.OnPropertyChanged("OptimizationPercentText");
            this.OnPropertyChanged("SegmentAllText");
            this.OnPropertyChanged("SegmentRecommendedText");
            this.OnPropertyChanged("SegmentEnabledText");
            this.OnPropertyChanged("SegmentDisabledText");
            this.OnPropertyChanged("SegmentNeedsReviewText");
            this.OnPropertyChanged("TweaksTotalText");
            this.OnPropertyChanged("LastDiagnosticSummary");
            this.OnPropertyChanged("DatabaseStatsDisplay");
            this.OnPropertyChanged("EngineSummaryFooterText");
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
                Task.Run(() => this._settingsManager.Save());
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

        private void OnShowRecommended(object obj)
        {
            for (int n = 0; n < this._navItems.Count; n++)
            {
                if (this._navItems[n].Id == "tweaks")
                {
                    this.SelectedNavItem = this._navItems[n];
                    break;
                }
            }
            for (int c = 0; c < this._categories.Count; c++)
            {
                if (this._categories[c].Id == "recommended")
                {
                    this.SelectedCategory = this._categories[c];
                    break;
                }
            }
            this.FilterSegment = 0;
        }

        private void OnSelectAllVisible(object obj)
        {
            for (int i = 0; i < this._filteredTweaks.Count; i++)
            {
                var tw = this._filteredTweaks[i];
                if (!tw.IsActionTweak && tw.Status != TweakStatus.Unavailable)
                {
                    tw.IsSelectedForBatch = true;
                }
            }
        }

        private void OnClearSelection(object obj)
        {
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                this._allTweaks[i].IsSelectedForBatch = false;
            }
        }

        private void OnApplySelected(object obj)
        {
            List<TweakCardViewModel> rawSelected = new List<TweakCardViewModel>();
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                if (this._allTweaks[i].IsSelectedForBatch)
                {
                    rawSelected.Add(this._allTweaks[i]);
                }
            }

            if (rawSelected.Count > 0)
            {
                this.ExecuteBatchApplyAsync(rawSelected, "Применение выбранных твиков", false);
                return;
            }

            // Если галочки не расставлены — применяем ВСЕ видимые неприменённые оптимизации текущей вкладки/раздела
            List<TweakCardViewModel> visibleTargets = new List<TweakCardViewModel>();
            int alreadyActiveCount = 0;
            int manualOrUnavailableCount = 0;
            for (int i = 0; i < this._filteredTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._filteredTweaks[i];
                if (tw.Status == TweakStatus.Enabled)
                {
                    alreadyActiveCount++;
                }
                else if (IsEligibleForContextBatchApply(tw))
                {
                    visibleTargets.Add(tw);
                }
                else
                {
                    manualOrUnavailableCount++;
                }
            }

            if (visibleTargets.Count == 0)
            {
                WinTweaker.UI.AxisDialogWindow.ShowInfo(
                    "Массовое применение",
                    string.Format("В текущем списке уже активно: {0} • Прочих (разовые/недоступно/ручные): {1}.\nНовых оптимизаций к применению нет.",
                        alreadyActiveCount, manualOrUnavailableCount),
                    "Всё уже применено");
                return;
            }

            string scopeName = (this._selectedCategory != null) ? this._selectedCategory.Name : "Все твики";
            this.ExecuteBatchApplyAsync(visibleTargets, string.Format("Применение списка: {0}", scopeName), false, alreadyActiveCount, manualOrUnavailableCount);
        }

        private static readonly HashSet<string> ManualOnlyPreferenceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "visibility-taskbar-never-combine",
            "visibility-taskbar-align-left",
            "visibility-widgets",
            "visibility-task-view",
            "visibility-pen-menu",
            "visibility-share-any-window-from-my-taskbar",
            "visibility-show-flashing-on-taskbar-apps",
            "visibility-taskbarsd",
            "visibility-slideshowincludecameraroll",
            "visibility-navpane-show-all-folders",
            "visibility-show-frequently-used-folders",
            "visibility-show-recently-used-files",
            "visibility-show-libraries",
            "visibility-show-type-overlay",
            "visibility-folder-contents-info-tip",
            "visibility-show-encrypt-compressed-color",
            "visibility-show-info-tip",
            "visibility-show-preview-handlers",
            "visibility-show-status-bar",
            "visibility-persist-browsers",
            "visibility-show-badges-on-taskbar-apps",
            "visibility-showsecondsinsystemclock",
            "visibility-use-default-tile",
            "system-enabletaskgroups",
            "system-notifications-enable-legacy-balloons",
            "security-wu-allow-servicing-enterprise-features",
            "security-uac-enable-uia-desktop-toggle",
            "security-pua-protection",
            "security-block-at-first-seen",
            "security-enable-network-protection",
            "security-controlled-folder-access",
            "security-defender-disable-behavior-monitoring",
            "security-defender-disable-ioav-protection",
            "security-defender-disable-on-access-protection",
            "security-defender-disable-scan-realtime-enable",
            "security-defender-disable-email-scanning",
            "security-defender-allow-network-protection-server",
            "security-defender-disable-antispyware",
            "security-wu-donot-enforce-cert-pinning",
            "security-wu-au-donot-enforce-cert-pinning",
            "security-powershell-enable-module-logging",
            "security-powershell-enable-transcripting",
            "security-powershell-enable-scriptblock-logging",
            "security-powershell-invocation-header",
            "security-powershell-scriptblock-invocation-logging",
            "security-powershell-user-invocation-header",
            "security-powershell-user-scriptblock-invocation-logging",
            // Экспериментальные / отладочные параметры GPU, DWM, MMCSS и таймеров ядра (риск зависаний при массовом применении)
            "system-dxg-hw-queue-packet-cap",
            "system-dxg-hwsch-thread-offload",
            "system-kernel-disable-low-qos-timer-resolution",
            "system-dwm-enable-front-buffer-render-checks",
            "system-dwm-disable-draw-list-caching",
            "system-dwm-disable-device-bitmaps",
            "system-dwm-disable-locking-memory",
            "system-mmcss-no-lazy-mode",
            "system-mmcss-idle-detection-cycles",
            "system-mmcss-lazy-mode-timeout",
            "system-mmcss-scheduler-period",
            "system-mmcss-max-threads-process",
            "system-mmcss-max-threads-total"
        };

        internal static bool IsEligibleForContextBatchApply(TweakCardViewModel tw)
        {
            if (tw == null || tw.IsActionTweak) return false;
            if (tw.Status == TweakStatus.Enabled || tw.Status == TweakStatus.Unavailable) return false;
            if (!string.IsNullOrEmpty(tw.Id) && ManualOnlyPreferenceIds.Contains(tw.Id)) return false;
            if (TweakEngine.HasApplyBlock(tw.Model)) return false;
            return true;
        }

        private static bool IsSecuritySensitiveTweak(TweakCardViewModel tw)
        {
            if (tw == null) return false;
            if (tw.Risk == TweakRisk.High) return true;
            string id = tw.Id != null ? tw.Id.ToLowerInvariant() : "";
            return id.Contains("defender") || id.Contains("firewall") || id.Contains("uac") ||
                   id.Contains("smartscreen") || id.Contains("vbs") || id.Contains("hvci") ||
                   id.Contains("mitigation") || id.Contains("spectre") || id.Contains("meltdown") ||
                   id.Contains("windows-update") || id.Contains("disable-update");
        }

        private void OnApplyAllRecommended(object obj)
        {
            if (!this.CanApplyRecommended)
            {
                WinTweaker.UI.AxisDialogWindow.ShowInfo("Применение рекомендаций", "Пресеты пока не заданы.");
                return;
            }

            int currentBuild = Environment.OSVersion.Version.Build;
            List<TweakCardViewModel> targets = new List<TweakCardViewModel>();
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._allTweaks[i];
                if (!tw.IsActionTweak &&
                    tw.IsRecommended &&
                    tw.Risk == TweakRisk.Low &&
                    TweakEngine.IsEligibleForBatchApply(tw.Model, currentBuild, tw.Status))
                {
                    targets.Add(tw);
                }
            }

            if (targets.Count == 0)
            {
                WinTweaker.UI.AxisDialogWindow.ShowInfo(
                    "Рекомендуемые твики",
                    string.Format("Все безопасные рекомендуемые твики уже применены ({0} из {1} активны).",
                        this.RecommendedAppliedCount, this.RecommendedTotalCount),
                    "Оптимизация актуальна");
                return;
            }

            this.ExecuteBatchApplyAsync(targets, "Применение рекомендуемых твиков", true);
        }

        private async void ExecuteBatchApplyAsync(
            List<TweakCardViewModel> rawCandidates,
            string dialogTitle,
            bool isRecommendedAll,
            int alreadyActiveCount = 0,
            int manualOrUnavailableCount = 0)
        {
            int currentBuild = Environment.OSVersion.Version.Build;
            List<TweakCardViewModel> safeTargets = new List<TweakCardViewModel>();
            List<TweakCardViewModel> highRiskTargets = new List<TweakCardViewModel>();
            List<string> skippedWarnings = new List<string>();
            Dictionary<string, string> keyAssignments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < rawCandidates.Count; i++)
            {
                TweakCardViewModel tw = rawCandidates[i];
                if (tw.Model == null || tw.Model.registry == null || tw.Model.registry.Count == 0)
                {
                    skippedWarnings.Add(string.Format("[Пропущено] {0} — отсутствуют действия реестра", tw.Name));
                    continue;
                }
                if (tw.Model.min_build.HasValue && currentBuild < tw.Model.min_build.Value)
                {
                    skippedWarnings.Add(string.Format("[Несовместимо] {0} — требуется сборка Windows {1}+", tw.Name, tw.Model.min_build.Value));
                    continue;
                }
                if (tw.Status == TweakStatus.Unavailable)
                {
                    skippedWarnings.Add(string.Format("[Недоступно] {0} — оборудование/приложение не найдено", tw.Name));
                    continue;
                }

                bool hasApplyBlock = false;
                bool hasConflict = false;
                for (int r = 0; r < tw.Model.registry.Count; r++)
                {
                    var act = tw.Model.registry[r];
                    if (isRecommendedAll && !string.IsNullOrEmpty(act.apply_block))
                    {
                        hasApplyBlock = true;
                        break;
                    }
                    string keySig = string.Format("{0}\\{1}\\{2}", act.hive ?? "", act.path ?? "", act.name ?? "");
                    string valSig = act.apply != null ? act.apply.ToString() : "";
                    if (keySig.IndexOf(@"\00XX", StringComparison.OrdinalIgnoreCase) < 0 &&
                        keySig.IndexOf(@"\...", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        string existingVal;
                        if (keyAssignments.TryGetValue(keySig, out existingVal) &&
                            !string.Equals(existingVal, valSig, StringComparison.OrdinalIgnoreCase))
                        {
                            hasConflict = true;
                            break;
                        }
                        keyAssignments[keySig] = valSig;
                    }
                }

                if (hasApplyBlock)
                {
                    skippedWarnings.Add(string.Format("[Ограничение] {0} — заблокирован для автопакета (apply_block)", tw.Name));
                    continue;
                }
                if (hasConflict)
                {
                    skippedWarnings.Add(string.Format("[Конфликт ключей] {0} — конфликтует с другим выбранным твиком", tw.Name));
                    continue;
                }

                if (!isRecommendedAll && IsSecuritySensitiveTweak(tw))
                {
                    highRiskTargets.Add(tw);
                }
                else
                {
                    safeTargets.Add(tw);
                }
            }

            if (safeTargets.Count == 0 && highRiskTargets.Count == 0)
            {
                WinTweaker.UI.AxisDialogWindow.ShowWarning(
                    dialogTitle,
                    "Ни один из выбранных твиков не подходит для применения из-за ограничений совместимости или конфликтов.",
                    "Нет подходящих операций",
                    skippedWarnings);
                return;
            }

            List<string> previewItems = new List<string>();
            int rebootCount = 0;
            for (int i = 0; i < safeTargets.Count; i++)
            {
                if (safeTargets[i].RequiresReboot) rebootCount++;
                string rebootTag = safeTargets[i].RequiresReboot ? " [перезагрузка]" : "";
                previewItems.Add(string.Format("{0}. {1} ({2}){3}", i + 1, safeTargets[i].Name, safeTargets[i].CategoryDisplayName, rebootTag));
            }
            for (int i = 0; i < highRiskTargets.Count; i++)
            {
                previewItems.Add(string.Format("⚠️ [Защита ОС / Риск] {0} ({1})", highRiskTargets[i].Name, highRiskTargets[i].CategoryDisplayName));
            }
            for (int i = 0; i < skippedWarnings.Count; i++)
            {
                previewItems.Add(skippedWarnings[i]);
            }

            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("Безопасных / стандартных к применению: {0}.", safeTargets.Count);
            if (highRiskTargets.Count > 0)
            {
                msg.AppendFormat(" Настроек снижения защиты / повышенного риска: {0} (применяются только при отметке галочки ниже).", highRiskTargets.Count);
            }
            if (isRecommendedAll)
            {
                msg.AppendFormat(" (Всего рекомендовано: {0}, уже активно: {1}).", this.RecommendedTotalCount, this.RecommendedAppliedCount);
            }
            msg.AppendLine();
            msg.AppendLine("• Исходные значения сохраняются в state.json перед изменением для точного отката.");
            if (this._isRealMode && !IsUserAdmin())
            {
                msg.AppendLine("• ВНИМАНИЕ: приложение запущено без прав администратора — системные ключи могут быть недоступны.");
            }
            if (rebootCount > 0)
            {
                msg.AppendFormat("• Для {0} твик(ов) потребуется перезагрузка ПК.", rebootCount);
            }

            string guardText = highRiskTargets.Count > 0
                ? string.Format("Включить также настройки повышенного риска / защиты ОС ({0} шт.)", highRiskTargets.Count)
                : null;

            string confirmSubtitle = (!isRecommendedAll && (alreadyActiveCount > 0 || manualOrUnavailableCount > 0))
                ? string.Format("Уже активно: {0} • К применению: {1} (риск/защита: {2}) • Разовые/ручные/недоступно: {3}",
                    alreadyActiveCount, safeTargets.Count + highRiskTargets.Count, highRiskTargets.Count, manualOrUnavailableCount + skippedWarnings.Count)
                : string.Format("Безопасных: {0} • Рискованных/Защита: {1} • Пропущено: {2}", safeTargets.Count, highRiskTargets.Count, skippedWarnings.Count);

            bool includeHighRisk;
            bool confirmed = WinTweaker.UI.AxisDialogWindow.ShowBatchConfirmWithGuard(
                dialogTitle,
                confirmSubtitle,
                msg.ToString().Trim(),
                previewItems,
                guardText,
                out includeHighRisk,
                "Применить",
                "Отмена");

            if (!confirmed) return;

            List<TweakCardViewModel> finalTargets = new List<TweakCardViewModel>(safeTargets);
            if (includeHighRisk)
            {
                finalTargets.AddRange(highRiskTargets);
            }

            if (finalTargets.Count == 0)
            {
                WinTweaker.UI.AxisDialogWindow.ShowInfo(dialogTitle, "Безопасных настроек в списке нет, а применение настроек риска не отмечено галочкой.");
                return;
            }

            this.StatusNotification = string.Format("Применение {0} твиков...", finalTargets.Count);

            int appliedCount = 0;
            int errorCount = 0;
            int runtimeSkippedCount = skippedWarnings.Count;
            List<string> resultLines = new List<string>();
            TweakStatus[] resolvedStatuses = new TweakStatus[finalTargets.Count];

            await Task.Run(() =>
            {
                if (this._engine != null && this._engine.RestorePointManager != null && this._isRealMode)
                {
                    string rpMsg;
                    this._engine.RestorePointManager.EnsureRestorePoint(dialogTitle ?? "AXIS Batch Apply", this._journalManager, out rpMsg);
                }

                if (this._stateManager != null) this._stateManager.BeginBatch();
                if (this._journalManager != null) this._journalManager.BeginBatch();
                try
                {
                    for (int i = 0; i < finalTargets.Count; i++)
                    {
                        TweakCardViewModel tw = finalTargets[i];
                        try
                        {
                            if (this._engine.CheckStatus(tw.Model) == TweakStatus.Unavailable)
                            {
                                runtimeSkippedCount++;
                                resolvedStatuses[i] = TweakStatus.Unavailable;
                                resultLines.Add("[ПРОПУЩЕНО] " + tw.Name + " — недоступно на этом ПК");
                                continue;
                            }

                            bool ok = this._engine.Apply(tw.Model, true);
                            if (ok)
                            {
                                appliedCount++;
                                resolvedStatuses[i] = TweakStatus.Enabled;
                                resultLines.Add("[OK] " + tw.Name);
                            }
                            else
                            {
                                errorCount++;
                                resolvedStatuses[i] = this._engine.CheckStatus(tw.Model);
                                resultLines.Add("[ОШИБКА] " + tw.Name + " — не удалось применить");
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            resolvedStatuses[i] = TweakStatus.Unknown;
                            resultLines.Add("[ОШИБКА] " + tw.Name + ": " + ex.Message);
                        }
                    }
                }
                finally
                {
                    if (this._stateManager != null) this._stateManager.EndBatch();
                    if (this._journalManager != null) this._journalManager.EndBatch();
                    SpecialTweakExecutor.InvalidateCaches();
                }

                // Точно сверяем фактический статус применённых твиков на фоновом потоке
                for (int i = 0; i < finalTargets.Count; i++)
                {
                    if (resolvedStatuses[i] != TweakStatus.Unavailable)
                    {
                        resolvedStatuses[i] = this._engine.CheckStatus(finalTargets[i].Model);
                    }
                }
            });

            for (int i = 0; i < finalTargets.Count; i++)
            {
                finalTargets[i].ApplyResolvedStatus(resolvedStatuses[i], true);
                finalTargets[i].IsSelectedForBatch = false;
            }

            this.OnTweakStateChanged();

            string statusMsg = string.Format("Применено: {0}", appliedCount);
            if (errorCount > 0) statusMsg += string.Format(", ошибок: {0}", errorCount);
            this.StatusNotification = statusMsg;

            WinTweaker.UI.AxisDialogWindow.ShowBatchReport(
                dialogTitle,
                string.Format("Успешно: {0} • Уже были активны: {1} • Ошибок: {2} • Пропущено: {3}",
                    appliedCount, alreadyActiveCount, errorCount, runtimeSkippedCount),
                string.Format("Применение завершено за один пакет. Исходные значения сохранены в state.json.{0}",
                    rebootCount > 0 && appliedCount > 0 ? "\nДля полного эффекта рекомендуется перезагрузить компьютер." : ""),
                resultLines,
                errorCount > 0);
        }

        private async void OnRevertAll(object obj)
        {
            List<TweakCardViewModel> targets = new List<TweakCardViewModel>();
            int withoutSnapshotCount = 0;
            for (int i = 0; i < this._allTweaks.Count; i++)
            {
                TweakCardViewModel tw = this._allTweaks[i];
                if (tw.Status == TweakStatus.Enabled && !tw.IsActionTweak)
                {
                    if (tw.HasSnapshotRecord && tw.CanRevert)
                    {
                        targets.Add(tw);
                    }
                    else
                    {
                        withoutSnapshotCount++;
                    }
                }
            }

            if (targets.Count == 0)
            {
                string extra = withoutSnapshotCount > 0
                    ? string.Format("\n\nОбнаружено {0} активных параметров без сохранённого снимка AXIS (изменены до использования программы или вручную) — они не будут затронуты.", withoutSnapshotCount)
                    : "";
                WinTweaker.UI.AxisDialogWindow.ShowInfo(
                    "Откат твиков",
                    "Нет применённых твиков с сохранённым исходным снимком для безопасного отката." + extra,
                    "Снимки не найдены");
                return;
            }

            List<string> revertPreview = new List<string>();
            for (int i = 0; i < targets.Count; i++)
            {
                revertPreview.Add(string.Format("{0}. {1} ({2})", i + 1, targets[i].Name, targets[i].CategoryDisplayName));
            }

            string confirmMsg = string.Format(
                "Вернуть {0} твик(ов) к точным исходным значениям, сохранённым в state.json?\n" +
                "• Параметры, чьё исходное состояние не сохранялось ({1} шт.), изменены не будут.",
                targets.Count, withoutSnapshotCount);

            bool confirmed = WinTweaker.UI.AxisDialogWindow.ShowConfirm(
                "Подтверждение массового отката",
                string.Format("Будет откачено: {0} • Без снимка (не трогаются): {1}", targets.Count, withoutSnapshotCount),
                confirmMsg,
                revertPreview,
                "Откатить все",
                "Отмена",
                true);

            if (!confirmed) return;

            this.StatusNotification = string.Format("Откат {0} твиков...", targets.Count);

            int revertedCount = 0;
            int errorCount = 0;
            List<string> resultLines = new List<string>();

            TweakStatus[] revertStatuses = new TweakStatus[targets.Count];
            await Task.Run(() =>
            {
                if (this._stateManager != null) this._stateManager.BeginBatch();
                if (this._journalManager != null) this._journalManager.BeginBatch();
                try
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        TweakCardViewModel tw = targets[i];
                        try
                        {
                            bool ok = this._engine.Revert(tw.Model);
                            if (ok)
                            {
                                revertedCount++;
                                revertStatuses[i] = TweakStatus.Disabled;
                                resultLines.Add("[ОТКАТ OK] " + tw.Name);
                            }
                            else
                            {
                                errorCount++;
                                revertStatuses[i] = this._engine.CheckStatus(tw.Model);
                                resultLines.Add("[ОШИБКА] " + tw.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            revertStatuses[i] = TweakStatus.Unknown;
                            resultLines.Add("[ОШИБКА] " + tw.Name + ": " + ex.Message);
                        }
                    }
                }
                finally
                {
                    if (this._stateManager != null) this._stateManager.EndBatch();
                    if (this._journalManager != null) this._journalManager.EndBatch();
                    SpecialTweakExecutor.InvalidateCaches();
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    revertStatuses[i] = this._engine.CheckStatus(targets[i].Model);
                }
            });

            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].ApplyResolvedStatus(revertStatuses[i], true);
            }

            this.OnTweakStateChanged();

            string resultMsg = string.Format("Откачено: {0}", revertedCount);
            if (errorCount > 0) resultMsg += string.Format(", ошибок: {0}", errorCount);
            this.StatusNotification = resultMsg;

            WinTweaker.UI.AxisDialogWindow.ShowBatchReport(
                "Отчёт о массовом откате",
                string.Format("Восстановлено: {0} • Ошибок: {1} • Не затронуто без снимка: {2}", revertedCount, errorCount, withoutSnapshotCount),
                "Операция возврата к сохранённым исходным значениям завершена.",
                resultLines,
                errorCount > 0);
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

        private void SetPollingInterval(int sec)
        {
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.PollingIntervalSeconds = sec;
                this._settingsManager.Save();
            }
            if (this._liveTimer != null)
            {
                if (sec <= 0)
                {
                    this._liveTimer.Stop();
                }
                else
                {
                    this._liveTimer.Interval = (sec == 1) ? TimeSpan.FromMilliseconds(500) : TimeSpan.FromSeconds(sec);
                    if (!this._liveTimer.IsEnabled) this._liveTimer.Start();
                }
            }
            this.OnPropertyChanged("IsPolling1Sec");
            this.OnPropertyChanged("IsPolling2Sec");
            this.OnPropertyChanged("IsPolling5Sec");
            this.OnPropertyChanged("IsPollingPaused");
        }

        private async void OnCreateRestorePoint(object obj)
        {
            if (this._isCreatingRestorePoint) return;
            this.IsCreatingRestorePoint = true;
            this.RestorePointNotification = "Создание контрольной точки Windows...";

            string resultMsg = "";
            bool ok = false;

            await Task.Run(() =>
            {
                try
                {
                    string args = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description 'AXIS Manual Checkpoint' -RestorePointType 'MODIFY_SETTINGS'\"";
                    ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", args)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    using (Process proc = Process.Start(psi))
                    {
                        proc.WaitForExit(35000);
                        if (proc.ExitCode == 0)
                        {
                            ok = true;
                            resultMsg = "Точка восстановления успешно создана!";
                        }
                        else
                        {
                            string err = proc.StandardError.ReadToEnd();
                            resultMsg = "Ошибка создания: " + (string.IsNullOrEmpty(err) ? ("код " + proc.ExitCode) : err.Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultMsg = "Ошибка: " + ex.Message;
                }
            });

            this.IsCreatingRestorePoint = false;
            this.RestorePointNotification = resultMsg;
            this.UpdateRestorePointStatus();
            if (this._journalManager != null)
            {
                this._journalManager.Log(ok ? "INFO" : "WARN", "", resultMsg);
            }
        }

        private void OnOpenSystemProtection(object obj)
        {
            try
            {
                Process.Start("SystemPropertiesProtection.exe");
            }
            catch (Exception ex)
            {
                WinTweaker.UI.AxisDialogWindow.ShowWarning("Защита системы", "Не удалось открыть окно параметров защиты: " + ex.Message);
            }
        }

        private async void OnEnableSystemRestore(object obj)
        {
            this.RestorePointNotification = "Активация Защиты системы (C:)...";
            await Task.Run(() =>
            {
                try
                {
                    string args = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"Enable-ComputerRestore -Drive 'C:\\'\"";
                    ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", args)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (Process proc = Process.Start(psi))
                    {
                        proc.WaitForExit(15000);
                    }
                }
                catch { }
            });
            this.UpdateRestorePointStatus();
            this.RestorePointNotification = "Защита системы активирована для диска C:.";
        }

        private void OnExportSnapshots(object obj)
        {
            try
            {
                if (this._stateManager != null && File.Exists(this._stateManager.StateFilePath))
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string target = Path.Combine(desktop, string.Format("axis_backup_{0:yyyyMMdd_HHmmss}.json", DateTime.Now));
                    File.Copy(this._stateManager.StateFilePath, target, true);
                    WinTweaker.UI.AxisDialogWindow.ShowInfo(
                        "Экспорт снимка AXIS",
                        "Резервная копия снимка состояния успешно создана на Рабочем столе:\n" + target,
                        "state.json сохранён");
                }
                else
                {
                    WinTweaker.UI.AxisDialogWindow.ShowInfo("Экспорт снимка", "Снимки пока не созданы (нет активных твиков с историей).");
                }
            }
            catch (Exception ex)
            {
                WinTweaker.UI.AxisDialogWindow.ShowError("Экспорт снимка", "Ошибка при экспорте: " + ex.Message);
            }
        }

        private void OnCopyDiagnosticReport(object obj)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("==================================================================");
                sb.AppendLine("        AXIS 3.0 (2026) — ДИАГНОСТИЧЕСКИЙ ОТЧЕТ СИСТЕМЫ           ");
                sb.AppendLine("==================================================================");
                sb.AppendLine(string.Format("Время отчета: {0:yyyy-MM-dd HH:mm:ss}", DateTime.Now));
                sb.AppendLine(string.Format("ОС:           {0}", this._osSpec));
                sb.AppendLine(string.Format("Процессор:    {0}", this._cpuSpec));
                sb.AppendLine(string.Format("Плата:        {0}", this._motherboardSpec));
                sb.AppendLine(string.Format("Видеокарта:   {0}", this._gpuSpec));
                sb.AppendLine(string.Format("Память:       {0}", this._ramSpec));
                sb.AppendLine(string.Format("Диски:        {0}", this._disksSpec));
                sb.AppendLine("------------------------------------------------------------------");
                sb.AppendLine("ПОДСИСТЕМЫ ЯДРА И ЗАДЕРЖКИ:");
                sb.AppendLine(string.Format("• GPU HAGS:                   {0}", this._hagsStatusText));
                sb.AppendLine(string.Format("• Hardware Flip Queue:        {0}", this._flipQueueStatusText));
                sb.AppendLine(string.Format("• MMCSS Responsiveness:       {0}", this._mmcssRespText));
                sb.AppendLine(string.Format("• MMCSS Throttling:           {0}", this._mmcssThrottlingText));
                sb.AppendLine(string.Format("• Game Mode (WNF):            {0}", this.GameModeShortStatus));
                sb.AppendLine("------------------------------------------------------------------");
                sb.AppendLine("СТАТИСТИКА ТВИТОВ AXIS:");
                int activeCount = 0;
                for (int i = 0; i < this._allTweaks.Count; i++)
                {
                    if (this._allTweaks[i].Status == TweakStatus.Enabled) activeCount++;
                }
                sb.AppendLine(string.Format("• Всего в базе:               {0} твиков (12 категорий)", this._allTweaks.Count));
                sb.AppendLine(string.Format("• Активно оптимизаций:        {0} ({1:F0}%)", activeCount, this._allTweaks.Count > 0 ? (activeCount * 100.0 / this._allTweaks.Count) : 0));
                sb.AppendLine(string.Format("• Снимков отката в базе:      {0}", this._stateManager != null ? this._stateManager.SnapshotCount : 0));
                sb.AppendLine(string.Format("• Защита системы Windows:     {0} (Последняя: {1})", this._systemRestoreStatus, this._lastRestorePointDisplay));
                sb.AppendLine("==================================================================");

                Clipboard.SetText(sb.ToString());
                this.SettingsSaveNotification = "Диагностический отчет скопирован в буфер обмена!";
            }
            catch (Exception ex)
            {
                WinTweaker.UI.AxisDialogWindow.ShowWarning("Отчет", "Ошибка копирования отчета: " + ex.Message);
            }
        }

        private void OnSetPingPreset(object obj)
        {
            string host = obj as string;
            if (host == "gateway")
            {
                this.PingCustomHost = "";
            }
            else if (!string.IsNullOrEmpty(host))
            {
                this.PingCustomHost = host;
            }
            this.OnSaveSettings(null);
        }

        public void Dispose()
        {
            if (this._isDisposed) return;
            this._isDisposed = true;
            if (this._liveTimer != null)
            {
                try
                {
                    this._liveTimer.Stop();
                    this._liveTimer.Tick -= this.OnLiveTimerTick;
                }
                catch { }
            }
            if (this._systemInfoService != null)
            {
                try { this._systemInfoService.Dispose(); } catch { }
            }
        }
    }
}
