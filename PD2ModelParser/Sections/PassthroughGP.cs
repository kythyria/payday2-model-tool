using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class PassthroughGP
    {
        private static uint passthroughGP_tag = 0xE3A3B1CA; // PassthroughGP
        public UInt32 id;
        public UInt32 size;

        public UInt32 geometry_section;
        public UInt32 topology_section;

        public byte[] remaining_data = null;

        public PassthroughGP(uint sec_id, uint geom_id, uint face_id)
        {
            this.id = sec_id;
            this.size = 8;
            this.geometry_section = geom_id;
            this.topology_section = face_id;
        }

        public PassthroughGP(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;
            this.size = section.size;
            this.geometry_section = instream.ReadUInt32();
            this.topology_section = instream.ReadUInt32();
            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(passthroughGP_tag);
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
            outstream.Write(this.geometry_section);
            outstream.Write(this.topology_section);
        }

        public override string ToString()
        {
            return "[PassthroughGP] ID: " + this.id + " size: " + this.size + " PassthroughGP_geometry_section: " + this.geometry_section + " PassthroughGP_facelist_section: " + this.topology_section + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
