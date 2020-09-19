using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml;
using System.Xml.Linq;
using PD2ModelParser.Sections;

namespace PD2ModelParser.Modelscript
{
    class DumpAnims : ScriptItem
    {
        [Required] public string File { get; set; }

        public override void Execute(ScriptState state)
        {
            var filepath = state.ResolvePath(File);

            var xws = new XmlWriterSettings()
            {
                Indent = true
            };
            using XmlWriter xw = XmlWriter.Create(filepath, xws);

            xw.WriteStartDocument(true);
            xw.WriteStartElement("modelscript");

            foreach(var obj in state.Data.SectionsOfType<Object3D>())
            {
                if(obj.Animations.Count == 0) { continue; }

                xw.WriteStartElement("animate");
                xw.WriteAttributeString("object", obj.Name);

                foreach(var anim in obj.Animations)
                {
                    if(anim == null) { 
                        xw.WriteStartElement("null"); 
                        xw.WriteEndElement(); 
                        continue; 
                    }

                    xw.WriteStartElement(anim switch
                    {
                        LinearFloatController _ => "float",
                        LinearVector3Controller _ => "vector3",
                        QuatLinearRotationController _ => "quaternion",
                        _ => throw new Exception($"Unrecognised animation controller type in {obj.Name}")
                    });

                    if (anim.Flags != 0) xw.WriteAttributeString("flags", "f");

                    foreach (var line in GetControllerLines(anim))
                    {
                        xw.WriteWhitespace("\n");
                        xw.WriteString(line);
                    }

                    xw.WriteEndElement();
                }

                xw.WriteEndElement();
            }

            xw.WriteEndElement();
        }

        IEnumerable<string> GetControllerLines(ISection anim)
        {
            if (anim == null) return Enumerable.Empty<string>();

            var floats = (anim switch
            {
                LinearFloatController lf => lf.Keyframes.Select(i => new float[] { i.Timestamp, i.Value }),
                LinearVector3Controller lv3 => lv3.Keyframes.Select(i => new float[] { i.Timestamp, i.Value.X, i.Value.Y, i.Value.Z }),
                QuatLinearRotationController qlr => qlr.Keyframes.Select(i => new float[] { i.Timestamp, i.Value.X, i.Value.Y, i.Value.Z, i.Value.W }),
                _ => throw new Exception($"Unrecognised animation controller type {anim.GetType().Name}")
            }).ToList();

            var strings = floats.Select(line => string.Join("  ", line.Select(i => string.Format("{0,14:g9}", i))));
            return strings.ToList();
        }
    }

    class Animate : IScriptItem
    {
        public enum ItemType
        {
            Null,
            Float,
            Vector3,
            Quaternion
        }
        public class Item
        {
            public ItemType Type { get; set; }
            public string Name { get; set; }
            public List<float> Values { get; set; }
            public uint? Flags { get; set; }
        }

        public string Object { get; set; }
        public List<Item> Controllers { get; set; } = new List<Item>();

        public void ParseXml(XElement element)
        {
            this.Object = ScriptXml.RequiredAttr(element, "object");
            foreach (var ec in element.Elements())
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
                if (uint.TryParse(ec.Attribute("flags")?.Value, System.Globalization.NumberStyles.HexNumber, null, out var flags))
                {
                    item.Flags = flags;
                }
                this.Controllers.Add(item);
                if (item.Type == Animate.ItemType.Null) continue;

                item.Values = ScriptXml.FloatsFromText(ec);
            }
        }

        public void Execute(ScriptState state)
        {
            if (Object == null) throw new ArgumentNullException("Must supply an object name to use <animate>.", "Object");
            var obj = state.Data.SectionsOfType<Object3D>().FirstOrDefault(i => i.Name.ToLowerInvariant() == Object.ToLowerInvariant());
            if (obj == null) throw new Exception($"Object {Object} not found");

            state.Log.Status($"Setting animations for {Object}");
            obj.Animations.Clear();
            foreach(var item in Controllers)
            {
                if(item.Type == ItemType.Null) { obj.Animations.Add(null); continue; }

                IAnimationController c;
                switch (item.Type)
                {
                    case ItemType.Float:
                        if (item.Values.Count % 2 != 0)
                            throw new Exception($"Controller for {obj.Name} is wrong length (must be even");
                        var fc = new LinearFloatController(item.Name);
                        for (var i = 0; i < item.Values.Count; i += 2)
                        {
                            fc.Keyframes.Add(new Keyframe<float>(item.Values[i], item.Values[i + 1]));
                        }
                        SetLength(fc);
                        c = fc;
                        break;
                    case ItemType.Vector3:
                        if (item.Values.Count % 4 != 0)
                            throw new Exception($"Controller for {obj.Name} is wrong length (must be multiple of 4");
                        var vc = new LinearVector3Controller(item.Name);
                        for (var i = 0; i < item.Values.Count; i += 4)
                        {
                            vc.Keyframes.Add(new Keyframe<Vector3>(item.Values[i], new Vector3(item.Values[i + 1], item.Values[i + 2], item.Values[i + 3])));
                        }
                        SetLength(vc);
                        c = vc;
                        break;
                    case ItemType.Quaternion:
                        if (item.Values.Count % 5 != 0)
                            throw new Exception($"Controller for {obj.Name} is wrong length (must be multiple of 5");
                        var qc = new QuatLinearRotationController(item.Name);
                        for (var i = 0; i < item.Values.Count; i += 5)
                        {
                            qc.Keyframes.Add(new Keyframe<Quaternion>(item.Values[i], new Quaternion(item.Values[i + 1], item.Values[i + 2], item.Values[i + 3], item.Values[i + 4])));
                        }
                        SetLength(qc);
                        c = qc;
                        break;
                    default:
                        throw new Exception("Unknown animation type");
                }
                c.Flags = item.Flags ?? 0;
                state.Data.AddSection(c);
                obj.Animations.Add(c);
            }
        }

        private void SetLength<T>(IAnimationController<T> c)
            => c.KeyframeLength = c.Keyframes.Max(kf => kf.Timestamp);
    }
}
