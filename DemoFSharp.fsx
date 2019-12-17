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


@"c:\Pluton\WebP.NET\ImgExamples\Lossy.webp"
|> WebP.load
|> fun x ->
    let bf = BitmapFrame()
    use be =
    

//var backBuffer = new byte[stride * normalizedBitmap.PixelHeight];
//normalizedBitmap.CopyPixels(backBuffer, stride, 0);

(*let (+/) path1 path2 = Path.Combine(path1, path2)

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

imgDir |> Process.Start*)