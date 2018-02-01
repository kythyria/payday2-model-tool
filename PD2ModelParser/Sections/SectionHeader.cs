using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    public class SectionHeader
    {
        public UInt32 type;
        public UInt32 id;
        public UInt32 size;
        public long offset;

        public SectionHeader(uint sec_id)
        {
            this.id = sec_id;
        }

        public SectionHeader(BinaryReader instream)
        {
            this.offset = instream.BaseStream.Position;
            this.type = instream.ReadUInt32();
            this.id = instream.ReadUInt32();
            this.size = instream.ReadUInt32();
        }

        public override string ToString()
        {
            return "[SectionHeader] Type: " + this.type + "  ID: " + this.id + " Size: " + this.size + " Offset: " + this.offset;
        }
    }
}
