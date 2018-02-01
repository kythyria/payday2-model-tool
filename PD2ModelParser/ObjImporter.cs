
using Nexus;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PD2ModelParser
{
    class ObjImporter
    {
        public static uint animation_data_tag = 0x5DC011B8; // Animation data
        public static uint author_tag = 0x7623C465; // Author tag
        public static uint material_group_tag = 0x29276B1D; // Material Group
        public static uint material_tag = 0x3C54609C; // Material
        public static uint object3D_tag = 0x0FFCD100; // Object3D
        public static uint model_data_tag = 0x62212D88; // Model data
        public static uint geometry_tag = 0x7AB072D3; // Geometry
        public static uint topology_tag = 0x4C507A13; // Topology
        public static uint passthroughGP_tag = 0xE3A3B1CA; // PassthroughGP
        public static uint topologyIP_tag = 0x03B634BD;  // TopologyIP
        public static uint quatLinearRotationController_tag = 0x648A206C; // QuatLinearRotationController
        public static uint quatBezRotationController_tag = 0x197345A5; // QuatBezRotationController
        public static uint skinbones_tag = 0x65CC1825; // SkinBones
        public static uint bones_tag = 0xEB43C77; // Bones
        public static uint light_tag = 0xFFA13B80; //Light
        public static uint lightSet_tag = 0x33552583; //LightSet
        public static uint linearVector3Controller_tag = 0x26A5128C; //LinearVector3Controller
        public static uint linearFloatController_tag = 0x76BF5B66; //LinearFloatController
        public static uint lookAtConstrRotationController = 0x679D695B; //LookAtConstrRotationController
        public static uint camera_tag = 0x46BF31A7; //Camera

        public List<SectionHeader> sections = new List<SectionHeader>();
        public Dictionary<UInt32, object> parsed_sections = new Dictionary<UInt32, object>();
        public byte[] leftover_data = null;

        public void Open(string filepath, string rp = null)
        {
            StaticStorage.hashindex.Load();

            Console.WriteLine("Loading: " + filepath);

            uint offset = 0;

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int random = br.ReadInt32();
                    offset += 4;
                    int filesize = br.ReadInt32();
                    offset += 4;
                    int sectionCount;
                    if (random == -1)
                    {
                        sectionCount = br.ReadInt32();
                        offset += 4;
                    }
                    else
                        sectionCount = random;

                    Console.WriteLine("Size: " + filesize + " bytes, Sections: " + sectionCount);

                    for (int x = 0; x < sectionCount; x++)
                    {
                        SectionHeader sectionHead = new SectionHeader(br);
                        sections.Add(sectionHead);
                        Console.WriteLine(sectionHead);
                        offset += sectionHead.size + 12;
                        Console.WriteLine("Next offset: " + offset);
                        fs.Position = (long)offset;
                    }


                    foreach (SectionHeader sh in sections)
                    {
                        object section = new object();

                        fs.Position = sh.offset + 12;

                        if (sh.type == animation_data_tag)
                        {
                            Console.WriteLine("Animation Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Animation(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == author_tag)
                        {
                            Console.WriteLine("Author Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Author(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == material_group_tag)
                        {
                            Console.WriteLine("Material Group Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Material_Group(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == material_tag)
                        {
                            Console.WriteLine("Material Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Material(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == object3D_tag)
                        {
                            Console.WriteLine("Object 3D Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Object3D(br, sh);

                            if ((section as Object3D).hashname == 4921176767231919846)
                                Console.WriteLine();

                            Console.WriteLine(section);
                        }
                        else if (sh.type == geometry_tag)
                        {
                            Console.WriteLine("Geometry Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Geometry(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == model_data_tag)
                        {
                            Console.WriteLine("Model Data Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Model(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == topology_tag)
                        {
                            Console.WriteLine("Topology Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Topology(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == passthroughGP_tag)
                        {
                            Console.WriteLine("passthroughGP Tag at " + sh.offset + " Size: " + sh.size);

                            section = new PassthroughGP(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == topologyIP_tag)
                        {
                            Console.WriteLine("TopologyIP Tag at " + sh.offset + " Size: " + sh.size);

                            section = new TopologyIP(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == bones_tag)
                        {
                            Console.WriteLine("Bones Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Bones(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == skinbones_tag)
                        {
                            Console.WriteLine("SkinBones Tag at " + sh.offset + " Size: " + sh.size);

                            section = new SkinBones(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == quatLinearRotationController_tag)
                        {
                            Console.WriteLine("QuatLinearRotationController Tag at " + sh.offset + " Size: " + sh.size);

                            section = new QuatLinearRotationController(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == linearVector3Controller_tag)
                        {
                            Console.WriteLine("QuatLinearRotationController Tag at " + sh.offset + " Size: " + sh.size);

                            section = new LinearVector3Controller(br, sh);

                            Console.WriteLine(section);
                        }
                        else
                        {
                            Console.WriteLine("UNKNOWN Tag at " + sh.offset + " Size: " + sh.size);
                            fs.Position = sh.offset;

                            section = new Unknown(br, sh);

                            Console.WriteLine(section);
                        }

                        parsed_sections.Add(sh.id, section);
                    }

                    if (fs.Position < fs.Length)
                        leftover_data = br.ReadBytes((int)(fs.Length - fs.Position));

                    br.Close();
                }
                fs.Close();
            }

            if (rp != null)
            {
                this.updateRP(rp);
            }


            //Generate outinfo.txt - Used for research and debug purposes
            using (FileStream fs = new FileStream("outinfo.txt", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (SectionHeader sectionheader in sections)
                    {
                        if (sectionheader.type == passthroughGP_tag)
                        {
                            PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[sectionheader.id];
                            Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                            Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];
                            sw.WriteLine("Object ID: " + sectionheader.id);
                            sw.WriteLine("Verts (x, z, y)");
                            foreach (Vector3D vert in geometry_section.verts)
                            {
                                sw.WriteLine(vert.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Z.ToString("0.000000", CultureInfo.InvariantCulture) + " " + (-vert.Y).ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("UV (u, -v)");
                            foreach (Vector2D uv in geometry_section.uvs)
                            {
                                sw.WriteLine(uv.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + uv.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("Normals (i, j, k)");

                            //Testing
                            List<Vector3D> norm_tangents = new List<Vector3D>();
                            List<Vector3D> norm_binorms = new List<Vector3D>();

                            foreach (Vector3D norm in geometry_section.normals)
                            {
                                sw.WriteLine(norm.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Z.ToString("0.000000", CultureInfo.InvariantCulture));


                                Vector3D norm_t;
                                Vector3D binorm;

                                Vector3D t1 = Vector3D.Cross(norm, Vector3D.Right);
                                Vector3D t2 = Vector3D.Cross(norm, Vector3D.Forward);

                                if (t1.Length() > t2.Length())
                                    norm_t = t1;
                                else
                                    norm_t = t2;
                                norm_t.Normalize();
                                norm_tangents.Add(norm_t);

                                binorm = Vector3D.Cross(norm, norm_t);
                                binorm = new Vector3D(binorm.X * -1.0f, binorm.Y * -1.0f, binorm.Z * -1.0f);
                                binorm.Normalize();
                                norm_binorms.Add(binorm);

                            }

                            if (norm_binorms.Count > 0 && norm_tangents.Count > 0)
                            {
                                Vector3D[] arranged_unknown20 = norm_binorms.ToArray();
                                Vector3D[] arranged_unknown21 = norm_tangents.ToArray();

                                for (int fcount = 0; fcount < topology_section.facelist.Count; fcount++)
                                {
                                    Face f = topology_section.facelist[fcount];

                                    //unknown_20
                                    arranged_unknown20[f.x] = norm_binorms[topology_section.facelist[fcount].x];
                                    arranged_unknown20[f.y] = norm_binorms[topology_section.facelist[fcount].y];
                                    arranged_unknown20[f.z] = norm_binorms[topology_section.facelist[fcount].z];

                                    //unknown_21
                                    arranged_unknown21[f.x] = norm_tangents[topology_section.facelist[fcount].x];
                                    arranged_unknown21[f.y] = norm_tangents[topology_section.facelist[fcount].y];
                                    arranged_unknown21[f.z] = norm_tangents[topology_section.facelist[fcount].z];

                                }
                                norm_binorms = arranged_unknown20.ToList();
                                norm_tangents = arranged_unknown21.ToList();
                            }

                            sw.WriteLine("Pattern UVs (u, v)");
                            foreach (Vector2D pattern_uv_entry in geometry_section.pattern_uvs)
                            {
                                sw.WriteLine(pattern_uv_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + pattern_uv_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }

                            int unk20tangcount = 0;
                            int unk21tangcount = 0;
                            sw.WriteLine("Unknown 20 (float, float, float) - Normal tangents???");
                            foreach (Vector3D unknown_20_entry in geometry_section.unknown20)
                            {
                                sw.WriteLine(unknown_20_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_20_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_20_entry.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                Vector3D normt = norm_tangents[unk20tangcount];
                                sw.WriteLine("* " + normt.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                unk20tangcount++;
                            }
                            sw.WriteLine("Unknown 21 (float, float, float) - Normal tangents???");
                            foreach (Vector3D unknown_21_entry in geometry_section.unknown21)
                            {
                                sw.WriteLine(unknown_21_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_21_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_21_entry.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                Vector3D normt = norm_binorms[unk21tangcount];
                                sw.WriteLine("* " + normt.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                unk21tangcount++;
                            }

                            sw.WriteLine("Unknown 15 (float, float) - Weights???");
                            foreach (GeometryWeightGroups unknown_15_entry in geometry_section.weight_groups)
                            {
                                sw.WriteLine(unknown_15_entry.Bones1.ToString() + " " + unknown_15_entry.Bones2.ToString() + " " + unknown_15_entry.Bones3.ToString() + " " + unknown_15_entry.Bones4.ToString());

                                //sw.WriteLine(unknown_15_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_15_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }

                            sw.WriteLine("Unknown 17 (float, float, float) - Weights???");
                            foreach (Vector3D unknown_17_entry in geometry_section.weights)
                            {
                                sw.WriteLine(unknown_17_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_17_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }

                            foreach (Byte[] gunkid in geometry_section.unknown_item_data)
                            {
                                if (gunkid.Length % 8 == 0)
                                {
                                    sw.WriteLine("Unknown X (float, float)");
                                    for (int x = 0; x < gunkid.Length;)
                                    {
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture) + " ");
                                        x += 4;
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture));
                                        x += 4;
                                        sw.WriteLine();

                                    }
                                }
                                else if (gunkid.Length % 12 == 0)
                                {
                                    sw.WriteLine("Unknown X (float, float, float)");
                                    for (int x = 0; x < gunkid.Length;)
                                    {
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture) + " ");
                                        x += 4;
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture) + " ");
                                        x += 4;
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture));
                                        x += 4;
                                        sw.WriteLine();

                                    }
                                }
                                else if (gunkid.Length % 4 == 0)
                                {
                                    sw.WriteLine("Unknown X (float)");
                                    for (int x = 0; x < gunkid.Length;)
                                    {
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture));
                                        x += 4;
                                        sw.WriteLine();

                                    }
                                }
                                else
                                {
                                    sw.Write("Something else... [for debugging]");
                                }
                            }

                            sw.WriteLine("Faces (f1, f2, f3)");
                            foreach (Face face in topology_section.facelist)
                            {
                                sw.WriteLine((face.x + 1) + " " + (face.y + 1) + " " + (face.z + 1));
                            }

                            sw.WriteLine();
                            geometry_section.PrintDetailedOutput(sw);
                            sw.WriteLine();
                        }

                    }

                    sw.Close();
                }
                fs.Close();
            }


            //Generate obj
            ushort maxfaces = 0;
            UInt32 uvcount = 0;
            UInt32 normalcount = 0;
            string newfolder = "";//@"c:/Program Files (x86)/Steam/SteamApps/common/PAYDAY 2/models/";

            Directory.CreateDirectory(Path.GetDirectoryName((newfolder + filepath).Replace(".model", ".obj")));

            using (FileStream fs = new FileStream((newfolder + filepath).Replace(".model", ".obj"), FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (SectionHeader sectionheader in sections)
                    {
                        if (sectionheader.type == model_data_tag)
                        {
                            Model model_data = (Model)parsed_sections[sectionheader.id];
                            if (model_data.version == 6)
                                continue;
                            PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[model_data.passthroughGP_ID];
                            Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                            Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];



                            sw.WriteLine("#");
                            sw.WriteLine("# object " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            sw.WriteLine("#");
                            sw.WriteLine();
                            sw.WriteLine("o " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Vector3D vert in geometry_section.verts)
                            {
                                sw.WriteLine("v " + vert.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.verts.Count + " vertices");
                            sw.WriteLine();

                            foreach (Vector2D uv in geometry_section.uvs)
                            {
                                sw.WriteLine("vt " + uv.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + uv.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.uvs.Count + " UVs");
                            sw.WriteLine();

                            foreach (Vector3D norm in geometry_section.normals)
                            {
                                sw.WriteLine("vn " + norm.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.normals.Count + " Normals");
                            sw.WriteLine();

                            sw.WriteLine("g " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Face face in topology_section.facelist)
                            {
                                //x
                                sw.Write("f " + (maxfaces + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.x + 1));

                                //y
                                sw.Write(" " + (maxfaces + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.y + 1));

                                //z
                                sw.Write(" " + (maxfaces + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.z + 1));

                                sw.WriteLine();
                            }
                            sw.WriteLine("# " + topology_section.facelist.Count + " Faces");
                            sw.WriteLine();

                            maxfaces += (ushort)geometry_section.verts.Count;
                            uvcount += (ushort)geometry_section.uvs.Count;
                            normalcount += (ushort)geometry_section.normals.Count;
                        }
                    }

                    sw.Close();
                }
                fs.Close();
            }

            //Pattern UV
            maxfaces = 0;
            uvcount = 0;
            normalcount = 0;

            Directory.CreateDirectory(Path.GetDirectoryName((newfolder + filepath).Replace(".model", "_pattern_uv.obj")));

            using (FileStream fs = new FileStream((newfolder + filepath).Replace(".model", "_pattern_uv.obj"), FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (SectionHeader sectionheader in sections)
                    {
                        if (sectionheader.type == model_data_tag)
                        {
                            Model model_data = (Model)parsed_sections[sectionheader.id];
                            if (model_data.version == 6)
                                continue;
                            PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[model_data.passthroughGP_ID];
                            Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                            Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];



                            sw.WriteLine("#");
                            sw.WriteLine("# object " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            sw.WriteLine("#");
                            sw.WriteLine();
                            sw.WriteLine("o " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Vector3D vert in geometry_section.verts)
                            {
                                sw.WriteLine("v " + vert.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.verts.Count + " vertices");
                            sw.WriteLine();

                            foreach (Vector2D uv in geometry_section.pattern_uvs)
                            {
                                sw.WriteLine("vt " + uv.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + (-uv.Y).ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.pattern_uvs.Count + " UVs");
                            sw.WriteLine();

                            foreach (Vector3D norm in geometry_section.normals)
                            {
                                sw.WriteLine("vn " + norm.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.normals.Count + " Normals");
                            sw.WriteLine();

                            sw.WriteLine("g " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Face face in topology_section.facelist)
                            {
                                //x
                                sw.Write("f " + (maxfaces + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.x + 1));

                                //y
                                sw.Write(" " + (maxfaces + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.y + 1));

                                //z
                                sw.Write(" " + (maxfaces + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.z + 1));

                                sw.WriteLine();
                            }
                            sw.WriteLine("# " + topology_section.facelist.Count + " Faces");
                            sw.WriteLine();

                            maxfaces += (ushort)geometry_section.verts.Count;
                            uvcount += (ushort)geometry_section.pattern_uvs.Count;
                            normalcount += (ushort)geometry_section.normals.Count;
                        }
                    }

                    sw.Close();
                }
                fs.Close();
            }



        }

        public bool updateRP(string rp)
        {
            ulong rp_hash = Hash64.HashString(rp);
            foreach (object section in this.parsed_sections.Values)
            {
                if (section is Object3D)
                {
                    if ((section as Object3D).hashname == rp_hash)
                    {
                        StaticStorage.rp_id = (section as Object3D).id;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
