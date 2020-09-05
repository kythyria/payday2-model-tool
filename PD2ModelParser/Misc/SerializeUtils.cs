namespace PD2ModelParser
{
    static class SerializeUtils
    {
        static public System.Numerics.Vector3 ReadVector3(this System.IO.BinaryReader self)
            => new System.Numerics.Vector3(self.ReadSingle(), self.ReadSingle(), self.ReadSingle());

        static public System.Numerics.Quaternion ReadQuaternion(this System.IO.BinaryReader self)
            => new System.Numerics.Quaternion(self.ReadSingle(), self.ReadSingle(), self.ReadSingle(), self.ReadSingle());

        static public void Write(this System.IO.BinaryWriter self, System.Numerics.Vector3 vec)
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
