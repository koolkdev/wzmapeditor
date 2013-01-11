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

namespace WZ.Operation
{
    class Copy : WZPatchOperation
    {
        WZFile source;
        int offset;
        int size;

        public Copy(WZFile source, int offset, int size)
        {
            this.source = source;
            this.offset = offset;
            this.size = size ;
        }
        public void Patch(BinaryWriter file)
        {
            byte[] bytes = new byte[size];
            long pos = source.file.BaseStream.Position;
            source.file.BaseStream.Seek(offset, SeekOrigin.Begin);
            source.file.Read(bytes, 0, size);
            source.file.BaseStream.Seek(pos, SeekOrigin.Begin);
            file.Write(bytes);
        }

        public void Write(BinaryWriter file)
        {
            file.Write(size);
            file.Write(offset);
        }
    }
}
