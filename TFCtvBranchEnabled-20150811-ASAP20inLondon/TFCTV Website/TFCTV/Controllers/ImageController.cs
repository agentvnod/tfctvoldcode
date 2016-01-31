using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ImageResizer;

namespace TFCTV.Controllers
{
    public class ImageController : Controller
    {
        //
        // GET: /Image/

        public FileResult Generate(string source, int width, int height)
        {
            string url = Uri.UnescapeDataString(source);
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream stream = httpWebReponse.GetResponseStream();

            var quality = 80;
            byte[] resized;
            using (var outStream = new MemoryStream())
            {
                var settings = new ResizeSettings
                {
                    Mode = FitMode.Crop,
                    Width = width,
                    Height = height,
                    Format = "jpg"
                };
                settings.Add("quality", quality.ToString());
                ImageBuilder.Current.Build(stream, outStream, settings);
                resized = outStream.ToArray();
            }

            stream.Dispose();

            return new FileStreamResult(new MemoryStream(resized, 0, resized.Length), "image/jpeg");
        }
    }
}