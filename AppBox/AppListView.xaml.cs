using HandyControl.Tools;
//using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppBox
{
    /// <summary>
    /// AppListView.xaml 的交互逻辑
    /// </summary>
    public partial class AppListView : UserControl
    {
        public AppListView()
        {
            InitializeComponent();
            Drop +=AppListView_Drop;
        }

        private void AppListView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string fileName = GetDropFile(e);

                if (DataContext is ConfigAppList cfg)
                {
                    cfg.Drog(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private static string GetDropFile(DragEventArgs e)
        {
            return ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {

        }

        private void AppItem_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (Window.GetWindow(this) is Window win)
                    win.Focus();
                string fileName = GetDropFile(e);
                if (DataContext is ConfigAppList cfg 
                    && (sender as Grid).DataContext is AppItem app)
                {
                    cfg.Drog(fileName, app);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (Window.GetWindow(this) is Window win2)
                win2.Focus();
        }
    }
}
