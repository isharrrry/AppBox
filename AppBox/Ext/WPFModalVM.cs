
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;
using TextBox = HandyControl.Controls.TextBox;

namespace Common
{
    public class WPFModalWinVM : ConsoleModalVM, IDisposable
    {
        public static int Width = 400;
        public override void Confirm(string msg, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null, MessageBoxButton yesNo = MessageBoxButton.YesNoCancel, MessageBoxImage question = MessageBoxImage.Question)
        {
            System.Windows.MessageBoxButton button = (System.Windows.MessageBoxButton)yesNo;
            System.Windows.MessageBoxImage icon = (System.Windows.MessageBoxImage)question;
            var res = MessageBox.Show(msg, title ?? "提示", button, icon);
            if (res == System.Windows.MessageBoxResult.OK || res == System.Windows.MessageBoxResult.Yes)
                YesExec?.Invoke();
            else if (res == System.Windows.MessageBoxResult.No)
                NoExec?.Invoke();
            else if (res == System.Windows.MessageBoxResult.Cancel)
                CancelExec?.Invoke();
        }
        public override void Show(string msg, string title = null, MessageBoxButton yesNo = default, MessageBoxImage question = default)
        {
            Confirm(msg, title, null, null, null, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public override void ObjectConfirm<T>(string msg, T inputObj, Func<T, string> AllowYes, string title = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null)
        {
            var ModalWin = new ModalWin();
            ModalWin.Title = title ?? "配置";
            PropertyGrid configObject = new PropertyGrid()
            {
                SelectedObject = inputObj,
                MinWidth = Width,
            };
            ModalWin.SetView(msg, configObject, () => AllowYes(inputObj), YesExec, NoExec, CancelExec);
            ModalWin.ShowDialog();
        }

        public override void AskText(string Ask, string AckDefault = null, string title = null, Action<string> YesExec = null, Action NoExec = null, Action CancelExec = null)
        {
            var ModalWin = new ModalWin();
            ModalWin.Title = title ?? "请输入";
            var gd = new Grid();
            gd.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            gd.RowDefinitions.Add(new RowDefinition());
            var header = new TextBlock() { 
                Text = Ask,
                Margin = new Thickness(5)
            };
            var ipt = new TextBox()
            {
                Text = AckDefault ?? "",
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5)
            };
            if(!string.IsNullOrWhiteSpace(Ask))
                gd.Children.Add(header);
            gd.Children.Add(ipt);
            Grid.SetRow(header, 0);
            Grid.SetRow(ipt, 1);
            ModalWin.SetView(Ask, gd,
                null, () => YesExec?.Invoke(ipt.Text), NoExec, CancelExec);
            ModalWin.ShowDialog();
        }

        public override void AskSelect<T>(string Ask, IList<T> Items, Func<T, string> AllowYes, T AckDefault = default, string title = null, Action YesExec = null, Action CancelExec = null)
        {
            var ModalWin = new ModalWin();
            ModalWin.Title = title ?? "请输入";

            ListSelectVM<T> ListSelectVM = new ListSelectVM<T>();
            ListSelectVM.Items = Items;
            ListSelectVM.Notice = Ask;
            if (AckDefault != null)
                ListSelectVM.Selected = AckDefault;
            var view = new StringListView(ListSelectVM)
            {
                MaxHeight = 400,
                Margin = new Thickness(5)
            };

            ModalWin.SetView(
                Ask, view,
                () => { return ListSelectVM.Selected == null ? "请选择条目！" : AllowYes(ListSelectVM.Selected); },
                YesExec, null, CancelExec);
            ModalWin.ShowDialog();
        }

        public override void Busy(string msg, Action work, string doneMsg = null, bool autoClose = false)
        {
            var ModalWin = new ModalWin();
            ModalWin.Title = "处理中...";
            var view = new LoadingLine()
            {
                Margin = new Thickness(5)
            };
            ModalWin.SetView(msg, view);
            view.Loaded += (s, e) =>
            {
                Thread.Sleep(100);
                new Task(() => {
                    try
                    {
                        work();
                    }
                    catch (Exception ex)
                    {
                        ex.LogErr(ex);
                        doneMsg = ex.Message.ToString();
                    }
                    UIInvokeHelper.Invoke(() => {
                        ModalWin.Close();
                        if (autoClose == true && string.IsNullOrWhiteSpace(doneMsg))
                        { }
                        else
                            Show(string.IsNullOrWhiteSpace(doneMsg) ? "完成" : doneMsg);
                    });
                }).Start();
            };
            ModalWin.Show();
        }

    }

}
