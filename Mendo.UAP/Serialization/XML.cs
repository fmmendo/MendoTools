using Mendo.UAP.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Mendo.UAP.Serialization
{
    public class XML : Singleton<XML>, ISerializer
    {
        public SerializationMode SupportedModes()
        {
            return SerializationMode.String;
        }

        #region Serialization

        public Task<String> SerializeAsync<T>(T value)
        {
            return Task.Run(() =>
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringBuilder stringBuilder = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                    OmitXmlDeclaration = true,
                };

                using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }

                return stringBuilder.ToString();
            });
        }

        public Task SerializeAsync<T>(T data, Stream stream)
        {
            throw new NotSupportedException("XML Serializer supports String modes only");
        }
        #endregion

        #region Deserialization

        public Task<T> DeserializeAsync<T>(string xml) => Task.Run(() => Deserialize<T>(xml));

        public T Deserialize<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T value = default(T);

            using (StringReader stringReader = new StringReader(xml))
            {
                object deserialized = serializer.Deserialize(stringReader);
                value = (T)deserialized;
            }

            return value;
        }

        public Task<T> DeserializeAsync<T>(Stream stream)
        {
            throw new NotSupportedException("XML Serializer supports String modes only");
        }

        public T Deserialize<T>(Stream stream)
        {
            throw new NotSupportedException("XML Serializer supports String modes only");
        }

        #endregion

    }
}
