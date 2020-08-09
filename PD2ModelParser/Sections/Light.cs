using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = System.IO.BinaryWriter;

namespace PD2ModelParser.Sections
{
    class LightColour
    {
        public float R { get; set; } = 0;
        public float G { get; set; } = 0;
        public float B { get; set; } = 0;
        public float A { get; set; } = 0;

        public LightColour() { }
        public LightColour(BinaryReader instream)
        {
            R = instream.ReadSingle();
            G = instream.ReadSingle();
            B = instream.ReadSingle();
            A = instream.ReadSingle();
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(R);
            outstream.Write(G);
            outstream.Write(B);
            outstream.Write(A);
        }
    }

    [ModelFileSection(Tags.light_tag)]
    class Light : Object3D, ISection
    {
        /* zdann says that
         *   color - vector3
         *   multiplier - float
         *   far_range - float
         *   spot_angle_end - float
         *   enable - bool
         *   falloff_exponent - float
         *   properties - string
         *   final_color - vector3
         *   specular_multiplier - float
         *   ambient_cube_side - vector3
         * are pertinent properties to lights */

        public byte unknown_1 { get; set; } // 1 in all known lights
        public int LightType { get; set; } // it's 1 in light_omni and 2 in light_spot
        public LightColour Colour { get; set; }
        public float NearRange { get; set; } // probably NearRange
        public float FarRange { get; set; } // probably FarRange
        public float unknown_6 { get; set; }
        public float unknown_7 { get; set; }
        public float unknown_8 { get; set; } // BitConverter.ToSingle(byte[4] { 4, 0, 0, 0 }, 0) in all known lights

        public Light(string name, Object3D parent) : base(name, parent) { }

        public Light(BinaryReader instream, SectionHeader section) : base(instream)
        {
            this.SectionId = section.id;
            this.size = section.size;

            unknown_1 = instream.ReadByte();
            LightType = instream.ReadInt32();

            Colour = new LightColour(instream);

            NearRange = instream.ReadSingle();
            FarRange = instream.ReadSingle();
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
            outstream.Write(LightType);
            Colour.StreamWriteData(outstream);
            outstream.Write(NearRange);
            outstream.Write(FarRange);
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
