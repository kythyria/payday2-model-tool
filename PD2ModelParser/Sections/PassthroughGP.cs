using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    [SectionId(Tags.passthroughGP_tag)]
    class PassthroughGP : AbstractSection, ISection, IPostLoadable
    {
        public UInt32 size;

        UInt32 geometry_section;
        UInt32 topology_section;

        public Geometry Geometry { get => _geometry; set { geometry_section = value.SectionId; _geometry = value; } }
        public Topology Topology { get => _topology; set { topology_section = value.SectionId; _topology = value; } }

        public byte[] remaining_data = null;
        private Geometry _geometry;
        private Topology _topology;

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
            return $"{base.ToString()} size: {this.size} geometry_section: {this.geometry_section} topology_section: {this.topology_section}" + (this.remaining_data != null ? $" REMAINING DATA! {this.remaining_data.Length} bytes" : "");
        }

        public void PostLoad(uint id, Dictionary<uint, ISection> parsed_sections)
        {
            try
            {
                Geometry = (Geometry)parsed_sections[geometry_section];
            }
            catch (Exception e)
            {
                throw new Exception($"PassthroughGP {SectionId} isn't pointing to a valid Geometry", e);
            }

            try
            {
                Topology = (Topology)parsed_sections[topology_section];
            }
            catch (Exception e)
            {
                throw new Exception($"PassthroughGP {SectionId} isn't pointing to a valid Topology", e);
            }
        }
    }
}
