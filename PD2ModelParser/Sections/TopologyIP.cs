using System;
using System.IO;

namespace PD2ModelParser.Sections
{
    [SectionId(Tags.topologyIP_tag)]
    class TopologyIP : AbstractSection, ISection, IPostLoadable
    {
        public UInt32 size = 0;
        public Topology Topology { get; set; }
        public byte[] remaining_data = null;

        public TopologyIP(uint sec_id, Topology top) : this(top)
        {
            this.SectionId = sec_id;
        }
        public TopologyIP(Topology top)
        {
            this.Topology = top;
        }

        public TopologyIP(BinaryReader br, SectionHeader sh)
        {
            this.SectionId = sh.id;
            this.size = sh.size;
            PostloadRef(br.ReadUInt32(), this, i => i.Topology);
            this.remaining_data = null;
            if ((sh.offset + 12 + sh.size) > br.BaseStream.Position)
                this.remaining_data = br.ReadBytes((int)((sh.offset + 12 + sh.size) - br.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.Topology.SectionId);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString() => $"{base.ToString()} size: {size} Topology sectionID: {Topology.SectionId}" + (remaining_data != null ? $" REMAINING DATA! {remaining_data.Length} bytes" : "");
    }
}
