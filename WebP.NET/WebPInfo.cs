using Media.WebP.Extern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Media.WebP
{
    public class WebPInfo
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

        protected WebPInfo(int width, int height, bool isWebPImage)
        {
            IsWebPImage = isWebPImage;
            Width = width;
            Height = height;
        }
        public static WebPInfo From(IntPtr sourcePtr, long length)
        {
            int width = 0, height = 0;
            var result = NativeMethods.WebPGetInfo(sourcePtr, (UIntPtr)length, ref width, ref height);
            return new WebPInfo(width, height, result != 0);
        }

        public unsafe static WebPInfo From(byte[] source)
        {
            fixed (byte* dataPtr = source)
            {
                return WebPInfo.From((IntPtr)dataPtr, source.Length);
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsWebPImage { get; private set; }
    }
}