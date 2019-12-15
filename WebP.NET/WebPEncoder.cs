using System;
using System.Runtime.InteropServices;
using Media.WebP.Extern;
using System.IO; 
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Media.WebP
{
    /// <summary>
    /// Encodes Bitmap objects into WebP format
    /// </summary>
    public static class WebPEncoder
    {
        /// <summary>
        /// Encodes the given RGB(A) bitmap to the given stream. Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        public static void Encode(BitmapSource bitmapSource, Stream target, float quality)
        {
            IntPtr result;
            long length;
            Encode(bitmapSource, quality, out result, out length);
            try
            {
                byte[] buffer = new byte[4096];
                for (int i = 0; i < length; i += buffer.Length)
                {
                    var used = (int)Math.Min((int)buffer.Length, length - i);
                    Marshal.Copy((IntPtr)((long)result + i), buffer, 0, used);
                    target.Write(buffer, 0, used);
                }
            }
            finally
            {
                NativeMethods.WebPSafeFree(result);
            }
        }

        /// <summary>
        /// Encodes the given RGB(A) bitmap to an unmanged memory buffer (returned via result/length). Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        public static unsafe void Encode(BitmapSource bitmapSource, float quality, out IntPtr targetPtr, out long length)
        {
            if (quality < -1) quality = -1;
            if (quality > 100) quality = 100;
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            var hasAlphaChannel = pixelFormatsWithAlphaChannel.Contains(bitmapSource.Format);
            var normalizedBitmap = normalizePixelFormats(bitmapSource, hasAlphaChannel);
            var stride = normalizedBitmap.PixelWidth * (normalizedBitmap.Format.BitsPerPixel + 7) / 8;
            var backBuffer = new byte[stride * normalizedBitmap.PixelHeight];
            normalizedBitmap.CopyPixels(backBuffer, stride, 0);
            fixed (void* backBufferVoidPtr = backBuffer)
            {
                var backBufferPtr = (IntPtr)backBufferVoidPtr;
                targetPtr = IntPtr.Zero;
                if (hasAlphaChannel)
                {
                    if (quality == -1)
                    {
                        length = (long)NativeMethods.WebPEncodeLosslessBGRA(backBufferPtr, width, height, stride, ref targetPtr);
                    }
                    else
                    {
                        length = (long)NativeMethods.WebPEncodeBGRA(backBufferPtr, width, height, stride, quality, ref targetPtr);
                    }
                }
                else
                {
                    if (quality == -1)
                    {
                        length = (long)NativeMethods.WebPEncodeLosslessBGR(backBufferPtr, width, height, stride, ref targetPtr);
                    }
                    else
                    {
                        length = (long)NativeMethods.WebPEncodeBGR(backBufferPtr, width, height, stride, quality, ref targetPtr);
                    }
                }
                if (length == 0) throw new Exception("WebP encode failed!");
            }
        }

        public static string GetEncoderVersion()
        {
            uint v = (uint)NativeMethods.WebPGetEncoderVersion();
            var revision = v % 256;
            var minor = (v >> 8) % 256;
            var major = (v >> 16) % 256;
            return major + "." + minor + "." + revision;
        }

        private static HashSet<PixelFormat> pixelFormatsWithAlphaChannel = new HashSet<PixelFormat>()
        {
            PixelFormats.Bgra32,
            PixelFormats.Pbgra32,
            PixelFormats.Rgba64,
            PixelFormats.Prgba64,
            PixelFormats.Prgba128Float,
            PixelFormats.Rgba128Float
        };

        private static BitmapSource maybeReplacePixelFormat(BitmapSource bitmapSource, PixelFormat neededFormat)
        {
            return (
                bitmapSource.Format == neededFormat
                    ? bitmapSource
                    : new FormatConvertedBitmap(bitmapSource, neededFormat, null, 0)
            );
        }

        private static BitmapSource normalizePixelFormats(BitmapSource bitmapSource, bool hasAlphaChannel)
        {
            return (
                maybeReplacePixelFormat(
                    bitmapSource,
                    hasAlphaChannel
                        ? PixelFormats.Bgra32
                        : PixelFormats.Bgr24
                )
            );
        }
    }
}
