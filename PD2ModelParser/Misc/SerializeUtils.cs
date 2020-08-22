namespace PD2ModelParser
{
    static class SerializeUtils
    {
        static public Nexus.Vector3D ReadNexusVector3D(this System.IO.BinaryReader self)
            => new Nexus.Vector3D(self.ReadSingle(), self.ReadSingle(), self.ReadSingle());

        static public Nexus.Quaternion ReadNexusQuaternion(this System.IO.BinaryReader self)
            => new Nexus.Quaternion(self.ReadSingle(), self.ReadSingle(), self.ReadSingle(), self.ReadSingle());

        static public void Write(this System.IO.BinaryWriter self, Nexus.Vector3D vec)
        {
            self.Write(vec.X);
            self.Write(vec.Y);
            self.Write(vec.Z);
        }

        //TODO: What encoding are these? This only actually works for ASCII-7bit
        static public string ReadCString(this System.IO.BinaryReader self)
        {
            var sb = new System.Text.StringBuilder();
            int buf;
            while ((buf = self.ReadByte()) != 0)
                sb.Append((char)buf);
            return sb.ToString();
        }
    }
}
