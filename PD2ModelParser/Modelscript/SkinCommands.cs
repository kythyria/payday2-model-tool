using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using PD2ModelParser.Sections;

namespace PD2ModelParser.Modelscript
{
    class DumpSkins : IScriptItem
    {
        [Required] public string File { get; set; }

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
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    sb.AppendFormat("  {0,14:g9}", ma.Index(i, j));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
