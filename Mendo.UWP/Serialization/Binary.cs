﻿using Mendo.UWP.Common;
using Polenter.Serialization;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Mendo.UWP.Serialization
{
    public class Binary : Singleton<Binary>, ISerializer
    {
        private SharpSerializer serializer = null;

        public Binary()
        {
            SharpSerializerBinarySettings settings = new SharpSerializerBinarySettings(BinarySerializationMode.SizeOptimized);
            serializer = new SharpSerializer(settings);
            serializer.PropertyProvider.AttributesToIgnore.Add(typeof(IgnoreDataMemberAttribute));
        }

        /// <summary>
        /// Stream Serialization
        /// </summary>
        /// <returns></returns>

        public SerializationMode SupportedModes()
        {
            return SerializationMode.Stream;
        }

        /// <summary>
        /// STRING MODES NOT SUPPORTED FOR BINARY
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Serialize<T>(T value)
        {
            throw new NotSupportedException("Binary Serializer supports stream modes only");
        }

        /// <summary>
        /// STRING MODES NOT SUPPORTED FOR BINARY
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<string> SerializeAsync<T>(T value)
        {
            throw new NotSupportedException("Binary Serializer supports stream modes only");
        }

        public Task SerializeAsync<T>(T data, Stream stream)
        {
            return Task.Run(() => serializer.Serialize(data, stream));
        }

        /// <summary>
        /// STRING MODES NOT SUPPORTED FOR BINARY
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<T> DeserializeAsync<T>(string data)
        {
            T result = default(T);

            using (var stream = Encoding.UTF8.GetBytes(data).AsBuffer().AsStream())
            {
                result = await DeserializeAsync<T>(stream);
            }

            return result;
        }

        public Task<T> DeserializeAsync<T>(Stream stream) => Task.Run(() => Deserialize<T>(stream));


        public T Deserialize<T>(Stream stream)
        {
            T output = default(T);
            output = (T)serializer.Deserialize((Stream)stream);
            return output;
        }
    }
}
