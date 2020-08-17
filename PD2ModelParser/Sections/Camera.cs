using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.camera_tag, ShowInInspectorRoot = false)]
    class Camera : Object3D, ISection
    {
        public float Unknown1 { get; set; }
        public float Unknown2 { get; set; }
        public float Unknown3 { get; set; }
        public float Unknown4 { get; set; }
        public float Unknown5 { get; set; }
        public float Unknown6 { get; set; }

        public Camera(string name, Object3D parent) : base(name, parent) { }

        public Camera(BinaryReader instream, SectionHeader section) : base(instream)
        {
            this.SectionId = section.id;
            this.size = section.size;

            Unknown1 = instream.ReadSingle();
            Unknown2 = instream.ReadSingle();
            Unknown3 = instream.ReadSingle();
            Unknown4 = instream.ReadSingle();
            Unknown5 = instream.ReadSingle();
            Unknown6 = instream.ReadSingle();
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            base.StreamWriteData(outstream);

            outstream.Write(Unknown1);
            outstream.Write(Unknown2);
            outstream.Write(Unknown3);
            outstream.Write(Unknown4);
            outstream.Write(Unknown5);
            outstream.Write(Unknown6);
        }
    }
}
