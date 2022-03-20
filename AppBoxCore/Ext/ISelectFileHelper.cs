
namespace Common
{
    public interface ISelectFileHelper
    {
        string OpenDir(string InitialDirectory = "");
        string OpenFile(string Filter = "(*.xml)|*.xml", string name = "", string InitialDirectory = null);
        string SaveFile(string Filter = "(*.xml)|*.xml", string nameHead = "", bool appendNameTime = true, string InitialDirectory = null);
    }
}