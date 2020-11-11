using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmlAttributeAttribute = System.Xml.Serialization.XmlAttributeAttribute;

namespace PD2ModelParser.Modelscript
{
    [Flags]
    enum PropertyMergeFlags
    {
        None = 0,
        NewObjects = 0x1,
        Parents = 0x2,
        Position = 0x4,
        Rotation = 0x8,
        Scale = 0x10,
        Transform = Position|Rotation|Scale,
    }

    enum ModelDataMergeMode
    {
        None,
        Recreate,
        Overwrite
    }

    [Flags]
    enum ModelAttributesMergeFlags
    {
        None = 0,
        Indices = 0x01,
        Positions = 0x02,
        Normals = 0x04,
        Colors = 0x08,
        Colours = 0x08,
        Weights = 0x10,
        UV0 = 0x20,
        UV1 = 0x40,
        UV2 = 0x80,
        UV3 = 0x100,
        UV4 = 0x200,
        UV5 = 0x400,
        UV6 = 0x800,
        UV7 = 0x1000,

        UVs = UV0 | UV1 | UV2 | UV3 | UV4 | UV5 | UV6 | UV7,
        Vertices = Positions | Normals | Colors | Weights | UVs
    }

    class Merge : IScriptItem 
    {
        [XmlAttribute("property-merge")] public PropertyMergeFlags PropertyMerge { get; set; } = PropertyMergeFlags.None;
        [XmlAttribute("model-merge")] public ModelDataMergeMode ModelMergeMode { get; set; } = ModelDataMergeMode.None;
        [XmlAttribute("model-attributes")] public ModelAttributesMergeFlags AttributeMergeMode { get; set; } = ModelAttributesMergeFlags.None;

        public void Execute(ScriptState state)
        {
            throw new NotImplementedException();
        }

        public void ParseXml(XElement elem)
        {
            throw new NotImplementedException();
        }
    }
}
