using Nexus;
using System;
using System.Collections.Generic;
using System.IO;

namespace PD2ModelParser.Sections
{
    public class LinearVector3Controller_KeyFrame
    {
        public float timestamp;
        public Vector3D vector;

        public LinearVector3Controller_KeyFrame(float timestamp, Vector3D vector)
        {
            this.timestamp = timestamp;
            this.vector = vector;
        }

        public LinearVector3Controller_KeyFrame(BinaryReader instream)
        {
            this.timestamp = instream.ReadSingle();
            this.vector = new Vector3D(instream.ReadSingle(), instream.ReadSingle(), instream.ReadSingle()); //Might be wrong order
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.timestamp);
            outstream.Write(this.vector.X);
            outstream.Write(this.vector.Y);
            outstream.Write(this.vector.Z);
        }

        public override string ToString()
        {
            return "Timestamp=" + this.timestamp + " Vector=[X=" + this.vector.X + ", Y=" + this.vector.Y + ", Z=" + this.vector.Z + "]";
        }

    }

    [ModelFileSection(Tags.linearVector3Controller_tag)]
    class LinearVector3Controller : AbstractSection, ISection, IHashNamed
    {
        public UInt32 size;

        public HashName HashName { get; set; }
        public byte Flag0 { get; set; }
        public byte Flag1 { get; set; }
        public byte Flag2 { get; set; }
        public byte Flag3 { get; set; }
        public uint Unknown1 { get; set; }
        public float KeyframeLength { get; set; }
        public List<LinearVector3Controller_KeyFrame> Keyframes { get; set; } = new List<LinearVector3Controller_KeyFrame>();

        public byte[] remaining_data = null;

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
                this.Keyframes.Add(new LinearVector3Controller_KeyFrame(instream));
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

            foreach (LinearVector3Controller_KeyFrame item in this.Keyframes)
            {
                item.StreamWriteData(outstream);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {

            string keyframes_string = (this.Keyframes.Count == 0 ? "none" : "");

            bool first = true;
            foreach (LinearVector3Controller_KeyFrame item in this.Keyframes)
            {
                keyframes_string += (first ? "" : ", ") + item;
                first = false;
            }

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
