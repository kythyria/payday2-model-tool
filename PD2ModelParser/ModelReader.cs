
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

        public static FullModelData Open(string filepath)
        {
            FullModelData data = new FullModelData();

            StaticStorage.hashindex.Load();

            Log.Default.Info("Opening Model: {0}", filepath);

            Read(data, filepath);

            return data;
        }

        public delegate void SectionVisitor(BinaryReader file, SectionHeader section);

        /// <summary>
        /// Iterate over each part of the model file, and letting the caller handle them.
        ///
        /// This allows much faster reading of a file if you're only interested in one
        /// specific part of it.
        /// </summary>
        /// <param name="filepath">The name of the file to open</param>
        public static void VisitModel(string filepath, SectionVisitor visitor)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    List<SectionHeader> headers = ReadHeaders(br);

                    foreach (SectionHeader header in headers)
                    {
                        fs.Position = header.Start;
                        visitor(br, header);
                    }
                }
            }
        }

        /// <summary>
        /// Reads all the section headers from a model file.
        ///
        /// Note this leaves the file's position just after the end of the last section.
        /// </summary>
        /// <param name="br">The input source</param>
        /// <returns>The list of section headers</returns>
        public static List<SectionHeader> ReadHeaders(BinaryReader br)
        {
            int random = br.ReadInt32();
            int filesize = br.ReadInt32();
            int sectionCount;
            if (random == -1)
            {
                sectionCount = br.ReadInt32();
            }
            else
                sectionCount = random;

            Log.Default.Debug("Size: {0} bytes, Sections: {1},{2}", filesize, sectionCount, br.BaseStream.Position);

            List<SectionHeader> sections = new List<SectionHeader>();

            for (int x = 0; x < sectionCount; x++)
            {
                SectionHeader sectionHead = new SectionHeader(br);
                sections.Add(sectionHead);
                Log.Default.Debug("Section: {0}", sectionHead);

                Log.Default.Debug("Next offset: {0}", sectionHead.End);
                br.BaseStream.Position = sectionHead.End;
            }

            return sections;
        }

        private static void Read(FullModelData data, string filepath)
        {
            List<SectionHeader> sections = data.sections;
            Dictionary<UInt32, ISection> parsed_sections = data.parsed_sections;

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    sections.Clear();
                    sections.AddRange(ReadHeaders(br));

                    foreach (SectionHeader sh in sections)
                    {
                        ISection section;

                        fs.Position = sh.Start;

                        if (sh.type == animation_data_tag)
                        {
                            section = new Animation(br, sh);
                        }
                        else if (sh.type == author_tag)
                        {
                            section = new Author(br, sh);
                        }
                        else if (sh.type == material_group_tag)
                        {
                            section = new Material_Group(br, sh);
                        }
                        else if (sh.type == material_tag)
                        {
                            section = new Material(br, sh);
                        }
                        else if (sh.type == object3D_tag)
                        {
                            section = new Object3D(br, sh);
                        }
                        else if (sh.type == geometry_tag)
                        {
                            section = new Geometry(br, sh);
                        }
                        else if (sh.type == model_data_tag)
                        {
                            section = new Model(br, sh);
                        }
                        else if (sh.type == topology_tag)
                        {
                            section = new Topology(br, sh);
                        }
                        else if (sh.type == passthroughGP_tag)
                        {
                            section = new PassthroughGP(br, sh);
                        }
                        else if (sh.type == topologyIP_tag)
                        {
                            section = new TopologyIP(br, sh);
                        }
                        else if (sh.type == bones_tag)
                        {
                            // I'm not sure this tag section ever appears in the model itself - instead, always at the start of a skinbones section

                            section = new Bones(br, sh);
                        }
                        else if (sh.type == skinbones_tag)
                        {
                            section = new SkinBones(br, sh);
                        }
                        else if (sh.type == quatLinearRotationController_tag)
                        {
                            section = new QuatLinearRotationController(br, sh);
                        }
                        else if (sh.type == linearVector3Controller_tag)
                        {
                            section = new LinearVector3Controller(br, sh);
                        }
                        else if (sh.type == custom_hashlist_tag)
                        {
                            section = new CustomHashlist(br, sh);
                        }
                        else
                        {
                            Log.Default.Warn("UNKNOWN Tag at {0} Size: {1}", sh.offset, sh.size);
                            fs.Position = sh.offset;

                            section = new Unknown(br, sh);
                        }

                        Log.Default.Debug("Section {0} at {1} length {2}",
                            section.GetType().Name, sh.offset, sh.size);

                        parsed_sections.Add(sh.id, section);
                    }

                    foreach(uint id in parsed_sections.Keys)
                    {
                        if (parsed_sections[id] is IPostLoadable)
                        {
                            IPostLoadable obj = (IPostLoadable) parsed_sections[id];
                            obj.PostLoad(id, parsed_sections);
                        }
                    }

                    if (fs.Position < fs.Length)
                        data.leftover_data = br.ReadBytes((int)(fs.Length - fs.Position));

                    br.Close();
                }
                fs.Close();
            }
        }
    }
}
