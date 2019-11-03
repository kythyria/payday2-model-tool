using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    public class GeometryWeightGroups
    {
        public UInt16 Bones1;
        public UInt16 Bones2;
        public UInt16 Bones3;
        public UInt16 Bones4;


        public GeometryWeightGroups(BinaryReader instream)
        {
            this.Bones1 = instream.ReadUInt16();
            this.Bones2 = instream.ReadUInt16();
            this.Bones3 = instream.ReadUInt16();
            this.Bones4 = instream.ReadUInt16();
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(this.Bones1);
            outstream.Write(this.Bones2);
            outstream.Write(this.Bones3);
            outstream.Write(this.Bones4);
        }

        public override string ToString()
        {
            return "{ Bones1=" + this.Bones1 + ", Bones2=" + this.Bones2 + ", Bones3=" + this.Bones3 + ", Bones4=" +
                   this.Bones4 + " }";
        }
    }

    public class GeometryColor
    {
        public Byte red = 0;
        public Byte green = 0;
        public Byte blue = 0;
        public Byte alpha = 0;

        public GeometryColor(BinaryReader instream)
        {
            this.blue = instream.ReadByte();
            this.green = instream.ReadByte();
            this.red = instream.ReadByte();
            this.alpha = instream.ReadByte();
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(this.blue);
            outstream.Write(this.green);
            outstream.Write(this.red);
            outstream.Write(this.alpha);
        }

        public override string ToString()
        {
            return "{Red=" + this.red + ", Green=" + this.green + ", Blue=" + this.blue + ", Alpha=" + this.alpha + "}";
        }
    }

    public class GeometryHeader
    {
        public UInt32 item_size;
        public UInt32 item_type;

        public GeometryHeader()
        {
        }

        public GeometryHeader(UInt32 size, UInt32 type)
        {
            item_size = size;
            item_type = type;
        }
    }

    // Extracted from dsl::wd3d::D3DShaderProgram::compile
    public enum GeometryChannelTypes
    {
        POSITION = 1,
        POSITION0 = 1,
        NORMAL = 2,
        NORMAL0 = 2,
        POSITION1 = 3,
        NORMAL1 = 4,
        COLOR = 5,
        COLOR0 = 5,
        COLOR1 = 6,
        TEXCOORD0 = 7,
        TEXCOORD1 = 8,
        TEXCOORD2 = 9,
        TEXCOORD3 = 10,
        TEXCOORD4 = 11,
        TEXCOORD5 = 12,
        TEXCOORD6 = 13,
        TEXCOORD7 = 14,
        BLENDINDICES = 15,
        BLENDINDICES0 = 15,
        BLENDINDICES1 = 16,
        BLENDWEIGHT = 17,
        BLENDWEIGHT0 = 17,
        BLENDWEIGHT1 = 18,
        POINTSIZE = 19,
        BINORMAL = 20,
        BINORMAL0 = 20,
        TANGENT = 21,
        TANGENT0 = 21,
    }

    class Geometry : ISection
    {
        private static uint geometry_tag = 0x7AB072D3; // Geometry
        public UInt32 id;

        // Count of everysingle item in headers (Verts, Normals, UVs, UVs for normalmap, Colors, Unknown 20, Unknown 21, etc)
        public UInt32 vert_count;

        public UInt32 header_count; //Count of all headers for items in this section
        public UInt32 geometry_size;
        public List<GeometryHeader> headers = new List<GeometryHeader>();
        public List<Vector3D> verts = new List<Vector3D>();
        public List<Vector2D> uvs = new List<Vector2D>();
        public List<Vector2D> pattern_uvs = new List<Vector2D>();
        public List<Vector3D> normals = new List<Vector3D>();
        public List<GeometryColor> vertex_colors = new List<GeometryColor>();
        public List<GeometryWeightGroups> weight_groups = new List<GeometryWeightGroups>(); //4 - Weight Groups
        public List<Vector3D> weights = new List<Vector3D>(); //3 - Weights
        public List<Vector3D> unknown20 = new List<Vector3D>(); //3 - Tangent/Binormal
        public List<Vector3D> unknown21 = new List<Vector3D>(); //3 - Tangent/Binormal

        // Unknown items from this section. Includes colors and a few other items.
        public List<byte[]> unknown_item_data = new List<byte[]>();

        public UInt64 hashname;

        public byte[] remaining_data = null;

        public Geometry(uint sec_id, obj_data newobject)
        {
            this.id = sec_id;

            this.vert_count = (uint) newobject.verts.Count;
            this.header_count = 5;

            this.headers.Add(new GeometryHeader(3, 1)); // vert
            this.headers.Add(new GeometryHeader(2, 7)); // uv
            this.headers.Add(new GeometryHeader(3, 2)); // norm
            this.headers.Add(new GeometryHeader(3, 20)); // unk20
            this.headers.Add(new GeometryHeader(3, 21)); // unk21

            this.verts = newobject.verts;
            this.uvs = newobject.uv;
            this.normals = newobject.normals;
            //this.unknown20;
            //this.unknown21;

            this.hashname = Hash64.HashString(newobject.object_name + ".Geometry");
        }

        public Geometry(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;

            UInt32[] size_index = {0, 4, 8, 12, 16, 4, 4, 8, 12};
            // Count of everysingle item in headers (Verts, Normals, UVs, UVs for normalmap, Colors, Unknown 20, Unknown 21, etc)
            this.vert_count = instream.ReadUInt32();
            //Count of all headers for items in this section
            this.header_count = instream.ReadUInt32();
            UInt32 calc_size = 0;
            for (int x = 0; x < this.header_count; x++)
            {
                GeometryHeader header = new GeometryHeader();
                header.item_size = instream.ReadUInt32();
                header.item_type = instream.ReadUInt32();
                calc_size += size_index[(int) header.item_size];
                this.headers.Add(header);
            }

            this.geometry_size = this.vert_count * calc_size;

            foreach (GeometryHeader head in this.headers)
            {
                //Console.WriteLine("Header type: " + head.item_type + " Size: " + head.item_size);
                if (head.item_type == 1)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D vert = new Vector3D();
                        vert.X = instream.ReadSingle();
                        vert.Y = instream.ReadSingle();
                        vert.Z = instream.ReadSingle();

                        this.verts.Add(vert);
                    }
                }
                else if (head.item_type == 7)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector2D uv = new Vector2D();
                        uv.X = instream.ReadSingle();
                        uv.Y = -instream.ReadSingle();
                        this.uvs.Add(uv);
                    }
                }
                else if (head.item_type == 2)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D norm = new Vector3D();
                        norm.X = instream.ReadSingle();
                        norm.Y = instream.ReadSingle();
                        norm.Z = instream.ReadSingle();
                        this.normals.Add(norm);
                    }
                }
                else if (head.item_type == 8)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector2D pattern_uv_entry = new Vector2D();
                        pattern_uv_entry.X = instream.ReadSingle();
                        pattern_uv_entry.Y = instream.ReadSingle();
                        this.pattern_uvs.Add(pattern_uv_entry);
                    }
                }
                else if (head.item_type == 5)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        this.vertex_colors.Add(new GeometryColor(instream));
                    }
                }
                //Below is unknown data

                else if (head.item_type == 20)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D unknown_20_entry = new Vector3D();
                        unknown_20_entry.X = instream.ReadSingle();
                        unknown_20_entry.Y = instream.ReadSingle();
                        unknown_20_entry.Z = instream.ReadSingle();
                        this.unknown20.Add(unknown_20_entry);
                    }
                }
                else if (head.item_type == 21)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D unknown_21_entry = new Vector3D();
                        unknown_21_entry.X = instream.ReadSingle();
                        unknown_21_entry.Y = instream.ReadSingle();
                        unknown_21_entry.Z = instream.ReadSingle();
                        this.unknown21.Add(unknown_21_entry);
                    }
                }

                //Weight Groups
                else if (head.item_type == 15)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        GeometryWeightGroups unknown_15_entry = new GeometryWeightGroups(instream);
                        this.weight_groups.Add(unknown_15_entry);
                    }
                }

                //Weights
                else if (head.item_type == 17)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D unknown_17_entry = new Vector3D();
                        unknown_17_entry.X = instream.ReadSingle();
                        unknown_17_entry.Y = instream.ReadSingle();
                        unknown_17_entry.Z = instream.ReadSingle();
                        this.weights.Add(unknown_17_entry);
                    }
                }
                else
                {
                    this.unknown_item_data.Add(
                        instream.ReadBytes((int) (size_index[head.item_size] * this.vert_count)));
                }
            }

            this.hashname = instream.ReadUInt64();

            this.remaining_data = null;
            long sect_end = section.offset + 12 + section.size;
            if (sect_end > instream.BaseStream.Position)
            {
                // If exists, this contains hashed name for this geometry (see hashlist.txt)
                remaining_data = instream.ReadBytes((int) (sect_end - instream.BaseStream.Position));
            }
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(geometry_tag);
            outstream.Write(this.id);
            long newsizestart = outstream.BaseStream.Position;
            outstream.Write((uint) 0);

            this.StreamWriteData(outstream);

            //update section size
            long newsizeend = outstream.BaseStream.Position;
            outstream.BaseStream.Position = newsizestart;
            outstream.Write((uint) (newsizeend - (newsizestart + 4)));

            outstream.BaseStream.Position = newsizeend;
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.vert_count);
            outstream.Write(this.header_count);
            foreach (GeometryHeader head in this.headers)
            {
                outstream.Write(head.item_size);
                outstream.Write(head.item_type);
            }

            List<Vector3D> verts = this.verts;
            int vert_pos = 0;
            List<Vector2D> uvs = this.uvs;
            int uv_pos = 0;
            List<Vector3D> normals = this.normals;
            int norm_pos = 0;
            List<Vector2D> pattern_uvs = this.pattern_uvs;
            int pattern_uvs_pos = 0;

            List<GeometryWeightGroups> unknown_15s = this.weight_groups;
            int unknown_15s_pos = 0;
            List<Vector3D> unknown_17s = this.weights;
            int unknown_17s_pos = 0;
            List<Vector3D> unknown_20s = this.unknown20;
            int unknown_20s_pos = 0;
            List<Vector3D> unknown_21s = this.unknown21;
            int unknown_21s_pos = 0;

            List<byte[]> unknown_data = this.unknown_item_data;
            int unknown_data_pos = 0;

            foreach (GeometryHeader head in this.headers)
            {
                if (head.item_type == 1)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D vert = verts[vert_pos];
                        outstream.Write(vert.X);
                        outstream.Write(vert.Y);
                        outstream.Write(vert.Z);
                        vert_pos++;
                    }
                }
                else if (head.item_type == 7)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector2D uv = uvs[uv_pos];
                        outstream.Write(uv.X);
                        outstream.Write(-uv.Y);
                        uv_pos++;
                    }
                }
                else if (head.item_type == 2)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D norm = normals[norm_pos];
                        outstream.Write(norm.X);
                        outstream.Write(norm.Y);
                        outstream.Write(norm.Z);
                        norm_pos++;
                    }
                }

                else if (head.item_type == 5)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        this.vertex_colors[x].StreamWrite(outstream);
                    }
                }

                else if (head.item_type == 8)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        if (this.pattern_uvs.Count != this.vert_count)
                        {
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                        }
                        else
                        {
                            Vector2D pattern_uv_entry = pattern_uvs[pattern_uvs_pos];
                            outstream.Write(pattern_uv_entry.X);
                            outstream.Write(-pattern_uv_entry.Y);
                            pattern_uvs_pos++;
                        }
                    }
                }
                else if (head.item_type == 20)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        if (this.unknown20.Count != this.vert_count)
                        {
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                        }
                        else
                        {
                            Vector3D unknown_20_entry = unknown_20s[x];
                            outstream.Write(unknown_20_entry.X);
                            outstream.Write(unknown_20_entry.Y);
                            outstream.Write(unknown_20_entry.Z);
                            unknown_20s_pos++;
                        }
                    }
                }
                else if (head.item_type == 21)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        if (this.unknown21.Count != this.vert_count)
                        {
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                        }
                        else
                        {
                            Vector3D unknown_21_entry = unknown_21s[x];
                            outstream.Write(unknown_21_entry.X);
                            outstream.Write(unknown_21_entry.Y);
                            outstream.Write(unknown_21_entry.Z);
                            unknown_21s_pos++;
                        }
                    }
                }
                else if (head.item_type == 15)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        if (this.weight_groups.Count != this.vert_count)
                        {
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                        }
                        else
                        {
                            GeometryWeightGroups unknown_15_entry = unknown_15s[x];
                            unknown_15_entry.StreamWrite(outstream);
                            unknown_15s_pos++;
                        }
                    }
                }
                else if (head.item_type == 17)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        if (this.weights.Count != this.vert_count)
                        {
                            outstream.Write(1.0f);
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                        }
                        else
                        {
                            Vector3D unknown_17_entry = unknown_17s[x];
                            outstream.Write(unknown_17_entry.X);
                            outstream.Write(unknown_17_entry.Y);
                            outstream.Write(unknown_17_entry.Z);
                            unknown_17s_pos++;
                        }
                    }
                }
                else
                {
                    outstream.Write(unknown_data[unknown_data_pos]);
                    unknown_data_pos++;
                }
            }

            outstream.Write(this.hashname);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public void PrintDetailedOutput(StreamWriter outstream)
        {
            //for debug purposes
            //following prints the suspected "weights" table

            if (this.weight_groups.Count > 0 && this.unknown20.Count > 0 && this.unknown21.Count > 0 &&
                this.weights.Count > 0)
            {
                outstream.WriteLine("Printing weights table for " + StaticStorage.hashindex.GetString(this.hashname));
                outstream.WriteLine("====================================================");
                outstream.WriteLine(
                    "unkn15_1\tunkn15_2\tunkn15_3\tunkn15_4\tunkn17_X\tunkn17_Y\tunk17_Z\ttotalsum\tunk_20_X\tunk_20_Y\tunk_20_Z\tunk21_X\tunk21_Y\tunk21_Z");


                for (int x = 0; x < this.weight_groups.Count; x++)
                {
                    outstream.WriteLine(this.weight_groups[x].Bones1.ToString() + "\t" +
                                        this.weight_groups[x].Bones2.ToString() + "\t" +
                                        this.weight_groups[x].Bones3.ToString() + "\t" +
                                        this.weight_groups[x].Bones4.ToString() + "\t" +
                                        this.weights[x].X.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.weights[x].Y.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.weights[x].Z.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        (this.weights[x].X + this.weights[x].Y + this.weights[x].Z).ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.unknown20[x].X.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.unknown20[x].Y.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.unknown20[x].Z.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.unknown21[x].X.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.unknown21[x].Y.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.unknown21[x].Z.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture));
                    //outstream.WriteLine(this.unknown15[x].X.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.unknown15[x].Y.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.weights[x].X.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.weights[x].Y.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.weights[x].Z.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + (this.unknown15[x].X + this.unknown15[x].Y + this.weights[x].X + this.weights[x].Y + this.weights[x].Z).ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.unknown20[x].X.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.unknown20[x].Y.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.unknown20[x].Z.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.unknown21[x].X.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.unknown21[x].Y.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "\t" + this.unknown21[x].Z.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture));
                }

                outstream.WriteLine("====================================================");
            }
        }

        public override string ToString()
        {
            return "[Geometry] ID: " + this.id +
                   " Count: " + this.vert_count +
                   " Count2: " + this.header_count +
                   " Headers: " + this.headers.Count +
                   " Size: " + this.geometry_size +
                   " Verts: " + this.verts.Count +
                   " UVs: " + this.uvs.Count +
                   " Pattern UVs: " + this.pattern_uvs.Count +
                   " Normals: " + this.normals.Count +
                   " unknown_15: " + this.weight_groups.Count +
                   " unknown_17: " + this.weights.Count +
                   " unknown_20: " + this.unknown20.Count +
                   " unknown_21: " + this.unknown21.Count +
                   " Geometry_unknown_item_data: " + this.unknown_item_data.Count +
                   " unknown_hash: " + StaticStorage.hashindex.GetString(this.hashname) +
                   (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

        public uint SectionId
        {
            get => id;
            set => id = value;
        }

        public uint TypeCode => Tags.geometry_tag;
    }
}
