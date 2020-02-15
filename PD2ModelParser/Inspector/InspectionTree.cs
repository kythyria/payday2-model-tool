using System;
using System.Collections.Generic;
using System.Linq;

namespace PD2ModelParser.Inspector
{
    interface IInspectorNode
    {
        string Key { get; }
        string IconName { get; }
        string Label { get; }
        object PropertyItem { get; }
        IEnumerable<IInspectorNode> GetChildren();
    }

    class ModelRootNode : IInspectorNode
    {
        FullModelData data;
        public ModelRootNode(FullModelData fmd) {
            data = fmd;
        }

        public string Key => "<root>";
        public string IconName => null;
        public string Label => "<root>";
        public object PropertyItem => data;
        public IEnumerable<IInspectorNode> GetChildren()
        {
            yield return new ObjectsRootNode(data);
            yield return new AllSectionsNode<Sections.Geometry>(data, "<geometries>", "<Geometry>", g => $"{g.SectionId} | Geometry ({new HashName(g.hashname).String})");
            yield return new AllSectionsNode<Sections.SkinBones>(data, "<skinbones>", "<SkinBones>", sb => $"{sb.SectionId} | SkinBones");
        }
    }


    class AllSectionsNode<TSection> : IInspectorNode
        where TSection : class, Sections.ISection
    {
        FullModelData data;
        Func<TSection, string> labeller;
        public AllSectionsNode(FullModelData fmd, string key, string label, Func<TSection, string> childLabel)
        {
            data = fmd;
            Key = key;
            Label = label;
            labeller = childLabel;
        }

        public string Key { get; private set; }
        public string IconName => null;
        public string Label { get; private set; }
        public object PropertyItem => null;
        public IEnumerable<IInspectorNode> GetChildren()
        {
            var cname = typeof(TSection).Name;
            return data.SectionsOfType<TSection>().Select(i => new GenericNode
            {
                Key = $"{cname}_{i.SectionId}",
                IconName = null,
                Label = labeller(i),
                PropertyItem = i
            });
        }
    }

    class GenericNode : IInspectorNode
    {
        public string Key { get; set; }
        public string IconName { get; set; }
        public string Label { get; set; }
        public object PropertyItem { get; set; }
        public IEnumerable<IInspectorNode> GetChildren() => Enumerable.Empty<IInspectorNode>();
    }

    class ObjectsRootNode : IInspectorNode
    {
        FullModelData data;
        public ObjectsRootNode(FullModelData fmd)
        {
            data = fmd;
        }

        public string Key => "<objects>";
        public string IconName => null;
        public string Label => "<Object3D>";
        public object PropertyItem => null;
        public IEnumerable<IInspectorNode> GetChildren()
        {
            return data.SectionsOfType<Sections.Object3D>().Where(i => i.parent == null).Select(i => new ObjectNode(data, i));
        }
    }

    class ObjectNode : IInspectorNode
    {
        FullModelData data;
        Sections.Object3D obj;
        public ObjectNode(FullModelData fmd, Sections.Object3D obj)
        {
            data = fmd;
            this.obj = obj;

            Label = $"{obj.SectionId} | {obj.Name}";
            if (obj.GetType() != typeof(Sections.Object3D))
            {
                Label += $" ({obj.GetType().Name})";
            }
        }

        public string Key => $"obj_{obj.SectionId}";
        public string IconName => null;
        public string Label { get; private set; }
        public object PropertyItem => obj;
        public IEnumerable<IInspectorNode> GetChildren()
        {
            return data.SectionsOfType<Sections.Object3D>().Where(i => i.parent == obj).Select(i => new ObjectNode(data, i));
        }
    }
}
