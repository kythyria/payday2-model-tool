using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    [SectionId(Tags.passthroughGP_tag)]
    class PassthroughGP : AbstractSection, ISection
    {
        public UInt32 size;

        public UInt32 geometry_section;
        public UInt32 topology_section;

        public byte[] remaining_data = null;

        public PassthroughGP(uint sec_id, Geometry geom, Topology topo)
        {
            this.SectionId = sec_id;
            this.size = 8;
            this.geometry_section = geom.SectionId;
            this.topology_section = topo.SectionId;
        }

        public PassthroughGP(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;
            this.geometry_section = instream.ReadUInt32();
            this.topology_section = instream.ReadUInt32();
            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.geometry_section);
            outstream.Write(this.topology_section);
        }

        public override string ToString()
        {
            return base.ToString() + " size: " + this.size + " PassthroughGP_geometry_section: " + this.geometry_section + " PassthroughGP_facelist_section: " + this.topology_section + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
