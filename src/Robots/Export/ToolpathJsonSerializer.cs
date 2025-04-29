using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots;

public class ToolpathJsonSerializer : JsonConverter
{
    private Dictionary<TargetAttribute, int> _referenceTracker = new Dictionary<TargetAttribute, int>();
    private List<object> _objectsList = new List<object>();

    public override bool CanConvert(Type objectType)
    {
        return
            objectType == typeof(Tool) ||
            objectType == typeof(Target) ||
            objectType == typeof(Speed) ||
            objectType == typeof(Frame);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        /*
        if (reader.TokenType == JsonToken.Null) return null;

        JObject obj = JObject.Load(reader);
        
        if (obj.ContainsKey("$ref"))
        {
            int refIndex = obj["$ref"].Value<int>();
            return _objectsList[refIndex];
        }

        object result = null;

        if (objectType == typeof(Shithole))
        {
            result = obj.ToObject<Shithole>(serializer);
        }
        else if (objectType == typeof(Foo))
        {
            result = obj.ToObject<Foo>(serializer);
        }
        else if (objectType == typeof(Uber))
        {
            result = obj.ToObject<Uber>(serializer);
        }
        else if (objectType == typeof(Wrapping))
        {
            result = obj.ToObject<Wrapping>(serializer);
        }
        
        _objectsList.Add(result);

        return result;
        */

        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        switch (value)
        {
            case Target target: break;
            case Tool tool: WriteTool(tool, writer, serializer); break;
        }

        /*
        if (_referenceTracker.ContainsKey(value))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("$ref");
            writer.WriteValue(_referenceTracker[value]);
            writer.WriteEndObject();
            return;
        }

        int index = _objectsList.Count;
        _referenceTracker[value] = index;
        _objectsList.Add(value);

        writer.WriteStartObject();

        if (value is Shithole shithole)
        {
            writer.WritePropertyName("Name");
            writer.WriteValue(shithole.Name);
        }
        else if (value is Foo foo)
        {
            writer.WritePropertyName("Property");
            serializer.Serialize(writer, foo.Property);
        }
        else if (value is Uber uber)
        {
            writer.WritePropertyName("Property");
            serializer.Serialize(writer, uber.Property);
        }
        else if (value is Wrapping wrapping)
        {
            writer.WritePropertyName("Foos");
            serializer.Serialize(writer, wrapping.Foos);

            writer.WritePropertyName("Ubers");
            serializer.Serialize(writer, wrapping.Ubers);
        }
        */




        writer.WriteEndObject();
    }

    private void WriteFrame(Frame frame, JsonWriter writer, JsonSerializer serializer)
    {
        WriteTargetAttribute(frame, writer, serializer);

    }

    private void WriteSpeed(Speed speed, JsonWriter writer, JsonSerializer serializer)
    {
        WriteTargetAttribute(speed, writer, serializer);

        // TODO - check if this Speed was already serialized
    }

    private void WriteTargetAttribute(TargetAttribute attribute, JsonWriter writer, JsonSerializer serializer)
    {
        writer.WritePropertyName("Name");
        serializer.Serialize(writer, attribute.Name);
    }

    private void WriteTool(Tool tool, JsonWriter writer, JsonSerializer serializer)
    {
        WriteTargetAttribute(tool, writer, serializer);

        // TODO - check if this Tool was already serialized

        /*
        writer.WritePropertyName("Name");
        serializer.Serialize(writer, attribute.Name);
        */
    }
}
