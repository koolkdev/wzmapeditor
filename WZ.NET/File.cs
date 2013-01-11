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
using System.Runtime.InteropServices;

namespace WZ
{
    public class File : WZWriteable
    {
        [DllImport("SomeC++Stuffs.dll", EntryPoint = "Decrypt")]
        private static extern void Decrypt(IntPtr buffer, int bufferLength, IntPtr key, IntPtr iv);

        public BinaryReader file;
        protected string fileName, path;
        public int FileStart;

        static public byte[] Key = new byte[0xffff];

        public File()
        {
        }

        public File(string path)
        {
            this.path = path;
            fileName = Path.GetFileName(path);
            fileName = fileName.ToLower();
            file = new BinaryReader(System.IO.File.OpenRead(path));
        }

        public static void GetPointer(byte[] buffer, int bufferOffset, ref GCHandle bufferHandle, ref IntPtr bufferPtr)
        {
            if (buffer == null)
                return;
            bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            bufferPtr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, bufferOffset);
        }
        public static void ReleasePointer(GCHandle bufferHandle)
        {
            if (bufferHandle.IsAllocated)
                bufferHandle.Free();
        }

        static public void InitializeKey()
        {

            byte [] key = { 0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00 };
            byte[] iv = { 0x4D, 0x23, 0xc7, 0x2b };

            GCHandle bufferHandle = new GCHandle();
            IntPtr bufferPtr = IntPtr.Zero;
            GCHandle keyHandle = new GCHandle();
            IntPtr keyPtr = IntPtr.Zero;
            GCHandle ivHandle = new GCHandle();
            IntPtr ivPtr = IntPtr.Zero;
            try
            {
                GetPointer(Key, 0, ref bufferHandle, ref bufferPtr);
                GetPointer(key, 0, ref keyHandle, ref keyPtr);
                GetPointer(iv, 0, ref ivHandle, ref ivPtr);
                Decrypt(bufferPtr, 0xffff, keyPtr, ivPtr);
            }
            finally
            {
                ReleasePointer(bufferHandle);
                ReleasePointer(keyHandle);
                ReleasePointer(ivHandle);
            }
        }

        public void Close()
        {
            file.Close();
        }

        public string DecodeString(int length)
        {
            byte[] bytes = file.ReadBytes(length);

            byte key = 0xAA;

            for (int i = 0; i < length; i++)
                bytes[i] ^= (byte)(Key[i] ^ key++);

            char[] chars = new char[length]; ;

            for (int i = 0; i < length; i++)
                chars[i] = (char)bytes[i];

            return new String(chars);
        }

        public string DecodeUnicodeString(int length)
        {
            byte[] bytes = file.ReadBytes(length*2);

            ushort key = 0xAAAA;

            for (int i = 0; i < length; i++)
            {
                bytes[i * 2] ^= (byte)(Key[i * 2] ^ (key % 0x100));
                bytes[i * 2 + 1] ^= (byte)(Key[i * 2 + 1] ^ (key++ / 0x100));
            }

            return Encoding.Unicode.GetString(bytes);
        }

        public int ReadInt()
        {
            return file.ReadInt32();
        }

        public short ReadShort()
        {
            return file.ReadInt16();
        }

        public byte ReadByte()
        {
            return file.ReadByte();
        }

        public double ReadDouble()
        {
            return file.ReadDouble();
        }

        public float ReadFloat()
        {
            return file.ReadSingle();
        }

        public long ReadLong()
        {
            return file.ReadInt64();
        }

        public int ReadValue()
        {
            sbyte value = (sbyte)ReadByte();
            if (value == SByte.MinValue) return ReadInt();
            return value;
        }

        public float ReadPackedFloat()
        {
            sbyte value = (sbyte)ReadByte();
            if (value == SByte.MinValue) return ReadFloat();
            if (value == 0) return 0;
            throw new Exception("Invaild packed float type.");
        }


        public string ReadString()
        {
            sbyte size = (sbyte)ReadByte();

            int fsize = 0;
          
            if (size > 0)
            {
                if (size == SByte.MaxValue) fsize = ReadInt();
                else fsize = size;
                return DecodeUnicodeString(fsize);
            }
            else if (size < 0)
            {
                if (size == SByte.MinValue) fsize = ReadInt();
                else fsize = -size;
                return DecodeString(fsize);
            }

            return "";
        }

        public string ReadString(int length)
        {
            return file.ReadBytes(length).ToString();
        }

        public string ReadStringAt(bool flag)
        {
            return ReadStringAt(0, flag);
        }

        public string ReadStringAt(long baseOffset)
        {
            return ReadStringAt(baseOffset, false);
        }

        public string ReadStringAt(long baseOffset, bool flag)
        {
            long offset = ReadInt();
            long pos = file.BaseStream.Position;
            
            file.BaseStream.Seek(baseOffset + offset + FileStart, SeekOrigin.Begin);

            if (flag) ReadByte();

            string str = ReadString();

            file.BaseStream.Seek(pos, SeekOrigin.Begin);

            return str;
        }

        public override void Write(BinaryWriter file)
        {
            throw new NotImplementedException();
        }
    }
}
