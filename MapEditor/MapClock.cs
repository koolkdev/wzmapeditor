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
using WZ.Objects;

namespace WZMapEditor
{
    class MapClock : MapItem
    {
        public override void Move(int x, int y)
        {
            Object.SetInt("x", Object.GetInt("x") + x);
            Object.SetInt("y", Object.GetInt("y") + y);
        }

        public override bool IsPointInArea(int x, int y)
        {
            return new Rectangle(Object.GetInt("x") + Map.Instance.CenterX + 10, Object.GetInt("y") + Map.Instance.CenterY + 76, Object.GetInt("width") - 8 - 10, Object.GetInt("height") - 78 - 76).Contains(x, y);
        }

        public override void Draw(Graphics g)
        {
        }

        public override void Draw(DevicePanel d)
        {
            if (MapEditor.Instance.EditMode.Checked && MapEditor.Instance.EditClock.Checked)
            {
                d.DrawRectangle(Object.GetInt("x") + Map.Instance.CenterX + 10, Object.GetInt("y") + Map.Instance.CenterY + 76, Object.GetInt("x") + Map.Instance.CenterX + Object.GetInt("width") - 8, Object.GetInt("y") + Map.Instance.CenterY + Object.GetInt("height") - 78, Selected ? Color.FromArgb(150, Color.Blue) : Color.FromArgb(150, 51, 17, 0));
            }
            WZCanvas draw = Image.GetCanvas("am");
            int x = Object.GetInt("x") + 16 + Map.Instance.CenterX, y = Object.GetInt("y") + 82 + Map.Instance.CenterY;
            d.DrawBitmap(draw.GetTexture(d._device), x, y, draw.width, draw.height, Selected, Transparency);
            x += draw.width + 17;
            draw = Image.GetCanvas("0");
            d.DrawBitmap(draw.GetTexture(d._device), x, y, draw.width, draw.height, Selected, Transparency);
            x += draw.width + 2;
            d.DrawBitmap(draw.GetTexture(d._device), x, y, draw.width, draw.height, Selected, Transparency);
            x += draw.width + 2;
            draw = Image.GetCanvas("comma");
            d.DrawBitmap(draw.GetTexture(d._device), x, y, draw.width, draw.height, Selected, Transparency);
            x += draw.width + 2;
            draw = Image.GetCanvas("0");
            d.DrawBitmap(draw.GetTexture(d._device), x, y, draw.width, draw.height, Selected, Transparency);
            x += draw.width + 2;
            d.DrawBitmap(draw.GetTexture(d._device), x, y, draw.width, draw.height, Selected, Transparency);
        }
    }
}
