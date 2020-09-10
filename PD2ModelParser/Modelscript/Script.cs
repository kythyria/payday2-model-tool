using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

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

        public static bool ExecuteWithMsgBox(IEnumerable<IScriptItem> items, ref FullModelData initialModel)
        {
            try
            {
                initialModel = ExecuteItems(items, Directory.GetCurrentDirectory(), initialModel);
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
            state.ExecuteItems(items);
            return state.Data;
        }

        public static FullModelData ExecuteItems(IEnumerable<IScriptItem> items, string workDir)
        {
            return ExecuteItems(items, workDir, new FullModelData());
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
                    case "createnew": yield return ParseSimpleItem<CreateNewObjects>(element); break;
                    case "rootpoint": yield return ParseSimpleItem<SetRootPoint>(element); break;
                    case "new": yield return ParseSimpleItem<NewModel>(element); break;
                    case "load": yield return ParseSimpleItem<LoadModel>(element); break;
                    case "save": yield return ParseSimpleItem<SaveModel>(element); break;
                    case "import": yield return ParseXmlImport(element); break;
                    case "patternuv": yield return ParseSimpleItem<PatternUV>(element); break;
                    case "export": yield return ParseSimpleItem<Export>(element); break;
                    case "exporttype": yield return ParseSimpleItem<SetDefaultExportType>(element); break;
                    case "batchexport": yield return ParseSimpleItem<BatchExport>(element); break;
                    case "object3d": yield return ParseXmlObject3d(element); break;
                    case "dumpanims": yield return ParseSimpleItem<DumpAnims>(element); break;
                    case "animate": yield return ParseXmlAnimate(element); break;
                    case "dumpskins": yield return ParseSimpleItem<DumpSkins>(element); break;
                }
            }
        }

        private static IScriptItem ParseSimpleItem<T>(XElement element) where T : IScriptItem, new()
            => ParseSimpleItem(typeof(T), element);

        private static IScriptItem ParseSimpleItem(Type T, XElement element)
        {
            var item = (IScriptItem)Activator.CreateInstance(T);

            foreach(var prop in T.GetProperties())
            {
                var requiredattr = (RequiredAttribute)(prop.GetCustomAttributes(typeof(RequiredAttribute), true).FirstOrDefault());
                var nameoverride = (XmlAttributeAttribute)(prop.GetCustomAttributes(typeof(XmlAttributeAttribute), true).FirstOrDefault());
                var attrname = nameoverride?.AttributeName ?? prop.Name.ToLower();

                string attrvalue = element.Attribute(attrname)?.Value;
                if(requiredattr != null && attrvalue == null)
                    throw new Exception($"Missing \"{attrname}\" attribute for <{element.Name}> element");

                if (!prop.CanWrite)
                    continue;

                SetItemProperty(item, prop, attrname, attrvalue);
            }

            return item;
        }

        private static Dictionary<Type, (Func<string,object>, string)> parsers = new Dictionary<Type, (Func<string, object>, string)>()
        {
            { typeof(string), (i => i, "") },
            { typeof(bool), (s => (object)(bool.Parse(s)), "must be either true or false") },
            { typeof(float), (s => (object)(float.Parse(s)), "must be a valid float according to System.Single.Parse") },
            { typeof(FileTypeInfo), (FileTypeInfo.ParseName, "must be a supported filetype") }
        };

        private static void SetItemProperty(IScriptItem item, PropertyInfo prop, string attrName, string attrval)
        {
            var pt = prop.PropertyType;
            var (typ, (parser, errmsg)) = parsers.First(i => pt.IsAssignableFrom(i.Key));

            object val;
            try
            {
                val = parser(attrval);
            }
            catch (Exception e)
            {
                throw new Exception($"Invalid value '{attrval}' for {attrName}: {errmsg}", e);
            }
            prop.SetValue(item, val);
        }

        private static readonly char[] AnimateValueSeparators = new char[] { ' ', '\t', '\r', '\n', ',' };

        private static IScriptItem ParseXmlAnimate(XElement element)
        {
            var cmd = new Animate();
            cmd.Object = RequiredAttr(element, "object");
            foreach(var ec in element.Elements())
            {
                var item = new Animate.Item();
                item.Type = ec.Name.LocalName.ToLower() switch
                {
                    "null" => Animate.ItemType.Null,
                    "float" => Animate.ItemType.Float,
                    "vector3" => Animate.ItemType.Vector3,
                    "quaternion" => Animate.ItemType.Quaternion,
                    _ => throw new Exception($"Invalid controller type {ec.Name}")
                };
                item.Name = ec.Attribute("name")?.Value;
                if(uint.TryParse(ec.Attribute("flags")?.Value, System.Globalization.NumberStyles.HexNumber, null, out var flags))
                {
                    item.Flags = flags;
                }
                cmd.Controllers.Add(item);
                if (item.Type == Animate.ItemType.Null) continue;

                item.Values = ec.Value
                    .Split(AnimateValueSeparators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => float.Parse(i))
                    .ToList();
            }

            return cmd;
        }

        private static IScriptItem ParseXmlImport(XElement element)
        {
            var item = new Import();

            item.File = RequiredAttr(element, "file");

            var strType = element.Attribute("type")?.Value;
            if(FileTypeInfo.TryParseName(strType, out var type)) {
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
        public string WorkDir { get; set; } = Directory.GetCurrentDirectory();
        public string ResolvePath(string path) => Path.Combine(WorkDir, path);
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

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    sealed class RequiredAttribute : Attribute { }
}
