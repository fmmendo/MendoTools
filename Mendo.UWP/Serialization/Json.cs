using Newtonsoft.Json;
using Mendo.UWP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mendo.UWP.Serialization
{
    public class Json : Singleton<Json>, ISerializer
    {
        JsonSerializer _serializer = new JsonSerializer();

        public SerializationMode SupportedModes()
        {
            return (SerializationMode.String | SerializationMode.Stream);
        }

        #region Serialization

        public Task<String> SerializeAsync<T>(T value)
        {
            return SerializeAsync<T>(value, DefaultValueHandling.Ignore, NullValueHandling.Ignore);
        }

        /// <summary>
        /// Asynchronously serializes an object into Json.
        /// Uses custom settings to ignore serializing null values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<String> SerializeAsync<T>(T value, DefaultValueHandling dvh = DefaultValueHandling.Ignore, NullValueHandling nvh = NullValueHandling.Ignore)
            => Task.Run(() => Serialize(value, dvh, nvh));

        public String Serialize<T>(T value, DefaultValueHandling dvh = DefaultValueHandling.Ignore, NullValueHandling nvh = NullValueHandling.Ignore)
        {
            string result = string.Empty;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.DefaultValueHandling = dvh;
            settings.NullValueHandling = nvh;

            result = JsonConvert.SerializeObject(value, settings);

            settings = null;

            return result;
        }

        public Task SerializeAsync<T>(T data, System.IO.Stream stream)
        {
            return Task.Run(() =>
            {
                _serializer.NullValueHandling = NullValueHandling.Ignore;
                _serializer.DefaultValueHandling = DefaultValueHandling.Ignore;

                using (StreamWriter writer = new StreamWriter(stream))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(jsonWriter, data);
                }
            });
        }

        #endregion

        #region Deserialization

        /// <summary>
        /// Asynchronously deserializes Json into the given object type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public Task<T> DeserializeAsync<T>(string json) => Task.Run(() => Deserialize<T>(json));

        public T Deserialize<T>(string json)
        {
            T result = default(T);

            if (!String.IsNullOrWhiteSpace(json))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }

        public Task<T> DeserializeAsync<T>(Stream stream) => Task.Run(() => Deserialize<T>(stream));

        public T Deserialize<T>(Stream stream)
        {
            T result = default(T);
            _serializer.NullValueHandling = NullValueHandling.Ignore;
            _serializer.DefaultValueHandling = DefaultValueHandling.Ignore;

            using (var reader = new StreamReader(stream))
            {
                result = (T)_serializer.Deserialize(reader, typeof(T));
            }

            return result;
        }

        #endregion
    }
}
