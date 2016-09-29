using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Mendo.UWP.Extensions
{
    /// <summary>
    /// Extension Methods for bytes, byte arrays and buffers
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Converts a byte array into a string. If no encoder is specified, UTF8 encoder is used by default.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="encoder"></param>
        /// <returns></returns>
        public static Task<String> AsStringAsync(this byte[] bytes, Encoding encoder = null)
        {
            return Task.Run(() =>
            {
                if (encoder == null)
                    encoder = Encoding.UTF8;

                return encoder.GetString(bytes, 0, bytes.Length);
            });
        }

        public static byte[] ToBytes(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Returns a task that returns the data this byte array represents as a stream
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Task<Stream> AsStreamAsync(this byte[] bytes)
        {
            return Task.Run(() =>
            {
                return bytes.AsBuffer().AsStream();
            });
        }

        public static Stream AsStream(this byte[] bytes)
        {
            return bytes.AsBuffer().AsStream();
        }
    }
}
