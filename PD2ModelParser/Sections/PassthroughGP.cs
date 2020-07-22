using System;
using System.IO;

namespace PD2ModelParser.Sections
{
    [SectionId(Tags.passthroughGP_tag)]
    class PassthroughGP : AbstractSection, ISection, IPostLoadable
    {
        public UInt32 size;
        public Geometry Geometry { get; set; }
        public Topology Topology { get; set; }
        public byte[] remaining_data = null;

        public PassthroughGP(uint sec_id, Geometry geom, Topology topo)
        {
            this.SectionId = sec_id;
            this.size = 8;
            this.Geometry = geom;
            this.Topology = topo;
        }

        public PassthroughGP(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;
            PostloadRef(instream.ReadUInt32(), this, i => i.Geometry);
            PostloadRef(instream.ReadUInt32(), this, i => i.Topology);
            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.Geometry.SectionId);
            outstream.Write(this.Topology.SectionId);
        }

        public override string ToString()
        {
            return $"{base.ToString()} size: {this.size} geometry_section: {this.Geometry.SectionId} topology_section: {this.Topology.SectionId}" + (this.remaining_data != null ? $" REMAINING DATA! {this.remaining_data.Length} bytes" : "");
        }
    }
}
