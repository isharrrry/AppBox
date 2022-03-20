using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace AppBox
{
    public class HttpAppUploadPlug : PluginBase, IHttpPlugin<IHttpSocketClient>
    {
        public static string Token { get; set; } = "";
        public static List<string> StaticDirs { get; set; } = new List<string> { };
        public static string TempDir { get; set; } = "./TempDir";
        public async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            try
            {
                if (e.Context.Request.IsPost())
                {
                    string token = e.Context.Request.Query["token"];
                    if (token != Token)
                    {
                        e.Context.Response
                            .SetStatus(403, "授权失败")
                            .Answer();
                        return;
                    }
                    if (e.Context.Request.UrlEquals("/uploaddept"))
                    {
                        string wsdir = e.Context.Request.Query["wsdir"] ?? "";
                        wsdir = Path.Combine(ConfigAppServer.Instance.RootFolder ?? "", wsdir);
                        if (!Directory.Exists(wsdir))
                        {
                            e.Context.Response
                                .SetStatus(501, $"找不到工作空间: {wsdir}")
                                .Answer();
                            return;
                        }
                        string AppItemType = e.Context.Request.Query["AppItemType"];
                        string Uri = e.Context.Request.Query["Uri"];
                        string Version = e.Context.Request.Query["Version"];
                        string Category = e.Context.Request.Query["Category"];
                        string InitFile = e.Context.Request.Query["InitFile"];
                        string Description = e.Context.Request.Query["Description"] ?? "无详情";
                        MainTask.WSInfoApp(wsdir, MainVM.Instance, AppItemType, Uri, Version, Category, InitFile, Description);
                        e.Context.Response
                                .SetStatus()
                                .FromText("Ok")
                                .Answer();
                        return;
                    }
                    else if (e.Context.Request.UrlEquals("/deleteby"))
                    {
                        string wsdir = e.Context.Request.Query["wsdir"] ?? "";
                        wsdir = Path.Combine(ConfigAppServer.Instance.RootFolder ?? "", wsdir);
                        if (!Directory.Exists(wsdir))
                        {
                            e.Context.Response
                                .SetStatus(501, $"找不到工作空间: {wsdir}")
                                .Answer();
                            return;
                        }
                        string AppItemType = e.Context.Request.Query["AppItemType"];
                        string Uri = e.Context.Request.Query["Uri"];
                        string Version = e.Context.Request.Query["Version"];
                        string InitFile = e.Context.Request.Query["InitFile"];
                        var count = MainTask.WSDeleteApp(wsdir, MainVM.Instance, AppItemType, Uri, Version, InitFile);
                        e.Context.Response
                                .SetStatus()
                                .FromText($"完成!已删除{count}条！")
                                .Answer();
                        return;
                    }
                    else if (e.Context.Request.UrlEquals("/uploadfile"))
                    {
                        string wsdir = e.Context.Request.Query["wsdir"] ?? "";
                        wsdir = Path.Combine(ConfigAppServer.Instance.RootFolder ?? "", wsdir);
                        if (!Directory.Exists(wsdir))
                        {
                            e.Context.Response
                                .SetStatus(501, $"找不到工作空间: {wsdir}")
                                .Answer();
                            return;
                        }

                        string filename = e.Context.Request.Query["filename"] ?? DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        //if (!Directory.Exists(wsdir))
                        //{
                        //    Directory.CreateDirectory(wsdir);
                        //}
                        if (e.Context.Request.ContentLength > 1024 * 1024 * 1000)//全部数据体超过1000Mb则直接拒绝接收。
                        {
                            e.Context.Response
                                .SetStatus(403, "数据过大 > 1000MB")
                                .Answer();
                            return;
                        }
                        else if (e.Context.Request.ContentLength > 1024 * 1024 * 100)//>100MB
                        {
                            var buffer = new byte[1024 * 64];
                            using (FileStream fileStream = new FileStream(Path.Combine(wsdir, filename), FileMode.Create, FileAccess.Write))
                            using (BinaryWriter writer = new BinaryWriter(fileStream))
                                while (true)//当数据太大时，可持续读取
                                {
                                    var r = e.Context.Request.Read(buffer, 0, buffer.Length);
                                    if (r == 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        //这里可以一直处理读到的数据。
                                        writer.Write(buffer, 0, r);
                                    }
                                }
                        }
                        else
                        {
                            if (e.Context.Request.TryGetContent(out var bodys))//一次性获取请求体
                            {
                                File.WriteAllBytes(Path.Combine(wsdir, filename), bodys);
                            }
                            else
                            {
                                e.Context.Response
                                        .SetStatus(501, "文件处理失败")
                                        .Answer();
                                return;
                            }
                        }
                        string AppItemType = e.Context.Request.Query["AppItemType"];
                        string Uri = e.Context.Request.Query["Uri"];
                        string Version = e.Context.Request.Query["Version"];
                        string Category = e.Context.Request.Query["Category"];
                        string InitFile = e.Context.Request.Query["InitFile"];
                        string Description = e.Context.Request.Query["Description"] ?? "无详情";
                        MainTask.WSAddApp(wsdir, MainVM.Instance, AppItemType, Uri, Version, Category, InitFile, Description);
                        //此操作会先接收全部数据，然后再分割数据。
                        //所以上传文件不宜过大，不然会内存溢出。
                        //var multifileCollection = e.Context.Request.GetMultifileCollection();
                        //foreach (var item in multifileCollection)
                        //{
                        //    var stringBuilder = new StringBuilder();
                        //    stringBuilder.Append($"文件名={item.FileName}\t");
                        //    stringBuilder.Append($"数据长度={item.Length}");
                        //    client.Logger.Info(stringBuilder.ToString());
                        //    File.WriteAllBytes(Path.Combine(wsdir, item.FileName), item.Data);
                        //}
                        e.Context.Response
                                .SetStatus()
                                .FromText("Ok")
                                .Answer();
                        return;
                    }

                }
                else if (e.Context.Request.IsGet())
                {
                    var url = e.Context.Request.RelativeURL;
                    foreach (var dir in StaticDirs)
                    {
                        var fileUri = dir + url;
                        if (dir.EndsWith('/') || dir.EndsWith('\\'))
                            fileUri = dir + url.Substring(1);
                        var fileFullPath = Path.GetFullPath(fileUri);
                        if (fileFullPath.StartsWith(dir))
                        {
                            if (!File.Exists(fileFullPath))
                            {
                                Console.WriteLine($"文件被请求[404]：{fileFullPath}");
                                e.Context.Response
                                    .SetStatus(404, $"找不到文件：{fileFullPath}").Answer();
                                return;
                            }
                            else
                            {
                                Console.WriteLine($"文件被请求：{fileFullPath}");
                                //返回文件
                                e.Context.Response
                                        .SetStatus()
                                        .FromFile(fileFullPath, e.Context.Request);
                                return;
                            }
                        }
                    }
                }
                await e.InvokeNext();
            }
            catch (Exception ex)
            {
                client.Logger.Exception(ex);
                e.Context.Response
                    .SetStatus(500, "异常")
                    .Answer();
                return;
            }
        }
    }
}
