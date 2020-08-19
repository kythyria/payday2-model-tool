using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PD2ModelParser.Misc
{
    static class XmlUtil
    {
        public static XmlElement Element(this XmlNode elem, string name, Action<XmlElement> cb)
        {
            var od = elem.OwnerDocument ?? (XmlDocument)elem;
            var el = od.CreateElement(name);
            elem.AppendChild(el);
            if (cb != null)
            {
                cb(el);
            }
            return el;
        }

        public static XmlElement Element(this XmlNode elem, string name, string contents = null)
        {
            return elem.Element(name, el =>
            {
                if (contents != null)
                    el.AppendChild(el.OwnerDocument.CreateTextNode(contents));
            });
        }

        public static void Elements(this XmlNode elem, string name, params object[] things)
        {
            foreach(var thing in things)
            {
                elem.Element(name, thing.ToString());
            }
        }
    }
}
