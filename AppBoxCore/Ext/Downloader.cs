using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum DownloadObjectType { Auto, Local, HTTP, FTP };
    public class Downloader
    {
        private readonly HttpClient httpClient;
        private string fileUrl;
        private string savePath;

        public Downloader(DownloadObjectType downloadObjectType = DownloadObjectType.Auto)
        {
            this.httpClient = new HttpClient();
        }


        public void DownloadFile(string savePath, string fileUrl, Action<double> progressCallback, int TimeoutSec = 0)
        {
            this.fileUrl = fileUrl;
            this.savePath = savePath;

            using (HttpClient httpClient = new HttpClient())
            {
                if (TimeoutSec != 0)
                    httpClient.Timeout = new TimeSpan(0, 0, TimeoutSec);
                HttpResponseMessage response = httpClient.GetAsync(fileUrl.Replace("\\", "/"), HttpCompletionOption.ResponseHeadersRead).Result;
                var msg = response.EnsureSuccessStatusCode();
                if (!msg.IsSuccessStatusCode)
                    return;
                //同样的，在此处可通过 ReadAsStreamAsync（）方法，以流的方式下载指定文件（或者将网络流通过 MemoryStream 转换为内存流，再转换为byte进行存储或保存），再通过 Image 对象从流中读取图片文件。
                //string retString = await response.Content.ReadAsStringAsync();
                //File.WriteAllText("D:\\index.html", retString);
                SaveFile(savePath, progressCallback, response);
            }
        }



        private static void SaveFile(string savePath, Action<double> progressCallback, HttpResponseMessage response)
        {
            using (Stream contentStream = response.Content.ReadAsStreamAsync().Result)
            {
                long totalBytes = response.Content.Headers.ContentLength ?? -1;
                long receivedBytes = 0;
                byte[] buffer = new byte[4096];
                int bytesRead;
                double progress = 0;

                using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    do
                    {
                        bytesRead = contentStream.ReadAsync(buffer, 0, buffer.Length).Result;
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        fileStream.WriteAsync(buffer, 0, bytesRead).Wait();

                        receivedBytes += bytesRead;
                        if (totalBytes > 0)
                        {
                            progress = (double)receivedBytes / totalBytes;
                            progressCallback?.Invoke(progress);
                        }
                    }
                    while (bytesRead > 0);
                }
            }
        }
    }
}

