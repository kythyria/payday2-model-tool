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
        public const uint animation_data_tag = 0x5DC011B8; // Animation data
        public const uint author_tag = 0x7623C465; // Author tag
        public const uint material_group_tag = 0x29276B1D; // Material Group
        public const uint material_tag = 0x3C54609C; // Material
        public const uint object3D_tag = 0x0FFCD100; // Object3D
        public const uint model_data_tag = 0x62212D88; // Model data
        public const uint geometry_tag = 0x7AB072D3; // Geometry
        public const uint topology_tag = 0x4C507A13; // Topology
        public const uint passthroughGP_tag = 0xE3A3B1CA; // PassthroughGP
        public const uint topologyIP_tag = 0x03B634BD; // TopologyIP
        public const uint quatLinearRotationController_tag = 0x648A206C; // QuatLinearRotationController
        public const uint quatBezRotationController_tag = 0x197345A5; // QuatBezRotationController
        public const uint skinbones_tag = 0x65CC1825; // SkinBones
        public const uint bones_tag = 0xEB43C77; // Bones
        public const uint light_tag = 0xFFA13B80; //Light
        public const uint lightSet_tag = 0x33552583; //LightSet
        public const uint linearVector3Controller_tag = 0x26A5128C; //LinearVector3Controller
        public const uint linearFloatController_tag = 0x76BF5B66; //LinearFloatController
        public const uint lookAtConstrRotationController = 0x679D695B; //LookAtConstrRotationController
        public const uint camera_tag = 0x46BF31A7; //Camera

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

        public static System.Numerics.Matrix4x4 ToMatrix4x4(this Matrix3D input)
        {
            return new System.Numerics.Matrix4x4(
                input.M11, input.M12, input.M13, input.M14,
                input.M21, input.M22, input.M23, input.M24,
                input.M31, input.M32, input.M33, input.M34,
                input.M41, input.M42, input.M43, input.M44
            );
        }

        public static Matrix3D ToNexusMatrix(this System.Numerics.Matrix4x4 input)
        {
            return new Matrix3D(
                input.M11, input.M12, input.M13, input.M14,
                input.M21, input.M22, input.M23, input.M24,
                input.M31, input.M32, input.M33, input.M34,
                input.M41, input.M42, input.M43, input.M44
            );
        }

        public static System.Numerics.Vector2 ToVector2(this Vector2D input) => new System.Numerics.Vector2(input.X, input.Y);
        public static System.Numerics.Vector3 ToVector3(this Vector3D input) => new System.Numerics.Vector3(input.X, input.Y, input.Z);
        public static System.Numerics.Vector4 ToVector4(this Sections.GeometryColor input) => new System.Numerics.Vector4(input.red/255.0f, input.green/255.0f, input.blue/255.0f, input.alpha/255.0f);

        public static Vector2D ToNexusVector(this System.Numerics.Vector2 input) => new Vector2D(input.X, input.Y);
        public static Vector3D ToNexusVector(this System.Numerics.Vector3 input) => new Vector3D(input.X, input.Y, input.Z);
        public static Sections.GeometryColor ToGeometryColor(this System.Numerics.Vector4 input) {
            return new Sections.GeometryColor(
                ClampFloatToByte(input.X),
                ClampFloatToByte(input.Y),
                ClampFloatToByte(input.Z),
                ClampFloatToByte(input.W));
        }
        public static Quaternion ToNexusQuaternion(this System.Numerics.Quaternion input) => new Quaternion(input.X, input.Y, input.Z, input.W);
        public static System.Numerics.Quaternion ToQuaternion(this Quaternion input) => new System.Numerics.Quaternion(input.X, input.Y, input.Z, input.W);

        public static Vector3D Max(Vector3D left, Vector3D right) => new Vector3D(Math.Max(left.X, right.X), Math.Max(left.Y, right.Y), Math.Max(left.Z, right.Z));
        public static Vector3D Min(Vector3D left, Vector3D right) => new Vector3D(Math.Min(left.X, right.X), Math.Min(left.Y, right.Y), Math.Min(left.Z, right.Z));

        /// <summary>
        /// Clamp a float to 0..1 and rescale it to 0..255
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte ClampFloatToByte(double input)
        {
            var clamped = input > 1.0f ? 1.0f : input;
            var scaled = input * 255;
            var rounded = Math.Round(scaled);
            return (byte)rounded;
        }

        public static Boolean IsFinite(this System.Numerics.Vector3 vec)
        {
            return !(
                float.IsInfinity(vec.X)
                || float.IsInfinity(vec.Y)
                || float.IsInfinity(vec.Z)
                || float.IsNaN(vec.X)
                || float.IsNaN(vec.Y)
                || float.IsNaN(vec.Z)
            );
        }

        // From https://github.com/KhronosGroup/glTF-Validator/blob/master/lib/src/errors.dart
        // which says, "these values are slightly greater than the maximum error from signed 8-bit quantization"
        const float UnitLengthThresholdVec3 = 0.00674f;
        const float UnitLengthThresholdVec4 = 0.00769f;

        public static Boolean IsUnitLength(this System.Numerics.Vector3 vec) =>
            Math.Abs(vec.Length() - 1) <= UnitLengthThresholdVec3;
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
    }

    [System.ComponentModel.TypeConverter(typeof(Inspector.HashNameConverter))]
    public class HashName
    {
        private ulong hash;
        private string str;
        private bool known;
    
        [System.ComponentModel.NotifyParentProperty(true)]
        public string String
        {
            get => str ?? StaticStorage.hashindex.GetString(hash);
            set
            {
                str = value ?? throw new ArgumentNullException(nameof(value));
                hash = Hash64.HashString(value);
            }
        }

        [System.ComponentModel.NotifyParentProperty(true)]
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

    static class NullableUtil
    {
        public static R WithValue<T,R>(this T? self, Func<T,R> cb) where T : struct
        {
            if(self.HasValue)
            {
                return cb(self.Value);
            }
            else
            {
                return default(R);
            }
        }

        public static void WithValue<T>(this T? self, Action<T> cb) where T: struct
        {
            if (self.HasValue) { cb(self.Value); }
        }
    }

    static class EnumerableUtil
    {
        public static IEnumerable<T> OrderedDistinct<T>(this IEnumerable<T> self)
        {
            var set = new HashSet<T>();
            foreach(var i in self)
            {
                if(set.Add(i))
                {
                    yield return i;
                    set.Add(i);
                }
            }
        }
    }
}
