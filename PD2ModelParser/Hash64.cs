using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PD2ModelParser
{
    /*class Hash64
    {
        [DllImport("hash64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong Hash(byte[] k, ulong length, ulong level);
        public static ulong HashString(string input, ulong level = 0)
        {
            return Hash(UTF8Encoding.UTF8.GetBytes(input), (ulong)UTF8Encoding.UTF8.GetByteCount(input), level);
        }
    }*/

    public class Hash64
    {
        // C# port of https://gitlab.com/znixian/payday2-lua-source/blob/vr-beta/idstring_hash.c
        // Verified against C version to produce correct hashes
        // Attribution for the original version is currently unknown, though it
        // most likely came from UnknownCreats.

        private static void Mix64(ref ulong a, ref ulong b, ref ulong c)
        {
            a -= b;
            a -= c;
            a ^= (c >> 43);
            b -= c;
            b -= a;
            b ^= (a << 9);
            c -= a;
            c -= b;
            c ^= (b >> 8);
            a -= b;
            a -= c;
            a ^= (c >> 38);
            b -= c;
            b -= a;
            b ^= (a << 23);
            c -= a;
            c -= b;
            c ^= (b >> 5);
            a -= b;
            a -= c;
            a ^= (c >> 35);
            b -= c;
            b -= a;
            b ^= (a << 49);
            c -= a;
            c -= b;
            c ^= (b >> 11);
            a -= b;
            a -= c;
            a ^= (c >> 12);
            b -= c;
            b -= a;
            b ^= (a << 18);
            c -= a;
            c -= b;
            c ^= (b >> 22);
        }

        private static unsafe ulong Hash64Impl(byte* k, ulong length, ulong level)
        {
            ulong a, b, c, len;

            /* Set up the internal state */
            len = length;
            a = b = level; /* the previous hash value */
            c = 0x9e3779b97f4a7c13L; /* the golden ratio; an arbitrary value */

            /*---------------------------------------- handle most of the key */
            while (len >= 24)
            {
                a += (k[0] + ((ulong) k[1] << 8) + ((ulong) k[2] << 16) + ((ulong) k[3] << 24)
                      + ((ulong) k[4] << 32) + ((ulong) k[5] << 40) + ((ulong) k[6] << 48) + ((ulong) k[7] << 56));
                b += (k[8] + ((ulong) k[9] << 8) + ((ulong) k[10] << 16) + ((ulong) k[11] << 24)
                      + ((ulong) k[12] << 32) + ((ulong) k[13] << 40) + ((ulong) k[14] << 48) + ((ulong) k[15] << 56));
                c += (k[16] + ((ulong) k[17] << 8) + ((ulong) k[18] << 16) + ((ulong) k[19] << 24)
                      + ((ulong) k[20] << 32) + ((ulong) k[21] << 40) + ((ulong) k[22] << 48) + ((ulong) k[23] << 56));
                Mix64(ref a, ref b, ref c);
                k += 24;
                len -= 24;
            }

            /*------------------------------------- handle the last 23 bytes */
            c += length;
            switch (len) /* all the case statements fall through */
            {
                case 23:
                    c += ((ulong) k[22] << 56);
                    // Some smartass at Microsoft decieded it'd be a great
                    // idea to make fallthroughs illegal without any easy
                    // way of enabling it.
                    goto case 22;
                case 22:
                    c += ((ulong) k[21] << 48);
                    goto case 21;
                case 21:
                    c += ((ulong) k[20] << 40);
                    goto case 20;
                case 20:
                    c += ((ulong) k[19] << 32);
                    goto case 19;
                case 19:
                    c += ((ulong) k[18] << 24);
                    goto case 18;
                case 18:
                    c += ((ulong) k[17] << 16);
                    goto case 17;
                case 17:
                    c += ((ulong) k[16] << 8);
                    goto case 16;
                /* the first byte of c is reserved for the length */
                case 16:
                    b += ((ulong) k[15] << 56);
                    goto case 15;
                case 15:
                    b += ((ulong) k[14] << 48);
                    goto case 14;
                case 14:
                    b += ((ulong) k[13] << 40);
                    goto case 13;
                case 13:
                    b += ((ulong) k[12] << 32);
                    goto case 12;
                case 12:
                    b += ((ulong) k[11] << 24);
                    goto case 11;
                case 11:
                    b += ((ulong) k[10] << 16);
                    goto case 10;
                case 10:
                    b += ((ulong) k[9] << 8);
                    goto case 9;
                case 9:
                    b += ((ulong) k[8]);
                    goto case 8;
                case 8:
                    a += ((ulong) k[7] << 56);
                    goto case 7;
                case 7:
                    a += ((ulong) k[6] << 48);
                    goto case 6;
                case 6:
                    a += ((ulong) k[5] << 40);
                    goto case 5;
                case 5:
                    a += ((ulong) k[4] << 32);
                    goto case 4;
                case 4:
                    a += ((ulong) k[3] << 24);
                    goto case 3;
                case 3:
                    a += ((ulong) k[2] << 16);
                    goto case 2;
                case 2:
                    a += ((ulong) k[1] << 8);
                    goto case 1;
                case 1:
                    a += ((ulong) k[0]);
                    /* case 0: nothing left to add */
                    break;
            }

            Mix64(ref a, ref b, ref c);
            /*-------------------------------------------- report the result */
            return c;
        }

        public static unsafe ulong Hash(byte[] k, ulong length, ulong level)
        {
            if ((ulong)k.Length < length)
            {
                throw new ArgumentOutOfRangeException("k is longer than length!");
            }

            fixed (byte* ptr = k)
            {
                return Hash64Impl(ptr, length, level);
            }
        }

        public static ulong HashString(string input, ulong level = 0)
        {
            return Hash(UTF8Encoding.UTF8.GetBytes(input), (ulong) UTF8Encoding.UTF8.GetByteCount(input), level);
        }
    }
}
