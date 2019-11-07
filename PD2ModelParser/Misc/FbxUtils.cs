using System;
using FbxNet;
using Nexus;

namespace PD2ModelParser.Misc
{
    public static class FbxUtils
    {
        public static FbxDouble3 D3(this FbxDouble4 vec)
        {
            SWIGTYPE_p_double data = vec.mData;
            double x = FbxNet.FbxNet.doubleArray_getitem(data, 0);
            double y = FbxNet.FbxNet.doubleArray_getitem(data, 1);
            double z = FbxNet.FbxNet.doubleArray_getitem(data, 2);
            return new FbxDouble3(x, y, z);
        }

        public static Vector3D V3(this FbxDouble4 vec)
        {
            SWIGTYPE_p_double data = vec.mData;
            double x = FbxNet.FbxNet.doubleArray_getitem(data, 0);
            double y = FbxNet.FbxNet.doubleArray_getitem(data, 1);
            double z = FbxNet.FbxNet.doubleArray_getitem(data, 2);
            return new Vector3D((float) x, (float) y, (float) z);
        }

        public static void Set(this FbxVector4 v, Vector3D other) => v.Set(other.X, other.Y, other.Z, 0);

        // ReSharper disable once InconsistentNaming
        public static Vector3D ToEulerZYX(this Quaternion q)
        {
            Vector3D angles;

            // roll (x-axis rotation)
            double sinr_cosp = +2.0 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float) Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float) Math.PI / 2 * (sinp > 0 ? 1 : -1); // use 90 degrees if out of range
            else
                angles.Y = (float) Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float) Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public static FbxDouble3 ToFbxD3(this Vector3D v) => new FbxDouble3(v.X, v.Y, v.Z);

        public static FbxVector2 ToFbxV2(this Vector2D v) => new FbxVector2(v.X, v.Y);
    }
}
