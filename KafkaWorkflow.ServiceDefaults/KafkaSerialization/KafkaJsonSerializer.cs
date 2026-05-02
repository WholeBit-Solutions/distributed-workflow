using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Text;

namespace KafkaWorkflow.ServiceDefaults.KafkaSerialization
{
    public class KafkaJsonSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, KafkaJsonSerializerConfig.SerializerSettings));
        }
    }

    public class KafkaJsonDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data), KafkaJsonSerializerConfig.SerializerSettings)!;
        }
    }


    internal static class KafkaJsonSerializerConfig
    {
        public static JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            Converters =
            [
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            ]
        };
    }
}
