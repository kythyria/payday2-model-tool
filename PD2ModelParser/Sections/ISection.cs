using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;

namespace PD2ModelParser.Sections
{
    public interface ISection
    {
        uint SectionId { get; set; }

        // The section type code - see Tags for these

        uint TypeCode { get; }

        /// <summary>
        /// Write the section, including header, to the given stream in Diesel format.
        /// </summary>
        /// <param name="output">Stream to write to</param>
        void StreamWrite(BinaryWriter output);
    }

    public interface IPostLoadable
    {
        void PostLoad(uint id, Dictionary<uint, ISection> parsed_sections);
    }

    public interface IHashContainer
    {
        void CollectHashes(CustomHashlist hashlist);
    }

    public abstract class AbstractSection : ISection
    {
        // These attributes are here because the auto lookup doesn't see them on interfaces.
        [Category("Section")]
        [DisplayName("ID")]
        [Description("File-internal identifier for this section.")]
        [ReadOnly(true)]
        public abstract uint SectionId { get; set; }

        [Category("Section")]
        [DisplayName("Type Code")]
        [Description("Integer that serialises which class this section is.")]
        [ReadOnly(true)]
        public abstract uint TypeCode { get; }

        public virtual void StreamWrite(BinaryWriter output)
        {
            output.Write(TypeCode);
            output.Write(SectionId);
            long size_pos = output.BaseStream.Position;
            output.Write(0); // gets overwritten
            long start_pos = output.BaseStream.Position;

            StreamWriteData(output);

            //update section size
            long end_pos = output.BaseStream.Position;
            output.BaseStream.Position = size_pos;
            output.Write((uint) (end_pos - start_pos));

            output.BaseStream.Position = end_pos;
        }

        public abstract void StreamWriteData(BinaryWriter output);

        public override string ToString()
        {
            return $"[{GetType().Name}] ID: {SectionId}";
        }
    }

    public static class SectionUtils
    {
        public static void CheckLength(long length, IList obj)
        {
            if (length != obj.Count)
                throw new Exception("Could not save model - bad list length, see stacktrace");
        }
    }
}
