using Nexus;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    static class DieselExporter
    {
        public static void ExportFile(FullModelData data, string path)
        {
            //you remove items from the parsed_sections
            //you edit items in the parsed_sections, they will get read and exported

            //Sort the sections
            List<Animation> animation_sections = new List<Animation>();
            List<Author> author_sections = new List<Author>();
            List<ISection> material_sections = new List<ISection>();
            List<Object3D> object3D_sections = new List<Object3D>();
            List<Model> model_sections = new List<Model>();
            List<ISection> other_sections = new List<ISection>();

            // Discard the old hashlist
            // Note that we use ToArray, which allows us to mutate the list without breaking anything
            foreach (SectionHeader header in data.sections.ToArray())
                if (header.type == Tags.custom_hashlist_tag)
                    data.RemoveById(header.id);

            CustomHashlist hashlist = new CustomHashlist();
            data.AddSection(hashlist);

            foreach (SectionHeader sectionheader in data.sections)
            {
                if (!data.parsed_sections.Keys.Contains(sectionheader.id))
                {
                    Log.Default.Warn($"BUG: SectionHeader with id {sectionheader.id} has no counterpart in parsed_sections");
                    continue;
                }

                var section = data.parsed_sections[sectionheader.id];

                if (section is Animation)
                {
                    animation_sections.Add(section as Animation);
                }
                else if (section is Author)
                {
                    author_sections.Add(section as Author);
                }
                else if (section is Material_Group)
                {
                    foreach(var matid in (section as Material_Group).items)
                    {
                        var matsec = data.parsed_sections[matid];
                        if(!material_sections.Contains(matsec))
                        {
                            material_sections.Add(matsec);
                        }
                    }
                    material_sections.Add(section);
                }
                else if (section is Model) // Has to be before Object3D, since it's a subclass.
                {
                    model_sections.Add(section as Model);
                }
                else if (section is Object3D)
                {
                    object3D_sections.Add(section as Object3D);
                }
                else if (section != null )
                {
                    other_sections.Add(section as ISection);
                }
                else
                {
                    Log.Default.Warn("BUG: Somehow a null or non-section found its way into the list of sections.");
                }

                if (section is IHashContainer container)
                {
                    container.CollectHashes(hashlist);
                }
            }

            var sections_to_write = Enumerable.Empty<ISection>()
                        .Concat(animation_sections)
                        .Concat(author_sections)
                        .Concat(material_sections)
                        .Concat(object3D_sections)
                        .Concat(model_sections)
                        .Concat(other_sections)
                        .ToList();

            //after each section, you go back and enter it's new size
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {

                    bw.Write(-1); //the - (yyyy)
                    bw.Write((UInt32)100); //Filesize (GO BACK AT END AND CHANGE!!!)
                    int sectionCount = data.sections.Count;
                    bw.Write(sectionCount); //Sections count

                    foreach (var sec in sections_to_write)
                    {
                        sec.StreamWrite(bw);
                    }

                    if(sections_to_write.Count != sectionCount)
                    {
                        Log.Default.Warn($"BUG : There were {sectionCount} sections to write but {sections_to_write.Count} were written");
                    }

                    if (data.leftover_data != null)
                        bw.Write(data.leftover_data);

                    fs.Position = 4;
                    bw.Write((UInt32)fs.Length);

                }
            }
        }
    }
}
