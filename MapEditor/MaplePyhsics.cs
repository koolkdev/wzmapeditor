/* 
  koolk's Map Editor
 
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
using WZ;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WZMapEditor
{
    class MaplePyhsics
    {
        private IMGEntry physics;

        public bool right, left, up, down, alt;

        public MaplePyhsics(IMGEntry physics, int x, int y)
        {
            this.physics = physics;
            this.x = x;
            this.y = y;
            new Thread(new ThreadStart(UpdateThread)).Start();
        }

        private void UpdateThread()
        {
            while (true)
            {
                int diff = 3;
                if (alt) diff = 10;
                if (up) this.y -= diff;
                if (down) this.y += diff;
                if (right) this.x += diff;
                if (left) this.x -= diff;
                Thread.Sleep(10);
            }
        }

        // REMOVED

        public int x { get; set; }

        public int y { get; set; }
    }
}
