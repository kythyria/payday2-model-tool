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
        public static uint topologyIP_tag = 0x03B634BD; // TopologyIP
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

        // Custom tags - not used in vanilla PD2, but used here for whatever reason
        // To generate these on Linux, use xxd -l 4 /dev/random
        public const uint custom_hashlist_tag = 0x7c7844fd;
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

        public static string SerializeToString(Matrix3D m)
        {
            return string.Format(
                "{0} {1} {2} {3}\n" +
                "{4} {5} {6} {7}\n" +
                "{8} {9} {10} {11}\n" +
                "{12} {13} {14} {15}",
                m.M11, m.M21, m.M31, m.M41,
                m.M12, m.M22, m.M32, m.M42,
                m.M13, m.M23, m.M33, m.M43,
                m.M14, m.M24, m.M34, m.M44
            );
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

    public static class MatrixExtensions
    {
        /**
         * Multiply each field of two vectors together, same as
         * the SSE function _mm_mul_ps.
         */
        public static Vector4D MultEach(this Vector4D a, Vector4D b)
        {
            return new Vector4D
            {
                X = a.X * b.X,
                Y = a.Y * b.Y,
                Z = a.Z * b.Z,
                W = a.W * b.W
            };
        }

        /**
         * Return a vector with all four variables set to a single variable from this vector.
         *
         * For example, Vector4D(9, 8, 7, 6).DupedField(2) would return Vector4D(7, 7, 7, 7)
         *
         * This is very similar to SSE's _mm_shuffle_epi32
         */
        public static Vector4D DupedField(this Vector4D a, int field)
        {
            float v;

            switch (field)
            {
                case 0:
                    v = a.X;
                    break;
                case 1:
                    v = a.Y;
                    break;
                case 2:
                    v = a.Z;
                    break;
                case 3:
                    v = a.W;
                    break;
                default:
                    throw new ArgumentException("Illegal field " + field + " - must be 0-3 inclusive");
            }

            return new Vector4D
            {
                X = v,
                Y = v,
                Z = v,
                W = v
            };
        }

        /**
         * Get a single column from this matrix, expressed as a vector.
         *
         * Note: the order of the column placement is 0-1-2-3 into X-Y-Z-W (so
         * 'W' is the last not first value).
         */
        public static Vector4D GetColumn(this Matrix3D this_, int column)
        {
            if (column < 0 || column >= 4)
            {
                throw new ArgumentOutOfRangeException(
                    "Column must be between 0-3 inclusive (real value " + column + ")");
            }

            return new Vector4D
            {
                X = this_[0, column],
                Y = this_[1, column],
                Z = this_[2, column],
                W = this_[3, column]
            };
        }

        /**
         * Return a copy of this matrix with the specified column set
         * to a value. See GetColumn for more information.
         */
        public static Matrix3D WithColumn(this Matrix3D this_, int column, Vector4D value)
        {
            if (column < 0 || column >= 4)
            {
                throw new ArgumentOutOfRangeException(
                    "Column must be between 0-3 inclusive (real value " + column + ")");
            }

            this_[0, column] = value.X;
            this_[1, column] = value.Y;
            this_[2, column] = value.Z;
            this_[3, column] = value.W;

            return this_;
        }

        /**
         * Multiply two vectors the same way Diesel does. This is a bit confusing, see the decompiled
         * code in 'decompiled matrix parsing' and 'decompiled matrix parsing 2' (in the
         * Research Notes directory). This is much cleaned up code originally produced by HexRays
         * from dsl::SkinBones::post_load in the GNU+Linux binary.
         *
         * This has been tested against the PAYDAY 2 GNU+Linux binary, and in all cases produced the correct
         * results. To repeat this, I'd recommend breaking after the first _mm_load_si128 call in
         * dsl::SkinBones::post_load and reading $xmm0 through $xmm3 (or $rsi+0xE0 through $rsi+0x110), which
         * represent the output values of the multiplication (originally performed in dsl::Object3D::post_load).
         * The input values are stored at $rsi+0xA0 through $rsi+0xD0, and the others are stored in the parent object
         * pointed to by $rsi+0x130, whose values are at 0xE0 through 0x110.
         *
         * In any case, this was an awful lot of work.
         *
         * See https://github.com/blt4linux/research for the relevant headers.
         */
        public static Matrix3D MultDiesel(this Matrix3D a, Matrix3D b)
        {
            // TODO cleanup
            Matrix3D result = Matrix3D.Identity;

            for (int i = 0; i < 4; i++)
            {
                Vector4D bas = a.GetColumn(i);
                /*Vector4D outcol = new Vector4D {
                    X = bas[0] * b.GetColumn(0)[0] + bas[1] * b.GetColumn(1)[0] + bas[2] * b.GetColumn(2)[0],
                    Y = bas[0] * b.GetColumn(0)[1] + bas[1] * b.GetColumn(1)[1] + bas[2] * b.GetColumn(2)[1],
                    Z = bas[0] * b.GetColumn(0)[2] + bas[1] * b.GetColumn(1)[2] + bas[2] * b.GetColumn(2)[2],
                    W = 0
                };*/

                Vector4D outcol;

                if (i == 3)
                {
                    outcol =
                        bas.DupedField(2).MultEach(b.GetColumn(2)) +
                        bas.DupedField(1).MultEach(b.GetColumn(1)) +
                        bas.DupedField(0).MultEach(b.GetColumn(0)) +
                        b.GetColumn(3);

                    outcol.W = 1;
                }
                else
                {
                    /*outcol =
                        new Vector4D(bas[2], bas[2], bas[2], bas[2]).MultEach(b.GetColumn(2)) +
                        new Vector4D(bas[1], bas[1], bas[1], bas[1]).MultEach(b.GetColumn(1)) +
                        new Vector4D(bas[0], bas[0], bas[0], bas[0]).MultEach(b.GetColumn(0))
                        ;*/
                    outcol =
                        bas.DupedField(2).MultEach(b.GetColumn(2)) +
                        bas.DupedField(1).MultEach(b.GetColumn(1)) +
                        bas.DupedField(0).MultEach(b.GetColumn(0))
                        ;

                    outcol.W = 0;
                }

                result = result.WithColumn(i, outcol);
            }

            return result;

            // Old system:
            /*Matrix3D result = Matrix3D.Identity;

            for (int i = 0; i < 4; i++)
            {
                Vector4D vec = (a.GetColumn(0) * b[i, 2]) + (a.GetColumn(1) * b[i, 1]) + (a.GetColumn(2) * b[i, 0]);
                if(i == 3)
                {
                    // Add the transform bit
                    vec += a.GetColumn(3);
                }
                result = result.WithColumn(i, vec);
            }

            return result;*/
        }

        public static System.Numerics.Matrix4x4 ToMatrix4x4(this Matrix3D input)
        {
            return new System.Numerics.Matrix4x4(
                input.M11, input.M12, input.M13, input.M14,
                input.M21, input.M22, input.M23, input.M24,
                input.M31, input.M32, input.M33, input.M34,
                input.M41, input.M42, input.M43, input.M44
            );
        }
    }

    public class HashName
    {
        private ulong hash;
        private string str;
        private bool known;

        public string String
        {
            get => str ?? StaticStorage.hashindex.GetString(hash);
            set
            {
                str = value ?? throw new ArgumentNullException(nameof(value));
                hash = Hash64.HashString(value);
            }
        }

        public ulong Hash
        {
            get => hash;
            set
            {
                if (hash == value)
                    return;

                hash = value;
                str = null;
            }
        }

        public bool Known => known || StaticStorage.hashindex.Contains(Hash);

        public HashName(string str)
        {
            this.str = str ?? throw new ArgumentNullException(nameof(str));
            hash = Hash64.HashString(str);
            known = true;
        }

        public HashName(ulong hash)
        {
            this.hash = hash;
            str = null;
            known = false;
        }
    }
}
