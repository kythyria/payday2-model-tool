using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class Unknown : AbstractSection, ISection
    {
        public UInt32 tag;

        public override uint SectionId { get; set; }
        public override uint TypeCode => this.tag;

        public UInt32 size;
        public byte[] data;

        public Unknown(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;

            this.tag = instream.ReadUInt32();

            instream.BaseStream.Position = section.offset + 12;

            this.data = instream.ReadBytes((int)section.size);
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.data);
        }

        public override string ToString()
        {
            return base.ToString() + " size: " + this.size + " tag: " + this.tag + " Unknown_data: " + this.data.Length;
        }
    }
}
