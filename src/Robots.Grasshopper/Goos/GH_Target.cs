using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;
using System.Security.Cryptography.Xml;
using Rhino.FileIO;
using Rhino.Runtime;
using Rhino;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace Robots.Grasshopper;

public class GH_Target : GH_Goo<Target>
{
    public GH_Target() { Value = Target.Default; }
    public GH_Target(GH_Target goo) { Value = goo.Value; }
    public GH_Target(Target native) { Value = native; }
    public override IGH_Goo Duplicate() => new GH_Target(this);
    public override bool IsValid => true;
    public override string TypeName => "Target";
    public override string TypeDescription => "Target";
    public override string ToString() => Value.ToString();

    public override bool CastFrom(object source)
    {
        switch (source)
        {
            case Target target:
                Value = target;
                return true;
            case GH_Point point:
                Value = new CartesianTarget(new Plane(point.Value, Vector3d.XAxis, Vector3d.YAxis));
                return true;
            case GH_Plane plane:
                Value = new CartesianTarget(plane.Value);
                return true;
            case GH_String text:
                {
                    string[] jointsText = text.Value.Split(',');

                    if (jointsText.Length != 6 && jointsText.Length != 7)
                        return false;

                    var joints = new double[jointsText.Length];

                    for (int i = 0; i < jointsText.Length; i++)
                        if (!GH_Convert.ToDouble_Secondary(jointsText[i], ref joints[i])) return false;

                    Value = new JointTarget(joints);
                    return true;
                }
        }

        return false;
    }

    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(Target)))
        {
            target = (Q)(object)Value;
            return true;
        }

        return false;
    }

    public override bool Write(GH_IWriter writer)
    {
        // writer.SetGuid("RefID", ReferenceID);
        if (m_value != null)
        {
            byte[] array = CommonObjectToByteArray(m_value);
            if (array != null)
            {
                writer.SetByteArray("ON_Data", array);
            }
        }
        return base.Write(writer);
    }

    public override bool Read(GH_IReader reader)
    {/*
        base.ReferenceID = Guid.Empty;
        Value = null;
        // ClearCaches();
        ReferenceID = reader.GetGuid("RefID");*/
        if (reader.ItemExists("ON_Data"))
        {
            byte[] byteArray = reader.GetByteArray("ON_Data");
            m_value = ByteArrayToCommonObject<Target>(byteArray);
        }

        return base.Read(reader);
    }

    internal static byte[] CommonObjectToByteArray(/*CommonObject*/ Target data, int rhinoVersion = 8)
    {
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }
        if (rhinoVersion > RhinoApp.ExeVersion)
        {
            return null;
        }
        try
        {
            SerializationOptions serializationOptions = new SerializationOptions();
            serializationOptions.RhinoVersion = rhinoVersion;
            serializationOptions.WriteUserData = true;
            StreamingContext context = new StreamingContext(StreamingContextStates.All, serializationOptions);
            MemoryStream memoryStream = new MemoryStream();
            new BinaryFormatter(null, context).Serialize(memoryStream, data);
            memoryStream.Close();
            return GH_Compression.Compress(memoryStream.GetBuffer());
        }
        catch (Exception ex)
        {/*
            ProjectData.SetProjectError(ex);
            Exception ex2 = ex;
            byte[] result = null;
            ProjectData.ClearProjectError();*/
            return null;
        }
    }

    internal static T ByteArrayToCommonObject<T>(byte[] data, int rhinoVersion = 8) where T : class
    {
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }
        if (data.Length == 0)
        {
            throw new ArgumentException("data");
        }
        if (rhinoVersion > RhinoApp.ExeVersion)
        {
            return null;
        }
        try
        {
            data = GH_Compression.Decompress(data);
            SerializationOptions serializationOptions = new SerializationOptions();
            serializationOptions.RhinoVersion = rhinoVersion;
            serializationOptions.WriteUserData = true;
            StreamingContext context = new StreamingContext(StreamingContextStates.All, serializationOptions);
            MemoryStream memoryStream = new MemoryStream(data);
            object objectValue = RuntimeHelpers.GetObjectValue(new BinaryFormatter(null, context).Deserialize(memoryStream));
            memoryStream.Close();
            if (objectValue == null)
            {
                return null;
            }
            return objectValue as T;
        }
        catch (Exception ex)
        {/*
            ProjectData.SetProjectError(ex);
            Exception ex2 = ex;
            T result = null;
            ProjectData.ClearProjectError();*/
            return null;
        }
    }
}
