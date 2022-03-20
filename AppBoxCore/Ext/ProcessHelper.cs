using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ProcessHelper
    {

        public static bool OpenFileByWs(string filePath, bool UseShellExecute = true, string args = null, string WorkingDirectory = null)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    filePath = filePath.Replace("/", "\\");
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = UseShellExecute,
                    Arguments = args ?? "",
                };
                if (WorkingDirectory == null)
                {
                    psi.WorkingDirectory = Path.GetDirectoryName(filePath);
                    psi.FileName = (filePath);
                }
                else
                    psi.WorkingDirectory = WorkingDirectory;
                Process.Start(psi);
            }
            catch (Exception e)
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))   //windows环境下打开文件
                    {
                        filePath = filePath.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start \"\" \"{filePath}\"") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))  //Linux环境下打开文件
                    {
                        Process.Start("xdg-open", filePath);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))  //Mac环境下打开文件
                    {
                        Process.Start("open", filePath);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public static void OpenFileWhereDir(string uri)
        {
            System.Diagnostics.Process.Start("Explorer", "/select,"+ uri);
        }

    }
}
