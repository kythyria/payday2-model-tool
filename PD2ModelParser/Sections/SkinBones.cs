using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class SkinBones
    {
        private static uint skinbones_tag = 0x65CC1825; // SkinBones

        public UInt32 id;
        public UInt32 size;

        public Bones bones;
        public UInt32 object3D_section_id;
        public UInt32 count;
        public List<UInt32> objects = new List<UInt32>();
        public List<Matrix3D> rotations = new List<Matrix3D>();
        public Matrix3D unknown_matrix = new Matrix3D();

        public byte[] remaining_data = null;

        public SkinBones(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;
            this.size = section.size;

            this.bones = new Bones(instream);
            this.object3D_section_id = instream.ReadUInt32();
            this.count = instream.ReadUInt32();
            for (int x = 0; x < this.count; x++)
                this.objects.Add(instream.ReadUInt32());
            for (int x = 0; x < this.count; x++)
            {
                this.rotations.Add(MathUtil.ReadMatrix(instream));
            }
            this.unknown_matrix = MathUtil.ReadMatrix(instream);

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position)); //If exists, this contains hashed name for this geometry (see hashlist.txt)
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(skinbones_tag);
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
            this.bones.StreamWriteData(outstream);
            outstream.Write(this.object3D_section_id);
            outstream.Write(this.count);
            foreach (UInt32 item in this.objects)
                outstream.Write(item);
            foreach (Matrix3D matrix in this.rotations)
            {
                MathUtil.WriteMatrix(outstream, matrix);
            }
            MathUtil.WriteMatrix(outstream, unknown_matrix);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            string objects_string = (this.objects.Count == 0 ? "none" : "");
            
            foreach(UInt32 obj in this.objects)
            {
                objects_string += obj + ", ";
            }

            string rotations_string = (this.rotations.Count == 0 ? "none" : "");
            
            foreach(Matrix3D rotation in this.rotations)
            {
                rotations_string += rotation + ", ";
            }

            return "[SkinBones] ID: " + this.id + " size: " + this.size + " bones: [ " + this.bones + " ] object3D_section_id: " + this.object3D_section_id + " count: " + this.count + " objects count: " + this.objects.Count + " objects:[ " + objects_string + " ] rotations count: " + this.rotations.Count + " rotations:[ " + rotations_string + " ] unknown_matrix: " + this.unknown_matrix + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
