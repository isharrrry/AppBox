using System;
using System.Collections.Generic;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;
using YamlDotNet.Serialization;
using Common;

namespace AppBox
{
    public class ConfigAppBox
    {
        public static string ConfigPath = "ConfigAppBox.yml";
        public ConfigAppBox() { }
        public ConfigWorkSpaceDir ConfigWorkSpaceDir { get; set; } = new ConfigWorkSpaceDir();
        public ConfigAppServer ConfigAppServer { get; set; } = new ConfigAppServer();
        //public List<KeyValuePair<string, string>> AppUpdateNameMap { get; set; } = new List<KeyValuePair<string, string>>();
        public Dictionary<string, string> AppUpdateInitFileMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> AppUpdateNameMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> AppUpdateVersionFileMap { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 仅用于命令行推送时使用
        /// </summary>
        public string UploadToken { get; set; } = "";
        public string Theme { get; set; } = "Default";

        [YamlIgnore]
        public static string Token = "";

        public static HttpResponse UploadFile(BinaryReader br, string filename, string RemoteURI, AppItem appItem, string token)
        {
            var IPHost = "http://localhost:7500";
            var wsdir = "";
            Uri uri = new Uri(RemoteURI);
            var leftPart = uri.GetLeftPart(UriPartial.Authority);
            wsdir = RemoteURI.Substring(leftPart.Length + 1);
            if (wsdir.Contains("?"))
            {
                wsdir = wsdir.Split('?')[0];
            }
            IPHost = leftPart;

            //var buffer = new byte[1024 * 64];
            HttpResponse rsp = new HttpResponse() { StatusCode = -1, StatusMessage = $"服务器连接失败：\n{IPHost}" };
            try { 
                using (var client = new HttpClient())
                {
                    client.Setup(new TouchSocketConfig()
                        .SetRemoteIPHost(new IPHost(IPHost)));
                    var en = client.TryConnect();//先做连接
                    if (!en.IsSuccess())
                    {
                        return rsp;
                    }
                                     //创建一个请求
                    using (var request = new HttpRequest(client))
                    {
                        request.InitHeaders()
                            .SetUrl($"/uploadfile")
                            .SetHost(client.RemoteIPHost.Host)
                            .AsPost()
                            .AddQuery("token", GetUrlString(token))
                            .AddQuery("filename", GetUrlString(filename))
                            .AddQuery("wsdir", GetUrlString(wsdir))
                            .AddQuery("Uri", GetUrlString(appItem.Uri))
                            .AddQuery("Version", GetUrlString(appItem.Version))
                            .AddQuery("Category", GetUrlString(appItem.Category))
                            .AddQuery("InitFile", GetUrlString(appItem.InitFile))
                            .AddQuery("Description", GetUrlString(appItem.Description))
                            ;
                        if (appItem.AppItemType == AppItemType.应用)
                            request.AddQuery("AppItemType", AppItemType.应用包.ToString());
                        else
                            request.AddQuery("AppItemType", appItem.AppItemType.ToString());

                        request.SetContent(br.ReadBytes((int)br.BaseStream.Length));
                        var respose = client.Request(request, false, 1000 * 30);
                        client.Close();
                        rsp = respose;
                        //调用 WriteContent后拿到HttpResponse的方法
                        //request.ContentLength = br.BaseStream.Length;
                        //while (true)//当数据太大时，可持续读取
                        //{
                        //    var r = br.Read(buffer, 0, buffer.Length);
                        //    if (r == 0)
                        //        break;
                        //    else
                        //    {
                        //        //这里可以一直处理读到的数据。
                        //        request.WriteContent(buffer, 0, r);
                        //    }
                        //}
                    }
                }
            }
            catch (Exception e) { e.Log(e, LogType.ERROR); }
            return rsp;
        }
        public static HttpResponse UploadDept(string RemoteURI, AppItem appItem, string token)
        {
            var IPHost = "http://localhost:7500";
            var wsdir = "";
            Uri uri = new Uri(RemoteURI);
            var leftPart = uri.GetLeftPart(UriPartial.Authority);
            wsdir = RemoteURI.Substring(leftPart.Length + 1);
            if (wsdir.Contains("?"))
            {
                wsdir = wsdir.Split('?')[0];
            }
            IPHost = leftPart;

            //var buffer = new byte[1024 * 64];
            HttpResponse rsp = new HttpResponse() { StatusCode = -1, StatusMessage = $"服务器连接失败：\n{IPHost}" };
            try { 
                using (var client = new HttpClient())
                {
                    client.Setup(new TouchSocketConfig()
                        .SetRemoteIPHost(new IPHost(IPHost)));
                    var en = client.TryConnect();//先做连接
                    if (!en.IsSuccess())
                    {
                        return rsp;
                    }
                    //创建一个请求
                    using (var request = new HttpRequest(client))
                    {
                        request.InitHeaders()
                            .SetUrl($"/uploaddept")
                            .SetHost(client.RemoteIPHost.Host)
                            .AsPost()
                            .AddQuery("token", GetUrlString(token))
                            .AddQuery("wsdir", GetUrlString(wsdir))
                            .AddQuery("Uri", GetUrlString(appItem.Uri))
                            .AddQuery("Version", GetUrlString(appItem.Version))
                            .AddQuery("Category", GetUrlString(appItem.Category))
                            .AddQuery("InitFile", GetUrlString(appItem.InitFile))
                            .AddQuery("Description", GetUrlString(appItem.Description))
                            ;
                        if (appItem.AppItemType == AppItemType.应用)
                            request.AddQuery("AppItemType", AppItemType.应用包.ToString());
                        else
                            request.AddQuery("AppItemType", appItem.AppItemType.ToString());

                        var respose = client.Request(request, false, 1000 * 30);
                        client.Close();
                        rsp = respose;
                    }
                }
            }
            catch (Exception e) { e.Log(e, LogType.ERROR); }
            return rsp;
        }
        public static HttpResponse DeleteBy(string RemoteURI, AppItem appItem, string token)
        {
            var IPHost = "http://localhost:7500";
            var wsdir = "";
            Uri uri = new Uri(RemoteURI);
            var leftPart = uri.GetLeftPart(UriPartial.Authority);
            wsdir = RemoteURI.Substring(leftPart.Length + 1);
            if (wsdir.Contains("?"))
            {
                wsdir = wsdir.Split('?')[0];
            }
            IPHost = leftPart;

            //var buffer = new byte[1024 * 64];
            HttpResponse rsp = new HttpResponse() { StatusCode = -1, StatusMessage = $"服务器连接失败：\n{IPHost}" };
            try
            {
                using (var client = new HttpClient())
                {
                    client.Setup(new TouchSocketConfig()
                        .SetRemoteIPHost(new IPHost(IPHost)));
                    var en = client.TryConnect();//先做连接
                    if (!en.IsSuccess())
                    {
                        return rsp;
                    }
                    //创建一个请求
                    using (var request = new HttpRequest(client))
                    {
                        request.InitHeaders()
                            .SetUrl($"/deleteby")
                            .SetHost(client.RemoteIPHost.Host)
                            .AsPost()
                            .AddQuery("token", GetUrlString(token))
                            .AddQuery("wsdir", GetUrlString(wsdir))
                            .AddQuery("Uri", GetUrlString(appItem.Uri))
                            .AddQuery("Version", GetUrlString(appItem.Version))
                            .AddQuery("InitFile", GetUrlString(appItem.InitFile))
                            ;
                        request.AddQuery("AppItemType", appItem.AppItemType.ToString());

                        var respose = client.Request(request, false, 1000 * 30);
                        client.Close();
                        rsp = respose;
                    }
                }
            }
            catch (Exception e) { e.Log(e, LogType.ERROR); }
            return rsp;
        }

        private static string GetUrlString(string uri)
        {
            return (uri ?? "").Replace(" ", "%20");
        }

        public static HttpResponse UploadFile(string path, string RemoteURI, AppItem appItem, string token)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fileStream))
            {
                return UploadFile(br, Path.GetFileName(path), RemoteURI, appItem, token);
            }
            //var client = new HttpClient();
            //client.Setup(new TouchSocketConfig()
            //    .SetRemoteIPHost(new IPHost("http://localhost:7500")));
            //client.Connect();//先做连接
            ////创建一个请求
            //var request = new HttpRequest();
            //request.InitHeaders()
            //    .SetUrl($"/uploadfile")//?token={HttpAppUploadPlug.Token}
            //    .SetHost(client.RemoteIPHost.Host)
            //    .AsPost()
            //    .AddQuery("token", HttpAppUploadPlug.Token);
            //var respose = client.Request(request, false, 1000 * 30);
            ////respose.FromFile(path, request, Path.GetFileName(path));
            //client.Close();
            //return respose;
        }
    }



}
