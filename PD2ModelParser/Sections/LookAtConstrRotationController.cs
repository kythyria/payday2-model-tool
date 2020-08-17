using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.lookAtConstrRotationController)]
    class LookAtConstrRotationController : AbstractSection, ISection, IPostLoadable, IHashNamed
    {
        public HashName HashName { get; set; }
        public uint Unknown1 { get; set; }
        
        [System.ComponentModel.TypeConverter(typeof(Inspector.Object3DReferenceConverter))]
        public ISection Unknown2 { get; set; }
        
        [System.ComponentModel.TypeConverter(typeof(Inspector.Object3DReferenceConverter))]
        public ISection Unknown3 { get; set; }
        
        [System.ComponentModel.TypeConverter(typeof(Inspector.Object3DReferenceConverter))]
        public ISection Unknown4 { get; set; }

        public LookAtConstrRotationController() { }

        public LookAtConstrRotationController(BinaryReader br, SectionHeader sh)
        {
            this.SectionId = sh.id;

            HashName = new HashName(br.ReadUInt64());
            Unknown1 = br.ReadUInt32();
            PostLoadRef<ISection>(br.ReadUInt32(), s => Unknown2 = s);
            PostLoadRef<ISection>(br.ReadUInt32(), s => Unknown3 = s);
            PostLoadRef<ISection>(br.ReadUInt32(), s => Unknown4 = s);
        }

        public override void StreamWriteData(BinaryWriter output)
        {
            output.Write(HashName.Hash);
            output.Write(Unknown1);
            output.Write(Unknown2?.SectionId ?? 0);
            output.Write(Unknown3?.SectionId ?? 0);
            output.Write(Unknown4?.SectionId ?? 0);
        }
    }
}
