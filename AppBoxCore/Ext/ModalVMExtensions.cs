
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common
{

    public static class ModalVMExtensions
    {
        public static IModalVM ModalVM { get; set; } = new ConsoleModalVM();
        /// <summary>
        /// 需要获取返回值则需要先调用SetMessageCallback，在回调中处理结果
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <param name="yesNo"></param>
        /// <param name="question"></param>
        public static void MessageBoxShow(this object referrer, string msg, string title = null, MessageBoxButton yesNo = default, MessageBoxImage question = default, bool NewModalDisplay = true)
        {
            ModalVM.Show(msg, title, yesNo, question);
        }

        public static void MessageBoxConfirm(this object referrer, string msg, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null)
        {
            ModalVM.Confirm(msg, title, YesExec, NoExec, CancelExec);
        }

        public static void MessageBoxObjectConfirm<T>(this object referrer, string msg, T inputObj, Func<T, string> checkObj, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null)
        {
            ModalVM.ObjectConfirm(msg, inputObj, checkObj, title, YesExec, NoExec, CancelExec);
        }

        public static void MessageBoxAskText(this object referrer, string Ask, Action<string> YesExec = null, Action CancelExec = null, Action NoExec = null)
        {
            referrer.MessageBoxAskText(Ask, null, null, YesExec, CancelExec, NoExec);
        }
        public static void MessageBoxBusy(this object referrer, string msg, Action work, string doneMsg = null, bool autoClose = false)
        {
            ModalVM.Busy(msg, work, doneMsg, autoClose);
        }

        public static void MessageBoxAskText(this object referrer, string Ask, string AckDefault = null, string title = null, Action<string> YesExec = null, Action CancelExec = null, Action NoExec = null)
        {
            ModalVM.AskText(Ask, AckDefault, title, YesExec, NoExec, CancelExec);
        }
        public static void MessageBoxAskSelect<T>(this object referrer, string Ask, IList<T> Items, Func<T, string> checkObj, T AckDefault = default, string title = null, Action YesExec = null, Action CancelExec = null)
        {
            ModalVM.AskSelect(Ask, Items, checkObj, AckDefault, title, YesExec, CancelExec);
        }
    }
}
