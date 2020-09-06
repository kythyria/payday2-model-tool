using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

    public class Import : IScriptItem, IReadsFile
    {
        public string File { get; set; }
        public FileTypeInfo ForceType { get; set; }
        public string DefaultRootPoint { get; set; }
        public Dictionary<string, string> Parents { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ImporterOptions { get; set; } = new Dictionary<string, string>();
        public bool? CreateNewObjects { get; set; }

        public void Execute(ScriptState state)
        {
            state.Log.Status($"Importing from {File}");
            var filepath = state.ResolvePath(File);
            FileTypeInfo effectiveType = null;
            if(ForceType != null)
            {
                effectiveType = ForceType;
            }
            else
            {
                var ext = System.IO.Path.GetExtension(filepath);
                if(!FileTypeInfo.TryParseName(ext, out effectiveType))
                {
                    throw new Exception($"Unrecognised file extension \"{ext}\". Use a conventional extension or specify the type explicitly.");
                }
            }

            if (!effectiveType.CanImport)
            {
                if (effectiveType == FileTypeInfo.Fbx)
                    throw new Exception("FBX support was not enabled at compile time");
                else
                    throw new Exception($"No import support for {effectiveType}");
            }

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
            var throwOnNoParent = effectiveType == FileTypeInfo.Obj;

            S.Object3D ParentFinder(string name)
            {
                if (parentObjects.ContainsKey(name)) return parentObjects[name];

                if(defaultRootObject == null && throwOnNoParent)
                    throw new Exception($"No default- nor object-rootpoint set for {name}");

                return defaultRootObject;
            }

            Importers.IOptionReceiver opts = effectiveType.CreateOptionReceiver();

            foreach(var kv in ImporterOptions)
            {
                opts.AddOption(kv.Key, kv.Value);
            }

            bool createObjects = CreateNewObjects ?? state.CreateNewObjects;
            if(effectiveType == FileTypeInfo.Fbx && createObjects)
                throw new Exception("Creating objects is not yet supported for FBX");

            effectiveType.Import(state.Data, filepath, createObjects, ParentFinder, opts);
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

    public class Export : IScriptItem
    {
        public string File { get; set; }
        public FileTypeInfo ForceType { get; set; }
        public void Execute(ScriptState state)
        {
            state.Log.Status($"Exporting to {File}");
            string path = state.ResolvePath(File);
            FileTypeInfo fti = ForceType;
            if(ForceType == null)
            {
                var ext = System.IO.Path.GetExtension(path);
                if(!FileTypeInfo.TryParseName(ext, out fti))
                {
                    throw new Exception($"Unrecognised file extension \"{ext}\". Use a conventional extension or specify the type explicitly.");
                }
            }

            if (fti == FileTypeInfo.Fbx && !fti.CanExport)
                throw new Exception("FBX support was not enabled at compile time");

            fti.Export(state.Data, path);
        }
    }

    public class BatchExport : IScriptItem
    {
        public FileTypeInfo FileType { get; set; }
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
                    File = System.IO.Path.ChangeExtension(path, actualType.Extension),
                    ForceType = actualType
                }.Execute(state);
            }
        }
    }

    public class SetDefaultType : IScriptItem
    {
        public FileTypeInfo FileType { get; set; }
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
            string oldBaseDir = state.WorkDir;
            state.WorkDir = System.IO.Path.GetDirectoryName(path);
            var script = Script.ParseXml(path);
            state.ExecuteItems(script);
            state.WorkDir = oldBaseDir;
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
            obj.Transform = tf;
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
                Matrix4x4.Decompose(tf, out var scale, out var rotation, out var translation);
                Rotation.WithValue(r => rotation = r);
                Scale.WithValue(s => scale = s);
                tf = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation);
            }
            Position.WithValue(p => tf.Translation = p);

            extant.Transform = tf;
        }
    }
}