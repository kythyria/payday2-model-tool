using Nexus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.skinbones_tag, ShowInInspectorRoot=false)]
    class SkinBones : Bones, ISection, IPostLoadable
    {
        private List<uint> objects { get; set; } = new List<uint>(); // of Object3D by SectionID

        [TypeConverter(typeof(Inspector.Object3DReferenceConverter))]
        public Object3D ProbablyRootBone { get; set; }
        public int count => Objects.Count;
        public List<Object3D> Objects { get; private set; } = new List<Object3D>();
        public List<Matrix3D> rotations { get; private set; } = new List<Matrix3D>();
        [TypeConverter(typeof(Inspector.NexusMatrixConverter))]
        public Matrix3D global_skin_transform { get; set; }

        // Post-loaded
        public List<Matrix3D> SkinPositions { get; private set; }

        public SkinBones() : base() { }

        public SkinBones(BinaryReader instream, SectionHeader section) : base(instream)
        {
            this.SectionId = section.id;
            this.size = section.size;

            PostLoadRef<Object3D>(instream.ReadUInt32(), i => ProbablyRootBone = i);
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
            base.StreamWriteData(outstream);
            outstream.Write(this.ProbablyRootBone.SectionId);
            outstream.Write(this.count);

            SectionUtils.CheckLength(count, Objects);
            SectionUtils.CheckLength(count, rotations);

            foreach (var item in this.Objects)
                outstream.Write(item.SectionId);
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
            string objects_string = (this.Objects.Count == 0 ? "none" : "");

            objects_string += string.Join(", ", this.Objects.Select(i => i.SectionId));

            string rotations_string = (this.rotations.Count == 0 ? "none" : "");

            foreach (Matrix3D rotation in this.rotations)
            {
                rotations_string += rotation + ", ";
            }

            return base.ToString() +
                   " object3D_section_id: " + this.ProbablyRootBone.SectionId +
                   " count: " + this.Objects.Count + " objects:[ " + objects_string + " ]" +
                   " rotations count: " + this.rotations.Count + " rotations:[ " + rotations_string + " ]" +
                   " global_skin_transform: " + this.global_skin_transform +
                   (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

        public override void PostLoad(uint id, Dictionary<uint, ISection> parsed_sections)
        {
            base.PostLoad(id, parsed_sections);
            SkinPositions = new List<Matrix3D>(count);

            for (int i = 0; i < objects.Count; i++)
            {
                Object3D obj = (Object3D) parsed_sections[objects[i]];
                Objects.Add(obj);

                System.Numerics.Matrix4x4 inter = rotations[i].ToMatrix4x4().MultDiesel(obj.WorldTransform.ToMatrix4x4());
                Matrix3D skin_node = inter.MultDiesel(global_skin_transform.ToMatrix4x4()).ToNexusMatrix();

                SkinPositions.Add(skin_node);
            }
            objects = null;
        }
    }
}
