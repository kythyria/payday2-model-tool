using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PD2ModelParser.Modelscript
{
    public static class Script
    {

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
                }
            }
        }

        private static IScriptItem ParseXmlImport(XElement element)
        {
            var item = new Import();

            item.File = RequiredAttr(element, "file");

            var strType = element.Attribute("type")?.Value;
            if(Enum.TryParse<ImportFileType>(strType, true, out var type)) {
                item.ForceType = type;
            }
            else if (strType.ToLower() == "glb") { item.ForceType = ImportFileType.Gltf; }
            else { item.ForceType = null; }

            foreach(var child in element.Elements())
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
            return new Export() { File = file };
        }

        private static IScriptItem ParseXmlExportType(XElement elem)
        {
            var type = RequiredAttr(elem, "type");
            if (FileTypeInfo.TryGetByExtension(type, out var fti))
            {
                return new SetDefaultType() { FileType = fti.ExportType };
            }
            throw new Exception($"Invalid value {type} for type: Unrecognised format.");
        }

        private static IScriptItem ParseXmlBatchExport(XElement elem)
        {
            var dir = RequiredAttr(elem, "sourcedir");
            var res = new BatchExport() { Directory = dir };
            var typeish = elem.Attribute("type")?.Value;
            if (typeish != null && FileTypeInfo.TryGetByExtension(typeish, out var fti)) {
                res.FileType = fti.ExportType;
            }
            return res;
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
    }

    public class ScriptState
    {
        public FullModelData Data { get; set; }
        public string WorkDir { get; set; }
        public string ResolvePath(string path) => System.IO.Path.Combine(WorkDir, path);
        public bool CreateNewObjects { get; set; }
        public Sections.Object3D DefaultRootPoint { get; set; }
        public ExportFileType DefaultExportType { get; set; }

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
