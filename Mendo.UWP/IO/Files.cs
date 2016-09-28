using Mendo.UWP.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Mendo.UWP.IO
{
    /// <summary>
    /// Helpful stuff for file IO
    /// </summary>
    public class Files
    {
        public static async Task<T> ReadSerializedStringAsync<T>
            (StorageFile file, ISerializer serializer)
        {
            T result = default(T);

            if ((serializer.SupportedModes() & SerializationMode.String) != 0)
                throw new NotSupportedException("The given serializer does not support string deserializing");

            try
            {
                String text = await FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false);
                result = await serializer.DeserializeAsync<T>(text).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// Gets and returns data written using a serializer.
        /// </summary>
        public static async Task<T> ReadSerializedStreamAsync<T>
            (StorageFile file, ISerializer serializer)
        {
            T result = default(T);

            if ((serializer.SupportedModes() & SerializationMode.Stream) != 0)
                throw new NotSupportedException("The given serializer does not support stream deserializing");

            try
            {
                using (var dataStream = new InMemoryRandomAccessStream())
                {
                    using (var fileStream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                    {
                        await fileStream.CopyToAsync(dataStream.AsStream()).ConfigureAwait(false);
                    }

                    dataStream.Seek(0);
                    result = await serializer.DeserializeAsync<T>(dataStream.AsStream()).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }


        public static async Task<Boolean> WriteSerializedStringAsync<T>
            (StorageFile file, T value, ISerializer serializer)
        {
            if ((serializer.SupportedModes() & SerializationMode.String) != 0)
                throw new NotSupportedException("The given serializer does not support string serializing");

            try
            {
                // Open the file stream for writing
                using (Stream fileStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                {
                    // Serialize data into string
                    String text = await serializer.SerializeAsync(value).ConfigureAwait(false);
                    await FileIO.WriteTextAsync(file, text).AsTask().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async Task<Boolean> WriteSerializedStreamAsync<T>
            (StorageFile file, T value, ISerializer serializer)
        {
            if ((serializer.SupportedModes() & SerializationMode.Stream) != 0)
                throw new NotSupportedException("The given serializer does not support stream serializing");

            try
            {
                using (Stream fileStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                {
                    using (var memoryStream = new InMemoryRandomAccessStream())
                    {
                        // Serialize data into memory stream
                        await serializer.SerializeAsync(value, memoryStream.AsStream()).ConfigureAwait(false);
                        memoryStream.Seek(0);
                        await memoryStream.AsStream().CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }












        ///// <summary>
        ///// Reads the contents of a compressed storage file and returns a byte array representing the decompressed data
        ///// </summary>
        ///// <param name="file"></param>
        ///// <param name="compressor"></param>
        ///// <returns></returns>
        //public static Task<Byte[]> ReadCompressedBytesAsync(StorageFile file, IStreamCompressor compressor)
        //{
        //    return Task.Run(async () =>
        //    {
        //        byte[] bytes = null;

        //        // 1. Open the file stream for reading
        //        using (var fileStream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
        //        {
        //            // 2. Create an InMemoryRandomAccess stream to buffer our data into
        //            using (var resultStream = new MemoryStream())
        //            {
        //                // 3. Decompress the data from the file stream into our buffer stream
        //                await compressor.DecompressToAsync(resultStream, fileStream).ConfigureAwait(false);

        //                // 5. Return our buffer as a basic stream
        //                bytes = resultStream.ToArray();
        //            }
        //        }

        //        //Logger.DebugLog(bytes.Length.ToString());
        //        return bytes;
        //    });

        //    // return (await ReadCompressedStreamAsync(file, compressor).ConfigureAwait(false)).ToArray();
        //}


    }
}
