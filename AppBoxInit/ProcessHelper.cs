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
        public static List<string> GetSubPathTreeList(string initPath, int deep = 1)
        {
            var ls = new List<string> { };
            try
            {
                {
                    string[] dirs = Directory.GetDirectories(initPath);
                    foreach (var dir in dirs)
                    {
                        if (!DirectoryPathEquals(dir))
                        {
                            ls.Add(dir);
                            if (deep > 1)
                            {
                                ls.AddRange(GetSubPathTreeList(dir, deep - 1));
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            return ls;
        }

        public static bool DirectoryPathEquals(string dir, string dir2 = "./")
        {
            var d1 = Path.GetFullPath(dir);
            if (d1.EndsWith("/"))
                d1 = d1.Substring(0, d1.Length - 1);
            if (d1.EndsWith("\\"))
                d1 = d1.Substring(0, d1.Length - 1);
            var d2 = Path.GetFullPath(dir2);
            if (d2.EndsWith("/"))
                d2 = d2.Substring(0, d2.Length - 1);
            if (d2.EndsWith("\\"))
                d2 = d2.Substring(0, d2.Length - 1);
            return d1 == d2;
        }

        /// <summary>
        /// 路径变量找到提供exe的路径
        /// </summary>
        /// <param name="pathStr"></param>
        /// <param name="exe"></param>
        /// <returns></returns>
        public static List<string> FindExeByPaths(IList<string> paths, string exe, bool all = true)
        {
            var res = new List<string> { };
            foreach (var item in paths)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;
                if (File.Exists(Path.Combine(item, exe)))
                {
                    res.Add(item);
                    if (!all)
                    {
                        return res;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// 如果包含eventHandler则阻塞等待程序结束，并执行回调
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Arguments"></param>
        /// <param name="WindowStyle"></param>
        /// <param name="eventHandler"></param>
        public static void Start(string FileName, string Arguments = "", ProcessWindowStyle WindowStyle = ProcessWindowStyle.Normal, string WorkingDirectory = null, EventHandler eventHandler = null)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = FileName;
            info.Arguments = Arguments;
            info.WindowStyle = ProcessWindowStyle.Normal;
            if (WorkingDirectory != null)
                info.UseShellExecute = true;
            info.WorkingDirectory = WorkingDirectory;
            Process _process = Process.Start(info);
            if (eventHandler != null)
            {
                _process.EnableRaisingEvents = true;
                _process.Exited += new EventHandler(eventHandler);
                _process.WaitForExit(2000);
            }
        }
    }
}
