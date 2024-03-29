﻿
using System;
using System.Collections.Generic;
using System.IO;

using PD2ModelParser.Sections;

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

            byte[] bytes;

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fs.Length];
                int res = fs.Read(bytes, 0, (int)fs.Length);
                if (res != fs.Length)
                    throw new Exception($"Failed to read {filepath} all in one go!");
            }

            using (var ms = new MemoryStream(bytes, 0, bytes.Length, false, true))
            using (var br = new BinaryReader(ms))
            {
                sections.Clear();
                sections.AddRange(ReadHeaders(br));

                foreach (SectionHeader sh in sections)
                {
                    ISection section;

                    ms.Position = sh.Start;

                    if (SectionMetaInfo.TryGetForTag(sh.type, out var mi))
                    {
                        section = mi.Deserialise(br, sh);
                    }
                    else
                    {
                        Log.Default.Warn("UNKNOWN Tag {2} at {0} Size: {1}", sh.offset, sh.size, sh.type);
                        ms.Position = sh.offset;

                        section = new Unknown(br, sh);
                    }

                    if (ms.Position != sh.End)
                    {
                        //throw new Exception(string.Format("Section of type {2} {0} read more than its length of {1} ", sh.id, sh.size, sh.type));
                        Log.Default.Warn("Section {0} (type {2:X}) was too short ({1} bytes read)", sh.id, sh.size, sh.type);
                    }

                    Log.Default.Debug("Section {0} at {1} length {2}",
                        section.GetType().Name, sh.offset, sh.size);

                    parsed_sections.Add(sh.id, section);
                }

                foreach (var i in parsed_sections)
                {
                    if (i.Value is IPostLoadable pl)
                    {
                        pl.PostLoad(i.Key, parsed_sections);
                    }
                }

                if (ms.Position < ms.Length)
                    data.leftover_data = br.ReadBytes((int)(ms.Length - ms.Position));
            }
        }
    }
}
