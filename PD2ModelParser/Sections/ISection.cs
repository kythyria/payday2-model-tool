using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Linq;

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
        public virtual uint SectionId { get; set; }

        [Category("Section")]
        [DisplayName("Type Code")]
        [Description("Integer that serialises which class this section is.")]
        [ReadOnly(true)]
        public virtual uint TypeCode => SectionMetaInfo.For(GetType()).Tag;

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

    public class SectionMetaInfo
    {
        private static Dictionary<uint, SectionMetaInfo> byTag;
        private static Dictionary<Type, SectionMetaInfo> byType;

        static SectionMetaInfo()
        {
            var types = System.Reflection.Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(i => i.CustomAttributes.Any(j => j.AttributeType == typeof(SectionIdAttribute)))
                .Select(i => new SectionMetaInfo(i))
                .ToList();
            byTag = types.ToDictionary(i => i.Tag);
            byType = types.ToDictionary(i => i.Type);
        }

        public static bool TryGetForTag(uint tag, out SectionMetaInfo result)
        {
            return byTag.TryGetValue(tag, out result);
        }

        public static SectionMetaInfo For<T>() => byType[typeof(T)];
        public static SectionMetaInfo For(Type t) => byType[t];

        private System.Reflection.ConstructorInfo deserialiseConstructor;

        public Type Type { get; private set; }
        public uint Tag { get; private set; }
        public ISection Deserialise(BinaryReader br, SectionHeader sh) {
            return (ISection)deserialiseConstructor.Invoke(new object[] { br, sh });
        }

        SectionMetaInfo(Type t)
        {
            this.Type = t;

            var idAttr = t.GetCustomAttributes(typeof(SectionIdAttribute), false)[0] as SectionIdAttribute;
            this.Tag = idAttr.Tag;

            this.deserialiseConstructor = t.GetConstructor(new Type[] { typeof(BinaryReader), typeof(SectionHeader) });
        }
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class SectionIdAttribute : Attribute
    {
        public SectionIdAttribute(uint tag)
        {
            Tag = tag;
        }

        public uint Tag { get; private set;  }
    }
}
