
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Common
{
    public interface IModalVM : IDisposable
    {
        void AskSelect<T>(string Ask, IList<T> Items, Func<T, string> AllowYes, T AckDefault = default, string title = null, Action YesExec = null, Action CancelExec = null);
        void AskText(string Ask, string AckDefault = null, string title = null, Action<string> YesExec = null, Action NoExec = null, Action CancelExec = null);
        void Busy(string msg, Action work, string doneMsg = null, bool autoClose = false);
        void Confirm(string msg, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null, MessageBoxButton yesNo = MessageBoxButton.YesNoCancel, MessageBoxImage question = MessageBoxImage.Question);
        void ObjectConfirm<T>(string msg, T inputObj, Func<T, string> AllowYes, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null);
        void Show(string msg, string title = null, MessageBoxButton yesNo = MessageBoxButton.OK, MessageBoxImage question = MessageBoxImage.None);
    }
}
