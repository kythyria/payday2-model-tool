using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace PD2ModelParser.Modelscript
{
    public static class Script
    {
        public static bool ExecuteFileWithMsgBox(ref FullModelData data, string file)
        {
            try
            {
                XElement root = XElement.Parse(File.ReadAllText(file));
                var script = ParseXml(root);
                data = ExecuteItems(script, Directory.GetCurrentDirectory(), data);
                return true;
            }
            catch (Exception exc)
            {
                Log.Default.Warn("Exception in script file: {0}", exc);
                System.Windows.Forms.MessageBox.Show("There was an error in the script file:\n" +
                                "(check the readme on GitLab for more information)\n" + exc.Message);
                return false;
            }
        }

        public static FullModelData ExecuteItems(IEnumerable<IScriptItem> items, string workDir, FullModelData initialModel)
        {
            var state = new ScriptState
            {
                Data = initialModel,
                WorkDir = workDir
            };
            foreach (var i in items)
            {
                i.Execute(state);
            }
            return state.Data;
        }

        public static FullModelData ExecuteItems(IEnumerable<IScriptItem> items, string workDir)
        {
            return ExecuteItems(items, workDir, new FullModelData());
        }

        public static IEnumerable<string> GetSourceFilenames(IEnumerable<IScriptItem> items)
        {
            return items.Where(i => i is IReadsFile).Select(i => (i as IReadsFile).File);
        }

        public static IEnumerable<IScriptItem> ParseXml(string path)
        {
            using (var tr = new StreamReader(path))
            {
                return ParseXml(tr);
            }
        }

        public static IEnumerable<IScriptItem> ParseXml(TextReader tr)
        {
            var xe = XElement.Load(tr);
            return ParseXml(xe);
        }

        public static IEnumerable<IScriptItem> ParseXml(XElement root)
        {
            if (root.Name != "modelscript")
                throw new Exception("Script root node is not named \"modelscript\"");

            foreach (var element in root.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "createnew": yield return ParseXmlCreatenew(element); break;
                    case "rootpoint": yield return ParseXmlRootpoint(element); break;
                    case "new": yield return new NewModel(); break;
                    case "load": yield return ParseXmlLoadModel(element); break;
                    case "save": yield return ParseXmlSaveModel(element); break;
                    case "import": yield return ParseXmlImport(element); break;
                    case "patternuv": yield return ParseXmlPatternuv(element); break;
                    case "export": yield return ParseXmlExport(element); break;
                    case "exporttype": yield return ParseXmlExportType(element); break;
                    case "batchexport": yield return ParseXmlBatchExport(element); break;
                    case "object3d": yield return ParseXmlObject3d(element); break;
                }
            }
        }

        private static IScriptItem ParseXmlImport(XElement element)
        {
            var item = new Import();

            item.File = RequiredAttr(element, "file");

            var strType = element.Attribute("type")?.Value;
            if(FileTypeInfo.TryGetByExtension(strType, out var type)) {
                item.ForceType = type;
            }
            else { item.ForceType = null; }

            var strCreateObjects = element.Attribute("create_objects")?.Value;
            if(strCreateObjects != null && bool.TryParse(strCreateObjects, out var createObjects))
            {
                item.CreateNewObjects = createObjects;
            }
            else if(strCreateObjects != null)
            {
                throw new Exception($"create_objects must be boolean, \"{bool.TrueString}\" or \"{bool.FalseString}\"");
            }

            foreach (var child in element.Elements())
            {
                switch(child.Name.ToString())
                {
                    case "rootpoint":
                        var targetname = RequiredAttr(child, "name");
                        foreach(var rpitem in child.Elements())
                        {
                            switch(rpitem.Name.ToString())
                            {
                                case "object":
                                    var childName = RequiredAttr(rpitem, "name");
                                    if(item.Parents.ContainsKey(childName))
                                        throw new Exception($"Cannot redefine rootpoint for object {childName}");
                                    item.Parents.Add(childName, targetname);
                                    break;
                                case "default":
                                    if(item.DefaultRootPoint == null)
                                        item.DefaultRootPoint = targetname;
                                    else
                                        throw new Exception($"Cannot redefine default rootpoint to {targetname}");
                                    break;
                                default:
                                    throw new Exception($"Invalid <rootpoint> child: {rpitem.Name}");
                            }
                        }
                        break;
                    case "option":
                        item.ImporterOptions.Add(RequiredAttr(child, "name"), child.Value.Trim());
                        break;
                }
            }
            return item;
        }

        private static IScriptItem ParseXmlSaveModel(XElement elem)
        {
            var file = RequiredAttr(elem, "file");
            return new SaveModel() { File = file };
        }

        private static IScriptItem ParseXmlLoadModel(XElement elem)
        {
            var file = RequiredAttr(elem, "file");
            return new LoadModel() { File = file };
        }

        private static IScriptItem ParseXmlCreatenew(XElement elem)
        {
            var create = RequiredBool(elem, "create");
            return new CreateNewObjects { Create = create };
        }

        private static IScriptItem ParseXmlRootpoint(XElement elem)
        {
            var name = elem.Attribute("name")?.Value;
            return new SetRootPoint() { Name = name };
        }

        private static IScriptItem ParseXmlPatternuv(XElement elem)
        {
            var file = RequiredAttr(elem, "file");
            return new PatternUV() { File = file };
        }

        private static IScriptItem ParseXmlExport(XElement elem)
        {
            var file = RequiredAttr(elem, "file");
            var ext = elem.Attribute("type")?.Value;
            var item = new Export() { File = file };
            if(FileTypeInfo.TryGetByExtension(ext, out var type))
            {
                item.ForceType = type;
            }
            else if(ext != null)
            {
                throw new Exception($"Invalid value {type} for type: Unrecognised format.");
            }
            return item;
        }

        private static IScriptItem ParseXmlExportType(XElement elem)
        {
            var type = RequiredAttr(elem, "type");
            if (FileTypeInfo.TryGetByExtension(type, out var fti))
            {
                return new SetDefaultType() { FileType = fti };
            }
            throw new Exception($"Invalid value {type} for type: Unrecognised format.");
        }

        private static IScriptItem ParseXmlBatchExport(XElement elem)
        {
            var dir = RequiredAttr(elem, "sourcedir");
            var res = new BatchExport() { Directory = dir };
            var typeish = elem.Attribute("type")?.Value;
            if (typeish != null && FileTypeInfo.TryGetByExtension(typeish, out var fti)) {
                res.FileType = fti;
            }
            return res;
        }

        private static IScriptItem ParseXmlObject3d(XElement elem)
        {
            var name = RequiredAttr(elem, "name");
            var mode = RequiredAttr(elem, "mode");

            var pos = new Vector3?();
            var scale = new Vector3?();
            var rot = new Quaternion?();

            var setParent = false;
            string parent = null;
            foreach(var child in elem.Elements())
            {
                switch(child.Name.ToString())
                {
                    case "position":
                        pos = VectorElement(child);
                        break;
                    case "rotation":
                        rot = QuaternionElement(child);
                        break;
                    case "scale":
                        scale = VectorElement(child);
                        break;
                    case "parent":
                        var root = child.Attribute("root")?.Value;
                        var parentname = child.Attribute("name")?.Value;
                        if (root != null) { }
                        else if (root != null && parentname != null)
                            throw new Exception("parent must have either root or name attributes, not both");
                        else
                            parent = parentname;
                        setParent = true;
                        break;
                    default:
                        throw new Exception($"Invalid Object3D child element {elem.Name}");
                }
            }

            if(mode == "add")
            {
                if (!setParent)
                    throw new Exception("Newly created objects must have their parents explicitly given");
                var result = new CreateObject3d()
                {
                    Name = name,
                    Parent = parent
                };
                pos.WithValue(p => result.Position = p);
                scale.WithValue(s => result.Scale = s);
                rot.WithValue(r => result.Rotation = r);
                return result;
            }
            else if(mode == "edit")
            {
                return new ModifyObject3d
                {
                    Name = name,
                    Parent = parent,
                    SetParent = setParent,
                    Position = pos,
                    Rotation = rot,
                    Scale = scale
                };
            }
            else
            {
                throw new Exception($"Object3D mode must be \"edit\" or \"add\".");
            }
        }

        private static string RequiredAttr(XElement elem, string attr)
        {
            return elem.Attribute(attr)?.Value ??
                   throw new Exception($"Missing \"{attr}\" attribute for {elem.Name} element");
        }

        private static bool RequiredBool(XElement elem, string attrName)
        {
            var str = RequiredAttr(elem, attrName);
            if(!bool.TryParse(str, out var result))
            {
                throw new Exception($"Invalid value '{str}' for {attrName}: "
                                    + "must either be true or false");
            }
            else
            {
                return result;
            }
        }

        private static float RequiredFloat(XElement elem, string attr)
        {
            var str = RequiredAttr(elem, attr);
            if(!float.TryParse(str, out var result))
            {
                throw new Exception($"Invalid value '{str}' for {attr}: "
                    + "must be a valid float according to System.Float.TryParse");
            }
            else
            {
                return result;
            }
        }

        private static Vector3 VectorElement(XElement elem)
        {
            var X = RequiredFloat(elem, "x");
            var Y = RequiredFloat(elem, "y");
            var Z = RequiredFloat(elem, "z");
            return new Vector3(X, Y, Z);
        }

        private static Quaternion QuaternionElement(XElement elem)
        {
            var vec = VectorElement(elem);
            var W = RequiredFloat(elem, "w");
            return new Quaternion(vec, W);
        }
    }

    public class ScriptState
    {
        public FullModelData Data { get; set; }
        public string WorkDir { get; set; }
        public string ResolvePath(string path) => System.IO.Path.Combine(WorkDir, path);
        public bool CreateNewObjects { get; set; }
        public Sections.Object3D DefaultRootPoint { get; set; }
        public FileTypeInfo DefaultExportType { get; set; } = FileTypeInfo.Gltf;

        public void ExecuteItems(IEnumerable<IScriptItem> items)
        {
            foreach (var i in items)
            {
                i.Execute(this);
            }
        }

        public ILogger Log => PD2ModelParser.Log.Default;
    }

    public interface IScriptItem
    {
        void Execute(ScriptState state);
    }

    public interface IReadsFile
    {
        string File { get; }
    }
}
