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
    class MapFoothold : MapItem
    {
        public MapFootholdSide s1, s2;

        public MapFootholds Group;

        public MapFoothold(int ID)
            : base()
        {
            this.ID = ID;
            Map.Instance.footholds.Add(ID);
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private static double Prod(int x1, int y1, int x2, int y2)
        {
            return (x1 * x2) + (y1 * y2);
        }

        public static double DistanceBetweenPointToLine(int x, int y, int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int vx = x - x1;
            int vy = y - y1;

            double t = Prod(dx, dy, vx, vy);
            double d = Prod(dx, dy, dx, dy);

            if (t <= 0)
            {
                return Distance(x, y, x1, y1);
            }
            else if (t >= d)
            {
                return Distance(x, y, x2, y2);
            }
            else
            {
                return Distance(x, y, x1 + ((t / d) * dx), y1 + ((t / d) * dy));
            }
        }

        public override bool IsPointInArea(int x, int y)
        {
            int cX = Map.Instance.CenterX;
            int cY = Map.Instance.CenterY;
            double distance = DistanceBetweenPointToLine(x, y, cX + Object.GetInt("x1"), cY + Object.GetInt("y1"), cX + Object.GetInt("x2"), cY + Object.GetInt("y2"));
            return distance <= 5;
        }

        public MapFootholdSide GetSideAt(int x, int y)
        {
            if (s1.IsPointInArea(x, y)) return s1;
            if (s2.IsPointInArea(x, y)) return s2;
            return null;
        }

        public override void Move(int x, int y)
        {
            throw new NotImplementedException();
        }
        public override void Draw(Graphics g) { }

        public override void Draw(DevicePanel d)
        {
            int cX = Map.Instance.CenterX;
            int cY = Map.Instance.CenterY;
            int x1 = cX + Object.GetInt("x1");
            int x2 = cX + Object.GetInt("x2");
            int y1 = cY + Object.GetInt("y1");
            int y2 = cY + Object.GetInt("y2");
            d.DrawLine(x1, y1, x2, y2, Color.FromArgb(Transparency, (Selected) ? Color.Blue : Color.Red));
            s1.Draw(d);
            s2.Draw(d);
        }

        public int m_x1 { get { return Object.GetInt("x1"); } }
        public int m_x2 { get { return Object.GetInt("x2"); } }
        public int m_y1 { get { return Object.GetInt("y1"); } }
        public int m_y2 { get { return Object.GetInt("y2"); } }
        public double m_len { get { return Math.Sqrt((m_x2 - m_x1) * (m_x2 - m_x1) + (m_y2 - m_y1) * (m_y2 - m_y1)); } }
        public double m_uvx { get { return (m_x2 - m_x1) / m_len; } }
        public double m_uvy { get { return (m_y2 - m_y1) / m_len; } }
        public int m_lPage { get { return int.Parse(Group.Object.parent.Name); } }
        public int m_lZMass { get { return int.Parse(Group.Object.Name); } }
        public double drag { get { return 1; } }
        public double force { get { return 0; } }
        public double walk { get { return 1; } }
        public MapFoothold m_pfhPrev { get { return Group.GetFootholdAt(Object.GetInt("prev")); } }
        public MapFoothold m_pfhNext { get { return Group.GetFootholdAt(Object.GetInt("next")); } }
    }
}
