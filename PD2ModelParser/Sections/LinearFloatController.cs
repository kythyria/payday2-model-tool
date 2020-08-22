using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.linearFloatController_tag)]
    class LinearFloatController : AbstractSection, ISection, IHashNamed, IAnimationController<float>
    {
        public HashName HashName { get; set; }
        public uint Flags { get; set; }
        public byte Flag0 { get => (byte)((Flags & 0x000000FF) >> 0); set => Flags = Flags & (uint)((value << 0) | 0xFFFFFF00); }
        public byte Flag1 { get => (byte)((Flags & 0x0000FF00) >> 8);  set => Flags = Flags & (uint)((value << 8) | 0xFFFF00FF); }
        public byte Flag2 { get => (byte)((Flags & 0x00FF0000) >> 16); set => Flags = Flags & (uint)((value << 16) | 0xFF00FFFF); }
        public byte Flag3 { get => (byte)((Flags & 0xFF000000) >> 24); set => Flags = Flags & (uint)((value << 24) | 0x00FFFFFF); }
        public uint Unknown2 { get; set; }
        public float KeyframeLength { get; set; }
        public IList<Keyframe<float>> Keyframes { get; set; } = new List<Keyframe<float>>();

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
                Keyframes.Add(new Keyframe<float>(instream.ReadSingle(), instream.ReadSingle()));
            }
        }

        public override void StreamWriteData(BinaryWriter output)
        {
            output.Write(HashName.Hash);
            output.Write(Flags);
            output.Write(Unknown2);
            output.Write(KeyframeLength);
            output.Write(Keyframes.Count);
            foreach(var i in Keyframes)
            {
                output.Write(i.Timestamp);
                output.Write(i.Value);
            }
        }
    }
}
