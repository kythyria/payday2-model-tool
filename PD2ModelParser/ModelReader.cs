
using System;
using System.Collections.Generic;
using System.IO;

using PD2ModelParser.Sections;

namespace PD2ModelParser
{
    static partial class ModelReader
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
                        fs.Position = sh.Start;
                        var section = ReadSection(br, sh);
                        parsed_sections.Add(sh.id, section);
                    }

                    foreach (var i in parsed_sections)
                    {
                        if(i.Value is IPostLoadable pl)
                        {
                            pl.PostLoad(i.Key, parsed_sections);
                        }
                    }

                    if (fs.Position < fs.Length)
                        data.leftover_data = br.ReadBytes((int)(fs.Length - fs.Position));
                }
            }
        }
    }
}
