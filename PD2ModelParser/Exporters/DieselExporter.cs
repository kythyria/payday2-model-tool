using Nexus;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    class DieselExporter
    {
        public static void ExportFile(FullModelData data, string path)
        {
            path = path.Replace(".model", "shot.model");

            //you remove items from the parsed_sections
            //you edit items in the parsed_sections, they will get read and exported

            //Sort the sections
            List<Animation> animation_sections = new List<Animation>();
            List<Author> author_sections = new List<Author>();
            List<Material_Group> material_group_sections = new List<Material_Group>();
            List<Object3D> object3D_sections = new List<Object3D>();
            List<Model> model_sections = new List<Model>();


            foreach (SectionHeader sectionheader in data.sections)
            {
                if (!data.parsed_sections.Keys.Contains(sectionheader.id))
                    continue;
                object section = data.parsed_sections[sectionheader.id];

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
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {

                        bw.Write(-1); //the - (yyyy)
                        bw.Write((UInt32)100); //Filesize (GO BACK AT END AND CHANGE!!!)
                        int sectionCount = data.sections.Count;
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
                                if (data.parsed_sections.Keys.Contains(id))
                                    (data.parsed_sections[id] as Material).StreamWrite(bw);
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


                        foreach (SectionHeader sectionheader in data.sections)
                        {
                            if (!data.parsed_sections.Keys.Contains(sectionheader.id))
                                continue;
                            object section = data.parsed_sections[sectionheader.id];

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
                                SkinBones sb = section as SkinBones;

                                //for(int i=0; i<sb.rotations.Count; i++)
                                //{
                                //    Matrix3D rot = new Matrix3D();
                                //    rot.M11 = 1;
                                //    rot.M22 = 1;
                                //    rot.M33 = 1;
                                //    rot.M44 = 1;

                                //    rot.Translation = new Vector3D
                                //    {
                                //        X = 0,
                                //        Y = 100,
                                //        Z = 0
                                //    };

                                //    sb.rotations[i] = rot;
                                //}

                                uint iid = 0;

                                Dictionary<string, Point3D> poses = new Dictionary<string, Point3D>();

                                for (int i = 0; i < sb.rotations.Count; i++)
                                {
                                    uint id = sb.objects[i];
                                    Object3D obj = (Object3D)data.parsed_sections[id];
                                    string name = StaticStorage.hashindex.GetString(obj.hashname);

                                    if (name == "Head") iid = id;


                                    if (name == "LeftShoulder")
                                    {
                                        Matrix3D m = sb.rotations[i];
                                        Console.WriteLine(m.Translation);
                                        Vector3D pos = m.Translation;
                                        m = Matrix3D.Identity;
                                        m.Translation = pos;
                                        //sb.rotations[i] = m; //= Matrix3D.Identity;
                                    }

                                    Matrix3D mat = sb.rotations[i];
                                    Point3D p = new Point3D(mat.Translation.X, mat.Translation.Y, mat.Translation.Z);
                                    mat.Translation = Vector3D.Zero;
                                    poses[name] = mat.Transform(p);
                                }

                                Console.WriteLine("Start!!");
                                foreach (var pair in poses.AsEnumerable().OrderBy(p => p.Value.Y))
                                {
                                    Console.WriteLine(pair.Key + ": " + pair.Value);
                                }
                                Console.WriteLine("End!!");

                                Matrix3D compoundTransform = Matrix3D.Identity;

                                while(iid != 0)
                                {
                                    Object3D obj = (Object3D)data.parsed_sections[iid];
                                    string name = StaticStorage.hashindex.GetString(obj.hashname);

                                    for (int i = 0; i < sb.rotations.Count; i++)
                                    {
                                        if (sb.objects[i] == iid)
                                        {
                                            Matrix3D rot = sb.rotations[i];

                                            if (rot.HasScale)
                                                throw new Exception("Cannot handle scaled bones!");

                                            Vector3D translation = rot.Translation;
                                            rot.Translation = Vector3D.Zero;

                                            rot.Translation = rot.Transform(translation);

                                            rot.Invert();
                                            //Console.WriteLine(rot);

                                            //compoundTransform = rot * compoundTransform;
                                            compoundTransform *= rot;

                                            goto found;
                                        }
                                    }

                                    if(obj.parentID != 0) // Don't require the root point
                                    {
                                        //Console.WriteLine("Missing " + name);
                                        goto no_transform_path;
                                    }

                                    found:

                                    iid = obj.parentID;
                                    //Console.WriteLine(name + ": " + iid);
                                }

                                Point3D point = new Point3D(0, 0, 0);
                                point = compoundTransform.Transform(point);
                                Console.WriteLine(point);

                                no_transform_path:

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

                        if (data.leftover_data != null)
                            bw.Write(data.leftover_data);


                        fs.Position = 4;
                        bw.Write((UInt32)fs.Length);

                    }
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
            }
        }
    }
}
