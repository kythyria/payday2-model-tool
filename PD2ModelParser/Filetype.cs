using System;
using System.Collections.Generic;
using System.Linq;
using PD2ModelParser.Importers;

namespace PD2ModelParser
{
    public abstract class FileTypeInfo
    {
        public abstract string Extension { get; }
        public abstract string Name { get; }
        public virtual string FormatName => $"{Name} (.{Extension})";
        public abstract bool CanExport { get; }
        public abstract bool CanImport { get; }
        public abstract string Export(FullModelData data, string path);
        public abstract void Import(FullModelData data, string path, bool createModels, Func<string, Sections.Object3D> parentFinder, IOptionReceiver options);
        public virtual IOptionReceiver CreateOptionReceiver() => new GenericOptionReceiver();
        public override string ToString() => Extension.ToUpper();

        public static bool TryParseName(string name, out FileTypeInfo result)
        {
            if(name == null) { result = null;  return false; }
            var ident = name.ToLower().TrimStart('.');
            result = Types.FirstOrDefault(i => i.Extension == ident || i.Name == ident);
            return result != null;
        }

        public static bool TryParseFromExtension(string path, out FileTypeInfo result)
        {
            return TryParseName(System.IO.Path.GetExtension(path), out result);
        }

        class FbxType : FileTypeInfo
        {
            public override string Extension => "fbx";
            public override string Name => "Filmbox";
#if !NO_FBX
            public override bool CanExport => true;
            public override bool CanImport => true;
            public override string Export(FullModelData data, string path) => Exporters.FbxExporter.ExportFile(data, path);
            public override void Import(FullModelData data, string path, bool createModels, Func<string, Sections.Object3D> parentFinder, IOptionReceiver options)
                => FilmboxImporter.Import(data, path, createModels, parentFinder, options);
            public override IOptionReceiver CreateOptionReceiver() => new FilmboxImporter.FbxImportOptions();
#else
            public override bool CanExport => false;
            public override bool CanImport => false;
            public override void Import(FullModelData data, string path, bool createModels, Func<string, Sections.Object3D> parentFinder, IOptionReceiver options)
                => throw new NotSupportedException("Model tool was built without FBX support");
            public override string Export(FullModelData data, string path) => throw new NotSupportedException("Model tool was built without FBX support");
            public override IOptionReceiver CreateOptionReceiver() => throw new NotSupportedException("Model tool was built without FBX support");
#endif
        }
        public static readonly FileTypeInfo Fbx = new FbxType();

        class ObjType : FileTypeInfo
        {
            public override string Extension => "obj";
            public override string Name => "Object";
            public override bool CanExport => true;
            public override bool CanImport => true;
            public override void Import(FullModelData data, string path, bool createModels, Func<string, Sections.Object3D> parentFinder, IOptionReceiver options)
                => NewObjImporter.ImportNewObj(data, path, createModels, parentFinder, options);
            public override string Export(FullModelData data, string path) => ObjWriter.ExportFile(data, path);
        }
        public static readonly FileTypeInfo Obj = new ObjType();

        class DaeType : FileTypeInfo
        {
            public override string Extension => "dae";
            public override string Name => "Collada";
            public override bool CanExport => true;
            public override bool CanImport => false;
            public override void Import(FullModelData data, string path, bool createModels, Func<string, Sections.Object3D> parentFinder, IOptionReceiver options)
                => throw new Exception("Importing DAE files is not supported.");
            public override string Export(FullModelData data, string path) => ColladaExporter.ExportFile(data, path);
        }
        public static readonly FileTypeInfo Dae = new DaeType();

        class GltfType : FileTypeInfo
        {
            public override string Extension => "gltf";
            public override string Name => "glTF Separate Files";
            public override bool CanExport => true;
            public override bool CanImport => true;
            public override void Import(FullModelData data, string path, bool createModels, Func<string, Sections.Object3D> parentFinder, IOptionReceiver options)
                => GltfImporter.Import(data, path, createModels, parentFinder, options);
            public override string Export(FullModelData data, string path)
                => Exporters.GltfExporter.ExportFile(data, path, false);
        }
        public static readonly FileTypeInfo Gltf = new GltfType();

        class GlbType : GltfType
        {
            public override string Extension => "glb";
            public override string Name => "glTF Binary";
            public override string Export(FullModelData data, string path)
                => Exporters.GltfExporter.ExportFile(data, path, true);
        }
        public static readonly FileTypeInfo Glb = new GlbType();

        public static IReadOnlyList<FileTypeInfo> Types { get; } = new List<FileTypeInfo>() {
            FileTypeInfo.Fbx,
            FileTypeInfo.Dae,
            FileTypeInfo.Obj,
            FileTypeInfo.Gltf,
            FileTypeInfo.Glb
        };
    }
}
