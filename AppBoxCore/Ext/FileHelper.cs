using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Common
{
    public class FileHelper
    {
        /// <summary>
        /// 复制⽂件夹及⽂件
        /// </summary>
        /// <param name="sourceFolder">原⽂件路径</param>
        /// <param name="destFolder">⽬标⽂件路径</param>
        /// <returns></returns>
        public static void CopyFolder(string sourceFolder, string destFolder, Action<double> processValBack = null)
        {
            try
            {
                //如果⽬标路径不存在,则创建⽬标路径
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                //得到原⽂件根⽬录下的所有⽂件
                string[] files = Directory.GetFiles(sourceFolder);
                //得到原⽂件根⽬录下的所有⽂件夹
                string[] folders = Directory.GetDirectories(sourceFolder);
                double count = 0;
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    // 复制⽂件
                    File.Copy(file, dest, true);
                    Debug.WriteLine(file);
                    if (processValBack != null)
                        processValBack(++count/(files.Length + folders.Length));
                }
                foreach (string folder in folders)
                {
                    string dirName = folder.Split('\\')[folder.Split('\\').Length - 1];
                    string destfolder = Path.Combine(destFolder, dirName);
                    // 递归调⽤
                    if (processValBack != null)
                    {
                        //var baseCount = count/(files.Length + folders.Length);
                        CopyFolder(folder, destfolder, (x) =>
                        {
                            processValBack((count + x / 10)/(files.Length + folders.Length));
                        });
                        Debug.WriteLine(folder);
                        processValBack(++count/(files.Length + folders.Length));
                    }
                    else
                    {
                        CopyFolder(folder, destfolder);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"copy file Error:{ex.Message}\r\n source:{ex.StackTrace}");
            }
        }
        /// <summary>
        /// 移动⽂件
        /// </summary>
        /// <param name="sourceFolder">源⽂件夹</param>
        /// <param name="destFolder">⽬标⽂件呢</param>
        public static void MoveFolder(string sourceFolder, string destFolder)
        {
            try
            {
                //如果⽬标路径不存在,则创建⽬标路径
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                //得到原⽂件根⽬录下的所有⽂件
                string[] files = Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    // 移动⽂件
                    File.Move(file, dest);
                }
                //得到原⽂件根⽬录下的所有⽂件夹
                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string dirName = folder.Split('\\')[folder.Split('\\').Length - 1];
                    string destfolder = Path.Combine(destFolder, dirName);
                    // 递归调⽤
                    MoveFolder(folder, destfolder);
                }
                // 删除源⽂件夹
                Directory.Delete(sourceFolder);
            }
            catch (Exception ex)
            {
                throw new Exception($"move file Error:{ex.Message}\r\n source:{ex.StackTrace}");
            }
        }

        #region 文件大小
        public static string GetFileSizeInfo(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                var info = new FileInfo(FilePath);
                return FileHelper.ConvertDiskSpace(info.Length);
            }
            return "";
        }
        public static double GetLength(double size, int i, out int index)
        {
            index = i;
            if (i == 5)
                return size;
            if (size > 1024)
            {
                size = (size / 1024);
                i++;
                size = GetLength(size, i, out index);
            }
            return size;

        }

        public static string ConvertDiskSpace(double res, int index = -1)
        {
            string diskSize = "";
            switch (index)
            {
                case -1:
                    int idx;
                    var len = GetLength(res, 0, out idx);
                    diskSize = ConvertDiskSpace(len, idx);
                    break;
                case 0:
                    diskSize = $"{res.ToString("0.00")} B";
                    break;
                case 1:
                    diskSize = $"{res.ToString("0.00")} KB";
                    break;
                case 2:
                    diskSize = $"{res.ToString("0.00")} MB";
                    break;
                case 3:
                    diskSize = $"{res.ToString("0.00")} GB";
                    break;
                case 4:
                    diskSize = $"{res.ToString("0.00")} TB";
                    break;
                default:
                    break;
            }
            return diskSize;
        }
        #endregion
    }
}
