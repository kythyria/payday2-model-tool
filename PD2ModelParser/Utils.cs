using Nexus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    static class Tags
    {
        public static uint animation_data_tag = 0x5DC011B8; // Animation data
        public static uint author_tag = 0x7623C465; // Author tag
        public static uint material_group_tag = 0x29276B1D; // Material Group
        public static uint material_tag = 0x3C54609C; // Material
        public static uint object3D_tag = 0x0FFCD100; // Object3D
        public static uint model_data_tag = 0x62212D88; // Model data
        public static uint geometry_tag = 0x7AB072D3; // Geometry
        public static uint topology_tag = 0x4C507A13; // Topology
        public static uint passthroughGP_tag = 0xE3A3B1CA; // PassthroughGP
        public static uint topologyIP_tag = 0x03B634BD;  // TopologyIP
        public static uint quatLinearRotationController_tag = 0x648A206C; // QuatLinearRotationController
        public static uint quatBezRotationController_tag = 0x197345A5; // QuatBezRotationController
        public static uint skinbones_tag = 0x65CC1825; // SkinBones
        public static uint bones_tag = 0xEB43C77; // Bones
        public static uint light_tag = 0xFFA13B80; //Light
        public static uint lightSet_tag = 0x33552583; //LightSet
        public static uint linearVector3Controller_tag = 0x26A5128C; //LinearVector3Controller
        public static uint linearFloatController_tag = 0x76BF5B66; //LinearFloatController
        public static uint lookAtConstrRotationController = 0x679D695B; //LookAtConstrRotationController
        public static uint camera_tag = 0x46BF31A7; //Camera
    }

    static class MathUtil
    {
        public static double[] Serialize(Matrix3D matrix)
        {
            return new double[]
            {
                matrix.M11, matrix.M21, matrix.M31, matrix.M41,
                matrix.M12, matrix.M22, matrix.M32, matrix.M42,
                matrix.M13, matrix.M23, matrix.M33, matrix.M43,
                matrix.M14, matrix.M24, matrix.M34, matrix.M44,
            };
        }


        public static Matrix3D ReadMatrix(BinaryReader instream)
        {
            Matrix3D m;

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

        public static void WriteMatrix(BinaryWriter outstream, Matrix3D matrix)
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
