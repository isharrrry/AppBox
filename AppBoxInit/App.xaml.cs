using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Common;

namespace AppBoxInit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<string> AppPathSet = new List<string>();
        public static string File = "AppBox.exe";
        protected override void OnStartup(StartupEventArgs e)
        {
            var paths = ProcessHelper.GetSubPathTreeList("./" , 3);
            AppPathSet = ProcessHelper.FindExeByPaths(paths, File, true);
            if (AppPathSet.Count > 0)
            {
                if(AppPathSet.Count == 1 && AppPathSet.First() is string path)
                {
                    ProcessHelper.Start(Path.Combine(path, File), "", System.Diagnostics.ProcessWindowStyle.Normal, System.IO.Directory.GetCurrentDirectory(), new EventHandler(eventHandler));
                    eventHandler(null, null);
                }
                else
                {
                    //选择一个启动

                }
            }
            else
            {
                MessageBox.Show("找不到主程序！");
                eventHandler(null, null);
            }
        }

        private void eventHandler(object sender, EventArgs e)
        {
            Environment.Exit(0);
            //Application.Current.Shutdown();
        }
    }

}
