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
        public readonly FullModelData data;

        public FileManager()
        {
            data = new FullModelData();
        }

        public FileManager(FullModelData data)
        {
            this.data = data;
        }

        public bool GenerateModelFromObj(String filepath)
        {
            Log.Default.Info("Importing new obj with file: {0}", filepath);

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

    }
}
