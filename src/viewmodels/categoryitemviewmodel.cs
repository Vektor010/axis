using WinTweaker.Common;

namespace WinTweaker.ViewModels
{
    public class CategoryItemViewModel : ObservableObject
    {
        private string _id;
        private string _name;
        private string _iconGlyph;
        private int _count;
        private bool _isSelected;

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

        public string IconGlyph
        {
            get { return this._iconGlyph; }
            set { this._iconGlyph = value; this.OnPropertyChanged("IconGlyph"); }
        }

        public int Count
        {
            get { return this._count; }
            set { this._count = value; this.OnPropertyChanged("Count"); }
        }

        public bool IsSelected
        {
            get { return this._isSelected; }
            set { this._isSelected = value; this.OnPropertyChanged("IsSelected"); }
        }

        public CategoryItemViewModel(string id, string name, string iconGlyph, int count)
        {
            this._id = id;
            this._name = name;
            this._iconGlyph = iconGlyph;
            this._count = count;
            this._isSelected = false;
        }
    }
}
