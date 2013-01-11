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
    class MapObject : MapLayerObject
    {
        public List<List<MapFootholdDesign>> Footholds = new List<List<MapFootholdDesign>>();
        public List<MapLRDesign> LRs = new List<MapLRDesign>();
        public List<MapSeatDesign> Seats = new List<MapSeatDesign>();

        List<MapObjectFrame> frames = null;
        int animationTime=0;

        public override void DrawAnimation(DevicePanel d)
        {
            if (animationTime == 0)
            {
                Draw(d);
                return;
            }
            int time = System.Environment.TickCount % animationTime;
            
            foreach(MapObjectFrame frame in frames)
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
            if(Image.GetChild("delay") == null) return;
            frames = new List<MapObjectFrame>();
            IMGEntry originalImage = Image.parent;
            foreach (IMGEntry frame in originalImage.childs.Values)
            {
                MapObjectFrame f = new MapObjectFrame();
                try
                {
                    f.ID = int.Parse(frame.Name);
                }
                catch
                {
                    continue; // to handle:"blend"
                }
                f.Image = Map.GetRealImage(frame);
                f.Object = Object;

                animationTime += f.Image.GetInt("delay");

                frames.Add(f);
            }
            frames = frames.OrderBy(x => x.ID).ToList<MapObjectFrame>();
        }
        
        public void CreateFootholdDesignList()
        {
            GenerateFrames();

            IMGEntry entry = Image.GetChild("foothold");
            if (entry != null)
            {
                List<MapFootholdDesign> list = new List<MapFootholdDesign>();
                for (int i = 0; i < entry.childs.Count; i++)
                {
                    if (entry.childs[i].value == null)
                    {
                        List<MapFootholdDesign> l = new List<MapFootholdDesign>();
                        for (int j = 0; j < entry.childs[i].childs.Count; j++)
                        {
                            MapFootholdDesign fh = new MapFootholdDesign();
                            fh.Object = Object;
                            fh.Image = entry.childs[i].childs[j];
                            l.Add(fh);
                        }
                        Footholds.Add(l);
                    }
                    else
                    {
                        MapFootholdDesign fh = new MapFootholdDesign();
                        fh.Object = Object;
                        fh.Image = entry.childs[i];
                        list.Add(fh);
                    }
                }
                if (list.Count > 0)
                {
                    Footholds.Add(list);
                }
            }
            entry = Image.GetChild("ladder");
            if (entry != null)
            {
                foreach (IMGEntry e in entry.childs.Values)
                {
                    MapLRDesign lr = new MapLRDesign(true);
                    lr.Object = Object;
                    lr.Image = e;
                    LRs.Add(lr);
                }
            }
            entry = Image.GetChild("rope");
            if (entry != null)
            {
                foreach (IMGEntry e in entry.childs.Values)
                {
                    MapLRDesign lr = new MapLRDesign(false);
                    lr.Object = Object;
                    lr.Image = e;
                    LRs.Add(lr);
                }
            }
            entry = Image.GetChild("..").GetChild("seat");
            if (entry != null)
            {
                foreach (IMGEntry e in entry.childs.Values)
                {
                    MapSeatDesign s = new MapSeatDesign();
                    s.Object = Object;
                    s.Image = e;
                    Seats.Add(s);
                }
            }
        }

        public override object Clone()
        {
            MapObject obj = new MapObject();

            obj.Image = Image;
            obj.Object = Object.Clone() as IMGEntry;
            obj.CreateFootholdDesignList();

            return obj;
        }
        /*public int MagnetY;
        public bool Magnet;

        public override void Draw(DevicePanel d)
        {
            if (!Magnet)
            {
                base.Draw(d);
            }
            else
            {
                d.DrawBitmap(Image.GetCanvas().GetTexture(d._device), Map.Instance.CenterX + Object.GetInt("x") - Image.GetVector("origin").x, Map.Instance.CenterY + MagnetY - Image.GetVector("origin").y, Image.GetCanvas().width, Image.GetCanvas().height, Selected, Transparency);
            }
        }*/

        public override string ToString()
        {
            return "Type: Object  Z: " + Object.GetInt("z") + "  Image: " + Image.parent.parent.parent.parent.Name + "/" + Image.parent.parent.parent.Name + "/" + Image.parent.parent.Name + "/" + Image.parent.Name + "/" + Image.Name;
        }
    }
}
