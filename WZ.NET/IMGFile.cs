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
using WZ.Objects;
using System.Collections;

namespace WZ
{
    public class IMGFile : IMGEntry
    {
        public int Size;
        public int baseOffset;
        public WZDirectory Directory;
        public int Checksum;

        File file;

        public IMGFile(string Name)
        {
            loaded = true;
            this.Name = Name;
            ToSave = true;
        }
        public IMGFile(string Name, File file) : this(Name, file, 0, null, 0, 0) { }
        public IMGFile(string Name, File file, int baseOffset, WZDirectory parent, int Size, int checksum)
        {
            loaded = false;
            this.Name = Name;
            this.baseOffset = baseOffset;
            this.file = file;
            this.Size = Size;
            this.Directory = parent;
            this.Checksum = checksum;
        }

        public void CalculateChecksum()
        {
            if (ToSave)
            {
                if (imgsFiles.Contains(this))
                {
                    BinaryReader temp = new BinaryReader(System.IO.File.OpenRead((string)imgsFiles[this]));

                    CalculateChecksum(temp);

                    temp.Close();
                }
            }
            else
            {
                if (Checksum == 0)
                {
                    long pos = this.file.file.BaseStream.Position;

                    this.file.file.BaseStream.Seek(baseOffset + this.file.FileStart, SeekOrigin.Begin);

                    Checksum = 0;

                    for (int i = 0; i < Size; i++)
                    {
                        Checksum += file.ReadByte();
                    }

                    this.file.file.BaseStream.Seek(pos, SeekOrigin.Begin);
                }
            }
        }

        public void CalculateChecksum(BinaryReader file)
        {
            Checksum = 0;

            for (int i = 0; i < file.BaseStream.Length; i++)
            {
                Checksum += file.ReadByte();
            }
        }

        public override IMGFile Open()
        {
            loaded = true;

            long offset = file.file.BaseStream.Position;

            file.file.BaseStream.Seek(baseOffset + file.FileStart, SeekOrigin.Begin);

            CreateEntry(this, (WZProperty)ReadComplex());

            file.file.BaseStream.Seek(offset, SeekOrigin.Begin);

            return this;
        }

        string ReadString()
        {
            switch (file.ReadByte())
            {
                case 0x00:
                case 0x73: return file.ReadString();
                case 0x01:
                case 0x1B: return file.ReadStringAt(baseOffset);
                default: throw new Exception("Invalid string type.");
            }
        }

        WZObject ReadObject()
        {
            string name = ReadString();
            switch (file.ReadByte())
            {
                case 0x00: return new WZEmpty(name);
                case 0x02: return new WZShort(name, file.ReadShort());
                case 0x03: return new WZInteger(name, file.ReadValue());
                case 0x04: return new WZFloat(name, file.ReadPackedFloat());
                case 0x05: return new WZDouble(name, file.ReadDouble());
                case 0x08: return new WZString(name, ReadString());
                case 0x09: int size = file.ReadInt(); /* size */ return new WZComplex(name, ReadComplex(size + (int)file.file.BaseStream.Position)); // size
                default: throw new Exception("Invaild simple object type.");		
            }
        }

        // temp
        WZObject ReadComplex()
        {
            return ReadComplex(0);
        }

        WZObject ReadComplex(int end)
        {
            string type = ReadString();
            switch (type)
            {
                case "Property":
                    {
                        WZProperty p = new WZProperty();
                        file.ReadShort(); // null

                        int count = file.ReadValue();

                        for (int i = 0; i < count; i++)
                        {
                            p.objects.Add(ReadObject());
                        }

                        if (end != 0 && end != file.file.BaseStream.Position) throw new Exception("...");
                        return p;
                    }
                case "Shape2D#Convex2D":
                    {
                        WZConvex c = new WZConvex();

                        int count = file.ReadValue();

                        for (int i = 0; i < count; i++)
                        {
                            c.objects.Add(ReadComplex());
                        }

                        if (end != 0 && end != file.file.BaseStream.Position) throw new Exception("...");

                        return c;
                    }
                case "Shape2D#Vector2D":
                    {
                        int x = file.ReadValue();
                        int y = file.ReadValue();


                        if (end != 0 && end != file.file.BaseStream.Position) throw new Exception("...");
                        return new WZVector(x, y);
                    }
                case "Canvas":
                    {
                        WZCanvas c = new WZCanvas();
                        c.file = file.file;

                        file.ReadByte(); // null

                        if (file.ReadByte() != 0)
                        {
                            file.ReadShort(); // null
                            int count = file.ReadValue();

                            for (int i = 0; i < count; i++)
                            {
                                c.objects.Add(ReadObject());
                            }
                        }

                        c.width = file.ReadValue();
                        c.height = file.ReadValue();
                        int format = file.ReadValue();
                        c.format = (WZCanvas.ImageFormat)(file.ReadValue() + format);
                        file.ReadInt(); // null
                        c.size = file.ReadInt();
                        c.offset = (int)file.file.BaseStream.Position;
                        file.file.BaseStream.Seek(c.size, SeekOrigin.Current);

                        if (end != 0 && end != file.file.BaseStream.Position) throw new Exception("...");

                        return c;
                    }
                case "Sound_DX8":
                    {
                        // TODO
                        file.file.BaseStream.Seek(end, SeekOrigin.Begin);

                        if (end != 0 && end != file.file.BaseStream.Position) throw new Exception("...");
                        return new WZSound();
                    }
                case "UOL":
                    {
                        file.ReadByte(); // null

                        //if (end != 0 && end != file.file.BaseStream.Position) throw new Exception("...");

                        return new WZUOL(ReadString());
                    }

            }
            throw new Exception("Invalid Object type: " + type);
        }

        public void Save(string fileName)
        {
            BinaryWriter write = new BinaryWriter(System.IO.File.OpenWrite(fileName));
            Write(write);
            write.Close();
        }

        public override void Write(BinaryWriter file)
        {
            tStrings.Clear();

            if(imgOffsets.Contains(this))
            {
                WriteOffsetAt(file, (int)imgOffsets[this], (int)file.BaseStream.Position);
            }

            if (ToSave)
            {
                if(imgsFiles.Contains(this))
                {
                    BinaryReader temp = new BinaryReader(System.IO.File.OpenRead((string)imgsFiles[this]));

                    file.Write(temp.ReadBytes((int)temp.BaseStream.Length));

                    temp.Close();

                    System.IO.File.Delete((string)imgsFiles[this]);
                }
                else
                {
                    WZObject.FromEntry(this).Write(file);
                }
            }
            else
            {
                long pos = this.file.file.BaseStream.Position;

                this.file.file.BaseStream.Seek(baseOffset + this.file.FileStart, SeekOrigin.Begin);

                file.Write(this.file.file.ReadBytes(Size));

                this.file.file.BaseStream.Seek(pos, SeekOrigin.Begin);
            }
        }

        public void CreateTemp()
        {
            String tempName = Path.GetTempFileName();

            BinaryWriter temp = new BinaryWriter(System.IO.File.OpenWrite(tempName));

            Write(temp);

            imgsFiles[this] = tempName;

            Size = (int)temp.BaseStream.Length;

            temp.Close();
        }

        public string GetFullName()
        {
            if(Directory == null) return Name;
            return Directory.GetFullName() + "/" + Name;
        }

        public bool IsLoaded()
        {
            return loaded;
        }

        public void Close()
        {
            if (loaded && file != null)
            {
                Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                loaded = false;
            }
        }
    }
}
