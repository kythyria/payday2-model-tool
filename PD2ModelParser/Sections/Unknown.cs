using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class Unknown
    {
        public UInt32 id;
        public UInt32 size;

        public UInt32 tag;

        public byte[] data;

        public Unknown(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;
            this.size = section.size;

            this.tag = instream.ReadUInt32();

            instream.BaseStream.Position = section.offset + 12;

            this.data = instream.ReadBytes((int)section.size);
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(this.tag);
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
            outstream.Write(this.data);
        }

        public override string ToString()
        {
            return "[UNKNOWN] ID: " + this.id + " size: " + this.size + " tag: " + this.tag + " Unknown_data: " + this.data.Length;
        }
    }
}
