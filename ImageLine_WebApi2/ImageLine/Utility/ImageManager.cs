﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace ImageLine.Utility
{
    public class ImageManager
    {

        public static string GetAppconfigSimplePath(ImageType imageType)
        {
            return imageType == ImageType.SimpleImage ? ConfigurationManager.AppSettings["simplePath"] : ConfigurationManager.AppSettings["originalPath"];
        }

        public static string CreatefilePath(string theme, ImageType imageType)
        {
            var uploadFile = HttpContext.Current.Request.Files["file"];

            string filepath = imageType == ImageType.SimpleImage 
                ? GetAppconfigSimplePath(ImageType.SimpleImage) + theme 
                : GetAppconfigSimplePath(ImageType.OriginalImage) + theme;

            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            string imageName = Path.GetFileNameWithoutExtension(uploadFile.FileName) + "_" + theme + "_" +  DateTime.Now.ToString("yymmssfff") + Path.GetExtension(uploadFile.FileName);

            var imagePath = filepath + "\\" + imageName;

            return imagePath;
        }


        public static string CreateTempfilePath()
        {
            var uploadFile = HttpContext.Current.Request.Files["file"];

            string tempPath = ConfigurationManager.AppSettings["tempPath"];

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            string imageName = Path.GetFileNameWithoutExtension(uploadFile.FileName) + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(uploadFile.FileName);

            var tempImagePath = tempPath + "\\" + imageName;

            return tempImagePath;
        }

        public static async Task SaveFileAsync(Stream fileStream, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            {
                var bytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length);
                await fs.WriteAsync(bytes, 0, bytes.Count());
            }
        }

        public static async Task<byte[]> LoadFileAsync(string filePath)
        {
            using (FileStream SourceStream = File.Open(filePath, FileMode.Open))
            {
                byte[]  result = new byte[SourceStream.Length];
                await SourceStream.ReadAsync(result, 0, (int)SourceStream.Length);
                return result;
            }

           
        }

        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sFile">原图片地址</param>
        /// <param name="dFile">压缩后保存图片地址</param>
        /// <param name="flag">压缩质量（数字越小压缩率越高）1-100</param>
        /// <param name="size">压缩后图片的最大大小</param>
        /// <param name="sfsc">是否是第一次调用</param>
        /// <returns></returns>
        public static bool CompressImage(string sFile, string dFile , int size, int flag,  int dHeight = -1, int dWidth = -1)
        {
            Image iSource = Image.FromFile(sFile);

            ImageFormat tFormat = iSource.RawFormat;

            if (dHeight == -1 && dWidth == -1)
            {
                dHeight = iSource.Height / 2;
                dWidth = iSource.Width / 2;
            }
          
            int sW = 0, sH = 0;
            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();

            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;

            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径
                    FileInfo fi = new FileInfo(dFile);
                    if (fi.Length > 1024 * size)
                    {
                        flag = flag - 10;
                        CompressImage(sFile, dFile, flag, size);
                    }
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }

        public enum ImageType
        {
            OriginalImage,
            SimpleImage,
        }
    }
}