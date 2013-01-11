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
using WZ;

namespace WZMapEditor
{
    abstract class MapLife : MapItem, ICloneable, ICached
    {
        Point lastKnown;

        public int MagnetY;
        public bool Magnet;

        private int _x, _y;
        protected Rectangle _size;
        private bool _f;

        private bool cached = false;

        public override bool IsPointInArea(int x, int y)
        {
            int topLeftX = Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x);
            int topLeftY = Map.Instance.CenterY + Object.GetInt("cy") - Image.GetVector("origin").y;
            int width = Image.GetCanvas().width;
            int height = Image.GetCanvas().height;
            if (x >= topLeftX && x < topLeftX + width && y >= topLeftY && y < topLeftY + height)
            {
                Bitmap b = (Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap();
                return b.GetPixel(x - topLeftX, y - topLeftY).A > 0;
            }
            return false;
        }

        public bool IsObjectInArea(Rectangle area)
        {
            Rectangle ImageArea = new Rectangle(Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x), Map.Instance.CenterY + Object.GetInt("y") - Image.GetVector("origin").y, Image.GetCanvas().width, Image.GetCanvas().height);
            if (ImageArea.Contains(area)) return true;
            Rectangle common = Rectangle.Intersect(ImageArea, area);
            if (common != Rectangle.Empty)
            {
                if (common.Contains(lastKnown)) return true;
                Bitmap b = (Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap();

                bool toSwitchX = MapEditor.Instance.selectingX > common.X;
                bool toSwitchY = MapEditor.Instance.selectingY > common.Y;
                for (int x = toSwitchX ? common.X - ImageArea.X + common.Width - 1 : common.X - ImageArea.X; toSwitchX ? x >= common.X - ImageArea.X : x < common.X - ImageArea.X + common.Width; x += toSwitchX ? -1 : 1)
                {
                    for (int y = toSwitchY ? common.Y - ImageArea.Y + common.Height - 1 : common.Y - ImageArea.Y; toSwitchY ? y >= common.Y - ImageArea.Y : y < common.Y - ImageArea.Y + common.Height; y += toSwitchY ? -1 : 1)
                    {
                        if (b.GetPixel(x, y).A > 0)
                        {
                            lastKnown = new Point(x + ImageArea.X, y + ImageArea.Y);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public override void Move(int x, int y)
        {
            Object.SetInt("x", Object.GetInt("x") + x);
            Object.SetInt("rx0", Object.GetInt("rx0") + x);
            Object.SetInt("rx1", Object.GetInt("rx1") + x);
            Object.SetInt("y", Object.GetInt("y") + y);
            Object.SetInt("cy", Object.GetInt("cy") + y);
            Cache();
        }
        public void Cache()
        {
            _x = Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x);
            _y = Map.Instance.CenterY + Object.GetInt("cy") - Image.GetVector("origin").y;
            _size = new Rectangle(0, 0, Image.GetCanvas().width, Image.GetCanvas().height);
            _f = Object.GetBool("f");
            cached = true;
        }
        public override void Draw(Graphics g)
        {
            if (!cached) Cache();
            g.DrawImage((_f) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x), Map.Instance.CenterY + Object.GetInt("cy") - Image.GetVector("origin").y);
        }

        public override void Draw(DevicePanel d)
        {
            if (!cached) Cache();
            if (Magnet)
            {
                d.DrawBitmap((_f) ? Image.GetCanvas().GetFlippedTexture(d._device) : Image.GetCanvas().GetTexture(d._device), Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x), Map.Instance.CenterY + MagnetY - Image.GetVector("origin").y, _size, Selected, Transparency);
            }
            else
            {
                d.DrawBitmap((_f) ? Image.GetCanvas().GetFlippedTexture(d._device) : Image.GetCanvas().GetTexture(d._device), _x, _y, _size, Selected, Transparency);
            }
        }

        public abstract object Clone();

        public override string ToString()
        {
            return "ID: " + Image.parent.parent.Name.Replace(".img", "");
        }
    }
}
