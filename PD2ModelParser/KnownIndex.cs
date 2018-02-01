using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PD2Bundle
{
    class Hash64
    {
        [DllImport("hash64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong Hash(byte[] k, ulong length, ulong level);
        public static ulong HashString(string input, ulong level = 0)
        {
            return Hash(UTF8Encoding.UTF8.GetBytes(input), (ulong)UTF8Encoding.UTF8.GetByteCount(input), level);
        }
    }

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
                Console.WriteLine("Hash collision: {0:x} : {1} == {2}", hash, item[hash], value);
            }
        }

        public void Clear()
        {
            this.hashes.Clear();
        }


        public bool Load()
        {
            try
            {
                using (StreamReader sr = new StreamReader(new FileStream("hashes.txt", FileMode.Open)))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        ulong hash = Hash64.HashString(line);
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
            return true;
        }
    }
}
