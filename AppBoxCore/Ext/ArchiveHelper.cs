using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace Common
{
    public class ArchiveHelper
    {
        public static ArchiveEncoding ArchiveEncodinUTF8 = new SharpCompress.Common.ArchiveEncoding()
        {
            Default = Encoding.GetEncoding("utf-8")
        };
        /// <summary>
        /// 压缩（zip格式）
        /// </summary>
        /// <param name="fromFileDirectory">待压缩目录</param>
        /// <param name="outFilePath">压缩后文全件路径</param>
        public static void ZipCompress(List<string> targetFile, string zipFile, Action<double> Compressed, CompressionType compressionType = CompressionType.LZMA, Func<string, bool> isIgnore = null)
        {
            var dir = Path.GetDirectoryName(zipFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            //解决中文乱码问题
            SharpCompress.Writers.WriterOptions options = new SharpCompress.Writers.WriterOptions(compressionType);
            options.ArchiveEncoding = ArchiveEncodinUTF8;

            using (var archive = ZipArchive.Create())
            {
                var Percent = 0;
                for (int i = 0; i < targetFile.Count; i++)
                {
                    Compressed(1.0 *i/targetFile.Count);
                    var file = targetFile[i];
                    if (file != null)
                    {
                        if (File.Exists(file))
                        {
                            if(((isIgnore?.Invoke(file)) ?? false) == false)
                                archive.AddEntry(Path.GetFileName(file), file);
                        }
                        else if (Directory.Exists(file))
                        {
                            if (((isIgnore?.Invoke(file)) ?? false) == false)
                                ArchiveAddDirectory(archive, Path.GetFileName(file), file, isIgnore);
                            //archive.AddAllFromDirectory(file);
                        }
                    }
                }
                var totalSize = archive.TotalSize;
                archive.CompressedBytesRead +=(object sender, CompressedBytesReadEventArgs e) =>
                {
                    var Percentage = ((double)e.CompressedBytesRead / (double)totalSize) * 100;
                    Compressed(Percentage);
                };
                using (var zip = System.IO.File.OpenWrite(zipFile))
                    archive.SaveTo(zip, options);
            }
        }

        public static void ArchiveAddDirectory(ZipArchive archive, string key, string path, Func<string, bool> isIgnore = null)
        {
            if (Directory.Exists(path))
            {
                var dls = new DirectoryInfo(path).GetDirectories();
                foreach (var item in dls)
                {
                    if (((isIgnore?.Invoke(item.FullName)) ?? false) == false)
                        ArchiveAddDirectory(archive, Path.Combine(key, item.Name), item.FullName, isIgnore);
                }
                var ls = new DirectoryInfo(path).GetFiles();
                foreach (var item in ls)
                {
                    if (((isIgnore?.Invoke(item.FullName)) ?? false) == false)
                        archive.AddEntry(Path.Combine(key, item.Name), item);
                }
            }
        }
        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="extractPath"></param>
        /// <param name="progressCallback"></param>
        public static void Extract(string archivePath, string extractPath, Action<double> progressCallback)
        {
            Extract(archivePath, extractPath, (x, y) => progressCallback(y));
        }
        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="extractPath"></param>
        /// <param name="progressCallback"></param>
        public static void Extract(string archivePath, string extractPath, Action<IArchiveEntry, double> progressCallback)
        {
            using (Stream stream = File.OpenRead(archivePath))
            {
                using (var archive = ArchiveFactory.Open(stream))
                {
                    int totalEntries = archive.Entries.Count();
                    int processedEntries = 0;

                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(extractPath, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                        }

                        processedEntries++;
                        double progress = (double)processedEntries / totalEntries;
                        progressCallback?.Invoke(entry, progress);
                    }
                }
            }
        }
        public static List<string> ExtractFileList(string archivePath)
        {
            var ls = new List<string>();
            using (Stream stream = File.OpenRead(archivePath))
            {
                using (var archive = ArchiveFactory.Open(stream))
                {
                    foreach (var entry in archive.Entries)
                    {
                        ls.Add(entry.Key);
                    }
                }
            }
            return ls;
        }
    }
}
