using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    /// <summary>
    /// Represents an entry in dsl::BoneMapping, storing indexed mappings to SkinBones.SkinPositions
    /// </summary>
    /// <remarks>
    /// Inside PD2, there is the dsl::BoneMapping class. This is used for some unknown purpose,
    /// however what is known is that it builds a list of matrices. These are referred to by
    /// indexes into a runtime table built by SkinBones (Bones::matrices).
    ///
    /// This runtime table is built by multiplying together the world transform and global skin
    /// transform onto each SkinBones matrix. This is done in C#, loaded into the SkinPositions
    /// list in SkinBones.
    /// </remarks>
    class BoneMappingItem
    {
        public readonly List<UInt32> bones = new List<UInt32>();

        public override string ToString()
        {
            string verts_string = (bones.Count == 0 ? "none" : "");

            foreach (UInt32 vert in bones)
            {
                verts_string += vert + ", ";
            }

            return "count: " + bones.Count + " verts: [" + verts_string + "]";
        }
    }

    /// <summary>
    /// Represents dsl::Bones, an abstract base class inside PD2.
    /// </summary>
    ///
    /// <remarks>
    /// See the dumped vtables:
    /// https://raw.githubusercontent.com/blt4linux/blt4l/master/doc/payday2_vtables
    /// Note it has pure virtual methods.
    ///
    /// As an abstract class, it can never be found by itself, only embedded within
    /// SkinBones (it's only known - and likely only - subclass).
    /// </remarks>
    class Bones
    {
        private static uint bones_tag = 0xEB43C77; // Bones
        public UInt32 id;
        public UInt32 size;

        public UInt32 count;
        public List<BoneMappingItem> bone_mappings = new List<BoneMappingItem>();

        public byte[] remaining_data = null;

        internal Bones()
        {
        }

        public Bones(BinaryReader instream, SectionHeader section) : this(instream)
        {
            this.id = section.id;
            this.size = section.size;

            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data =
                    instream.ReadBytes((int) ((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public Bones(BinaryReader instream)
        {
            this.count = instream.ReadUInt32();

            for (int x = 0; x < this.count; x++)
            {
                BoneMappingItem bone_mapping_item = new BoneMappingItem();
                uint bone_count = instream.ReadUInt32();
                for (int y = 0; y < bone_count; y++)
                    bone_mapping_item.bones.Add(instream.ReadUInt32());
                bone_mappings.Add(bone_mapping_item);
            }

            this.remaining_data = null;
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(bones_tag);
            outstream.Write(this.id);
            long newsizestart = outstream.BaseStream.Position;
            outstream.Write(this.size);

            this.StreamWriteData(outstream);

            //update section size
            long newsizeend = outstream.BaseStream.Position;
            outstream.BaseStream.Position = newsizestart;
            outstream.Write((uint) (newsizeend - (newsizestart + 4)));

            outstream.BaseStream.Position = newsizeend;
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.count);
            System.Diagnostics.Debug.Assert(this.count == this.bone_mappings.Count, "[Bones] this.count != this.bones.Count");
            foreach (BoneMappingItem bone in this.bone_mappings)
            {
                outstream.Write(bone.bones.Count);
                foreach (UInt32 vert in bone.bones)
                    outstream.Write(vert);
            }

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            string bones_string = (bone_mappings.Count == 0 ? "none" : "");

            foreach (BoneMappingItem bone in bone_mappings)
            {
                bones_string += bone + ", ";
            }

            return "[Bones] ID: " + this.id + " size: " + this.size + " count: " + this.count + " bones:[ " +
                   bones_string + " ]" + (this.remaining_data != null
                       ? " REMAINING DATA! " + this.remaining_data.Length + " bytes"
                       : "");
        }
    }
}
