using System;
using System.Windows;
//using Window = System.Windows.Window;
using Window = HandyControl.Controls.Window;

namespace Common
{
    /// <summary>
    /// ModalWin.xaml 的交互逻辑
    /// </summary>
    public partial class ModalWin : Window
    {
        Action YesExec = null;
        Action NoExec = null;
        Action CancelExec = null;
        bool IsEnd = false;
        public ModalWin()
        {
            InitializeComponent();
            //Topmost = true;
            Loaded += ModalWin_Loaded;
            IsVisibleChanged +=ModalWin_IsVisibleChanged;
            SizeChanged +=ModalWin_SizeChanged;
            Closed +=ModalWin_Closed;
        }

        public void SetView(string msg, UIElement view, Func<string> YesCheck = null, Action YesExec = null, Action NoExec = null, Action CancelExec = null)
        {
            btYes.Visibility = (YesExec != null) ? Visibility.Visible : Visibility.Collapsed;
            btNo.Visibility = (NoExec != null) ? Visibility.Visible : Visibility.Collapsed;
            btCancel.Visibility = (CancelExec != null) ? Visibility.Visible : Visibility.Collapsed;
            this.YesExec = YesExec;
            this.NoExec = NoExec;
            this.CancelExec = CancelExec;
            btYes.Click += (object sender, RoutedEventArgs e) =>
            {
                if (YesCheck != null)
                {
                    var checkRes = YesCheck();
                    if (string.IsNullOrWhiteSpace(checkRes))
                        YesExec?.Invoke();
                    else
                    {
                        lbNotice.Visibility = Visibility.Visible;
                        lbNotice.Content = checkRes;
                        return;
                    }
                }
                else
                    YesExec?.Invoke();
                if (!IsEnd)
                {
                    IsEnd = true;
                    DialogResult = true;
                }
            };
            cpMain.Content = view;
        }

        private void cancelClick(object sender, RoutedEventArgs e)
        {
            if (!IsEnd)
            {
                IsEnd = true;
                DialogResult = null;
                Close();
                CancelExec?.Invoke();
            }
        }

        private void ModalWin_Closed(object? sender, EventArgs e)
        {
            if (!IsEnd)
            {
                IsEnd = true;
                CancelExec?.Invoke();
            }
        }

        private void noClick(object sender, RoutedEventArgs e)
        {
            if (!IsEnd)
            {
                IsEnd = true;
                DialogResult = false;
                NoExec?.Invoke();
            }
        }

        private void ModalWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsEnd)
            {
                CenterWindowOnScreen();
            }
        }

        private void ModalWin_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible && !IsEnd)
            {
                SizeToContent = SizeToContent.Manual;
                SizeToContent = SizeToContent.WidthAndHeight;
            }
        }
        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.ActualWidth;
            double windowHeight = this.ActualHeight;
            this.MaxHeight = screenHeight - (screenHeight / 5);
            this.MaxWidth = screenWidth;

            var scale = GetScreenScalingFactor();
            this.Left = (screenWidth - windowWidth) / 2 / scale;
            this.Top = (screenHeight - windowHeight) / 2 /scale;
        }

        public double GetScreenScalingFactor()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1.0; // 默认缩放比例为1.0
        }
        private void ModalWin_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
