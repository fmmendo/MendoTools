using Mendo.UWP.Common;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Mendo.UWP.Compression
{
    /// <summary>
    /// Provides ability to compress and decompress stream data using the Gzip compression format
    /// </summary>
    public class GZip : Singleton<GZip>, IStreamCompressor
    {
        // Only one async read/write operation allowed by the framework apparently ;___;
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public async Task CompressToAsync(Stream data, Stream destination, CompressionLevel level = CompressionLevel.Optimal)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var compressionStream = new GZipStream(destination, level, true))
                {
                    await data.CopyToAsync(compressionStream).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphore?.Release();
            }
        }

        public Task<byte[]> CompressToBytesAsync(Stream data, CompressionLevel level = CompressionLevel.Optimal)
        {
            return Task.Run(async () =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                byte[] bytes = null;
                try
                {
                    using (MemoryStream destination = new MemoryStream())
                    {
                        using (var compressionStream = new GZipStream(destination, level))
                        {
                            data.CopyTo(compressionStream);
                        }

                        bytes = destination.ToArray();
                    }

                }
                finally
                {
                    semaphore?.Release();
                }

                return bytes;
            });
        }

        public Task DecompressToAsync(Stream destination, Stream source)
        {
            return Task.Run(async () =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    using (var compressionStream = new GZipStream(source, CompressionMode.Decompress, true))
                    {
                        compressionStream.CopyTo(destination);
                    }
                }
                finally
                {
                    semaphore?.Release();
                }
            });
        }
    }
}
