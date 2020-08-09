using System;
using System.IO;
using System.ComponentModel;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.passthroughGP_tag)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    class PassthroughGP : AbstractSection, ISection, IPostLoadable
    {
        public UInt32 size = 8;
        [Category("PassthroughGP")]
        public Geometry Geometry { get; set; }
        [Category("PassthroughGP")]
        public Topology Topology { get; set; }
        public byte[] remaining_data = null;

        public PassthroughGP(Geometry geom, Topology topo)
        {
            this.Geometry = geom;
            this.Topology = topo;
        }

        public PassthroughGP(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;
            PostLoadRef<Geometry>(instream.ReadUInt32(), i => this.Geometry = i);
            PostLoadRef<Topology>(instream.ReadUInt32(), i => this.Topology = i);
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
