using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PD2ModelParser;

namespace PD2Bundle
{
    public class KnownIndex
    {
        private Dictionary<ulong, string> hashes = new Dictionary<ulong, string>();

        public string GetString(ulong hash)
        {
            if (hashes.ContainsKey(hash))
            {
                return hashes[hash];
            }
            return Convert.ToString(hash);
        }

        public bool Contains(ulong hash)
        {
            return hashes.ContainsKey(hash);
        }

        private void CheckCollision(Dictionary<ulong, string> item, ulong hash, string value)
        {
            if ( item.ContainsKey(hash) && (item[hash] != value) )
            {
                Log.Default.Warn("Hash collision: {0:x} : {1} == {2}", hash, item[hash], value);
            }
        }

        public void Clear()
        {
            this.hashes.Clear();
            loaded = false;
        }

        bool loaded = false;

        public bool Load()
        {
            if (loaded) return true;

            foreach(var name in GetHashfileNames())
            {
                loaded |= TryLoad(name);
            }

            return loaded;
        }

        public bool TryLoad(string filename)
        {
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        Hint(line);
                        line = sr.ReadLine();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Default.Warn("Couldn't read hashlist file \"{0}\": {1}", filename, e.Message);
                return false;
            }
        }

        private IEnumerable<string> GetHashfileNames()
        {
            var exepath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var exedir = Path.GetDirectoryName(exepath);
            var cwd = Directory.GetCurrentDirectory();

            var hashregex = new Regex(@"hash(list|es)(-\d+)?(\.txt)?");
            var names = Directory.GetFiles(cwd).Where(i=>hashregex.IsMatch(i));
            if(exedir != cwd)
            {
                names = names.Concat(Directory.GetFiles(exedir).Where(i => hashregex.IsMatch(i)));
            }
            return names;
        }

        public void Hint(string line)
        {
            ulong hash = Hash64.HashString(line);
            CheckCollision(hashes, hash, line);
            hashes[hash] = line;
        }
    }
}
