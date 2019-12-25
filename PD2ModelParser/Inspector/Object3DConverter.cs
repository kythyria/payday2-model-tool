using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Inspector
{
    class AbstractSectionConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(new PropertyDescriptor[]
            {
                new PropId()
            });
        }

        private class PropId : SimplePropertyDescriptor 
        {
            public PropId() : base(typeof(Sections.AbstractSection), "SectionId", typeof(uint)) { }
            public override bool IsReadOnly => true;
            public override string DisplayName => "ID";
            public override string Category => "Section";
            public override string Description => "File-internal identifier for this section";
            public override void SetValue(object component, object value)
            {
                throw new NotImplementedException();
            }
            public override object GetValue(object component) => (component as Sections.AbstractSection).SectionId;
        }
    }
}
