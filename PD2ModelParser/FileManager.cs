//Original code by PoueT

using Nexus;
using PD2Bundle;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    /// <summary>
    ///     The static storage.
    /// </summary>
    public static class StaticStorage
    {
        #region Static Fields
        /// <summary>
        ///     The known index.
        /// </summary>
        public static KnownIndex hashindex = new KnownIndex();

        #endregion
    }

    public class FileManager
    {

        public List<SectionHeader> sections = new List<SectionHeader>();
        public Dictionary<UInt32, object> parsed_sections = new Dictionary<UInt32, object>();
        public byte[] leftover_data = null;

        public bool GenerateModelFromObj(String filepath)
        {
            Console.WriteLine("Importing new obj with file: " + filepath);

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

                                if (reading_faces && obj.faces.Count > 0)
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
                            else if (line.StartsWith("f "))
                            {
                                reading_faces = true;
                                String[] faces = line.Substring(2).Split(' ');
                                foreach (string f in faces)
                                {
                                    Face face = new Face();
                                    if (obj.verts.Count > 0)
                                        face.a = (ushort)(Convert.ToUInt16(f.Split('/')[0]) - prevMaxVerts - 1);
                                    if (obj.uv.Count > 0)
                                        face.b = (ushort)(Convert.ToUInt16(f.Split('/')[1]) - prevMaxUvs - 1);
                                    if (obj.normals.Count > 0)
                                        face.c = (ushort)(Convert.ToUInt16(f.Split('/')[2]) - prevMaxNorms - 1);
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
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
                return false;
            }
            return true;
        }


        public bool GenerateNewModel(String filename)
        {

            //you remove items from the parsed_sections
            //you edit items in the parsed_sections, they will get read and exported

            //Sort the sections
            List<Animation> animation_sections = new List<Animation>();
            List<Author> author_sections = new List<Author>();
            List<Material_Group> material_group_sections = new List<Material_Group>();
            List<Object3D> object3D_sections = new List<Object3D>();
            List<Model> model_sections = new List<Model>();


            foreach (SectionHeader sectionheader in sections)
            {
                if (!parsed_sections.Keys.Contains(sectionheader.id))
                    continue;
                object section = parsed_sections[sectionheader.id];

                if (section is Animation)
                {
                    animation_sections.Add(section as Animation);
                }
                else if (section is Author)
                {
                    author_sections.Add(section as Author);
                }
                else if (section is Material_Group)
                {
                    material_group_sections.Add(section as Material_Group);
                }
                else if (section is Object3D)
                {
                    object3D_sections.Add(section as Object3D);
                }
                else if (section is Model)
                {
                    model_sections.Add(section as Model);
                }

            }

            //after each section, you go back and enter it's new size
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {

                        bw.Write(-1); //the - (yyyy)
                        bw.Write((UInt32)100); //Filesize (GO BACK AT END AND CHANGE!!!)
                        int sectionCount = sections.Count;
                        bw.Write(sectionCount); //Sections count

                        foreach (Animation anim_sec in animation_sections)
                        {
                            anim_sec.StreamWrite(bw);
                        }

                        foreach (Author author_sec in author_sections)
                        {
                            author_sec.StreamWrite(bw);
                        }

                        foreach (Material_Group mat_group_sec in material_group_sections)
                        {
                            mat_group_sec.StreamWrite(bw);
                            foreach (uint id in mat_group_sec.items)
                            {
                                if (parsed_sections.Keys.Contains(id))
                                    (parsed_sections[id] as Material).StreamWrite(bw);
                            }
                        }

                        foreach (Object3D obj3d_sec in object3D_sections)
                        {
                            obj3d_sec.StreamWrite(bw);
                        }

                        foreach (Model model_sec in model_sections)
                        {
                            model_sec.StreamWrite(bw);
                        }


                        foreach (SectionHeader sectionheader in sections)
                        {
                            if (!parsed_sections.Keys.Contains(sectionheader.id))
                                continue;
                            object section = parsed_sections[sectionheader.id];

                            if (section is Unknown)
                            {
                                (section as Unknown).StreamWrite(bw);
                            }
                            else if (section is Animation ||
                                    section is Author ||
                                    section is Material_Group ||
                                    section is Material ||
                                    section is Object3D ||
                                    section is Model
                                )
                            {
                                continue;
                            }
                            else if (section is Geometry)
                            {
                                (section as Geometry).StreamWrite(bw);
                            }
                            else if (section is Topology)
                            {
                                (section as Topology).StreamWrite(bw);
                            }
                            else if (section is PassthroughGP)
                            {
                                (section as PassthroughGP).StreamWrite(bw);
                            }
                            else if (section is TopologyIP)
                            {
                                (section as TopologyIP).StreamWrite(bw);
                            }
                            else if (section is Bones)
                            {
                                (section as Bones).StreamWrite(bw);
                            }
                            else if (section is SkinBones)
                            {
                                (section as SkinBones).StreamWrite(bw);
                            }
                            else if (section is QuatLinearRotationController)
                            {
                                (section as QuatLinearRotationController).StreamWrite(bw);
                            }
                            else if (section is LinearVector3Controller)
                            {
                                (section as LinearVector3Controller).StreamWrite(bw);
                            }
                            else
                            {
                                Console.WriteLine("Tried to export an unknown section.");
                            }
                        }

                        if (leftover_data != null)
                            bw.Write(leftover_data);


                        fs.Position = 4;
                        bw.Write((UInt32)fs.Length);

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

        
    }
}
