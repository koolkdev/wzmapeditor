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

namespace WZ
{
    public class WZPatch : File
    {
        public static Crc32 Crc32 = new Crc32();

        public List<WZPatchFile> files = new List<WZPatchFile>();
        
        public WZPatch()
        {
        }
        public WZPatch(string patch)
        {
            // Open and write to temp file the unpacked patch
        }

        public void Patch(WZFile file)
        {
        }

        public void Save(string filename)
        {
            String tempName = Path.GetTempFileName();

            BinaryWriter temp = new BinaryWriter(System.IO.File.OpenWrite(tempName));

            Write(temp);

            temp.Close();

            BinaryReader readTemp = new BinaryReader(System.IO.File.OpenRead(tempName));

            byte[] bytes = new byte[readTemp.BaseStream.Length];

            readTemp.Read(bytes, 0, bytes.Length);

            byte[] compressed = WZCanvas.StrongCompress(bytes);

            BinaryWriter file = new BinaryWriter(System.IO.File.Create(filename));

            byte[] header = {0x57, 0x7A, 0x50, 0x61, 0x74, 0x63, 0x68, 0x1A, 0x02, 0x00, 0x00, 0x00};

            file.Write(header);
            file.Write(Crc32.Compute(compressed));
            file.Write(compressed);

            file.Close();

            readTemp.Close();

            System.IO.File.Delete(tempName);
        }

        public override void Write(BinaryWriter file)
        {
            foreach (WZPatchFile f in files)
            {
                f.Write(file);
            }

        }

    }
}
