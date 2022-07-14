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
            {
                float value = ma.Index(i);
                if (Math.Abs(value) < 0.000001)
                    value = 0;
                sb.AppendFormat("  {0,14:g5}{1}", value, i % 4 == 3 ? Environment.NewLine : "");
            }
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

            var skins = state.Data.SectionsOfType<SkinBones>().ToArray();
            foreach(var i in skins)
            {
                state.Data.RemoveSection(i);
            }

        }
    }

    /**
     * Load the bones and skin weights from an existing Diesel model, and use them to overwrite
     * the corresponding data in the currently loaded model.
     *
     * This correlates bones and models by name, so any bones and models that only exist in either
     * the currently loaded model OR the specified Diesel file will be ignored.
     *
     * This is intended for use with resetting bones where Blender changed their transform matrices
     * in a way that works fine in game with animations exported from Blender, but breaks any
     * animations made on the in-built model.
     */
    class PortRigging : ScriptItem
    {
        [Required] public string File { get; set; }
        public bool SetModelParents { get; set; } = true;

        public override void Execute(ScriptState state)
        {
            string resolvedPath = state.ResolvePath(File);
            state.Log.Status($"Loading model (for rigging port) from {resolvedPath}");
            FullModelData srcData = ModelReader.Open(resolvedPath);

            // Find all the objects and particularly those shared across both models
            Dictionary<string, Object3D> destObjects = FindObjects(state.Data);
            Dictionary<string, Object3D> srcObjects = FindObjects(srcData);

            HashSet<string> sharedObjects = new HashSet<string>(destObjects.Keys);
            sharedObjects.IntersectWith(srcObjects.Keys);

            // Go through every object that's common and copy across it's transform
            // For any models here, also copy across the bind poses
            foreach (string name in sharedObjects)
            {
                Object3D destObj = destObjects[name];
                Object3D srcObj = srcObjects[name];

                destObj.Transform = srcObj.Transform;

                // If this object is a model, also copy across the inverse bind transforms which
                // need to be adjusted to match the new transform.
                if (destObj is not Model destModel)
                    continue;
                Model srcModel = (Model)srcObj;

                // If the user has selected it, parent the models to the same nodes as in the source model. This
                // avoids the hassle of setting up root points.
                if (SetModelParents)
                {
                    Object3D srcParent = srcModel.Parent;
                    Object3D newDestParent = srcParent == null ? null : destObjects[srcParent.HashName.String];
                    if (srcParent != null && newDestParent != null)
                    {
                        destModel.SetParent(newDestParent);
                    }
                }

                // If the model isn't rigged then it should not come as a huge surprise that there's
                // no need to copy any rigging data.
                if (destModel.SkinBones == null)
                    continue;

                // We absolutely need to copy over the global skin transform. I'm not certain about the root
                // bone, but that's probably necessary too.
                destModel.SkinBones.global_skin_transform = srcModel.SkinBones.global_skin_transform;
                destModel.SkinBones.ProbablyRootBone = destObjects[srcModel.SkinBones.ProbablyRootBone.HashName.String];

                // In case the src and dest models have the bones in a different order, build a lookup
                // table to find the ID (and thus inverse bind transform) for a given object from it's
                // parsed object.
                Dictionary<Object3D, int> srcIds = new Dictionary<Object3D, int>();
                for (int i = 0; i < srcModel.SkinBones.count; i++)
                {
                    srcIds[srcModel.SkinBones.Objects[i]] = i;
                }

                // Go through each bone in the destination, find it's corresponding bone in the source, and
                // copy the inverse bind transform over.
                for (int i = 0; i < destModel.SkinBones.count; i++)
                {
                    Object3D destBone = destModel.SkinBones.Objects[i];
                    Object3D srcBone = srcObjects.GetValueOrDefault(destBone.HashName.String, null);

                    // Skip bones that only exist in one file, or exist in both files but are only
                    // associated with the model's skin in one of the files.
                    if (srcBone == null)
                        continue;

                    if (!srcIds.ContainsKey(srcBone))
                        continue;
                    int srcId = srcIds[srcBone];

                    // Copy across the inverse bind transform
                    destModel.SkinBones.rotations[i] = srcModel.SkinBones.rotations[srcId];
                }
            }
        }

        private Dictionary<string, Object3D> FindObjects(FullModelData data)
        {
            Dictionary<string, Object3D> objects = new Dictionary<string, Object3D>();
            foreach (ISection section in data.parsed_sections.Values)
            {
                if (section is Object3D obj)
                {
                    objects[obj.HashName.String] = obj;
                }
            }
            return objects;
        }
    }
}
