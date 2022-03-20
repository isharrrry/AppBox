using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Common
{
    public class UriHelper 
    {
        static UriHelper()
        {
            AssemblyName = Assembly.GetEntryAssembly().GetName().Name;
        }
        public static string AssemblyName;
        public static string PackHead = "pack://";
        public static string UriFomart = @"pack://application:,,,/{0};component/{1}";
        /// <summary>
        /// 字符串相对路径转资源URI，不指定程序集名称则使用进入程序集
        /// </summary>
        /// <param name="rawUri"></param>
        /// <param name="assName"></param>
        /// <returns></returns>
        public static Uri GetBy(string rawUri, string assName = null)
        {
            Uri uri;
            // Allow for assembly overrides
            if (rawUri.StartsWith(PackHead))
            {
                uri = new Uri(rawUri);
            }
            else
            {
                string assemblyName = assName ?? AssemblyName ?? ""; 
                uri = new Uri(string.Format(UriFomart, assemblyName, rawUri.TrimStart('/')), UriKind.Absolute);
            }
            return uri;
        }

    }
}
