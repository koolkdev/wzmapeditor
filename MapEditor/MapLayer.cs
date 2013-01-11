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
using System.Collections;
using WZ.Objects;

namespace WZMapEditor
{
    class MapLayer : IDrawable
    {
        public IMGEntry info;
        public IMGEntry layer;
        public List<MapObject> objects = new List<MapObject>();
        public List<MapTile> tiles = new List<MapTile>();
        public Hashtable footholdGroups = new Hashtable();
        public int ID;

        public MapLayer() { }

        public void Delete(MapItem item)
        {
            if (item is MapObject)
            {
                MapObject o = (MapObject)item;
                if (objects.Contains(o))
                {
                    objects.Remove(o);
                    layer.GetChild("obj").childs.Remove(o.Object.Name);
                    Map.Pad(layer.GetChild("obj"), objects.ToArray());
                }
            }
            else if (item is MapTile)
            {
                MapTile t = (MapTile)item;
                if (tiles.Contains(t))
                {
                    tiles.Remove(t);
                    layer.GetChild("tile").childs.Remove(t.Object.Name);
                    Map.Pad(layer.GetChild("tile"), tiles.ToArray());
                }
            }
            else
            {
                Map.Instance.Delete(item);
            }
        }

        public void Add(MapItem item)
        {
            if (item is MapObject)
            {
                MapObject o = (MapObject)item;
                int id = 0;
                while(layer.GetChild("obj").childs.Contains(id.ToString())) id++;
                o.ID = id;
                o.Object.Name = o.ID.ToString();
                objects.Add(o);
                objects.Sort(Map.CompareItems);
                layer.GetChild("obj").Add(o.Object);
                OrderObjects();
            }
            else if (item is MapTile)
            {
                MapTile t = (MapTile)item;

                string style = info.GetString("tS");

                IMGFile tilesF = MapEditor.file.Directory.GetIMG("Tile/" + style + ".img");

                if (tilesF.GetChild(t.Object.GetString("u") + "/" + t.Object.GetInt("no").ToString()) == null)
                {
                    t.Object.SetInt("no", MapEditor.Instance.random.Next(tilesF.GetChild(t.Object.GetString("u")).childs.Count));
                }
                IMGEntry image = MapEditor.file.Directory.GetIMG("Tile/" + style + ".img").GetChild(t.Object.GetString("u") + "/" + t.Object.GetInt("no").ToString());
                if (t.Image != image) t.Image = image;

                int id = 0;
                while (layer.GetChild("tile").childs.Contains(id.ToString())) id++;
                t.ID = id;
                t.Object.Name = t.ID.ToString();
                tiles.Add(t);
                tiles.Sort(Map.CompareItems);
                layer.GetChild("tile").Add(t.Object);
                OrderTiles();
            }
            else
            {
                Map.Instance.Add(item);
            }
        }

        public List<MapItem> GetItemsAt(Rectangle area)
        {
            List<MapItem> items = new List<MapItem>();
            if (MapEditor.Instance.EditObj.Checked)
            {
                foreach (MapObject o in objects)
                {
                    if (o.IsObjectInArea(area))
                    {
                        items.Add(o);
                    }
                }
            }

            if (MapEditor.Instance.EditTile.Checked)
            {
                foreach (MapTile t in tiles)
                {
                    if (t.IsObjectInArea(area))
                    {
                        items.Add(t);
                    }
                }
            }
            if (MapEditor.Instance.EditLife.Checked)
            {
                foreach (MapLife l in Map.Instance.lifes)
                {
                    if (l.Transparency == 0xff)
                    {
                        if (l.IsObjectInArea(area))
                        {
                            items.Add(l);
                        }
                    }
                }
            }
            if (MapEditor.Instance.EditReactor.Checked)
            {
                foreach (MapReactor r in Map.Instance.reactors)
                {
                    if (r.Transparency == 0xff)
                    {
                        if (r.IsObjectInArea(area))
                        {
                            items.Add(r);
                        }
                    }
                }
            }
            if (MapEditor.Instance.EditBack.Checked)
            {
                foreach (MapBack b in Map.Instance.backs)
                {
                    if (b.Transparency == 0xff)
                    {
                        if (b.IsObjectInArea(area))
                        {
                            items.Add(b);
                        }
                    }
                }
            }
            return items;
        }

        public MapItem GetItemAt(int x, int y)
        {
            MapItem item = null;
            if (MapEditor.Instance.EditObj.Checked)
            {
                for (int i = 0; i <= 20; i++)
                {
                    foreach (MapObject o in objects)
                    {
                        if (o.Object.GetInt("z") == i)
                        {
                            if (o.IsPointInArea(x, y))
                            {
                                item = o;
                            }
                        }
                    }
                }
            }

            if (MapEditor.Instance.EditTile.Checked)
            {
                for (int i = -5; i <= 0; i++)
                {
                    foreach (MapTile t in tiles)
                    {
                        if (t.Image.GetInt("z") == i)
                        {
                            if (t.IsPointInArea(x, y))
                            {
                                item = t;
                            }
                        }
                    }
                }
            }
            if (MapEditor.Instance.EditLife.Checked)
            {
                foreach (MapLife l in Map.Instance.lifes)
                {
                    if (l.Transparency == 0xff)
                    {
                        if (l.IsPointInArea(x, y))
                        {
                            item = l;
                        }
                    }
                }
            }
            if (MapEditor.Instance.EditReactor.Checked)
            {
                foreach (MapReactor r in Map.Instance.reactors)
                {
                    if (r.Transparency == 0xff)
                    {
                        if (r.IsPointInArea(x, y))
                        {
                            item = r;
                        }
                    }
                }
            }
            if (MapEditor.Instance.EditBack.Checked)
            {
                foreach (MapBack b in Map.Instance.backs)
                {
                    if (b.Transparency == 0xff)
                    {
                        if (b.IsPointInArea(x, y))
                        {
                            item = b;
                        }
                    }
                }
            }
            return item;
        }

        public Point GetTileDesignPosition(MapTile mapTile)
        {
            Point empty = new Point(0xffff, 0xffff);
            foreach (MapTile t in tiles)
            {
                int multi = t.Image.parent.parent.GetInt("info/mag");
                if (multi == 0) multi = 1;
                Point p = t.Design.GetMath(mapTile.Design.type, mapTile.Object.GetInt("x"), mapTile.Object.GetInt("y"), multi);
                if (p != empty) return p;
            }
            return empty;
        }

        public MapFootholdDesign GetFootholdDesignAt(int x, int y)
        {
            foreach (MapTile t in tiles)
            {
                foreach (MapFootholdDesign fh in t.Footholds)
                {
                    if (fh.IsPointInArea(x, y))
                    {
                        return fh;
                    }
                }
            }
            /*foreach (MapObject o in objects)
            {
                foreach (MapFootholdDesign fh in o.Footholds)
                {
                    if (fh.IsPointInArea(x, y))
                    {
                        return fh;
                    }
                }
            }*/
            return null;
        }

        public MapLRDesign GetLRDesignAt(int x, int y)
        {
            foreach (MapObject o in objects)
            {
                foreach (MapLRDesign lr in o.LRs)
                {
                    if (lr.IsPointInArea(x, y))
                    {
                        return lr;
                    }
                }
            }
            return null;
        }
        public MapFootholds GetFootholdGroupAt(int x, int y)
        {
            foreach (MapFootholds f in footholdGroups.Values)
            {
                if (f.IsPointInArea(x, y)) return f;
            }
            return null;
        }

        public MapFoothold GetFootholdAt(int x, int y)
        {
            foreach (MapFootholds f in footholdGroups.Values)
            {
                MapFoothold fh = f.GetFootholdAt(x, y);
                if (fh != null)
                {
                    return fh;
                }
            }
            return null;
        }

        public List<MapItem> GetConnectionsAt(Rectangle rectangle)
        {
            List<MapItem> items = new List<MapItem>();

            foreach (MapFootholds f in footholdGroups.Values)
            {
                items.AddRange(f.GetConnectionsAt(rectangle));
            }
            return items;
        }

        public List<MapFootholdSide> GetConnectionAt(int x, int y)
        {
            foreach (MapFootholds f in footholdGroups.Values)
            {
                List<MapFootholdSide> l = f.GetConnectionAt(x, y);
                if (l.Count > 0)
                {
                    return l;
                }
            }
            return new List<MapFootholdSide>();
        }

        public MapLRSide GetLRSideAt(int x, int y)
        {
            foreach (MapLR lr in Map.Instance.lrs)
            {
                if (lr.Object.GetInt("page") == ID)
                {
                    if (lr.GetSideAt(x, y) != null)
                    {
                        return lr.GetSideAt(x, y);
                    }
                }
            }
            return null;
        }

        internal MapLR GetLRAt(int x, int y)
        {
            foreach (MapLR lr in Map.Instance.lrs)
            {
                if (lr.Object.GetInt("page") == ID)
                {
                    if (lr.IsPointInArea(x, y))
                    {
                        return lr;
                    }
                }
            }
            return null;
        }

        public void Draw(Graphics g)
        {
            for (int i = 0; i <= 20; i++)
            {
                foreach (MapObject o in objects)
                {
                    if (o.Object.GetInt("z") == i)
                        o.Draw(g);
                }
            }
            foreach (MapTile t in tiles)
            {
                t.Draw(g);
            }
        }

        public bool IsContainFoothold(int id)
        {
            foreach (MapFootholds f in footholdGroups.Values)
            {
                if (f.footholds.Contains(id))
                {
                    return true;
                }
            }
            return false;
        }

        public void Draw(DevicePanel d)
        {
            int transparency = (MapEditor.Instance.EditMode.Checked && (!MapEditor.Instance.EditObj.Checked || MapEditor.Instance.Layer.Value != ID)) ? 0x32 : 0xFF;
            if (MapEditor.Instance.ShowObj.Checked || MapEditor.Instance.EditObj.Checked)
            {
                //for (int i = 0; i <= 20; i++)
                {
                    foreach (MapObject o in objects)
                    {
                        //if (o.Object.GetInt("z") == i)
                       {
                            o.Transparency = transparency;
                            o.Draw(d);
                        }
                    }
                }
            }

            transparency = (MapEditor.Instance.EditMode.Checked && (!MapEditor.Instance.EditTile.Checked || MapEditor.Instance.Layer.Value != ID)) ? 0x32 : 0xFF;
            if (MapEditor.Instance.ShowTile.Checked || MapEditor.Instance.EditTile.Checked)
            {
               // for (int i = -5; i <= 0; i++)
                {
                    foreach (MapTile t in tiles)
                    {
                        //if (!t.Selected && t.Image.GetInt("z") == i)
                        {
                            t.Transparency = transparency;
                            t.Draw(d);
                        }
                    }
                }
                /*for (int i = -5; i <= 0; i++)
                {
                    foreach (MapTile t in tiles)
                    {
                        if (t.Selected && t.Image.GetInt("z") == i)
                        {
                            t.Transparency = transparency;
                            t.Draw(d);
                        }
                    }
                }*/
            }

            transparency = (MapEditor.Instance.EditMode.Checked && (!(MapEditor.Instance.EditFH.Checked || MapEditor.Instance.LinkToFH.Checked) || MapEditor.Instance.Layer.Value != ID)) ? 0x32 : 0xFF;
            if (MapEditor.Instance.ShowFH.Checked || MapEditor.Instance.EditFH.Checked)
            {
                foreach (MapFootholds f in footholdGroups.Values)
                {
                    f.Transparency = transparency;
                    f.Draw(d);
                }
            }

           transparency = (MapEditor.Instance.EditMode.Checked && (!MapEditor.Instance.EditLR.Checked || MapEditor.Instance.Layer.Value != ID)) ? 0x32 : 0xFF;
           if (MapEditor.Instance.ShowLR.Checked || MapEditor.Instance.EditLR.Checked)
            {
                foreach (MapLR lr in Map.Instance.lrs)
                {
                    if (lr.Object.GetInt("page") == ID)
                    {
                        lr.Transparency = transparency;
                        lr.Draw(d);
                    }
                }
            }

            transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditLife.Checked) ? 0x32 : 0xFF;
            if (MapEditor.Instance.ShowLife.Checked || MapEditor.Instance.EditLife.Checked)
            {
                foreach (MapLife l in Map.Instance.lifes)
                {
                    if (Map.Instance.footholds.Contains(l.Object.GetInt("fh")))
                    {
                        if (IsContainFoothold(l.Object.GetInt("fh")))
                        {
                            l.Transparency = transparency;
                            l.Draw(d);
                        }
                    }
                }
            }
            if (MapEditor.Instance.Layer.Value == ID && MapEditor.Instance.EditFH.Checked && MapEditor.Instance.FootholdsCreator.Checked)
            {
                List<MapFootholdDesign> l = MapEditor.Instance.FHCreating;
                for(int i=0; i<l.Count; i++)
                {
                    if(i != l.Count - 1)
                    {
                        d.DrawLine(l[i].GetX(), l[i].GetY(), l[i + 1].GetX(), l[i + 1].GetY(), Color.Red);
                    }
                    else
                    {
                        d.DrawLine(l[i].GetX(), l[i].GetY(), MapEditor.Instance.lastX, MapEditor.Instance.lastY, Color.Red);
                    }
                }
                foreach (MapTile t in tiles)
                {
                    foreach (MapFootholdDesign fh in t.Footholds)
                    {
                        fh.Draw(d);
                    }
                }
                /*foreach (MapObject o in objects)
                {
                    foreach (MapFootholdDesign fh in o.Footholds)
                    {
                        fh.Draw(d);
                    }
                }*/
            }
            if (MapEditor.Instance.Layer.Value == ID && MapEditor.Instance.EditLR.Checked && MapEditor.Instance.FootholdsCreator.Checked)
            {
                List<MapLRDesign> l = MapEditor.Instance.LRCreating;
                for (int i = 0; i < l.Count; i++)
                {
                    if (i != l.Count - 1)
                    {
                        d.DrawLine(l[i].GetX(), l[i].GetY(), l[i + 1].GetX(), l[i + 1].GetY(), Color.Green);
                    }
                    else
                    {
                        d.DrawLine(l[i].GetX(), l[i].GetY(), MapEditor.Instance.lastX, MapEditor.Instance.lastY, Color.Red);
                    }
                }
                foreach (MapObject o in objects)
                {
                    foreach (MapLRDesign lr in o.LRs)
                    {
                        lr.Draw(d);
                    }
                }
            }
        }

        public void DrawAnimation(DevicePanel d)
        {
            //for (int i = 0; i <= 20; i++)
            {
                foreach (MapObject o in objects)
                {
                    //if (o.Object.GetInt("z") == i)
                    {
                        o.DrawAnimation(d);
                    }
                }
            }
            //for (int i = -5; i <= 0; i++)
            {
                foreach (MapTile t in tiles)
                {
                    //if (!t.Selected && t.Image.GetInt("z") == i)
                    {
                        t.DrawAnimation(d);
                    }
                }
            }/*
            for (int i = -5; i <= 0; i++)
            {
                foreach (MapTile t in tiles)
                {
                    if (t.Selected && t.Image.GetInt("z") == i)
                    {
                        t.DrawAnimation(d);
                    }
                }
            }*/
        }
        
        public void ChangeTileStyle(string style)
        {
            info.SetString("tS", style);

            IMGFile tilesF = MapEditor.file.Directory.GetIMG("Tile/" + style + ".img");

            foreach (MapTile t in tiles)
            {
                if (tilesF.GetChild(t.Object.GetString("u") + "/" + t.Object.GetInt("no").ToString()) == null)
                {
                    t.Object.SetInt("no", MapEditor.Instance.random.Next(tilesF.GetChild(t.Object.GetString("u")).childs.Count));
                }
                t.Image = MapEditor.file.Directory.GetIMG("Tile/" + style + ".img").GetChild(t.Object.GetString("u") + "/" + t.Object.GetInt("no").ToString());
            }
        }

        public void OrderTiles()
        {
            tiles = tiles.OrderBy(t => t.Image.GetInt("z")).ToList<MapTile>();
            tiles = tiles.OrderBy(t => t.Selected).ToList<MapTile>();
        }

        public void OrderObjects()
        {
            objects = objects.OrderBy(o => o.Object.GetInt("z")).ToList<MapObject>();
        }
    }
}
