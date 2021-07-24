using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XmlAttributeAttribute = System.Xml.Serialization.XmlAttributeAttribute;

using D = PD2ModelParser.Sections;

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
        Materials = 0x20,
        Animations = 0x40,
        Transform = Position|Rotation|Scale,
        Everything = NewObjects|Parents|Materials|Transform|Animations,
    }

    enum ModelDataMergeMode
    {
        None,
        Recreate,
        Overwrite,
        VertexEdit
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

    class Merge : ScriptItem, IScriptItem 
    {
        [XmlAttribute("property-merge")] public PropertyMergeFlags PropertyMerge { get; set; } = PropertyMergeFlags.Everything;
        [XmlAttribute("model-merge")] public ModelDataMergeMode ModelMergeMode { get; set; } = ModelDataMergeMode.Overwrite;
        [XmlAttribute("model-attributes")] public ModelAttributesMergeFlags AttributeMergeMode { get; set; } = ModelAttributesMergeFlags.Vertices;
        [XmlAttribute("remap-uv")] public int[] RemapUV { get; set; } = new int[0];
        [NotAttribute] public IList<IScriptItem> Script { get; set; } = new List<IScriptItem>();

        public override void ParseXml(XElement elem)
        {
            base.ParseXml(elem);
            Script = Modelscript.Script.ParseXml(elem.Elements("modelscript").First());
        }

        public override void Execute(ScriptState state)
        {
            var childData = Modelscript.Script.ExecuteItems(this.Script, state.WorkDir);

            var rootObjects = childData.SectionsOfType<D.Object3D>().Where(i => i.Parent == null);
            
            foreach(var ro in rootObjects)
            {
                MergeObject(state.Data, ro);
            }
        }

        private void MergeObject(FullModelData targetData, D.Object3D sourceObject)
        {
               
        }
    }

    class TransplantAttributes : ScriptItem, IScriptItem
    {
        [XmlAttribute("models")] public string[] Models { get; set; } = new string[0];
        [NotAttribute] public IList<IScriptItem> Script { get; set; } = new List<IScriptItem>();

        public override void ParseXml(XElement elem)
        {
            base.ParseXml(elem);
            Script = Modelscript.Script.ParseXml(elem.Elements("modelscript").First());
        }

        public override void Execute(ScriptState state)
        {
            state.Log.Status("Run donor script");
            var donor = Modelscript.Script.ExecuteItems(Script, state.WorkDir);

            foreach(var name in Models)
            {
                state.Log.Status("Transfer attributes for {0}", name);
                var src_obj = GetModel(state, donor, name, "Source");
                var dst_obj = GetModel(state, state.Data, name, "Destination");

                var src_geo = src_obj.PassthroughGP.Geometry;
                var dst_geo = dst_obj.PassthroughGP.Geometry;

                dst_geo.Headers.Clear();
                dst_geo.Headers.AddRange(src_geo.Headers);

                TransplantAttribute(src_geo.verts, dst_geo.verts);
                TransplantAttribute(src_geo.normals, dst_geo.normals);
                TransplantAttribute(src_geo.vertex_colors, dst_geo.vertex_colors);
                TransplantAttribute(src_geo.weight_groups, dst_geo.weight_groups);
                TransplantAttribute(src_geo.weights, dst_geo.weights);
                TransplantAttribute(src_geo.binormals, dst_geo.binormals);
                TransplantAttribute(src_geo.tangents, dst_geo.tangents);
                for(var i = 0; i < src_geo.UVs.Length; i++)
                {
                    TransplantAttribute(src_geo.UVs[i], dst_geo.UVs[i]);
                }

                var src_topo = src_obj.PassthroughGP.Topology;
                var dst_topo = dst_obj.PassthroughGP.Topology;

                TransplantAttribute(src_topo.facelist, dst_topo.facelist);
                TransplantAttribute(src_obj.RenderAtoms, dst_obj.RenderAtoms);
                dst_geo.vert_count = src_geo.vert_count;
            }
        }

        private void TransplantAttribute<T>(List<T> src, List<T> dest)
        {
            dest.Clear();
            dest.Capacity = src.Capacity;
            dest.AddRange(src);
        }

        private D.Model GetModel(ScriptState state, FullModelData fmd, string name, string reponame)
        {
            var mod = fmd.GetObject3DByHash(HashName.FromNumberOrString(name)) as D.Model;
            if(mod == null)
            {
                string message = string.Format("{1} object {0} is nonexistent or not a model", name, reponame);
                state.Log.Error(message);
                throw new Exception(message);
            }

            if (mod.PassthroughGP == null)
            {
                string message = string.Format("{1} model {0} has no geometry provider", name, reponame);
                state.Log.Error(message);
                throw new Exception(message);
            }

            return mod;
        }
    }
}
