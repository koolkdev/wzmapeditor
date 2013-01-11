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
using WZMapEditor.TilesDesign;
using WZ;

namespace WZMapEditor
{
    class MapTile : MapLayerObject
    {
        public MapTileDesign Design;

        public List<MapFootholdDesign> Footholds = new List<MapFootholdDesign>();

        public void CreateFootholdDesignList()
        {
            IMGEntry entry = Image.GetChild("foothold");
            if (entry != null)
            {
                foreach (IMGEntry e in entry.childs.Values)
                {
                    MapFootholdDesign fh = new MapFootholdDesign();
                    fh.Object = Object;
                    fh.Image = e;
                    Footholds.Add(fh);
                }
            }
        }
        public int MagnetX, MagnetY;
        public bool Magnet;

        public void SetDesign(string type)
        {
            switch (type)
            {
                case "bsc": Design = new bsc(); break;
                case "enH0": Design = new enH0(); break;
                case "enH1": Design = new enH1(); break;
                case "enV0": Design = new enV0(); break;
                case "enV1": Design = new enV1(); break;
                case "edU": Design = new edU(); break;
                case "edD": Design = new edD(); break;
                case "slLU": Design = new slLU(); break;
                case "slRU": Design = new slRU(); break;
                case "slLD": Design = new slLD(); break;
                case "slRD": Design = new slRD(); break;
            }
            Design.Object = Object;
        }
        public override void Draw(DevicePanel d)
        {
            if (!Magnet)
            {
                base.Draw(d);
            }
            else
            {
                d.DrawBitmap(Image.GetCanvas().GetTexture(d._device), Map.Instance.CenterX + MagnetX - Image.GetVector("origin").x, Map.Instance.CenterY + MagnetY - Image.GetVector("origin").y, _size, Selected, Transparency);
            }
        }

        public override object Clone()
        {
            MapTile obj = new MapTile();


            obj.Image = Image;
            obj.Object = Object.Clone() as IMGEntry;
            obj.SetDesign(Object.GetString("u"));
            obj.CreateFootholdDesignList();

            return obj;
        }

        public override string ToString()
        {
            return "Type: Tile  Image: " + Image.parent.parent.Name + "/" + Image.parent.Name + "/" + Image.Name;
        }
    }
}
