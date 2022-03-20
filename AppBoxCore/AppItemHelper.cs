using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Common;
using SharpCompress.Common;

namespace AppBox
{
    public static class AppItemHelper
    {
        #region Attr

        public static string GetExeName(string fileName)
        {
            string res = "";
            try
            {
                var f = FileVersionInfo.GetVersionInfo(fileName);
                return f.ProductName ?? f.FileName;
            }
            catch (Exception ex)
            {
                return Path.GetFileNameWithoutExtension(fileName);
            }
        }

        public static string GetExeVersion(string lnkFile)
        {
            try
            {
                var f = FileVersionInfo.GetVersionInfo(lnkFile);
                var ver = f.FileVersion;
                if (ver.Contains("+"))
                {
                    ver = ver.Split('+')[0];
                }
                return ver ?? "0.1";
            }
            catch (Exception)
            {
                return "0.1";
            }
        }
        /// <summary>
        /// 判断App是否当前运行AppBox程序
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static bool IsAppBoxApp(this AppItem app, string WorkSpaceDir)
        {
            string exedir = Path.GetFullPath(app.InitDirPath(WorkSpaceDir));
            string maindir = Path.GetFullPath(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
            return exedir == maindir;
        }
        public static string GetPackName(this AppItem app)
        {
            return $"{app.Uri}_{app.Version}.zip";
        }
        public static string GetPackPath(this AppItem app, string WorkSpaceDir)
        {
            return Path.Combine(WorkSpaceDir, app.GetPackName());
        }

        public static void App安装描述填充(string path, AppItemInstallConfig cfg)
        {
            var name = AppItemHelper.GetExeName(Path.Combine(path, cfg.InitFile));
            if (string.IsNullOrWhiteSpace(cfg.Name))
            {
                cfg.Name = name ?? "";
            }
            if (string.IsNullOrWhiteSpace(cfg.Uri))
            {
                cfg.Uri = name ?? "";
            }
            if (string.IsNullOrWhiteSpace(cfg.Category))
            {
                cfg.Category = name ?? "";
            }
            if (string.IsNullOrWhiteSpace(cfg.Version))
            {
                var Version = AppItemHelper.GetExeVersion(Path.Combine(path, cfg.InitFile));
                cfg.Version = Version ?? DateTime.Now.ToString("yy.MMdd.HHmmss");
            }
        }
        public static Func<AppItem, object> GetIconByIconDeptHanler;
        public static object GetIconByIconDept(this AppItem app)
        {
            return GetIconByIconDeptHanler(app);
        }
        #endregion
        #region Path
        /// <summary>
        /// 包文件
        /// </summary>
        /// <returns></returns>
        public static string PackPath(this AppItem app)
        {
            if (app.AppItemType == AppItemType.应用)
            {
                return app.GetPackName();
            }
            else if (app.AppItemType == AppItemType.应用包)
                return app.GetPackName();
            else
                return app.InitFile;
        }
        /// <summary>
        /// 启动目录
        /// app 为版本目录
        /// 其他为appboxdata根目录
        /// </summary>
        /// <returns></returns>
        public static string InitDirPath(this AppItem app, string WorkSpaceDir)
        {
            if (app.AppItemType == AppItemType.应用)
            {
                return Path.Combine(WorkSpaceDir, app.Uri, app.Version);
            }
            else
                return WorkSpaceDir;
        }
        /// <summary>
        /// 启动路径
        ///APP Path.Combine(WorkSpaceDir, Uri, Version, InitFile)
        ///LNK/EXESIGNAL Path.Combine(WorkSpaceDir, InitFile)
        ///APPPACK WorkSpaceDir + $"{Uri}_{Version}.zip";
        /// </summary>
        /// <param name="WorkSpaceDir"></param>
        /// <returns></returns>
        public static string InitPath(this AppItem app, string WorkSpaceDir)
        {
            if (app.AppItemType == AppItemType.应用)
            {
                return Path.Combine(WorkSpaceDir, app.Uri, app.Version, app.InitFile);
            }
            else if (app.AppItemType == AppItemType.应用包)
                return app.GetPackPath(WorkSpaceDir);
            else
                return Path.Combine(WorkSpaceDir, app.InitFile ?? "");
        }
        /// <summary>
        /// 包或者单文件的路径
        /// </summary>
        /// <returns></returns>
        public static string InstallPackPath(this AppItem app, string WorkSpaceDir)
        {
            if (app.AppItemType == AppItemType.应用)
            {
                return null;
            }
            else if (app.AppItemType == AppItemType.应用包)
                return app.GetPackPath(WorkSpaceDir);
            else
                return Path.Combine(WorkSpaceDir, app.InitFile);
        }
        private static void AutoInitPath(this AppItem app, string lnkFile, string WorkSpaceDir)
        {
            var i = 2;
            var fileName = Path.GetFileNameWithoutExtension(lnkFile);
            var ext = Path.GetExtension(lnkFile);
            app.InitFile = Path.GetFileName(lnkFile);
            while (File.Exists(app.InitPath(WorkSpaceDir)))
            {
                app.InitFile = $"{fileName} - {i}.{ext}";
                i++;
            }
            return;
        }
        #endregion
        #region File
        public static void LoadByLNK(this AppItem app, string lnkFile, string WorkSpaceDir)
        {
            app.Name = Path.GetFileNameWithoutExtension(lnkFile);
            if (app.Name.Contains(" - 快捷方式"))
            {
                app.Name = app.Name.Replace(" - 快捷方式", "");
            }
            app.Category = "快捷方式";
            app.AppItemType = AppItemType.快链;
            app.MoveByFile(lnkFile, WorkSpaceDir);

        }
        public static void LoadByEXESIGNAL(this AppItem app, string lnkFile, string WorkSpaceDir)
        {
            app.Name = GetExeName(lnkFile);
            app.Version = GetExeVersion(lnkFile);
            app.Description = "可执行文件";
            app.AppItemType = AppItemType.EXE;
            app.Size = FileHelper.GetFileSizeInfo(lnkFile);
            app.MoveByFile(lnkFile, WorkSpaceDir);
        }

        public static void LoadByNormalFile(this AppItem app, string path, string WorkSpaceDir)
        {
            app.Name = Path.GetFileName(path);
            app.Description = $"{Path.GetExtension(path)} 普通文件";
            app.AppItemType = AppItemType.EXE;
            app.Size = FileHelper.GetFileSizeInfo(path);
            app.MoveByFile(path, WorkSpaceDir);
        }

        /// <summary>
        /// 导入APP
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isMove"></param>
        internal static void LoadByAPP(this AppItem app, string path, bool isMove, string WorkSpaceDir)
        {
            app.Size = FileHelper.GetFileSizeInfo(path);
            var dir = app.InitDirPath(WorkSpaceDir);
            if (isMove)
                FileHelper.MoveFolder(path, dir);
            else
                FileHelper.CopyFolder(path, dir);
            app.Category = "APP";
            app.AppItemType = AppItemType.应用;
        }


        private static void CopyByFile(this AppItem app, string lnkFile, string WorkSpaceDir)
        {
            app.AutoInitPath(lnkFile, WorkSpaceDir);
            File.Copy(lnkFile, app.InitPath(WorkSpaceDir));
        }
        private static void MoveByFile(this AppItem app, string lnkFile, string WorkSpaceDir)
        {
            app.AutoInitPath(lnkFile, WorkSpaceDir);
            File.Move(lnkFile, app.InitPath(WorkSpaceDir));
        }
        public static void ReplaceByFile(this AppItem app, string lnkFile, string WorkSpaceDir)
        {
            File.Move(lnkFile, app.InitPath(WorkSpaceDir));
        }

        public static void 删除文件(this AppItem app, string WorkSpaceDir)
        {
            if (app.AppItemType == AppItemType.应用)
            {
                try
                {
                    var exedir = Path.GetFullPath(app.InitDirPath(WorkSpaceDir));
                    if (app.IsAppBoxApp(WorkSpaceDir))
                    {
                        app.MessageBoxShow($"{exedir} 是当前程序运行目录不可删除，除非在安装运行其他版本后删除该版本。");
                        return;
                    }
                    if (Directory.Exists(exedir))
                        Directory.Delete(exedir, true);
                    var packdir = Path.GetFullPath(app.GetPackPath(WorkSpaceDir));
                    if (File.Exists(packdir))
                        File.Delete(packdir);
                }
                catch (Exception ex)
                {
                    app.Log(ex, LogType.ERROR);
                    app.MessageBoxShow(ex.Message);
                }
            }
            else
            {
                File.Delete(app.InitPath(WorkSpaceDir));
            }
        }
        public static AppItem PackAPP(this AppItem app, string WorkSpaceDir)
        {
            var pack = new AppItem()
            {
                AppItemType = AppItemType.应用包,
                Uri = app.Uri,
                Version = app.Version,
                Name = app.Name,
                Category = app.Category,
                InitFile = app.InitFile,
            };
            var initDirPaths = app.InitDirPath(WorkSpaceDir);
            var packInitPath = pack.InitPath(WorkSpaceDir);
            App应用打包压缩(initDirPaths, packInitPath);
            return pack;
        }
        /// <summary>
        /// 打包压缩
        /// </summary>
        /// <param name="initDirPaths">版本目录</param>
        /// <param name="packInitPath">包路径</param>
        public static void App应用打包压缩(string initDirPaths, string packInitPath)
        {
            var files = new List<string>(Directory.GetFiles(initDirPaths));
            files.AddRange(new List<string>(Directory.GetDirectories(initDirPaths)));
            ArchiveHelper.ZipCompress(files, packInitPath, d =>
            {
                //UIInvokeHelper.Invoke(() => {
                //    ModalVM.SetProcessBar(d);
                //});
            }
            , CompressionType.Deflate);
        }
        #endregion
    }
}
