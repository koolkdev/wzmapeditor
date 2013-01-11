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
using WZ;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WZMapEditor
{
    public class MapBackground
    {
        List<MapBack> backs = new List<MapBack>();

        static public IMGEntry Object;

        public Bitmap Bitmap;

        public void Load()
        {
            IMGEntry back = Object.GetChild("back");
            if (back == null) return;

            foreach (IMGEntry b in back.childs.Values)
            {
                if (b.GetInt("ani") != 1 && b.GetString("bS") != "")
                {
                    MapBack mb = new MapBack();

                    mb.Object = b;
                    mb.Image = MapEditor.file.Directory.GetIMG("Back/" + b.GetString("bS") + ".img").GetChild("back/" + b.GetInt("no").ToString());
                    mb.ID = int.Parse(b.Name);

                    backs.Add(mb);
                }
            }

            backs = backs.OrderBy(o => o.ID).ToList<MapBack>();

            Bitmap = new Bitmap(800, 600);
            using (Graphics g = Graphics.FromImage(Bitmap))
            {
                foreach (MapBack b in backs)
                {
                    b.Draw(g);
                }
            }
        }

        public Bitmap GetThumb()
        {
            Bitmap thumb = new Bitmap(160, 120);

            Graphics g = Graphics.FromImage(thumb);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if(Bitmap != null)
                g.DrawImage(Bitmap, 0, 0, 160, 120);
            g.Dispose();

            return thumb;
        }
    }
}
