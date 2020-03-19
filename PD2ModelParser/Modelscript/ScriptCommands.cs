using System;
using System.Collections.Generic;
using System.Linq;

using S = PD2ModelParser.Sections;

namespace PD2ModelParser.Modelscript
{
    public class CreateNewObjects : IScriptItem
    {
        public bool Create { get; set; }
        public void Execute(ScriptState state) => state.CreateNewObjects = Create;
    }

    public class SetRootPoint : IScriptItem
    {
        public string Name { get; set; }
        public void Execute(ScriptState state)
        {
            if(Name == null)
            {
                state.DefaultRootPoint = null;
            }
            else
            {
                var point = state.Data.SectionsOfType<S.Object3D>()
                    .FirstOrDefault(o => o.Name == Name);
                state.DefaultRootPoint =  point ?? throw new Exception($"Root point {Name} not found!");
            }
        }
    }

    public class NewModel : IScriptItem
    {
        public void Execute(ScriptState state) => state.Data = new FullModelData();
    }

    public class LoadModel : IScriptItem, IReadsFile
    {
        public string File { get; set; }
        public void Execute(ScriptState state) => state.Data = ModelReader.Open(state.ResolvePath(File));
    }

    public class SaveModel : IScriptItem
    {
        public string File { get; set; }
        public void Execute(ScriptState state) => DieselExporter.ExportFile(state.Data, state.ResolvePath(File));
    }

    public enum ImportFileType
    {
        Obj,
        Fbx,
        Gltf
    }

    public class Import : IScriptItem, IReadsFile
    {
        public string File { get; set; }
        public ImportFileType? ForceType { get; set; }
        public string DefaultRootPoint { get; set; }
        public Dictionary<string, string> Parents { get; set; } = new Dictionary<string, string>();
        public Importers.IOptionReceiver ImporterOptions { get; set; }

        public void Execute(ScriptState state)
        {
            var filepath = state.ResolvePath(File);
            ImportFileType effectiveType;
            if(ForceType.HasValue)
            {
                effectiveType = ForceType.Value;
            }
            else
            {
                var ext = System.IO.Path.GetExtension(filepath);
                switch(ext)
                {
                    case ".fbx": effectiveType = ImportFileType.Fbx; break;
                    case ".obj": effectiveType = ImportFileType.Obj; break;
                    case ".gltf":
                    case ".glb":
                        effectiveType = ImportFileType.Gltf; break;
                    default:
                        throw new Exception($"Unrecognised file extension \"{ext}\". Use a conventional extension or specify the type explicitly.");
                }
            }

#if NO_FBX
            if(effectiveType == ImportFileType.Fbx)
                throw new Exception("FBX support was not enabled at compile time");
#endif

            var parentObjects = new Dictionary<string, S.Object3D>();
            foreach(var kv in Parents)
            {
                var parent = state.Data.SectionsOfType<S.Object3D>().FirstOrDefault(i => i.Name == kv.Value);
                if (parent == null)
                    throw new Exception($"Cannot find rootpoint element {kv.Value}");
                parentObjects.Add(kv.Key, parent);
            }

            S.Object3D defaultRootObject;
            if (DefaultRootPoint != null)
            {
                defaultRootObject = state.Data.SectionsOfType<S.Object3D>().FirstOrDefault(i => i.Name == DefaultRootPoint);
            }
            else
            {
                defaultRootObject = state.DefaultRootPoint;
            }
            var throwOnNoParent = effectiveType == ImportFileType.Obj;

            S.Object3D ParentFinder(string name)
            {
                if (parentObjects.ContainsKey(name)) return parentObjects[name];

                if(defaultRootObject == null && throwOnNoParent)
                    throw new Exception($"No default- nor object-rootpoint set for {name}");

                return defaultRootObject;
            }

            Action<FullModelData, string, bool, Func<string, S.Object3D>, Importers.IOptionReceiver> importer;
            switch (effectiveType)
            {
                case ImportFileType.Obj:
                    importer = NewObjImporter.ImportNewObj;
                    break;
#if !NO_FBX
                case ImportFileType.Fbx: importer = Importers.FilmboxImporter.Import; break;
#endif
                case ImportFileType.Gltf: importer = Importers.GltfImporter.Import; break;
                default:
                    throw new Exception($"BUG: No importer for {effectiveType}");
            }

            importer(state.Data, filepath, state.CreateNewObjects, ParentFinder, ImporterOptions);
        }
    }

    public class PatternUV : IScriptItem, IReadsFile
    {
        public string File { get; set; }
        public void Execute(ScriptState state)
        {
            string path = state.ResolvePath(File);
            bool result = NewObjImporter.ImportNewObjPatternUV(state.Data, path);
            if (!result)
            {
                throw new Exception("There was an error importing Pattern UV OBJ - see console");
            }
        }
    }

    public enum ExportFileType
    {
        Obj,
        Fbx,
        Gltf,
        Dae
    }

    public class Export : IScriptItem
    {
        public string File { get; set; }
        public ExportFileType? ForceType { get; set; }
        public void Execute(ScriptState state)
        {
            string path = state.ResolvePath(File);
            ExportFileType effectiveType;
            if(ForceType.HasValue)
            {
                effectiveType = ForceType.Value;
            }
            else
            {
                var ext = System.IO.Path.GetExtension(path);
                switch (ext)
                {
                    case ".fbx": effectiveType = ExportFileType.Fbx; break;
                    case ".obj": effectiveType = ExportFileType.Obj; break;
                    case ".gltf":
                    case ".glb":
                        effectiveType = ExportFileType.Gltf; break;
                    case ".dae": effectiveType = ExportFileType.Dae; break;
                    default:
                        throw new Exception($"Unrecognised file extension \"{ext}\". Use a conventional extension or specify the type explicitly.");
                }
            }

#if NO_FBX
            if (effectiveType == ExportFileType.Fbx)
                throw new Exception("FBX support was not enabled at compile time");
#endif

            Func<FullModelData, string, string> exporter;
            switch(effectiveType)
            {
                case ExportFileType.Dae: exporter = ColladaExporter.ExportFile; break;
#if !NO_FBX
                case ExportFileType.Fbx: exporter = Exporters.FbxExporter.ExportFile; break;
#endif
                case ExportFileType.Gltf: exporter = Exporters.GltfExporter.ExportFile; break;
                case ExportFileType.Obj: exporter = ObjWriter.ExportFile; break;
                default:
                    throw new Exception($"BUG: No exporter for {effectiveType}");
            }

            exporter(state.Data, path);
        }
    }

    public class BatchExport : IScriptItem
    {
        public string Directory { get; set; }
        public void Execute(ScriptState state)
        {
            foreach(var (path, relpath, fmd) in BulkFunctions.EveryModel(state.ResolvePath(Directory))) {
                state.Data = fmd;
                new Export
                {
                    // TODO: Better extension handling
                    File = System.IO.Path.ChangeExtension(path, state.DefaultExportType.ToString().ToLower())
                }.Execute(state);
            }
        }
    }
}