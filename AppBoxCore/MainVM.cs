using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Common;

namespace AppBox
{
    public interface IBox
    {
        ConfigAppBox ConfigAppBox { get; set; }
        ConfigAppList LocalObject { get; set; }
        Dictionary<string, ConfigAppList> LocalObjects { get; }
        string LocalURI { get; set; }
    }
    /// <summary>
    /// 
    /// ConfigAppBox.yml为软件设置
    /// 目录下ConfigAppList.yml，为下载远程临时数据
    /// ConfigList是具有多个AppItem的，某文件夹内的app数据表
    /// Appitem是单个文件（包括app包）或者app文件夹的条目
    ///远程工作空间内下载单个文件的app项，根据需要进行解压部署
    ///本地工作空间可以拖入单文件、多文件进行收纳app。打包用得少，用于手动打包后手动拷贝维护其他app数据表
    /// </summary>
    public class MainVM : INotifyPropertyChanged, IBox
    {
        public ConfigAppList RemoteObject
        {
            get => remoteObject; set
            {
                remoteObject=value;
                remoteObject.IsRemote = true;
            }
        }
        public ConfigAppList LocalObject
        {
            get => localObject; set
            {
                localObject=value;
            }
        }
        public Dictionary<string, ConfigAppList> LocalObjects { get; } = new Dictionary<string, ConfigAppList> { };
        public String remoteURI;
        public String RemoteURI
        {
            get => remoteURI;
            set
            {
                remoteURI=value;
                RemoteObject = LoadConfigAppList(remoteURI);
                搜索(keyRemote, false);
            }
        }
        public String localURI;
        public String LocalURI
        {
            get => localURI;
            set
            {
                localURI=value;
                InitWS(localURI);
                LocalObject = LoadConfigAppList(localURI);
                LocalObjects[localURI] = LocalObject;
                if (remoteURI != null)
                    RemotePull.Execute(null);
                搜索(keyLocal, true);
            }
        }
        public ConfigAppBox ConfigAppBox { get => configAppBox; set => configAppBox=value; }
        private ConfigAppBox configAppBox = new ConfigAppBox();
        private ConfigAppList localObject = new ConfigAppList(DefaultWorkSpaceDir);
        private ConfigAppList remoteObject = new ConfigAppList(DefaultWorkSpaceDir);
        private string keyLocal;
        private string keyRemote;

        public string KeyLocal
        {
            get => keyLocal;
            set
            {
                keyLocal=value;
                搜索(value, true);
            }
        }
        public string KeyRemote
        {
            get => keyRemote;
            set
            {
                keyRemote=value;
                搜索(value, false);
            }
        }
        public ICommand KeyFind { get; set; }
        public ICommand RemotePull { get; set; }
        public ICommand LocalPull { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static MainVM Instance { get; private set; }

        public MainVM(bool initServer = true)
        {
            Instance = this;
            //加载配置
            CheckConfig();
            KeyFind = UICommand.Create(x =>
            {
                搜索(keyLocal, true);
            });
            RemotePull = UICommand.Create(x =>
            {
                if (remoteURI == null)
                {
                    this.MessageBoxShow("请先选择远程工作空间");
                }
                else
                {
                    RemoteObject = LoadConfigAppList(remoteURI);
                    //KeyRemote = "";
                    if (!string.IsNullOrEmpty(KeyRemote))
                        搜索(KeyRemote, false);
                }
            });
            LocalPull = UICommand.Create(x =>
            {
                if (LocalURI == null)
                {
                    this.MessageBoxShow("请先选择本地工作空间");
                }
                else
                {
                    InitWS(LocalURI);
                    LocalObject = LoadConfigAppList(LocalURI);
                    LocalObjects[LocalURI] = LocalObject;
                    //keyLocal = "";
                    if (!string.IsNullOrEmpty(keyLocal))
                        搜索(keyLocal, true);
                    if (remoteURI != null)
                        RemotePull.Execute(null);
                }
            });
            MainTask = new MainTask(this.ConfigAppBox, initServer);
            localObject.LoadAppList();
        }
        public MainTask MainTask;
        private void 搜索(string key, bool islocal)
        {
            if (string.IsNullOrEmpty(key))
            {
                //显示所有
                if (islocal)
                    foreach (var app in LocalObject.AppList)
                    {
                        app.IsKeySelected = true;
                    }
                else
                    foreach (var app in RemoteObject.AppList)
                    {
                        app.IsKeySelected = true;
                    }
            }
            else
            {
                if (islocal)
                    foreach (var app in LocalObject.AppList)
                    {
                        app.IsKeySelected = (app.Name ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase)
                            || (app.Uri ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase)
                            || (app.Version ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase)
                            || (app.Description ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase);
                    }
                else
                    foreach (var app in RemoteObject.AppList)
                    {
                        app.IsKeySelected = (app.Name ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase)
                            || (app.Uri ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase)
                            || (app.Version ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase)
                            || (app.Description ?? "").Contains(key, StringComparison.CurrentCultureIgnoreCase);
                    }
            }
        }
        public static string DefaultWorkSpaceDir = "../AppBoxData";
        public void CheckConfig()
        {
            if (File.Exists(ConfigAppBox.ConfigPath))
            {
                try
                {
                    if (YmlHelper.Load(ConfigAppBox.ConfigPath, ref configAppBox))
                    {
                        LocalURI = configAppBox.ConfigWorkSpaceDir.LocalURIs.First();
                        LocalObjects["../"] = LocalObject;
                    }
                }
                catch (Exception e)
                {
                    this.LogErr(e);
                }
            }
            else
            {
                InstallAppBoxData();
            }
            remoteObject.IsRemote = true;
        }

        private void InstallAppBoxData()
        {
            configAppBox.ConfigWorkSpaceDir.LocalURIs.Add(DefaultWorkSpaceDir);
            if (!Directory.Exists(DefaultWorkSpaceDir))
                Directory.CreateDirectory(DefaultWorkSpaceDir);
            configAppBox.ConfigWorkSpaceDir.RemoteURIs.Add("../AppBoxServer");
            if (!Directory.Exists(ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs.First()))
                Directory.CreateDirectory(ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs.First());
            YmlHelper.Save(ConfigAppBox.ConfigPath, configAppBox);
            localObject.SaveWSWhenAppAppend(new AppItem()
            {
                Name = "Test",
                Version = "1.0",
                Category = "调试",
                IconFile = "logo.ico",
                InitFile = "Test.exe",
                Size = "1MB",
                Uri = "Test",
            });
        }

        public ConfigAppList LoadConfigAppList(string configAppListDirPath = null)
        {
            ConfigAppList configAppList = new ConfigAppList(configAppListDirPath);
            try
            {
                //如果网络url需要下载文件
                var file = Path.Combine(configAppListDirPath, ConfigAppList.ConfigPath);
                if (!File.Exists(file))
                {
                    //下载
                    var Dl = new Common.Downloader();
                    Dl.DownloadFile(ConfigAppList.ConfigPath, file, x =>
                    {
                    }, 5);
                    file = ConfigAppList.ConfigPath;
                    if (!File.Exists(file))
                    {
                        this.MessageBoxShow($"下载服务器应用列表配置失败!\n{configAppListDirPath}");
                        return configAppList;
                    }
                    YmlHelper.Load(file, ref configAppList);
                    configAppList.WorkSpaceDir = configAppListDirPath;
                }
                else
                {
                    YmlHelper.Load(Path.Combine(configAppListDirPath, ConfigAppList.ConfigPath), ref configAppList);
                    configAppList.WorkSpaceDir = configAppListDirPath;
                }
                configAppList.LoadAppList();
            }
            catch (Exception ex)
            {
                this.MessageBoxShow(ex.Message);
            }
            return configAppList;
        }

        public string GetDefaultRemoteUri()
        {
            if (ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs.Count > 0)
                return ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs.First();
            return "";
        }

        public ConfigAppList GetLocalURIAppList(string localURI)
        {
            InitWS(localURI);
            var LocalObject = LoadConfigAppList(localURI);
            LocalObjects[localURI] = LocalObject;
            return LocalObjects[localURI];
        }

        public static void InitWS(string WorkSpaceDir)
        {
            var file = Path.Combine(WorkSpaceDir, ConfigAppList.ConfigPath);
            if (!File.Exists(file))
            {
                if (!Directory.Exists(WorkSpaceDir))
                    Directory.CreateDirectory(WorkSpaceDir);
                YmlHelper.Save(Path.Combine(WorkSpaceDir, ConfigAppList.ConfigPath), new ConfigAppList());
            }
        }


    }
}