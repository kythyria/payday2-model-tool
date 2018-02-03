
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
    static class ModelReader
    {

        public static FullModelData Open(string filepath, string rp = null)
        {
            FullModelData data = new FullModelData();

            StaticStorage.hashindex.Load();

            Console.WriteLine("Loading: " + filepath);

            Read(data, filepath, rp);

            if (rp != null)
            {
                UpdateRootPoint(data, rp);
            }

            return data;
        }

        private static void Read(FullModelData data, string filepath, string rp = null)
        {
            List<SectionHeader> sections = data.sections;
            Dictionary<UInt32, object> parsed_sections = data.parsed_sections;
            byte[] leftover_data = data.leftover_data;

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
                            // I'm not sure this tag section ever appears in the model itself - instead, always at the start of a skinbones section

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
        }

        private static bool UpdateRootPoint(FullModelData data, string rp)
        {
            ulong rp_hash = Hash64.HashString(rp);
            foreach (object section in data.parsed_sections.Values)
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
