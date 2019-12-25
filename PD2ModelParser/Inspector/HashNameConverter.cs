using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;

namespace PD2ModelParser.Inspector
{
    class HashNameConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType != typeof(string) || !(value is HashName hn))
                return base.ConvertTo(context, culture, value, destinationType);

            return hn.String;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new HashName(value as string);
        }
    }
}
