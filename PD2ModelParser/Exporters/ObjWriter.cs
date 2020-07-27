using Nexus;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using static PD2ModelParser.Tags;

namespace PD2ModelParser
{
    class ObjWriter
    {

        public static string ExportFile(FullModelData data, string filepath)
        {
            string output_file = filepath.Replace(".model", ".obj");

            GenerateOutputInfo(data, "outinfo.txt");

            ExportObj(data, output_file); // TODO configure output file

            ExportPatternUVObj(data, filepath.Replace(".model", "_pattern_uv.obj")); // TODO configure output file

            return output_file;
        }

        private static void GenerateOutputInfo(FullModelData data, string path)
        {
            List<SectionHeader> sections = data.sections;
            Dictionary<UInt32, ISection> parsed_sections = data.parsed_sections;
            byte[] leftover_data = data.leftover_data;

            //Generate outinfo.txt - Used for research and debug purposes
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (SectionHeader sectionheader in sections)
                    {
                        if (sectionheader.type == passthroughGP_tag)
                        {
                            PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[sectionheader.id];
                            Geometry geometry_section = passthrough_section.Geometry;
                            Topology topology_section = passthrough_section.Topology;
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
                                    arranged_unknown20[f.a] = norm_binorms[topology_section.facelist[fcount].a];
                                    arranged_unknown20[f.b] = norm_binorms[topology_section.facelist[fcount].b];
                                    arranged_unknown20[f.c] = norm_binorms[topology_section.facelist[fcount].c];

                                    //unknown_21
                                    arranged_unknown21[f.a] = norm_tangents[topology_section.facelist[fcount].a];
                                    arranged_unknown21[f.b] = norm_tangents[topology_section.facelist[fcount].b];
                                    arranged_unknown21[f.c] = norm_tangents[topology_section.facelist[fcount].c];

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
                            foreach (Vector3D unknown_20_entry in geometry_section.binormals)
                            {
                                sw.WriteLine(unknown_20_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_20_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_20_entry.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                Vector3D normt = norm_tangents[unk20tangcount];
                                sw.WriteLine("* " + normt.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                unk20tangcount++;
                            }
                            sw.WriteLine("Unknown 21 (float, float, float) - Normal tangents???");
                            foreach (Vector3D unknown_21_entry in geometry_section.tangents)
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
                                sw.WriteLine((face.a + 1) + " " + (face.b + 1) + " " + (face.c + 1));
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
        }

        private static void ExportObj(FullModelData data, string path)
        {
            List<SectionHeader> sections = data.sections;
            Dictionary<UInt32, ISection> parsed_sections = data.parsed_sections;
            byte[] leftover_data = data.leftover_data;

            //Generate obj
            ushort maxfaces = 0;
            UInt32 uvcount = 0;
            UInt32 normalcount = 0;

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
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
                            PassthroughGP passthrough_section = model_data.PassthroughGP;
                            Geometry geometry_section = passthrough_section.Geometry;
                            Topology topology_section = passthrough_section.Topology;



                            sw.WriteLine("#");
                            sw.WriteLine("# object " + model_data.Name);
                            sw.WriteLine("#");
                            sw.WriteLine();
                            sw.WriteLine("o " + model_data.Name);
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

                            sw.WriteLine("g " + model_data.Name);
                            foreach (Face face in topology_section.facelist)
                            {
                                //x
                                sw.Write("f " + (maxfaces + face.a + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.a + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.a + 1));

                                //y
                                sw.Write(" " + (maxfaces + face.b + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.b + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.b + 1));

                                //z
                                sw.Write(" " + (maxfaces + face.c + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.c + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.c + 1));

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
        }

        // Please don't ask what this does
        private static void ExportPatternUVObj(FullModelData data, string path)
        {
            List<SectionHeader> sections = data.sections;
            Dictionary<UInt32, ISection> parsed_sections = data.parsed_sections;
            byte[] leftover_data = data.leftover_data;

            //Generate obj
            ushort maxfaces = 0;
            UInt32 uvcount = 0;
            UInt32 normalcount = 0;

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
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
                            PassthroughGP passthrough_section = model_data.PassthroughGP;
                            Geometry geometry_section = passthrough_section.Geometry;
                            Topology topology_section = passthrough_section.Topology;

                            sw.WriteLine("#");
                            sw.WriteLine("# object " + model_data.Name);
                            sw.WriteLine("#");
                            sw.WriteLine();
                            sw.WriteLine("o " + model_data.Name);
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

                            sw.WriteLine("g " + model_data.Name);
                            foreach (Face face in topology_section.facelist)
                            {
                                //x
                                sw.Write("f " + (maxfaces + face.a + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.a + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.a + 1));

                                //y
                                sw.Write(" " + (maxfaces + face.b + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.b + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.b + 1));

                                //z
                                sw.Write(" " + (maxfaces + face.c + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.c + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.c + 1));

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
    }
}
