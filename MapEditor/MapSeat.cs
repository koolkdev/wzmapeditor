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
    class MapSeat : MapItem
    {
        public override bool IsPointInArea(int x, int y)
        {
            return MapFoothold.Distance(x, y, GetX(), GetY()) <= 5;
        }

        public int GetX()
        {
            return Map.Instance.CenterX + Object.GetVector().x;
        }

        public int GetY()
        {
            return Map.Instance.CenterY + Object.GetVector().y;
        }

        public override void Move(int x, int y)
        {
            Object.GetVector().x += x;
            Object.GetVector().y += y;
        }
        public override void Draw(Graphics g) { }

        public override void Draw(DevicePanel d)
        {
            d.DrawCircle(GetX(), GetY(), Color.FromArgb(Transparency, (Selected) ? Color.Blue : Color.DarkOrange));
        }
    }
}
