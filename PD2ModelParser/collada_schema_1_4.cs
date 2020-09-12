//------------------------------------------------------------------------------
// Collada auto-generated model object from schema 1.4.1
// This file was patched manually in order to be able to use it
// see http://code4k.blogspot.com/2010/08/import-and-export-3d-collada-files-with.html for more information
// Author: @lx
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Collada141
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public partial class COLLADA
    {
        public asset asset { get; set; }

        [XmlElement("library_animation_clips", typeof(library_animation_clips))]
        [XmlElement("library_animations", typeof(library_animations))]
        [XmlElement("library_cameras", typeof(library_cameras))]
        [XmlElement("library_controllers", typeof(library_controllers))]
        [XmlElement("library_effects", typeof(library_effects))]
        [XmlElement("library_force_fields", typeof(library_force_fields))]
        [XmlElement("library_geometries", typeof(library_geometries))]
        [XmlElement("library_images", typeof(library_images))]
        [XmlElement("library_lights", typeof(library_lights))]
        [XmlElement("library_materials", typeof(library_materials))]
        [XmlElement("library_nodes", typeof(library_nodes))]
        [XmlElement("library_physics_materials", typeof(library_physics_materials))]
        [XmlElement("library_physics_models", typeof(library_physics_models))]
        [XmlElement("library_physics_scenes", typeof(library_physics_scenes))]
        [XmlElement("library_visual_scenes", typeof(library_visual_scenes))]
        public object[] Items { get; set; }

        public COLLADAScene scene { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute]
        public VersionType version { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class asset
    {
        [XmlElement("contributor")]
        public assetContributor[] contributor { get; set; }

        public DateTime created { get; set; }
        public string keywords { get; set; }
        public DateTime modified { get; set; }
        public string revision { get; set; }
        public string subject { get; set; }
        public string title { get; set; }
        public assetUnit unit { get; set; }

        [DefaultValue(UpAxisType.Y_UP)]
        public UpAxisType up_axis { get; set; } = UpAxisType.Y_UP;
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class assetContributor
    {
        public string author { get; set; }
        public string authoring_tool { get; set; }
        public string comments { get; set; }
        public string copyright { get; set; }

        [XmlElement(DataType = "anyURI")]
        public string source_data { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_from_common
    {
        [XmlAttribute]
        [DefaultValue(typeof(uint), "0")]
        public uint mip { get; set; } = 0;

        [XmlAttribute]
        [DefaultValue(typeof(uint), "0")]
        public uint slice { get; set; } = 0;

        [XmlAttribute]
        [DefaultValue(fx_surface_face_enum.POSITIVE_X)]
        public fx_surface_face_enum face { get; set; } = fx_surface_face_enum.POSITIVE_X;

        [XmlText(DataType = "IDREF")]
        public string Value { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_surface_face_enum
    {
        POSITIVE_X,
        NEGATIVE_X,
        POSITIVE_Y,
        NEGATIVE_Y,
        POSITIVE_Z,
        NEGATIVE_Z,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_newparam_type
    {
        [XmlElement(DataType = "NCName")]
        public string semantic { get; set; }

        [XmlElement("float", typeof(double))]
        [XmlElement("float2", typeof(double))]
        [XmlElement("float3", typeof(double))]
        [XmlElement("float4", typeof(double))]
        [XmlElement("sampler2D", typeof(fx_sampler2D_common))]
        [XmlElement("surface", typeof(fx_surface_common))]
        [XmlChoiceIdentifier("ItemElementName")]
        public object Item { get; set; }

        [XmlIgnore]
        public ItemChoiceType ItemElementName { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_sampler2D_common
    {
        [XmlElement(DataType = "NCName")]
        public string source { get; set; }

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_s { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_t { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common minfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common magfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common mipfilter { get; set; } = fx_sampler_filter_common.NONE;

        public string border_color { get; set; }

        [DefaultValue(typeof(byte), "255")]
        public byte mipmap_maxlevel { get; set; } = 255;

        [DefaultValue(typeof(float), "0")]
        public float mipmap_bias { get; set; } = 0;

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_sampler_wrap_common
    {
        NONE,
        WRAP,
        MIRROR,
        CLAMP,
        BORDER,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_sampler_filter_common
    {
        NONE,
        NEAREST,
        LINEAR,
        NEAREST_MIPMAP_NEAREST,
        LINEAR_MIPMAP_NEAREST,
        NEAREST_MIPMAP_LINEAR,
        LINEAR_MIPMAP_LINEAR,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class extra
    {
        public asset asset { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute(DataType = "NMTOKEN")]
        public string type { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class technique
    {
        [XmlAnyElement]
        public XmlElement[] Any { get; set; }

        [XmlAttribute(DataType = "NMTOKEN")]
        public string profile { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_common
    {
        public object init_as_null { get; set; }
        public object init_as_target { get; set; }
        public fx_surface_init_cube_common init_cube { get; set; }
        public fx_surface_init_volume_common init_volume { get; set; }
        public fx_surface_init_planar_common init_planar { get; set; }

        [XmlElement("init_from")]
        public fx_surface_init_from_common[] init_from { get; set; }

        [XmlElement(DataType = "token")]
        public string format { get; set; }

        public fx_surface_format_hint_common format_hint { get; set; }

        [XmlElement("size", typeof(long))]
        [XmlElement("viewport_ratio", typeof(double))]
        public object Item { get; set; }

        [DefaultValue(typeof(uint), "0")]
        public uint mip_levels { get; set; } = 0;

        public bool mipmap_generate { get; set; }

        [XmlIgnore]
        public bool mipmap_generateSpecified { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute]
        public fx_surface_type_enum type { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_cube_common
    {
        [XmlElement("all", typeof(fx_surface_init_cube_commonAll))]
        [XmlElement("face", typeof(fx_surface_init_cube_commonFace))]
        [XmlElement("primary", typeof(fx_surface_init_cube_commonPrimary))]
        public object[] Items { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_cube_commonAll
    {
        [XmlAttribute(DataType = "IDREF")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_cube_commonFace
    {
        [XmlAttribute(DataType = "IDREF")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_cube_commonPrimary
    {
        [XmlElement("order")]
        public fx_surface_face_enum[] order { get; set; }

        [XmlAttribute(DataType = "IDREF")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_volume_common
    {
        [XmlElement("all", typeof(fx_surface_init_volume_commonAll))]
        [XmlElement("primary", typeof(fx_surface_init_volume_commonPrimary))]
        public object Item { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_volume_commonAll
    {
        [XmlAttribute(DataType = "IDREF")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_volume_commonPrimary
    {
        [XmlAttribute(DataType = "IDREF")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_planar_common
    {
        [XmlElement("all")]
        public fx_surface_init_planar_commonAll Item { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_init_planar_commonAll
    {
        [XmlAttribute(DataType = "IDREF")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_surface_format_hint_common
    {
        public fx_surface_format_hint_channels_enum channels { get; set; }
        public fx_surface_format_hint_range_enum range { get; set; }
        public fx_surface_format_hint_precision_enum precision { get; set; }

        [XmlIgnore]
        public bool precisionSpecified { get; set; }

        [XmlElement("option")]
        public fx_surface_format_hint_option_enum[] option { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_surface_format_hint_channels_enum
    {
        RGB,
        RGBA,
        L,
        LA,
        D,
        XYZ,
        XYZW,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_surface_format_hint_range_enum
    {
        SNORM,
        UNORM,
        SINT,
        UINT,
        FLOAT,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_surface_format_hint_precision_enum
    {
        LOW,
        MID,
        HIGH,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_surface_format_hint_option_enum
    {
        SRGB_GAMMA,
        NORMALIZED3,
        NORMALIZED4,
        COMPRESSABLE,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_surface_type_enum
    {
        UNTYPED,
        [XmlEnum("1D")] Item1D,
        [XmlEnum("2D")] Item2D,
        [XmlEnum("3D")] Item3D,
        RECT,
        CUBE,
        DEPTH,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IncludeInSchema = false)]
    public enum ItemChoiceType
    {
        @float,
        float2,
        float3,
        float4,
        sampler2D,
        surface,
    }

    [XmlInclude(typeof(common_transparent_type))]
    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_color_or_texture_type
    {
        [XmlElement("color", typeof(common_color_or_texture_typeColor))]
        [XmlElement("param", typeof(common_color_or_texture_typeParam))]
        [XmlElement("texture", typeof(common_color_or_texture_typeTexture))]
        public object Item { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_color_or_texture_typeColor
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertDoubleArray(value);
        }

        [XmlIgnore]
        public double[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_color_or_texture_typeParam
    {
        [XmlAttribute(DataType = "NCName")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_color_or_texture_typeTexture
    {
        public extra extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string texture { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string texcoord { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_transparent_type : common_color_or_texture_type
    {
        [XmlAttribute]
        [DefaultValue(fx_opaque_enum.A_ONE)]
        public fx_opaque_enum opaque { get; set; } = fx_opaque_enum.A_ONE;
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_opaque_enum
    {
        A_ONE,
        RGB_ZERO,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_float_or_param_type
    {
        [XmlElement("float", typeof(common_float_or_param_typeFloat))]
        [XmlElement("param", typeof(common_float_or_param_typeParam))]
        public object Item { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_float_or_param_typeFloat
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public double Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class common_float_or_param_typeParam
    {
        [XmlAttribute(DataType = "NCName")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_include_common
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string url { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_samplerDEPTH_common
    {
        [XmlElement(DataType = "NCName")]
        public string source { get; set; }

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_s { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_t { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common minfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common magfilter { get; set; } = fx_sampler_filter_common.NONE;

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_samplerRECT_common
    {
        [XmlElement(DataType = "NCName")]
        public string source { get; set; }

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_s { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_t { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common minfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common magfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common mipfilter { get; set; } = fx_sampler_filter_common.NONE;

        public string border_color { get; set; }

        [DefaultValue(typeof(byte), "255")]
        public byte mipmap_maxlevel { get; set; } = 255;

        [DefaultValue(typeof(float), "0")]
        public float mipmap_bias { get; set; } = 0;

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_samplerCUBE_common
    {
        [XmlElement(DataType = "NCName")]
        public string source { get; set; }

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_s { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_t { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_p { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common minfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common magfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common mipfilter { get; set; } = fx_sampler_filter_common.NONE;

        public string border_color { get; set; }

        [DefaultValue(typeof(byte), "255")]
        public byte mipmap_maxlevel { get; set; } = 255;

        [DefaultValue(typeof(float), "0")]
        public float mipmap_bias { get; set; } = 0;

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_sampler3D_common
    {
        [XmlElement(DataType = "NCName")]
        public string source { get; set; }

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_s { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_t { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_p { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common minfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common magfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common mipfilter { get; set; } = fx_sampler_filter_common.NONE;

        public string border_color { get; set; }

        [DefaultValue(typeof(byte), "255")]
        public byte mipmap_maxlevel { get; set; } = 255;

        [DefaultValue(typeof(float), "0")]
        public float mipmap_bias { get; set; } = 0;

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_sampler1D_common
    {
        [XmlElement(DataType = "NCName")]
        public string source { get; set; }

        [DefaultValue(fx_sampler_wrap_common.WRAP)]
        public fx_sampler_wrap_common wrap_s { get; set; } = fx_sampler_wrap_common.WRAP;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common minfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common magfilter { get; set; } = fx_sampler_filter_common.NONE;

        [DefaultValue(fx_sampler_filter_common.NONE)]
        public fx_sampler_filter_common mipfilter { get; set; } = fx_sampler_filter_common.NONE;

        public string border_color { get; set; }

        [DefaultValue(typeof(byte), "0")]
        public byte mipmap_maxlevel { get; set; } = 0;

        [DefaultValue(typeof(float), "0")]
        public float mipmap_bias { get; set; } = 0;

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class InputGlobal
    {
        [XmlAttribute(DataType = "NMTOKEN")]
        public string semantic { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string source { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_newparam_common
    {
        [XmlElement("annotate")]
        public fx_annotate_common[] annotate { get; set; }

        [XmlElement(DataType = "NCName")]
        public string semantic { get; set; }

        public fx_modifier_enum_common modifier { get; set; }

        [XmlIgnore]
        public bool modifierSpecified { get; set; }

        public bool @bool { get; set; }
        public string bool2 { get; set; }
        public string bool3 { get; set; }
        public string bool4 { get; set; }
        public long @int { get; set; }
        public string int2 { get; set; }
        public string int3 { get; set; }
        public string int4 { get; set; }
        public double @float { get; set; }
        public string float2 { get; set; }
        public string float3 { get; set; }
        public string float4 { get; set; }
        public double float1x1 { get; set; }
        public string float1x2 { get; set; }
        public string float1x3 { get; set; }
        public string float1x4 { get; set; }
        public string float2x1 { get; set; }
        public string float2x2 { get; set; }
        public string float2x3 { get; set; }
        public string float2x4 { get; set; }
        public string float3x1 { get; set; }
        public string float3x2 { get; set; }
        public string float3x3 { get; set; }
        public string float3x4 { get; set; }
        public string float4x1 { get; set; }
        public string float4x2 { get; set; }
        public string float4x3 { get; set; }
        public string float4x4 { get; set; }
        public fx_surface_common surface { get; set; }
        public fx_sampler1D_common sampler1D { get; set; }
        public fx_sampler2D_common sampler2D { get; set; }
        public fx_sampler3D_common sampler3D { get; set; }
        public fx_samplerCUBE_common samplerCUBE { get; set; }
        public fx_samplerRECT_common samplerRECT { get; set; }
        public fx_samplerDEPTH_common samplerDEPTH { get; set; }
        public string @enum { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class fx_annotate_common
    {
        public bool @bool { get; set; }
        public string bool2 { get; set; }
        public string bool3 { get; set; }
        public string bool4 { get; set; }
        public long @int { get; set; }
        public string int2 { get; set; }
        public string int3 { get; set; }
        public string int4 { get; set; }
        public double @float { get; set; }
        public string float2 { get; set; }
        public string float3 { get; set; }
        public string float4 { get; set; }
        public string float2x2 { get; set; }
        public string float3x3 { get; set; }
        public string float4x4 { get; set; }
        public string @string { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum fx_modifier_enum_common
    {
        CONST,
        UNIFORM,
        VARYING,
        STATIC,
        VOLATILE,
        EXTERN,
        SHARED,
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class InputLocalOffset
    {
        [XmlAttribute]
        public ulong offset { get; set; }

        [XmlAttribute(DataType = "NMTOKEN")]
        public string semantic { get; set; }

        [XmlAttribute]
        public string source { get; set; }

        [XmlAttribute]
        public ulong set { get; set; }

        [XmlIgnore]
        public bool setSpecified { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class TargetableFloat
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public double Value { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class InputLocal
    {
        [XmlAttribute(DataType = "NMTOKEN")]
        public string semantic { get; set; }

        [XmlAttribute]
        public string source { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class instance_material
    {
        [XmlElement("bind")]
        public instance_materialBind[] bind { get; set; }

        [XmlElement("bind_vertex_input")]
        public instance_materialBind_vertex_input[] bind_vertex_input { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string symbol { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string target { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_materialBind
    {
        [XmlAttribute(DataType = "NCName")]
        public string semantic { get; set; }

        [XmlAttribute(DataType = "token")]
        public string target { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_materialBind_vertex_input
    {
        [XmlAttribute(DataType = "NCName")]
        public string semantic { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string input_semantic { get; set; }

        [XmlAttribute]
        public ulong input_set { get; set; }

        [XmlIgnore]
        public bool input_setSpecified { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class assetUnit
    {
        [XmlAttribute]
        [DefaultValue(1D)]
        public double meter { get; set; } = 1D;

        [XmlAttribute(DataType = "NMTOKEN")]
        [DefaultValue("meter")]
        public string name { get; set; } = "meter";
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum UpAxisType
    {
        X_UP,
        Y_UP,
        Z_UP,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_animation_clips
    {
        public asset asset { get; set; }

        [XmlElement("animation_clip")]
        public animation_clip[] animation_clip { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class animation_clip
    {
        public asset asset { get; set; }

        [XmlElement("instance_animation")]
        public InstanceWithExtra[] instance_animation { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        [DefaultValue(0D)]
        public double start { get; set; } = 0D;

        [XmlAttribute]
        public double end { get; set; }

        [XmlIgnore]
        public bool endSpecified { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot("instance_camera", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class InstanceWithExtra
    {
        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string url { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_animations
    {
        public asset asset { get; set; }

        [XmlElement("animation")]
        public animation[] animation { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class animation
    {
        public asset asset { get; set; }

        [XmlElement("animation", typeof(animation))]
        [XmlElement("channel", typeof(channel))]
        [XmlElement("sampler", typeof(sampler))]
        [XmlElement("source", typeof(source))]
        public object[] Items { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class channel
    {
        [XmlAttribute]
        public string source { get; set; }

        [XmlAttribute(DataType = "token")]
        public string target { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class sampler
    {
        [XmlElement("input")]
        public InputLocal[] input { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class source
    {
        public asset asset { get; set; }

        [XmlElement("IDREF_array", typeof(IDREF_array))]
        [XmlElement("Name_array", typeof(Name_array))]
        [XmlElement("bool_array", typeof(bool_array))]
        [XmlElement("float_array", typeof(float_array))]
        [XmlElement("int_array", typeof(int_array))]
        public object Item { get; set; }

        public sourceTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class IDREF_array
    {
        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlText(DataType = "IDREFS")]
        public string Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class Name_array
    {
        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlElement("Name")]
        public string _Text_
        {
            get { return COLLADA.ConvertFromArray(Values); }

            set { Values = COLLADA.ConvertStringArray(value); }
        }

        [XmlIgnore]
        public string[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class bool_array
    {
        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertBoolArray(value);
        }

        [XmlIgnore]
        public bool[] Values { get; set; }
    }

    [Serializable]
    //[DebuggerStepThrough]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class float_array
    {
        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute]
        [DefaultValue(typeof(short), "6")]
        public short digits { get; set; } = 6;

        [XmlAttribute]
        [DefaultValue(typeof(short), "38")]
        public short magnitude { get; set; } = 38;

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertDoubleArray(value);
        }

        [XmlIgnore]
        public double[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class int_array
    {
        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "integer")]
        [DefaultValue("-2147483648")]
        public string minInclusive { get; set; } = "-2147483648";

        [XmlAttribute(DataType = "integer")]
        [DefaultValue("2147483647")]
        public string maxInclusive { get; set; } = "2147483647";

        [XmlText]
        public string _Text_
        {
            get { return COLLADA.ConvertFromArray(Values); }

            set { Values = COLLADA.ConvertIntArray(value); }
        }

        [XmlIgnore]
        public int[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class sourceTechnique_common
    {
        public accessor accessor { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class accessor
    {
        [XmlElement("param")]
        public param[] param { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute]
        [DefaultValue(typeof(ulong), "0")]
        public ulong offset { get; set; } = 0;

        [XmlAttribute(DataType = "anyURI")]
        public string source { get; set; }

        [XmlAttribute]
        [DefaultValue(typeof(ulong), "1")]
        public ulong stride { get; set; } = 1;
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class param
    {
        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NMTOKEN")]
        public string semantic { get; set; }

        [XmlAttribute(DataType = "NMTOKEN")]
        public string type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_cameras
    {
        public asset asset { get; set; }

        [XmlElement("camera")]
        public camera[] camera { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class camera
    {
        public asset asset { get; set; }
        public cameraOptics optics { get; set; }
        public cameraImager imager { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class cameraOptics
    {
        public cameraOpticsTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class cameraOpticsTechnique_common
    {
        [XmlElement("orthographic", typeof(cameraOpticsTechnique_commonOrthographic))]
        [XmlElement("perspective", typeof(cameraOpticsTechnique_commonPerspective))]
        public object Item { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class cameraOpticsTechnique_commonOrthographic
    {
        [XmlElement("aspect_ratio", typeof(TargetableFloat))]
        [XmlElement("xmag", typeof(TargetableFloat))]
        [XmlElement("ymag", typeof(TargetableFloat))]
        [XmlChoiceIdentifier("ItemsElementName")]
        public TargetableFloat[] Items { get; set; }

        [XmlElement("ItemsElementName")]
        [XmlIgnore]
        public ItemsChoiceType[] ItemsElementName { get; set; }

        public TargetableFloat znear { get; set; }

        public TargetableFloat zfar { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IncludeInSchema = false)]
    public enum ItemsChoiceType
    {
        aspect_ratio,
        xmag,
        ymag,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class cameraOpticsTechnique_commonPerspective
    {
        [XmlElement("aspect_ratio", typeof(TargetableFloat))]
        [XmlElement("xfov", typeof(TargetableFloat))]
        [XmlElement("yfov", typeof(TargetableFloat))]
        [XmlChoiceIdentifier("ItemsElementName")]
        public TargetableFloat[] Items { get; set; }

        [XmlElement("ItemsElementName")]
        [XmlIgnore]
        public ItemsChoiceType1[] ItemsElementName { get; set; }

        public TargetableFloat znear { get; set; }

        public TargetableFloat zfar { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IncludeInSchema = false)]
    public enum ItemsChoiceType1
    {
        aspect_ratio,
        xfov,
        yfov,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class cameraImager
    {
        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_controllers
    {
        public asset asset { get; set; }

        [XmlElement("controller")]
        public controller[] controller { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class controller
    {
        public asset asset { get; set; }

        [XmlElement("morph", typeof(morph))]
        [XmlElement("skin", typeof(skin))]
        public object Item { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class morph
    {
        [XmlElement("source")]
        public source[] source { get; set; }

        public morphTargets targets { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute]
        [DefaultValue(MorphMethodType.NORMALIZED)]
        public MorphMethodType method { get; set; } = MorphMethodType.NORMALIZED;

        [XmlAttribute("source", DataType = "anyURI")]
        public string source1 { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class morphTargets
    {
        [XmlElement("input")]
        public InputLocal[] input { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum MorphMethodType
    {
        NORMALIZED,
        RELATIVE,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class skin
    {
        public string bind_shape_matrix { get; set; }

        [XmlElement("source")]
        public source[] source { get; set; }

        public skinJoints joints { get; set; }
        public skinVertex_weights vertex_weights { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute("source", DataType = "anyURI")]
        public string source1 { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class skinJoints
    {
        [XmlElement("input")]
        public InputLocal[] input { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class skinVertex_weights
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        public string vcount { get; set; }
        public string v { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_effects
    {
        public asset asset { get; set; }

        [XmlElement("effect")]
        public effect[] effect { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class effect
    {
        public asset asset { get; set; }

        [XmlElement("annotate")]
        public fx_annotate_common[] annotate { get; set; }

        [XmlElement("image")]
        public image[] image { get; set; }

        [XmlElement("newparam")]
        public fx_newparam_common[] newparam { get; set; }

        [XmlElement("profile_COMMON")]
        public effectFx_profile_abstractProfile_COMMON[] Items { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class image
    {
        public asset asset { get; set; }

        [XmlElement("data", typeof(byte[]), DataType = "hexBinary")]
        [XmlElement("init_from", typeof(string), DataType = "anyURI")]
        public object Item { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute(DataType = "token")]
        public string format { get; set; }

        [XmlAttribute]
        public ulong height { get; set; }

        [XmlIgnore]
        public bool heightSpecified { get; set; }

        [XmlAttribute]
        public ulong width { get; set; }

        [XmlIgnore]
        public bool widthSpecified { get; set; }

        [XmlAttribute]
        [DefaultValue(typeof(ulong), "1")]
        public ulong depth { get; set; } = ((ulong)(1m));
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot("profile_COMMON", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class effectFx_profile_abstractProfile_COMMON
    {
        public asset asset { get; set; }

        [XmlElement("image", typeof(image))]
        [XmlElement("newparam", typeof(common_newparam_type))]
        public object[] Items { get; set; }

        public effectFx_profile_abstractProfile_COMMONTechnique technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class effectFx_profile_abstractProfile_COMMONTechnique
    {
        public asset asset { get; set; }

        [XmlElement("image", typeof(image))]
        [XmlElement("newparam", typeof(common_newparam_type))]
        public object[] Items { get; set; }

        [XmlElement("blinn", typeof(effectFx_profile_abstractProfile_COMMONTechniqueBlinn))]
        [XmlElement("constant", typeof(effectFx_profile_abstractProfile_COMMONTechniqueConstant))]
        [XmlElement("lambert", typeof(effectFx_profile_abstractProfile_COMMONTechniqueLambert))]
        [XmlElement("phong", typeof(effectFx_profile_abstractProfile_COMMONTechniquePhong))]
        public object Item { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class effectFx_profile_abstractProfile_COMMONTechniqueBlinn
    {
        public common_color_or_texture_type emission { get; set; }
        public common_color_or_texture_type ambient { get; set; }
        public common_color_or_texture_type diffuse { get; set; }
        public common_color_or_texture_type specular { get; set; }
        public common_float_or_param_type shininess { get; set; }
        public common_color_or_texture_type reflective { get; set; }
        public common_float_or_param_type reflectivity { get; set; }
        public common_transparent_type transparent { get; set; }
        public common_float_or_param_type transparency { get; set; }
        public common_float_or_param_type index_of_refraction { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class effectFx_profile_abstractProfile_COMMONTechniqueConstant
    {
        public common_color_or_texture_type emission { get; set; }
        public common_color_or_texture_type reflective { get; set; }
        public common_float_or_param_type reflectivity { get; set; }
        public common_transparent_type transparent { get; set; }
        public common_float_or_param_type transparency { get; set; }
        public common_float_or_param_type index_of_refraction { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class effectFx_profile_abstractProfile_COMMONTechniqueLambert
    {
        public common_color_or_texture_type emission { get; set; }
        public common_color_or_texture_type ambient { get; set; }
        public common_color_or_texture_type diffuse { get; set; }
        public common_color_or_texture_type reflective { get; set; }
        public common_float_or_param_type reflectivity { get; set; }
        public common_transparent_type transparent { get; set; }
        public common_float_or_param_type transparency { get; set; }
        public common_float_or_param_type index_of_refraction { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class effectFx_profile_abstractProfile_COMMONTechniquePhong
    {
        public common_color_or_texture_type emission { get; set; }
        public common_color_or_texture_type ambient { get; set; }
        public common_color_or_texture_type diffuse { get; set; }
        public common_color_or_texture_type specular { get; set; }
        public common_float_or_param_type shininess { get; set; }
        public common_color_or_texture_type reflective { get; set; }
        public common_float_or_param_type reflectivity { get; set; }
        public common_transparent_type transparent { get; set; }
        public common_float_or_param_type transparency { get; set; }
        public common_float_or_param_type index_of_refraction { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_force_fields
    {
        public asset asset { get; set; }

        [XmlElement("force_field")]
        public force_field[] force_field { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class force_field
    {
        public asset asset { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_geometries
    {
        public asset asset { get; set; }

        [XmlElement("geometry")]
        public geometry[] geometry { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class geometry
    {
        public asset asset { get; set; }

        [XmlElement("convex_mesh", typeof(convex_mesh))]
        [XmlElement("mesh", typeof(mesh))]
        [XmlElement("spline", typeof(spline))]
        public object Item { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class convex_mesh
    {
        [XmlElement("source")]
        public source[] source { get; set; }

        public vertices vertices { get; set; }

        [XmlElement("lines", typeof(lines))]
        [XmlElement("linestrips", typeof(linestrips))]
        [XmlElement("polygons", typeof(polygons))]
        [XmlElement("polylist", typeof(polylist))]
        [XmlElement("triangles", typeof(triangles))]
        [XmlElement("trifans", typeof(trifans))]
        [XmlElement("tristrips", typeof(tristrips))]
        public object[] Items { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string convex_hull_of { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class vertices
    {
        [XmlElement("input")]
        public InputLocal[] input { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class lines
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        public string p { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string material { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class linestrips
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        [XmlElement("p")]
        public string[] p { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string material { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class polygons
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        [XmlElement("p", typeof(string))]
        [XmlElement("ph", typeof(polygonsPH))]
        public object[] Items { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string material { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class polygonsPH
    {
        public string p { get; set; }

        [XmlElement("h")]
        public string[] h { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class polylist
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        public string vcount { get; set; }
        public string p { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string material { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class triangles
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        public string p { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string material { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class trifans
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        [XmlElement("p")]
        public string[] p { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string material { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class tristrips
    {
        [XmlElement("input")]
        public InputLocalOffset[] input { get; set; }

        [XmlElement("p")]
        public string[] p { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute]
        public ulong count { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string material { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class mesh
    {
        [XmlElement("source")]
        public source[] source { get; set; }

        public vertices vertices { get; set; }

        [XmlElement("lines", typeof(lines))]
        [XmlElement("linestrips", typeof(linestrips))]
        [XmlElement("polygons", typeof(polygons))]
        [XmlElement("polylist", typeof(polylist))]
        [XmlElement("triangles", typeof(triangles))]
        [XmlElement("trifans", typeof(trifans))]
        [XmlElement("tristrips", typeof(tristrips))]
        public object[] Items { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class spline
    {
        public spline()
        {
            closed = false;
        }

        [XmlElement("source")]
        public source[] source { get; set; }

        public splineControl_vertices control_vertices { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool closed { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class splineControl_vertices
    {
        [XmlElement("input")]
        public InputLocal[] input { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_images
    {
        public asset asset { get; set; }

        [XmlElement("image")]
        public image[] image { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_lights
    {
        public asset asset { get; set; }

        [XmlElement("light")]
        public light[] light { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class light
    {
        public asset asset { get; set; }

        public lightTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class lightTechnique_common
    {
        [XmlElement("ambient", typeof(lightTechnique_commonAmbient))]
        [XmlElement("directional", typeof(lightTechnique_commonDirectional))]
        [XmlElement("point", typeof(lightTechnique_commonPoint))]
        [XmlElement("spot", typeof(lightTechnique_commonSpot))]
        public object Item { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class lightTechnique_commonAmbient
    {
        public TargetableFloat3 color { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot("scale", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class TargetableFloat3
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertDoubleArray(value);
        }

        [XmlIgnore]
        public double[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class lightTechnique_commonDirectional
    {
        public TargetableFloat3 color { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class lightTechnique_commonPoint
    {
        public TargetableFloat3 color { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='1.0' attribute.
        public TargetableFloat constant_attenuation { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat linear_attenuation { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat quadratic_attenuation { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class lightTechnique_commonSpot
    {
        public TargetableFloat3 color { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='1.0' attribute.
        public TargetableFloat constant_attenuation { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat linear_attenuation { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat quadratic_attenuation { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='180.0' attribute.
        public TargetableFloat falloff_angle { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat falloff_exponent { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_materials
    {
        public asset asset { get; set; }

        [XmlElement("material")]
        public material[] material { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class material
    {
        public asset asset { get; set; }

        public instance_effect instance_effect { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class instance_effect
    {
        [XmlElement("technique_hint")]
        public instance_effectTechnique_hint[] technique_hint { get; set; }

        [XmlElement("setparam")]
        public instance_effectSetparam[] setparam { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string url { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_effectTechnique_hint
    {
        [XmlAttribute(DataType = "NCName")]
        public string platform { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string profile { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_effectSetparam
    {
        public bool @bool { get; set; }
        public string bool2 { get; set; }
        public string bool3 { get; set; }
        public string bool4 { get; set; }
        public long @int { get; set; }
        public string int2 { get; set; }
        public string int3 { get; set; }
        public string int4 { get; set; }
        public double @float { get; set; }
        public string float2 { get; set; }
        public string float3 { get; set; }
        public string float4 { get; set; }
        public double float1x1 { get; set; }
        public string float1x2 { get; set; }
        public string float1x3 { get; set; }
        public string float1x4 { get; set; }
        public string float2x1 { get; set; }
        public string float2x2 { get; set; }
        public string float2x3 { get; set; }
        public string float2x4 { get; set; }
        public string float3x1 { get; set; }
        public string float3x2 { get; set; }
        public string float3x3 { get; set; }
        public string float3x4 { get; set; }
        public string float4x1 { get; set; }
        public string float4x2 { get; set; }
        public string float4x3 { get; set; }
        public string float4x4 { get; set; }
        public fx_surface_common surface { get; set; }
        public fx_sampler1D_common sampler1D { get; set; }
        public fx_sampler2D_common sampler2D { get; set; }
        public fx_sampler3D_common sampler3D { get; set; }
        public fx_samplerCUBE_common samplerCUBE { get; set; }
        public fx_samplerRECT_common samplerRECT { get; set; }
        public fx_samplerDEPTH_common samplerDEPTH { get; set; }
        public string @enum { get; set; }

        [XmlAttribute(DataType = "token")]
        public string @ref { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_nodes
    {
        public asset asset { get; set; }

        [XmlElement("node")]
        public node[] node { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class node
    {
        public node()
        {
            type = NodeType.NODE;
        }

        public asset asset { get; set; }

        [XmlElement("lookat", typeof(lookat))]
        [XmlElement("matrix", typeof(matrix))]
        [XmlElement("rotate", typeof(rotate))]
        [XmlElement("scale", typeof(TargetableFloat3))]
        [XmlElement("skew", typeof(skew))]
        [XmlElement("translate", typeof(TargetableFloat3))]
        [XmlChoiceIdentifier("ItemsElementName")]
        public object[] Items { get; set; }

        [XmlElement("ItemsElementName")]
        [XmlIgnore]
        public ItemsChoiceType2[] ItemsElementName { get; set; }

        [XmlElement("instance_camera")]
        public InstanceWithExtra[] instance_camera { get; set; }

        [XmlElement("instance_controller")]
        public instance_controller[] instance_controller { get; set; }

        [XmlElement("instance_geometry")]
        public instance_geometry[] instance_geometry { get; set; }

        [XmlElement("instance_light")]
        public InstanceWithExtra[] instance_light { get; set; }

        [XmlElement("instance_node")]
        public InstanceWithExtra[] instance_node { get; set; }

        [XmlElement("node")]
        public node[] node1 { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute]
        [DefaultValue(NodeType.NODE)]
        public NodeType type { get; set; }

        [XmlAttribute(DataType = "Name")]
        public string[] layer { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class lookat
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertDoubleArray(value);
        }

        [XmlIgnore]
        public double[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class matrix
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertDoubleArray(value);
        }

        [XmlIgnore]
        public double[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class rotate
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertDoubleArray(value);
        }

        [XmlIgnore]
        public double[] Values { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class skew
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public string _Text_
        {
            get => COLLADA.ConvertFromArray(Values);
            set => Values = COLLADA.ConvertDoubleArray(value);
        }

        [XmlIgnore]
        public double[] Values { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IncludeInSchema = false)]
    public enum ItemsChoiceType2
    {
        lookat,
        matrix,
        rotate,
        scale,
        skew,
        translate,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class instance_controller
    {
        [XmlElement("skeleton", DataType = "anyURI")]
        public string[] skeleton { get; set; }

        public bind_material bind_material { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string url { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class bind_material
    {
        [XmlElement("param")]
        public param[] param { get; set; }

        [XmlArrayItem("instance_material", IsNullable = false)]
        public instance_material[] technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class instance_geometry
    {
        public bind_material bind_material { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string url { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum NodeType
    {
        JOINT,
        NODE,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_physics_materials
    {
        public asset asset { get; set; }

        [XmlElement("physics_material")]
        public physics_material[] physics_material { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class physics_material
    {
        public asset asset { get; set; }
        public physics_materialTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class physics_materialTechnique_common
    {
        public TargetableFloat dynamic_friction { get; set; }
        public TargetableFloat restitution { get; set; }
        public TargetableFloat static_friction { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_physics_models
    {
        public asset asset { get; set; }

        [XmlElement("physics_model")]
        public physics_model[] physics_model { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class physics_model
    {
        public asset asset { get; set; }

        [XmlElement("rigid_body")]
        public rigid_body[] rigid_body { get; set; }

        [XmlElement("rigid_constraint")]
        public rigid_constraint[] rigid_constraint { get; set; }

        [XmlElement("instance_physics_model")]
        public instance_physics_model[] instance_physics_model { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class rigid_body
    {
        public rigid_bodyTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_bodyTechnique_common
    {
        public rigid_bodyTechnique_commonDynamic dynamic { get; set; }
        public TargetableFloat mass { get; set; }

        [XmlArrayItem("rotate", typeof(rotate), IsNullable = false)]
        [XmlArrayItem("translate", typeof(TargetableFloat3), IsNullable = false)]
        public object[] mass_frame { get; set; }

        public TargetableFloat3 inertia { get; set; }

        [XmlElement("instance_physics_material", typeof(InstanceWithExtra))]
        [XmlElement("physics_material", typeof(physics_material))]
        public object Item { get; set; }

        [XmlElement("shape")]
        public rigid_bodyTechnique_commonShape[] shape { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_bodyTechnique_commonDynamic
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public bool Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_bodyTechnique_commonShape
    {
        public rigid_bodyTechnique_commonShapeHollow hollow { get; set; }
        public TargetableFloat mass { get; set; }
        public TargetableFloat density { get; set; }

        [XmlElement("instance_physics_material", typeof(InstanceWithExtra))]
        [XmlElement("physics_material", typeof(physics_material))]
        public object Item { get; set; }

        [XmlElement("box", typeof(box))]
        [XmlElement("capsule", typeof(capsule))]
        [XmlElement("cylinder", typeof(cylinder))]
        [XmlElement("instance_geometry", typeof(instance_geometry))]
        [XmlElement("plane", typeof(plane))]
        [XmlElement("sphere", typeof(sphere))]
        [XmlElement("tapered_capsule", typeof(tapered_capsule))]
        [XmlElement("tapered_cylinder", typeof(tapered_cylinder))]
        public object Item1 { get; set; }

        [XmlElement("rotate", typeof(rotate))]
        [XmlElement("translate", typeof(TargetableFloat3))]
        public object[] Items { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_bodyTechnique_commonShapeHollow
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public bool Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class box
    {
        public string half_extents { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class capsule
    {
        public double height { get; set; }
        public string radius { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class cylinder
    {
        public double height { get; set; }
        public string radius { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class plane
    {
        public string equation { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class sphere
    {
        public double radius { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class tapered_capsule
    {
        public double height { get; set; }
        public string radius1 { get; set; }
        public string radius2 { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class tapered_cylinder
    {
        public double height { get; set; }
        public string radius1 { get; set; }
        public string radius2 { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class rigid_constraint
    {
        public rigid_constraintRef_attachment ref_attachment { get; set; }
        public rigid_constraintAttachment attachment { get; set; }
        public rigid_constraintTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintRef_attachment
    {
        [XmlElement("extra", typeof(extra))]
        [XmlElement("rotate", typeof(rotate))]
        [XmlElement("translate", typeof(TargetableFloat3))]
        public object[] Items { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string rigid_body { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintAttachment
    {
        [XmlElement("extra", typeof(extra))]
        [XmlElement("rotate", typeof(rotate))]
        [XmlElement("translate", typeof(TargetableFloat3))]
        public object[] Items { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string rigid_body { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_common
    {
        // CODEGEN Warning: DefaultValue attribute on members of type rigid_constraintTechnique_commonEnabled is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='true' attribute.
        public rigid_constraintTechnique_commonEnabled enabled { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type rigid_constraintTechnique_commonInterpenetrate is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='false' attribute.
        public rigid_constraintTechnique_commonInterpenetrate interpenetrate { get; set; }

        public rigid_constraintTechnique_commonLimits limits { get; set; }
        public rigid_constraintTechnique_commonSpring spring { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonEnabled
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public bool Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonInterpenetrate
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public bool Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonLimits
    {
        public rigid_constraintTechnique_commonLimitsSwing_cone_and_twist swing_cone_and_twist { get; set; }
        public rigid_constraintTechnique_commonLimitsLinear linear { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonLimitsSwing_cone_and_twist
    {
        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat3 is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0 0.0 0.0' attribute.
        public TargetableFloat3 min { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat3 is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0 0.0 0.0' attribute.
        public TargetableFloat3 max { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonLimitsLinear
    {
        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat3 is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0 0.0 0.0' attribute.
        public TargetableFloat3 min { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat3 is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0 0.0 0.0' attribute.
        public TargetableFloat3 max { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonSpring
    {
        public rigid_constraintTechnique_commonSpringAngular angular { get; set; }
        public rigid_constraintTechnique_commonSpringLinear linear { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonSpringAngular
    {
        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='1.0' attribute.
        public TargetableFloat stiffness { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat damping { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat target_value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class rigid_constraintTechnique_commonSpringLinear
    {
        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='1.0' attribute.
        public TargetableFloat stiffness { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat damping { get; set; }

        // CODEGEN Warning: DefaultValue attribute on members of type TargetableFloat is not supported in this version of the .Net Framework.
        // CODEGEN Warning: 'default' attribute supported only for primitive types.  Ignoring default='0.0' attribute.
        public TargetableFloat target_value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class instance_physics_model
    {
        [XmlElement("instance_force_field")]
        public InstanceWithExtra[] instance_force_field { get; set; }

        [XmlElement("instance_rigid_body")]
        public instance_rigid_body[] instance_rigid_body { get; set; }

        [XmlElement("instance_rigid_constraint")]
        public instance_rigid_constraint[] instance_rigid_constraint { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string url { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string parent { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class instance_rigid_body
    {
        public instance_rigid_bodyTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string body { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string target { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_rigid_bodyTechnique_common
    {
        public instance_rigid_bodyTechnique_common()
        {
            angular_velocity = "0.0 0.0 0.0";
            velocity = "0.0 0.0 0.0";
        }

        [DefaultValue("0.0 0.0 0.0")]
        public string angular_velocity { get; set; }

        [DefaultValue("0.0 0.0 0.0")]
        public string velocity { get; set; }

        public instance_rigid_bodyTechnique_commonDynamic dynamic { get; set; }

        public TargetableFloat mass { get; set; }

        [XmlArrayItem("rotate", typeof(rotate), IsNullable = false)]
        [XmlArrayItem("translate", typeof(TargetableFloat3), IsNullable = false)]
        public object[] mass_frame { get; set; }

        public TargetableFloat3 inertia { get; set; }

        [XmlElement("instance_physics_material", typeof(InstanceWithExtra))]
        [XmlElement("physics_material", typeof(physics_material))]
        public object Item { get; set; }

        [XmlElement("shape")]
        public instance_rigid_bodyTechnique_commonShape[] shape { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_rigid_bodyTechnique_commonDynamic
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public bool Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_rigid_bodyTechnique_commonShape
    {
        public instance_rigid_bodyTechnique_commonShapeHollow hollow { get; set; }
        public TargetableFloat mass { get; set; }
        public TargetableFloat density { get; set; }

        [XmlElement("instance_physics_material", typeof(InstanceWithExtra))]
        [XmlElement("physics_material", typeof(physics_material))]
        public object Item { get; set; }

        [XmlElement("box", typeof(box))]
        [XmlElement("capsule", typeof(capsule))]
        [XmlElement("cylinder", typeof(cylinder))]
        [XmlElement("instance_geometry", typeof(instance_geometry))]
        [XmlElement("plane", typeof(plane))]
        [XmlElement("sphere", typeof(sphere))]
        [XmlElement("tapered_capsule", typeof(tapered_capsule))]
        [XmlElement("tapered_cylinder", typeof(tapered_cylinder))]
        public object Item1 { get; set; }

        [XmlElement("rotate", typeof(rotate))]
        [XmlElement("translate", typeof(TargetableFloat3))]
        public object[] Items { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class instance_rigid_bodyTechnique_commonShapeHollow
    {
        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlText]
        public bool Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class instance_rigid_constraint
    {
        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string constraint { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string sid { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_physics_scenes
    {
        public asset asset { get; set; }

        [XmlElement("physics_scene")]
        public physics_scene[] physics_scene { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class physics_scene
    {
        public asset asset { get; set; }

        [XmlElement("instance_force_field")]
        public InstanceWithExtra[] instance_force_field { get; set; }

        [XmlElement("instance_physics_model")]
        public instance_physics_model[] instance_physics_model { get; set; }

        public physics_sceneTechnique_common technique_common { get; set; }

        [XmlElement("technique")]
        public technique[] technique { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class physics_sceneTechnique_common
    {
        public TargetableFloat3 gravity { get; set; }
        public TargetableFloat time_step { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class library_visual_scenes
    {
        public asset asset { get; set; }

        [XmlElement("visual_scene")]
        public visual_scene[] visual_scene { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class visual_scene
    {
        public asset asset { get; set; }

        [XmlElement("node")]
        public node[] node { get; set; }

        [XmlElement("evaluate_scene")]
        public visual_sceneEvaluate_scene[] evaluate_scene { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string id { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class visual_sceneEvaluate_scene
    {
        [XmlElement("render")]
        public visual_sceneEvaluate_sceneRender[] render { get; set; }

        [XmlAttribute(DataType = "NCName")]
        public string name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class visual_sceneEvaluate_sceneRender
    {
        [XmlElement("layer", DataType = "NCName")]
        public string[] layer { get; set; }

        public instance_effect instance_effect { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string camera_node { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public class COLLADAScene
    {
        [XmlElement("instance_physics_scene")]
        public InstanceWithExtra[] instance_physics_scene { get; set; }

        public InstanceWithExtra instance_visual_scene { get; set; }

        [XmlElement("extra")]
        public extra[] extra { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum VersionType
    {
        [XmlEnum("1.4.0")] Item140,
        [XmlEnum("1.4.1")] Item141,
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    [XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public class ellipsoid
    {
        public string size { get; set; }
    }


    /// <summary>
    ///   Extend COLLADA class to provide convertion helpers
    /// </summary>
    public partial class COLLADA
    {
        private static Regex regex = new Regex(@"\s+");

        public static string ConvertFromArray<T>(IList<T> array)
        {
            if (array == null)
                return null;

            StringBuilder text = new StringBuilder();
            if (typeof(T) == typeof(double))
            {
                var items = (IList<double>)array;
                // If type is double, then use a plain ToString with no exponent
                for (int i = 0; i < array.Count; i++)
                {
                    text.Append(
                        items[i].ToString(
                            "0.000000",
                            NumberFormatInfo.InvariantInfo));
                    if ((i + 1) < array.Count)
                        text.Append(" ");
                }
            }
            else
            {
                for (int i = 0; i < array.Count; i++)
                {
                    text.Append(Convert.ToString(array[i], NumberFormatInfo.InvariantInfo));
                    if ((i + 1) < array.Count)
                        text.Append(" ");
                }
            }
            return text.ToString();
        }

        internal static string[] ConvertStringArray(string arrayStr)
            => regex.Split(arrayStr.Trim());

        internal static int[] ConvertIntArray(string arrayStr)
            => regex.Split(arrayStr.Trim()).Select(int.Parse).ToArray();

        internal static double[] ConvertDoubleArray(string arrayStr)
        {
            string[] elements = regex.Split(arrayStr.Trim());
            double[] ret = new double[elements.Length];
            try
            {
                ret = elements.Select(i => double.Parse(i, NumberStyles.Float, CultureInfo.InvariantCulture)).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return ret;
        }

        internal static bool[] ConvertBoolArray(string arrayStr)
            => regex.Split(arrayStr.Trim()).Select(bool.Parse).ToArray();

        public static COLLADA Load(string fileName)
        {
            using FileStream stream = new FileStream(fileName, FileMode.Open);
            return Load(stream);
        }

        public static COLLADA Load(Stream stream)
        {
            StreamReader str = new StreamReader(stream);
            XmlSerializer xSerializer = new XmlSerializer(typeof(COLLADA));

            return (COLLADA)xSerializer.Deserialize(str);
        }

        public void Save(string fileName)
        {
            using var stream = new FileStream(fileName, FileMode.Create);
            Save(stream);
        }

        public void Save(Stream stream)
        {
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            XmlSerializer xSerializer = new XmlSerializer(typeof(COLLADA));
            writer.Formatting = Formatting.Indented;
            xSerializer.Serialize(writer, this);
        }
    }
}