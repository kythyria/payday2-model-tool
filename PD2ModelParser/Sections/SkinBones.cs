using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    // SkinBones should extend Bones (as it does in the game), but that'd
    // be a bit of a pain to do.
    class SkinBones : AbstractSection, ISection, IPostLoadable
    {
        private static uint skinbones_tag = 0x65CC1825; // SkinBones

        public UInt32 id;

        public Bones bones;
        public UInt32 probably_root_bone;
        public int count => objects.Count;
        public List<UInt32> objects = new List<UInt32>();
        public List<Matrix3D> rotations = new List<Matrix3D>();
        public Matrix3D global_skin_transform;

        public byte[] remaining_data = null;

        public override uint SectionId
        {
            get => id;
            set => id = value;
        }

        public override uint TypeCode => Tags.skinbones_tag;

        // Post-loaded
        public List<Matrix3D> SkinPositions { get; private set; }

        public SkinBones(uint secId)
        {
            bones = new Bones();
            id = secId;
        }

        public SkinBones(BinaryReader instream, SectionHeader section) : this(section.id)
        {
            this.bones = new Bones(instream);
            this.probably_root_bone = instream.ReadUInt32();
            uint count = instream.ReadUInt32();
            for (int x = 0; x < count; x++)
                this.objects.Add(instream.ReadUInt32());
            for (int x = 0; x < count; x++)
            {
                this.rotations.Add(MathUtil.ReadMatrix(instream));
            }

            this.global_skin_transform = MathUtil.ReadMatrix(instream);

            this.remaining_data = null;

            long end_pos = section.offset + 12 + section.size;
            if (end_pos > instream.BaseStream.Position)
            {
                // If exists, this contains hashed name for this geometry (see hashlist.txt)
                remaining_data = instream.ReadBytes((int) (end_pos - instream.BaseStream.Position));
            }
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            this.bones.StreamWriteData(outstream);
            outstream.Write(this.probably_root_bone);
            outstream.Write(this.count);

            SectionUtils.CheckLength(count, objects);
            SectionUtils.CheckLength(count, rotations);

            foreach (UInt32 item in this.objects)
                outstream.Write(item);
            foreach (Matrix3D matrix in this.rotations)
            {
                MathUtil.WriteMatrix(outstream, matrix);
            }

            MathUtil.WriteMatrix(outstream, global_skin_transform);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            string objects_string = (this.objects.Count == 0 ? "none" : "");

            foreach (UInt32 obj in this.objects)
            {
                objects_string += obj + ", ";
            }

            string rotations_string = (this.rotations.Count == 0 ? "none" : "");

            foreach (Matrix3D rotation in this.rotations)
            {
                rotations_string += rotation + ", ";
            }

            return "[SkinBones] ID: " + this.id +
                   " bones: [ " + this.bones + " ]" +
                   " object3D_section_id: " + this.probably_root_bone +
                   " count: " + this.count + " objects" +
                   " count: " + this.objects.Count + " objects:[ " + objects_string + " ]" +
                   " rotations count: " + this.rotations.Count + " rotations:[ " + rotations_string + " ]" +
                   " global_skin_transform: " + this.global_skin_transform +
                   (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

        public void PostLoad(uint id, Dictionary<uint, object> parsed_sections)
        {
            SkinPositions = new List<Matrix3D>(count);

            for (int i = 0; i < objects.Count; i++)
            {
                Object3D obj = (Object3D) parsed_sections[objects[i]];

                Matrix3D inter = rotations[i].MultDiesel(obj.world_transform);
                Matrix3D skin_node = inter.MultDiesel(global_skin_transform);

                SkinPositions.Add(skin_node);
            }
        }
    }
}
