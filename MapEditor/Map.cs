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
using WZ.Objects;
using System.Collections;

namespace WZMapEditor
{
    class Map : IDrawable
    {
        public List<int> footholdsGroups = new List<int>();
        public List<int> footholds = new List<int>();

        public IMGEntry map;

        public List<MapLayer> layers = new List<MapLayer>();
        public List<MapLife> lifes = new List<MapLife>();
        public List<MapReactor> reactors = new List<MapReactor>();
        public List<MapLR> lrs = new List<MapLR>();
        public List<MapSeat> seats = new List<MapSeat>();
        public List<MapToolTip> tooltips = new List<MapToolTip>();
        public List<MapPortal> portals = new List<MapPortal>();
        public List<MapBack> backs = new List<MapBack>();
        public MapClock clock;

        public int CenterX;
        public int CenterY;
        public int Width, Height;
        public int VRLeft, VRTop, VRRight, VRBottom;

        public static Map Instance;

        public double fly;

        private static bool IsPadded(IMGEntry entry, int startID)
        {
            for (int i = startID; i < entry.childs.Count + startID; i++)
            {
                if (!entry.childs.Contains(i.ToString()))
                {
                    return false;
                }
            }
            return true;
        }
        private static void UpdateMapItems(MapItem[] items)
        {
            foreach (MapItem item in items)
            {
                item.ID = int.Parse(item.Object.Name);
            }
        }
        public static void Pad(IMGEntry entry, MapItem[] items, int startID)
        {
            while (!IsPadded(entry, startID))
            {
                foreach (IMGEntry e in entry.childs.Values)
                {
                    int ID = int.Parse(e.Name);
                    if (ID > startID)
                    {
                        if (!entry.childs.Contains((ID - 1).ToString()))
                        {
                            e.Rename((ID - 1).ToString());
                            break;
                        }
                    }
                }
            }
            UpdateMapItems(items);
        }
        public static void Pad(IMGEntry entry, IMGEntry entry2, MapItem[] items)
        {
            while (!IsPadded(entry, 0))
            {
                foreach (IMGEntry e in entry.childs.Values)
                {
                    int ID = int.Parse(e.Name);
                    if (ID > 0)
                    {
                        if (!entry.childs.Contains((ID - 1).ToString()))
                        {
                            e.Rename((ID - 1).ToString());
                            entry.childs[ID.ToString()].Rename((ID - 1).ToString());
                            break;
                        }
                    }
                }
            }
            UpdateMapItems(items);
        }

        public static void Pad(IMGEntry entry, MapItem[] items)
        {
            Pad(entry, items, 0);
        }
        public static int CompareItems(MapItem o1, MapItem o2)
        {
            return o1.ID.CompareTo(o2.ID);
        }

        public static IMGEntry GetRealImage(IMGEntry entry)
        {
            if (entry.value.type == WZObject.WZObjectType.WZ_CANVAS)
            {
                return entry;
            }
            else if (entry.value.type == WZObject.WZObjectType.WZ_UOL)
            {
                return GetRealImage(entry.parent.GetChild(((WZUOL)entry.value).path));
            }
            return null;
        }
        public Map(IMGEntry map)
        {
            Instance = this;

            MapEditor.Instance.EditBack.Enabled = false;

            this.map = map;

            IMGEntry Info = map.GetChild("info");
            IMGEntry MiniMap = map.GetChild("miniMap");
            if (Info.GetChild("VRTop") != null)
            {
                VRLeft = Info.GetInt("VRLeft");
                VRTop = Info.GetInt("VRTop");
                VRRight = Info.GetInt("VRRight");
                VRBottom = Info.GetInt("VRBottom");
            }
            if (MiniMap == null)
            {
                if (VRTop == 0) throw new Exception("Unhandled Map");
                CenterX = -VRLeft + 50;
                CenterY = -VRTop + 50;
                Width = VRRight + CenterX + 100;
                Height = VRBottom + CenterY + 100;
            }
            else
            {
                CenterX = MiniMap.GetInt("centerX");
                CenterY = MiniMap.GetInt("centerY");
                Width = MiniMap.GetInt("width");
                Height = MiniMap.GetInt("height");
                if (VRTop == 0)
                {
                    VRLeft = -CenterX + 69;
                    VRTop = -CenterY + 86;
                    VRRight = Width - CenterX - 69;
                    VRBottom = Height - CenterY - 86;
                }
            }

            fly = Info.GetBool("swim") ? -1 : 1;

            int maxX = Int32.MinValue;
            int maxY = Int32.MinValue;
            int minX = Int32.MaxValue;
            int minY = Int32.MaxValue;

            for (int i = 0; i < 8; i++)
            {
                MapLayer layer = new MapLayer();

                IMGEntry layerEntry = map.GetChild(i.ToString());

                layer.layer = layerEntry;
                layer.info = layerEntry.GetChild("info");
                layer.ID = i;

                IMGEntry objects = layerEntry.GetChild("obj");
                foreach (IMGEntry o in objects.childs.Values)
                {
                    MapObject mo = new MapObject();

                    mo.Object = o;
                    mo.Image = GetRealImage(MapEditor.file.Directory.GetIMG("Obj/" + o.GetString("oS") + ".img").GetChild(o.GetString("l0") + "/" + o.GetString("l1") + "/" + o.GetString("l2") + "/0"));
                    mo.ID = int.Parse(o.Name);

                    mo.CreateFootholdDesignList();

                    footholdsGroups.Add(o.GetInt("zM"));

                    layer.objects.Add(mo);
                }

                layer.objects.Sort(CompareItems);

                layer.OrderObjects();

                IMGEntry tiles = layerEntry.GetChild("tile");
                foreach (IMGEntry t in tiles.childs.Values)
                {
                    MapTile mt = new MapTile();

                    mt.Object = t;
                    mt.Image = MapEditor.file.Directory.GetIMG("Tile/" + layer.info.GetString("tS") + ".img").GetChild(t.GetString("u") + "/" + t.GetInt("no").ToString());
                    //mt.Image = MapEditor.file.Directory.GetIMG("Tile/blackTile.img").GetChild(t.GetString("u") + "/0");
                    mt.ID = int.Parse(t.Name);

                    mt.SetDesign(t.GetString("u"));

                    mt.CreateFootholdDesignList();

                    footholdsGroups.Add(t.GetInt("zM"));

                    layer.tiles.Add(mt);
                }

                layer.tiles.Sort(CompareItems);

                layer.OrderTiles();

                IMGEntry footholds = map.GetChild("foothold/" + i.ToString());
                if (footholds != null)
                {
                    foreach (IMGEntry group in footholds.childs.Values)
                    {
                        //if (group.Name != "3") continue;
                        MapFootholds f = new MapFootholds(int.Parse(group.Name));

                        f.Object = group;

                        layer.footholdGroups.Add(f.ID, f);

                        foreach (IMGEntry fhe in group.childs.Values)
                        {
                            MapFoothold fh = new MapFoothold(int.Parse(fhe.Name));

                            fh.Object = fhe;
                            fh.Group = f;
                            fh.s1 = new MapFootholdSide();
                            fh.s2 = new MapFootholdSide();
                            fh.s1.Object = fhe;
                            fh.s2.Object = fhe;
                            fh.s1.ID = 1;
                            fh.s2.ID = 2;
                            fh.s1.Foothold = fh;
                            fh.s2.Foothold = fh;

                            if (fhe.GetInt("x1") < minX) minX = fhe.GetInt("x1");
                            if (fhe.GetInt("y1") < minY) minY = fhe.GetInt("y1");
                            if (fhe.GetInt("x2") < minX) minX = fhe.GetInt("x2");
                            if (fhe.GetInt("y2") < minY) minY = fhe.GetInt("y2");
                            if (fhe.GetInt("x1") > maxX) maxX = fhe.GetInt("x1");
                            if (fhe.GetInt("y1") > maxY) maxY = fhe.GetInt("y1");
                            if (fhe.GetInt("x2") > maxX) maxX = fhe.GetInt("x2");
                            if (fhe.GetInt("y2") > maxY) maxY = fhe.GetInt("y2");

                            f.footholds.Add(fh.ID, fh);
                        }
                    }

                }


                layers.Add(layer);
            }
            /*if (VRTop == 0)
            {
                if (maxX != Int32.MinValue)
                {
                    VRLeft = minX + 30;
                    VRRight = maxX - 30;
                    VRTop = minY - 300;
                    VRBottom = maxY + 10;
                }
            }*/
            left = minX + 30;
            right = maxX - 30;
            top = minY - 300;
            bottom = maxY + 10;
            if (Info.GetChild("VRLeft") != null)
            {
                if (left < Info.GetInt("VRLeft") + 20) left = Info.GetInt("VRLeft") + 20;
                if (right > Info.GetInt("VRRight")) right = Info.GetInt("VRRight");
                if (top < Info.GetInt("VRTop") + 65) top = Info.GetInt("VRTop") + 65;
                if (bottom > Info.GetInt("VRBottom")) bottom = Info.GetInt("VRBottom");
            }
            left -= 10;
            right += 10;
            top -= 10;
            bottom += 10;

            IMGEntry back = map.GetChild("back");
            foreach (IMGEntry b in back.childs.Values)
            {
                if (b.GetString("bS") != "")
                {
                    MapBack mb = new MapBack();

                    mb.Object = b;
                    if (b.GetInt("ani") == 0) mb.Image = MapEditor.file.Directory.GetIMG("Back/" + b.GetString("bS") + ".img").GetChild("back/" + b.GetInt("no").ToString());
                    else mb.Image = MapEditor.file.Directory.GetIMG("Back/" + b.GetString("bS") + ".img").GetChild("ani/" + b.GetInt("no").ToString() + "/0");
                    mb.ID = int.Parse(b.Name);

                    if (b.GetInt("ani") == 1) mb.GenerateFrames();

                    backs.Add(mb);
                }
            }

            backs = backs.OrderBy(o => o.ID).ToList<MapBack>();

            IMGEntry elr = map.GetChild("ladderRope");
            if (elr != null)
            {
                foreach (IMGEntry lr in elr.childs.Values)
                {
                    MapLR mlr = new MapLR();

                    mlr.Object = lr;
                    mlr.ID = int.Parse(lr.Name);
                    mlr.s1 = new MapLRSide();
                    mlr.s2 = new MapLRSide();
                    mlr.s1.Object = lr;
                    mlr.s2.Object = lr;
                    mlr.s1.ID = 1;
                    mlr.s2.ID = 2;
                    mlr.s1.LR = mlr;
                    mlr.s2.LR = mlr;

                    lrs.Add(mlr);
                }
            }

            lrs = lrs.OrderBy(l => l.ID).ToList<MapLR>();

            IMGEntry eseats = map.GetChild("seat");
            if (eseats != null)
            {
                foreach (IMGEntry s in eseats.childs.Values)
                {
                    MapSeat ms = new MapSeat();

                    ms.Object = s;
                    ms.ID = int.Parse(s.Name);

                    seats.Add(ms);
                }
            }

            seats = seats.OrderBy(l => l.ID).ToList<MapSeat>();


            IMGEntry eLifes = map.GetChild("life");
            if (eLifes != null)
            {
                foreach (IMGEntry l in eLifes.childs.Values)
                {
                    MapLife ml;
                    if (l.GetString("type") == "n")
                    {
                        ml = new MapNPC();
                    }
                    else
                    {
                        ml = new MapMob();
                    }

                    ml.Object = l;

                    if (ml is MapNPC)
                    {
                        IMGFile npc = MapEditor.npc.Directory.GetIMG(l.GetString("id") + ".img");
                        if (npc.GetChild("info/link") != null)
                        {
                            npc = MapEditor.npc.Directory.GetIMG(npc.GetString("info/link") + ".img");
                        }
                        ml.Image = npc.GetChild("stand/0");

                    }
                    else
                    {
                        IMGFile mob = MapEditor.mob.Directory.GetIMG(l.GetString("id") + ".img");
                        if (mob.GetChild("info/link") != null)
                        {
                            mob = MapEditor.mob.Directory.GetIMG(mob.GetString("info/link") + ".img");
                        }
                        ml.Image = mob.GetChild("stand/0");
                        if (ml.Image == null) ml.Image = mob.GetChild("fly/0");
                    }
                    ml.Image = GetRealImage(ml.Image);
                    ml.ID = int.Parse(l.Name);

                    lifes.Add(ml);
                }
            }

            lifes = lifes.OrderBy(l => l.ID).ToList<MapLife>();

            IMGEntry eReactors = map.GetChild("reactor");
            if (eReactors != null)
            {
                foreach (IMGEntry r in eReactors.childs.Values)
                {
                    MapReactor mr = new MapReactor();
                    mr.Object = r;

                    IMGFile reactor = MapEditor.reactor.Directory.GetIMG(r.GetString("id") + ".img");
                    if (reactor.GetChild("info/link") != null)
                    {
                        reactor = MapEditor.reactor.Directory.GetIMG(reactor.GetString("info/link") + ".img");
                    }
                    mr.Image = reactor.GetChild("0/0");
                    mr.Image = GetRealImage(mr.Image);
                    mr.ID = int.Parse(r.Name);

                    reactors.Add(mr);
                }
            }

            lifes = lifes.OrderBy(l => l.ID).ToList<MapLife>();

            IMGEntry pImage = MapEditor.file.Directory.GetIMG("MapHelper.img").GetChild("portal/game/pv/0");

            IMGEntry ePortals = map.GetChild("portal");
            if (ePortals != null)
            {
                foreach (IMGEntry p in ePortals.childs.Values)
                {
                    MapPortal mp = new MapPortal();

                    mp.Object = p;
                    mp.ID = int.Parse(p.Name);
                    mp.Image = pImage;

                    portals.Add(mp);
                }
            }

            seats = seats.OrderBy(l => l.ID).ToList<MapSeat>();

            IMGEntry tooltipsE = map.GetChild("ToolTip");
            if (tooltipsE != null)
            {
                foreach (IMGEntry tte in tooltipsE.childs.Values)
                {
                    if (tte.Name.Contains("char")) continue;
                    MapToolTip tt = new MapToolTip();

                    tt.Object = tte;
                    tt.ID = int.Parse(tte.Name);
                    tt.Image = MapEditor.stringf.Directory.GetIMG("ToolTipHelp.img").GetChild("Mapobject").GetChild(int.Parse(MapEditor.Instance.MapID).ToString()).GetChild(tte.Name);
                    tt.c1 = new MapToolTipCorner();
                    tt.c1.Object = tte;
                    tt.c1.type = MapToolTipCornerType.TopLeft;
                    tt.c1.ToolTip = tt;
                    tt.c2 = new MapToolTipCorner();
                    tt.c2.Object = tte;
                    tt.c2.type = MapToolTipCornerType.TopRight;
                    tt.c2.ToolTip = tt;
                    tt.c3 = new MapToolTipCorner();
                    tt.c3.Object = tte;
                    tt.c3.type = MapToolTipCornerType.BottomLeft;
                    tt.c3.ToolTip = tt;
                    tt.c4 = new MapToolTipCorner();
                    tt.c4.Object = tte;
                    tt.c4.type = MapToolTipCornerType.BottomRight;
                    tt.c4.ToolTip = tt;

                    tooltips.Add(tt);
                }
            }
            IMGEntry clockEntry = map.GetChild("clock");
            if (clockEntry != null)
            {
                clock = new MapClock();

                clock.Object = clockEntry;
                clock.Image = MapEditor.file.Directory.GetIMG("Obj/etc.img").GetChild("clock/fontTime");
            }

        }
        public int GenerateFootholdsGroupID()
        {
            int id = 0;
            while (footholdsGroups.Contains(id)) { id++; }
            return id;
        }

        public int GenerateFootholdID()
        {
            int id = 1;
            while (footholds.Contains(id)) { id++; }
            return id;
        }

        public void Delete(MapItem item)
        {
            if (item is MapLife)
            {
                MapLife l = (MapLife)item;
                if (lifes.Contains(l))
                {
                    lifes.Remove(l);
                    map.GetChild("life").childs.Remove(l.Object.Name);
                    Map.Pad(map.GetChild("life"), lifes.ToArray());
                }
            }
            else if (item is MapReactor)
            {
                MapReactor r = (MapReactor)item;
                if (reactors.Contains(r))
                {
                    reactors.Remove(r);
                    map.GetChild("reactor").childs.Remove(r.Object.Name);
                    Map.Pad(map.GetChild("reactor"), reactors.ToArray());
                }
            }
            else if (item is MapLR)
            {
                MapLR l = (MapLR)item;
                if (lrs.Contains(l))
                {
                    lrs.Remove(l);
                    map.GetChild("ladderRope").childs.Remove(l.Object.Name);
                    Map.Pad(map.GetChild("ladderRope"), lrs.ToArray(), 1);
                }
            }
            else if (item is MapSeat)
            {
                MapSeat s = (MapSeat)item;
                if (seats.Contains(s))
                {
                    seats.Remove(s);
                    map.GetChild("seat").childs.Remove(s.Object.Name);
                    Map.Pad(map.GetChild("seat"), seats.ToArray());
                }
            }
            else if (item is MapPortal)
            {
                MapPortal p = (MapPortal)item;
                if (portals.Contains(p))
                {
                    portals.Remove(p);
                    map.GetChild("portal").childs.Remove(p.Object.Name);
                    Map.Pad(map.GetChild("portal"), portals.ToArray());
                    // TODO SORT
                }
            }
            else if (item is MapToolTip)
            {
                MapToolTip t = (MapToolTip)item;
                if (tooltips.Contains(t))
                {
                    tooltips.Remove(t);
                    map.GetChild("ToolTip").childs.Remove(t.Object.Name);
                    t.Image.parent.childs.Remove(t.Object.Name);
                    Map.Pad(map.GetChild("ToolTip"), t.Image.parent, tooltips.ToArray());
                }
            }
            else if (item is MapToolTipCorner)
            {
                Delete(((MapToolTipCorner)item).ToolTip);
            }
            else if (item is MapClock)
            {
                if (clock != null)
                {
                    map.childs.Remove("clock");
                    clock = null;
                }
            }
            else
            {
                layers[(int)MapEditor.Instance.Layer.Value].Delete(item);
            }
        }

        public void Add(MapItem item)
        {
            if (item is MapLife)
            {
                MapLife l = (MapLife)item;
                int id = 0;
                IMGEntry le = map.GetChild("life");
                if (le == null)
                {
                    le = new IMGEntry();
                    le.Name = "life";
                    Map.Instance.map.Add(le);
                }
                while (le.childs.Contains(id.ToString())) id++;
                l.ID = id;
                l.Object.Name = l.ID.ToString();
                lifes.Add(l);
                lifes.Sort(Map.CompareItems);
                le.Add(l.Object);
            }
            else if (item is MapReactor)
            {
                MapReactor r = (MapReactor)item;
                int id = 0;
                IMGEntry re = map.GetChild("reactor");
                if (re == null)
                {
                    re = new IMGEntry();
                    re.Name = "reactor";
                    Map.Instance.map.Add(re);
                }
                while (re.childs.Contains(id.ToString())) id++;
                r.ID = id;
                r.Object.Name = r.ID.ToString();
                reactors.Add(r);
                reactors.Sort(Map.CompareItems);
                re.Add(r.Object);
            }
            else if (item is MapLR)
            {
                MapLR l = (MapLR)item;
                int id = 1;
                IMGEntry lrse = map.GetChild("ladderRope");
                if (lrse == null)
                {
                    lrse = new IMGEntry();
                    lrse.Name = "ladderRope";
                    Map.Instance.map.Add(lrse);
                }
                while (lrse.childs.Contains(id.ToString())) id++;
                l.ID = id;
                l.Object.Name = l.ID.ToString();
                lrs.Add(l);
                lrs.Sort(Map.CompareItems);
                lrse.Add(l.Object);
            }
            else if (item is MapSeat)
            {
                MapSeat s = (MapSeat)item;
                int id = 0;
                IMGEntry seate = map.GetChild("seat");
                if (seate == null)
                {
                    seate = new IMGEntry();
                    seate.Name = "seat";
                    Map.Instance.map.Add(seate);
                }
                while (seate.childs.Contains(id.ToString())) id++;
                s.ID = id;
                s.Object.Name = s.ID.ToString();
                seats.Add(s);
                seats.Sort(Map.CompareItems);
                seate.Add(s.Object);
            }
            else if (item is MapPortal)
            {
                MapPortal p = (MapPortal)item;
                int id = 0;
                IMGEntry portalse = map.GetChild("portal");
                if (portalse == null)
                {
                    portalse = new IMGEntry();
                    portalse.Name = "portal";
                    Map.Instance.map.Add(portalse);
                }
                while (portalse.childs.Contains(id.ToString())) id++;
                p.ID = id;
                p.Object.Name = p.ID.ToString();
                p.Image = MapEditor.file.Directory.GetIMG("MapHelper.img").GetChild("portal/game/pv/0");
                portals.Add(p);
                portals.Sort(Map.CompareItems);
                portalse.Add(p.Object);
            }
            else if (item is MapToolTip)
            {
                MapToolTip t = (MapToolTip)item;
                int id = 0;
                IMGEntry ToolTips = map.GetChild("ToolTip");
                if (ToolTips == null)
                {
                    ToolTips = new IMGEntry();
                    ToolTips.Name = "ToolTip";
                    Map.Instance.map.Add(ToolTips);
                }
                while (ToolTips.childs.Contains(id.ToString())) id++;
                t.ID = id;
                t.Object.Name = t.ID.ToString();
                tooltips.Add(t);
                tooltips.Sort(Map.CompareItems);
                ToolTips.Add(t.Object);
            }
            else if (item is MapClock)
            {
                if (clock == null)
                {
                    clock = item as MapClock;
                    clock.Object.Name = "clock";
                    clock.Image = MapEditor.file.Directory.GetIMG("Obj/etc.img").GetChild("clock/fontTime");
                    map.Add(clock.Object);
                }
            }
            else
            {
                layers[(int)MapEditor.Instance.Layer.Value].Add(item);
            }
        }

        public MapItem GetItemAt(int x, int y)
        {
            if (MapEditor.Instance.EditSeat.Checked)
            {
                foreach (MapSeat seat in seats)
                {
                    if (seat.IsPointInArea(x, y))
                    {
                        return seat;
                    }
                }
            }
            else if (MapEditor.Instance.EditPortal.Checked)
            {
                foreach (MapPortal portal in portals)
                {
                    if (portal.IsPointInArea(x, y))
                    {
                        return portal;
                    }
                }
            }
            else if (MapEditor.Instance.EditToolTip.Checked)
            {
                foreach (MapToolTip tooltip in tooltips)
                {
                    MapItem corner = tooltip.GetItemAt(x, y);

                    if (corner != null)
                    {
                        return corner;
                    }
                }
            }
            else if (MapEditor.Instance.EditClock.Checked)
            {
                if (clock != null)
                {
                    return clock.IsPointInArea(x, y) ? clock : null;
                }
            }
            return null;
        }


        public MapPortal GetPortal(string name)
        {
            foreach (MapPortal portal in portals)
            {
                if (portal.Object.GetString("pn") == name)
                {
                    return portal;
                }
            }
            return null;
        }

        public Point GetLifeDesignPosition(MapLife life)
        {
            int x = life.Object.GetInt("x");
            int cy = life.Object.GetInt("cy");
            double minDis = 0;
            int fhid = 0, y = 0;
            foreach (MapLayer l in layers)
            {
                foreach (MapFootholds f in l.footholdGroups.Values)
                {
                    foreach (MapFoothold fh in f.footholds.Values)
                    {
                        int x1 = fh.Object.GetInt("x1");
                        int y1 = fh.Object.GetInt("y1");
                        int x2 = fh.Object.GetInt("x2");
                        int y2 = fh.Object.GetInt("y2");
                        if ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))
                        {
                            double nY = (double)(y1 - y2) / (x1 - x2) * (x - x1) + y1;
                            double dis = Math.Abs(nY - cy);
                            if ((fhid == 0 || dis < minDis) && dis <= 10)
                            {
                                minDis = dis;
                                fhid = fh.ID;
                                y = (int)nY;
                            }
                        }
                    }
                }
            }
            return new Point(y, fhid);
        }

        /*public Point GetObjectDesignPosition(MapObject obj)
        {
            int x = obj.Object.GetInt("x");
            int cy = obj.Object.GetInt("y");
            double minDis = 0;
            int fhid = -1, y = 0;
            foreach (MapLayer l in layers)
            {
                foreach (MapFootholds f in l.footholdGroups.Values)
                {
                    foreach (MapFoothold fh in f.footholds.Values)
                    {
                        int x1 = fh.Object.GetInt("x1");
                        int y1 = fh.Object.GetInt("y1");
                        int x2 = fh.Object.GetInt("x2");
                        int y2 = fh.Object.GetInt("y2");
                        if ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))
                        {
                            double nY = (double)(y1 - y2) / (x1 - x2) * (x - x1) + y1;
                            double dis = Math.Abs(nY - cy);
                            if ((fhid == -1 || dis < minDis) && dis <= 10)
                            {
                                minDis = dis;
                                fhid = f.ID;
                                y = (int)nY;
                            }
                        }
                    }
                }
            }
            return new Point(y, fhid);
        }*/

        public void Draw(Graphics g)
        {
            foreach (MapLayer l in layers)
            {
                l.Draw(g);
            }
        }

        public void DrawAll(Graphics g)
        {
            foreach (MapBack b in backs)
            {
                if (b.Object.GetInt("front") == 0)
                {
                    b.DrawStatic(g);
                }
            }
            foreach (MapLayer l in layers)
            {
                l.Draw(g);
            }
            foreach (MapLife l in lifes)
            {
                if (!footholds.Contains(l.Object.GetInt("fh")))
                {
                    l.Draw(g);
                }
            }
            foreach (MapReactor r in reactors)
            {
                r.Draw(g);
            }
            foreach (MapPortal p in portals)
            {
                p.Draw(g);
            }
            foreach (MapBack b in backs)
            {
                if (b.Object.GetInt("front") == 1)
                {
                    b.DrawStatic(g);
                }
            }
        }
        public void Draw(DevicePanel d)
        {
            d.Cache();

            if (MapEditor.Instance.ShowBack.Checked || MapEditor.Instance.EditBack.Checked)
            {
                int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditBack.Checked) ? 0x32 : 0xFF;
                foreach (MapBack b in backs)
                {
                    if (b.Object.GetInt("front") == 0)
                    {
                        b.Transparency = transparency;
                        b.Draw(d);
                    }
                }
            }
            //long a = System.DateTime.Now.Millisecond;
            foreach (MapLayer l in layers)
            {
                l.Draw(d);
            }
            //Console.Out.WriteLine(System.DateTime.Now.Millisecond - a);
            if (MapEditor.Instance.ShowLife.Checked || MapEditor.Instance.EditLife.Checked)
            {
                int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditLife.Checked) ? 0x32 : 0xFF;
                foreach (MapLife l in lifes)
                {
                    if (!footholds.Contains(l.Object.GetInt("fh")))
                    {
                        l.Transparency = transparency;
                        l.Draw(d);
                    }
                }
            }
            if (MapEditor.Instance.ShowReactor.Checked || MapEditor.Instance.EditReactor.Checked)
            {
                int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditReactor.Checked) ? 0x32 : 0xFF;
                foreach (MapReactor r in reactors)
                {
                    r.Transparency = transparency;
                    r.Draw(d);
                }
            }
            if (MapEditor.Instance.ShowClock.Checked || MapEditor.Instance.EditClock.Checked)
            {
                if (clock != null)
                {
                    int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditClock.Checked) ? 0x32 : 0xFF;
                    clock.Transparency = transparency;
                    clock.Draw(d);
                }
            }
            if (MapEditor.Instance.ShowSeat.Checked || MapEditor.Instance.EditSeat.Checked)
            {
                int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditSeat.Checked) ? 0x32 : 0xFF;
                foreach (MapSeat s in seats)
                {
                    s.Transparency = transparency;
                    s.Draw(d);
                }
            }
            if (MapEditor.Instance.ShowPortal.Checked || MapEditor.Instance.EditPortal.Checked)
            {
                int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditPortal.Checked) ? 0x32 : 0xFF;
                foreach (MapPortal p in portals)
                {
                    p.Transparency = transparency;
                    p.Draw(d);
                }
            }
            if (MapEditor.Instance.ShowBack.Checked || MapEditor.Instance.EditBack.Checked)
            {
                int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditBack.Checked) ? 0x32 : 0xFF;
                foreach (MapBack b in backs)
                {
                    if (b.Object.GetInt("front") == 1)
                    {
                        b.Transparency = transparency;
                        b.Draw(d);
                    }
                }
            }
            if (MapEditor.Instance.ShowToolTip.Checked || MapEditor.Instance.EditToolTip.Checked)
            {
                int transparency = (MapEditor.Instance.EditMode.Checked && !MapEditor.Instance.EditToolTip.Checked) ? 0x32 : 0xFF;
                foreach (MapToolTip t in tooltips)
                {
                    t.Transparency = transparency;
                    t.Draw(d);
                }
            }
            if (MapEditor.Instance.ShowLimits.Checked && VRTop != 0)
            {
                int x1 = CenterX + VRLeft, y1 = CenterY + VRTop, x2 = CenterX + VRRight, y2 = CenterY + VRBottom;
                /*
                 * (X1, Y1)         (X2, Y1)
                 * 
                 * 
                 * (X1, Y2)         (X2, Y2)
                 */

                d.DrawLine(x1, y1, x2, y1, Color.DarkBlue);
                d.DrawLine(x1, y1, x1, y2, Color.DarkBlue);
                d.DrawLine(x2, y2, x2, y1, Color.DarkBlue);
                d.DrawLine(x2, y2, x1, y2, Color.DarkBlue);
            }
            if (MapEditor.Instance.draggingSelection)
            {
                int x1 = (int)(MapEditor.Instance.selectingX / MapEditor.Instance.Zoom), x2 = (int)(MapEditor.Instance.lastX / MapEditor.Instance.Zoom);
                int y1 = (int)(MapEditor.Instance.selectingY / MapEditor.Instance.Zoom), y2 = (int)(MapEditor.Instance.lastY / MapEditor.Instance.Zoom);
                if (x1 != x2 && y1 != y2)
                {
                    if (x1 > x2)
                    {
                        x1 = (int)(MapEditor.Instance.lastX / MapEditor.Instance.Zoom);
                        x2 = (int)(MapEditor.Instance.selectingX / MapEditor.Instance.Zoom);
                    }
                    if (y1 > y2)
                    {
                        y1 = (int)(MapEditor.Instance.lastY / MapEditor.Instance.Zoom);
                        y2 = (int)(MapEditor.Instance.selectingY / MapEditor.Instance.Zoom);
                    }

                    d.DrawRectangle(x1, y1, x2, y2, Color.FromArgb(50, Color.Purple));

                    d.DrawLine(x1, y1, x2, y1, Color.Purple);
                    d.DrawLine(x1, y1, x1, y2, Color.Purple);
                    d.DrawLine(x2, y2, x2, y1, Color.Purple);
                    d.DrawLine(x2, y2, x1, y2, Color.Purple);
                }
            }
            if (MapEditor.Instance.DrawMode.Checked)
            {
                foreach (MapCurve curve in MapEditor.Instance.curves)
                {
                    curve.Draw(d);
                }
            }
        }

        public void DrawAnimation(DevicePanel d)
        {
            d.Cache();

            //long a = System.DateTime.Now.Millisecond;
            //Console.Out.WriteLine(System.DateTime.Now.Millisecond - a);

            foreach (MapBack b in backs)
            {
                if (b.Object.GetInt("front") == 0)
                {
                    b.DrawAnimation(d);
                }
            }

            foreach (MapLayer l in layers)
            {
                l.DrawAnimation(d);
            }

            foreach (MapBack b in backs)
            {
                if (b.Object.GetInt("front") == 1)
                {
                    b.DrawAnimation(d);
                }
            }
        }

        public MapLR GetLadderOrRope(int x1, int y1, int x2, int y2)
        {
            if (y1 < y2)
            {
                int temp = y2;
                y2 = y1;
                y1 = temp;
            }
            foreach (MapLR lr in lrs)
            {
                if (lr.x >= x1 - 10 && lr.x <= x2 + 10 && lr.y2 >= y2 && lr.y1 <= y1)
                {
                    return lr;
                }
            }
            return null;
        }

        public double walk { get { return 1; } }
        public double drag { get { return 1; } }
        public double g { get { return 1; } }
        public int left, right, top, bottom;
        public int m_nBaseZMass { get { foreach (MapLayer layer in layers) if (layer.footholdGroups.Count > 0) return layer.ID; return 0; } }

        internal List<MapFoothold> GetFootholds(int x0, int y0, int x, int y)
        {
            int l = Math.Min(x0, x), r = Math.Max(x0, x);
            int t = Math.Min(y0, y), b = Math.Max(y0, y);
            List<MapFoothold> list = new List<MapFoothold>();
            foreach (MapLayer layer in layers)
            {
                foreach (MapFootholds group in layer.footholdGroups.Values)
                {
                    foreach (MapFoothold fh in group.footholds.Values)
                    {
                        int fhl = Math.Min(fh.Object.GetInt("x1"), fh.Object.GetInt("x2")), fhr = Math.Max(fh.Object.GetInt("x1"), fh.Object.GetInt("x2"));
                        int fht = Math.Min(fh.Object.GetInt("y1"), fh.Object.GetInt("y2")), fhb = Math.Max(fh.Object.GetInt("y1"), fh.Object.GetInt("y2"));
                        if (fhr >= l && fhl <= r && fhb >= t && fht <= b)
                        {
                            list.Add(fh);
                        }
                    }
                }
            }
            return list;
        }

        internal bool IsFootholdBelow(double x, double y, int height, MapFoothold ofh)
        {
            foreach (MapLayer layer in layers)
            {
                foreach (MapFootholds group in layer.footholdGroups.Values)
                {
                    foreach (MapFoothold fh in group.footholds.Values)
                    {
                        if (fh == ofh || fh.m_x1 == fh.m_x2) continue;
                        if (x >= fh.m_x1 && x <= fh.m_x2 && Math.Max(fh.m_y1, fh.m_y2) >= y && Math.Min(fh.m_y1, fh.m_y2) <= y + height)
                        {
                            double ty = ((x - fh.m_x1) * (fh.m_y2 - fh.m_y1) / (double)(fh.m_x2 - fh.m_x1) + fh.m_y1);
                            if (ty >= y && ty <= y + height) return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
