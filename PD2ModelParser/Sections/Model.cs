using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    public class ModelItem
    {
        public UInt32 unknown1;
        public UInt32 vertCount; //Verts/Uvs/Normals/etc Count
        public UInt32 unknown2;
        public UInt32 faceCount; //Face count
        public UInt32 material_id;

        public override string ToString()
        {
            return "{unknown1=" + this.unknown1 + " vertCount=" + this.vertCount + " unknown2=" + this.unknown2 + " faceCount=" + this.faceCount + " material_id=" + this.material_id + "}";
        }
    }
    
    class Model
    {
        private static uint model_data_tag = 0x62212D88; // Model data
        public UInt32 id;
        public UInt32 size;

        public Object3D object3D;
        public UInt32 version;
        //Version 6
        public UInt32 v6_unknown7;
        public UInt32 v6_unknown8;
        //Other Versions
        public UInt32 passthroughGP_ID; //ID of associated PassthroughGP
        public UInt32 topologyIP_ID; //ID of associated TopologyIP
        public UInt32 count;
        public List<ModelItem> items = new List<ModelItem>();
        //public UInt32 unknown9;
        public UInt32 material_group_section_id;
        public UInt32 lightset_ID;
        public Vector3D bounds_min; // Z (max), X (low), Y (low)
        public Vector3D bounds_max; // Z (low), X (max), Y (max)
        public UInt32 properties_bitmap;
        public UInt32 unknown12;
        public UInt32 unknown13;
        public UInt32 skinbones_ID;

        public byte[] remaining_data = null;

        public Model(obj_data obj, uint passGP_ID, uint topoIP_ID, uint matg_id, Object3D parent)
        {
            this.id = (uint)obj.object_name.GetHashCode();
            this.size = 0;

            this.object3D = new Object3D(obj.object_name, parent);

            this.version = 3;
            this.passthroughGP_ID = passGP_ID;
            this.topologyIP_ID = topoIP_ID;
            this.count = 1;
            this.items = new List<ModelItem>();
            ModelItem nmi = new ModelItem();
            nmi.unknown1 = 0;
            nmi.vertCount = (uint)obj.verts.Count; //vert count
            nmi.unknown2 = 0;
            nmi.faceCount = (uint)obj.faces.Count; //face count
            nmi.material_id = 0;
            

            this.items.Add(nmi);

            //this.unknown9 = 0;
            this.material_group_section_id = matg_id;
            this.lightset_ID = 0;
            this.bounds_min.Z = 0.0f;
            this.bounds_min.X = 0.0f;
            this.bounds_min.Y = 0.0f;
            this.bounds_max.Z = 0.0f;
            this.bounds_max.X = 0.0f;
            this.bounds_max.Y = 0.0f;
            this.properties_bitmap = 0;
            this.unknown12 = 1;
            this.unknown13 = 6;
            this.skinbones_ID = 0;

        }

        public Model(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;
            this.size = section.size;

            this.object3D = new Object3D(instream);

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
                this.count = instream.ReadUInt32();

                for (int x = 0; x < this.count; x++)
                {
                    ModelItem item = new ModelItem();
                    item.unknown1 = instream.ReadUInt32();
                    item.vertCount = instream.ReadUInt32(); //Verts/Uvs/Normals/etc Count
                    item.unknown2 = instream.ReadUInt32();
                    item.faceCount = instream.ReadUInt32(); //Face count
                    item.material_id = instream.ReadUInt32();
                    this.items.Add(item);
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

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(model_data_tag);
            outstream.Write(this.id);
            long newsizestart = outstream.BaseStream.Position;
            outstream.Write(this.size);

            this.StreamWriteData(outstream);

            //update section size
            long newsizeend = outstream.BaseStream.Position;
            outstream.BaseStream.Position = newsizestart;
            outstream.Write((uint)(newsizeend - (newsizestart + 4)));

            outstream.BaseStream.Position = newsizeend;
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            this.object3D.StreamWriteData(outstream);
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
                outstream.Write(this.count);
                foreach (ModelItem modelitem in this.items)
                {
                    outstream.Write(modelitem.unknown1);
                    outstream.Write(modelitem.vertCount);
                    outstream.Write(modelitem.unknown2);
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
                return "[Model_v6] ID: " + this.id + " size: " + this.size + " Object3D: [ " + this.object3D + " ] version: " + this.version + " unknown5: " + this.bounds_min + " unknown6: " + this.bounds_max + " unknown7: " + this.v6_unknown7 + " unknown8: " + this.v6_unknown8 + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
            else
            {
                StringBuilder items_builder = new StringBuilder();
                bool first = true;
                foreach (ModelItem item in this.items)
                {
                    items_builder.Append( (first ? "" : ", ") + item.ToString());
                    first = false;
                }

                return "[Model] ID: " + this.id + " size: " + this.size + " Object3D: [ " + this.object3D + " ] version: " + this.version + " passthroughGP_ID: " + this.passthroughGP_ID + " topologyIP_ID: " + this.topologyIP_ID + " count: " + this.count + " items: [" + items_builder + "] material_group_section_id: " + this.material_group_section_id + " unknown10: " + this.lightset_ID + " bounds_min: " + this.bounds_min + " bounds_max: " + this.bounds_max + " unknown11: " + this.properties_bitmap + " unknown12: " + this.unknown12 + " unknown13: " + this.unknown13 + " skinbones_ID: " + this.skinbones_ID + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
            }
        }
    }
}
