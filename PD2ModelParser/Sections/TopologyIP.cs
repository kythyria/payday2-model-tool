using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class TopologyIP : ISection
    {
        private static uint topologyIP_tag = 0x03B634BD;  // TopologyIP
        public UInt32 id;
        public UInt32 size;

        public UInt32 sectionID;

        public byte[] remaining_data = null;

        public TopologyIP(uint sec_id, uint top_id)
        {
            this.id = sec_id;
            this.size = 0;
            this.sectionID = top_id;
        }

        public TopologyIP(BinaryReader br, SectionHeader sh)
        {
            this.id = sh.id;
            this.size = sh.size;
            this.sectionID = br.ReadUInt32();
            this.remaining_data = null;
            if ((sh.offset + 12 + sh.size) > br.BaseStream.Position)
                this.remaining_data = br.ReadBytes((int)((sh.offset + 12 + sh.size) - br.BaseStream.Position));
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(topologyIP_tag);
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
            outstream.Write(this.sectionID);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            return "[TopologyIP] ID: " + this.id + " size: " + this.size + " TopologyIP sectionID: " + this.sectionID + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

        public uint SectionId
        {
            get => id;
            set => id = value;
        }

        public uint TypeCode => Tags.topologyIP_tag;
    }
}
