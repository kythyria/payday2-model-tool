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

        public static System.Numerics.Matrix4x4 ReadMatrix(this System.IO.BinaryReader instream)
        {
            System.Numerics.Matrix4x4 m;

            // Yes, the matricies appear to be written top-down in colums, this isn't the field names being wrong
            // This is how a multidimensional array is layed out in memory.

            // First column
            m.M11 = instream.ReadSingle();
            m.M12 = instream.ReadSingle();
            m.M13 = instream.ReadSingle();
            m.M14 = instream.ReadSingle();

            // Second column
            m.M21 = instream.ReadSingle();
            m.M22 = instream.ReadSingle();
            m.M23 = instream.ReadSingle();
            m.M24 = instream.ReadSingle();

            // Third column
            m.M31 = instream.ReadSingle();
            m.M32 = instream.ReadSingle();
            m.M33 = instream.ReadSingle();
            m.M34 = instream.ReadSingle();

            // Fourth column
            m.M41 = instream.ReadSingle();
            m.M42 = instream.ReadSingle();
            m.M43 = instream.ReadSingle();
            m.M44 = instream.ReadSingle();

            return m;
        }

        public static void Write(this System.IO.BinaryWriter outstream, System.Numerics.Matrix4x4 matrix)
        {
            outstream.Write(matrix.M11);
            outstream.Write(matrix.M12);
            outstream.Write(matrix.M13);
            outstream.Write(matrix.M14);
            outstream.Write(matrix.M21);
            outstream.Write(matrix.M22);
            outstream.Write(matrix.M23);
            outstream.Write(matrix.M24);
            outstream.Write(matrix.M31);
            outstream.Write(matrix.M32);
            outstream.Write(matrix.M33);
            outstream.Write(matrix.M34);
            outstream.Write(matrix.M41);
            outstream.Write(matrix.M42);
            outstream.Write(matrix.M43);
            outstream.Write(matrix.M44);
        }
    }
}
