using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using Nexus;

namespace PD2ModelParser.Sections
{
    //[System.ComponentModel.TypeConverter(typeof(Inspector.AbstractSectionConverter))]
    [SectionId(Tags.object3D_tag)]
    public class Object3D : AbstractSection, ISection, IPostLoadable, IHashContainer
    {
        public UInt32 size;

        [Category("Object3D")]
        [DisplayName("Name")]
        public HashName HashName { get; set; } //Hashed object root point name (see hashlist.txt)
        public List<uint> animation_ids = new List<uint>(); // This is NOT a list of Object3Ds (or Models). Maybe animation related?
        private Matrix3D _rotation = new Matrix3D(); // 4x4 transform matrix - for translation/scale too

        [Category("Object3D")]
        [TypeConverter(typeof(Inspector.NexusMatrixConverter))]
        public Matrix3D Transform
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateTransforms();
            }
        }

        [Browsable(false)]
        public uint parentID => Parent?.SectionId ?? 0;

        public byte[] remaining_data = null;

        // Non-written fields
        private bool has_post_loaded;

        [Category("Object3D")]
        [TypeConverter(typeof(Inspector.NexusMatrixConverter))]
        public Matrix3D WorldTransform { get; private set; }

        [Category("Object3D")]
        [DisplayName("Parent")]
        [TypeConverter(typeof(Inspector.Object3DReferenceConverter))]
        public Object3D Parent { get; set; }

        public List<Object3D> children = new List<Object3D>();

        public void SetParent(Object3D newParent)
        {
            var oldParent = Parent;
            if(oldParent != null)
            {
                oldParent.children.Remove(this);
            }
            if (newParent != null)
            {
                newParent.children.Add(this);
            }
            Parent = newParent;
        }

        [Browsable(false)]
        public string Name => HashName.String;

        public Object3D(string object_name, Object3D parent)
        {
            this.SectionId = 0;
            this.size = 0;

            this.HashName = HashName.FromNumberOrString(object_name);
            this.animation_ids = new List<uint>();
            this.Transform = Matrix3D.Identity;

            this.Parent = parent;

            UpdateTransforms();
        }

        public Object3D(BinaryReader instream, SectionHeader section) : this(instream)
        {
            this.SectionId = section.id;
            this.size = section.size;

            if (section.End > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int) (section.End - instream.BaseStream.Position));
        }

        public Object3D(BinaryReader instream)
        {
            // In Object3D::load
            this.HashName = new HashName(instream.ReadUInt64());

            // in dsl::ParamBlock::load
            uint child_count = instream.ReadUInt32();

            for (int x = 0; x < child_count; x++)
            {
                uint item = instream.ReadUInt32(); // This is a reference thing, probably not important
                instream.ReadUInt64(); // Skip eight bytes, as per PD2
                this.animation_ids.Add(item);
            }

            // In Object3D::load
            Matrix3D transform = new Matrix3D();
            transform.M11 = instream.ReadSingle();
            transform.M12 = instream.ReadSingle();
            transform.M13 = instream.ReadSingle();
            transform.M14 = instream.ReadSingle();
            transform.M21 = instream.ReadSingle();
            transform.M22 = instream.ReadSingle();
            transform.M23 = instream.ReadSingle();
            transform.M24 = instream.ReadSingle();
            transform.M31 = instream.ReadSingle();
            transform.M32 = instream.ReadSingle();
            transform.M33 = instream.ReadSingle();
            transform.M34 = instream.ReadSingle();
            transform.M41 = instream.ReadSingle();
            transform.M42 = instream.ReadSingle();
            transform.M43 = instream.ReadSingle();
            transform.M44 = instream.ReadSingle();

            transform.M41 = instream.ReadSingle();
            transform.M42 = instream.ReadSingle();
            transform.M43 = instream.ReadSingle();

            Transform = transform;

            PostLoadRef<Object3D>(instream.ReadUInt32(), i => this.Parent = i);

            this.remaining_data = null;
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.HashName.Hash);
            outstream.Write(animation_ids.Count);
            foreach (uint item in this.animation_ids)
            {
                outstream.Write(item);
                outstream.Write((ulong) 0); // Bit to skip - the PD2 binary does the exact same thing
            }

            outstream.Write(this.Transform.M11);
            outstream.Write(this.Transform.M12);
            outstream.Write(this.Transform.M13);
            outstream.Write(this.Transform.M14);
            outstream.Write(this.Transform.M21);
            outstream.Write(this.Transform.M22);
            outstream.Write(this.Transform.M23);
            outstream.Write(this.Transform.M24);
            outstream.Write(this.Transform.M31);
            outstream.Write(this.Transform.M32);
            outstream.Write(this.Transform.M33);
            outstream.Write(this.Transform.M34);
            outstream.Write(this.Transform.M41);
            outstream.Write(this.Transform.M42);
            outstream.Write(this.Transform.M43);
            outstream.Write(this.Transform.M44);
            outstream.Write(this.Transform.M41); // Write the position out again, as for some reason
            outstream.Write(this.Transform.M42); // it's not stored in the main matrix
            outstream.Write(this.Transform.M43);
            outstream.Write(this.parentID);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            Vector3D scale = new Vector3D();
            Quaternion rot = new Quaternion();
            Vector3D translation = new Vector3D();
            this.Transform.Decompose(out scale, out rot, out translation);
            return base.ToString() +
                   " size: " + this.size +
                   " HashName: " + this.HashName.String +
                   " animations: " + this.animation_ids.Count +
                   " mat.scale: " + scale +
                   " mat.rotation: [x: " + rot.X + " y: " + rot.Y + " z: " + rot.Z + " w: " + rot.W + "]" +
                   " Parent ID: " + this.parentID +
                   (remaining_data != null ? " REMAINING DATA! " + remaining_data.Length + " bytes" : "");
        }

        public void CollectHashes(CustomHashlist hashlist)
        {
            hashlist.Hint(HashName);
        }

        public override void PostLoad(uint id, Dictionary<uint, ISection> parsed_sections)
        {
            base.PostLoad(id, parsed_sections);
            if (Parent != null)
            {
                if (!Parent.has_post_loaded)
                    Parent.PostLoad(Parent.SectionId, parsed_sections);

                if (!Parent.children.Contains(this))
                {
                    Parent.children.Add(this);
                }
            }

            UpdateTransforms();

            has_post_loaded = true;
        }

        public void UpdateTransforms()
        {
            if (Parent == null)
            {
                WorldTransform = Transform;
                return;
            }

            WorldTransform = Transform.MultDiesel(Parent.WorldTransform);
        }
    }
}
