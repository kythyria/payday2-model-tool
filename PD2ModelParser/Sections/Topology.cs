using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    /** A triangular face */
    public class Face
    {
        /** The index of the first vertex in this face */
        public ushort a;

        /** The index of the second vertex in this face */
        public ushort b;

        /** The index of the third (last) vertex in this face */
        public ushort c;

        public Face OffsetBy(int offset)
        {
            return new Face
            {
                a = (ushort)(a + offset),
                b = (ushort)(b + offset),
                c = (ushort)(c + offset)
            };
        }

        public bool BoundsCheck(int vertlen)
        {
            return a >= 0 && b >= 0 && c >= 0 && a < vertlen && b < vertlen && c < vertlen;
        }
    }

    class Topology
    {
        private static uint topology_tag = 0x4C507A13; // Topology
        public UInt32 id;

        public UInt32 unknown1;
        public List<Face> facelist = new List<Face>();
        public UInt32 count2;
        public byte[] items2;
        public UInt64 hashname;  //Hashed name of this topology (see hashlist.txt)

        public byte[] remaining_data = null;

        public Topology(uint sec_id, obj_data obj)
        {
            this.id = sec_id;

            this.unknown1 = 0;
            this.facelist = obj.faces;

            this.count2 = 0;
            this.items2 = new byte[0];
            this.hashname = Hash64.HashString(obj.object_name + ".Topology");
        }

        public Topology(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;

            this.unknown1 = instream.ReadUInt32();
            uint count1 = instream.ReadUInt32();
            for (int x = 0; x < count1 / 3; x++)
            {
                Face face = new Face();
                face.a = instream.ReadUInt16();
                face.b = instream.ReadUInt16();
                face.c = instream.ReadUInt16();
                this.facelist.Add(face);
            }
            this.count2 = instream.ReadUInt32();
            this.items2 = instream.ReadBytes((int)this.count2);
            this.hashname = instream.ReadUInt64();

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(topology_tag);
            outstream.Write(this.id);
            long newsizestart = outstream.BaseStream.Position;
            outstream.Write((uint) 0);

            this.StreamWriteData(outstream);

            //update section size
            long newsizeend = outstream.BaseStream.Position;
            outstream.BaseStream.Position = newsizestart;
            outstream.Write((uint)(newsizeend - (newsizestart + 4)));

            outstream.BaseStream.Position = newsizeend;
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.unknown1);
            outstream.Write(facelist.Count * 3);
            foreach (Face face in facelist)
            {
                outstream.Write(face.a);
                outstream.Write(face.b);
                outstream.Write(face.c);
            }
            outstream.Write(this.count2);
            outstream.Write(this.items2);
            outstream.Write(this.hashname);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            return "[Topology] ID: " + this.id + " unknown1: " + this.unknown1 + " facelist: " + this.facelist.Count + " count2: " + this.count2 + " items2: " + this.items2.Length + " hashname: " + StaticStorage.hashindex.GetString( this.hashname ) + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
