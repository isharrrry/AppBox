using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppBox.Ext;
public class ImgMgrHelper
{
    /// <summary>
    /// 文件转字节数组
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    static public byte[] FileToByte(string filename)
    {
        FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        fs.Close();
        fs.Dispose();
        return buffer;
    }

    //加载Byte
    /// <summary>
    /// 字节加载
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    static public ImageSource ByteToImg(byte[] buffer)
    {
        return MSToImg(new MemoryStream(buffer));
    }

    static public byte[] ImgToByte(BitmapSource img)
    {
        byte[] bytes = null;
        // 创建PngBitmapEncoder并添加BitmapSource
        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(img));
        // 将编码器的输出保存到内存中
        using (MemoryStream memoryStream = new MemoryStream())
        {
            encoder.Save(memoryStream);
            // 将内存流转换为字节数组
            bytes = memoryStream.ToArray();
        }
        return bytes;
    }

    //加载ms
    /// <summary>
    /// 流转图
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    static public ImageSource MSToImg(MemoryStream ms)
    {
        BitmapImage img = new BitmapImage();
        img.BeginInit();
        img.StreamSource = ms;
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.EndInit();
        img.Freeze();
        ms.Dispose();
        return img;
    }

    //加载base64
    static public ImageSource StrToImg(string str)
    {
        return ByteToImg(System.Convert.FromBase64String(str));
    }

    //保存base64
    static public string ImgToStr(ImageSource soc)
    {
        return System.Convert.ToBase64String(ImgToByte((BitmapSource)soc));
    }
}