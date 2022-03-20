using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Controls;
using Window = HandyControl.Controls.Window;
namespace AppBox.Ext
{
    public class ConfigWindow
    {
        public static void Config(object Value, bool ShowDialog = false, bool IsNamelessSpik = true, bool IsInheritSpik = false)
        {
            var win = new Window() {
                Title = Value.GetType().Name,
                Width = 400,
                Height = 500,
            };
            var pd = new PropertyGrid();
            pd.SelectedObject = Value;
            win.Content = pd;
            if (ShowDialog)
                win.ShowDialog();
            else
                win.Show();
        }
    }
}
