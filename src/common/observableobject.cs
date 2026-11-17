using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WinTweaker.Common
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Высокопроизводительная коллекция с поддержкой пакетного обновления (ReplaceAll) без лишних UI-инвалидаций.
    /// </summary>
    public class BatchObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification;

        public void ReplaceAll(IEnumerable<T> items)
        {
            this._suppressNotification = true;
            try
            {
                this.Items.Clear();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        this.Items.Add(item);
                    }
                }
            }
            finally
            {
                this._suppressNotification = false;
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!this._suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!this._suppressNotification)
            {
                base.OnPropertyChanged(e);
            }
        }
    }
}
