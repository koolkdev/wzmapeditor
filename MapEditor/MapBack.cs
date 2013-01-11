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
    class MapBack : MapItem
    {
        Point lastKnown;

        List<MapBackFrame> frames = null;
        int animationTime = 0;

        private int _x, _y, _mX, _mY;
        private double _rX, _rY;
        private Rectangle _size;
        private bool _f;

        private bool cached;

        public override void DrawAnimation(DevicePanel d)
        {
            if (animationTime == 0)
            {
                Draw(d);
                return;
            }
            int time = System.Environment.TickCount % animationTime;

            foreach (MapBackFrame frame in frames)
            {
                time -= frame.Image.GetInt("delay");
                if (time < 0)
                {
                    frame.Draw(d);
                    break;
                }
            }
        }

        public void GenerateFrames()
        {
            frames = new List<MapBackFrame>();
            IMGEntry originalImage = Image.parent;
            foreach (IMGEntry frame in originalImage.childs.Values)
            {
                MapBackFrame f = new MapBackFrame();
                f.ID = int.Parse(frame.Name);
                f.Image = Map.GetRealImage(frame);
                f.Object = Object;

                animationTime += f.Image.GetInt("delay");

                frames.Add(f);
            }
            frames = frames.OrderBy(x => x.ID).ToList<MapBackFrame>();
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
            Object.SetInt("y", Object.GetInt("y") + y);
        }

        public override bool IsPointInArea(int x, int y)
        {
            int topLeftX = Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x);
            int topLeftY = Map.Instance.CenterY + Object.GetInt("y") - Image.GetVector("origin").y;
            int width = Image.GetCanvas().width;
            int height = Image.GetCanvas().height;
            if (x >= topLeftX && x < topLeftX + width && y >= topLeftY && y < topLeftY + height)
            {
                Bitmap b = (Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap();
                return b.GetPixel(x - topLeftX, y - topLeftY).A > 0;
            }
            return false;
        }

        public void Cache()
        {
            _size = new Rectangle(0, 0, Image.GetCanvas().width, Image.GetCanvas().height);
            _f = Object.GetBool("f");

            _rX = Object.GetInt("rx") / 100.0;
            _rY = Object.GetInt("ry") / 100.0;

            switch (Object.GetInt("type"))
            {
                case 1:
                case 4:
                    {
                        _x = Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x);
                        _mX = (Object.GetInt("cx") == 0) ? Image.GetCanvas().width : Object.GetInt("cx");
                        _y = Map.Instance.CenterY + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y;
                        _mY = 0;                        
                        break;
                    }
                case 2:
                    {
                        _x = Map.Instance.CenterX + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x);
                        _mX = 0;
                        _y = Map.Instance.CenterY + Object.GetInt("y") - Image.GetVector("origin").y;
                        _mY = ((Object.GetInt("cy") == 0) ? Image.GetCanvas().height : Object.GetInt("cy"));
                        break;
                    }
                case 3:
                    {
                        _mX = (Object.GetInt("cx") != 0) ? Object.GetInt("cx") : Image.GetCanvas().width;
                        _mY = (Object.GetInt("cy") != 0) ? Object.GetInt("cy") : Image.GetCanvas().height;
                        _x = Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x);
                        _y = Map.Instance.CenterY + Object.GetInt("y") - Image.GetVector("origin").y;
                        //if (Object.GetInt("cx") == 0 && Object.GetInt("cy") == 0)
                        //    d.DrawBitmap((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedTexture(d._device) : Image.GetCanvas().GetTexture(d._device), 0, 0, Map.Instance.Width, Map.Instance.Height, Selected, Transparency);
                        break;
                    }
                case 0:
                    {
                        _x = Map.Instance.CenterX + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x);
                        _y = Map.Instance.CenterY + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y;
                        _mX = 0;
                        _mY = 0;
                        break;
                    }
                
            }

            cached = true;
        }

        public override void Draw(Graphics g)
        {

            switch (Object.GetInt("type"))
            {
                case 1:
                    {
                        int width = Image.GetCanvas().GetBitmap().Width;
                        int times = 5000 / width;
                        for (int i = -times * 2; i <= times * 2; i++)
                        {
                            g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), 400 + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x) + ((Object.GetInt("cx") == 0) ? width : Object.GetInt("cx")) * i, 400 + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y);
                        }
                        break;
                    }
                case 4:
                    {
                        int width = Image.GetCanvas().GetBitmap().Width;
                        int times = 5000 / width;
                        for (int i = -times * 2; i <= times * 2; i++)
                        {
                            g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), 400 + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x) + width * i, 400 + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y);
                        }
                        break;
                    }
                case 2:
                    {
                        int height = Image.GetCanvas().GetBitmap().Height;
                        int times = 5000 / height;
                        for (int i = -times * 2; i <= times * 2; i++)
                        {
                            g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), 400 + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x), 400 + Object.GetInt("y") - Image.GetVector("origin").y + ((Object.GetInt("cy") == 0) ? height : Object.GetInt("cy")) * i);
                        }
                        break;
                    }
                case 3:
                    {
                        int width = Image.GetCanvas().GetBitmap().Width;
                        int timesx = 5000 / width;
                        int height = Image.GetCanvas().GetBitmap().Height;
                        int timesy = 5000 / height;
                        int changex = (Object.GetInt("cx") != 0) ? Object.GetInt("cx") : width;
                        int changey = (Object.GetInt("cy") != 0) ? Object.GetInt("cy") : height;
                        for (int i = -timesx * 2; i <= timesx * 2; i++)
                        {
                            for (int j = -timesy * 2; j <= timesy * 2; j++)
                            {
                                g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), 400 + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x) + changex * i, 400 + Object.GetInt("y") - Image.GetVector("origin").y + changey * j);
                            }
                        }
                        break;
                    }
                case 0: g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), 400 + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x), 400 + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y); break;

            }
        }


        public void DrawStatic(Graphics g)
        {
            if (!((Object.GetInt("rx") == 0 && Object.GetInt("ry") == 0) || (Object.GetInt("rx") == -100 && Object.GetInt("ry") == -100))) return;

            switch (Object.GetInt("type"))
            {
                case 1:
                    {
                        int width = Image.GetCanvas().GetBitmap().Width;
                        int times = 5000 / width;
                        for (int i = -times * 2; i <= times * 2; i++)
                        {
                            g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x) + ((Object.GetInt("cx") == 0) ? width : Object.GetInt("cx")) * i, Map.Instance.CenterY + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y);
                        }
                        break;
                    }
                case 4:
                    {
                        int width = Image.GetCanvas().GetBitmap().Width;
                        int times = 5000 / width;
                        for (int i = -times * 2; i <= times * 2; i++)
                        {
                            g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), Map.Instance.CenterX + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x) + width * i, Map.Instance.CenterY + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y);
                        }
                        break;
                    }
                case 2:
                    {
                        int height = Image.GetCanvas().GetBitmap().Height;
                        int times = 5000 / height;
                        for (int i = -times * 2; i <= times * 2; i++)
                        {
                            g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), Map.Instance.CenterX + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x), Map.Instance.CenterY + Object.GetInt("y") - Image.GetVector("origin").y + ((Object.GetInt("cy") == 0) ? height : Object.GetInt("cy")) * i);
                        }
                        break;
                    }
                case 3:
                    {
                        int width = Image.GetCanvas().GetBitmap().Width;
                        int timesx = 5000 / width;
                        int height = Image.GetCanvas().GetBitmap().Height;
                        int timesy = 5000 / height;
                        int changex = (Object.GetInt("cx") != 0) ? Object.GetInt("cx") : width;
                        int changey = (Object.GetInt("cy") != 0) ? Object.GetInt("cy") : height;
                        for (int i = -timesx * 2; i <= timesx * 2; i++)
                        {
                            for (int j = -timesy * 2; j <= timesy * 2; j++)
                            {
                                g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), Map.Instance.CenterX + Object.GetInt("x") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x) + changex * i, Map.Instance.CenterY + Object.GetInt("y") - Image.GetVector("origin").y + changey * j);
                            }
                        }
                        break;
                    }
                case 0: g.DrawImage((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedBitmap() : Image.GetCanvas().GetBitmap(), Map.Instance.CenterX + Object.GetInt("x") + Object.GetInt("cx") - ((Object.GetBool("f")) ? Image.GetCanvas().width - Image.GetVector("origin").x : Image.GetVector("origin").x), Map.Instance.CenterY + Object.GetInt("y") + Object.GetInt("cy") - Image.GetVector("origin").y); break;

            }
        }

        public override void Draw(DevicePanel d)
        {
            if (!cached) Cache();

            int X = MapEditor.Instance.ShiftX - Map.Instance.CenterX + 400;
            int Y = MapEditor.Instance.ShiftY - Map.Instance.CenterY + 300;

            X += (int)(X * _rX) + _x; 
            Y += (int)(Y * _rY) + _y;


            int xStart = 0, xEnd = 1, yStart = 0, yEnd = 1;
            if (_mX != 0)
            {
                xStart = (d.GetScreen().X - X) / _mX - 1;
                xEnd = (d.GetScreen().Right - X) / _mX + 1;
            }
            if (_mY != 0)
            {
                yStart = (d.GetScreen().Y - Y) / _mY - 1;
                yEnd = (d.GetScreen().Bottom - Y) / _mY + 1;
            }

            //if(_mX != 0 && _mY != 0 && Object.GetInt("cx") == 0 && Object.GetInt("cy") == 0)
            //    d.DrawBitmap((Object.GetBool("f")) ? Image.GetCanvas().GetFlippedTexture(d._device) : Image.GetCanvas().GetTexture(d._device), 0, 0, Map.Instance.Width, Map.Instance.Height, Selected, Transparency);
            //else
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    d.DrawBitmap(_f ? Image.GetCanvas().GetFlippedTexture(d._device) : Image.GetCanvas().GetTexture(d._device), X + _mX * x, Y + _mY * y, _size, Selected, Transparency); 
                }
            }
            
        }
    }
}
