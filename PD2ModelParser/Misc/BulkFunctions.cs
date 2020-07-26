using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PD2ModelParser
{
    static class BulkFunctions
    {
        public static IEnumerable<FileInfo> WalkDirectoryTreeDepth(DirectoryInfo dir, string filepattern)
        {
            foreach (var i in dir.EnumerateFiles(filepattern))
            {
                if (i.Attributes.HasFlag(FileAttributes.Directory)) continue;
                yield return i;
            }

            foreach (var i in dir.EnumerateDirectories())
            {
                foreach (var f in WalkDirectoryTreeDepth(i, filepattern))
                {
                    yield return f;
                }
            }
        }

        public static IEnumerable<(string fullpath, string relativepath, FullModelData data)> EveryModel(string root)
        {
            foreach (var i in WalkDirectoryTreeDepth(new DirectoryInfo(root), "*.model"))
            {
                FullModelData fmd = null;
                try
                {
                    fmd = ModelReader.Open(i.FullName);
                }
                catch(Exception e)
                {
                    Log.Default.Warn($"Unable to read {i.FullName}: {e}");
                }
                if (fmd != null)
                {
                yield return (i.FullName, i.FullName.Substring(root.Length), fmd);
            }
        }
        }

        public static void WriteSimpleCsvLike<T>(TextWriter tw, IEnumerable<T> items)
        {
            var fields = typeof(T).GetFields().OrderBy(i => i.MetadataToken).ToList();
            tw.WriteLine(string.Join(",", fields.Select(i => i.Name)));
            var count = 1000;
            var inc = 0;
            foreach (var i in items)
            {
                var values = fields.Select(field => field.GetValue(i));
                tw.WriteLine(string.Join(",", values));
                if (count-- == 0)
                {
                    count = 1000;
                    tw.Flush();
                    Log.Default.Status($"{++inc}");
                }
            }
        }
    }
}