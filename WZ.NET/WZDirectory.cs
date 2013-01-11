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
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using WZ.Operation;

namespace WZ
{
    public class WZDirectory : WZWriteable
    {
        private SortedList _Directories = new SortedList();
        private SortedList _IMGs = new SortedList();

        public SortedList Directories 
        { 
            get
            { 
                if (!loaded)
                    Open();
                return _Directories;
            }
        }
        public SortedList IMGs
        {
            get
            {
                if (!loaded)
                    Open();
                return _IMGs;
            }
        }

        public string Name;

        public int baseOffset;
        WZFile file;

        WZDirectory parent;

        public int Size;

        bool loaded = false;

        public WZDirectory(string Name, WZFile file) : this(Name, file, 0, null, 0) { }
        public WZDirectory(string Name, WZFile file, int baseOffset) : this(Name, file, baseOffset, null, 0) { }
        public WZDirectory(string Name, WZFile file, int baseOffset, WZDirectory parent, int Size)
        {
            this.Name = Name;
            this.baseOffset = baseOffset;
            this.file = file;
            this.Size = Size;
            this.parent = parent;
        }

        public WZDirectory Open()
        {
            if (!loaded)
            {
                loaded = true;

                long soffset = file.file.BaseStream.Position;

                file.file.BaseStream.Seek(baseOffset + file.FileStart, SeekOrigin.Begin);

                int count = file.ReadValue();

                for (int i = 0; i < count; i++)
                {
                    string Name = "";
                    int size, checksum, offset;
                    byte type = file.ReadByte();

                    switch (type)
                    {
                        case 0x02: Name = file.ReadStringAt(true); break;
                        case 0x03:
                        case 0x04: Name = file.ReadString(); break;
                    }

                    size = file.ReadValue();
                    checksum = file.ReadValue();
                    offset = file.ReadOffset();

                    switch (type)
                    {
                        case 0x02:
                        case 0x04: IMGs.Add(Name, new IMGFile(Name, file, offset, this, size, checksum)); break;
                        case 0x03: Directories.Add(Name, new WZDirectory(Name, file, offset, this, size)); break;
                    }
                }

                file.file.BaseStream.Seek(soffset, SeekOrigin.Begin);
            }
            return this;
        }

        public WZDirectory GetDirectory(string name)
        {
            int find = name.IndexOf("/");
            if (find != -1)
            {
                string tof = name.Substring(0, find);
                if (Directories.ContainsKey(tof))
                {
                    return ((WZDirectory)Directories[tof]).GetDirectory(name.Substring(find + 1));
                }
            }
            else if (Directories.ContainsKey(name))
            {
                return (WZDirectory)Directories[name];
            }
            return null;
        }

        public IMGFile GetIMG(string name)
        {
            int find = name.IndexOf("/");
            if (find != -1)
            {
                string tof = name.Substring(0, find);
                if (Directories.ContainsKey(tof))
                {
                    return ((WZDirectory)Directories[tof]).GetIMG(name.Substring(find + 1));
                }
            }
            else if (IMGs.ContainsKey(name))
            {
                return (IMGFile)IMGs[name];
            }
            return null;
        }

        public void CreateTempIMGs()
        {
            foreach (DictionaryEntry entry in Directories)
            {
                WZDirectory directory = (WZDirectory)entry.Value;
                directory.CreateTempIMGs();
            }
            foreach (DictionaryEntry entry in IMGs)
            {
                IMGFile img = (IMGFile)entry.Value;
                if (img.ToSave)
                {
                    img.CreateTemp();
                }
            }
        }

        private int CalculateIMGsSize()
        {
            int size = 0;
            foreach (DictionaryEntry entry in Directories)
            {
                WZDirectory directory = (WZDirectory)entry.Value;
                size += directory.CalculateIMGsSize();
            }
            foreach(DictionaryEntry entry in IMGs)
            {
                IMGFile img = (IMGFile)entry.Value;
                size += img.Size;
            }
            return size;
        }
        public string GetFullName()
        {
            if (parent == null) return Name;
            return parent.GetFullName() + "/" + Name;
        }

        public override void Write(BinaryWriter file)
        {
            if (dirOffsets.Contains(this))
            {
                WriteOffsetAt(file, (int)dirOffsets[this], (int)file.BaseStream.Position);
            }

            WritePackedInt(file, Directories.Count + IMGs.Count);

            foreach (DictionaryEntry entry in Directories)
            {
                WZDirectory directory = (WZDirectory)entry.Value;
                WriteString(file, directory.Name, 3, 1, true);
                WritePackedInt(file, directory.CalculateIMGsSize());
                WritePackedInt(file, 0); // checksum
                dirOffsets[directory] = (int)file.BaseStream.Position;
                file.Write((int)0); // keep place for the offset
            }

            foreach (DictionaryEntry entry in IMGs)
            {
                IMGFile img = (IMGFile)entry.Value;
                img.CalculateChecksum();
                WriteString(file, img.Name, 4, 2, true);
                WritePackedInt(file, img.Size);
                WritePackedInt(file, img.Checksum);
                imgOffsets[img] = (int)file.BaseStream.Position;
                file.Write((int)0); // keep place for the offset
            }

            foreach (DictionaryEntry entry in Directories)
            {
                WZDirectory directory = (WZDirectory)entry.Value;
                directory.Write(file);
            }
            /*foreach (DictionaryEntry entry in IMGs)
            {
                IMGFile img = (IMGFile)entry.Value;
                img.Write(file);
            }*/
        }

        public void WriteIMGs(BinaryWriter file)
        {
            
            foreach (DictionaryEntry entry in IMGs)
            {
                IMGFile img = (IMGFile)entry.Value;
                img.Write(file);
            }
             

            foreach (DictionaryEntry entry in Directories)
            {
                WZDirectory directory = (WZDirectory)entry.Value;
                directory.WriteIMGs(file);
            }
          
        }

        public void Open(WZDirectory other, List<WZPatchOperation> operations)
        {
            long soffset = file.file.BaseStream.Position;

            List<WZPatchOperation> ops = new List<WZPatchOperation>();
            file.file.BaseStream.Seek(baseOffset + file.FileStart, SeekOrigin.Begin);

            Open(other, ops, "");

            int size = (int)file.file.BaseStream.Position;

            operations.Add(new Add(file, 0, size));

            operations.AddRange(ops);

            file.file.BaseStream.Seek(soffset, SeekOrigin.Begin);
        }

        public void Open(WZDirectory other, List<WZPatchOperation> operations, string Base)
        {
            int count = file.ReadValue();

            List<string> names = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string Name = "";
                int size, checksum, offset;
                byte type = file.ReadByte();

                switch (type)
                {
                    case 0x02: Name = file.ReadStringAt(true); break;
                    case 0x03:
                    case 0x04: Name = file.ReadString(); break;
                }

                size = file.ReadValue();
                checksum = file.ReadValue();
                offset = file.ReadOffset();

                switch (type)
                {
                    case 0x02:
                    case 0x04:
                        {
                            IMGFile img = other.GetIMG(Base + ((Base == "") ? "" : "/") + Name);
                            if (img == null || img.Checksum != checksum)
                            {
                                operations.Add(new Add(file, offset + file.FileStart, size));
                            }
                            else
                            {
                                operations.Add(new Copy(other.file, img.baseOffset + other.file.FileStart, img.Size));
                            }
                            break;
                        }
                    case 0x03:
                        {
                            names.Add(Name);
                            break;
                        }
                }
            }
            foreach (string name in names)
            {
                Open(other, operations, Base + ((Base == "") ? "" : "/") + name);
            }
        }

        public void Close()
        {
            foreach(IMGFile img in IMGs.Values)
            {
                if (img.IsLoaded())
                {
                    img.Close();
                }
            }
            foreach(WZDirectory dir in Directories.Values)
            {
                dir.Close();
            }
        }
    }
}
