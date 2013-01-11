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
using System.Drawing;

namespace WZMapEditor
{
    abstract class MapTileDesign : MapTile
    {
        public string type;

        public List<MapTileDesignPotential> potentials = new List<MapTileDesignPotential>();

        public MapTileDesign()
        {
        }

        protected void SetImage()
        {
            Image = MapEditor.file.Directory.GetIMG("Tile/blackTile.img").GetChild(type + "/0");
        }

        public Point GetMath(string type, int x, int y, int multi)
        {
            x -= Object.GetInt("x");
            y -= Object.GetInt("y");
            foreach (MapTileDesignPotential p in potentials)
            {
                if (p.IsMatch(type, x, y, multi))
                {
                    return new Point(p.x * multi + Object.GetInt("x"), p.y * multi + Object.GetInt("y"));
                }
            }
            return new Point(0xffff, 0xffff);
        }
    }
}
