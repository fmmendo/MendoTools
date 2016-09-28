using System;
using System.IO;
using System.Threading.Tasks;

namespace Mendo.UWP.Serialization
{
    public interface ISerializer
    {
        SerializationMode SupportedModes();

        Task<T> DeserializeAsync<T>(String data);
        Task<String> SerializeAsync<T>(T value);
        Task<T> DeserializeAsync<T>(Stream stream);
        T Deserialize<T>(Stream stream);
        Task SerializeAsync<T>(T data, Stream stream);
    }

    /// <summary>
    /// Defines the types of serialization the serializer supports.
    /// Methods will return NotSupportedExceptions depending on this value
    /// </summary>
    [Flags]
    public enum SerializationMode
    {
        /// <summary>
        /// Serializer supports serializing to and from Strings
        /// </summary>
        String,
        /// <summary>
        /// Serializer supports serializing to and from Streams
        /// </summary>
        Stream,
        /// <summary>
        /// [NOT IMPLEMENETD] - For future use only.
        /// </summary>
        All
    }
}
