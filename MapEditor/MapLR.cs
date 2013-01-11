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
    class MapLR : MapItem
    {
        public MapLRSide s1, s2;

        public void Fix()
        {
            if (Object.GetInt("y1") > Object.GetInt("y2"))
            {
                MapFootholds.SwapInt(Object.GetChild("y1"), Object.GetChild("y2"));
                int temp = s1.ID;
                s1.ID = s2.ID;
                s2.ID = temp;
            }
        }

        public override bool IsPointInArea(int x, int y)
        {
            int cX = Map.Instance.CenterX;
            int cY = Map.Instance.CenterY;
            double distance = MapFoothold.DistanceBetweenPointToLine(x, y, cX + Object.GetInt("x"), cY + Object.GetInt("y1"), cX + Object.GetInt("x"), cY + Object.GetInt("y2"));
            return distance <= 5;
        }

        public MapLRSide GetSideAt(int x, int y)
        {
            if (s1.IsPointInArea(x, y)) return s1;
            if (s2.IsPointInArea(x, y)) return s2;
            return null;
        }

        public override void Move(int x, int y)
        {
            throw new NotImplementedException();
        }

        public override void Draw(Graphics g)
        { }
        public override void Draw(DevicePanel d)
        {
            if (!s1.Selected)
            {
                s1.Transparency = Transparency;
            }
            if (!s2.Selected)
            {
                s2.Transparency = Transparency;
            }
            int cX = Map.Instance.CenterX;
            int cY = Map.Instance.CenterY;
            int x = cX + Object.GetInt("x");
            int y1 = cY + Object.GetInt("y1");
            int y2 = cY + Object.GetInt("y2");
            d.DrawLine(x, y1, x, y2, Color.FromArgb(Transparency, (Selected) ? Color.Blue : Color.Green));
            s1.Draw(d);
            s2.Draw(d);
        }

        public int nPage { get { return Object.GetInt("page"); } }
        public int x { get { return Object.GetInt("x"); } }
        public int y1 { get { return Object.GetInt("y1"); } }
        public int y2 { get { return Object.GetInt("y2"); } }
        public bool bUpperFoothold { get { return Object.GetBool("uf"); } }
    }
}
