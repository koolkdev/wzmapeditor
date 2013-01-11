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
using System.Collections;
using WZ;

namespace WZMapEditor
{
    class MapFootholds : MapItem
    {
        public Hashtable footholds = new Hashtable();

        public bool ToFix;
        public MapFootholds(int ID)
            : base()
        {
            this.ID = ID;
            Map.Instance.footholdsGroups.Add(ID);
        }

        public override bool IsPointInArea(int x, int y)
        {
            foreach (MapFoothold f in footholds.Values)
            {
                if (f.IsPointInArea(x, y))
                {
                    return true;
                }
            }
            return false;
        }


        public MapFoothold GetFootholdAt(int x, int y)
        {
            foreach (MapFoothold f in footholds.Values)
            {
                if (f.IsPointInArea(x, y))
                {
                    return f;
                }
            }
            return null;
        }
        public MapFoothold GetFootholdAt(int id)
        {
            if (footholds.Contains(id)) return footholds[id] as MapFoothold;
            return null;
        }

        public List<MapItem> GetConnectionsAt(Rectangle rectangle)
        {
            List<MapItem> l = new List<MapItem>();
            foreach (MapFoothold f in footholds.Values)
            {
                if (f.s1.IsPointInArea(rectangle))
                {
                    l.Add(f.s1);
                }
                if (f.s2.IsPointInArea(rectangle))
                {
                    l.Add(f.s2);
                }
            }
            return l;
        }
        public List<MapFootholdSide> GetConnectionAt(int x, int y)
        {
            List<MapFootholdSide> l = new List<MapFootholdSide>();
            foreach (MapFoothold f in footholds.Values)
            {
                MapFootholdSide side = f.GetSideAt(x, y);
                if(side != null)
                {
                    l.Add(side);

                    int other = side.GetConnected();
                    if (footholds.Contains(other))
                    {
                        l.Add(((MapFoothold)footholds[other]).GetSideAt(x,y));
                    }
                    return l;
                }
            }
            return l;
        }

        public override void Move(int x, int y)
        {
            throw new NotImplementedException();
        }
        public override void Draw(Graphics g) { }
        
        public override void Draw(DevicePanel d)
        {
            foreach (MapFoothold f in footholds.Values)
            {
                if (MapEditor.Instance.LinkToFH.Checked)
                {
                    f.Selected = Selected;
                    f.s1.Selected = Selected;
                    f.s2.Selected = Selected;
                }
                else if (f.Selected)
                {
                    f.Selected = false;
                    f.s1.Selected = false;
                    f.s2.Selected = false;
                }
                f.Transparency = Transparency;
                f.s1.Transparency = Transparency;
                f.s2.Transparency = Transparency;
                f.Draw(d);
            }
        }

        public static void SwapInt(IMGEntry e1, IMGEntry e2)
        {
            int temp = e1.GetInt();
            e1.SetInt(e2.GetInt());
            e2.SetInt(temp);

        }

        public void Fix()
        {
            if(ToFix)
            {
                int sum = 0;
                foreach(MapFoothold fh in footholds.Values)
                {
                    sum += Math.Sign(fh.Object.GetInt("x2") - fh.Object.GetInt("x1"));
                }
                if (sum < 0)
                {
                    // Fix
                    foreach (MapFoothold fh in footholds.Values)
                    {
                        SwapInt(fh.Object.GetChild("x1"), fh.Object.GetChild("x2"));
                        SwapInt(fh.Object.GetChild("y1"), fh.Object.GetChild("y2"));
                        SwapInt(fh.Object.GetChild("prev"), fh.Object.GetChild("next"));
                        int temp = fh.s1.ID;
                        fh.s1.ID = fh.s2.ID;
                        fh.s2.ID = temp;
                    }
                }
            }
        }
    }
}
