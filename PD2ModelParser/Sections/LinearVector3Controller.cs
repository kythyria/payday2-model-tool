using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.linearVector3Controller_tag)]
    class LinearVector3Controller : AbstractSection, ISection, IHashNamed, IAnimationController<Vector3D>
    {
        public UInt32 size;

        public HashName HashName { get; set; }
        public byte Flag0 { get; set; }
        public byte Flag1 { get; set; }
        public byte Flag2 { get; set; }
        public byte Flag3 { get; set; }

        public uint Flags
        {
            get => Flag0 | ((uint)Flag1 << 8) | ((uint)Flag2 << 16) | ((uint)Flag3 << 24);
            set
            {
                Flag0 = (byte)(value & 0x000000FF);
                Flag1 = (byte)((value >> 8) & 0x0000FF00);
                Flag2 = (byte)((value >> 16) & 0x00FF0000);
                Flag3 = (byte)((value >> 24) & 0xFF000000);
            }
        }

        public uint Unknown1 { get; set; }
        public float KeyframeLength { get; set; }
        public IList<Keyframe<Vector3D>> Keyframes { get; set; } = new List<Keyframe<Vector3D>>();

        public byte[] remaining_data = null;

        public LinearVector3Controller(string name = null) => HashName = new HashName(name ?? "");

        public LinearVector3Controller(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;

            this.HashName = new HashName(instream.ReadUInt64());
            this.Flag0 = instream.ReadByte();
            this.Flag1 = instream.ReadByte();
            this.Flag2 = instream.ReadByte();
            this.Flag3 = instream.ReadByte();
            this.Unknown1 = instream.ReadUInt32();
            this.KeyframeLength = instream.ReadSingle();
            var keyframe_count = instream.ReadUInt32();

            for (int x = 0; x < keyframe_count; x++)
            {
                this.Keyframes.Add(new Keyframe<Vector3D>(instream.ReadSingle(), instream.ReadNexusVector3D()));
            }

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.HashName.Hash);
            outstream.Write(this.Flag0);
            outstream.Write(this.Flag1);
            outstream.Write(this.Flag2);
            outstream.Write(this.Flag3);
            outstream.Write(this.Unknown1);
            outstream.Write(this.KeyframeLength);
            outstream.Write(this.Keyframes.Count);

            foreach (var kf in this.Keyframes)
            {
                outstream.Write(kf.Timestamp);
                outstream.Write(kf.Value);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {

            string keyframes_string = (this.Keyframes.Count == 0 ? "none" : "");
            keyframes_string += string.Join(", ", this.Keyframes.Select(i => i.ToString()));

            return base.ToString() +
                " size: " + this.size +
                " HashName: " + this.HashName.String +
                " flag0: " + this.Flag0 +
                " flag1: " + this.Flag1 +
                " flag2: " + this.Flag2 +
                " flag3: " + this.Flag3 +
                " unknown1: " + this.Unknown1 +
                " keyframe_length: " + this.KeyframeLength +
                " count: " + this.Keyframes.Count + " items: [ " + keyframes_string + " ] " +
                (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

    }
}
