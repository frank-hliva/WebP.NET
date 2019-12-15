using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Media.WebP.Extern;

namespace Media.WebP
{
    public static class WebPDecoder
    {
        public static BitmapSource Decode(Stream source)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                source.CopyTo(memoryStream);
                return Decode(memoryStream.ToArray());
            }
        }

        public static unsafe BitmapSource Decode(byte[] source) {
            fixed (void* dataPtr = source) {
                return Decode((IntPtr)dataPtr, source.Length);
            }
        }

        public static BitmapSource Decode(IntPtr sourcePtr, long length) {
            int w = 0, h = 0;
            if (NativeMethods.WebPGetInfo(sourcePtr, (UIntPtr)length, ref w, ref h) == 0)
            {
                throw new Exception("Invalid WebP header detected");
            }
            var success = false;
            WriteableBitmap writeableBitmap = null;
            try {
                writeableBitmap = new WriteableBitmap(w, h, 96.0, 96.0, PixelFormats.Bgra32, null);
                writeableBitmap.Lock();
                var result = NativeMethods.WebPDecodeBGRAInto(sourcePtr, (UIntPtr)length, writeableBitmap.BackBuffer, (UIntPtr)(writeableBitmap.BackBufferStride * writeableBitmap.PixelHeight), writeableBitmap.BackBufferStride);
                if (writeableBitmap.BackBuffer != result)
                {
                    throw new Exception("Failed to decode WebP image with error " + (long)result);
                }
                success = true;
            } finally {
                if (writeableBitmap != null) writeableBitmap.Unlock();
                if (!success && writeableBitmap != null) writeableBitmap = null;
            }
            return writeableBitmap;
        }

        public static string GetDecoderVersion()
        {
            uint v = (uint)NativeMethods.WebPGetDecoderVersion();
            var revision = v % 256;
            var minor = (v >> 8) % 256;
            var major = (v >> 16) % 256;
            return major + "." + minor + "." + revision;
        }
    }
}
