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
    class MapPortal : MapItem
    {
        public override bool IsPointInArea(int x, int y)
        {
            if (Object.GetChild("script") != null) return false;
            switch(Object.GetInt("pt"))
            {
                case 0: 
                case 1:
                case 3:
                case 10: return MapFoothold.Distance(x, y, Map.Instance.CenterX + Object.GetInt("x"), Map.Instance.CenterY + Object.GetInt("y")) <= 5;
                case 2:
                    {
                        int topLeftX = Map.Instance.CenterX + Object.GetInt("x") - Image.GetVector("origin").x;
                        int topLeftY = Map.Instance.CenterY + Object.GetInt("y") - Image.GetVector("origin").y;
                        int width = Image.GetCanvas().width;
                        int height = Image.GetCanvas().height;
                        if (x >= topLeftX && x < topLeftX + width && y >= topLeftY && y < topLeftY + height)
                        {
                            return Image.GetCanvas().GetBitmap().GetPixel(x - topLeftX, y - topLeftY).A > 0;
                        }
                        return false;
                    }
                default: return false;
            }
        }


        public override void Move(int x, int y)
        {
            Object.SetInt("x", Object.GetInt("x") + x);
            Object.SetInt("y", Object.GetInt("y") + y);
        }
        public override void Draw(Graphics g) 
        {
            int type = Object.GetInt("pt");

            int cx = Map.Instance.CenterX;
            int cy = Map.Instance.CenterY;
            switch (Object.GetInt("pt"))
            {
                case 2:
                    g.DrawImage(Image.GetCanvas().GetBitmap(), cx + Object.GetInt("x") - Image.GetVector("origin").x, cy + Object.GetInt("y") - Image.GetVector("origin").y); break;
            }
        }

        public override void Draw(DevicePanel d)
        {
            if (Object.GetChild("script") != null) return;
            int type = Object.GetInt("pt");
            bool arrow = ((type == 1 || type == 3 || type == 10) && Object.GetInt("tm") == int.Parse(MapEditor.Instance.MapID) && Map.Instance.GetPortal(Object.GetString("tn")) != null);

            int cx = Map.Instance.CenterX;
            int cy = Map.Instance.CenterY;
            switch (Object.GetInt("pt"))
            {
                case 0:
                case 1:
                case 10:
                    d.DrawCircle(cx + Object.GetInt("x"), cy + Object.GetInt("y"), Color.FromArgb(Transparency, (Selected) ? Color.Blue : Color.RoyalBlue)); break;
                case 3:
                    d.DrawCircle(cx + Object.GetInt("x"), cy + Object.GetInt("y"), Color.FromArgb(Transparency, (Selected) ? Color.Blue : Color.RoyalBlue)); 
                    d.DrawEmptyCircle(cx + Object.GetInt("x"), cy + Object.GetInt("y"), Color.FromArgb(Transparency, (Selected) ? Color.Blue : Color.RoyalBlue)); break;
                case 2:
                    d.DrawBitmap(Image.GetCanvas().GetTexture(d._device), cx + Object.GetInt("x") - Image.GetVector("origin").x, cy + Object.GetInt("y") - Image.GetVector("origin").y, Image.GetCanvas().width, Image.GetCanvas().height, Selected, (Transparency == 50) ? 100 : Transparency); break;
            }

            if (arrow)
            {
                MapPortal p = Map.Instance.GetPortal(Object.GetString("tn"));
                int x = p.Object.GetInt("x");
                int y = p.Object.GetInt("y");
                double di = MapFoothold.Distance(Object.GetInt("x"), Object.GetInt("y"), x, y);
                if (di != 0)
                {
                    double xp = (Object.GetInt("x") * 5 + x * (di - 5)) / di;
                    double yp = (Object.GetInt("y") * 5 + y * (di - 5)) / di;
                    d.DrawArrow(Object.GetInt("x") + cx, Object.GetInt("y") + cy, (int)xp + cx, (int)yp + cy, Color.FromArgb(Transparency, (Selected) ? Color.Blue : Color.RoyalBlue));
                }
            }
        }
    }
}
