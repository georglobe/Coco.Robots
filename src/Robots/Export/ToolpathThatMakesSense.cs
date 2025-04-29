using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Robots;

class ToolpathThatMakesSense : IToolpath, ISerializable
{
    private List<Target> targets = new List<Target>();

    public string Name { get; set; } = string.Empty;

    public IEnumerable<Target> Targets => targets;

    /*
    public ToolpathThatMakesSense()
    {
    }
    */



    public void Add(Target target)
    {
        targets.Add(target);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }

    public IToolpath ShallowClone(List<Target>? targets = null)
    {
        throw new NotImplementedException();
    }

    public void ToJson()
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new ToolpathJsonSerializer());
        settings.Formatting = Formatting.Indented;

        string json = JsonConvert.SerializeObject(this, settings);
        Console.WriteLine(json);

        // var deserializedWrapping = JsonConvert.DeserializeObject<ToolpathThatMakesSense>(json, settings);
    }
}

