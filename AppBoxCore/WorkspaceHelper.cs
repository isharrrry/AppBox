using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Common;

namespace AppBox
{
    public static class WorkspaceHelper
    {
        #region WS

        public static void AppMoveToWS(this ConfigAppList that, AppItem app, ConfigAppList ws)
        {
            var WorkSpaceDir = that.WorkSpaceDir;
            //移动文件
            if (app.AppItemType == AppItemType.应用)
            {
                var path1 = Path.GetFullPath(Path.GetFullPath(app.InitDirPath(WorkSpaceDir)));
                var path2 = Path.GetFullPath(Path.GetFullPath(app.InitDirPath(ws.WorkSpaceDir)));
                // 如果目标路径不存在，则创建路径
                if (!Directory.Exists(Path.GetDirectoryName(path2)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path2));
                Directory.Move(path1, path2);
            }
            else
            {
                var path1 = Path.GetFullPath(app.InitPath(WorkSpaceDir));
                var path2 = Path.GetFullPath(app.InitPath(ws.WorkSpaceDir));
                // 如果目标路径不存在，则创建路径
                if (!Directory.Exists(Path.GetDirectoryName(path2)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path2));
                if (File.Exists(path2))
                    File.Delete(path2);
                File.Move(path1, path2);
            }
            UIInvokeHelper.Invoke(() => {
                //保存配置
                ws.SaveWSWhenAppAppend(app);
                //删除配置
                that.AppListRemove(app, true);
            });
        }

        public static Action<string> ClipboardSetText;

        public static void CopyURI(this ConfigAppList that, AppItem app)
        {
            var uri = app.InitPath(that.WorkSpaceDir);
            ClipboardSetText?.Invoke(uri.Replace("\\", "/"));
        }
        public static void App打包应用Quite(AppItem app, ConfigAppList ConfigAppList)
        {
            if (File.Exists(app.GetPackPath(ConfigAppList.WorkSpaceDir)))
                File.Delete(app.GetPackPath(ConfigAppList.WorkSpaceDir));
            if (app.PackAPP(ConfigAppList.WorkSpaceDir) is AppItem pack)
            {
                UIInvokeHelper.Invoke(() =>
                {
                    ConfigAppList.SaveWSWhenAppAppend(pack);
                });
            }
        }
        private static int ListRemoveWhen<T>(IList<T> List, Func<T, bool> match)
        {
            bool find = false;
            int count = 0;
            do
            {
                find = false;
                for (int i = 0; i < List.Count; i++)
                {
                    if (match(List[i]))
                    {
                        List.RemoveAt(i);
                        count++;
                        find = true;
                        break;
                    }
                }
            } while (find);
            return count;
        }

        public static void AppListRemove(this ConfigAppList that, AppItem app, bool save)
        {
            //if (that.AppUniqueList.Contains(app))
            //    that.AppUniqueList.Remove(app);
            //if (that.AppVersionList.Contains(app))
            //    that.AppUniqueList.Remove(app);
            if (that.AppList.Contains(app))
                that.AppList.Remove(app);
            if (save)
                that.SaveWS();
        }

        public static void SaveWSWhenAppAppend(this ConfigAppList that, AppItem app)
        {
            that.WSRemoveApp(app, false, false, true);
        }

        //匹配删除
        public static int WSRemoveApp(this ConfigAppList that, AppItem app, bool AllUriNull = false, bool AllVersionNull = false, bool isReplace = false)
        {
            int count = 0;
            if (that.AppList.Contains(app))
            {
                that.AppList.Remove(app);
                count++;
            }
            count += that.AppList.ListRemoveByApp(app);
            //that.AppUniqueList.ListRemoveByApp(app);
            //that.AppVersionList.ListRemoveByApp(app);
            if(isReplace)
            {
                that.AppList.Add(app);
                that.SaveWS();//保存刷新
            }
            return count;
        }
        //匹配删除
        public static int ListRemoveByApp(this ObservableCollection<AppItem> AppList, AppItem app, bool AllUriNull = false, bool AllVersionNull = false)
        {
            return ListRemoveWhen(AppList, x =>
            (AllUriNull || !string.IsNullOrWhiteSpace(x.Uri))
            && (AllVersionNull || !string.IsNullOrWhiteSpace(x.Version))
            && x.Uri == app.Uri
            && app.Version == x.Version
            && app.AppItemType == x.AppItemType
            && (app.AppItemType == AppItemType.应用 || app.AppItemType == AppItemType.应用包 || app.InitFile == x.InitFile));//app文件夹唯一为准，其他通过启动文件、版本为准
        }

        public static void WSUpdateApp(this ConfigAppList that, AppItem app, bool AllUriNull = false, bool AllVersionNull = false)
        {
            var ls = (that.AppList).Where(x =>
            (AllUriNull || !string.IsNullOrWhiteSpace(x.Uri))
            && (AllVersionNull || !string.IsNullOrWhiteSpace(x.Version))
            && x.Uri == app.Uri
            && app.Version == x.Version
            && app.AppItemType == x.AppItemType
            && (app.AppItemType == AppItemType.应用 || app.InitFile == x.InitFile));//app文件夹唯一为准，其他通过启动文件、版本为准
            foreach (var item in ls)
            {
                item.Category = app.Category;
                item.Description = app.Description;
                if (!string.IsNullOrWhiteSpace(app.IconDept))
                    item.IconDept = app.IconDept;
            }
        }

        public static void SaveWS(this ConfigAppList that)
        {
            YmlHelper.Save(Path.Combine(that.WorkSpaceDir, ConfigAppList.ConfigPath), that);
            that.LoadAppList();
        }

        public static void InstallAPP(AppItem app, string LocalObjectWorkSpaceDir)
        {
            //下载完成app则进行解压，更新进度条
            if (app.AppItemType == AppItemType.应用包)
            {
                //安装本地解压app
                var resapp = new AppItem()
                {
                    AppItemType = AppItemType.应用,
                    Uri = app.Uri,
                    Version = app.Version,
                    Name = app.Name,
                    Category = app.Category,
                    InitFile = app.InitFile,
                    IconDept = app.IconDept,
                    Description = app.Description,
                };
                var dir = resapp.InitDirPath(LocalObjectWorkSpaceDir);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                app.Status = "安装中";
                Task.Run(() =>
                {
                    // 执行耗时的任务
                    ArchiveHelper.Extract(app.InstallPackPath(LocalObjectWorkSpaceDir), resapp.InitDirPath(LocalObjectWorkSpaceDir), x =>
                    {
                        UIInvokeHelper.Invoke(() =>
                        {
                            app.Status = $"{((x * 0.1 + 0.9) * 100).ToString("F2")}%";
                        });
                    });
                }).ContinueWith(task =>
                {
                    // 在任务完成后执行的代码
                    UIInvokeHelper.Invoke(() =>
                    {
                        InstallAppEnd(resapp);
                        app.Status = "";
                        app.Icon = null;
                    });
                });
            }
            else
                InstallAppEnd(app);
        }

        private static void InstallAppEnd(AppItem app)
        {
            app.Status = "";
            //解压成功则更新图标
            //app.OnPropertyChanged(nameof(app.Icon));
            //app.Icon = app.Icon;
            MainVM.Instance?.LocalObject?.SaveWSWhenAppAppend(app);
        }
        #endregion
        //#region WS
        //#endregion
    }
}
