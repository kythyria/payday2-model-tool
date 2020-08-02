using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

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
            // Start custom objects at ID 10001, so they are easy to identify (there's no requirement
            // we start at this point, however).
            uint id = 10001;

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

        public void RemoveById(uint id)
        {
            SectionHeader header = sections.Find(s => s.id == id);
            if (header == null)
                throw new ArgumentException("Cannot remove missing header", nameof(id));

            if (!parsed_sections.ContainsKey(id))
                throw new ArgumentException("Cannot remove unparsed header", nameof(id));

            parsed_sections.Remove(id);
            sections.Remove(header);
        }

        public IEnumerable<T> SectionsOfType<T>() where T : class
        {
            return parsed_sections.Where(i => i.Value is T).Select(i => i.Value as T);
        }
    }
}
