using System;
using System.IO;

namespace PD2ModelParser
{
    public class SectionHeader
    {
        public UInt32 type;
        public UInt32 id;
        public UInt32 size;
        public long offset;

        /// <summary>
        /// Get the starting position for the contents of this section
        /// </summary>
        public long Start
        {
            get
            {
                return offset + 12; // 12 represents the three int32s that are part of the header
            }
        }

        /// <summary>
        /// Get the ending position for this section - this is one byte past the last end of this
        /// section, and is equal to the offset value of the next section header.
        /// </summary>
        public long End
        {
            get
            {
                return Start + size;
            }
        }

        public SectionHeader(uint sec_id)
        {
            this.id = sec_id;
        }

        public SectionHeader(BinaryReader instream)
        {
            this.offset = instream.BaseStream.Position;
            this.type = instream.ReadUInt32();
            this.id = instream.ReadUInt32();
            this.size = instream.ReadUInt32();
        }

        public override string ToString()
        {
            return "[SectionHeader] Type: " + this.type + "  ID: " + this.id + " Size: " + this.size + " Offset: " + this.offset;
        }
    }
}
