using Nexus;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    class NewObjImporter
    {

        public static bool ImportNewObj(FileManager fm, String filepath, bool addNew = false)
        {
            Console.WriteLine("Importing new obj with file: " + filepath);

            //Preload the .obj
            List<obj_data> objects = new List<obj_data>();
            List<obj_data> toAddObjects = new List<obj_data>();

            ref List<SectionHeader> sections = ref fm.sections;
            ref Dictionary<UInt32, object> parsed_sections = ref fm.parsed_sections;

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
                                    Console.WriteLine("Object " + (objects.Count + 1) + " named: " + obj.object_name);
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
                                Vector3D vert = new Vector3D();
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
                                Vector2D uv = new Vector2D();
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
                                Vector3D norm = new Vector3D();
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
                                    Face face = new Face();
                                    if (obj.verts.Count > 0)
                                        face.a = (ushort)(Convert.ToUInt16(faces[x].Split('/')[0]) - prevMaxVerts - 1);
                                    if (obj.uv.Count > 0)
                                        face.b = (ushort)(Convert.ToUInt16(faces[x].Split('/')[1]) - prevMaxUvs - 1);
                                    if (obj.normals.Count > 0)
                                        face.c = (ushort)(Convert.ToUInt16(faces[x].Split('/')[2]) - prevMaxNorms - 1);
                                    if (face.a < 0 || face.b < 0 || face.c < 0)
                                        throw new Exception();
                                    obj.faces.Add(face);
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
                    uint modelSectionid = 0;
                    foreach (KeyValuePair<uint, object> pair in parsed_sections)
                    {
                        if (modelSectionid != 0)
                            break;

                        if (pair.Value is Model)
                        {
                            UInt64 tryp;
                            if (UInt64.TryParse(obj.object_name, out tryp))
                            {
                                if (tryp == ((Model)pair.Value).object3D.hashname)
                                    modelSectionid = pair.Key;
                            }
                            else
                            {
                                if (Hash64.HashString(obj.object_name) == ((Model)pair.Value).object3D.hashname)
                                    modelSectionid = pair.Key;
                            }
                        }
                    }

                    //Apply new changes
                    if (modelSectionid == 0)
                    {
                        toAddObjects.Add(obj);
                        continue;
                    }

                    Model model_data_section = (Model)parsed_sections[modelSectionid];
                    PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[model_data_section.passthroughGP_ID];
                    Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                    Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];

                    uint geometry_size = geometry_section.size - (uint)((geometry_section.verts.Count * 12) + (geometry_section.uvs.Count * 8) + (geometry_section.normals.Count * 12));
                    uint facelist_size = topology_section.size - (uint)(topology_section.facelist.Count * 6);

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

                        Face new_face = new Face();
                        if (replacedF > -1)
                        {
                            new_face.a = obj.faces[replacedF].a;
                            new_face.b = obj.faces[replacedF].b;
                            new_face.c = obj.faces[dupe].c;

                        }
                        else
                        {
                            new_face.a = (ushort)obj.verts.Count;
                            new_face.b = obj.faces[dupe].b;
                            new_face.c = obj.faces[dupe].c;
                            obj.verts.Add(obj.verts[obj.faces[dupe].a]);

                            done_faces.Add(dupe, obj.faces[dupe]);
                        }

                        obj.faces[dupe] = new_face;
                    }

                    Vector3D new_Model_data_bounds_min = new Vector3D();// Z (max), X (low), Y (low)
                    Vector3D new_Model_data_bounds_max = new Vector3D();// Z (low), X (max), Y (max)

                    foreach (Vector3D vert in obj.verts)
                    {
                        //Z
                        if (new_Model_data_bounds_min.Z == null)
                        {
                            new_Model_data_bounds_min.Z = vert.Z;
                        }
                        else
                        {
                            if (vert.Z > new_Model_data_bounds_min.Z)
                                new_Model_data_bounds_min.Z = vert.Z;
                        }

                        if (new_Model_data_bounds_max.Z == null)
                        {
                            new_Model_data_bounds_max.Z = vert.Z;
                        }
                        else
                        {
                            if (vert.Z < new_Model_data_bounds_max.Z)
                                new_Model_data_bounds_max.Z = vert.Z;
                        }

                        //X
                        if (new_Model_data_bounds_min.X == null)
                        {
                            new_Model_data_bounds_min.X = vert.X;
                        }
                        else
                        {
                            if (vert.X < new_Model_data_bounds_min.X)
                                new_Model_data_bounds_min.X = vert.X;
                        }

                        if (new_Model_data_bounds_max.X == null)
                        {
                            new_Model_data_bounds_max.X = vert.X;
                        }
                        else
                        {
                            if (vert.X > new_Model_data_bounds_max.X)
                                new_Model_data_bounds_max.X = vert.X;
                        }

                        //Y
                        if (new_Model_data_bounds_min.Y == null)
                        {
                            new_Model_data_bounds_min.Y = vert.Y;
                        }
                        else
                        {
                            if (vert.Y < new_Model_data_bounds_min.Y)
                                new_Model_data_bounds_min[2] = vert.Y;
                        }

                        if (new_Model_data_bounds_max.Y == null)
                        {
                            new_Model_data_bounds_max.Y = vert.Y;
                        }
                        else
                        {
                            if (vert.Y > new_Model_data_bounds_max.Y)
                                new_Model_data_bounds_max.Y = vert.Y;
                        }
                    }

                    //Arrange UV and Normals
                    List<Vector2D> new_arranged_Geometry_UVs = new List<Vector2D>();
                    List<Vector3D> new_arranged_Geometry_normals = new List<Vector3D>();
                    List<Vector3D> new_arranged_Geometry_unknown20 = new List<Vector3D>();
                    List<Vector3D> new_arranged_Geometry_unknown21 = new List<Vector3D>();
                    List<int> added_uvs = new List<int>();
                    List<int> added_normals = new List<int>();

                    Vector2D[] new_arranged_UV = new Vector2D[obj.verts.Count];
                    for (int x = 0; x < new_arranged_UV.Length; x++)
                        new_arranged_UV[x] = new Vector2D(100f, 100f);
                    Vector2D sentinel = new Vector2D(100f, 100f);
                    Vector3D[] new_arranged_Normals = new Vector3D[obj.verts.Count];
                    Vector3D[] new_arranged_unknown20 = new Vector3D[obj.verts.Count];
                    Vector3D[] new_arranged_unknown21 = new Vector3D[obj.verts.Count];

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
                        Face new_f = new Face();
                        new_f.a = f1.a;
                        new_f.b = f2.a;
                        new_f.c = f3.a;

                        new_faces.Add(new_f);
                    }

                    List<Vector3D> obj_verts = obj.verts;
                    ComputeTangentBasis(ref new_faces, ref obj_verts, ref new_arranged_UV, ref new_arranged_Normals, ref new_arranged_unknown20, ref new_arranged_unknown21);

                    UInt32[] size_index = { 0, 4, 8, 12, 16, 4, 4, 8, 12 };
                    UInt32 calc_size = 0;
                    foreach (GeometryHeader head in geometry_section.headers)
                    {
                        calc_size += size_index[(int)head.item_size];
                    }

                    List<ModelItem> new_Model_items2 = new List<ModelItem>();

                    foreach (ModelItem modelitem in model_data_section.items)
                    {
                        ModelItem new_model_item = new ModelItem();
                        new_model_item.unknown1 = modelitem.unknown1;
                        new_model_item.vertCount = (uint)new_faces.Count;
                        new_model_item.unknown2 = modelitem.unknown2;
                        new_model_item.faceCount = (uint)obj.verts.Count;
                        new_model_item.material_id = modelitem.material_id;

                        new_Model_items2.Add(new_model_item);
                    }

                    model_data_section.items = new_Model_items2;
                    geometry_section.vert_count = (uint)obj.verts.Count;
                    geometry_section.verts = obj.verts;
                    topology_section.facelist = new_faces;
                    geometry_section.uvs = new_arranged_UV.ToList();
                    geometry_section.normals = new_arranged_Normals.ToList();

                    if (model_data_section.version != 6)
                    {
                        model_data_section.bounds_min = new_Model_data_bounds_min;
                        model_data_section.bounds_max = new_Model_data_bounds_max;
                    }

                    UInt32 geometry_calulated_size = calc_size * (UInt32)obj.verts.Count + (8 + ((UInt32)geometry_section.headers.Count * 8)) + (UInt32)geometry_section.unknown_item_data.Count;
                    if (geometry_section.remaining_data != null)
                        geometry_calulated_size += (UInt32)geometry_section.remaining_data.Length;

                    geometry_section.size = geometry_calulated_size;
                    topology_section.size = facelist_size + (uint)(new_faces.Count * 6);
                    geometry_section.size = calc_size * (UInt32)obj.verts.Count;

                    geometry_section.vert_count = (uint)obj.verts.Count;
                    geometry_section.verts = obj.verts;
                    geometry_section.normals = new_arranged_Normals.ToList();
                    geometry_section.uvs = new_arranged_UV.ToList();
                    geometry_section.unknown20 = new_arranged_unknown20.ToList();
                    geometry_section.unknown21 = new_arranged_unknown21.ToList();

                    topology_section.count1 = (uint)(new_faces.Count * 3);
                    topology_section.facelist = new_faces;

                }


                //Add new objects
                if (addNew)
                {
                    foreach (obj_data obj in toAddObjects)
                    {
                        //create new Model
                        Material newMat = new Material((uint)(obj.object_name + ".material").GetHashCode(), obj.material_name);
                        Material_Group newMatG = new Material_Group((uint)(obj.object_name + ".materialGroup").GetHashCode(), newMat.id);
                        Geometry newGeom = new Geometry((uint)(obj.object_name + ".geom").GetHashCode(), obj);
                        Topology newTopo = new Topology((uint)(obj.object_name + ".topo").GetHashCode(), obj);

                        PassthroughGP newPassGP = new PassthroughGP((uint)(obj.object_name + ".passGP").GetHashCode(), newGeom.id, newTopo.id);
                        TopologyIP newTopoIP = new TopologyIP((uint)(obj.object_name + ".topoIP").GetHashCode(), newTopo.id);

                        Model newModel = new Model(obj, newPassGP.id, newTopoIP.id, newMatG.id);

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

                            Face new_face = new Face();
                            if (replacedF > -1)
                            {
                                new_face.a = obj.faces[replacedF].a;
                                new_face.b = obj.faces[replacedF].b;
                                new_face.c = obj.faces[dupe].c;

                            }
                            else
                            {
                                new_face.a = (ushort)obj.verts.Count;
                                new_face.b = obj.faces[dupe].b;
                                new_face.c = obj.faces[dupe].c;
                                obj.verts.Add(obj.verts[obj.faces[dupe].a]);

                                done_faces.Add(dupe, obj.faces[dupe]);
                            }

                            obj.faces[dupe] = new_face;
                        }

                        Vector3D new_Model_data_bounds_min = new Vector3D();// Z (max), X (low), Y (low)
                        Vector3D new_Model_data_bounds_max = new Vector3D();// Z (low), X (max), Y (max)

                        foreach (Vector3D vert in obj.verts)
                        {
                            //Z
                            if (new_Model_data_bounds_min.Z == null)
                            {
                                new_Model_data_bounds_min.Z = vert.Z;
                            }
                            else
                            {
                                if (vert.Z > new_Model_data_bounds_min.Z)
                                    new_Model_data_bounds_min.Z = vert.Z;
                            }

                            if (new_Model_data_bounds_max.Z == null)
                            {
                                new_Model_data_bounds_max.Z = vert.Z;
                            }
                            else
                            {
                                if (vert.Z < new_Model_data_bounds_max.Z)
                                    new_Model_data_bounds_max.Z = vert.Z;
                            }

                            //X
                            if (new_Model_data_bounds_min.X == null)
                            {
                                new_Model_data_bounds_min.X = vert.X;
                            }
                            else
                            {
                                if (vert.X < new_Model_data_bounds_min.X)
                                    new_Model_data_bounds_min.X = vert.X;
                            }

                            if (new_Model_data_bounds_max.X == null)
                            {
                                new_Model_data_bounds_max.X = vert.X;
                            }
                            else
                            {
                                if (vert.X > new_Model_data_bounds_max.X)
                                    new_Model_data_bounds_max.X = vert.X;
                            }

                            //Y
                            if (new_Model_data_bounds_min.Y == null)
                            {
                                new_Model_data_bounds_min.Y = vert.Y;
                            }
                            else
                            {
                                if (vert.Y < new_Model_data_bounds_min.Y)
                                    new_Model_data_bounds_min[2] = vert.Y;
                            }

                            if (new_Model_data_bounds_max.Y == null)
                            {
                                new_Model_data_bounds_max.Y = vert.Y;
                            }
                            else
                            {
                                if (vert.Y > new_Model_data_bounds_max.Y)
                                    new_Model_data_bounds_max.Y = vert.Y;
                            }
                        }

                        //Arrange UV and Normals
                        List<Vector2D> new_arranged_Geometry_UVs = new List<Vector2D>();
                        List<Vector3D> new_arranged_Geometry_normals = new List<Vector3D>();
                        List<Vector3D> new_arranged_Geometry_unknown20 = new List<Vector3D>();
                        List<Vector3D> new_arranged_Geometry_unknown21 = new List<Vector3D>();
                        List<int> added_uvs = new List<int>();
                        List<int> added_normals = new List<int>();

                        Vector2D[] new_arranged_UV = new Vector2D[obj.verts.Count];
                        for (int x = 0; x < new_arranged_UV.Length; x++)
                            new_arranged_UV[x] = new Vector2D(100f, 100f);
                        Vector2D sentinel = new Vector2D(100f, 100f);
                        Vector3D[] new_arranged_Normals = new Vector3D[obj.verts.Count];
                        for (int x = 0; x < new_arranged_Normals.Length; x++)
                            new_arranged_Normals[x] = new Vector3D(0f, 0f, 0f);
                        Vector3D[] new_arranged_unknown20 = new Vector3D[obj.verts.Count];
                        Vector3D[] new_arranged_unknown21 = new Vector3D[obj.verts.Count];

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
                                new_arranged_Normals[f1.a] += obj.normals[f1.c];
                                new_arranged_Normals[f2.a] += obj.normals[f2.c];
                                new_arranged_Normals[f3.a] += obj.normals[f3.c];
                            }
                            Face new_f = new Face();
                            new_f.a = f1.a;
                            new_f.b = f2.a;
                            new_f.c = f3.a;

                            new_faces.Add(new_f);
                        }

                        for (int x = 0; x < new_arranged_Normals.Length; x++)
                            new_arranged_Normals[x].Normalize();

                        List<Vector3D> obj_verts = obj.verts;
                        ComputeTangentBasis(ref new_faces, ref obj_verts, ref new_arranged_UV, ref new_arranged_Normals, ref new_arranged_unknown20, ref new_arranged_unknown21);

                        UInt32[] size_index = { 0, 4, 8, 12, 16, 4, 4, 8, 12 };
                        UInt32 calc_size = 0;
                        foreach (GeometryHeader head in newGeom.headers)
                        {
                            calc_size += size_index[(int)head.item_size];
                        }

                        List<ModelItem> new_Model_items2 = new List<ModelItem>();

                        foreach (ModelItem modelitem in newModel.items)
                        {
                            ModelItem new_model_item = new ModelItem();
                            new_model_item.unknown1 = modelitem.unknown1;
                            new_model_item.vertCount = (uint)new_faces.Count;
                            new_model_item.unknown2 = modelitem.unknown2;
                            new_model_item.faceCount = (uint)obj.verts.Count;
                            new_model_item.material_id = modelitem.material_id;

                            new_Model_items2.Add(new_model_item);
                        }

                        newModel.items = new_Model_items2;
                        newGeom.vert_count = (uint)obj.verts.Count;
                        newGeom.verts = obj.verts;
                        newTopo.facelist = new_faces;
                        newGeom.uvs = new_arranged_UV.ToList();
                        newGeom.normals = new_arranged_Normals.ToList();

                        if (newModel.version != 6)
                        {
                            newModel.bounds_min = new_Model_data_bounds_min;
                            newModel.bounds_max = new_Model_data_bounds_max;
                        }

                        newGeom.vert_count = (uint)obj.verts.Count;
                        newGeom.verts = obj.verts;
                        newGeom.normals = new_arranged_Normals.ToList();
                        newGeom.uvs = new_arranged_UV.ToList();
                        newGeom.unknown20 = new_arranged_unknown20.ToList();
                        newGeom.unknown21 = new_arranged_unknown21.ToList();

                        newTopo.count1 = (uint)(new_faces.Count * 3);
                        newTopo.facelist = new_faces;


                        //Add new sections
                        parsed_sections.Add(newMat.id, newMat);
                        sections.Add(new SectionHeader(newMat.id));
                        parsed_sections.Add(newMatG.id, newMatG);
                        sections.Add(new SectionHeader(newMatG.id));
                        parsed_sections.Add(newGeom.id, newGeom);
                        sections.Add(new SectionHeader(newGeom.id));
                        parsed_sections.Add(newTopo.id, newTopo);
                        sections.Add(new SectionHeader(newTopo.id));
                        parsed_sections.Add(newPassGP.id, newPassGP);
                        sections.Add(new SectionHeader(newPassGP.id));
                        parsed_sections.Add(newTopoIP.id, newTopoIP);
                        sections.Add(new SectionHeader(newTopoIP.id));
                        parsed_sections.Add(newModel.id, newModel);
                        sections.Add(new SectionHeader(newModel.id));
                    }
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
                return false;
            }
            return true;
        }

        private static void ComputeTangentBasis(ref List<Face> faces, ref List<Vector3D> verts, ref Vector2D[] uvs, ref Vector3D[] normals, ref Vector3D[] tangents, ref Vector3D[] binormals)
        {
            //Taken from various sources online. Search up Normal Vector Tangent calculation.

            List<ushort> parsed = new List<ushort>();

            foreach (Face f in faces)
            {
                Vector3D P0 = verts[f.a];
                Vector3D P1 = verts[f.b];
                Vector3D P2 = verts[f.c];

                Vector2D UV0 = uvs[f.a];
                Vector2D UV1 = uvs[f.b];
                Vector2D UV2 = uvs[f.c];


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

                Vector3D tangent = verts[f.c] * u + verts[f.b] * v - verts[f.a] * (u + v);

                //vert1
                if (!parsed.Contains(f.a))
                {
                    binormals[f.a] = Vector3D.Cross(tangent, normals[f.a]);
                    binormals[f.a].Normalize();
                    tangents[f.a] = Vector3D.Cross(binormals[f.a], normals[f.a]);
                    tangents[f.a].Normalize();
                    parsed.Add(f.a);
                }

                //vert2
                if (!parsed.Contains(f.b))
                {
                    binormals[f.b] = Vector3D.Cross(tangent, normals[f.b]);
                    binormals[f.b].Normalize();
                    tangents[f.b] = Vector3D.Cross(binormals[f.b], normals[f.b]);
                    tangents[f.b].Normalize();
                    parsed.Add(f.b);
                }
                //vert3
                if (!parsed.Contains(f.c))
                {
                    binormals[f.c] = Vector3D.Cross(tangent, normals[f.c]);
                    binormals[f.c].Normalize();
                    tangents[f.c] = Vector3D.Cross(binormals[f.c], normals[f.c]);
                    tangents[f.c].Normalize();
                    parsed.Add(f.c);
                }

            }
        }
    }
}
