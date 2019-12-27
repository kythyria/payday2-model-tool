using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PD2ModelParser.Sections
{
    [Flags]
    public enum ModelProperties : uint
    {
        CastShadows = 0x00000001,
        HasOpacity = 0x00000004
    }

    public class RenderAtom
    {
        public UInt32 unknown1;
        public UInt32 vertCount;  //Number of vertices in this RenderAtom
        public UInt32 baseVertex; //Probably the offset from the start of the Topology where this RA starts, in vertices.
        public UInt32 faceCount;  //Face count
        public UInt32 material_id;

        public override string ToString()
        {
            return "{unknown1=" + this.unknown1 + " vertCount=" + this.vertCount + " unknown2=" + this.baseVertex + " faceCount=" + this.faceCount + " material_id=" + this.material_id + "}";
        }
    }

    class Model : Object3D, ISection, IPostLoadable, IHashContainer
    {
        [Category("Model")]
        [DisplayName("Version")]
        public UInt32 version { get; set; }
        //Version 6
        [Category("Model")]
        public float v6_unknown7 { get; set; }
        [Category("Model")]
        public UInt32 v6_unknown8 { get; set; }
        //Other Versions
        [Category("Model")]
        public UInt32 passthroughGP_ID { get; set; } //ID of associated PassthroughGP
        [Category("Model")]
        public UInt32 topologyIP_ID { get; set; } //ID of associated TopologyIP
        public List<RenderAtom> renderAtoms = new List<RenderAtom>();
        //public UInt32 unknown9;
        [Category("Model")]
        public UInt32 material_group_section_id { get; set; }
        [Category("Model")]
        public UInt32 lightset_ID { get; set; }

        private Vector3D bounds_min; // Z (max), X (low), Y (low)
        private Vector3D bounds_max; // Z (low), X (max), Y (max)

        [Category("Model"), DisplayName("Bounds Min"), Description("Minimum corner of the bounding box.")]
        [TypeConverter(typeof(Inspector.NexusVector3DConverter))]
        public Vector3D BoundsMin { get => bounds_min; set => bounds_min = value; }

        [Category("Model"), DisplayName("Bounds Max"), Description("Maximum corner of the bounding box.")]
        [TypeConverter(typeof(Inspector.NexusVector3DConverter))]
        public Vector3D BoundsMax { get => bounds_max; set => bounds_max = value; }

        [Category("Model")]
        public UInt32 properties_bitmap { get; set; }

        [Category("Model")]
        public float BoundingRadius { get; set; }

        [Category("Model")]
        public UInt32 unknown13 { get; set; }

        [Category("Model")]
        public UInt32 skinbones_ID { get; set; }

        public Model(string object_name, uint vertCount, uint faceCount, PassthroughGP passGP, TopologyIP topoIP, Material_Group matg, Object3D parent)
            : base(object_name, parent)
        {
            this.size = 0;
            SectionId = (uint)object_name.GetHashCode();

            this.version = 3;
            this.passthroughGP_ID = passGP.id;
            this.topologyIP_ID = topoIP.id;
            this.renderAtoms = new List<RenderAtom>();
            RenderAtom nmi = new RenderAtom
            {
                unknown1 = 0,
                vertCount = vertCount, //vert count
                baseVertex = 0,
                faceCount = faceCount, //face count
                material_id = 0
            };

            this.renderAtoms.Add(nmi);

            //this.unknown9 = 0;
            this.material_group_section_id = matg.id;
            this.lightset_ID = 0;
            this.bounds_min = new Vector3D(0, 0, 0);
            this.bounds_max = new Vector3D(0, 0, 0);
            this.properties_bitmap = 0;
            this.BoundingRadius = 1;
            this.unknown13 = 6;
            this.skinbones_ID = 0;

        }

        public Model(obj_data obj, PassthroughGP passGP, TopologyIP topoIP, Material_Group matg, Object3D parent)
            : this(obj.object_name, (uint)obj.verts.Count, (uint)obj.faces.Count, passGP, topoIP, matg, parent) { }

        public Model(BinaryReader instream, SectionHeader section)
            : base(instream)
        {
            this.size = section.size;
            SectionId = section.id;

            this.version = instream.ReadUInt32();

            if (this.version == 6)
            {
                this.bounds_min.X = instream.ReadSingle();
                this.bounds_min.Y = instream.ReadSingle();
                this.bounds_min.Z = instream.ReadSingle();

                this.bounds_max.X = instream.ReadSingle();
                this.bounds_max.Y = instream.ReadSingle();
                this.bounds_max.Z = instream.ReadSingle();
                
                this.v6_unknown7 = instream.ReadSingle();
                this.v6_unknown8 = instream.ReadUInt32();
            }
            else
            {
                this.passthroughGP_ID = instream.ReadUInt32();
                this.topologyIP_ID = instream.ReadUInt32();
                var renderAtomCount = instream.ReadUInt32();

                for (int x = 0; x < renderAtomCount; x++)
                {
                    RenderAtom item = new RenderAtom();
                    item.unknown1 = instream.ReadUInt32();
                    item.vertCount = instream.ReadUInt32(); //Verts/Uvs/Normals/etc Count
                    item.baseVertex = instream.ReadUInt32();
                    item.faceCount = instream.ReadUInt32(); //Face count
                    item.material_id = instream.ReadUInt32();
                    this.renderAtoms.Add(item);
                }

                //this.unknown9 = instream.ReadUInt32();
                this.material_group_section_id = instream.ReadUInt32();
                this.lightset_ID = instream.ReadUInt32(); // this is a section id afaik

                // Bitmap that stores properties about the model
                // Bits:
                // 1: cast_shadows
                // 3: has_opacity
                this.properties_bitmap = instream.ReadUInt32();

                // Order: maxX, minX, minY, minZ, maxX, maxY - Don't ask why.
                this.bounds_min.X = instream.ReadSingle();
                this.bounds_min.Y = instream.ReadSingle();
                this.bounds_min.Z = instream.ReadSingle();
                this.bounds_max.X = instream.ReadSingle();
                this.bounds_max.Y = instream.ReadSingle();
                this.bounds_max.Z = instream.ReadSingle();

                this.BoundingRadius = instream.ReadSingle();
                this.unknown13 = instream.ReadUInt32();
                this.skinbones_ID = instream.ReadUInt32();
            }
            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));

        }

        public override void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(Tags.model_data_tag);
            outstream.Write(SectionId);
            long newsizestart = outstream.BaseStream.Position;
            outstream.Write(this.size);

            this.StreamWriteData(outstream);

            //update section size
            long newsizeend = outstream.BaseStream.Position;
            outstream.BaseStream.Position = newsizestart;
            outstream.Write((uint)(newsizeend - (newsizestart + 4)));

            outstream.BaseStream.Position = newsizeend;
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            base.StreamWriteData(outstream);
            outstream.Write(this.version);
            if (this.version == 6)
            {
                outstream.Write(this.bounds_min.X);
                outstream.Write(this.bounds_min.Y);
                outstream.Write(this.bounds_min.Z);
                outstream.Write(this.bounds_max.X);
                outstream.Write(this.bounds_max.Y);
                outstream.Write(this.bounds_max.Z);
                outstream.Write(this.v6_unknown7);
                outstream.Write(this.v6_unknown8);
            }
            else
            {
                outstream.Write(this.passthroughGP_ID);
                outstream.Write(this.topologyIP_ID);
                outstream.Write((uint)this.renderAtoms.Count);
                foreach (RenderAtom modelitem in this.renderAtoms)
                {
                    outstream.Write(modelitem.unknown1);
                    outstream.Write(modelitem.vertCount);
                    outstream.Write(modelitem.baseVertex);
                    outstream.Write(modelitem.faceCount);
                    outstream.Write(modelitem.material_id);
                }

                //outstream.Write(this.unknown9);
                outstream.Write(this.material_group_section_id);
                outstream.Write(this.lightset_ID);

                outstream.Write(this.properties_bitmap);

                outstream.Write(this.bounds_min.X);
                outstream.Write(this.bounds_min.Y);
                outstream.Write(this.bounds_min.Z);
                outstream.Write(this.bounds_max.X);
                outstream.Write(this.bounds_max.Y);
                outstream.Write(this.bounds_max.Z);

                outstream.Write(this.BoundingRadius);
                outstream.Write(this.unknown13);
                outstream.Write(this.skinbones_ID);

            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            if (this.version == 6)
                return "[Model_v6] " + base.ToString() + " version: " + this.version + " unknown5: " + this.bounds_min + " unknown6: " + this.bounds_max + " unknown7: " + this.v6_unknown7 + " unknown8: " + this.v6_unknown8 + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
            else
            {
                var atoms_string = string.Join(",", renderAtoms.Select(i => i.ToString()));
                return base.ToString() + " version: " + this.version + " passthroughGP_ID: " + this.passthroughGP_ID + " topologyIP_ID: " + this.topologyIP_ID + " RenderAtoms: " + this.renderAtoms.Count + " items: [" + atoms_string + "] material_group_section_id: " + this.material_group_section_id + " unknown10: " + this.lightset_ID + " bounds_min: " + this.bounds_min + " bounds_max: " + this.bounds_max + " unknown11: " + this.properties_bitmap + " BoundingRadius: " + this.BoundingRadius + " unknown13: " + this.unknown13 + " skinbones_ID: " + this.skinbones_ID + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
            }
        }

        public override uint TypeCode => Tags.model_data_tag;

        public void UpdateBounds(FullModelData fmd)
        {
            if (version != 3) { return; }

            var gp = fmd.parsed_sections[passthroughGP_ID] as PassthroughGP;
            if (gp == null) { return; }

            var geo = fmd.parsed_sections[gp.geometry_section] as Geometry;
            if (geo == null) { return; }

            if(geo.verts.Count == 0) { return; }

            BoundsMax = geo.verts.Aggregate(MathUtil.Max);
            BoundsMin = geo.verts.Aggregate(MathUtil.Min);
            BoundingRadius = geo.verts.Select(i => i.Length()).Max();
        }
    }
}
