using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Common;

namespace AppBoxInit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> Items { get => App.AppPathSet;  }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid label && label.DataContext is string path)
            {
                ProcessHelper.Start(Path.Combine(path, App.File), "", System.Diagnostics.ProcessWindowStyle.Normal, System.IO.Directory.GetCurrentDirectory());
                Environment.Exit(0);
            }
        }
    }
}