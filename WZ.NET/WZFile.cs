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

namespace WZ
{
    public class WZFile : File
    {
        public string Name;
        public string Header;
        public string Copyright;
        public long FileSize;
        public byte Version;
        private bool loaded;
        public static int VersionSum;
        public WZDirectory _directory;
        public WZDirectory Directory
        {
            get
            {
                if (!loaded) Open();
                return _directory;
            }
            set
            {
                _directory = value;
            }
        }

        public WZFile(string path, byte Version)
            : base(path)
        {
            loaded = false;

            this.Version = Version;

            Name = Path.GetFileNameWithoutExtension(path);
        }

        public WZFile(string path)
            : base(path)
        {
            loaded = false;

            this.Version = 0;

            Name = Path.GetFileNameWithoutExtension(path);
        }

        public byte DetectVersion()
        {
            file.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

            ReadString(4);
            ReadLong();
            Copyright = ReadString(ReadInt() - (int)Position());
            byte EncodedVersion = ReadByte();
            Version = 0;
            while (EncodedVersion != EncodeVersion() && Version != 0xFF) Version++;

            return Version;
        }

        public void Open()
        {
            if (!loaded)
            {
                file.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                Header = ReadString(4);
                FileSize = ReadLong();
                FileStart = ReadInt();
                Copyright = ReadString(FileStart - (int)Position());
                byte EncodedVersion = ReadByte();
                if (EncodeVersion() != EncodedVersion) throw new Exception("Incorrect version error");
                ReadByte(); // null

                Directory = new WZDirectory(Name, this, 2);

                loaded = true;
            }
        }

        public void Save()
        {
            string tempName = WriteTemp();

            file.Close();

            BinaryWriter wfile = new BinaryWriter(System.IO.File.Create(path));

            WriteHeader(wfile);
            WriteBody(wfile, tempName);
            System.IO.File.Delete(tempName);
            wfile.Close();
        }
        public void Save(string path)
        {
            BinaryWriter file = new BinaryWriter(System.IO.File.Create(path));

            Write(file);
        }

        private string WriteTemp()
        {
            // To initalize BaseOffset
            string tempHeader = Path.GetTempFileName();
            BinaryWriter tempHeaderFile = new BinaryWriter(System.IO.File.Create(tempHeader));
            WriteHeader(tempHeaderFile);
            tempHeaderFile.Close();
            System.IO.File.Delete(tempHeader);

            String tempName = Path.GetTempFileName();

            BinaryWriter temp = new BinaryWriter(System.IO.File.Create(tempName));

            temp.Write((byte)EncodeVersion());
            temp.Write((byte)0);

            Directory.CreateTempIMGs();

            Directory.Write(temp);
            Directory.WriteIMGs(temp);

            temp.Close();

            return tempName;
        }
        public long Position()
        {
            return file.BaseStream.Position;
        }
        private void WriteHeader(BinaryWriter file)
        {
            WriteString(file, "PKG1", 4);

            file.Write((long)0);
            file.Write((int)0);

            //string copyright = "Package file v1.0 Copyright 2002 Wizet, ZMS. Repacked with WZMapEditor by koolk.";
            string copyright = "Package file v1.0 Copyright 2002 Wizet, ZMS";
            WriteString(file, copyright, copyright.Length);

            file.Write((byte)0); // end of string

            WZWriteable.BaseOffset = (int)file.BaseStream.Position;
            WZWriteable.tStrings.Clear();

            WriteIntAt(file, 12, WZWriteable.BaseOffset);
        }
        private void WriteBody(BinaryWriter file, string BodyFile)
        {
            BinaryReader tempRead = new BinaryReader(System.IO.File.OpenRead(BodyFile));
            byte[] block = new byte[1024 * 32];
            int blockSize;
            while ((blockSize = tempRead.BaseStream.Read(block, 0, block.Length)) > 0)
            {
                file.Write(block, 0, blockSize);
            }

            WriteLongAt(file, 4, tempRead.BaseStream.Length);
            tempRead.Close();
        }
        public override void Write(BinaryWriter file)
        {
            WriteHeader(file);

            string tempName = WriteTemp();

            WriteBody(file, tempName);

            System.IO.File.Delete(tempName);

            file.Close();
        }

        public byte EncodeVersion()
        {
            VersionSum = 0;
            foreach (char ch in this.Version.ToString())
            {
                VersionSum = ((VersionSum * 0x20) + ((byte)ch)) + 1;
            }
            int num = (VersionSum >> 0x18) & 0xff;
            int num2 = (VersionSum >> 0x10) & 0xff;
            int num3 = (VersionSum >> 8) & 0xff;
            int num4 = VersionSum & 0xff;

            return (byte)~(((num ^ num2) ^ num3) ^ num4);
        }

        /*internal uint RotateLeft(uint x, byte n)
        {
            return ((x << n) | (x >> (0x20 - n)));
        }*/


        public int ReadOffset()
        {
            int offset = (~((int)Position() - FileStart)) * VersionSum - WZWriteable.GetNum();
            //!int offset = (~((int)file.BaseStream.Position - FileStart)) * VersionSum - 0x581C3F6D;
            offset = (int)RotateLeft((uint)offset, (byte)(offset & 0x1F)) ^ ReadInt();
            return offset + FileStart;
        }
    }
}
