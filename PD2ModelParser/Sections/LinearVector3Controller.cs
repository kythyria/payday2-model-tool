using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    public class LinearVector3Controller_KeyFrame
    {
        public float timestamp;
        public Vector3D vector;

        public LinearVector3Controller_KeyFrame(BinaryReader instream)
        {
            this.timestamp = instream.ReadSingle();
            this.vector = new Vector3D(instream.ReadSingle(), instream.ReadSingle(), instream.ReadSingle()); //Might be wrong order
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.timestamp);
            outstream.Write(this.vector.X);
            outstream.Write(this.vector.Y);
            outstream.Write(this.vector.Z);
        }

        public override string ToString()
        {
            return "Timestamp=" + this.timestamp + " Vector=[X=" + this.vector.X + ", Y=" + this.vector.Y + ", Z=" + this.vector.Z + "]";
        }

    }

    class LinearVector3Controller : AbstractSection, ISection
    {
        public override uint SectionId { get; set; }
        public override uint TypeCode => Tags.linearVector3Controller_tag;
        public UInt32 size;

        public UInt64 hashname; //Hashed material name (see hashlist.txt)
        public Byte flag0; // 2 = Loop?
        public Byte flag1;
        public Byte flag2;
        public Byte flag3;

        public UInt32 unknown1;
        public float keyframe_length;
        public UInt32 keyframe_count;
        public List<LinearVector3Controller_KeyFrame> keyframes = new List<LinearVector3Controller_KeyFrame>();

        public byte[] remaining_data = null;

        public LinearVector3Controller(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;

            this.hashname = instream.ReadUInt64();
            this.flag0 = instream.ReadByte();
            this.flag1 = instream.ReadByte();
            this.flag2 = instream.ReadByte();
            this.flag3 = instream.ReadByte();
            this.unknown1 = instream.ReadUInt32();
            this.keyframe_length = instream.ReadSingle();
            this.keyframe_count = instream.ReadUInt32();

            for (int x = 0; x < this.keyframe_count; x++)
            {
                this.keyframes.Add(new LinearVector3Controller_KeyFrame(instream));
            }

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.hashname);
            outstream.Write(this.flag0);
            outstream.Write(this.flag1);
            outstream.Write(this.flag2);
            outstream.Write(this.flag3);
            outstream.Write(this.unknown1);
            outstream.Write(this.keyframe_length);
            outstream.Write(this.keyframe_count);

            foreach (LinearVector3Controller_KeyFrame item in this.keyframes)
            {
                item.StreamWriteData(outstream);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {

            string keyframes_string = (this.keyframes.Count == 0 ? "none" : "");

            bool first = true;
            foreach (LinearVector3Controller_KeyFrame item in this.keyframes)
            {
                keyframes_string += (first ? "" : ", ") + item;
                first = false;
            }

            return base.ToString() +
                " size: " + this.size +
                " HashName: " + StaticStorage.hashindex.GetString(this.hashname) +
                " flag0: " + this.flag0 +
                " flag1: " + this.flag1 +
                " flag2: " + this.flag2 +
                " flag3: " + this.flag3 +
                " unknown1: " + this.unknown1 +
                " keyframe_length: " + this.keyframe_length +
                " count: " + this.keyframe_count + " items: [ " + keyframes_string + " ] " +
                (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

    }
}
