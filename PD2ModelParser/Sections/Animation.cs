using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    public class Animation : AbstractSection, ISection
    {
        private static uint animation_data_tag = 0x5DC011B8; // Animation data
        public override uint SectionId { get; set; }
        public override uint TypeCode => animation_data_tag;
        public UInt32 size;

        public UInt64 hashname; //Hashed name for the animation (see hashlist.txt)
        public UInt32 unknown2;
        public float keyframe_length;
        public UInt32 count;
        public List<float> items = new List<float>();

        public byte[] remaining_data = null;

        public Animation(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;
            this.hashname = instream.ReadUInt64();
            this.unknown2 = instream.ReadUInt32();
            this.keyframe_length = instream.ReadSingle();
            this.count = instream.ReadUInt32();

            List<float> items = new List<float>();
            for (int x = 0; x < this.count; x++)
                this.items.Add(instream.ReadSingle());

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.hashname);
            outstream.Write(this.unknown2);
            outstream.Write(this.keyframe_length);
            outstream.Write(this.count);
            foreach (float item in this.items)
            {
                outstream.Write(item);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            return "[Animation] ID: " + this.SectionId + " size: " + this.size + " hashname: " + StaticStorage.hashindex.GetString(this.hashname) + " unknown2: " + this.unknown2 + " keyframe_length: " + this.keyframe_length + " count: " + this.count + " items: (count=" + this.items.Count + ")" + (remaining_data != null ? " REMAINING DATA! " + remaining_data.Length + " bytes" : "");
        }
    }
}
