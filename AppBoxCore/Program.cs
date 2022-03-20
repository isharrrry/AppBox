using System;
using Common;

namespace AppBox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Common.LOGHelperExtensions.LogCall = LogCall;
            UIInvokeHelper.InvokeExec = call => { call?.Invoke(); };
            ModalVMExtensions.ModalVM = new ConsoleModalVM();

            var vm = new AppBox.MainVM(true);
            Console.WriteLine($"AppBox Server Running!");
            while (true)
            {
                Console.Read();
            }
        }

        private static void LogCall(object info, LogType type)
        {
            Console.Write($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")} [{type}]");
            Console.WriteLine(info);
        }
    }
}
