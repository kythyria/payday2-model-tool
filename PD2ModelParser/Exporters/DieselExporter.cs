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
                                Log.Default.Warn("Tried to export an unknown section {0}",
                                    section.GetType().FullName);
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
