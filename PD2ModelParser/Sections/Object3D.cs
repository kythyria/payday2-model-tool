using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class Object3D
    {
        private static uint object3D_tag = 0x0FFCD100; // Object3D
        public UInt32 id;
        public UInt32 size;

        public UInt64 hashname; //Hashed object root point name (see hashlist.txt)
        public UInt32 count;
        public List<Vector3D> items = new List<Vector3D>();
        public Matrix3D rotation = new Matrix3D(); //4x4 Rotation Matrix
        public Vector3D position = new Vector3D(); //Actual position of the object
        public UInt32 parentID; //ID of parent object

        public byte[] remaining_data = null;

        public Object3D(string object_name, uint parent)
        {
            this.id = 0;
            this.size = 0;

            this.hashname = Hash64.HashString(object_name);
            this.count = 0;
            this.items = new List<Vector3D>();
            this.rotation = new Matrix3D(1.0f, 0.0f, 0.0f, 0.0f,
                                        0.0f, 1.0f, 0.0f, 0.0f,
                                        0.0f, 0.0f, 1.0f, 0.0f, 
                                        0.0f, 0.0f, 0.0f, 0.0f);
            this.position = new Vector3D(0.0f, 0.0f, 0.0f);
            this.parentID = parent;
        }

        public Object3D(BinaryReader instream, SectionHeader section) : this(instream)
        {
            this.id = section.id;
            this.size = section.size;
            StaticStorage.objects_list.Add(StaticStorage.hashindex.GetString(this.hashname));

            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public Object3D(BinaryReader instream)
        {
            this.hashname = instream.ReadUInt64();
            this.count = instream.ReadUInt32();

            for (int x = 0; x < this.count; x++)
            {
                Vector3D item = new Vector3D();
                item.X = instream.ReadSingle();
                item.Y = instream.ReadSingle();
                item.Z = instream.ReadSingle();
                this.items.Add(item);
            }

            this.rotation.M11 = instream.ReadSingle();
            this.rotation.M12 = instream.ReadSingle();
            this.rotation.M13 = instream.ReadSingle();
            this.rotation.M14 = instream.ReadSingle();
            this.rotation.M21 = instream.ReadSingle();
            this.rotation.M22 = instream.ReadSingle();
            this.rotation.M23 = instream.ReadSingle();
            this.rotation.M24 = instream.ReadSingle();
            this.rotation.M31 = instream.ReadSingle();
            this.rotation.M32 = instream.ReadSingle();
            this.rotation.M33 = instream.ReadSingle();
            this.rotation.M34 = instream.ReadSingle();
            this.rotation.M41 = instream.ReadSingle();
            this.rotation.M42 = instream.ReadSingle();
            this.rotation.M43 = instream.ReadSingle();
            this.rotation.M44 = instream.ReadSingle();

            this.position.X = instream.ReadSingle();
            this.position.Y = instream.ReadSingle();
            this.position.Z = instream.ReadSingle();

            this.parentID = instream.ReadUInt32();

            this.remaining_data = null;
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(object3D_tag);
            outstream.Write(this.id);
            long newsizestart = outstream.BaseStream.Position;
            outstream.Write(this.size);

            this.StreamWriteData(outstream);

            //update section size
            long newsizeend = outstream.BaseStream.Position;
            outstream.BaseStream.Position = newsizestart;
            outstream.Write((uint)(newsizeend - (newsizestart + 4)));

            outstream.BaseStream.Position = newsizeend;
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.hashname);
            outstream.Write(this.count);
            foreach (Vector3D item in this.items)
            {
                outstream.Write(item.X);
                outstream.Write(item.Y);
                outstream.Write(item.Z);
            }
            outstream.Write(this.rotation.M11);
            outstream.Write(this.rotation.M12);
            outstream.Write(this.rotation.M13);
            outstream.Write(this.rotation.M14);
            outstream.Write(this.rotation.M21);
            outstream.Write(this.rotation.M22);
            outstream.Write(this.rotation.M23);
            outstream.Write(this.rotation.M24);
            outstream.Write(this.rotation.M31);
            outstream.Write(this.rotation.M32);
            outstream.Write(this.rotation.M33);
            outstream.Write(this.rotation.M34);
            outstream.Write(this.rotation.M41);
            outstream.Write(this.rotation.M42);
            outstream.Write(this.rotation.M43);
            outstream.Write(this.rotation.M44);
            outstream.Write(this.position.X);
            outstream.Write(this.position.Y);
            outstream.Write(this.position.Z);
            outstream.Write(this.parentID);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            Vector3D scale = new Vector3D();
            Quaternion rot = new Quaternion();
            Vector3D translation = new Vector3D();
            this.rotation.Decompose(out scale, out rot, out translation);
            return "[Object3D] ID: " + this.id + " size: " + this.size + " hashname: " + StaticStorage.hashindex.GetString(this.hashname) + " count: " + this.count + " items: " + this.items.Count + " mat.scale: " + scale + " mat.rotation: [x: " + rot.X + " y: " + rot.Y + " z: " + rot.Z + " w: " + rot.W + "] mat.position: " + position + " position: [" + this.position.X + " " + this.position.Y + " " + this.position.Z + "] Parent ID: " + this.parentID + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

    }
}
