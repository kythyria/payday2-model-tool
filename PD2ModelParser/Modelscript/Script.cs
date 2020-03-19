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

            var options = new Dictionary<string, string>();
            foreach(var child in element.Elements())
            {
                switch(child.Name.ToString())
                {
                    case "rootpoint":
                        foreach(var rpitem in child.Elements())
                        {
                            switch(rpitem.Name.ToString())
                            {
                                case "object":
                                    break;
                                case "default":
                                    break;
                            }
                        }
                        break;
                    case "option":
                        options.Add(RequiredAttr(child, "name"), child.Value.Trim());
                        break;
                }
            }
            return null;
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
                throw new Exception($"Invalid value '{str}' for create_objects: "
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
