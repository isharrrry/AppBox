using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Input
{
    public class UICommand : ICommand, INotifyPropertyChanged
    {
        private Action<object> executeAction;
        private Func<object, bool> canExecuteFunc;
        private Action closing;
        private Func<object, bool> value;
        bool isEnable = true;
        public bool IsEnabled { get => isEnable; set {
                isEnable = value;
                CanExecuteInvoke(null, null);
            } 
        }

        public event EventHandler CanExecuteChanged;
        public event PropertyChangedEventHandler PropertyChanged;


        public static UICommand Create(Action<object> execute, Func<object, bool> canExecute = null, EventHandler CanExecuteChanged = null)
        {
            var dg = new UICommand(execute, canExecute, CanExecuteChanged);
            return dg;
        }

        public UICommand(Action<object> execute, Func<object, bool> canExecute = null, EventHandler CanExecuteChanged = null)
        {
            if (execute == null)
            {
                return;
            }
            executeAction = execute;
            canExecuteFunc = canExecute;
            if (CanExecuteChanged != null)
            {
                this.CanExecuteChanged = CanExecuteChanged;
            }
        }
        public bool CanExecute(object parameter)
        {
            if (canExecuteFunc == null)
            {
                return IsEnabled;
            }
            return canExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            if (executeAction == null)
            {
                return;
            }
            executeAction(parameter);
        }
        public void CanExecuteInvoke(object sender, EventArgs e)
        {
            if(CanExecuteChanged != null)
                CanExecuteChanged(sender, e);
        }
    }
}
