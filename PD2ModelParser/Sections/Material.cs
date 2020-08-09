using System;
using System.Collections.Generic;
using System.IO;

namespace PD2ModelParser.Sections
{
    public class MaterialItem
    {
        public UInt32 unknown1;
        public UInt32 unknown2;

        public override string ToString()
        {
            return "unknown1: " + this.unknown1 + " unknown2: " + this.unknown2;
        }
    }

    [ModelFileSection(Tags.material_tag)]
    class Material : AbstractSection, ISection, IHashNamed
    {
        public UInt32 size;

        public HashName HashName { get; set; } //Hashed material name (see hashlist.txt)
        public byte[] skipped;
        public uint count;
        public List<MaterialItem> items = new List<MaterialItem>();

        public byte[] remaining_data = null;

        public Material(string mat_name)
        {

            this.size = 0;
            this.HashName = new HashName(mat_name);
            this.skipped = new byte[48];
            this.count = 0;
        }

        public Material(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;

            this.HashName = new HashName(instream.ReadUInt64());
            this.skipped = instream.ReadBytes(48);
            this.count = instream.ReadUInt32();

            for (int x = 0; x < this.count; x++)
            {
                MaterialItem item = new MaterialItem();
                item.unknown1 = instream.ReadUInt32();
                item.unknown2 = instream.ReadUInt32();
                this.items.Add(item);
            }

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data =
                    instream.ReadBytes((int) ((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.HashName.Hash);
            outstream.Write(this.skipped);
            outstream.Write(this.count);
            foreach (MaterialItem item in this.items)
            {
                outstream.Write(item.unknown1);
                outstream.Write(item.unknown2);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            string items_string = (this.items.Count == 0 ? "none" : "");

            foreach (MaterialItem item in this.items)
            {
                items_string += item + ", ";
            }

            return base.ToString() +
                   " size: " + this.size +
                   " HashName: " + this.HashName.String +
                   " count: " + this.count +
                   " items: [ " + items_string + " ] " +
                   (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
