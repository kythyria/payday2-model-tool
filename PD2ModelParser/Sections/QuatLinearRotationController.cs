using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    public class  QuatLinearRotationController_KeyFrame
    {
        public float timestamp;
        public Quaternion rotation;

        public QuatLinearRotationController_KeyFrame(BinaryReader instream)
        {
            this.timestamp = instream.ReadSingle();
            this.rotation = new Quaternion(instream.ReadSingle(), instream.ReadSingle(), instream.ReadSingle(), instream.ReadSingle()); //Might be wrong order
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.timestamp);
            outstream.Write(this.rotation.X);
            outstream.Write(this.rotation.Y);
            outstream.Write(this.rotation.Z);
            outstream.Write(this.rotation.W);
        }

        public override string ToString()
        {
            return "Timestamp=" + this.timestamp + " Rotation=[X=" + this.rotation.X + ", Y=" + this.rotation.Y + ", Z=" + this.rotation.Z + ", W=" + this.rotation.W + "]";
        }

    }
    
    [SectionId(Tags.quatLinearRotationController_tag)]
    class QuatLinearRotationController : AbstractSection, ISection
    {
        public UInt32 size;

        public UInt64 hashname; //Hashed material name (see hashlist.txt)
        public Byte flag0; // 2 = Loop?
        public Byte flag1;
        public Byte flag2;
        public Byte flag3;

        public UInt32 unknown1;
        public float keyframe_length;
        public UInt32 keyframe_count;
        public List<QuatLinearRotationController_KeyFrame> keyframes = new List<QuatLinearRotationController_KeyFrame>();

        
        public byte[] remaining_data = null;

        public QuatLinearRotationController(BinaryReader instream, SectionHeader section)
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

            for(int x = 0; x < this.keyframe_count; x++)
            {
                this.keyframes.Add(new QuatLinearRotationController_KeyFrame(instream));
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

            foreach (QuatLinearRotationController_KeyFrame item in this.keyframes)
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
            foreach (QuatLinearRotationController_KeyFrame item in this.keyframes)
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
