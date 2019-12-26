using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PD2ModelParser.Inspector
{
    class NexusMatrixConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string) || !(value is Nexus.Matrix3D matrix))
                return base.ConvertTo(context, culture, value, destinationType);

            if(matrix.Decompose(out var scale, out var rotation, out var translation))
            {
                return $"T:{translation.ToString()}, R:{rotation.ToString()}, S:{scale.ToString()}";
            }

            return $"({matrix.M11} {matrix.M12} {matrix.M13} {matrix.M14}) " +
                   $"({matrix.M21} {matrix.M22} {matrix.M23} {matrix.M24}) " +
                   $"({matrix.M31} {matrix.M32} {matrix.M33} {matrix.M34}) " +
                   $"({matrix.M41} {matrix.M42} {matrix.M43} {matrix.M44})";
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var affine = SharpGLTF.Transforms.AffineTransform.Create(((Nexus.Matrix3D)value).ToMatrix4x4());

            if (affine.IsValid)
            {
                return new PropertyDescriptorCollection(new PropertyDescriptor[]
                {
                    new TranslationProperty(),
                    new RotationProperty(),
                    new ScaleProperty()
                });
            }
            else
            {
                return base.GetProperties(context, value, attributes);
            }
        }

        class TranslationProperty : SimplePropertyDescriptor
        {
            public TranslationProperty() : base(typeof(Nexus.Matrix3D), "Translation", typeof(Nexus.Vector3D)) { }
            public override object GetValue(object component)
            {
                var affine = SharpGLTF.Transforms.AffineTransform.Create(((Nexus.Matrix3D)component).ToMatrix4x4());
                return affine.Translation.ToNexusVector();
            }
            public override void SetValue(object component, object value)
            {
                throw new NotImplementedException();
            }
            public override TypeConverter Converter => new NexusVector3DConverter();
        }

        class RotationProperty : SimplePropertyDescriptor
        {
            public RotationProperty() : base(typeof(Nexus.Matrix3D), "Rotation", typeof(Nexus.Quaternion)) { }
            public override object GetValue(object component)
            {
                var affine = SharpGLTF.Transforms.AffineTransform.Create(((Nexus.Matrix3D)component).ToMatrix4x4());
                return affine.Rotation.ToNexusQuaternion();
            }
            public override void SetValue(object component, object value)
            {
                throw new NotImplementedException();
            }
            public override TypeConverter Converter => new NexusQuaternionConverter();
        }

        class ScaleProperty : SimplePropertyDescriptor
        {
            public ScaleProperty() : base(typeof(Nexus.Matrix3D), "Scale", typeof(Nexus.Vector3D)) { }
            public override object GetValue(object component)
            {
                var affine = SharpGLTF.Transforms.AffineTransform.Create(((Nexus.Matrix3D)component).ToMatrix4x4());
                return affine.Scale.ToNexusVector();
            }
            public override void SetValue(object component, object value)
            {
                throw new NotImplementedException();
            }
            public override TypeConverter Converter => new NexusVector3DConverter();
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => true;
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");

            if(propertyValues.Contains("Scale") && propertyValues.Contains("Rotation") && propertyValues.Contains("Translation"))
            {
                var affine = new SharpGLTF.Transforms.AffineTransform();
                affine.Rotation = ((Nexus.Quaternion)propertyValues["Rotation"]).ToQuaternion();
                affine.Translation = ((Nexus.Vector3D)propertyValues["Translation"]).ToVector3();
                affine.Scale = ((Nexus.Vector3D)propertyValues["Scale"]).ToVector3();
                return affine.Matrix.ToNexusMatrix();
            }
            else
            {
                throw new ArgumentException("Affine transform parameters missing");
            }
        }
    }

    class StructFieldProperty<TObject, TField> : PropertyDescriptor
    {
        public StructFieldProperty(string name) : base(name, new Attribute[] { }) { }

        public override Type ComponentType => typeof(TObject);
        public override Type PropertyType => typeof(TField);
        public override bool IsReadOnly => typeof(TObject).GetField(Name).Attributes.HasFlag(System.Reflection.FieldAttributes.InitOnly);

        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) => throw new NotImplementedException();

        public override bool ShouldSerializeValue(object component) => true;

        public override void SetValue(object component, object value)
        {
            var fi = typeof(TObject).GetField(Name);
            fi.SetValue(component, Convert.ChangeType(value, fi.FieldType));
        }
        public override object GetValue(object component)
        {
            var fi = typeof(TObject).GetField(Name);
            return fi.GetValue(component);
        }
    }

    class NexusVector3DConverter : ExpandableObjectConverter
    {

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => true;
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");

            var result = new Nexus.Vector3D();
            if (propertyValues.Contains("X")) { result.X = (float)Convert.ChangeType(propertyValues["X"], typeof(float)); }
            if (propertyValues.Contains("Y")) { result.Y = (float)Convert.ChangeType(propertyValues["Y"], typeof(float)); }
            if (propertyValues.Contains("Z")) { result.Z = (float)Convert.ChangeType(propertyValues["Z"], typeof(float)); }

            object boxed = result;
            return (Nexus.Vector3D)boxed;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(new PropertyDescriptor[]
            {
                new StructFieldProperty<Nexus.Vector3D, float>("X"),
                new StructFieldProperty<Nexus.Vector3D, float>("Y"),
                new StructFieldProperty<Nexus.Vector3D, float>("Z")
            });
        }

        private Regex vec3Parser = new Regex(@"\{?(?:X:)?([-+E\.0-9]+) *,? *(?:Y:)?([-+E\.0-9]+) *,? *(?:Z:)?([-+E\.0-9]+)\}?", RegexOptions.IgnoreCase);
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() != typeof(string))
                return base.ConvertFrom(context, culture, value);

            var match = vec3Parser.Match((string)value);
            if (!match.Success) return null;

            if (   float.TryParse(match.Groups[1].Value, out float X)
                && float.TryParse(match.Groups[2].Value, out float Y)
                && float.TryParse(match.Groups[3].Value, out float Z))
            {
                return new Nexus.Vector3D(X, Y, Z);
            }
            else
            {
                return null;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string) || base.CanConvertFrom(context, destinationType);
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string) || !(value is Nexus.Vector3D vec))
                return base.ConvertTo(context, culture, value, destinationType);

            return $"{{X:{vec.X.ToString("G9")} Y:{vec.Y.ToString("G9")} Z:{vec.Z.ToString("G9")}}}";
        }
    }

    class NexusQuaternionConverter : ExpandableObjectConverter
    {

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => true;
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");

            var result = new Nexus.Vector3D();
            if (propertyValues.Contains("X")) { result.X = (float)Convert.ChangeType(propertyValues["X"], typeof(float)); }
            if (propertyValues.Contains("Y")) { result.Y = (float)Convert.ChangeType(propertyValues["Y"], typeof(float)); }
            if (propertyValues.Contains("Z")) { result.Z = (float)Convert.ChangeType(propertyValues["Z"], typeof(float)); }
            if (propertyValues.Contains("W")) { result.Z = (float)Convert.ChangeType(propertyValues["W"], typeof(float)); }

            object boxed = result;
            return (Nexus.Vector3D)boxed;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(new PropertyDescriptor[]
            {
                new StructFieldProperty<Nexus.Quaternion, float>("X"),
                new StructFieldProperty<Nexus.Quaternion, float>("Y"),
                new StructFieldProperty<Nexus.Quaternion, float>("Z"),
                new StructFieldProperty<Nexus.Quaternion, float>("W")
            });
        }

        private Regex vec3Parser = new Regex(@"\{?(?:X:)?([-+E\.0-9]+) *,? *(?:Y:)?([-+E\.0-9]+) *,? *(?:Z:)?([-+E\.0-9]+) *,? *(?:W:)?([-+E\.0-9]+)\}?", RegexOptions.IgnoreCase);
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() != typeof(string))
                return base.ConvertFrom(context, culture, value);

            var match = vec3Parser.Match((string)value);
            if (!match.Success) return null;

            if (float.TryParse(match.Groups[1].Value, out float X)
                && float.TryParse(match.Groups[2].Value, out float Y)
                && float.TryParse(match.Groups[3].Value, out float Z)
                && float.TryParse(match.Groups[3].Value, out float W))
            {
                return new Nexus.Vector3D(X, Y, Z);
            }
            else
            {
                return null;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string) || base.CanConvertFrom(context, destinationType);
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string) || !(value is Nexus.Quaternion vec))
                return base.ConvertTo(context, culture, value, destinationType);

            return $"{{X:{vec.X.ToString("G9")} Y:{vec.Y.ToString("G9")} Z:{vec.Z.ToString("G9")} W:{vec.W.ToString("G9")}}}";
        }
    }

    class StructConverter<T> : ExpandableObjectConverter
    {
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => true;
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");

            T ret = default(T);
            object boxed = ret;
            foreach (DictionaryEntry entry in propertyValues)
            {
                System.Reflection.PropertyInfo pi = ret.GetType().GetProperty(entry.Key.ToString());
                if (pi != null && pi.CanWrite)
                {
                    pi.SetValue(boxed, Convert.ChangeType(entry.Value, pi.PropertyType), null);
                }
            }
            return (T)boxed;
        }
    }
}
