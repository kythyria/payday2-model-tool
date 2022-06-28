using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Numerics;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.object3D_tag, RootInspectorNode=typeof(Inspector.ObjectsRootNode))]
    public class Object3D : AbstractSection, ISection, IPostLoadable, IHashContainer, IHashNamed
    {
        public UInt32 size;

        [Category("Object3D")]
        [DisplayName("Name")]
        public HashName HashName { get; set; } //Hashed object root point name (see hashlist.txt)
        private Matrix4x4 _rotation = new Matrix4x4(); // 4x4 transform matrix - for translation/scale too

        /// <summary>
        /// Animation controllers affecting this object.
        /// </summary>
        /// <remarks>
        /// The meaning of this property varies with the actual type of self.
        /// 
        /// <list type="bullet">
        /// <item>
        /// <term>self is Light</term>
        /// <description>[float brightness, vector3 colour, null, vector3 position]</description>
        /// </item>
        /// 
        /// <item>
        /// <term>self is Light</term>
        /// <description>[float brightness, null, null, null]</description>
        /// </item>
        /// 
        /// <item><term>self is Light</term>
        /// <description>[vector3 colour, null, null]</description>
        /// </item>
        /// 
        /// <item>
        /// <term>self is exactly Object3D</term>
        /// <description>[quaternion rotation, null]</description>
        /// </item>
        /// <item>
        /// 
        /// <term>self is Model</term>
        /// <description>[quaternion rotation, null]</description>
        /// </item>
        /// </list>
        /// </remarks>
        [Category("Object3D")]
        public List<IAnimationController> Animations { get; private set; } = new List<IAnimationController>();

        [Category("Object3D")]
        public Matrix4x4 Transform
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
        public Matrix4x4 WorldTransform { get; private set; }

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
            if (newParent == this)
            {
                throw new Exception($"Object {Name}({SectionId}) attempted to have itself as parent");
            }
            else if (newParent != null)
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
            this.Transform = Matrix4x4.Identity;

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
            var animation_ids = new List<uint>();

            for (int x = 0; x < child_count; x++)
            {
                uint item = instream.ReadUInt32(); // This is a reference thing, probably not important
                instream.ReadUInt64(); // Skip eight bytes, as per PD2
                animation_ids.Add(item);
            }
            postloadCallbacks.Add((self, sections) => Animations.AddRange(animation_ids.Select(i => sections.ContainsKey(i) ? (IAnimationController)sections[i] : null)));

            // In Object3D::load
            Matrix4x4 transform = instream.ReadMatrix();

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
            outstream.Write(this.Animations.Count);
            foreach (var item in this.Animations)
            {
                outstream.Write((item?.SectionId).GetValueOrDefault());
                outstream.Write((ulong) 0); // Bit to skip - the PD2 binary does the exact same thing
            }

            outstream.Write(this.Transform);
            outstream.Write(this.Transform.M41); // Write the position out again, as for some reason
            outstream.Write(this.Transform.M42); // it's not stored in the main matrix
            outstream.Write(this.Transform.M43);
            outstream.Write(this.parentID);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            var scale = new Vector3();
            var rot = new System.Numerics.Quaternion();
            var translation = new Vector3();
            Matrix4x4.Decompose(this.Transform, out scale, out rot, out translation);
            return base.ToString() +
                   " size: " + this.size +
                   " HashName: " + this.HashName.String +
                   " animations: " + this.Animations.Count +
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
                if (Parent.SectionId == SectionId)
                {
                    Log.Default.Warn("Object {0}({1}) has itself as parent", Name, SectionId);
                    this.SetParent(null);
                }
                else
                {
                    if (!Parent.has_post_loaded)
                        Parent.PostLoad(Parent.SectionId, parsed_sections);

                    if (!Parent.children.Contains(this))
                    {
                        Parent.children.Add(this);
                    }
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
