using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PD2ModelParser.Sections
{
    public class GeometryWeightGroups
    {
        public UInt16 Bones1;
        public UInt16 Bones2;
        public UInt16 Bones3;
        public UInt16 Bones4;

        public GeometryWeightGroups()
        {
        }

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

        public GeometryColor()
        {
        }

        public GeometryColor(byte red, byte green, byte blue, byte alpha)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

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
        /// <summary>
        /// Byte sizes for various types of geometry buffer. Official data is consistent regarding
        /// which one goes with which <see cref="GeometryChannelTypes"/>.
        /// </summary>
        public static readonly IReadOnlyList<uint> ItemSizes = new List<uint> { 0, 4, 8, 12, 16, 4, 4, 8, 12 };

        /// <summary>
        /// Index into <see cref="ItemSizes"/>
        /// </summary>
        public UInt32 item_size;
        public GeometryChannelTypes item_type;

        public GeometryHeader()
        {
        }

        public GeometryHeader(UInt32 size, GeometryChannelTypes type)
        {
            item_size = size;
            item_type = type;
        }

        public uint ItemSizeBytes { get { return ItemSizes[(int)item_size]; } }
    }

    // Extracted from dsl::wd3d::D3DShaderProgram::compile
    // These are the actual names of each channel, as passed to the shader
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

    [SectionId(Tags.geometry_tag)]
    class Geometry : AbstractSection, ISection
    {
        // Count of everysingle item in headers (Verts, Normals, UVs, UVs for normalmap, Colors, Unknown 20, Unknown 21, etc)
        public UInt32 vert_count;

        public List<Vector2D>[] UVs = new List<Vector2D>[8];

        public UInt32 geometry_size;
        public List<GeometryHeader> headers = new List<GeometryHeader>();
        public List<Vector3D> verts = new List<Vector3D>();
        public List<Vector2D> uvs => UVs[0];
        public List<Vector2D> pattern_uvs => UVs[1];
        public List<Vector3D> normals = new List<Vector3D>();
        public List<GeometryColor> vertex_colors = new List<GeometryColor>();
        public List<GeometryWeightGroups> weight_groups = new List<GeometryWeightGroups>(); //4 - Weight Groups
        public List<Vector3D> weights = new List<Vector3D>(); //3 - Weights
        public List<Vector3D> binormals = new List<Vector3D>(); //3 - Tangent/Binormal
        public List<Vector3D> tangents = new List<Vector3D>(); //3 - Tangent/Binormal

        // Unknown items from this section. Includes colors and a few other items.
        public List<byte[]> unknown_item_data = new List<byte[]>();

        public UInt64 hashname;

        public byte[] remaining_data = null;

        public Geometry(uint id)
        {
            this.SectionId = id;

            for (int i = 0; i < UVs.Length; i++)
            {
                UVs[i] = new List<Vector2D>();
            }
        }

        public Geometry(uint secId, obj_data newobject) : this(secId)
        {
            this.vert_count = (uint) newobject.verts.Count;

            this.headers.Add(new GeometryHeader(3, GeometryChannelTypes.POSITION)); // vert
            this.headers.Add(new GeometryHeader(2, GeometryChannelTypes.TEXCOORD0)); // uv
            this.headers.Add(new GeometryHeader(3, GeometryChannelTypes.NORMAL0)); // norm
            this.headers.Add(new GeometryHeader(3, GeometryChannelTypes.BINORMAL0)); // unk20
            this.headers.Add(new GeometryHeader(3, GeometryChannelTypes.TANGENT0)); // unk21

            this.verts = newobject.verts;
            this.UVs[0] = newobject.uv;
            this.normals = newobject.normals;
            //this.binormals;
            //this.tangents;

            this.hashname = Hash64.HashString(newobject.object_name + ".Geometry");
        }

        public Geometry(BinaryReader instream, SectionHeader section) : this(section.id)
        {
            // Count of everysingle item in headers (Verts, Normals, UVs, UVs for normalmap, Colors, Unknown 20, Unknown 21, etc)
            this.vert_count = instream.ReadUInt32();
            //Count of all headers for items in this section
            uint header_count = instream.ReadUInt32();
            UInt32 calc_size = 0;
            for (int x = 0; x < header_count; x++)
            {
                GeometryHeader header = new GeometryHeader();
                header.item_size = instream.ReadUInt32();
                header.item_type = (GeometryChannelTypes) instream.ReadUInt32();
                calc_size += header.ItemSizeBytes;
                this.headers.Add(header);
            }

            this.geometry_size = this.vert_count * calc_size;

            foreach (GeometryHeader head in this.headers)
            {
                //Console.WriteLine("Header type: " + head.item_type + " Size: " + head.item_size);
                if (head.item_type == GeometryChannelTypes.POSITION)
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
                else if (head.item_type == GeometryChannelTypes.NORMAL)
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
                else if (head.item_type == GeometryChannelTypes.COLOR0)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        this.vertex_colors.Add(new GeometryColor(instream));
                    }
                }
                //Below is unknown data

                else if (head.item_type == GeometryChannelTypes.BINORMAL0)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D binormal_entry = new Vector3D();
                        binormal_entry.X = instream.ReadSingle();
                        binormal_entry.Y = instream.ReadSingle();
                        binormal_entry.Z = instream.ReadSingle();
                        this.binormals.Add(binormal_entry);
                    }
                }
                else if (head.item_type == GeometryChannelTypes.TANGENT0)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D tangent_entry = new Vector3D();
                        tangent_entry.X = instream.ReadSingle();
                        tangent_entry.Y = instream.ReadSingle();
                        tangent_entry.Z = instream.ReadSingle();
                        this.tangents.Add(tangent_entry);
                    }
                }

                //Weight Groups
                else if (head.item_type == GeometryChannelTypes.BLENDINDICES0)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        GeometryWeightGroups unknown_15_entry = new GeometryWeightGroups(instream);
                        this.weight_groups.Add(unknown_15_entry);
                    }
                }

                //Weights
                else if (head.item_type == GeometryChannelTypes.BLENDWEIGHT0)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D unknown_17_entry = new Vector3D();
                        unknown_17_entry.X = instream.ReadSingle();
                        unknown_17_entry.Y = instream.ReadSingle();
                        if (head.item_size == 3)
                            unknown_17_entry.Z = instream.ReadSingle();
                        else if (head.item_size != 2)
                            throw new Exception("Bad BLENDWEIGHT0 item size " + head.item_size);
                        this.weights.Add(unknown_17_entry);
                    }
                }
                else if (head.item_type >= GeometryChannelTypes.TEXCOORD0 &&
                         head.item_type <= GeometryChannelTypes.TEXCOORD7)
                {
                    int idx = head.item_type - GeometryChannelTypes.TEXCOORD0;
                    for (int x = 0; x < vert_count; x++)
                    {
                        // Previously, the Y was only inverted on the TEXCOORD0 channel, and
                        // not on the TEXCOORD1 channel. I assume that was incorrect, TODO check?
                        Vector2D uv = new Vector2D {X = instream.ReadSingle(), Y = -instream.ReadSingle()};
                        UVs[idx].Add(uv);
                    }
                }
                else
                {
                    this.unknown_item_data.Add(
                        instream.ReadBytes((int) (head.ItemSizeBytes * this.vert_count)));
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

        public bool HasHeader(GeometryChannelTypes type)
        {
            return headers.Any(h => h.item_type == type);
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            outstream.Write(this.vert_count);
            outstream.Write(headers.Count);
            foreach (GeometryHeader head in this.headers)
            {
                outstream.Write(head.item_size);
                outstream.Write((uint) head.item_type);
            }

            List<Vector3D> verts = this.verts;
            int vert_pos = 0;
            List<Vector3D> normals = this.normals;
            int norm_pos = 0;

            List<GeometryWeightGroups> unknown_15s = this.weight_groups;
            int unknown_15s_pos = 0;
            List<Vector3D> binormals = this.binormals;
            int binormals_pos = 0;
            List<Vector3D> tangents = this.tangents;
            int tangents_pos = 0;

            List<byte[]> unknown_data = this.unknown_item_data;
            int unknown_data_pos = 0;

            foreach (GeometryHeader head in this.headers)
            {
                if (head.item_type == GeometryChannelTypes.POSITION)
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
                else if (head.item_type == GeometryChannelTypes.NORMAL)
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

                else if (head.item_type == GeometryChannelTypes.COLOR)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        this.vertex_colors[x].StreamWrite(outstream);
                    }
                }
                else if (head.item_type == GeometryChannelTypes.BINORMAL)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        if (this.binormals.Count != this.vert_count)
                        {
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                        }
                        else
                        {
                            Vector3D binormal_entry = binormals[x];
                            outstream.Write(binormal_entry.X);
                            outstream.Write(binormal_entry.Y);
                            outstream.Write(binormal_entry.Z);
                            binormals_pos++;
                        }
                    }
                }
                else if (head.item_type == GeometryChannelTypes.TANGENT)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        if (this.tangents.Count != this.vert_count)
                        {
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                            outstream.Write(0.0f);
                        }
                        else
                        {
                            Vector3D tangent_entry = tangents[x];
                            outstream.Write(tangent_entry.X);
                            outstream.Write(tangent_entry.Y);
                            outstream.Write(tangent_entry.Z);
                            tangents_pos++;
                        }
                    }
                }
                else if (head.item_type == GeometryChannelTypes.BLENDINDICES)
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
                else if (head.item_type == GeometryChannelTypes.BLENDWEIGHT)
                {
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector3D weight = this.weights.Count != this.vert_count ? Vector3D.UnitX : weights[x];
                        outstream.Write(weight.X);
                        outstream.Write(weight.Y);

                        if (head.item_size == 3)
                            outstream.Write(weight.Z);
                        else if (head.item_size != 2)
                            throw new Exception("Cannot write bad header BLENDWEIGHT s=" + head.item_size);
                    }
                }
                else if (head.item_type >= GeometryChannelTypes.TEXCOORD0 &&
                         head.item_type <= GeometryChannelTypes.TEXCOORD7)
                {
                    int idx = head.item_type - GeometryChannelTypes.TEXCOORD0;
                    for (int x = 0; x < this.vert_count; x++)
                    {
                        Vector2D uv = UVs[idx][x];
                        outstream.Write(uv.X);
                        outstream.Write(-uv.Y);
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

            if (this.weight_groups.Count > 0 && this.binormals.Count > 0 && this.tangents.Count > 0 &&
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
                                        this.binormals[x].X.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.binormals[x].Y.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.binormals[x].Z.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.tangents[x].X.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.tangents[x].Y.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture) + "\t" +
                                        this.tangents[x].Z.ToString("0.000000",
                                            System.Globalization.CultureInfo.InvariantCulture));
                }

                outstream.WriteLine("====================================================");
            }
        }

        public override string ToString()
        {
            return base.ToString() +
                   " Count: " + this.vert_count +
                   " Headers: " + this.headers.Count +
                   " Size: " + this.geometry_size +
                   " Verts: " + this.verts.Count +
                   " UVs: " + this.uvs.Count +
                   " Pattern UVs: " + this.pattern_uvs.Count +
                   " Normals: " + this.normals.Count +
                   " weight_groups: " + this.weight_groups.Count +
                   " weights: " + this.weights.Count +
                   " binormals: " + this.binormals.Count +
                   " tangents: " + this.tangents.Count +
                   " Geometry_unknown_item_data: " + this.unknown_item_data.Count +
                   " unknown_hash: " + StaticStorage.hashindex.GetString(this.hashname) +
                   (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }
    }
}
