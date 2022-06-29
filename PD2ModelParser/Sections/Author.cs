using System;
using System.IO;

namespace PD2ModelParser.Sections
{
    [ModelFileSection(Tags.author_tag)]
    class Author : AbstractSection, ISection, IHashNamed
    {
        public UInt32 size;

        public HashName HashName { get; set; }
        public String email; //Author's email address
        public String source_file; //Source model file
        public UInt32 unknown2;

        public byte[] remaining_data = null;

        public Author(BinaryReader instream, SectionHeader section)
        {
            this.SectionId = section.id;
            this.size = section.size;
            this.HashName = new HashName(instream.ReadUInt64());

            this.email = instream.ReadCString();
            this.source_file = instream.ReadCString();

            this.unknown2 = instream.ReadUInt32();

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public override void StreamWriteData(BinaryWriter outstream)
        {
            Byte zero = 0;
            outstream.Write(this.HashName.Hash);
            outstream.Write(this.email.ToCharArray());
            outstream.Write(zero);
            outstream.Write(this.source_file.ToCharArray());
            outstream.Write(zero);
            outstream.Write(this.unknown2);

            if (this.remaining_data != null)
                outstream.Write(this.remaining_data);
        }

        public override string ToString()
        {
            return $"{base.ToString()} size: {this.size} HashName: {this.HashName} email: {this.email} Source file: {this.source_file} unknown2: {this.unknown2}{(this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "")}";
        }

    }
}
