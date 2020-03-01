using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Modelscript
{
    public static class Script
    {
        public static FullModelData ExecuteItems(IEnumerable<IScriptItem> items, string workDir)
        {
            var state = new ScriptState
            {
                Data = new FullModelData(),
                WorkDir = workDir
            };
            foreach(var i in items)
            {
                i.Execute(state);
            }
            return state.Data;
        }

        public static IEnumerable<string> GetSourceFilenames(IEnumerable<IScriptItem> items)
        {
            return items.Where(i => i is IReadsFile).Select(i => (i as IReadsFile).File);
        }
    }

    public class ScriptState
    {
        public FullModelData Data { get; set; }
        public string WorkDir { get; set; }
        public string ResolvePath(string path) => System.IO.Path.Combine(WorkDir, path);
        public bool CreateNewObjects { get; set; }
        public Sections.Object3D DefaultRootPoint { get; set; }
    }

    public interface IScriptItem
    {
        void Execute(ScriptState state);
    }

    public interface IReadsFile
    {
        string File { get; }
    }
}
