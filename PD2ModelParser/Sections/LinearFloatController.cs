using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.linearFloatController_tag)]
    class LinearFloatController : AbstractSection, ISection, IHashNamed
    {
        public class Keyframe
        {
            public Keyframe(float timestamp, float value)
            {
                Timestamp = timestamp;
                Value = value;
            }

            public float Timestamp { get; set; }
            public float Value { get; set; }

            public void StreamWriteData(BinaryWriter output)
            {
                output.Write(Timestamp);
                output.Write(Value);
            }
        }

        public HashName HashName { get; set; }
        public uint Flags { get; set; }
        public uint Unknown2 { get; set; }
        public float KeyframeLength { get; set; }
        public List<Keyframe> Keyframes { get; set; } = new List<Keyframe>();

        public LinearFloatController()
        {

        }

        public LinearFloatController(System.IO.BinaryReader instream, SectionHeader section)
        {
            SectionId = section.id;
            HashName = new HashName(instream.ReadUInt64());
            Flags = instream.ReadUInt32();
            Unknown2 = instream.ReadUInt32();
            KeyframeLength = instream.ReadSingle();
            var count = instream.ReadUInt32();
            for (var i = 0; i < count; i++) {
                Keyframes.Add(new Keyframe(instream.ReadSingle(), instream.ReadSingle()));
            }
        }

        public override void StreamWriteData(BinaryWriter output)
        {
            output.Write(HashName.Hash);
            output.Write(Flags);
            output.Write(Unknown2);
            output.Write(KeyframeLength);
            output.Write(Keyframes.Count);
            for(var i = 0; i < Keyframes.Count; i++)
            {
                Keyframes[i].StreamWriteData(output);
            }
        }
    }
}
