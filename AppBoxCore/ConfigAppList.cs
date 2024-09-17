using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Common;
using System.Windows.Input;
using YamlDotNet.Serialization;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using YamlDotNet.Core;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharpCompress.Common;
using System.Windows;
using PropertyChanged;
using System.Threading.Tasks;
using System.Linq;
using TouchSocket.Http;

namespace AppBox
{
    //[DoNotNotifyAttribute]
    public class ConfigAppList : INotifyPropertyChanged
    {
        public static string ConfigPath = "ConfigAppList.yml";
        private string workSpaceDir = "../AppBoxData";
        private bool isRemote = false;
        private AppItem selectedApp;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<AppItem> AppList { get; set; }
        [YamlIgnore]
        public ObservableCollection<AppItem> AppUniqueList { get; set; } = new ObservableCollection<AppItem>();
        [YamlIgnore]
        public ObservableCollection<AppItem> AppVersionList { get; set; } = new ObservableCollection<AppItem>();
        //[YamlIgnore]
        //public AppItem SelectedItem
        //{
        //    get => this.GetValue(x => SelectedItem);
        //    set => this.SetValue(x => x.SelectedItem, value);
        //}
        [YamlIgnore]
        public ICommand DoubleClick { get; set; }
        [YamlIgnore]
        public ICommand SignalClick { get; set; }
        [YamlIgnore]
        //public ConfigAppBox ConfigAppBox { get; set; }
        public string LocalObjectWorkSpaceDir
        {
            get => MainVM.Instance?.LocalObject?.WorkSpaceDir;
        }
        public bool IsRemote
        {
            get => isRemote; set
            {
                isRemote=value;
            }
        }
        [YamlIgnore]
        public string WorkSpaceDir
        {
            get => workSpaceDir; set
            {
                workSpaceDir=value;
            }
        }
        [YamlIgnore]
        public AppItem SelectedApp { get => selectedApp; set { selectedApp=value; LoadAppVersionList(value); } }
        //[YamlIgnore]
        //public string RemoteUri { get; set; }
        [YamlIgnore]
        public IEnumerable<MenuItemVM> MenuItemList { get; set; } = new List<MenuItemVM>();

        public ConfigAppList() : this(null)
        {
        }
        public ConfigAppList(string WorkSpaceDir)
        {
            AppList = new ObservableCollection<AppItem>();
            this.WorkSpaceDir = WorkSpaceDir;
            DoubleClick = UICommand.Create(Open);
            SignalClick = UICommand.Create(x =>
            {
                if (x is AppItem app)
                    SelectedApp = app;
            });
            if (!IsRemote)
            {
                MenuItemList = new List<MenuItemVM> {
                new MenuItemVM()
                {
                    Name = "打开",
                    CMD = UICommand.Create(Open)
                }
                ,new MenuItemVM()
                {
                    Name = "设置",
                    CMD = UICommand.Create(设置),
                }
                ,new MenuItemVM()
                {
                    Name = "浏览所在文件夹",
                    CMD = UICommand.Create(所在文件夹)
                }
                ,new MenuItemVM()
                {
                    Name = "部署",
                    CMD = UICommand.Create(部署)
                },
                new MenuItemVM()
                {
                    Name = "移动到其他工作空间...",
                    CMD = UICommand.Create(移动到其他工作空间),
                }
                ,new MenuItemVM()
                {
                    Name = "打包",
                    CMD = UICommand.Create(打包)
                }
                ,new MenuItemVM()
                {
                    Name = "把包上传到当前远程工作空间",
                    CMD = UICommand.Create(推包)
                }
                ,new MenuItemVM()
                {
                    Name = "删除",
                    CMD = UICommand.Create(删除)
                }
                ,new MenuItemVM()
                {
                    Name = "更换图标",
                    CMD = UICommand.Create(更换图标)
                }
                ,new MenuItemVM()
                {
                    Name = "固化图标",
                    CMD = UICommand.Create(固化图标)
                }
                ,new MenuItemVM()
                {
                    Name = "复制路径链接",
                    CMD = UICommand.Create(复制路径链接)
                }
                ,
            };
            }
        }

        public void LoadAppList()
        {
            AppUniqueList.Clear();
            var result = AppList.Reverse().GroupBy(item => item.Uri);
            foreach (var group in result)
            {
                if (string.IsNullOrWhiteSpace(group.FirstOrDefault()?.Uri))
                {
                    group.ToList().ForEach(x => AppUniqueList.Add(x));
                }
                else
                {
                    var first = group.FirstOrDefault();
                    if(string.IsNullOrWhiteSpace(first.IconDept))
                        first.IconDept = group.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.IconDept))?.IconDept;
                    AppUniqueList.Add(first);//.OrderByDescending(item => item.Version)
                }
            }
            AppVersionList.Clear();
        }

        public void LoadAppVersionList(AppItem item)
        {
            AppVersionList.Clear();
            if(!string.IsNullOrWhiteSpace(item.Uri))
                AppList.Where(x => x.Uri == item.Uri).Reverse().ToList().ForEach(x => AppVersionList.Add(x));//.OrderByDescending(x => x.Version)
            else
                AppList.Where(x => x == item).Reverse().ToList().ForEach(x => AppVersionList.Add(x));//.OrderByDescending(x => x.Version)
        }

        private void 复制路径链接(object x)
        {
            if (x is AppItem app)
            {
                this.CopyURI(app);
            }
        }

        private void 移动到其他工作空间(object obj)
        {
            if (IsRemote)
            {
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app)
            {
                //选择工作空间
                var set = MainVM.Instance?.ConfigAppBox.ConfigWorkSpaceDir.LocalURIs;
                var newPath = "";
                this.MessageBoxAskSelect($"选择移动到哪个工作空间,如果存在相同版本则覆盖", set,
                x =>
                {
                    if (string.IsNullOrWhiteSpace(x))
                        return "不能为空";
                    newPath = x;
                    return "";
                }
                , null
                , null
                , () =>
                {
                    //移动
                    var ws = MainVM.Instance.GetLocalURIAppList(newPath);
                    if (ws != null)
                    {
                        MainVM.Instance.MessageBoxBusy($"正在移动{app.Name}...", () =>
                        {
                            this.AppMoveToWS(app, ws);
                        }
                        , null, false);
                    }
                }
                );
            }
        }

        private void 固化图标(object obj)
        {
            if (IsRemote)
            {
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app)
            {
                try
                {
                    app.IconDept = ImageSourceToIconDept(app.Icon);
                    this.SaveWS();
                }
                catch (Exception ex)
                {
                    this.MessageBoxShow($"图片固化失败！\n{ex}");
                }
            }
        }
        public delegate string ImageFilePathToIconDeptHandler(string imgpath);
        public static ImageFilePathToIconDeptHandler ImageFilePathToIconDept = (o) => null;
        public delegate string ImageSourceToIconDeptHandler(object soc);
        public static ImageSourceToIconDeptHandler ImageSourceToIconDept = (o) => null;

        private void 更换图标(object obj)
        {
            if (IsRemote)
            {
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app)
            {
                if (UISelectFileHelper.OpenFile("图片文件 (*.jpg,*.jpeg,*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*") is string imgpath)
                {
                    try
                    {
                        if (imgpath != null)
                        {
                            app.IconDept = ImageFilePathToIconDept(imgpath);
                            this.SaveWS();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.MessageBoxShow($"图片加载失败！\n{ex}");
                    }
                }
            }
        }

        private void 部署(object obj)
        {
            if (IsRemote)
            {
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app)
            {
                if (app.AppItemType != AppItemType.应用包)
                    this.MessageBoxShow($"{app.AppItemType}不支持部署，应用包才允许部署");
                else
                    WorkspaceHelper.InstallAPP(app, LocalObjectWorkSpaceDir);
            }
        }

        private void 所在文件夹(object obj)
        {
            if (IsRemote)
            {
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app)
            {
                var uri = Path.GetFullPath(app.InitPath(WorkSpaceDir));
                ProcessHelper.OpenFileWhereDir(uri);
                //ProcessHelper.OpenFile(app.InitDirPath());
            }
        }

        private void 删除(object obj)
        {
            if (IsRemote)
            {
                if (obj is AppItem rapp)
                {
                    rapp.MessageBoxAskText($"请输入远程删除“{rapp.Name}-{rapp.Version}”需要的密码", token => {
                        rapp.MessageBoxBusy($"删除{rapp.ToString()}...",
                            () => {
                                var res = ConfigAppBox.DeleteBy(MainVM.Instance.RemoteURI, rapp, token);
                                if (res.StatusCode == 403)
                                    rapp.MessageBoxShow("失败，请检查密码!", $"结果代码：{res.StatusCode}");
                                else if (res.StatusCode == 404)
                                    rapp.MessageBoxShow("失败，请确认目标服务器允许删除应用!", $"结果代码：{res.StatusCode}");
                                else
                                {
                                    if (res.StatusCode == 200)
                                    {
                                        UIInvokeHelper.Invoke(() =>
                                        {
                                            this.AppListRemove(rapp, false);
                                            this.LoadAppList();
                                        });
                                    }
                                    rapp.MessageBoxShow($"{res.StatusMessage}\n{res.GetBody()}", $"结果代码：{res.StatusCode}");
                                }
                            }, null, true);
                    });
                }
                else
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app)
            {
                if (app.IsAppBoxApp(WorkSpaceDir))
                {
                    this.MessageBoxShow($"{Path.GetFullPath(app.InitDirPath(WorkSpaceDir))} 是当前程序运行目录不可删除，除非在安装运行其他版本后删除该版本。");
                    return;
                }
                this.MessageBoxConfirm($"是否删除？\n {app.Name}\n{app.Version}", null,
                    () =>
                    {
                        app.删除文件(WorkSpaceDir);
                        this.AppListRemove(app, true);
                    });
            }
        }

        private void 打包(object obj)
        {
            if (IsRemote)
            {
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app && app.AppItemType == AppItemType.应用)
            {
                ConfigAppList.App打包应用(app, this);
            }
            else
                this.MessageBoxShow("只有应用类型才支持打包");
        }

        private void 推包(object obj)
        {
            if (IsRemote)
            {
                this.MessageBoxShow("不支持操作");
            }
            else if (obj is AppItem app)
            {
                if(app.AppItemType != AppItemType.应用)
                    ConfigAppList.App推包(app, this);
                else if (app.AppItemType == AppItemType.应用 && File.Exists(app.GetPackPath(WorkSpaceDir)))
                    ConfigAppList.App推包(app, this);
                else
                    this.MessageBoxShow("应用未打包不支持推包");
            }
            else
                this.MessageBoxShow("该应用不支持推包");
        }

        /// <summary>
        /// 推送上传，应用类型除外
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configAppList"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void App推包(AppItem app, ConfigAppList configAppList)
        {
            if(string.IsNullOrWhiteSpace(MainVM.Instance.RemoteURI))
            {
                app.MessageBoxShow("请先切换远程的工作空间为要推送的工作空间!");
                return;
            }
            var path = app.InitPath(configAppList.WorkSpaceDir);
            if(app.AppItemType == AppItemType.应用)
                path = app.GetPackPath(configAppList.WorkSpaceDir);
            if (File.Exists(path))
            {
                app.MessageBoxAskText("请输入推送密码", token => {
                    app.MessageBoxBusy($"推送{app.ToString()}",
                        () => {
                            //ConfigAppBox.UpdateFile(File.ReadAllBytes(path));
                            var res = ConfigAppBox.UploadFile(path, MainVM.Instance.RemoteURI, app, token);
                            if (res.StatusCode == 403)
                                app.MessageBoxShow("失败，请检查密码!", $"结果代码：{res.StatusCode}");
                            else if (res.StatusCode == 404)
                                app.MessageBoxShow("失败，请确认目标服务器允许推送包!", $"结果代码：{res.StatusCode}");
                            else
                                app.MessageBoxShow($"{res.StatusMessage}\n{res.GetBody()}", $"结果代码：{res.StatusCode}");
                        });
                });
            }
        }

        public static void App打包应用(AppItem app, ConfigAppList ConfigAppList)
        {
            if (File.Exists(app.GetPackPath(ConfigAppList.WorkSpaceDir)))
                app.MessageBoxShow($"应用包已存在:\n{Path.GetFullPath(app.GetPackPath(ConfigAppList.WorkSpaceDir))}");
            else
            {
                //打包
                app.MessageBoxBusy("正在打包...", () =>
                {
                    if (app.PackAPP(ConfigAppList.WorkSpaceDir) is AppItem pack)
                    {
                        UIInvokeHelper.Invoke(() =>
                        {
                            ConfigAppList.SaveWSWhenAppAppend(pack);
                        });
                    }
                }
                , "打包完成");
            }
        }

        private void 设置(object obj)
        {
            if (IsRemote && obj is AppItem rapp)
            {
                rapp.MessageBoxAskText("请输入推送设置需要的密码", token => {
                    SettingApp(rapp, cfg => {
                        rapp.Name = cfg.Name;
                        rapp.Category = cfg.Category;
                        rapp.Description = cfg.Description;
                        rapp.InfoWithDescription = cfg.Description;
                        rapp.MessageBoxBusy($"更新设置{rapp.ToString()}",
                            () => {
                                //ConfigAppBox.UpdateFile(File.ReadAllBytes(path));
                                var res = ConfigAppBox.UploadDept(MainVM.Instance.RemoteURI, rapp, token);
                                if (res.StatusCode == 403)
                                    rapp.MessageBoxShow("失败，请检查密码!", $"结果代码：{res.StatusCode}");
                                else if(res.StatusCode == 404)
                                    rapp.MessageBoxShow("失败，请确认目标服务器允许修改应用信息!", $"结果代码：{res.StatusCode}");
                                else
                                    rapp.MessageBoxShow($"{res.StatusMessage}\n{res.GetBody()}", $"结果代码：{res.StatusCode}");
                            });
                    });
                });
            }
            else if (obj is AppItem app)
            {
                SettingApp(app, cfg => {
                    app.Name = cfg.Name;
                    app.Category = cfg.Category;
                    app.Description = cfg.Description;
                    app.InfoWithDescription = cfg.Description;
                    this.SaveWSWhenAppAppend(app);
                });
            }
        }

        public void SettingApp(AppItem app, Action<AppItemConfig> Save)
        {
            AppItemConfig cfg = new AppItemConfig()
            {
                Name = app.Name ?? "",
                Category = app.Category ?? "",
                Description = app.Description ?? "",
            };
            this.MessageBoxObjectConfirm("配置"
            , cfg
            , x =>
            {
                if (string.IsNullOrWhiteSpace(x.Name))
                {
                    return "APP名称不能为空";
                }
                else if (string.IsNullOrWhiteSpace(x.Category))
                {
                    return "分类名不能为空";
                }
                return "";
            }
            , null
            , () =>
            {
                Save(cfg);
            });
        }

        private void Open(object x)
        {
            if (x is AppItem app)
            {
                var uri = app.InitPath(LocalObjectWorkSpaceDir);
                //本地存在则打开
                if (IsRemote)
                {
                    string msg = $"是否下载安装？\n{app.Name}\n{app.Version}";
                    if (File.Exists(uri))
                    {
                        var localPath = Path.Combine(LocalObjectWorkSpaceDir, app.PackPath());
                        if (File.Exists(localPath))
                        {
                            this.MessageBoxConfirm($"已存在安装包，是否重新下载？\n {localPath}", null,
                                () =>
                                {
                                    //msg = $"已存在{uri}!\n是否重新下载安装？\n{app.Name}\n{app.Version}";
                                    DownloadAPP(app);
                                },
                                () =>
                                {
                                    WorkspaceHelper.InstallAPP(app, LocalObjectWorkSpaceDir);
                                });
                        }
                    }
                    else
                        //确认下载
                        this.MessageBoxConfirm(msg, null, () =>
                        {
                            DownloadAPP(app);
                        });
                }
                else
                {
                    if (app.IsAppBoxApp(WorkSpaceDir))
                    {
                        this.MessageBoxShow($"{Path.GetFullPath(app.InitDirPath(WorkSpaceDir))} 是当前程序。");
                        return;
                    }
                    //MessageBox.Show(node.Name);
                    ProcessHelper.OpenFileByWs(uri, false);
                }
            }
        }
        /// <summary>
        /// 导入单文件没有Uri值，导入文件夹才有
        /// </summary>
        /// <param name="path"></param>
        /// <param name="app"></param>
        public void Drog(string path, AppItem app = null)
        {
            if (IsRemote)
            {

            }
            else if (app != null)
            {
                var uri = app.InitPath(WorkSpaceDir);
                ProcessHelper.OpenFileByWs(uri, false, path);
            }
            else
            {
                var ExtName = Path.GetExtension(path.ToLower());
                var FileSizeInfo = FileHelper.GetFileSizeInfo(path);
                // 快捷方式不需要获取目标文件路径
                if (ExtName.EndsWith("lnk") || ExtName.EndsWith("url"))
                {
                    this.MessageBoxAskText("请设置分组名称", name =>
                    {
                        var app = new AppItem();
                        app.LoadByLNK(path, WorkSpaceDir);
                        app.Category = name;
                        this.SaveWSWhenAppAppend(app);
                    });
                }
                else if (ExtName.EndsWith("exe"))
                {
                    this.MessageBoxAskText($"文件大小{FileSizeInfo}\n(如果不是单个可执行exe文件，则不能拖入单个exe，\n需要拖入exe依赖的整个文件夹！)\n\n请设置分组名称：", name =>
                    {
                        var app = new AppItem();
                        app.LoadByEXESIGNAL(path, WorkSpaceDir);
                        app.Category = name;
                        this.SaveWSWhenAppAppend(app);
                    });
                }
                else if (Directory.Exists(path))
                {
                    this.MessageBoxConfirm($"将目录导入为APP，还需要选择该目录范围内的启动exe文件", null, () =>
                    {
                        if (UISelectFileHelper.OpenFile("(*.exe)|*.exe", "", path) is string FilePath)
                        {
                            if (FilePath.StartsWith(path))
                            {
                                //导入
                                var file = FilePath.Substring(path.Length);
                                if (file.StartsWith("/") || file.StartsWith("\\"))
                                    ImportAPP(path, file.Substring(1));
                            }
                            else
                            {
                                this.MessageBoxShow($"{FilePath}\n不属于\n{path}");
                            }
                        }
                    });
                }
                else
                {
                    if (
                   ExtName.EndsWith("rar")
                   || ExtName.EndsWith("zip")
                   || ExtName.EndsWith("7z")
                   || ExtName.EndsWith("gz")
                   )
                    {
                        var type = "";
                        this.MessageBoxAskSelect($"文件大小{FileSizeInfo}（文件格式{ExtName}）\n请选择导入方式！", new List<string> { "解压安装应用", "存储该应用包", "导入为文件" },
                                  x =>
                                  {
                                      type = x;
                                      return "";
                                  }
                                  , null
                                  , null
                                  , () =>
                                  {
                                      if (type == "解压安装应用")
                                      {
                                          导入应用包(path, FileSizeInfo, true);
                                      }
                                      else if (type == "存储该应用包")
                                      {
                                          导入应用包(path, FileSizeInfo);
                                      }
                                      else if (type == "导入为文件")
                                      {
                                          ImportNormalFile(path, ExtName, FileSizeInfo);
                                      }
                                  });
                    }
                    else
                        ImportNormalFile(path, ExtName, FileSizeInfo);
                }
            }
        }

        private void 导入应用包(string path, string FileSizeInfo, bool install = false)
        {
            string initFile = "";
            var ls = ArchiveHelper.ExtractFileList(path);
            //列出文件列表供选择启动文件
            this.MessageBoxAskSelect("选择APP启动文件", ls,//new List<string> { "1221321", "wadwdn", "dwadwa" }, 
                x =>
                {
                    initFile = x;
                    if (string.IsNullOrWhiteSpace(initFile))
                        return "请选择文件";
                    return "";
                }
                , null
                , null
                , () =>
                {
                    var cfg = new AppItemInstallConfig();
                    cfg.InitFile = initFile;
                    cfg.Name = Path.GetFileName(initFile);
                    cfg.Uri = cfg.Name;
                    cfg.Category = cfg.Name;
                    this.MessageBoxObjectConfirm("导入APP配置"
                        , cfg
                        , x =>
                        {
                            if (string.IsNullOrWhiteSpace(x.InitFile))
                            {
                                return "启动文件不能为空";
                            }
                            else if (string.IsNullOrWhiteSpace(x.Uri))
                            {
                                return "全局APP名不能为空";
                            }
                            else if (string.IsNullOrWhiteSpace(x.Name))
                            {
                                return "APP名称不能为空";
                            }
                            else if (string.IsNullOrWhiteSpace(x.Version))
                            {
                                return "版本号不能为空";
                            }
                            else if (string.IsNullOrWhiteSpace(x.Category))
                            {
                                return "分类名不能为空";
                            }
                            else if (Directory.Exists(Path.Combine(WorkSpaceDir, x.Uri, x.Version)))
                            {
                                return "文件夹已存在，请使用不同的版本号或者全局APP名";
                            }
                            return "";
                        }
                        , null
                        , () =>
                        {
                            var app = new AppItem();
                            app.Description = $"{Path.GetExtension(path)} 应用包";
                            app.AppItemType = AppItemType.应用包;
                            app.Size = FileSizeInfo;
                            app.Name = cfg.Name;
                            app.Uri = cfg.Uri;
                            app.InitFile = cfg.InitFile;
                            app.Version = cfg.Version;
                            app.Category = cfg.Category;
                            app.ReplaceByFile(path, WorkSpaceDir);
                            this.SaveWSWhenAppAppend(app);
                            if (install)
                            {
                                WorkspaceHelper.InstallAPP(app, LocalObjectWorkSpaceDir);
                            }
                        });
                });
        }

        private void ImportNormalFile(string path, string ExtName, string FileSizeInfo)
        {
            this.MessageBoxAskText($"文件大小{FileSizeInfo}\n（文件格式{ExtName}）\n请设置分组名称", name =>
            {
                var app = new AppItem();
                app.LoadByNormalFile(path, WorkSpaceDir);
                app.Category = name;
                this.SaveWSWhenAppAppend(app);
            });
        }

        private void ImportAPP(string path, string InitFile)
        {
            var name = AppItemHelper.GetExeName(Path.Combine(path, InitFile));
            var Version = AppItemHelper.GetExeVersion(Path.Combine(path, InitFile));
            AppItemInstallConfig cfg = new AppItemInstallConfig()
            {
                InitFile = InitFile,
            };
            AppItemHelper.App安装描述填充(path, cfg);
            cfg.Category = "其他";

            this.MessageBoxObjectConfirm("导入APP配置"
                , cfg
                , x =>
                {
                    if (string.IsNullOrWhiteSpace(x.InitFile))
                    {
                        return "启动文件不能为空";
                    }
                    else if (string.IsNullOrWhiteSpace(x.Uri))
                    {
                        return "全局APP名不能为空";
                    }
                    else if (string.IsNullOrWhiteSpace(x.Name))
                    {
                        return "APP名称不能为空";
                    }
                    else if (string.IsNullOrWhiteSpace(x.Version))
                    {
                        return "版本号不能为空";
                    }
                    else if (string.IsNullOrWhiteSpace(x.Category))
                    {
                        return "分类名不能为空";
                    }
                    else if (Directory.Exists(Path.Combine(WorkSpaceDir, x.Uri, x.Version)))
                    {
                        return "文件夹已存在，请使用不同的版本号或者全局APP名";
                    }
                    return "";
                }
                , null
                , () =>
                {
                    App通过文件夹和配置进行导入(path, cfg);
                });
        }

        private bool App通过文件夹和配置进行导入(string path, AppItemInstallConfig cfg)
        {
            AppItemHelper.App安装描述填充(path, cfg);
            if (Directory.Exists(Path.Combine(WorkSpaceDir, cfg.Uri, cfg.Version)))
            {
                return false;
            }
            var app = new AppItem();
            app.Version = cfg.Version;
            app.Uri = cfg.Uri;
            var dir = app.InitDirPath(WorkSpaceDir);
            if (!Directory.Exists(dir))
            {
                this.MessageBoxBusy("正在导入...", () =>
                {
                    app.LoadByAPP(path, cfg.IsMove, WorkSpaceDir);
                    app.Name = cfg.Name;
                    app.InitFile = cfg.InitFile;
                    app.Category = cfg.Category;
                    UIInvokeHelper.Invoke(() =>
                    {
                        this.SaveWSWhenAppAppend(app);
                    });
                }
                , "导入完成");
                return true;
            }
            return false;
        }



        public void DownloadAPP(AppItem app)
        {
            try
            {
                app.Status = "下载中";
                var localPath = Path.Combine(LocalObjectWorkSpaceDir, app.PackPath());
                var remotePath = Path.Combine(workSpaceDir, app.PackPath());
                //下载文件，更新进度条
                Task.Run(() =>
                {
                    // 执行耗时的任务
                    var Dl = new Common.Downloader();
                    Dl.DownloadFile(localPath, remotePath, x =>
                    {
                        UIInvokeHelper.Invoke(() =>
                        {
                            app.Status = $"{((x*0.9)*100).ToString("F2")}%";
                        });
                    });
                }).ContinueWith(task =>
                {
                    if (File.Exists(localPath))
                    {
                        // 在任务完成后执行的代码
                        UIInvokeHelper.Invoke(() =>
                        {
                            WorkspaceHelper.InstallAPP(app, LocalObjectWorkSpaceDir);
                        });
                    }
                    else
                    {
                        UIInvokeHelper.Invoke(() =>
                        {
                            app.Status = "失败";
                            this.MessageBoxShow($"下载失败!\n {remotePath}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                app.Status = "异常";
                this.Log(ex, LogType.ERROR);
                this.MessageBoxShow(ex.ToString());
            }
        }
    }
}