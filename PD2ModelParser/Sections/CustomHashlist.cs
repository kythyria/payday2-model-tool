using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PD2ModelParser.Sections
{
    public class CustomHashlist : AbstractSection, ISection
    {
        public override uint TypeCode => Tags.custom_hashlist_tag;

        public HashSet<string> Strings { get; } = new HashSet<string>();

        public CustomHashlist()
        {
        }

        public CustomHashlist(BinaryReader br, SectionHeader sh)
        {
            ushort version = br.ReadUInt16();

            // The number of hash strings
            uint count = br.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                int length = br.ReadUInt16();

                if (br.BaseStream.Position + length > sh.End)
                {
                    Log.Default.Warn("Malformed hashlist, too long");
                    return;
                }

                byte[] bytes = br.ReadBytes(length);
                string str = Encoding.UTF8.GetString(bytes);
                StaticStorage.hashindex.Hint(str);
            }
        }

        public override void StreamWriteData(BinaryWriter output)
        {
            output.Write((ushort) 1);
            output.Write((uint) Strings.Count);

            foreach (string s in Strings)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                output.Write((ushort) bytes.Length);
                output.Write(bytes);
            }
        }

        public void Hint(HashName hashname)
        {
            if (!hashname.Known)
                return;

            Strings.Add(hashname.String);
        }
    }
}
