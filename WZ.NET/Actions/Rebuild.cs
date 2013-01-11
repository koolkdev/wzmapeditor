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

namespace WZ.Actions
{
    class Rebuild : WZPatchFile
    {
        public List<WZPatchOperation> operations = new List<WZPatchOperation>();
        
        WZFile o, n;
        public Rebuild(string name)
            : base(name)
        {
        }

        public Rebuild(string name, WZFile oldWZ, WZFile newWZ)
            : base(name)
        {
            o = oldWZ;
            n = newWZ;
        }

        public override void Patch(BinaryWriter file)
        {
            // TODO: patch
        }

        public override void Write(BinaryWriter file)
        {
            base.Write(file);

            file.Write((byte)0x01);

            Stream stream = o.file.BaseStream;
            stream.Seek(0, SeekOrigin.Begin);
            file.Write(WZPatch.Crc32.Compute(stream));

            stream = n.file.BaseStream;
            stream.Seek(0, SeekOrigin.Begin);
            file.Write(WZPatch.Crc32.Compute(stream));

            foreach (WZPatchOperation operation in operations)
            {
                operation.Write(file);
            }

            file.Write((int)0);
        }
    }
}
