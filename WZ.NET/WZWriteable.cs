/* 
  WZ.NET library
 
  Copyright (c) 2009-2013 koolk

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

     1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.

     2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.

     3. This notice may not be removed or altered from any source
     distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using WZ.Objects;

namespace WZ
{
    public abstract class WZWriteable
    {
        public abstract void Write(BinaryWriter file);

        protected static Hashtable tStrings = new Hashtable();
        protected static Hashtable dirOffsets = new Hashtable();
        protected static Hashtable imgOffsets = new Hashtable();
        protected static Hashtable imgsFiles = new Hashtable();
        protected static int BaseOffset;

        internal uint RotateLeft(uint x, byte n)
        {
            return ((x << n) | (x >> (0x20 - n)));
        }

        internal static int Decode(string s)
        {
            string n = "";
            foreach(char c in s)
            {
                n += (char)(c - 1);
            }
            return Convert.ToInt32(n, 16);
        }

        internal static int GetNum()
        {
            return Decode(WZCanvas.t.ToString() + WZConvex.t + WZDouble.t + WZEmpty.t + WZFloat.t + WZInteger.t + WZProperty.t + WZShort.t + WZSound.t + WZString.t);//"1y692D4G7E");
        }

        protected void WriteOffsetAt(BinaryWriter file, int at, int offset)
        {

            long pos = file.BaseStream.Position; ;
            file.BaseStream.Seek(at, SeekOrigin.Begin);

            int nOffset = (int)((~(file.BaseStream.Position)) * WZFile.VersionSum - GetNum());
            //!int nOffset = (int)((~(file.BaseStream.Position)) * WZFile.VersionSum - 0x581C3F6D);

            nOffset = (int)RotateLeft((uint)nOffset, (byte)(nOffset & 0x1F));
            file.Write((int)(nOffset ^ (offset - BaseOffset)));

            file.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        protected void WriteIntAt(BinaryWriter file, int at, int value)
        {
            long pos = file.BaseStream.Position;
            file.BaseStream.Seek(at, SeekOrigin.Begin);

            file.Write(value);

            file.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        protected void WriteLongAt(BinaryWriter file, int at, long value)
        {
            long pos = file.BaseStream.Position;
            file.BaseStream.Seek(at, SeekOrigin.Begin);

            file.Write(value);

            file.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        protected void WritePackedInt(BinaryWriter file, int value)
        {
            if (value > SByte.MinValue && value <= SByte.MaxValue)
            {
                file.Write((byte)value);
            }
            else
            {
                file.Write(SByte.MinValue);
                file.Write(value);
            }
        }
        protected void WritePackedFloat(BinaryWriter file, float value)
        {
            if (value == 0) file.Write((byte)0);
            else
            {
                file.Write(SByte.MinValue);
                file.Write(value);
            }
        }
        protected void WriteString(BinaryWriter file, string str)
        {
            int len = str.Length;
            // TODO unicode (check if there is a unicode char)

            if (len <= SByte.MaxValue)
            {
                WritePackedInt(file, -len);
            }
            else
            {
                WritePackedInt(file, len);
            }

            byte[] bytes = new byte[str.ToCharArray().Length];
            int i = 0;
            foreach (char ch in str.ToCharArray())
            {
                bytes[i] = (byte)ch;
                i++;
            }

            byte key = 0xAA;

            for (i = 0; i < str.Length; i++)
                bytes[i] ^= (byte)(File.Key[i] ^ key++);

            file.Write(bytes);
        }

        protected void WriteString(BinaryWriter file, string str, int len)
        {
            int c = 0;
            foreach (char ch in str.ToCharArray())
            {
                if (c++ < len)
                {
                    file.Write((byte)ch);
                }
            }
        }

        protected void WriteString(BinaryWriter file, string str, int n, int e)
        {
            WriteString(file, str, n, e, false);
        }

        protected void WriteString(BinaryWriter file, string str, int n, int e, bool wFlag)
        {
            if (tStrings.Contains(str))
            {
                file.Write((byte)e);
                file.Write((int)tStrings[str]);
            }
            else
            {
                if (wFlag) tStrings[str] = (int)file.BaseStream.Position;
                file.Write((byte)n);
                if (!wFlag) tStrings[str] = (int)file.BaseStream.Position;
                WriteString(file, str);
            }
        }
    }
}
