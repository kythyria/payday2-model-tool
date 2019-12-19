using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PD2ModelParser.Sections
{
    public interface ISection
    {
        uint SectionId { get; set; }

        // The section type code - see Tags for these
        uint TypeCode { get; }
    }

    public interface IPostLoadable
    {
        void PostLoad(uint id, Dictionary<uint, object> parsed_sections);
    }

    public interface IHashContainer
    {
        void CollectHashes(CustomHashlist hashlist);
    }

    public abstract class AbstractSection : ISection
    {
        public abstract uint SectionId { get; set; }
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
