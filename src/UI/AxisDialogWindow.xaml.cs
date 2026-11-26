using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinTweaker.Common;
using WinTweaker.ViewModels;

namespace WinTweaker.UI
{
    public enum AxisDialogKind
    {
        Info,
        Success,
        Question,
        Warning,
        Error,
        Details
    }

    public partial class AxisDialogWindow : Window
    {
        private readonly Action _secondaryAction;
        private readonly Action _primaryAction;

        public AxisDialogWindow()
            : this(AxisDialogKind.Info, "AXIS", null, "", null, null, "ОК", null, null)
        {
        }

        private AxisDialogWindow(
            AxisDialogKind kind,
            string title,
            string subtitle,
            string message,
            IEnumerable<KeyValuePair<string, string>> metadata,
            IEnumerable<string> items,
            string primaryText,
            string cancelText,
            string secondaryText,
            bool primaryIsDanger = false,
            Action primaryAction = null,
            Action secondaryAction = null)
        {
            InitializeComponent();
            this._primaryAction = primaryAction;
            this._secondaryAction = secondaryAction;

            this.TitleText.Text = string.IsNullOrEmpty(title) ? "AXIS" : title;
            if (!string.IsNullOrEmpty(subtitle))
            {
                this.SubtitleText.Text = subtitle;
                this.SubtitleText.Visibility = Visibility.Visible;
            }
            this.MessageText.Text = message ?? "";

            this.ConfigureKind(kind);

            if (metadata != null)
            {
                bool anyMeta = false;
                foreach (KeyValuePair<string, string> kv in metadata)
                {
                    anyMeta = true;
                    Grid row = new Grid();
                    row.Margin = new Thickness(0, 2, 0, 2);
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    TextBlock kBlock = new TextBlock
                    {
                        Text = kv.Key + ":",
                        FontSize = 11.5,
                        Foreground = (Brush)this.TryFindResource("TextSecondary") ?? Brushes.Gray,
                        Margin = new Thickness(0, 0, 12, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    TextBlock vBlock = new TextBlock
                    {
                        Text = kv.Value ?? "—",
                        FontSize = 11.5,
                        FontWeight = FontWeights.Medium,
                        Foreground = (Brush)this.TryFindResource("TextPrimary") ?? Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        TextAlignment = TextAlignment.Right,
                        TextWrapping = TextWrapping.Wrap
                    };
                    Grid.SetColumn(kBlock, 0);
                    Grid.SetColumn(vBlock, 1);
                    row.Children.Add(kBlock);
                    row.Children.Add(vBlock);
                    this.MetadataPanel.Children.Add(row);
                }
                if (anyMeta) this.MetadataBorder.Visibility = Visibility.Visible;
            }

            if (items != null)
            {
                List<string> list = new List<string>(items);
                if (list.Count > 0)
                {
                    this.ItemsList.ItemsSource = list;
                    this.ItemsBorder.Visibility = Visibility.Visible;
                }
            }

            this.PrimaryButton.Content = string.IsNullOrEmpty(primaryText) ? "ОК" : primaryText;
            if (primaryIsDanger)
            {
                Style dangerStyle = this.TryFindResource("DangerButtonStyle") as Style;
                if (dangerStyle != null) this.PrimaryButton.Style = dangerStyle;
            }

            if (!string.IsNullOrEmpty(cancelText))
            {
                this.CancelButton.Content = cancelText;
                this.CancelButton.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(secondaryText))
            {
                this.SecondaryButton.Content = secondaryText;
                this.SecondaryButton.Visibility = Visibility.Visible;
            }

            this.PreviewKeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    this.DialogResult = false;
                    this.Close();
                }
            };
        }

        private void ConfigureKind(AxisDialogKind kind)
        {
            string glyph = "\uE946";
            string brushKey = "AccentBrush";
            string fillKey = "AccentFill";
            string borderKey = "AccentBorder";

            switch (kind)
            {
                case AxisDialogKind.Success:
                    glyph = "\uE73E";
                    brushKey = "SuccessBrush";
                    fillKey = "SuccessFill";
                    borderKey = "SuccessBorder";
                    break;
                case AxisDialogKind.Question:
                    glyph = "\uE897";
                    break;
                case AxisDialogKind.Warning:
                    glyph = "\uE7BA";
                    brushKey = "WarningBrush";
                    fillKey = "WarningFill";
                    borderKey = "WarningBorder";
                    break;
                case AxisDialogKind.Error:
                    glyph = "\uEA39";
                    brushKey = "DangerBrush";
                    fillKey = "DangerFill";
                    borderKey = "DangerBorder";
                    break;
                case AxisDialogKind.Details:
                    glyph = "\uE9F9";
                    break;
            }

            this.IconGlyphText.Text = glyph;
            Brush fg = this.TryFindResource(brushKey) as Brush;
            Brush bg = this.TryFindResource(fillKey) as Brush;
            Brush bd = this.TryFindResource(borderKey) as Brush;
            if (fg != null) this.IconGlyphText.Foreground = fg;
            if (bg != null) this.IconBadgeBorder.Background = bg;
            if (bd != null) this.IconBadgeBorder.BorderBrush = bd;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { this.DragMove(); } catch { }
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._primaryAction != null)
            {
                this._primaryAction();
            }
            this.DialogResult = true;
            this.Close();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._secondaryAction != null)
            {
                this._secondaryAction();
            }
            this.DialogResult = false;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private static Window ResolveOwner()
        {
            if (Application.Current == null) return null;
            Window mw = Application.Current.MainWindow;
            if (mw != null && mw.IsLoaded && mw.IsVisible) return mw;
            return null;
        }

        public static void ShowInfo(string title, string message, string subtitle = null, IEnumerable<string> items = null)
        {
            if (Application.Current == null) return;
            AxisDialogWindow dlg = new AxisDialogWindow(
                AxisDialogKind.Info, title, subtitle, message, null, items, "Закрыть", null, null);
            Window owner = ResolveOwner();
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
        }

        public static void ShowWarning(string title, string message, string subtitle = null, IEnumerable<string> items = null)
        {
            if (Application.Current == null) return;
            AxisDialogWindow dlg = new AxisDialogWindow(
                AxisDialogKind.Warning, title, subtitle, message, null, items, "Понятно", null, null);
            Window owner = ResolveOwner();
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
        }

        public static void ShowError(string title, string message, string subtitle = null)
        {
            if (Application.Current == null) return;
            AxisDialogWindow dlg = new AxisDialogWindow(
                AxisDialogKind.Error, title, subtitle, message, null, null, "Закрыть", null, null);
            Window owner = ResolveOwner();
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
        }

        public static void ShowBatchReport(string title, string subtitle, string message, IEnumerable<string> items, bool hasErrors)
        {
            if (Application.Current == null) return;
            AxisDialogWindow dlg = new AxisDialogWindow(
                hasErrors ? AxisDialogKind.Warning : AxisDialogKind.Success,
                title, subtitle, message, null, items, "Готово", null, null);
            Window owner = ResolveOwner();
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
        }

        public static bool ShowConfirm(
            string title,
            string subtitle,
            string message,
            IEnumerable<string> items = null,
            string confirmText = "Подтвердить",
            string cancelText = "Отмена",
            bool isDanger = false)
        {
            if (Application.Current == null) return false;
            AxisDialogWindow dlg = new AxisDialogWindow(
                isDanger ? AxisDialogKind.Warning : AxisDialogKind.Question,
                title, subtitle, message, null, items, confirmText, cancelText, null, isDanger);
            Window owner = ResolveOwner();
            if (owner != null) dlg.Owner = owner;
            bool? res = dlg.ShowDialog();
            return res == true;
        }

        public static bool ShowBatchConfirmWithGuard(
            string title,
            string subtitle,
            string message,
            IEnumerable<string> items,
            string guardCheckBoxText,
            out bool guardChecked,
            string confirmText = "Применить",
            string cancelText = "Отмена")
        {
            guardChecked = false;
            if (Application.Current == null) return false;
            AxisDialogWindow dlg = new AxisDialogWindow(
                string.IsNullOrEmpty(guardCheckBoxText) ? AxisDialogKind.Question : AxisDialogKind.Warning,
                title, subtitle, message, null, items, confirmText, cancelText, null, false);
            if (!string.IsNullOrEmpty(guardCheckBoxText))
            {
                dlg.GuardOptionCheckBox.Content = guardCheckBoxText;
                dlg.GuardOptionBorder.Visibility = Visibility.Visible;
            }
            Window owner = ResolveOwner();
            if (owner != null) dlg.Owner = owner;
            bool? res = dlg.ShowDialog();
            guardChecked = dlg.GuardOptionCheckBox.IsChecked == true;
            return res == true;
        }

        public static void ShowTweakDetails(TweakCardViewModel card)
        {
            if (Application.Current == null || card == null) return;

            List<KeyValuePair<string, string>> meta = new List<KeyValuePair<string, string>>();
            meta.Add(new KeyValuePair<string, string>("Категория", card.CategoryDisplayName));
            meta.Add(new KeyValuePair<string, string>("Рекомендован", card.IsRecommended ? "Да (безопасный профиль)" : "Дополнительный / профильный"));
            meta.Add(new KeyValuePair<string, string>("Текущее в системе", card.LiveSystemValueText));
            meta.Add(new KeyValuePair<string, string>("Состояние", card.OptimizationStatusText));
            meta.Add(new KeyValuePair<string, string>("Целевое значение", card.RecommendedValueText));
            meta.Add(new KeyValuePair<string, string>("Ограничения и ОС", card.CompatibilityText));
            meta.Add(new KeyValuePair<string, string>("Перезагрузка", card.RebootRequirementText));
            meta.Add(new KeyValuePair<string, string>("Откат", card.RollbackCapabilityText));
            meta.Add(new KeyValuePair<string, string>("Путь реестра", card.RegistryPath));
            meta.Add(new KeyValuePair<string, string>("Параметр", card.RegistryName));
            meta.Add(new KeyValuePair<string, string>("Источник", card.SourceLocation));

            string primaryBtn = card.IsActionTweak ? card.DetailsActionButtonText : "Применить";
            string revertBtn = (!card.IsActionTweak && card.CanRevert) ? "Откатить" : null;

            Action onPrimary = delegate
            {
                if (card.IsActionTweak && card.ExecuteActionCommand != null && card.ExecuteActionCommand.CanExecute(null))
                {
                    card.ExecuteActionCommand.Execute(null);
                }
                else if (card.ApplyTweakCommand != null && card.ApplyTweakCommand.CanExecute(null))
                {
                    card.ApplyTweakCommand.Execute(null);
                }
            };

            Action onSecondary = delegate
            {
                if (card.RevertTweakCommand != null && card.RevertTweakCommand.CanExecute(null))
                {
                    card.RevertTweakCommand.Execute(null);
                }
            };

            AxisDialogWindow dlg = new AxisDialogWindow(
                AxisDialogKind.Details,
                card.Name,
                string.Format("{0} • ID: {1}", card.CategoryDisplayName, card.Id),
                card.ExpandedDescription,
                meta,
                null,
                primaryBtn,
                "Закрыть",
                revertBtn,
                false,
                onPrimary,
                onSecondary);
            Window owner = ResolveOwner();
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
        }
    }
}
