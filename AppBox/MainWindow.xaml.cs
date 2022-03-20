using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AppBox.Ext;
using Common;
using Application = System.Windows.Application;
using ComboBox = System.Windows.Controls.ComboBox;
using Window = HandyControl.Controls.Window;

namespace AppBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainVM MainVM;
        public MainWindow()
        {
            MainServices.InitCapExption(AppDomain.CurrentDomain, System.Windows.Application.Current);
            InitResources();
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                //进入配置界面
                var ConfigCMD = args[1];
                var cfgpath = Path.GetFileName(ConfigCMD);
                object obj = "";
                if (cfgpath == ConfigAppList.ConfigPath)
                {
                    var cl = new ConfigAppList();
                    YmlHelper.Load(ConfigCMD, ref cl);
                    obj = cl;
                    AppBox.Ext.ConfigWindow.Config(obj, true, false);
                    YmlHelper.Save(ConfigCMD, cl);
                }
                else if (cfgpath == ConfigAppBox.ConfigPath)
                {
                    var cb = new ConfigAppBox();
                    YmlHelper.Load(ConfigCMD, ref cb);
                    obj = cb;
                    AppBox.Ext.ConfigWindow.Config(obj, true, false);
                    YmlHelper.Save(ConfigCMD, cb);
                }
                else if (args.Length >= 4 && ConfigCMD == "UploadByAppDir")
                {
                    var vm = new MainVM(false);
                    var RemotePath = args[2];
                    var AppDir = args[3];
                    var AppInitFile = args[4];
                    var winCloseCall = () => {
                        if (MainVM == null)
                        {
                            ModalVMExtensions.ModalVM?.Dispose();
                            Environment.Exit(0);
                        }
                    };
                    this.Hide();
                    this.MessageBoxAskText($"是否上传应用?\n{RemotePath}\n{AppDir}\n{AppInitFile}\n{vm.MainTask.GetFileVersion(AppDir, AppInitFile)}\n\n请输入应用描述信息：", "", "确认上传",
                        dept => {
                            if (RemotePath != "_")
                                推送上传(vm, RemotePath, AppDir, AppInitFile, dept, winCloseCall);
                            else
                                this.MessageBoxAskSelect($"选择上传到哪里", vm.ConfigAppBox.ConfigWorkSpaceDir.RemoteURIs,
                                    x =>
                            {
                                if (string.IsNullOrWhiteSpace(x))
                                    return "不能为空";
                                RemotePath = x;
                                return "";
                            }
                                    , vm.RemoteURI
                                    , null
                                    , () =>
                            {
                                推送上传(vm, RemotePath, AppDir, AppInitFile, dept, winCloseCall);
                            }
                                    , winCloseCall);
                        }, winCloseCall, winCloseCall);
                }
                else
                {
                    DataContext = obj = MainVM = new MainVM();
                    AppBox.Ext.ConfigWindow.Config(obj, true, false);
                    if (MainVM == null)
                    {
                        ModalVMExtensions.ModalVM?.Dispose();
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                if (MainVM == null)
                    DataContext = MainVM = new MainVM();
                Loaded +=MainWindow_Loaded;
                tabMain.SelectionChanged +=TabMain_SelectionChanged;
                Closing += MainWindow_Closing;
                InitTheme();
            }
        }

        private void InitTheme()
        {
            MoreWin.ConfigAppBox = MainVM.ConfigAppBox;
            if (MoreWin.ConfigAppBox?.Theme != null)
                MoreWin.SwicthColor(MoreWin.ConfigAppBox.Theme);
        }

        public void InitResources()
        {
            UIInvokeHelper.InvokeExec = x => Application.Current.Dispatcher.Invoke(x);
            UISelectFileHelper.SelectFileHelper = new WinSelectFileHelper();
            InitImage();
            WorkspaceHelper.ClipboardSetText = v => Clipboard.SetText(v);
            ModalVMExtensions.ModalVM = new WPFModalWinVM();
        }

        private void InitImage()
        {
            AppItem.WaitInstallIcon = ImageHelper.GetFileIconImageSource(
                Process.GetCurrentProcess().MainModule.FileName
                , GetSystemIcon.IMAGELIST_SIZE_FLAG.SHIL_EXTRALARGE);
            AppItemHelper.GetIconByIconDeptHanler = GetIconByIconDeptHanler;
            ConfigAppList.ImageFilePathToIconDept = ImageHelper.ImageFilePathToIconDept;
            ConfigAppList.ImageSourceToIconDept = o => ImageHelper.ImageSourceToIconDept((ImageSource)o);
        }

        public object GetIconByIconDeptHanler(AppItem app)
        {
            if (!string.IsNullOrWhiteSpace(app.IconDept))
            {
                var img = ImageHelper.IconDeptToImageSource(app.IconDept);
                if (img != null)
                    return img;
            }
            if (MainVM.Instance?.LocalObject?.WorkSpaceDir == null)
                return AppItem.WaitInstallIcon;
            var url = app.InitPath(MainVM.Instance.LocalObject.WorkSpaceDir);
            if (File.Exists(url))
            {
                return ImageHelper.GetFileIconImageSource(
                url
                , GetSystemIcon.IMAGELIST_SIZE_FLAG.SHIL_EXTRALARGE);
            }
            else
                return AppItem.WaitInstallIcon;
        }

        private void 推送上传(MainVM vm, string RemotePath, string AppDir, string AppInitFile,string Description, Action winCloseCall)
        {
            this.MessageBoxBusy($"推送{AppInitFile.ToString()}",
                () =>
                {
                    vm.MainTask.UploadByAppDir(RemotePath, AppDir, AppInitFile, Description, winCloseCall);
                });
        }

        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabRemote.IsSelected)
            {
                //TabRemote_Loaded(null, null);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //string filePath = "demosds.sti";
            //XmlHelper.Parse(filePath);
            //加载远程空间列表
            cbRemote.SelectedItem = null;
            (cbRemote as ComboBox).SelectionChanged += (s, e) => {
                if (cbRemote.SelectedItem is string uri)
                {
                    MainVM.Instance.MessageBoxBusy("正在加载...", () =>
                    {
                        MainVM.RemoteURI = uri;
                    }
                    , null, true);
                }
            };
        }
        bool IsInitedRemote = false;
        private void TabRemote_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsInitedRemote)
            {
                IsInitedRemote = true;
                MainVM.Instance.MessageBoxBusy("正在加载...", () =>
                {
                    MainVM.RemoteURI = (MainVM.GetDefaultRemoteUri());
                }
                , null, true);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState != WindowState.Minimized)
            {
                // 取消关闭操作
                e.Cancel = true;
                // 最小化窗口
                WindowState = WindowState.Minimized;
            }
            else
            {
                ModalVMExtensions.ModalVM?.Dispose();
                Environment.Exit(0);
            }
        }

        private void 更多(object sender, RoutedEventArgs e)
        {
            var win = new MoreWin();
            MoreWin.ConfigAppBox = MainVM.ConfigAppBox;
            win.ShowDialog();
        }
    }

}
