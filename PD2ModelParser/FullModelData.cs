using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static PD2ModelParser.Tags;

namespace PD2ModelParser
{
    public class FullModelData
    {
        public List<SectionHeader> sections = new List<SectionHeader>();
        public Dictionary<UInt32, object> parsed_sections = new Dictionary<UInt32, object>();
        public byte[] leftover_data = null;

        /// <summary>
        /// Adds a section to the model data, and returns it's new ID
        /// </summary>
        /// <param name="obj">The section to add</param>
        /// <param name="typecode">The ID of the section header - see <see cref="Tags"/> for these</param>
        /// <returns>The new ID of the section</returns>
        public uint AddSection(object obj, uint typecode)
        {
            // Start custom objects at ID 10001, so they are easy to identify (there's no requirement
            // we start at this point, however).
            uint id = 10001;

            // Find the first unused ID
            while (parsed_sections.ContainsKey(id))
                id++;

            // Load the new section into the parsed_sections dictionary
            parsed_sections[id] = obj;

            // And create a header for it
            SectionHeader header = new SectionHeader(id) {type = typecode};
            sections.Add(header);

            return id;
        }
    }
}
