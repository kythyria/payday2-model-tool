using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PD2ModelParser.Sections
{
    /** A triangular face */
    public struct Face
    {
        /** The index of the first vertex in this face */
        public readonly ushort a;

        /** The index of the second vertex in this face */
        public readonly ushort b;

        /** The index of the third (last) vertex in this face */
        public readonly ushort c;

        public Face(ushort a, ushort b, ushort c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Face OffsetBy(int offset)
        {
            return new Face(
                (ushort) (a + offset),
                (ushort) (b + offset),
                (ushort) (c + offset)
            );
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

    [ModelFileSection(Tags.topology_tag)]
    class Topology : AbstractSection, ISection, IHashNamed
    {
        public UInt32 unknown1 { get; set; }
        public List<Face> facelist = new List<Face>();
        public UInt32 count2;
        public byte[] items2;
        public HashName HashName { get; set; }

        public byte[] remaining_data = null;

        public Topology Clone(string newName)
        {
            var dst = new Topology(newName);
            dst.unknown1 = this.unknown1;
            dst.facelist.Capacity = this.facelist.Count;
            dst.facelist.AddRange(this.facelist.Select(f => new Face(f.a, f.b, f.c )));
            dst.count2 = this.count2;
            dst.items2 = (byte[])(this.items2.Clone());
            return dst;
        }

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
                var a = instream.ReadUInt16();
                var b = instream.ReadUInt16();
                var c = instream.ReadUInt16();
                this.facelist.Add(new Face(a,b,c));
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
                   $" unknown1: {unknown1} facelist: {facelist.Count} count2: {count2}" +
                   $" items2: {items2.Length} HashName: {HashName}" +
                   (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
