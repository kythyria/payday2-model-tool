﻿using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PD2ModelParser.Importers
{
    static class NewObjImporter
    {
        public static void ImportNewObj(FullModelData fmd, String filepath, bool addNew, Func<string, Object3D> root_point, Importers.IOptionReceiver _)
        {
            Log.Default.Info("Importing new obj with file: {0}", filepath);

            //Preload the .obj
            List<obj_data> objects = new List<obj_data>();
            List<obj_data> toAddObjects = new List<obj_data>();

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    obj_data obj = new obj_data();
                    bool reading_faces = false;
                    int prevMaxVerts = 0;
                    int prevMaxUvs = 0;
                    int prevMaxNorms = 0;
                    string current_shade_group = null;

                    while ((line = sr.ReadLine()) != null)
                    {

                        //preloading objects
                        if (line.StartsWith("#"))
                            continue;
                        else if (line.StartsWith("o ") || line.StartsWith("g "))
                        {

                            if (reading_faces)
                            {
                                reading_faces = false;
                                prevMaxVerts += obj.verts.Count;
                                prevMaxUvs += obj.uv.Count;
                                prevMaxNorms += obj.normals.Count;

                                objects.Add(obj);
                                obj = new obj_data();
                                current_shade_group = null;
                            }

                            if (String.IsNullOrEmpty(obj.object_name))
                            {
                                obj.object_name = line.Substring(2);
                                Log.Default.Debug("Object {0} named: {1}", objects.Count + 1, obj.object_name);
                            }
                        }
                        else if (line.StartsWith("usemtl "))
                        {
                            obj.material_name = line.Substring(7);
                        }
                        else if (line.StartsWith("v "))
                        {

                            if (reading_faces)
                            {
                                reading_faces = false;
                                prevMaxVerts += obj.verts.Count;
                                prevMaxUvs += obj.uv.Count;
                                prevMaxNorms += obj.normals.Count;

                                objects.Add(obj);
                                obj = new obj_data();
                            }

                            String[] verts = line.Replace("  ", " ").Split(' ');
                            Vector3 vert = new Vector3();
                            vert.X = Convert.ToSingle(verts[1], CultureInfo.InvariantCulture);
                            vert.Y = Convert.ToSingle(verts[2], CultureInfo.InvariantCulture);
                            vert.Z = Convert.ToSingle(verts[3], CultureInfo.InvariantCulture);

                            obj.verts.Add(vert);
                        }
                        else if (line.StartsWith("vt "))
                        {

                            if (reading_faces)
                            {
                                reading_faces = false;
                                prevMaxVerts += obj.verts.Count;
                                prevMaxUvs += obj.uv.Count;
                                prevMaxNorms += obj.normals.Count;

                                objects.Add(obj);
                                obj = new obj_data();
                            }

                            String[] uvs = line.Split(' ');
                            Vector2 uv = new Vector2();
                            uv.X = Convert.ToSingle(uvs[1], CultureInfo.InvariantCulture);
                            uv.Y = Convert.ToSingle(uvs[2], CultureInfo.InvariantCulture);

                            obj.uv.Add(uv);
                        }
                        else if (line.StartsWith("vn "))
                        {

                            if (reading_faces)
                            {
                                reading_faces = false;
                                prevMaxVerts += obj.verts.Count;
                                prevMaxUvs += obj.uv.Count;
                                prevMaxNorms += obj.normals.Count;

                                objects.Add(obj);
                                obj = new obj_data();
                            }

                            String[] norms = line.Split(' ');
                            Vector3 norm = new Vector3();
                            norm.X = Convert.ToSingle(norms[1], CultureInfo.InvariantCulture);
                            norm.Y = Convert.ToSingle(norms[2], CultureInfo.InvariantCulture);
                            norm.Z = Convert.ToSingle(norms[3], CultureInfo.InvariantCulture);

                            obj.normals.Add(norm);
                        }
                        else if (line.StartsWith("s "))
                        {
                            current_shade_group = line.Substring(2);
                        }
                        else if (line.StartsWith("f "))
                        {
                            reading_faces = true;

                            if (current_shade_group != null)
                            {
                                if (obj.shading_groups.ContainsKey(current_shade_group))
                                    obj.shading_groups[current_shade_group].Add(obj.faces.Count);
                                else
                                {
                                    List<int> newfaces = new List<int>();
                                    newfaces.Add(obj.faces.Count);
                                    obj.shading_groups.Add(current_shade_group, newfaces);
                                }
                            }

                            String[] faces = line.Substring(2).Split(' ');
                            for (int x = 0; x < 3; x++)
                            {
                                ushort fa = 0, fb = 0, fc = 0;
                                if (obj.verts.Count > 0)
                                    fa = (ushort)(Convert.ToUInt16(faces[x].Split('/')[0]) - prevMaxVerts - 1);
                                if (obj.uv.Count > 0)
                                    fb = (ushort)(Convert.ToUInt16(faces[x].Split('/')[1]) - prevMaxUvs - 1);
                                if (obj.normals.Count > 0)
                                    fc = (ushort)(Convert.ToUInt16(faces[x].Split('/')[2]) - prevMaxNorms - 1);
                                if (fa < 0 || fb < 0 || fc < 0)
                                    throw new Exception("What the actual flapjack, something is *VERY* wrong");
                                obj.faces.Add(new Face(fa, fb, fc));
                            }
                        }
                    }

                    if (!objects.Contains(obj))
                        objects.Add(obj);
                }
            }


            //Read each object
            foreach (obj_data obj in objects)
            {
                //One would fix Tatsuto's broken shading here.

                //Locate the proper model
                var hashname = HashName.FromNumberOrString(obj.object_name);
                Model modelSection = fmd.parsed_sections
                    .Where(i => i.Value is Model mod && hashname.Hash == mod.HashName.Hash)
                    .Select(i => i.Value as Model)
                    .FirstOrDefault();

                //Apply new changes
                if (modelSection == null)
                {
                    toAddObjects.Add(obj);
                    continue;
                }

                PassthroughGP passthrough_section = modelSection.PassthroughGP;
                Geometry geometry_section = passthrough_section.Geometry;
                Topology topology_section = passthrough_section.Topology;

                AddObject(false, obj,
                    modelSection, passthrough_section,
                    geometry_section, topology_section);
            }


            //Add new objects
            if (addNew)
            {
                foreach (obj_data obj in toAddObjects)
                {
                    //create new Model
                    Material newMat = new Material(obj.material_name);
                    fmd.AddSection(newMat);
                    MaterialGroup newMatG = new MaterialGroup(newMat);
                    fmd.AddSection(newMatG);
                    Geometry newGeom = new Geometry(obj);
                    fmd.AddSection(newGeom);
                    Topology newTopo = new Topology(obj);
                    fmd.AddSection(newTopo);

                    PassthroughGP newPassGP = new PassthroughGP(newGeom, newTopo);
                    fmd.AddSection(newPassGP);
                    TopologyIP newTopoIP = new TopologyIP(newTopo);
                    fmd.AddSection(newTopoIP);

                    Object3D parent = root_point.Invoke(obj.object_name);
                    Model newModel = new Model(obj, newPassGP, newTopoIP, newMatG, parent);
                    fmd.AddSection(newModel);

                    AddObject(true, obj,
                        newModel, newPassGP, newGeom, newTopo);

                    //Add new sections
                }
            }
        }

        private static void AddObject(bool is_new, obj_data obj,
            Model model_data_section, PassthroughGP passthrough_section,
            Geometry geometry_section, Topology topology_section)
        {
            List<Face> called_faces = new List<Face>();
            List<int> duplicate_verts = new List<int>();
            Dictionary<int, Face> dup_faces = new Dictionary<int, Face>();

            bool broken = false;
            for (int x_f = 0; x_f < obj.faces.Count; x_f++)
            {
                Face f = obj.faces[x_f];
                broken = false;

                foreach (Face called_f in called_faces)
                {
                    if (called_f.a == f.a && called_f.b != f.b)
                    {
                        duplicate_verts.Add(x_f);
                        broken = true;
                        break;
                    }
                }

                if (!broken)
                    called_faces.Add(f);
            }

            Dictionary<int, Face> done_faces = new Dictionary<int, Face>();

            foreach (int dupe in duplicate_verts)
            {
                int replacedF = -1;
                foreach (KeyValuePair<int, Face> pair in done_faces)
                {
                    Face f = pair.Value;
                    if (f.a == obj.faces[dupe].a && f.b == obj.faces[dupe].b)
                    {
                        replacedF = pair.Key;
                    }
                }

                Face new_face;
                if (replacedF > -1)
                {
                    new_face = new Face(obj.faces[replacedF].a, obj.faces[replacedF].b, obj.faces[dupe].c);

                }
                else
                {
                    new_face = new Face((ushort)obj.verts.Count, obj.faces[dupe].b, obj.faces[dupe].c);
                    obj.verts.Add(obj.verts[obj.faces[dupe].a]);

                    done_faces.Add(dupe, obj.faces[dupe]);
                }

                obj.faces[dupe] = new_face;
            }

            Vector3 new_Model_data_bounds_min = new Vector3();// Z (max), X (low), Y (low)
            Vector3 new_Model_data_bounds_max = new Vector3();// Z (low), X (max), Y (max)

            foreach (Vector3 vert in obj.verts)
            {
                //Z
                // Note these were previously broken
                if (vert.Z < new_Model_data_bounds_min.Z)
                    new_Model_data_bounds_min.Z = vert.Z;

                if (vert.Z > new_Model_data_bounds_max.Z)
                    new_Model_data_bounds_max.Z = vert.Z;

                //X
                if (vert.X < new_Model_data_bounds_min.X)
                    new_Model_data_bounds_min.X = vert.X;
                if (vert.X > new_Model_data_bounds_max.X)
                    new_Model_data_bounds_max.X = vert.X;

                //Y
                if (vert.Y < new_Model_data_bounds_min.Y)
                    new_Model_data_bounds_min.Y = vert.Y;

                if (vert.Y > new_Model_data_bounds_max.Y)
                    new_Model_data_bounds_max.Y = vert.Y;
            }

            //Arrange UV and Normals
            List<Vector3> new_arranged_Geometry_normals = new List<Vector3>();
            List<Vector3> new_arranged_Geometry_unknown20 = new List<Vector3>();
            List<Vector3> new_arranged_Geometry_unknown21 = new List<Vector3>();
            List<int> added_uvs = new List<int>();
            List<int> added_normals = new List<int>();

            Vector2[] new_arranged_UV = new Vector2[obj.verts.Count];
            for (int x = 0; x < new_arranged_UV.Length; x++)
                new_arranged_UV[x] = new Vector2(100f, 100f);
            Vector2 sentinel = new Vector2(100f, 100f);
            Vector3[] new_arranged_Normals = new Vector3[obj.verts.Count];
            for (int x = 0; x < new_arranged_Normals.Length; x++)
                new_arranged_Normals[x] = new Vector3(0f, 0f, 0f);
            Vector3[] new_arranged_unknown20 = new Vector3[obj.verts.Count];
            Vector3[] new_arranged_unknown21 = new Vector3[obj.verts.Count];

            List<Face> new_faces = new List<Face>();

            for (int fcount = 0; fcount < obj.faces.Count; fcount += 3)
            {
                Face f1 = obj.faces[fcount + 0];
                Face f2 = obj.faces[fcount + 1];
                Face f3 = obj.faces[fcount + 2];

                //UV
                if (obj.uv.Count > 0)
                {
                    if (new_arranged_UV[f1.a].Equals(sentinel))
                        new_arranged_UV[f1.a] = obj.uv[f1.b];
                    if (new_arranged_UV[f2.a].Equals(sentinel))
                        new_arranged_UV[f2.a] = obj.uv[f2.b];
                    if (new_arranged_UV[f3.a].Equals(sentinel))
                        new_arranged_UV[f3.a] = obj.uv[f3.b];
                }

                //normal
                if (obj.normals.Count > 0)
                {
                    new_arranged_Normals[f1.a] = obj.normals[f1.c];
                    new_arranged_Normals[f2.a] = obj.normals[f2.c];
                    new_arranged_Normals[f3.a] = obj.normals[f3.c];
                }

                Face new_f = new Face(f1.a, f2.a, f3.a);

                new_faces.Add(new_f);
            }

            for (int x = 0; x < new_arranged_Normals.Length; x++)
                new_arranged_Normals[x] = Vector3.Normalize(new_arranged_Normals[x]);

            List<Vector3> obj_verts = obj.verts;
            ComputeTangentBasis(ref new_faces, ref obj_verts, ref new_arranged_UV, ref new_arranged_Normals, ref new_arranged_unknown20, ref new_arranged_unknown21);

            List<RenderAtom> new_Model_items2 = new List<RenderAtom>();

            foreach (RenderAtom modelitem in model_data_section.RenderAtoms)
            {
                RenderAtom new_model_item = new RenderAtom();
                new_model_item.BaseVertex = modelitem.BaseVertex;
                new_model_item.TriangleCount = (uint)new_faces.Count;
                new_model_item.BaseIndex = modelitem.BaseIndex;
                new_model_item.GeometrySliceLength = (uint)obj.verts.Count;
                new_model_item.MaterialId = modelitem.MaterialId;

                new_Model_items2.Add(new_model_item);
            }

            model_data_section.RenderAtoms = new_Model_items2;

            if (model_data_section.version != 6)
            {
                model_data_section.BoundsMin = new_Model_data_bounds_min;
                model_data_section.BoundsMax = new_Model_data_bounds_max;
                model_data_section.BoundingRadius = obj.verts.Select(i => i.Length()).Max();
            }

            geometry_section.vert_count = (uint)obj.verts.Count;
            geometry_section.verts = obj.verts;
            geometry_section.normals = new_arranged_Normals.ToList();
            geometry_section.UVs[0] = new_arranged_UV.ToList();
            geometry_section.binormals = new_arranged_unknown20.ToList();
            geometry_section.tangents = new_arranged_unknown21.ToList();

            topology_section.facelist = new_faces;
        }

        private static void ComputeTangentBasis(ref List<Face> faces, ref List<Vector3> verts, ref Vector2[] uvs, ref Vector3[] normals, ref Vector3[] tangents, ref Vector3[] binormals)
        {
            //Taken from various sources online. Search up Normal Vector Tangent calculation.

            List<ushort> parsed = new List<ushort>();

            foreach (Face f in faces)
            {
                float u02 = (uvs[f.c].X - uvs[f.a].X);
                float v02 = (uvs[f.c].Y - uvs[f.a].Y);
                float u01 = (uvs[f.b].X - uvs[f.a].X);
                float v01 = (uvs[f.b].Y - uvs[f.a].Y);
                float dot00 = u02 * u02 + v02 * v02;
                float dot01 = u02 * u01 + v02 * v01;
                float dot11 = u01 * u01 + v01 * v01;
                float d = dot00 * dot11 - dot01 * dot01;
                float u = 1.0f;
                float v = 1.0f;
                if (d != 0.0f)
                {
                    u = (dot11 * u02 - dot01 * u01) / d;
                    v = (dot00 * u01 - dot01 * u02) / d;
                }

                Vector3 tangent = verts[f.c] * u + verts[f.b] * v - verts[f.a] * (u + v);

                //vert1
                if (!parsed.Contains(f.a))
                {
                    binormals[f.a] = Vector3.Normalize(Vector3.Cross(tangent, normals[f.a]));
                    tangents[f.a] = Vector3.Normalize(Vector3.Cross(binormals[f.a], normals[f.a]));
                    parsed.Add(f.a);
                }

                //vert2
                if (!parsed.Contains(f.b))
                {
                    binormals[f.b] = Vector3.Normalize(Vector3.Cross(tangent, normals[f.b]));
                    tangents[f.b] = Vector3.Normalize(Vector3.Cross(binormals[f.b], normals[f.b]));
                    parsed.Add(f.b);
                }
                //vert3
                if (!parsed.Contains(f.c))
                {
                    binormals[f.c] = Vector3.Normalize(Vector3.Cross(tangent, normals[f.c]));
                    tangents[f.c] = Vector3.Normalize(Vector3.Cross(binormals[f.c], normals[f.c]));
                    parsed.Add(f.c);
                }

            }
        }

        public static bool ImportNewObjPatternUV(FullModelData fm, string filepath)
        {
            Log.Default.Info("Importing new obj with file for UV patterns: {0}", filepath);

            //Preload the .obj
            List<obj_data> objects = new List<obj_data>();

            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        obj_data obj = new obj_data();
                        bool reading_faces = false;
                        int prevMaxVerts = 0;
                        int prevMaxUvs = 0;
                        int prevMaxNorms = 0;


                        while ((line = sr.ReadLine()) != null)
                        {

                            //preloading objects
                            if (line.StartsWith("#"))
                                continue;
                            else if (line.StartsWith("o ") || line.StartsWith("g "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                obj.object_name = line.Substring(2);
                            }
                            else if (line.StartsWith("usemtl "))
                            {
                                obj.material_name = line.Substring(2);
                            }
                            else if (line.StartsWith("v "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] verts = line.Replace("  ", " ").Split(' ');
                                Vector3 vert = new Vector3();
                                vert.X = Convert.ToSingle(verts[1], CultureInfo.InvariantCulture);
                                vert.Y = Convert.ToSingle(verts[2], CultureInfo.InvariantCulture);
                                vert.Z = Convert.ToSingle(verts[3], CultureInfo.InvariantCulture);

                                obj.verts.Add(vert);
                            }
                            else if (line.StartsWith("vt "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] uvs = line.Split(' ');
                                Vector2 uv = new Vector2();
                                uv.X = Convert.ToSingle(uvs[1], CultureInfo.InvariantCulture);
                                uv.Y = Convert.ToSingle(uvs[2], CultureInfo.InvariantCulture);

                                obj.uv.Add(uv);
                            }
                            else if (line.StartsWith("vn "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] norms = line.Split(' ');
                                Vector3 norm = new Vector3();
                                norm.X = Convert.ToSingle(norms[1], CultureInfo.InvariantCulture);
                                norm.Y = Convert.ToSingle(norms[2], CultureInfo.InvariantCulture);
                                norm.Z = Convert.ToSingle(norms[3], CultureInfo.InvariantCulture);

                                obj.normals.Add(norm);
                            }
                            else if (line.StartsWith("f "))
                            {
                                reading_faces = true;
                                String[] faces = line.Substring(2).Split(' ');
                                for (int x = 0; x < 3; x++)
                                {
                                    ushort fa = 0, fb = 0, fc = 0;
                                    if (obj.verts.Count > 0)
                                        fa = (ushort)(Convert.ToUInt16(faces[x].Split('/')[0]) - prevMaxVerts - 1);
                                    if (obj.uv.Count > 0)
                                        fb = (ushort)(Convert.ToUInt16(faces[x].Split('/')[1]) - prevMaxUvs - 1);
                                    if (obj.normals.Count > 0)
                                        fc = (ushort)(Convert.ToUInt16(faces[x].Split('/')[2]) - prevMaxNorms - 1);
                                    if (fa < 0 || fb < 0 || fc < 0)
                                        throw new Exception("What the actual flapjack, something is *VERY* wrong");
                                    obj.faces.Add(new Face(fa, fb, fc));
                                }

                            }
                        }

                        if (!objects.Contains(obj))
                            objects.Add(obj);

                    }
                }



                //Read each object
                foreach (obj_data obj in objects)
                {

                    //Locate the proper model
                    uint modelSectionid = 0;
                    foreach (KeyValuePair<uint, ISection> pair in fm.parsed_sections)
                    {
                        if (modelSectionid != 0)
                            break;

                        if (pair.Value is Model)
                        {
                            UInt64 tryp;
                            if (UInt64.TryParse(obj.object_name, out tryp))
                            {
                                if (tryp == ((Model)pair.Value).HashName.Hash)
                                    modelSectionid = pair.Key;
                            }
                            else
                            {
                                if (Hash64.HashString(obj.object_name) == ((Model)pair.Value).HashName.Hash)
                                    modelSectionid = pair.Key;
                            }
                        }
                    }

                    //Apply new changes
                    if (modelSectionid == 0)
                        continue;

                    Model model_data_section = (Model)fm.parsed_sections[modelSectionid];
                    PassthroughGP passthrough_section = model_data_section.PassthroughGP;
                    Geometry geometry_section = passthrough_section.Geometry;
                    Topology topology_section = passthrough_section.Topology;

                    //Arrange UV and Normals
                    Vector2[] new_arranged_UV = new Vector2[geometry_section.verts.Count];
                    for (int x = 0; x < new_arranged_UV.Length; x++)
                        new_arranged_UV[x] = new Vector2(100f, 100f);
                    Vector2 sentinel = new Vector2(100f, 100f);

                    if (topology_section.facelist.Count != obj.faces.Count / 3)
                        return false;

                    for (int fcount = 0; fcount < topology_section.facelist.Count; fcount += 3)
                    {
                        Face f1 = obj.faces[fcount + 0];
                        Face f2 = obj.faces[fcount + 1];
                        Face f3 = obj.faces[fcount + 2];

                        //UV
                        if (obj.uv.Count > 0)
                        {
                            if (new_arranged_UV[topology_section.facelist[fcount / 3 + 0].a].Equals(sentinel))
                                new_arranged_UV[topology_section.facelist[fcount / 3 + 0].a] = obj.uv[f1.b];
                            if (new_arranged_UV[topology_section.facelist[fcount / 3 + 0].b].Equals(sentinel))
                                new_arranged_UV[topology_section.facelist[fcount / 3 + 0].b] = obj.uv[f2.b];
                            if (new_arranged_UV[topology_section.facelist[fcount / 3 + 0].c].Equals(sentinel))
                                new_arranged_UV[topology_section.facelist[fcount / 3 + 0].c] = obj.uv[f3.b];
                        }
                    }



                    geometry_section.UVs[1] = new_arranged_UV.ToList();

                    passthrough_section.Geometry.UVs[1] = new_arranged_UV.ToList();
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
                return false;
            }
            return true;
        }
    }
}
