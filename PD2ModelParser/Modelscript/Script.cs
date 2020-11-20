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

            var script = new List<IScriptItem>();

            foreach (var element in root.Elements())
            {
                if(element.Name.ToString() == "object3d")
                {
                    script.Add(ParseXmlObject3d(element));
                    continue;
                }

                IScriptItem si = (element.Name.ToString()) switch
                {
                    "createnew" => new CreateNewObjects(),
                    "rootpoint" => new SetRootPoint(),
                    "new" => new NewModel(),
                    "load" => new LoadModel(),
                    "save" => new SaveModel(),
                    "patternuv" => new PatternUV(),
                    "export" => new Export(),
                    "exporttype" => new SetDefaultExportType(),
                    "batchexport" => new BatchExport(),
                    "dumpanims" => new DumpAnims(),
                    "dumpskins" => new DumpSkins(),
                    "import" => new Import(),
                    "animate" => new Animate(),
                    "skin" => new Skin(),
                    "removeskin" => new RemoveSkin(),
                    "runscript" => new RunScript(),
                    _ => throw new Exception($"Unknown command {element.Name}"),
                };
                si.ParseXml(element);
                script.Add(si);
            }

            return script;
        }

        private static IScriptItem ParseXmlObject3d(XElement elem)
        {
            var name = ScriptXml.RequiredAttr(elem, "name");
            var mode = ScriptXml.RequiredAttr(elem, "mode");

            var pos = new Vector3?();
            var scale = new Vector3?();
            var rot = new Quaternion?();

            var setParent = false;
            string parent = null;
            foreach (var child in elem.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "position":
                        pos = ScriptXml.VectorElement(child);
                        break;
                    case "rotation":
                        rot = ScriptXml.QuaternionElement(child);
                        break;
                    case "scale":
                        scale = ScriptXml.VectorElement(child);
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

            if (mode == "add")
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
            else if (mode == "edit")
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
    }

    static class ScriptXml {

        public static string RequiredAttr(XElement elem, string attr)
        {
            return elem.Attribute(attr)?.Value ??
                   throw new Exception($"Missing \"{attr}\" attribute for {elem.Name} element");
        }

        public static float RequiredFloat(XElement elem, string attr)
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

        public static Vector3 VectorElement(XElement elem)
        {
            var X = RequiredFloat(elem, "x");
            var Y = RequiredFloat(elem, "y");
            var Z = RequiredFloat(elem, "z");
            return new Vector3(X, Y, Z);
        }

        public static Quaternion QuaternionElement(XElement elem)
        {
            var vec = VectorElement(elem);
            var W = RequiredFloat(elem, "w");
            return new Quaternion(vec, W);
        }

        public static readonly char[] ValueSeparators = new char[] { ' ', '\t', '\r', '\n', ',' };
        public static List<float> FloatsFromText(XElement elem)
            => elem.Value.Trim(ValueSeparators)
                .Split(ValueSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(i => float.Parse(i))
                .ToList();

        public static Matrix4x4 MatrixFromText(XElement elem)
        {
            var floats = FloatsFromText(elem);
            Matrix4x4 m = new Matrix4x4();
            for (var i = 0; i < 16; i++)
                m.Index(i) = floats[i];
            return m;
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
        void ParseXml(XElement elem);
    }

    public interface IComplexParseItem
    {
    }

    public abstract class ScriptItem : IScriptItem, IComplexParseItem
    {
        public abstract void Execute(ScriptState state);
        public virtual void ParseXml(XElement elem)
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                var requiredattr = (RequiredAttribute)(prop.GetCustomAttributes(typeof(RequiredAttribute), true).FirstOrDefault());
                var nameoverride = (XmlAttributeAttribute)(prop.GetCustomAttributes(typeof(XmlAttributeAttribute), true).FirstOrDefault());
                var attrname = nameoverride?.AttributeName ?? prop.Name.ToLower();

                string attrvalue = elem.Attribute(attrname)?.Value;
                if (requiredattr != null && attrvalue == null)
                    throw new Exception($"Missing \"{attrname}\" attribute for <{elem.Name}> element");
                else if (requiredattr == null && attrvalue == null)
                    continue;

                if (!prop.CanWrite)
                    continue;

                var pt = prop.PropertyType;

                Type typ; Func<string, object> parser; string errmsg;
                if (pt.BaseType == typeof(Enum))
                {
                    typ = pt;
                    parser = (s) => Enum.Parse(pt, s, true);
                    if(pt.GetCustomAttribute(typeof(FlagsAttribute)) != null)
                    {
                        errmsg = "must be comma-separated list of at least one of ";
                    }
                    else
                    {
                        errmsg = "must be one of ";
                    }
                    errmsg += string.Join(", ", Enum.GetNames(pt));
                }
                else
                { 
                    (typ, (parser, errmsg)) = parsers.First(i => pt.IsAssignableFrom(i.Key));
                }

                object val;
                try
                {
                    val = parser(attrvalue);
                }
                catch (Exception e)
                {
                    throw new Exception($"Invalid value '{attrvalue}' for {attrname}: {errmsg}", e);
                }
                prop.SetValue(this, val);
            }
        }

        private static Dictionary<Type, (Func<string, object>, string)> parsers = new Dictionary<Type, (Func<string, object>, string)>()
        {
            { typeof(string), (i => i, "") },
            { typeof(bool), (s => (object)(bool.Parse(s)), "must be either true or false") },
            { typeof(float), (s => (object)(float.Parse(s)), "must be a valid float according to System.Single.Parse") },
            { typeof(FileTypeInfo), (FileTypeInfo.ParseName, "must be a supported filetype") }
        };
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    sealed class RequiredAttribute : Attribute { }
}
