using System;
using System.IO;
using Common;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;
using YamlDotNet.Serialization;

namespace AppBox
{
    public class ConfigAppServer
    {
        public bool IsEnable { get => isEnable; set { isEnable = (value); } }
        public bool IsAppUploadEnable { get; set; }
        public int Port { get; set; } = 7500;
        public string Token { get; set; } = "links";
        public string RootFolder { get; set; } = "./";
        [YamlIgnore]
        public HttpService HttpService;
        private bool isEnable = false;
        [YamlIgnore]
        public static ConfigAppServer Instance { get; set; }

        public void LoadServer(bool value)
        {
            Instance = this;
            try
            {
                if (!value)
                {
                    HttpService?.Stop();
                    isEnable = false;
                    return;
                }
                if (string.IsNullOrWhiteSpace(RootFolder.Trim()))
                    return;
                HttpService = new HttpService();
                var config = new TouchSocketConfig();
                config
                    .SetListenIPHosts(Port)
                    .ConfigureContainer(a =>
                    {
                        a.AddConsoleLogger();
                    })
                    .ConfigurePlugins(a =>
                    {
                        //var StaticPage = a.UseHttpStaticPage();
                        //添加静态页面文件夹
                        var folders = RootFolder.Split(";");
                        HttpAppUploadPlug.StaticDirs.Clear();
                        foreach (var item in folders)
                        {
                            var dir = item.Trim();
                            this.LogInfo($"Http目录: {dir}");
                            if (!string.IsNullOrWhiteSpace(dir))
                            {
                                if (Directory.Exists(dir))
                                    HttpAppUploadPlug.StaticDirs.Add(Path.GetFullPath(dir));
                                //StaticPage.AddFolder(dir);
                            }
                        }
                        HttpAppUploadPlug.Token = Token;
                        if (IsAppUploadEnable)
                            a.Add<HttpAppUploadPlug>();
                        //default插件应该最后添加，其作用是
                        //1、为找不到的路由返回404
                        //2、处理header为Option的探视跨域请求。
                        a.UseDefaultHttpServicePlugin();
                    });

                HttpService.Setup(config);
                HttpService.Start();
                this.LogInfo("Http服务器已启动");
                isEnable = value;
            }
            catch (Exception ex)
            {
                this.LogErr(ex);
                this.MessageBoxShow(ex.ToString());
            }
        }


        public ConfigAppServer()
        {
        }
    }
}
