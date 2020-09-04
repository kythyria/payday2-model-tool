using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.quatLinearRotationController_tag)]
    class QuatLinearRotationController : AbstractSection, ISection, IHashNamed, IAnimationController<Quaternion>
    {
        public UInt32 size;

        public HashName HashName { get; set; }
        public Byte flag0; // 2 = Loop?
        public Byte flag1;
        public Byte flag2;
        public Byte flag3;

        public uint Flags
        { 
            get => flag0 | ((uint)flag1 << 8) | ((uint)flag2 << 16) | ((uint)flag3 << 24);
            set
            {
                flag0 = (byte)(value & 0x000000FF);
                flag1 = (byte)((value >> 8) & 0x0000FF00);
                flag2 = (byte)((value >> 16) & 0x00FF0000);
                flag3 = (byte)((value >> 24) & 0xFF000000);
            }
        }

        public UInt32 unknown1;
        public float KeyframeLength { get; set; }
        public IList<Keyframe<Quaternion>> Keyframes { get; set; } = new List<Keyframe<Quaternion>>();
        
        public byte[] remaining_data = null;

        public QuatLinearRotationController(string name = null) => HashName = new HashName(name ?? "");

        public QuatLinearRotationController(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;

            this.HashName = new HashName(instream.ReadUInt64());
            this.flag0 = instream.ReadByte();
            this.flag1 = instream.ReadByte();
            this.flag2 = instream.ReadByte();
            this.flag3 = instream.ReadByte();
            this.unknown1 = instream.ReadUInt32();
            this.KeyframeLength = instream.ReadSingle();
            var keyframe_count = instream.ReadUInt32();

            for(int x = 0; x < keyframe_count; x++)
            {
                this.Keyframes.Add(new Keyframe<Quaternion>(instream.ReadSingle(), instream.ReadQuaternion()));
            }

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.HashName.Hash);
            outstream.Write(this.flag0);
            outstream.Write(this.flag1);
            outstream.Write(this.flag2);
            outstream.Write(this.flag3);
            outstream.Write(this.unknown1);
            outstream.Write(this.KeyframeLength);
            outstream.Write(this.Keyframes.Count);

            foreach (var item in this.Keyframes)
            {
                outstream.Write(item.Timestamp);
                outstream.Write(item.Value.X);
                outstream.Write(item.Value.Y);
                outstream.Write(item.Value.Z);
                outstream.Write(item.Value.W);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {

            string keyframes_string = (this.Keyframes.Count == 0 ? "none" : "");
            keyframes_string += string.Join(", ", Keyframes.Select(i => i.ToString()));

            return base.ToString() + 
                " size: " + this.size +
                " HashName: " + this.HashName.String +
                " flag0: " + this.flag0 +
                " flag1: " + this.flag1 +
                " flag2: " + this.flag2 +
                " flag3: " + this.flag3 +
                " unknown1: " + this.unknown1 +
                " keyframe_length: " + this.KeyframeLength +
                " count: " + this.Keyframes.Count + " items: [ " + keyframes_string + " ] " + 
                (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

    }
}
