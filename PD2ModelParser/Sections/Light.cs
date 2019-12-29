using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinaryReader = System.IO.BinaryReader;

namespace PD2ModelParser.Sections
{
    class Light : Object3D, ISection
    {
        public override uint TypeCode => Tags.light_tag;

        public byte unknown_1 { get; set; }
        public int unknown_2 { get; set; }
        public float[] Colour { get; set; }
        public float unknown_4 { get; set; }
        public float unknown_5 { get; set; }
        public float unknown_6 { get; set; }
        public float unknown_7 { get; set; }
        public float unknown_8 { get; set; }

        public Light(BinaryReader instream, SectionHeader section) : base(instream)
        {
            this.SectionId = section.id;
            this.size = section.size;

            unknown_1 = instream.ReadByte();
            unknown_2 = instream.ReadInt32();

            Colour = new float[4];
            for(var i = 0; i < 4; i++)
            {
                Colour[i] = instream.ReadSingle();
            }
            unknown_4 = instream.ReadSingle();
            unknown_5 = instream.ReadSingle();
            unknown_6 = instream.ReadSingle();
            unknown_7 = instream.ReadSingle();
            unknown_8 = instream.ReadSingle();

            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            base.StreamWriteData(outstream);
            outstream.Write(unknown_1);
            outstream.Write(unknown_2);
            for(var i = 0; i < Colour.Length; i++)
            {
                outstream.Write(Colour[i]);
            }
            outstream.Write(unknown_4);
            outstream.Write(unknown_5);
            outstream.Write(unknown_6);
            outstream.Write(unknown_7);
            outstream.Write(unknown_8);
            if(remaining_data.Length > 0)
            {
                outstream.Write(remaining_data, 0, remaining_data.Length);
            }
        }
    }
}
