using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PD2ModelParser.Inspector
{
    class Object3DReferenceConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string) || !(value is Sections.Object3D section))
                return base.ConvertTo(context, culture, value, destinationType);

            return section.HashName.String;
        }
    }
}
