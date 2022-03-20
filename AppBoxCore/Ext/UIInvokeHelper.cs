using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class UIInvokeHelper
    {
        public static Action<Action> InvokeExec { get; set; } = x => { x(); };

        public static void Invoke(Action action) {
            InvokeExec(action);
        }
    }
}
