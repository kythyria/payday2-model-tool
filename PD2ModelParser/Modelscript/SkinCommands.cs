using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using PD2ModelParser.Sections;

namespace PD2ModelParser.Modelscript
{
    class DumpSkins : ScriptItem
    {
        [Required] public string File { get; set; }

        public override void Execute(ScriptState state)
        {
            var filepath = state.ResolvePath(File);

            state.Log.Status("Writing skinning script to {0}", filepath);

            var xws = new XmlWriterSettings()
            {
                Indent = true
            };
            using XmlWriter xw = XmlWriter.Create(filepath, xws);

            xw.WriteStartDocument(true);
            xw.WriteStartElement("modelscript");

            foreach (var obj in state.Data.SectionsOfType<Model>())
            {
                if(obj.SkinBones == null) { continue; }

                xw.WriteStartElement("skin");
                xw.WriteAttributeString("object", obj.Name);
                var sb = obj.SkinBones;

                xw.WriteAttributeString("root", sb.ProbablyRootBone.Name);

                xw.WriteStartElement("global_skin_transform");
                xw.WriteWhitespace("\n");
                xw.WriteString(GetMatrixString(sb.global_skin_transform));
                xw.WriteEndElement();

                foreach (var bmi in sb.bone_mappings)
                {
                    xw.WriteStartElement("bone_mapping");
                    xw.WriteWhitespace("\n");
                    xw.WriteString(string.Join("  ", bmi.bones));
                    xw.WriteWhitespace("\n");
                    xw.WriteEndElement();
                }

                for(var i = 0; i < sb.rotations.Count; i++)
                {
                    xw.WriteStartElement("joint");
                    xw.WriteAttributeString("object", sb.Objects[i].Name);
                    xw.WriteWhitespace("\n");
                    xw.WriteString(GetMatrixString(sb.rotations[i]));
                    xw.WriteEndElement();
                } 

                xw.WriteEndElement();
            }

            xw.WriteEndElement();
        }

        string GetMatrixString(Matrix4x4 ma)
        {
            var sb = new StringBuilder(4 * (4 * 17 + 2));
            for (var i = 0; i < 16; i++)
                sb.AppendFormat("  {0,14:g9}{1}", ma.Index(i), i % 4 == 3 ? Environment.NewLine : "");
            return sb.ToString();
        }
    }

    class Skin : IScriptItem
    {
        public List<string> Objects { get; set; } = new List<string>();
        public string ProbablyRootBone { get; set; }
        public List<List<int>> BoneMappings { get; set; } = new List<List<int>>();
        public Matrix4x4 GlobalSkinTransform { get; set; }
        public List<(string, Matrix4x4)> Joints { get; set; } = new List<(string, Matrix4x4)>();

        public void ParseXml(XElement element)
        {
            Objects.Add(ScriptXml.RequiredAttr(element, "object"));
            ProbablyRootBone = ScriptXml.RequiredAttr(element, "root");

            foreach (var child in element.Elements())
            {
                switch(child.Name.ToString())
                {
                    case "global_skin_transform":
                        GlobalSkinTransform = ScriptXml.MatrixFromText(child);
                        break;
                    case "bone_mapping":
                        BoneMappings.Add(child.Value
                            .Split(ScriptXml.ValueSeparators, StringSplitOptions.RemoveEmptyEntries)
                            .Select(i => int.Parse(i))
                            .ToList());
                        break;
                    case "joint":
                        var bone = ScriptXml.RequiredAttr(child, "object");
                        var mat = ScriptXml.MatrixFromText(child);
                        Joints.Add((bone, mat));
                        break;
                }
            }
        }

        public void Execute(ScriptState state)
        {
            if (Objects.Count == 0) throw new ArgumentNullException("Must supply an object name to use <skin>.", "Objects");
            var models = new List<Model>();
            var modelnames = new HashSet<string>(Objects);
            foreach (var m in state.Data.SectionsOfType<Model>())
            {
                if (modelnames.Contains(m.Name))
                {
                    models.Add(m);
                    modelnames.Remove(m.Name);
                }
            }
            if(modelnames.Count > 0)
                throw new Exception($"One or more models not found: {string.Join(", ", modelnames)}");

            var rootbone = state.Data.SectionsOfType<Object3D>().FirstOrDefault(i => i.Name == ProbablyRootBone);
            if (rootbone == null)
                throw new Exception($"Could not find root bone '{ProbablyRootBone}'");

            var resolvedJoints = new List<(Object3D bone, Matrix4x4 transform)>();
            var objectsByName = state.Data.SectionsOfType<Object3D>().ToDictionary(i => i.Name, i => i);
            var notfound = new List<string>();
            foreach(var (bn, tf) in Joints)
            {
                if(objectsByName.TryGetValue(bn, out var o))
                {
                    resolvedJoints.Add((o, tf));
                }
                else
                {
                    notfound.Add(bn);
                }
            }
            if (notfound.Count > 0)
                throw new Exception($"One or more joints not found: {string.Join(", ", notfound.Select(i => "\"" + i + "\""))}");

            state.Log.Status("Add skinning to {0}", string.Join(", ", Objects.Select(i => "\"" + i + "\"")));

            var sb = new SkinBones();
            state.Data.AddSection(sb);
            sb.ProbablyRootBone = rootbone;
            foreach(var bl in BoneMappings)
            {
                var bmi = new BoneMappingItem();
                bmi.bones.AddRange(bl.Select(i => (uint)i));
                sb.bone_mappings.Add(bmi);
            }
            sb.global_skin_transform = this.GlobalSkinTransform;
            sb.Objects.AddRange(resolvedJoints.Select(i => i.bone));
            sb.rotations.AddRange(resolvedJoints.Select(i => i.transform));

            foreach(var m in models)
            {
                m.SkinBones = sb;
            }
        }
    }

    class RemoveSkin : ScriptItem
    {
        string Model { get; set; }

        public override void Execute(ScriptState state)
        {
            if (Model == null)
            {
                state.Log.Status("Removing all skinning data");
                foreach (var model in state.Data.SectionsOfType<Model>())
                {
                    model.SkinBones = null;
                }
            }
            else
            {
                var obj = state.Data.SectionsOfType<Model>().FirstOrDefault(i => i.Name.ToLowerInvariant() == Model.ToLowerInvariant());
                state.Log.Status("Clear skinning of \"{0}\"", Model);
                obj.SkinBones = null;
            }
        }
    }
}
