using Media.WebP.Extern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Media.WebP
{
    public enum WebPFormat
    {
        Undefined = 0,
        Lossy = 1,
        Lossless = 2
    }

    public class WebPFeatures
    {
        public static bool IsWebP(IntPtr sourcePtr, long length)
        {
            return (
                length > 12 &&
                Marshal.PtrToStringAnsi(sourcePtr, 4) == "RIFF" &&
                Marshal.PtrToStringAnsi(sourcePtr + 8, 4) == "WEBP"
            );
        }

        public unsafe static bool IsWebP(byte[] source)
        {
            fixed (byte* dataPtr = source)
            {
                return IsWebP((IntPtr)dataPtr, source.Length);
            }
        }

        protected WebPFeatures(
            int width,
            int height,
            bool isWebPImage,
            bool hasAlpha,
            bool hasAnimation,
            WebPFormat format,
            uint[] paddings
        )
        {
            Width = width;
            Height = height;
            IsWebPImage = isWebPImage;
            HasAlpha = hasAlpha;
            HasAnimation = hasAnimation;
            Format = format;
            Paddings = paddings;
        }
        public static WebPFeatures Create(IntPtr sourcePtr, long length)
        {
            int width = 0, height = 0;
            WebPBitstreamFeatures features = default;
            var result = NativeMethods.WebPGetFeatures(sourcePtr, (UIntPtr)length, ref features);
            return new WebPFeatures(
                width,
                height,
                result == 0,
                features.has_alpha != 0,
                features.has_animation != 0,
                (WebPFormat)features.format,
                features.pad
            );
        }

        public unsafe static WebPFeatures Create(byte[] source)
        {
            fixed (byte* dataPtr = source)
            {
                return WebPFeatures.Create((IntPtr)dataPtr, source.Length);
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsWebPImage { get; private set; }

        public bool HasAlpha { get; private set; }
        public bool HasAnimation { get; private set; }
        public WebPFormat Format { get; private set; }

        public uint[] Paddings { get; private set; }
    }
}