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
    class MapToolTip : MapItem
    {
        public MapToolTipCorner c1, c2, c3, c4;

        public void Fix()
        {
            if (Object.GetInt("x1") > Object.GetInt("x2"))
            {
                MapFootholds.SwapInt(Object.GetChild("x1"), Object.GetChild("x2"));
                Swap(MapToolTipCornerType.TopLeft, MapToolTipCornerType.TopRight);
                Swap(MapToolTipCornerType.BottomLeft, MapToolTipCornerType.BottomRight);
            }
            if (Object.GetInt("y1") > Object.GetInt("y2"))
            {
                MapFootholds.SwapInt(Object.GetChild("y1"), Object.GetChild("y2"));
                Swap(MapToolTipCornerType.TopLeft, MapToolTipCornerType.BottomLeft);
                Swap(MapToolTipCornerType.TopRight, MapToolTipCornerType.BottomRight);
            }
        }

        private void Swap(MapToolTipCornerType type1, MapToolTipCornerType type2)
        {
            MapToolTipCorner c1 = GetCorner(type1);
            MapToolTipCorner c2 = GetCorner(type2);
            c1.type = type2;
            c2.type = type1;
        }

        private MapToolTipCorner GetCorner(MapToolTipCornerType type)
        {
            if (c1.type == type) return c1;
            if (c2.type == type) return c2;
            if (c3.type == type) return c3;
            if (c4.type == type) return c4;
            return null;
        }

        public override bool IsPointInArea(int x, int y)
        {
            int cX = Map.Instance.CenterX;
            int cY = Map.Instance.CenterY;
            int x1 = cX + Object.GetInt("x1");
            int x2 = cX + Object.GetInt("x2");
            int y1 = cY + Object.GetInt("y1");
            int y2 = cY + Object.GetInt("y2");
            return x >= x1 && x <= x2 && y >= y1 && y <= y2;
        }

        public MapToolTipCorner GetCornerAt(int x, int y)
        {
            if (c1.IsPointInArea(x, y)) return c1;
            if (c2.IsPointInArea(x, y)) return c2;
            if (c3.IsPointInArea(x, y)) return c3;
            if (c4.IsPointInArea(x, y)) return c4;
            return null;
        }
        public MapItem GetItemAt(int x, int y)
        {
            MapItem item = GetCornerAt(x, y);
            if (item == null) if (IsPointInArea(x, y)) return this;
            return item;
        }

        public override void Move(int x, int y)
        {
            Object.SetInt("x1", Object.GetInt("x1") + x);
            Object.SetInt("y1", Object.GetInt("y1") + y);
            Object.SetInt("x2", Object.GetInt("x2") + x);
            Object.SetInt("y2", Object.GetInt("y2") + y);
        }

        public override void Draw(Graphics g)
        { }
        public override void Draw(DevicePanel d)
        {
            if (!c1.Selected)
            {
                c1.Transparency = Transparency;
            }
            if (!c2.Selected)
            {
                c2.Transparency = Transparency;
            }
            if (!c3.Selected)
            {
                c3.Transparency = Transparency;
            }
            if (!c4.Selected)
            {
                c4.Transparency = Transparency;
            }
            int cX = Map.Instance.CenterX;
            int cY = Map.Instance.CenterY;
            int x1 = cX + Object.GetInt("x1");
            int x2 = cX + Object.GetInt("x2");
            int y1 = cY + Object.GetInt("y1");
            int y2 = cY + Object.GetInt("y2");

            Color color = Selected ? Color.Blue : Color.MediumTurquoise;

            d.DrawRectangle(x1, y1, x2, y2, Color.FromArgb(50 * Transparency / 0xFF, color));

            d.DrawLine(x1, y1, x2, y1, Color.FromArgb(Transparency, color));
            d.DrawLine(x1, y1, x1, y2, Color.FromArgb(Transparency, color));
            d.DrawLine(x2, y2, x2, y1, Color.FromArgb(Transparency, color));
            d.DrawLine(x2, y2, x1, y2, Color.FromArgb(Transparency, color));

            c1.Draw(d);
            c2.Draw(d);
            c3.Draw(d);
            c4.Draw(d);

            d.DrawText(Image.GetString("Title"), x1 + 7, y1 + 2, 0, Color.FromArgb(Transparency, color), true);
            d.DrawText(Image.GetString("Desc"), x1 + 7, y1 + 14, x2 - x1 - 14, Color.FromArgb(Transparency, color), false);
        }
    }
}
