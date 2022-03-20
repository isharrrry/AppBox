using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppBox.Ext
{
    public class ImageHelper
    {
        public static ImageSource IconDeptToImageSource(string iconDept)
        {
            if (iconDept.StartsWith("data:image") && iconDept.Split("base64,") is string[] dats && dats.Length >= 2)
            {
                return ImgMgrHelper.StrToImg(dats[1]);
            }
            return null;
        }

        public static ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            ImageSource imageSource;
            using (Bitmap bmp = bitmap)
            {
                var stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                imageSource = BitmapFrame.Create(stream);
            }
            return imageSource;
        }
        /// <summary>
        /// 文件获取图标
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ImageSource GetFileIconImageSource(string path, GetSystemIcon.IMAGELIST_SIZE_FLAG _iconSize)
        {
            path = path.Replace("/", "\\");
            if (!File.Exists(path))
            {
                return null;
            }
            ImageSource imageSource;
            using (var fileIcon =
                GetSystemIcon.GetIconFromFile(
                path,
                GetSystemIcon.IMAGELIST_SIZE_FLAG.SHIL_EXTRALARGE))
            {
                if (fileIcon == null)
                    return null;
                imageSource = ImageHelper.BitmapToImageSource(fileIcon.ToBitmap());
            }
            return imageSource;
        }

        public static string ImageSourceToIconDept(ImageSource soc)
        {
            return "data:image/jpeg;base64," + ImgMgrHelper.ImgToStr(soc);
        }

        public static string ImageFilePathToIconDept(string imgpath)
        {
            if (ImgMgrHelper.ByteToImg(ImgMgrHelper.FileToByte(imgpath)) is ImageSource img)
            {
                return ImageHelper.ImageSourceToIconDept(img);
            }
            return null;
        }
    }
}
