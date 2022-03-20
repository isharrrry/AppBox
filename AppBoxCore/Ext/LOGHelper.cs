using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    public enum LogType { 
        /// <summary>
        /// 流程相关的信息
        /// </summary>
        LOG, 
        /// <summary>
        /// 频繁输出的信息，非重要
        /// </summary>
        DEBUG,
        /// <summary>
        /// 重要数据的信息
        /// </summary>
        INFO, 
        /// <summary>
        /// 稳定性的信息
        /// </summary>
        WARNING,
        /// <summary>
        /// 严重错误的信息
        /// </summary>
        ERROR
    };
    /// <summary>
    /// 日志扩展
    /// </summary>
    public static class LOGHelperExtensions
    {
        public static UdpClient LogClient = new UdpClient();
        public static IPEndPoint LOGEP = new IPEndPoint(IPAddress.Parse("224.239.239.239"), 21705);
        public static Action<object, LogType> LogCall = LogToUDP;
        public static void LogToUDP(object logInfo, LogType type = LogType.LOG)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes($"[{type.ToString()}]{logInfo.ToString()}");
                LogClient.Send(bytes, bytes.Length, LOGEP);
            }
            catch
            {
            }
        }
        public static void Log(this object referrer, object logInfo, LogType type = LogType.LOG)
        {
          
            LogCall(logInfo, type);
        }

        /// <summary>
        /// 严重错误的信息
        /// </summary>
        /// <param name="referrer"></param>
        /// <param name="logInfo"></param>
        public static void LogErr(this object referrer, object logInfo)
        {
            LogCall(logInfo, LogType.ERROR);
        }

        /// <summary>
        /// 重要数据的信息
        /// </summary>
        /// <param name="referrer"></param>
        /// <param name="logInfo"></param>
        public static void LogInfo(this object referrer, object logInfo)
        {
            LogCall(logInfo, LogType.INFO);
        }

        public static string CreateLogText(string text, bool TimeHead = true, bool TimeNewLine = false, bool NewLine = true)
        {
            return string.Format(
                            "{0}{1}{2}{3}",
                            TimeHead ? DateTime.Now.ToString("HH:mm:ss.fff") : "",
                            TimeNewLine ? Environment.NewLine : " ",
                            text,
                            NewLine ? Environment.NewLine : ""
                        );
        }
    }

}
