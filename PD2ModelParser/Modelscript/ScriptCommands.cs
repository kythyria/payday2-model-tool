using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using S = PD2ModelParser.Sections;

namespace PD2ModelParser.Modelscript
{
    public class CreateNewObjects : IScriptItem
    {
        public bool Create { get; set; }
        public void Execute(ScriptState state)
        {
            state.Log.Status(Create ? "New objects will be created" : "New objects will NOT be created");
            state.CreateNewObjects = Create;
        }
    }

    public class SetRootPoint : IScriptItem
    {
        public string Name { get; set; }
        public void Execute(ScriptState state)
        {
            if(Name == null)
            {
                state.Log.Status("Clearing default root point");
                state.DefaultRootPoint = null;
            }
            else
            {
                state.Log.Status($"Setting default root point to {Name}");
                var point = state.Data.SectionsOfType<S.Object3D>()
                    .FirstOrDefault(o => o.Name == Name);
                state.DefaultRootPoint =  point ?? throw new Exception($"Root point {Name} not found!");
            }
        }
    }

    public class NewModel : IScriptItem
    {
        public void Execute(ScriptState state)
        {
            state.Log.Status("Creating new model");
            state.Data = new FullModelData();
        }

    }

    public class LoadModel : IScriptItem, IReadsFile
    {
        public string File { get; set; }
        public void Execute(ScriptState state)
        {
            state.Log.Status($"Loading model from {File}");
            state.Data = ModelReader.Open(state.ResolvePath(File));
        }
    }

    public class SaveModel : IScriptItem
    {
        public string StatusMessage => $"Save model to {0}";
        public string File { get; set; }
        public void Execute(ScriptState state)
        {
            state.Log.Status($"Saving model to {File}");
            DieselExporter.ExportFile(state.Data, state.ResolvePath(File));
        }
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
        public Dictionary<string, string> ImporterOptions { get; set; } = new Dictionary<string, string>();
        public bool? CreateNewObjects { get; set; }

        public void Execute(ScriptState state)
        {
            state.Log.Status($"Importing from {File}");
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

            Importers.IOptionReceiver opts = new Importers.GenericOptionReceiver();
#if !NO_FBX
            opts = new Importers.FilmboxImporter.FbxImportOptions()
#endif
            foreach(var kv in ImporterOptions)
            {
                opts.AddOption(kv.Key, kv.Value);
            }

            bool createObjects = CreateNewObjects ?? state.CreateNewObjects;
            if(effectiveType == ImportFileType.Fbx && createObjects)
                throw new Exception("Creating objects is not yet supported for FBX");

            importer(state.Data, filepath, createObjects, ParentFinder, opts);
        }
    }

    public class PatternUV : IScriptItem, IReadsFile
    {
        public string File { get; set; }
        public void Execute(ScriptState state)
        {
            state.Log.Status($"Reading pattern UVs from {File}");
            string path = state.ResolvePath(File);
            if(!path.EndsWith(".obj"))
            {
                throw new Exception($"Using \"{0}\" for pattern UV import requires it be OBJ format");
            }
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
            state.Log.Status($"Exporting to {File}");
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
        public ExportFileType? FileType { get; set; }
        public string Directory { get; set; }

        public void Execute(ScriptState state)
        {
            state.Log.Status($"Batch exporting in {Directory}");
            var actualType = FileType ?? state.DefaultExportType;
            foreach(var (path, relpath, fmd) in BulkFunctions.EveryModel(state.ResolvePath(Directory))) {
                state.Data = fmd;
                new Export
                {
                    // TODO: Better extension handling
                    File = System.IO.Path.ChangeExtension(path, actualType.ToString().ToLower())
                }.Execute(state);
            }
        }
    }

    public class SetDefaultType : IScriptItem
    {
        public ExportFileType FileType { get; set; }
        public void Execute(ScriptState state)
        {
            state.Log.Status($"Default batch export type is {FileType}");
            state.DefaultExportType = FileType;
        }
    }

    public class RunScript : IScriptItem, IReadsFile
    {
        public string File { get; set; }
        public void Execute(ScriptState state)
        {
            state.Log.Status($"Running other modelscript {File}");
            string path = state.ResolvePath(File);
            var script = Script.ParseXml(path);
            state.ExecuteItems(script);
            state.Log.Status($"Finished running {File}");
        }
    }

    public class CreateObject3d : IScriptItem
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Quaternion Rotation { get; set; } = new Quaternion(0, 0, 0, 1);
        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        public void Execute(ScriptState state)
        {
            state.Log.Status($"Creating object {Name}");
            var extant = state.Data.SectionsOfType<S.Object3D>()
                .Where(i => i.Name == Name)
                .FirstOrDefault();
            if(extant != null)
            {
                throw new Exception($"Cannot create object {Name}: Already exists.");
            }

            S.Object3D parent = null;
            if (Parent != null)
            {
                parent = state.Data.SectionsOfType<S.Object3D>()
                    .Where(i => i.Name == Parent)
                    .FirstOrDefault();
                if (parent == null)
                {
                    throw new Exception($"Cannot find Object3D named \"{Parent}\" to use as a parent for {Name}");
                }
            }

            var obj = new S.Object3D(Name, parent);
            var tf = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation);
            tf.Translation = Position;
            obj.Transform = tf.ToNexusMatrix();
        }
    }

    public class ModifyObject3d : IScriptItem
    {
        public string Name { get; set; }
        public bool SetParent { get; set; }
        public string Parent { get; set; }
        public Vector3? Position { get; set; }
        public Quaternion? Rotation { get; set; }
        public Vector3? Scale { get; set; }

        public void Execute(ScriptState state)
        {
            state.Log.Status($"Modifying object {Name}");
            var extant = state.Data.SectionsOfType<S.Object3D>()
                .Where(i => i.Name == Name)
                .FirstOrDefault();
            if (extant == null)
            {
                throw new Exception($"Cannot modify object {Name} Does not exist.");
            }

            if (SetParent && Parent != null)
            {
                var parent = state.Data.SectionsOfType<S.Object3D>()
                    .Where(i => i.Name == Parent)
                    .FirstOrDefault();
                if (parent == null)
                {
                    throw new Exception($"Cannot find Object3D named \"{Parent}\" to use as a parent for {Name}");
                }
                extant.SetParent(parent);
            }
            else if (SetParent)
            {
                extant.SetParent(null);
            }

            var tf = extant.Transform;
            if (Rotation.HasValue || Scale.HasValue)
            {
                tf.Decompose(out var scale, out var rotation, out var translation);
                Rotation.WithValue(r => rotation = r.ToNexusQuaternion());
                Scale.WithValue(s => scale = s.ToNexusVector());
                tf = Nexus.Matrix3D.CreateScale(scale) * Nexus.Matrix3D.CreateFromQuaternion(rotation);
            }
            Position.WithValue(p => tf.Translation = p.ToNexusVector());

            extant.Transform = tf;
        }
    }
}