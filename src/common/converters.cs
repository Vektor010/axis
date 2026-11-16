using System;
using System.Globalization;
using System.Windows.Data;
using WinTweaker.Models;
using WinTweaker.ViewModels;

namespace WinTweaker.Common
{
    public class GroupToEnabledCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CollectionViewGroup group = value as CollectionViewGroup;
            if (group == null || group.Items == null) return "включено 0";
            int enabled = 0;
            for (int i = 0; i < group.Items.Count; i++)
            {
                TweakCardViewModel tw = group.Items[i] as TweakCardViewModel;
                if (tw != null && tw.Status == TweakStatus.Enabled)
                {
                    enabled++;
                }
            }
            return string.Format("включено {0}", enabled);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            return flag ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Visibility)
            {
                return (System.Windows.Visibility)value != System.Windows.Visibility.Visible;
            }
            return false;
        }
    }
}
