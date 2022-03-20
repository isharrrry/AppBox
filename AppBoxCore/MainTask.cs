using System;
using System.IO;
using Common;
using TouchSocket.Http;

namespace AppBox
{
    /// <summary>
    /// APPListUpdate更新服务
    /// </summary>
    public class MainTask
    {
        private ConfigAppBox ConfigAppBox;

        public MainTask(ConfigAppBox cfg, bool initServer)
        {
            ConfigAppBox = cfg;

            //启动服务
            if (!initServer)
                return;
            if (ConfigAppBox.ConfigAppServer.IsEnable)
            {
                ConfigAppBox.ConfigAppServer.LoadServer(true);
            }
        }

        private static string AppNameMap(string name)
        {
            foreach (var item in MainVM.Instance.ConfigAppBox.AppUpdateNameMap)
            {
                if (name == item.Key)
                    return item.Value;
            }
            return name;
        }

        private string InitFileMap(string 路径, string InitFile)
        {
            foreach (var item in ConfigAppBox.AppUpdateInitFileMap)
            {
                if (InitFile == item.Key && File.Exists(Path.Combine(路径, item.Value)))
                    return item.Value;
            }
            return InitFile;
        }


        public object GetFileVersion(string 路径, string appInitFile)
        {
            var file = Path.Combine(路径, appInitFile);
            if (!File.Exists(file))
            {
                return "(文件不存在)";
            }
            foreach (var item in ConfigAppBox.AppUpdateVersionFileMap)
            {
                if (appInitFile == item.Key && File.Exists(Path.Combine(路径, item.Value)))
                    return AppItemHelper.GetExeVersion(Path.Combine(路径, item.Value));
            }
            return AppItemHelper.GetExeVersion(file);
        }

        private string VersionMap(string 路径, AppItemInstallConfig cfg, string Version)
        {
            foreach (var item in ConfigAppBox.AppUpdateVersionFileMap)
            {
                if (cfg.InitFile == item.Key && File.Exists(Path.Combine(路径, item.Value)))
                    return AppItemHelper.GetExeVersion(Path.Combine(路径, item.Value));
            }
            return Version;
        }

        private Tuple<AppItemInstallConfig, string> PackTempApp(string 远程路径, string 路径, string 入口)
        {
            //压缩
            var initDirPaths = 路径;
            var lnkFile = Path.Combine(路径, 入口);
            AppItemInstallConfig cfg = new AppItemInstallConfig()
            {
                InitFile = 入口,
            };
            AppItemHelper.App安装描述填充(路径, cfg);
            var Uri = cfg.Uri;
            cfg.Version = VersionMap(路径, cfg, cfg.Version);
            cfg.InitFile = InitFileMap(路径, cfg.InitFile);
            var Version = cfg.Version;
            //var Name = cfg.Name;
            var Category = cfg.Category;
            if (string.IsNullOrWhiteSpace(Uri))
            {
                Uri = "Demo";
            }
            if (string.IsNullOrWhiteSpace(Category))
            {
                Category = "其他";
            }
            var packInitfile = $"{Uri}_{Version}.zip";
            var tempPackPath = Path.Combine(Path.GetTempPath(), packInitfile);
            if (File.Exists(tempPackPath))
                File.Delete(tempPackPath);
            AppItemHelper.App应用打包压缩(initDirPaths, tempPackPath);
            return new Tuple<AppItemInstallConfig, string>(cfg, tempPackPath);
        }

        public static void WSInfoApp(string WsDir, IBox LocalVM, string 类型, string Uri, string Version, string Category, string InitFile, string Info)
        {
            if (LocalVM.ConfigAppBox.ConfigWorkSpaceDir.LocalURIs.Contains(WsDir))
            {
                var app = new AppItem()
                {
                    AppItemType = AppItemType.应用包,
                    Uri = Uri,
                    Version = Version,
                    Category = Category,
                    InitFile = InitFile,
                    Description = Info
                };
                if (Enum.TryParse<AppItemType>(类型, true, out var type))
                    app.AppItemType = type;
                if (类型 == "APPPack")
                    app.AppItemType = AppItemType.应用包;
                if (app.AppItemType == AppItemType.应用包)
                    app.Name = app.Uri;
                else
                    app.Name = app.InitFile;
                if (LocalVM.LocalObjects.ContainsKey(WsDir))
                {
                    UIInvokeHelper.Invoke(() =>
                    {
                        LocalVM.LocalObjects[WsDir].WSUpdateApp(app, true, true);
                        LocalVM.LocalObjects[WsDir].SaveWS();
                    });
                }
                else
                {
                    LocalVM.LocalURI = WsDir;
                    LocalVM.LocalObjects[WsDir] = LocalVM.LocalObject;
                    UIInvokeHelper.Invoke(() =>
                    {
                        LocalVM.LocalObjects[WsDir].WSUpdateApp(app, true, true);
                        LocalVM.LocalObjects[WsDir].SaveWS();
                    });
                }
                WsDir.Log($"Info完成 {app.Uri} {app.Version}");
            }
            else
            {
                WsDir.Log($"不存在工作空间 {WsDir}");
            }
        }
        public static int WSDeleteApp(string WsDir, IBox LocalVM, string 类型, string Uri, string Version, string InitFile)
        {
            var count = 0;
            if (LocalVM.ConfigAppBox.ConfigWorkSpaceDir.LocalURIs.Contains(WsDir))
            {
                var app = new AppItem()
                {
                    AppItemType = AppItemType.应用包,
                    Uri = Uri,
                    Version = Version,
                    InitFile = InitFile,
                };
                if (Enum.TryParse<AppItemType>(类型, true, out var type))
                    app.AppItemType = type;
                if (类型 == "APPPack")
                    app.AppItemType = AppItemType.应用包;
                if (LocalVM.LocalObjects.ContainsKey(WsDir))
                {
                    UIInvokeHelper.Invoke(() =>
                    {
                        count = LocalVM.LocalObjects[WsDir].WSRemoveApp(app, true, true);
                        LocalVM.LocalObjects[WsDir].SaveWS();
                    });
                }
                WsDir.Log($"Delete完成 {app.Uri} {app.Version}");
            }
            else
            {
                WsDir.Log($"不存在工作空间 {WsDir}");
            }
            return count;
        }
        public static void WSAddApp(string WsDir, IBox LocalVM, string 类型, string Uri, string Version, string Category, string InitFile, string Description = "")
        {
            if (LocalVM.ConfigAppBox.ConfigWorkSpaceDir.LocalURIs.Contains(WsDir))
            {
                var app = new AppItem()
                {
                    AppItemType = AppItemType.应用包,
                    Uri = Uri,
                    Version = Version,
                    Category = Category,
                    InitFile = InitFile,
                };
                if (Enum.TryParse<AppItemType>(类型, true, out var type))
                    app.AppItemType = type;
                if (类型 == "APPPack")
                    app.AppItemType = AppItemType.应用包;
                if (app.AppItemType == AppItemType.应用包)
                {
                    app.Name = app.Uri;
                }
                else
                    app.Name = Path.GetFileNameWithoutExtension(app.InitFile);
                var mapName = AppNameMap(app.InitFile);
                if (mapName != app.InitFile)
                {
                    app.Name = mapName;
                    app.Category = mapName;
                }
                var path = app.InitPath(WsDir);
                if (app.AppItemType != AppItemType.应用 && File.Exists(path))
                    app.Size = FileHelper.GetFileSizeInfo(path);
                app.Description = Description;
                if (LocalVM.LocalObjects.ContainsKey(WsDir))
                {
                    UIInvokeHelper.Invoke(() =>
                    {
                        LocalVM.LocalObjects[WsDir].SaveWSWhenAppAppend(app);
                    });
                }
                else
                {
                    LocalVM.LocalURI = WsDir;
                    LocalVM.LocalObjects[WsDir] = LocalVM.LocalObject;
                    UIInvokeHelper.Invoke(() =>
                    {
                        LocalVM.LocalObjects[WsDir].SaveWSWhenAppAppend(app);
                    });
                }
                //ConfigAppList.App打包应用Quite(pack, LocalObject);
                //if (File.Exists(app.GetPackPath()))
                //    File.Delete(app.GetPackPath());
                WsDir.Log($"Add完成 {app.Uri} {app.Version}");
            }
            else
            {
                WsDir.Log($"不存在工作空间 {WsDir}");
            }
        }

        /// <summary>
        /// 应用app压缩上传
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="appDir"></param>
        /// <param name="appInitFile"></param>
        /// <param name="EndCall"></param>
        public void UploadByAppDir(string 远程路径, string 路径, string 入口, string Description, Action EndCall)
        {
            try
            {
                //压缩
                this.Log("压缩...");
                var cfg = PackTempApp(远程路径, 路径, 入口);
                var app = new AppItem()
                {
                    AppItemType = AppItemType.应用包,
                    Name = cfg.Item1.Name,
                    Uri = cfg.Item1.Uri,
                    Version =  cfg.Item1.Version,
                    Category =  cfg.Item1.Category,
                    InitFile =  cfg.Item1.InitFile,
                    Description =  Description,
                };
                this.LogInfo($"压缩完成，开始上传\n{cfg.Item2}\n{远程路径}");
                this.Log("上传...");
                //上传
                UploadByApp(cfg.Item2, 远程路径, app, true, EndCall);
            }
            catch (Exception ex)
            {
                this.LogErr(ex);
            }
        }

        /// <summary>
        /// 上传app文件
        /// </summary>
        /// <param name="tempPackPath"></param>
        /// <param name="app"></param>
        /// <param name="EndCall"></param>
        internal void UploadByApp(string tempPackPath, string 远程路径, AppItem app, bool autoRemove, Action EndCall)
        {
            if (File.Exists(tempPackPath))
            {
                var res = ConfigAppBox.UploadFile(tempPackPath, 远程路径, app, MainVM.Instance.ConfigAppBox.UploadToken);
                if (autoRemove && File.Exists(tempPackPath))
                    File.Delete(tempPackPath);
                if (res.StatusCode == 404)
                    app.MessageBoxConfirm("失败，请确认目标服务器允许推送包!", $"结果代码：{res.StatusCode}", EndCall, EndCall, EndCall);
                else
                    app.MessageBoxConfirm($"{res.StatusMessage}\n{res.GetBody()}", $"结果代码：{res.StatusCode}", EndCall, EndCall, EndCall);
            }
        }
    }
}