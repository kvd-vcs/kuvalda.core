using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kuvalda.Core.Data
{
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