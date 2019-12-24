using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Object3D object3D => this as Object3D;
        public UInt32 version;
        //Version 6
        public UInt32 v6_unknown7;
        public UInt32 v6_unknown8;
        //Other Versions
        public UInt32 passthroughGP_ID; //ID of associated PassthroughGP
        public UInt32 topologyIP_ID; //ID of associated TopologyIP
        public List<RenderAtom> renderAtoms = new List<RenderAtom>();
        //public UInt32 unknown9;
        public UInt32 material_group_section_id;
        public UInt32 lightset_ID;
        public Vector3D bounds_min; // Z (max), X (low), Y (low)
        public Vector3D bounds_max; // Z (low), X (max), Y (max)
        public UInt32 properties_bitmap;
        public UInt32 unknown12;
        public UInt32 unknown13;
        public UInt32 skinbones_ID;

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
            this.unknown12 = 1;
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
                
                this.v6_unknown7 = instream.ReadUInt32();
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

                this.unknown12 = instream.ReadUInt32();
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

                outstream.Write(this.unknown12);
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
                return base.ToString() + " version: " + this.version + " passthroughGP_ID: " + this.passthroughGP_ID + " topologyIP_ID: " + this.topologyIP_ID + " RenderAtoms: " + this.renderAtoms.Count + " items: [" + atoms_string + "] material_group_section_id: " + this.material_group_section_id + " unknown10: " + this.lightset_ID + " bounds_min: " + this.bounds_min + " bounds_max: " + this.bounds_max + " unknown11: " + this.properties_bitmap + " unknown12: " + this.unknown12 + " unknown13: " + this.unknown13 + " skinbones_ID: " + this.skinbones_ID + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
            }
        }

        public override uint TypeCode => Tags.model_data_tag;
    }
}
