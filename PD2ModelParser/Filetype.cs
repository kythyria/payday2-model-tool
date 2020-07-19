using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD2ModelParser.Importers;

namespace PD2ModelParser
{
    public abstract class FileTypeInfo
    {
        public abstract string Extension { get; }
        public abstract bool CanExport { get; }
        public abstract bool CanImport { get; }
        public abstract string Export(FullModelData data, string path);
        public virtual IOptionReceiver CreateOptionReceiver() => new GenericOptionReceiver();
        public override string ToString() => Extension.ToUpper();

        public static bool TryGetByExtension(string extension, out FileTypeInfo result)
        {
            var ident = extension.ToLower().TrimStart('.');
            switch(ident)
            {
                case "fbx": result = FileTypeInfo.Fbx;  return true;
                case "obj": result = FileTypeInfo.Obj;  return true;
                case "dae": result = FileTypeInfo.Dae;  return true;
                case "gltf":result = FileTypeInfo.Gltf; return true;
                case "glb": result = FileTypeInfo.Gltf; return true;
                default: result = null; return false;
            }
        }

        class FbxType : FileTypeInfo
        {
            public override string Extension => "fbx";
#if !NO_FBX
            public override bool CanExport => true;
            public override bool CanImport => true;
            public override string Export(FullModelData data, string path) => Exporters.FbxExporter.ExportFile(data, path);
            public override IOptionReceiver CreateOptionReceiver() => new FilmboxImporter.FbxImportOptions();
#else
            public override bool CanExport => false;
            public override bool CanImport => false;
            public override string Export(FullModelData data, string path) => throw new NotSupportedException("Model tool was built without FBX support");
            public override IOptionReceiver CreateOptionReceiver() => throw new NotSupportedException("Model tool was built without FBX support");
#endif
        }
        public static FileTypeInfo Fbx = new FbxType();

        class ObjType : FileTypeInfo
        {
            public override string Extension => "obj";
            public override bool CanExport => true;
            public override bool CanImport => true;
            public override string Export(FullModelData data, string path) => ObjWriter.ExportFile(data, path);
        }
        public static FileTypeInfo Obj = new ObjType();

        class DaeType : FileTypeInfo
        {
            public override string Extension => "dae";
            public override bool CanExport => true;
            public override bool CanImport => false;
            public override string Export(FullModelData data, string path) => ColladaExporter.ExportFile(data, path);
        }
        public static FileTypeInfo Dae = new DaeType();

        class GltfType : FileTypeInfo
        {
            public override string Extension => "gltf";
            public override bool CanExport => true;
            public override bool CanImport => true;
            public override string Export(FullModelData data, string path) => Exporters.GltfExporter.ExportFile(data, path);
        }
        public static FileTypeInfo Gltf = new GltfType();
    }
}
