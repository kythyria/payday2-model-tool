using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    class Material
    {
        private static uint material_tag = 0x3C54609C; // Material
        public UInt32 id;
        public UInt32 size;

        public UInt64 hashname; //Hashed material name (see hashlist.txt)
        public byte[] skipped;
        public uint count;
        public List<MaterialItem> items = new List<MaterialItem>();

        public byte[] remaining_data = null;

        public Material(uint sec_id, string mat_name)
        {
            this.id = sec_id;
            this.size = 0;
            this.hashname = Hash64.HashString(mat_name);
            this.skipped = new byte[48];
            this.count = 0;
        }

        public Material(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;
            this.size = section.size;

            this.hashname = instream.ReadUInt64();
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
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(material_tag);
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
            outstream.Write(this.hashname);
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

            return "[Material] ID: " + this.id + " size: " + this.size + " hashname: " + StaticStorage.hashindex.GetString(this.hashname) + " count: " + this.count + " items: [ " + items_string + " ] " + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
