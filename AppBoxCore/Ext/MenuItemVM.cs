using Common;
using System.ComponentModel;
using System.Windows.Input;

namespace Common
{

    public class MenuItemVM : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public object Icon { get; set; }
        public ICommand CMD { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}