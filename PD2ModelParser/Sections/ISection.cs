using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;

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

    public interface IHashNamed
    {
        HashName HashName { get; set; }
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
            output.Write((uint)(end_pos - start_pos));

            output.BaseStream.Position = end_pos;
        }

        public abstract void StreamWriteData(BinaryWriter output);

        public override string ToString()
        {
            return $"[{GetType().Name}] ID: {SectionId}";
        }

        protected delegate void PostLoadCallback(ISection self, Dictionary<uint, ISection> sections);
        protected List<PostLoadCallback> postloadCallbacks = new List<PostLoadCallback>();

        protected void PostLoadRef<TRef>(uint id, Action<TRef> setter, [CallerFilePath] string fp = "(unknown)", [CallerLineNumber] int linenum = 0) where TRef: class
        {
            postloadCallbacks.Add((thisid, sections) => {
                ISection target;
                if (id == 0)
                {
                    setter(null);
                    return;
                }
                else if (sections.TryGetValue(id, out target)) { }
                else throw new Exception($"Couldn't resolve section reference at {fp}:{linenum}: {SectionId} points to non-section {id}");

                if (target is TRef typedTarget) // no need to worry about target==null, that only happens legit when id==0
                {
                    setter(typedTarget);
                }
                else
                {
                    throw new InvalidCastException($"Couldn't resolve section reference at {fp}:{linenum}: {SectionId} expects {id} to be a {typeof(TRef).Name} (got {target.GetType().Name})");
                }
            });
        }

        public virtual void PostLoad(uint id, Dictionary<uint, ISection> sections)
        {
            foreach (var cb in postloadCallbacks)
            {
                cb(this, sections);
            }
            postloadCallbacks = null;
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
                .Where(i => i.CustomAttributes.Any(j => j.AttributeType == typeof(ModelFileSectionAttribute)))
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
        public static IEnumerable<SectionMetaInfo> All() => byTag.Values;

        private System.Reflection.ConstructorInfo deserialiseConstructor;
        private Func<BinaryReader, SectionHeader, ISection> deserialiseDelegate;
        public bool ShowInInspectorRoot { get; private set; }
        public Type DeclaredRootInspectorType { get; private set; }

        public Type Type { get; private set; }
        public uint Tag { get; private set; }

        public ISection Deserialise(BinaryReader br, SectionHeader sh)
        {
            //return (ISection)deserialiseConstructor.Invoke(new object[] { br, sh });
            return deserialiseDelegate(br, sh);
        }

        public Inspector.IInspectorNode GetRootInspector(FullModelData data)
        {
            Type inspectortype;
            System.Reflection.ConstructorInfo constructor;
            if (DeclaredRootInspectorType != null)
            {
                inspectortype = DeclaredRootInspectorType;
            }
            else
            {
                inspectortype = typeof(Inspector.AllSectionsNode<>).MakeGenericType(new Type[] { this.Type });
            }
            constructor = inspectortype.GetConstructor(new Type[] { typeof(FullModelData) });
            if (constructor == null) { throw new Exception($"The root inspector type {inspectortype.FullName} does not have a suitable constructor"); }
            return (Inspector.IInspectorNode)constructor.Invoke(new object[] { data });
        }

        SectionMetaInfo(Type t)
        {
            this.Type = t;

            var idAttr = t.GetCustomAttributes(typeof(ModelFileSectionAttribute), false)[0] as ModelFileSectionAttribute;
            this.Tag = idAttr.Tag;
            this.DeclaredRootInspectorType = idAttr.RootInspectorNode;
            this.ShowInInspectorRoot = idAttr.ShowInInspectorRoot;

            this.deserialiseConstructor = t.GetConstructor(new Type[] { typeof(BinaryReader), typeof(SectionHeader) });

            var dm = new DynamicMethod("ConstructSection_" + t.Name, t, new Type[] { typeof(BinaryReader), typeof(SectionHeader) }, typeof(AbstractSection).Module);
            var dmil = dm.GetILGenerator();
            dmil.Emit(OpCodes.Ldarg_0);
            dmil.Emit(OpCodes.Ldarg_1);
            dmil.Emit(OpCodes.Newobj, deserialiseConstructor);
            dmil.Emit(OpCodes.Ret);
            deserialiseDelegate = (Func<BinaryReader, SectionHeader, ISection>)dm.CreateDelegate(typeof(Func<BinaryReader, SectionHeader, ISection>));
        }
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ModelFileSectionAttribute : Attribute
    {
        public ModelFileSectionAttribute(uint tag)
        {
            Tag = tag;
        }

        public uint Tag { get; private set; }
        public Type RootInspectorNode { get; set; }
        public bool ShowInInspectorRoot { get; set; } = true;
    }
}
