using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Common;
using Window = HandyControl.Controls.Window;
using AppBox.Ext;

namespace AppBox
{
    /// <summary>
    /// MoreWin.xaml 的交互逻辑
    /// </summary>
    public partial class MoreWin : Window
    {
        public static ConfigAppBox ConfigAppBox = new ConfigAppBox();
        public string Version { get => "版本：" + Assembly.GetExecutingAssembly().GetName().Version.ToString(); }

        public MoreWin()
        {
            InitializeComponent();
            Loaded +=MoreWin_Loaded;
        }

        private void MoreWin_Loaded(object sender, RoutedEventArgs e)
        {
            txtLocal.Text = string.Join("\n", ConfigAppBox.ConfigWorkSpaceDir.LocalURIs);
            txtRemote.Text = string.Join("\n", ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs);
        }

        private void 深色(object sender, RoutedEventArgs e)
        {
            SwicthColor();
            DialogResult = true;
        }

        private void 浅色(object sender, RoutedEventArgs e)
        {
            SwicthColor("Default");
            DialogResult = true;
        }
        bool StyleDrak = false;
        private void 切换主题(object sender, RoutedEventArgs e)
        {
            if (StyleDrak)
            {
                SwicthColor("Default");
                StyleDrak = false;
            }
            else
            {
                SwicthColor();
                StyleDrak = true;
            }

        }
        public static object SwicthColor(string ColorName = "Drak")
        {
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
            {
                Source = UriHelper.GetBy(
                    ColorName == "Drak" ? @"Themes/Basic/Colors/ColorsDark.xaml" : @"Themes/Basic/Colors/Colors.xaml", "HandyControl")
            };
            Application.Current.Resources.MergedDictionaries[1] = new ResourceDictionary()
            {
                Source = UriHelper.GetBy(@"Themes/Theme.xaml", "HandyControl")
            };
            if(ConfigAppBox.Theme != ColorName)
            {
                ConfigAppBox.Theme = ColorName;
                YmlHelper.Save(ConfigAppBox.ConfigPath, ConfigAppBox);
            }
            return Application.Current.Resources;
        }

        private void 保存(object sender, RoutedEventArgs e)
        {
            string locals = txtLocal.Text;
            string remotes = txtRemote.Text;
            var localURIs = locals.Trim().Split("\n");
            var remoteURIs = remotes.Trim().Split("\n");
            ConfigAppBox.ConfigWorkSpaceDir.LocalURIs.Clear();
            foreach (var item in localURIs)
            {
                ConfigAppBox.ConfigWorkSpaceDir.LocalURIs.Add(item.Trim());
            }
            ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs.Clear();
            foreach (var item in remoteURIs)
            {
                ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs.Add(item.Trim());
            }
            YmlHelper.Save(ConfigAppBox.ConfigPath, ConfigAppBox);
            DialogResult = true;
        }

        private void ng(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = ((Hyperlink)sender).NavigateUri.ToString() });
        }
    }
}
