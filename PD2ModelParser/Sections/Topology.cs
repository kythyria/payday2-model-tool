using System;
using System.Collections.Generic;
using System.IO;

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
                a = (ushort) (a + offset),
                b = (ushort) (b + offset),
                c = (ushort) (c + offset)
            };
        }

        public bool BoundsCheck(int vertlen)
        {
            return a >= 0 && b >= 0 && c >= 0 && a < vertlen && b < vertlen && c < vertlen;
        }

        public override string ToString()
        {
            return $"{a}, {b}, {c}";
        }
    }

    [SectionId(Tags.topology_tag)]
    class Topology : AbstractSection, ISection, IHashNamed
    {
        public UInt32 unknown1;
        public List<Face> facelist = new List<Face>();
        public UInt32 count2;
        public byte[] items2;
        public HashName HashName { get; set; }

        public byte[] remaining_data = null;

        public Topology(string objectName)
        {
            this.unknown1 = 0;

            this.count2 = 0;
            this.items2 = new byte[0];
            this.HashName = new HashName(objectName + ".Topology");
        }

        public Topology(obj_data obj) : this(obj.object_name)
        {
            this.facelist = obj.faces;
        }

        public Topology(BinaryReader instream, SectionHeader section)
        {
            SectionId = section.id;
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
            this.items2 = instream.ReadBytes((int) this.count2);
            this.HashName = new HashName(instream.ReadUInt64());

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
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
            outstream.Write(this.HashName.Hash);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            return base.ToString() +
                   " unknown1: " + this.unknown1 +
                   " facelist: " + this.facelist.Count +
                   " count2: " + this.count2 +
                   " items2: " + this.items2.Length +
                   " HashName: " + this.HashName.String +
                   (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
