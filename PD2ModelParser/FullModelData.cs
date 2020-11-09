using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PD2ModelParser
{
    public class FullModelData
    {
        public List<SectionHeader> sections = new List<SectionHeader>();
        public Dictionary<UInt32, ISection> parsed_sections = new Dictionary<UInt32, ISection>();
        public byte[] leftover_data = null;

        /// <summary>
        /// Adds a section to the model data.
        /// </summary>
        /// <remarks>
        /// This sets the Section ID of the passed object.
        /// </remarks>
        /// <param name="obj">The section to add</param>
        public void AddSection(ISection obj)
        {
            // Shouldn't add twice
            if(parsed_sections.ContainsValue(obj)) { return; }

            // Objects with no ID already start at 10001. There's no real reason for this
            // but we have to start somewhere and this is smaller than what overkill uses.
            uint id = obj.SectionId != 0 ? obj.SectionId : 10001;

            // Find the first unused ID
            while (parsed_sections.ContainsKey(id))
                id++;

            // Set the object's ID
            obj.SectionId = id;

            // Load the new section into the parsed_sections dictionary
            parsed_sections[id] = obj;

            // And create a header for it
            SectionHeader header = new SectionHeader(id) {type = obj.TypeCode};
            sections.Add(header);
        }

        public void RemoveSection(uint id)
        {
            SectionHeader header = sections.Find(s => s.id == id);
            if (header == null)
                throw new ArgumentException("Cannot remove missing header", nameof(id));

            if (!parsed_sections.ContainsKey(id))
                throw new ArgumentException("Cannot remove unparsed header", nameof(id));

            parsed_sections.Remove(id);
            sections.Remove(header);
        }

        public void RemoveSection(ISection section) => RemoveSection(section.SectionId);

        public IEnumerable<T> SectionsOfType<T>() where T : class
        {
            return parsed_sections.Where(i => i.Value is T).Select(i => i.Value as T);
        }

        /// <summary>
        /// Enforces that sections with a <see cref="IHashNamed.HashName"/> have unique names.
        /// </summary>
        public void UniquifyNames()
        {
            // The trick we use here is that since AbstractSection is abstract, we can group objects
            // by their highest non-abstract base class and be pretty much right. The worst that can
            // happen is a too-wide namespace anyway, and that won't hurt much.

            var seenNamesOverall = new Dictionary<Type, HashSet<ulong>>();

            foreach(var sec in SectionsOfType<IHashNamed>())
            {
                var st = sec.GetType();
                while (!st.BaseType.IsAbstract) { st = st.BaseType; }

                if(!seenNamesOverall.TryGetValue(st, out var seenNames))
                {
                    seenNames = seenNamesOverall[st] = new HashSet<ulong>();
                }

                var candidateName = sec.HashName;

                while (seenNames.Contains(candidateName.Hash))
                {
                    if(!candidateName.Known)
                    {
                        candidateName = new HashName(candidateName.Hash++);
                    }
                    else
                    {
                        var m = Regex.Match(candidateName.String, @"\.(\d+)$");
                        if(m == null)
                        {
                            candidateName = new HashName(candidateName.String + ".001");
                        }
                        else
                        {
                            var count = int.Parse(m.Groups[1].Value);
                            count++;
                            var baseName = candidateName.String.Substring(0, candidateName.String.Length - m.Length);
                            candidateName = new HashName(string.Format("{0}.{1:D3}", baseName, count));
                        }
                    }
                }
            }
        }
    }
}
