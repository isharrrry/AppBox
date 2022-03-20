
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Common
{
    public class UISelectFileHelperVoid : ISelectFileHelper
    {
        public string OpenDir(string InitialDirectory = "")
        {
            return null;
        }

        public string OpenFile(string Filter = "(*.xml)|*.xml", string name = "", string InitialDirectory = null)
        {
            return null;
        }

        public string SaveFile(string Filter = "(*.xml)|*.xml", string nameHead = "", bool appendNameTime = true, string InitialDirectory = null)
        {
            return null;
        }
    }
    public class UISelectFileHelper
    {
        public static ISelectFileHelper SelectFileHelper = new UISelectFileHelperVoid();
        public static string OpenDir(string InitialDirectory = "")
        {
            return SelectFileHelper.OpenDir(InitialDirectory);
        }

        public static string OpenFile(string Filter = "(*.xml)|*.xml", string name = "", string InitialDirectory = null)
        {
            return SelectFileHelper.OpenFile(Filter, name, InitialDirectory);
        }

        public static string SaveFile(string Filter = "(*.xml)|*.xml", string nameHead = "", bool appendNameTime = true, string InitialDirectory = null)
        {
            return SelectFileHelper.SaveFile(Filter, nameHead, appendNameTime, InitialDirectory);
        }
        public static string SaveFileInitialDirectory(string Filter = "(*.xml)|*.xml", string nameHead = "", string InitialDirectory = null)
        {
            return SelectFileHelper.SaveFile(Filter, nameHead, true, InitialDirectory);
        }
        public static string StarFilter = "(*.*)|*.*";
        public static string XmlFilter = "(*.xml)|*.xml";

    }
}
