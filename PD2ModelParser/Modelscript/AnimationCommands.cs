using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework.Constraints;
using PD2ModelParser.Sections;

namespace PD2ModelParser.Modelscript
{
    class DumpAnims : IScriptItem
    {
        public string File { get; set; }

        public void Execute(ScriptState state)
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
}
