# WebP.NET

A WebP.NET is a library for encoding / decoding images in the google [WebP format](https://en.wikipedia.org/wiki/WebP) to WPF. The Library encapsulates a C++ libwebp. The library is based on the older library [https://github.com/imazen/libwebp-net](https://github.com/imazen/libwebp-net), but that library worked only with the outdated WindowsForms [System.Drawing.Bitmap](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap). This Library works with the WPF [System.Windows.Media.BitmapSource](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.bitmapsource). 

## Requirements

- .NET framework 4.6.2 or higher
- libwebp: [libwebp.dll-0.6.0.zip](/libwebp.dll-0.6.0.zip)
- [Windows Presentation Foundation (WPF)](https://en.wikipedia.org/wiki/Windows_Presentation_Foundation)

## License:

NEW BSD License https://github.com/frank-hliva/WebP.NET/blob/master/LICENSE.md

## C# Example:

```csharp
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
            using (var fileStream = new FileStream(path, FileMode.Create))
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
```

## F# Example:

```fsharp
open System.Net
open System.Diagnostics

(* WPF Libraries *)
#r "WindowsBase"
#r "PresentationCore"
#r "PresentationFramework"
#r "System.Xaml"

(* WebP.NET *)
#r "./WebP.NET/bin/Release/WebP.NET.dll"

open System
open System.IO
open System.Windows.Media.Imaging
open Media.WebP

module WebP =

    let load (path : string) =
        use fileStream = new FileStream(path, FileMode.Open)
        WebPDecoder.Decode(fileStream)

    let save (path : string) (quality : float32) (source : BitmapSource) =
        use fileStream = new FileStream(path, FileMode.Create)
        WebPEncoder.Encode(source, fileStream, quality)
        fileStream.Flush()

let (+/) path1 path2 = Path.Combine(path1, path2)

let webClient = WebClient()

printfn "Plase wait to creation of webp images ..."
let imgDir = __SOURCE_DIRECTORY__  +/ "ImgExamples"

[
    "http://pluton.cloud/attachment/d65118ca-cd58-427a-96c5-2da3c3be541d/taylorlayos.tif", "Lossless", -1.0f
    "http://pluton.cloud/attachment/410a27bf-49da-468c-982b-7aa4417380cb/kocky.png", "Lossy", 90.0f
] 
|> List.map
    (fun (url, name, quality) -> async {
        let! data = url |> Uri |> WebClient().AsyncDownloadData
        use memoryStream = new MemoryStream(data)
        let bitmapImage = new BitmapImage()
        bitmapImage.BeginInit()
        bitmapImage.CacheOption <- BitmapCacheOption.OnLoad
        bitmapImage.StreamSource <- memoryStream
        bitmapImage.EndInit()
        let fileName = imgDir +/ (sprintf "%s.webp" name)
        bitmapImage |> WebP.save fileName quality
        printfn "Creating the %s file: %s" name fileName
    })
|> Async.Parallel
|> Async.RunSynchronously

imgDir |> Process.Start
```
