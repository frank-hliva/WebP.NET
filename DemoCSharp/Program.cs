using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Media.WebP;

namespace DemoCSharp
{
    static class Program
    {
        static BitmapSource LoadWebP(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                return WebPDecoder.Decode(fileStream);
            }
        }

        static void SaveWebP(BitmapSource source, string path, float quality)
        {
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                WebPEncoder.Encode(source, fileStream, quality);
                fileStream.Flush();
            }
        }

        private static string ParentDir(string path, int level)
        {
            return level == 0 ? path : Program.ParentDir(Directory.GetParent(path).FullName, level - 1);
        }

        private static string ImgDir = Path.Combine(
            ParentDir(AppDomain.CurrentDomain.BaseDirectory, 3),
            "ImgExamples"
        );

        private static string ImgPath(string fileName)
        {
            return Path.Combine(ImgDir, fileName);
        }

        public async static Task MainAsync(string[] args)
        {
            var webClient = new WebClient();
            var imageBytes = await webClient.DownloadDataTaskAsync(@"http://pluton.cloud/attachment/d65118ca-cd58-427a-96c5-2da3c3be541d/taylorlayos.tif");
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                Console.WriteLine("Plase wait to creation of webp images ...");
                foreach (var target in new [] {
                    new { fileName = "Lossy.webp", quality = 90 }, //lossy 0..100
                    new { fileName = "Lossless.webp", quality = -1 } // -1 loseless
                })
                {
                    var path = ImgPath(target.fileName);
                    Console.WriteLine("Creating the {0} file: {1}", Path.GetFileNameWithoutExtension(path).ToLower(), path);
                    SaveWebP(bitmapImage, path, target.quality);
                }
                Process.Start(ImgDir);
                Console.WriteLine("Opening the directory...");
            }
        }

        static void Main(string[] args)
        {
            MainAsync(args);
            Console.WriteLine("Press the enter key to ending the application...");
            Console.ReadLine();
        }
    }
}
