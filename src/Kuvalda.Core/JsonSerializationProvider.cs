using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kuvalda.Core
{
    public class JsonSerializationProvider : ISerializationProvider
    {
        public void Serialize(object entity, Stream stream)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            
            var data = JsonConvert.SerializeObject(entity, Formatting.Indented);
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            
            var reader = new StreamReader(stream);
            var data = reader.ReadToEnd();
            var entity = JsonConvert.DeserializeObject<T>(data, new TreeNodeConverter());
            return entity;
        }
    }

    public class TreeNodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(TreeNode));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            
            if (jo.ContainsKey("Nodes"))
            {
                return jo.ToObject<TreeNodeFolder>(serializer);
            }
            
            return jo.ToObject<TreeNodeFile>(serializer);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}