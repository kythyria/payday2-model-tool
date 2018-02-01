using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    class Author
    {
        private static uint author_tag = 0x7623C465; // Author tag
        public UInt32 id;
        public UInt32 size;

        public UInt64 hashname; //Hashed name (see hashlist.txt)
        public String email; //Author's email address
        public String source_file; //Source model file
        public UInt32 unknown2;

        public byte[] remaining_data = null;
        

        public Author(BinaryReader instream, SectionHeader section)
        {
            this.id = section.id;
            this.size = section.size;
            this.hashname = instream.ReadUInt64();

            StringBuilder email_sb = new StringBuilder();
            int buf;
            while ((buf = instream.ReadByte()) != 0)
                email_sb.Append((char)buf);
            this.email = email_sb.ToString();

            StringBuilder source_file_sb = new StringBuilder();
            while ((buf = instream.ReadByte()) != 0)
                source_file_sb.Append((char)buf);
            this.source_file = source_file_sb.ToString();

            this.unknown2 = instream.ReadUInt32();

            this.remaining_data = null;
            if ((section.offset + 12 + section.size) > instream.BaseStream.Position)
                this.remaining_data = instream.ReadBytes((int)((section.offset + 12 + section.size) - instream.BaseStream.Position));
        }

        public void StreamWrite(BinaryWriter outstream)
        {
            outstream.Write(author_tag);
            outstream.Write(this.id);
            long newsizestart = outstream.BaseStream.Position;
            outstream.Write(this.size);

            this.StreamWriteData(outstream);

            //update section size
            long newsizeend = outstream.BaseStream.Position;
            outstream.BaseStream.Position = newsizestart;
            outstream.Write((uint)(newsizeend - (newsizestart + 4)));

            outstream.BaseStream.Position = newsizeend;
        }

        public void StreamWriteData(BinaryWriter outstream)
        {
            Byte zero = 0;
            outstream.Write(this.hashname);
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
            return "[Author] ID: " + this.id + " size: " + this.size + " hashname: " + StaticStorage.hashindex.GetString( this.hashname ) + " email: " + this.email + " Source file: " + this.source_file + " unknown2: " + this.unknown2 + (this.remaining_data != null ? " REMAINING DATA! " + this.remaining_data.Length + " bytes" : "");
        }

    }
}
