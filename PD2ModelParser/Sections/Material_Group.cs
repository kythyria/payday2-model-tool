using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class Material_Group : AbstractSection, ISection
    {
        private static uint material_group_tag = 0x29276B1D; // Material Group
        public UInt32 id;
        public UInt32 size;

        public UInt32 count;
        public List<UInt32> items = new List<UInt32>();

        public byte[] remaining_data = null;

        public Material_Group(uint sec_id, uint mat_id)
        {
            this.id = sec_id;
            this.size = 0;
            this.count = 1;
            this.items.Add(mat_id);
        }

        public Material_Group(uint sec_id, IEnumerable<uint> mat_ids)
        {
            this.id = sec_id;
            this.size = 0;
            this.items = mat_ids.ToList();
            this.count = (uint)this.items.Count;
        }

        public Material_Group(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;
            this.size = section.size;
            
            this.count = instream.ReadUInt32();
            for (int x = 0; x < this.count; x++)
            {
                this.items.Add(instream.ReadUInt32());
            }
            byte[] remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
            {
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
            }
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.count);
            foreach (UInt32 item in this.items)
                outstream.Write(item);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            string items_string = (this.items.Count == 0 ? "none" : "");

            foreach (UInt32 item in this.items)
            {
                items_string += item + ", ";
            }

            return "[Material Group] ID: " + this.id + " size: " + this.size + " Count: " + this.count + " Items: [ " + items_string + " ] " + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

        public override uint SectionId
        {
            get => id;
            set => id = value;
        }

        public override uint TypeCode => Tags.material_group_tag;
    }
}
