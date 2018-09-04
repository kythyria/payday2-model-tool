using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
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
        }

        bool loaded = false;

        public bool Load()
        {
            if (loaded) return true;

            string filename = "hashes.txt";

            Stream stream;

            // Use the file if we have it
            if(File.Exists(filename))
            {
                stream = new FileStream(filename, FileMode.Open);
            }
            else
            {
                // Not very memory efficent, but in a world of almost all computers having 4+ GiB of RAM,
                // what harm does 20MiB or so do?
                stream = new MemoryStream(Encoding.UTF8.GetBytes(PD2ModelParser.Properties.Resources.hashes));
            }

            try
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        ulong hash = PD2ModelParser.Hash64.HashString(line);
                        this.CheckCollision(this.hashes, hash, line);
                        this.hashes[hash] = line;
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            loaded = true;
            return true;
        }
    }
}
