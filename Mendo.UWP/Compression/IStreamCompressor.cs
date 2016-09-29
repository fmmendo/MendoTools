using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Mendo.UWP.Compression
{
    public interface IStreamCompressor
    {
        Task CompressToAsync(Stream data, Stream destination, CompressionLevel level);
        Task<byte[]> CompressToBytesAsync(Stream data, CompressionLevel level = CompressionLevel.Optimal);
        Task DecompressToAsync(Stream destination, Stream source);
    }
}
