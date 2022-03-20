
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Common
{
    public enum MessageBoxButton
    {
        OK,
        YesNo,
        YesNoCancel = 3,
        OKCancel = 4,
    }
    public enum MessageBoxImage
    {
        //
        // 摘要:
        //     The message box contains no symbols.
        None = 0,
        //
        // 摘要:
        //     The message box contains a symbol consisting of white X in a circle with a red
        //     background.
        Error = 16,
        //
        // 摘要:
        //     The message box contains a symbol consisting of a white X in a circle with a
        //     red background.
        Hand = 16,
        //
        // 摘要:
        //     The message box contains a symbol consisting of white X in a circle with a red
        //     background.
        Stop = 16,
        //
        // 摘要:
        //     The message box contains a symbol consisting of a question mark in a circle.
        //     The question mark message icon is no longer recommended because it does not clearly
        //     represent a specific type of message and because the phrasing of a message as
        //     a question could apply to any message type. In addition, users can confuse the
        //     question mark symbol with a help information symbol. Therefore, do not use this
        //     question mark symbol in your message boxes. The system continues to support its
        //     inclusion only for backward compatibility.
        Question = 32,
        //
        // 摘要:
        //     The message box contains a symbol consisting of an exclamation point in a triangle
        //     with a yellow background.
        Exclamation = 48,
        //
        // 摘要:
        //     The message box contains a symbol consisting of an exclamation point in a triangle
        //     with a yellow background.
        Warning = 48,
        //
        // 摘要:
        //     The message box contains a symbol consisting of a lowercase letter i in a circle.
        Asterisk = 64,
        //
        // 摘要:
        //     The message box contains a symbol consisting of a lowercase letter i in a circle.
        Information = 64
    }
    public enum MessageBoxResult
    {
        Cancel,
        Yes,
        No,
    }
    public class ConsoleModalVM : INotifyPropertyChanged, IDisposable, IModalVM
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public ConsoleModalVM()
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual void AskSelect<T>(string Ask, IList<T> Items, Func<T, string> AllowYes, T AckDefault = default, string title = null, Action YesExec = null, Action CancelExec = null)
        {
            Console.WriteLine($"[{title}]{Ask}");
        }

        public virtual void AskText(string Ask, string AckDefault = null, string title = null, Action<string> YesExec = null, Action NoExec = null, Action CancelExec = null)
        {
            Console.WriteLine($"[{title}]{Ask}");
        }

        public virtual void Busy(string msg, Action work, string doneMsg = null, bool autoClose = false)
        {
            Console.WriteLine($"[{doneMsg}]{msg}");
        }

        public virtual void Confirm(string msg, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null, MessageBoxButton yesNo = MessageBoxButton.YesNoCancel, MessageBoxImage question = MessageBoxImage.Question)
        {
            Console.WriteLine($"[{title}]{msg}");
        }

        public virtual void Show(string msg, string title = null, MessageBoxButton yesNo = MessageBoxButton.OK, MessageBoxImage question = MessageBoxImage.None)
        {
            Console.WriteLine($"[{title}]{msg}");
        }

        public virtual void ObjectConfirm<T>(string msg, T inputObj, Func<T, string> AllowYes, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null)
        {
            Console.WriteLine($"[{title}]{msg}");
        }
    }

    public class ListSelectVM<T>
    {
        public virtual T Selected { get; set; }
        public virtual IList<T> Items { get; set; }
        public string Notice { get; set; }
    }
}