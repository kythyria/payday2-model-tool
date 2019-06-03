using System;
using System.Collections.Generic;

namespace PD2ModelParser.Sections
{
    public interface ISection
    {
        uint SectionId
        {
            get;
            set;
        }
    }

    public interface IPostLoadable
    {
        void PostLoad(uint id, Dictionary<uint, object> parsed_sections);
    }
}
