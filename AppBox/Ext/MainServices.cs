
using System;
using System.Threading.Tasks;
using System.Windows;
using Common;

namespace AppBox.Ext
{
    public class MainServices
    {

        #region 异常捕获
        public static void InitCapExption(AppDomain CurrentDomain, Application Current)
        {
            HandleExceptionHandle = HandleException;
            CurrentDomain.Log($"[软件启动]{CurrentDomain.FriendlyName}");
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //UI线程未捕获异常处理事件（UI主线程）
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            HandleExceptionHandle(e.Exception, "[UnobservedTaskException]");
        }

        private static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleExceptionHandle(e.Exception, "[UI]");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleExceptionHandle(e.ExceptionObject as Exception, "[UnhandledException]");
        }

        public delegate void HandleExceptionEvent(Exception exception, string type);
        public static HandleExceptionEvent HandleExceptionHandle;
        static bool IsHandling = false;
        static bool IsFatalAbnormalState = false;
        public static void HandleException(Exception exception, string type)
        {
            exception.Log(exception, LogType.ERROR);
            if (exception is OutOfMemoryException)
                exception.LogInfo("警报：内存申请致命异常，需要重启软件！！！");
#if DEBUG
            if (IsHandling == false)//防止同时弹多个
            {
                IsHandling = true;
                MessageBox.Show(exception.ToString(), $"{type}未预料异常！");
                IsHandling = false;
            }
#else
#endif
        }
        #endregion
    }
}
