using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

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
        public virtual uint TypeCode => SectionMetaInfo.TagFor(this);

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
            output.Write((uint)(end_pos - start_pos));

            output.BaseStream.Position = end_pos;
        }

        public abstract void StreamWriteData(BinaryWriter output);

        public override string ToString()
        {
            return $"[{GetType().Name}] ID: {SectionId}";
        }

        delegate void PostLoadCallback(ISection self, Dictionary<uint, ISection> sections);
        List<PostLoadCallback> postloadCallbacks = new List<PostLoadCallback>();


        /// <summary>
        /// Record that a section ID was read, for assigning the actual section later when all sections exist.
        /// </summary>
        /// <param name="id">Section ID.</param>
        /// <param name="self">Object which has the reference property on it.</param>
        /// <param name="prop">Expression reading the property in question.</param>
        /// 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "Exists to force TSelf to the correct type")]
        protected void PostloadRef<TSelf, TRef>(uint id, TSelf self, Expression<Func<TSelf, TRef>> prop)
        {
            var body = prop.Body as MemberExpression;
            var propinfo = body.Member as System.Reflection.PropertyInfo;
            postloadCallbacks.Add((thisSection, secs) => DeferredRefAssignment(secs, propinfo, id));
        }

        private void DeferredRefAssignment(Dictionary<uint, ISection> sections, System.Reflection.PropertyInfo pi, uint id)
        {
            ISection target;
            try
            {
                target = id != 0 ? sections[id] : null;
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't load {pi.DeclaringType.Name}.{pi.Name}: Section {SectionId} points to non-section {id}", e);
            }
            pi.SetValue(this, target);
        }

        public virtual void PostLoad(uint id, Dictionary<uint, ISection> sections)
        {
            foreach (var cb in postloadCallbacks)
            {
                cb(this, sections);
            }
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

    public static partial class SectionMetaInfo
    {
        public static uint TagFor(ISection s) => s is Unknown u ? u.TypeCode : tagByType[s.GetType()];
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class SectionIdAttribute : Attribute
    {
        public SectionIdAttribute(uint tag)
        {
            Tag = tag;
        }

        public uint Tag { get; private set; }
    }
}
