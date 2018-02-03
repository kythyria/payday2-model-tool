using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class Bone
    {
        public UInt32 vert_count;
        public List<UInt32> verts = new List<UInt32>();

        public override string ToString()
        {
            string verts_string = (this.verts.Count == 0 ? "none" : "");

            foreach (UInt32 vert in this.verts)
            {
                verts_string += vert + ", ";
            }

            return "vert_count: " + this.vert_count + " verts: [" + verts_string + "]";
        }
    }
    
    class Bones
    {
        private static uint bones_tag = 0xEB43C77; // Bones
        public UInt32 id;
        public UInt32 size;

        public UInt32 count;
        public List<Bone> bones = new List<Bone>();

        public byte[] remaining_data = null;

        public Bones(BinaryReader instream, SectionHeader section) : this(instream)
        {
            this.id = section.id;
            this.size = section.size;

            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public Bones(BinaryReader instream)
        {
            this.count = instream.ReadUInt32();

            for (int x = 0; x < this.count; x++)
            {
                Bone bone = new Bone();
                bone.vert_count = instream.ReadUInt32();
                for (int y = 0; y < bone.vert_count; y++)
                    bone.verts.Add(instream.ReadUInt32());
                bones.Add(bone);
            }

            this.remaining_data = null;
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(bones_tag);
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
            outstream.Write(this.count);
            System.Diagnostics.Debug.Assert(this.count == this.bones.Count, "[Bones] this.count != this.bones.Count");
            foreach (Bone bone in this.bones)
            {
                outstream.Write(bone.vert_count);
                System.Diagnostics.Debug.Assert(bone.vert_count == bone.verts.Count, "[Bone] bone.vert_count != bone.verts.Count");
                foreach (UInt32 vert in bone.verts)
                    outstream.Write(vert);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {

            string bones_string = (this.bones.Count == 0 ? "none" : "");

            foreach (Bone bone in this.bones)
            {
                bones_string += bone + ", ";
            }

            return "[Bones] ID: " + this.id + " size: " + this.size + " count: " + this.count + " bones:[ " + bones_string + " ]" + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
