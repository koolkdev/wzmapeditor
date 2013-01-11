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
    enum MapToolTipCornerType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    class MapToolTipCorner : MapItem
    {
        public MapToolTip ToolTip;
        public MapToolTipCornerType type;

        private static double Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

        }
        public override bool IsPointInArea(int x, int y)
        {
            return Distance(x, y, GetX(), GetY()) <= 5;
        }

        public int GetX()
        {
            return Map.Instance.CenterX + Object.GetInt("x" + GetXID().ToString());
        }

        public int GetY()
        {
            return Map.Instance.CenterY + Object.GetInt("y" + GetYID().ToString());
        }

        public int GetXID()
        {
            switch (type)
            {
                case MapToolTipCornerType.TopRight: return 1;
                case MapToolTipCornerType.BottomRight: return 1;
                case MapToolTipCornerType.TopLeft: return 2;
                case MapToolTipCornerType.BottomLeft: return 2;
            }
            throw new Exception();
        }
        public int GetYID()
        {
            switch (type)
            {
                case MapToolTipCornerType.TopRight: return 1;
                case MapToolTipCornerType.BottomRight: return 2;
                case MapToolTipCornerType.TopLeft: return 1;
                case MapToolTipCornerType.BottomLeft: return 2;
            }
            throw new Exception();
        }

        public override void Move(int x, int y)
        {
            ToolTip.Fix();
            Object.SetInt("x" + GetXID().ToString(), Object.GetInt("x" + GetXID().ToString()) + x);
            Object.SetInt("y" + GetYID().ToString(), Object.GetInt("y" + GetYID().ToString()) + y);
        }
        public int GetConnected()
        {
            return (ID == 1) ? Object.GetInt("prev") : Object.GetInt("next");
        }
        public override void Draw(Graphics g) { }

        public override void Draw(DevicePanel d)
        {
            d.DrawCircle(GetX(), GetY(), Color.FromArgb(Transparency, (Selected || ToolTip.Selected) ? Color.Blue : Color.MediumTurquoise));
        }
    }
}
