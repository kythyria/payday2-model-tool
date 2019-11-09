using System;
using FbxNet;
using Nexus;
using PD2ModelParser.Sections;

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

        public static Vector3D V3(this FbxDouble3 vec)
        {
            SWIGTYPE_p_double data = vec.mData;
            double x = FbxNet.FbxNet.doubleArray_getitem(data, 0);
            double y = FbxNet.FbxNet.doubleArray_getitem(data, 1);
            double z = FbxNet.FbxNet.doubleArray_getitem(data, 2);
            return new Vector3D((float) x, (float) y, (float) z);
        }

        public static Vector3D V3(this FbxVector4 vec)
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

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public static Quaternion EulerToQuaternionXYZ(this Vector3D v)
        {
            Quaternion roll = Quaternion.CreateFromAxisAngle(Vector3D.UnitX, v.X);
            Quaternion pitch = Quaternion.CreateFromAxisAngle(Vector3D.UnitY, v.Y);
            Quaternion yaw = Quaternion.CreateFromAxisAngle(Vector3D.UnitZ, v.Z);

            return yaw * pitch * roll;
        }

        public static FbxDouble3 ToFbxD3(this Vector3D v) => new FbxDouble3(v.X, v.Y, v.Z);

        public static FbxVector2 ToFbxV2(this Vector2D v) => new FbxVector2(v.X, v.Y);

        public static GeometryColor ToGeomColour(this FbxColor c)
        {
            return new GeometryColor
            {
                red = (byte) (c.mRed * 255),
                green = (byte) (c.mGreen * 255),
                blue = (byte) (c.mBlue * 255),
                alpha = (byte) (c.mAlpha * 255),
            };
        }

        public static Matrix3D GetNexusTransform(this FbxNode node)
        {
            // TODO do we need to support other rotation orders? They all seem to come out as XYZ
            if (node.RotationOrder.Get() != FbxEuler.EOrder.eOrderXYZ)
                throw new Exception("Currently only the XYZ rotation order is supported");

            Vector3D radians_euler = node.LclRotation.Get().V3() / 180 * (float) Math.PI;
            Quaternion rotation = radians_euler.EulerToQuaternionXYZ();

            Matrix3D transform = Matrix3D.CreateFromQuaternion(rotation);
            transform.Translation = node.LclTranslation.Get().V3();
            return transform;
        }
    }
}
