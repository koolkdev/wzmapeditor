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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WZ;
using WZ.Objects;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing.Drawing2D;
using WZMapEditor.Actions;
using System.Collections;
using System.Net;

namespace WZMapEditor
{
    public partial class MapEditor : Form
    {
        public static String version = "2.0.2.58";
        public Random random = new Random();
        public List<MapItem> selected = new List<MapItem>();
        bool dragged;
        public int lastX, lastY, sX, sY;
        public int ShiftX = 0, ShiftY = 0, ScreenWidth, ScreenHeight;
        int layerLastValue;
        bool CtrlPressed;
        bool ShiftPressed;
        MapItem selectedDragged;
        MapItem selectedDragged2;
        MapFootholds selectedGroup;
        public List<MapFootholdDesign> FHCreating = new List<MapFootholdDesign>();
        public List<MapLRDesign> LRCreating = new List<MapLRDesign>();
        public string MapID;
        public Stack<IAction> undo = new Stack<IAction>();
        public Stack<IAction> redo = new Stack<IAction>();
        public bool draggingSelection;
        public int selectingX, selectingY;
        string StringFileName, MapFileName, UIFileName;
        public double Zoom = 1;
        public int ZoomLevel = 0;
        public IMGFile SelectedIMG;
        public static object MapLock = new object(), MobLock = new object(), NpcLock = new object(), StringLock = new object(), SoundLock = new object(), UILock = new object(), ReactorLock = new object();
        public bool scrolling = false;
        private bool scrollingDragged = false, wheelClicked = false;
        public Point scrollPosition;
        public List<MapCurve> curves = new List<MapCurve>();
        private MapCurve createdCurve;

        public Hashtable mobNames = new Hashtable();
        public Hashtable npcNames = new Hashtable();

        public List<MapItem> TempChanged = new List<MapItem>();

        public List<MapItem> Clipboard = new List<MapItem>();
        int ClipMode;
        string ClipLayerType;

        public System.Windows.Forms.NumericUpDown Layer;

        private ImageViewer m_ActiveImageViewer;

        List<String> ObjectsIMGs;

        public static MapEditor Instance;

        public MapEditor()
        {
            Instance = this;
            InitializeComponent();

            this.splitContainer1.Panel2MinSize = 254;

            GraphicPanel.GotFocus += new EventHandler(OnGotFocus);
            GraphicPanel.LostFocus += new EventHandler(OnLostFocus);

            Layer = LayerItem.NumericUpDownControl;

            GraphicPanel.Width = splitContainer1.SplitterDistance - (vScrollBar1.Visible ? 19 : 0);
            GraphicPanel.Height = splitContainer1.Height - (hScrollBar1.Visible ? 18 : 0);
        }

        public static WZFile file = null, mob, npc, sound, stringf, reactor, ui;

        public void OnGotFocus(object sender, EventArgs e)
        {
        }
        public void OnLostFocus(object sender, EventArgs e)
        {
        }

        public int GetMode()
        {
            if (EditTile.Checked) return 1;
            if (EditObj.Checked) return 2;
            if (EditLife.Checked) return 3;
            if (EditReactor.Checked) return 4;
            return 0;
        }

        public void CreateFootholds(List<MapFootholdDesign> footholds)
        {
            if (footholds.Count == 1) return;
            int sum = 0;
            for (int i = 0; i < footholds.Count - 1; i++)
            {
                sum += Math.Sign(footholds[i + 1].Object.GetInt("x") - footholds[i].Object.GetInt("x"));
            }
            if (sum < 0) footholds.Reverse();
            IMGEntry gentry = new IMGEntry();
            gentry.Name = Map.Instance.GenerateFootholdsGroupID().ToString();
            IMGEntry layer = Map.Instance.map.GetChild("foothold/" + (int)Layer.Value);
            if (layer == null)
            {
                layer = new IMGEntry();
                layer.Name = ((int)Layer.Value).ToString();
                Map.Instance.map.GetChild("foothold").Add(layer);
            }
            layer.Add(gentry);
            MapFootholds group = new MapFootholds(int.Parse(gentry.Name));
            group.Object = gentry;
            Map.Instance.layers[(int)Layer.Value].footholdGroups.Add(group.ID, group);
            List<IMGEntry> fhs = new List<IMGEntry>();
            for (int i = 0; i < footholds.Count - 1; i++)
            {
                IMGEntry entry = new IMGEntry();
                entry.Name = Map.Instance.GenerateFootholdID().ToString();
                entry.SetInt("x1", footholds[i].GetX() - Map.Instance.CenterX);
                entry.SetInt("y1", footholds[i].GetY() - Map.Instance.CenterY);
                entry.SetInt("x2", footholds[i + 1].GetX() - Map.Instance.CenterX);
                entry.SetInt("y2", footholds[i + 1].GetY() - Map.Instance.CenterY);
                if (i == 0)
                {
                    entry.SetInt("prev", 0);
                }
                else
                {
                    entry.SetInt("prev", int.Parse(fhs[i - 1].Name));
                    fhs[i - 1].SetInt("next", int.Parse(entry.Name));
                }
                entry.SetInt("next", 0);
                gentry.Add(entry);
                MapFoothold fh = new MapFoothold(int.Parse(entry.Name));
                fh.Object = entry;
                fh.s1 = new MapFootholdSide();
                fh.s2 = new MapFootholdSide();
                fh.s1.ID = 1;
                fh.s2.ID = 2;
                fh.s1.Object = entry;
                fh.s2.Object = entry;
                fh.s1.Foothold = fh;
                fh.s2.Foothold = fh;
                fh.Group = group;
                group.footholds.Add(fh.ID, fh);
                fhs.Add(entry);
            }
        }

        private MapFootholds CreateFootholds(List<List<MapFootholdDesign>> footholdslist)
        {
            IMGEntry gentry = new IMGEntry();
            gentry.Name = Map.Instance.GenerateFootholdsGroupID().ToString();
            IMGEntry layer = Map.Instance.map.GetChild("foothold/" + (int)Layer.Value);
            if (layer == null)
            {
                layer = new IMGEntry();
                layer.Name = ((int)Layer.Value).ToString();
                Map.Instance.map.GetChild("foothold").Add(layer);
            }
            layer.Add(gentry);
            MapFootholds group = new MapFootholds(int.Parse(gentry.Name));
            group.Object = gentry;
            Map.Instance.layers[(int)Layer.Value].footholdGroups.Add(group.ID, group);
            foreach (List<MapFootholdDesign> footholds in footholdslist)
            {
                List<IMGEntry> fhs = new List<IMGEntry>();
                for (int i = 0; i < footholds.Count - 1; i++)
                {
                    IMGEntry entry = new IMGEntry();
                    entry.Name = Map.Instance.GenerateFootholdID().ToString();
                    entry.SetInt("x1", footholds[i].GetX() - Map.Instance.CenterX);
                    entry.SetInt("y1", footholds[i].GetY() - Map.Instance.CenterY);
                    entry.SetInt("x2", footholds[i + 1].GetX() - Map.Instance.CenterX);
                    entry.SetInt("y2", footholds[i + 1].GetY() - Map.Instance.CenterY);
                    if (i == 0)
                    {
                        entry.SetInt("prev", 0);
                    }
                    else
                    {
                        entry.SetInt("prev", int.Parse(fhs[i - 1].Name));
                        fhs[i - 1].SetInt("next", int.Parse(entry.Name));
                    }
                    entry.SetInt("next", 0);
                    gentry.Add(entry);
                    MapFoothold fh = new MapFoothold(int.Parse(entry.Name));
                    fh.Object = entry;
                    fh.s1 = new MapFootholdSide();
                    fh.s2 = new MapFootholdSide();
                    fh.s1.ID = 1;
                    fh.s2.ID = 2;
                    fh.s1.Object = entry;
                    fh.s2.Object = entry;
                    fh.s1.Foothold = fh;
                    fh.s2.Foothold = fh;
                    fh.Group = group;
                    group.footholds.Add(fh.ID, fh);
                    fhs.Add(entry);
                }
            }
            return group;
        }
        private void CreateLRs(List<MapLRDesign> LRCreating)
        {
            if (LRCreating[0].GetY() > LRCreating[1].GetY())
            {
                LRCreating.Reverse();
            }
            IMGEntry entry = new IMGEntry();
            entry.SetInt("l", LRCreating[0].ladder ? 1 : 0);
            entry.SetInt("uf", 1);
            entry.SetInt("x", LRCreating[0].GetX() - Map.Instance.CenterX);
            entry.SetInt("y1", LRCreating[0].GetY() - Map.Instance.CenterY);
            entry.SetInt("y2", LRCreating[1].GetY() - Map.Instance.CenterY);
            entry.SetInt("page", (int)Layer.Value);
            MapLR lr = new MapLR();
            lr.Object = entry;
            lr.s1 = new MapLRSide();
            lr.s2 = new MapLRSide();
            lr.s1.ID = 1;
            lr.s2.ID = 2;
            lr.s1.Object = entry;
            lr.s2.Object = entry;
            lr.s1.LR = lr;
            lr.s2.LR = lr;
            Map.Instance.Add(lr);
        }
        private void CreateSeat(MapSeatDesign seatd)
        {
            IMGEntry entry = new IMGEntry();
            entry.SetVector(new WZVector(seatd.GetX() - Map.Instance.CenterX, seatd.GetY() - Map.Instance.CenterY));

            MapSeat seat = new MapSeat();
            seat.Object = entry;

            Map.Instance.Add(seat);
        }

        public void MagnetDisable()
        {
            if (EditTile.Checked && selected.Count == 1)
            {
                if (((MapTile)selected[0]).Magnet)
                {
                    ((MapTile)selected[0]).Magnet = false;
                    ((MapTile)selected[0]).Object.SetInt("x", ((MapTile)selected[0]).MagnetX);
                    ((MapTile)selected[0]).Object.SetInt("y", ((MapTile)selected[0]).MagnetY);
                    ((MapTile)selected[0]).Cache();
                }
            }
            if (EditLife.Checked && selected.Count == 1)
            {
                MapLife l = ((MapLife)selected[0]);
                if (l.Magnet)
                {
                    l.Magnet = false;
                    int h = l.MagnetY - l.Object.GetInt("cy");
                    l.Object.SetInt("cy", l.MagnetY);
                    l.Object.SetInt("y", l.Object.GetInt("y") + h);
                    l.Cache();
                }
            }
            /*if (EditObj.Checked && selected.Count == 1)
            {
                MapObject o = ((MapObject)selected[0]);
                if (o.Magnet)
                {
                    o.Magnet = false;
                    o.Object.SetInt("y", o.MagnetY);
                }
            }*/
        }

        public void DeleteSelected()
        {
            if (EditObj.Checked || EditTile.Checked || EditLife.Checked || EditReactor.Checked)
            {
                foreach (MapItem s in selected)
                {
                    Map.Instance.layers[(int)Layer.Value].Delete(s);
                }
                undo.Push(new ActionDelete(new List<MapItem>(selected), (int)Layer.Value));
                redo.Clear();
                Deselect();
            }
            else if (EditFH.Checked)
            {
                if (selected.Count == 1)
                {
                    MapFootholdSide fh = (MapFootholdSide)selected[0];
                    if (fh.Foothold.Group.footholds.Count == 1)
                    {
                        Map.Instance.layers[(int)Layer.Value].footholdGroups.Remove(fh.Foothold.Group.ID);
                        foreach (MapFoothold f in fh.Foothold.Group.footholds.Values)
                        {
                            Map.Instance.footholds.Remove(f.ID);
                        }
                        Map.Instance.footholdsGroups.Remove(fh.Foothold.Group.ID);
                        Map.Instance.map.GetChild("foothold/" + (int)Layer.Value).childs.Remove(fh.Foothold.Group.ID.ToString());
                    }
                    else
                    {
                        fh.Foothold.Group.Object.childs.Remove(fh.Foothold.Object.Name);
                        fh.Foothold.Group.footholds.Remove(fh.Foothold.ID);
                        Map.Instance.footholds.Remove(fh.Foothold.ID);
                        if (fh.ID == 1)
                        {
                            fh = fh.Foothold.s2;
                        }
                        else
                        {
                            fh = fh.Foothold.s1;
                        }
                        if (fh.Foothold.Group.footholds.Contains(fh.GetConnected()))
                        {
                            if (fh.ID == 1)
                            {
                                ((MapFoothold)fh.Foothold.Group.footholds[fh.GetConnected()]).Object.SetInt("next", 0);
                            }
                            else
                            {
                                ((MapFoothold)fh.Foothold.Group.footholds[fh.GetConnected()]).Object.SetInt("prev", 0);
                            }
                        }

                    }
                    Deselect();
                }
                else if (selected.Count == 2)
                {
                    // TODO
                }
            }
            else if (EditLR.Checked)
            {
                Map.Instance.Delete(((MapLRSide)selected[0]).LR);
            }
            else if (EditSeat.Checked || EditPortal.Checked || EditToolTip.Checked || EditClock.Checked)
            {
                Map.Instance.Delete(selected[0]);
                undo.Push(new ActionDelete(selected[0]));
                redo.Clear();
                Deselect();
            }
        }

        private void ShortFoothold(MapFootholdSide fh)
        {
            int x1 = fh.Object.GetInt("x1");
            int y1 = fh.Object.GetInt("y1");
            int x2 = fh.Object.GetInt("x2");
            int y2 = fh.Object.GetInt("y2");
            double d = MapFoothold.Distance(x1, y1, x2, y2);
            if (fh.ID == 1)
            {
                double xp = (x2 * 10 + x1 * (d - 10)) / d;
                double yp = (y2 * 10 + y1 * (d - 10)) / d;
                fh.Object.SetInt("x1", (int)xp);
                fh.Object.SetInt("y1", (int)yp);
            }
            else
            {
                double xp = (x1 * 10 + x2 * (d - 10)) / d;
                double yp = (y1 * 10 + y2 * (d - 10)) / d;
                fh.Object.SetInt("x2", (int)xp);
                fh.Object.SetInt("y2", (int)yp);
            }
        }

        public void GraphicPanel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                CtrlPressed = true;
            }
            else if (e.KeyCode == Keys.ShiftKey)
            {
                ShiftPressed = true;
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                if (undo.Count > 0)
                {
                    IAction act = undo.Pop();
                    act.Undo();
                    redo.Push(act.Redo());
                }
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                if (redo.Count > 0)
                {
                    IAction act = redo.Pop();
                    act.Undo();
                    undo.Push(act.Redo());
                }
            }
            else if (e.Control && e.KeyCode == Keys.O)
            {
                Open_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                New_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                Save_Click(null, null);
            }
            else if (e.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
            {
                ZoomLevel = 0;
                switch (ZoomLevel)
                {
                    case 3: Zoom = 2; break;
                    case 2: Zoom = 3 / 2.0; break;
                    case 1: Zoom = 5 / 4.0; break;
                    case 0: Zoom = 1; break;
                    case -1: Zoom = 2 / 3.0; break;
                    case -2: Zoom = 1 / 2.0; break;
                    case -3: Zoom = 1 / 3.0; break;
                    case -4: Zoom = 1 / 4.0; break;
                    case -5: Zoom = 1 / 8.0; break;
                }

                double maxZoomX = 8000.0 / Map.Instance.Width, maxZoomY = 8000.0 / Map.Instance.Height;
                Zoom = Math.Min(Zoom, Math.Min(maxZoomX, maxZoomY));
                SetMapSize((int)(Map.Instance.Width * Zoom), (int)(Map.Instance.Height * Zoom));
            }
            if (selected.Count > 0 && !LinkToFH.Checked && !FootholdsCreator.Checked)
            {
                if (e.KeyData == Keys.Up)
                {
                    foreach (MapItem s in selected)
                    {
                        s.Move(0, -1);
                    }
                    if (undo.Count > 0 && undo.Peek() is ActionMove && ((ActionMove)undo.Peek()).Keys == true)
                    {
                        ((ActionMove)undo.Peek()).y -= 1;
                    }
                    else
                    {
                        undo.Push(new ActionMove(new List<MapItem>(selected), 0, -1));
                        ((ActionMove)undo.Peek()).Keys = true;
                    }
                    redo.Clear();
                }
                else if (e.KeyData == Keys.Down)
                {
                    foreach (MapItem s in selected)
                    {
                        s.Move(0, 1);
                    }
                    if (undo.Count > 0 && undo.Peek() is ActionMove && ((ActionMove)undo.Peek()).Keys == true)
                    {
                        ((ActionMove)undo.Peek()).y += 1;
                    }
                    else
                    {
                        undo.Push(new ActionMove(new List<MapItem>(selected), 0, 1));
                        ((ActionMove)undo.Peek()).Keys = true;
                    }
                    redo.Clear();
                }
                else if (e.KeyData == Keys.Left)
                {
                    foreach (MapItem s in selected)
                    {
                        s.Move(-1, 0);
                    }
                    if (undo.Count > 0 && undo.Peek() is ActionMove && ((ActionMove)undo.Peek()).Keys == true)
                    {
                        ((ActionMove)undo.Peek()).x -= 1;
                    }
                    else
                    {
                        undo.Push(new ActionMove(new List<MapItem>(selected), -1, 0));
                        ((ActionMove)undo.Peek()).Keys = true;
                    }
                    redo.Clear();
                }
                else if (e.KeyData == Keys.Right)
                {
                    foreach (MapItem s in selected)
                    {
                        s.Move(1, 0);
                    }
                    if (undo.Count > 0 && undo.Peek() is ActionMove && ((ActionMove)undo.Peek()).Keys == true)
                    {
                        ((ActionMove)undo.Peek()).x += 1;
                    }
                    else
                    {
                        undo.Push(new ActionMove(new List<MapItem>(selected), 1, 0));
                        ((ActionMove)undo.Peek()).Keys = true;
                    }
                    redo.Clear();
                }
                else if (e.KeyData == Keys.Delete)
                {
                    DeleteSelected();
                }
                else if (e.KeyData == Keys.End)
                {
                    if (Layer.Enabled)
                    {
                        if (Layer.Value < 7)
                        {
                            Layer.Value++;
                            Layer_ValueChanged(null, null);
                        }
                    }
                }
                else if (e.KeyData == Keys.Home)
                {
                    if (Layer.Enabled)
                    {
                        if (Layer.Value > 0)
                        {
                            Layer.Value--;
                            Layer_ValueChanged(null, null);
                        }
                    }
                }
                else if (EditObj.Checked)
                {
                    if (e.KeyData == Keys.PageUp)
                    {
                        foreach (MapItem s in selected)
                        {
                            int z = s.Object.GetInt("z") + 1;
                            if (z > 20) z = 20;
                            s.Object.SetInt("z", z);
                            Map.Instance.layers[(int)Layer.Value].OrderObjects();
                            if (s is ICached) (s as ICached).Cache();
                            if (selected.Count == 1)
                                toolStripStatusLabel1.Text = "Z: " + z;
                        }
                    }
                    else if (e.KeyData == Keys.PageDown)
                    {
                        foreach (MapItem s in selected)
                        {
                            int z = s.Object.GetInt("z") - 1;
                            if (z < 0) z = 0;
                            s.Object.SetInt("z", z);
                            Map.Instance.layers[(int)Layer.Value].OrderObjects();
                            if (s is ICached) (s as ICached).Cache();
                            if (selected.Count == 1)
                                toolStripStatusLabel1.Text = "Z: " + z;
                        }
                    }
                    else if (e.KeyData == Keys.F)
                    {
                        foreach (MapItem s in selected)
                        {
                            s.Object.SetInt("f", s.Object.GetBool("f") ? 0 : 1);
                            if (s is ICached) (s as ICached).Cache();
                        }
                    }
                }
                else if (EditLife.Checked || EditReactor.Checked)
                {
                    if (e.KeyData == Keys.F)
                    {
                        foreach (MapItem s in selected)
                        {
                            s.Object.SetInt("f", s.Object.GetBool("f") ? 0 : 1);
                            if (s is ICached) (s as ICached).Cache();
                        }
                    }
                }
                else if (EditFH.Checked)
                {
                    if (e.KeyData == Keys.S)
                    {
                        if (selected.Count == 2)
                        {
                            MapFootholdSide fh1 = (MapFootholdSide)selected[0];
                            MapFootholdSide fh2 = (MapFootholdSide)selected[1];

                            if (fh1.ID == 1)
                            {
                                fh1.Object.SetInt("prev", 0);
                                fh2.Object.SetInt("next", 0);
                            }
                            else
                            {
                                fh1.Object.SetInt("next", 0);
                                fh2.Object.SetInt("prev", 0);
                            }

                            ShortFoothold(fh1);
                            ShortFoothold(fh2);
                            Deselect();
                        }
                    }
                }
            }
            if (EditTile.Checked || EditObj.Checked || EditLife.Checked || EditReactor.Checked)
            {
                if (e.Control)
                {
                    if (selected.Count > 0)
                    {
                        if (e.KeyCode == Keys.D)
                        {
                            List<MapItem> items = new List<MapItem>(selected);
                            Deselect();
                            foreach (MapItem item in items)
                            {
                                MapItem citem = (item as ICloneable).Clone() as MapItem;
                                citem.Object.SetInt("x", citem.Object.GetInt("x") + 30);
                                citem.Object.SetInt("y", citem.Object.GetInt("y") + 30);
                                citem.Selected = true;
                                selected.Add(citem);
                                Map.Instance.Add(citem);
                                if (citem is ICached) (citem as ICached).Cache();
                            }
                            undo.Push(new ActionAdd(new List<MapItem>(selected), (int)Layer.Value));
                        }
                        else if (e.KeyCode == Keys.X)
                        {
                            Clipboard.Clear();
                            Clipboard.AddRange(selected);
                            ClipMode = GetMode();
                            if (ClipMode == 1) ClipLayerType = Map.Instance.layers[(int)Layer.Value].info.GetString("tS");
                            DeleteSelected();
                        }
                        else if (e.KeyCode == Keys.C)
                        {
                            Clipboard.Clear();
                            Clipboard.AddRange(selected);
                            ClipMode = GetMode();
                            if (ClipMode == 1) ClipLayerType = Map.Instance.layers[(int)Layer.Value].info.GetString("tS");
                        }
                    }
                    if (Clipboard.Count > 0 && ClipMode == GetMode())
                    {
                        if (e.KeyCode == Keys.V)
                        {
                            Deselect();
                            if (ClipMode == 1 && Map.Instance.layers[(int)Layer.Value].info.GetString("tS") == "")
                            {
                                Map.Instance.layers[(int)Layer.Value].info.SetString("tS", ClipLayerType);
                                UpdateIMGsList(true);
                            }
                            foreach (MapItem item in Clipboard)
                            {
                                MapItem paste = (item as ICloneable).Clone() as MapItem;
                                paste.Selected = true;
                                selected.Add(paste);
                                Map.Instance.Add(paste);
                            }
                            undo.Push(new ActionAdd(new List<MapItem>(selected), (int)Layer.Value));
                        }
                    }
                }
                if (EditTile.Checked) Map.Instance.layers[(int)Layer.Value].OrderTiles();
            }
        }
        public void GraphicPanel_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                CtrlPressed = false;
            }
            else if (e.KeyCode == Keys.ShiftKey)
            {
                ShiftPressed = false;
            }
        }
        private void GraphicPanel_OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (EditFH.Checked && !FootholdsCreator.Checked)
            {
                if (selected.Count == 1)
                {
                    MapFootholdSide fh = (MapFootholdSide)selected[0];
                    IMGEntry entry = new IMGEntry();
                    entry.Name = Map.Instance.GenerateFootholdID().ToString();
                    if(fh.ID == 1)
                    {
                        entry.SetInt("x1", fh.Object.GetInt("x1") - 20);
                        entry.SetInt("y1", fh.Object.GetInt("y1"));
                        entry.SetInt("x2", fh.Object.GetInt("x1"));
                        entry.SetInt("y2", fh.Object.GetInt("y1"));
                        entry.SetInt("prev", 0);
                        entry.SetInt("next", fh.Foothold.ID);                        
                    }
                    else if (fh.ID == 2)
                    {
                        entry.SetInt("x1", fh.Object.GetInt("x2"));
                        entry.SetInt("y1", fh.Object.GetInt("y2"));
                        entry.SetInt("x2", fh.Object.GetInt("x2") + 20);
                        entry.SetInt("y2", fh.Object.GetInt("y2"));
                        entry.SetInt("prev", fh.Foothold.ID);
                        entry.SetInt("next", 0);                        
                    }
                    fh.Foothold.Group.Object.Add(entry);
                    MapFoothold nFH = new MapFoothold(int.Parse(entry.Name));
                    nFH.Object = entry;
                    nFH.s1 = new MapFootholdSide();
                    nFH.s2 = new MapFootholdSide();
                    nFH.s1.ID = 1;
                    nFH.s2.ID = 2;
                    nFH.s1.Object = entry;
                    nFH.s2.Object = entry;
                    nFH.s1.Foothold = nFH;
                    nFH.s2.Foothold = nFH;
                    nFH.Group = fh.Foothold.Group;
                    fh.Foothold.Group.footholds.Add(nFH.ID, nFH);
                    if (fh.ID == 1)
                    {
                        fh.Object.SetInt("prev", nFH.ID);
                        selected.Add(nFH.s2);
                        nFH.s2.Selected = true;
                    }
                    else
                    {
                        fh.Object.SetInt("next", nFH.ID);
                        selected.Add(nFH.s1);
                        nFH.s1.Selected = true;
                    }
                }
                else if (selected.Count == 0)
                {
                    MapFoothold fh = Map.Instance.layers[(int)Layer.Value].GetFootholdAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                    if (fh != null)
                    {
                        MapFoothold next = (MapFoothold)fh.Group.footholds[fh.Object.GetInt("next")];
                        IMGEntry entry = new IMGEntry();
                        entry.Name = Map.Instance.GenerateFootholdID().ToString();
                        entry.SetInt("x1", e.X - Map.Instance.CenterX);
                        entry.SetInt("y1", e.Y - Map.Instance.CenterY);
                        entry.SetInt("x2", fh.Object.GetInt("x2"));
                        entry.SetInt("y2", fh.Object.GetInt("y2"));
                        entry.SetInt("prev", fh.ID);
                        entry.SetInt("next", (next == null) ? 0 : next.ID);
                        fh.Object.SetInt("x2", e.X - Map.Instance.CenterX);
                        fh.Object.SetInt("y2", e.Y - Map.Instance.CenterY);
                        fh.Object.SetInt("next", int.Parse(entry.Name));
                        if(next != null) next.Object.SetInt("prev", int.Parse(entry.Name));
                        fh.Group.Object.Add(entry);
                        MapFoothold nFH = new MapFoothold(int.Parse(entry.Name));
                        nFH.Object = entry;
                        nFH.s1 = new MapFootholdSide();
                        nFH.s2 = new MapFootholdSide();
                        nFH.s1.ID = 1;
                        nFH.s2.ID = 2;
                        nFH.s1.Object = entry;
                        nFH.s2.Object = entry;
                        nFH.s1.Foothold = nFH;
                        nFH.s2.Foothold = nFH;
                        nFH.Group = fh.Group;
                        fh.Group.footholds.Add(nFH.ID, nFH);
                        selected.Add(nFH.s1);
                        selected.Add(fh.s2);
                        nFH.s1.Selected = true;
                        fh.s2.Selected = true;
                    }
                }
            }
            else if (EditLife.Checked && selected.Count > 0)
            {
                int value = -1;
                foreach(MapLife life in selected)
                {
                    if (life is MapMob)
                    {
                        int mobTime = life.Object.GetInt("mobTime");
                        if (value != -1 && value != mobTime)
                        {
                            value = -1;
                            break;
                        }
                        if (value == -1)
                        {
                            value = mobTime;
                        }
                    }
                }
                if (value != -1)
                {
                    GetRespawnTime t = new GetRespawnTime(value);
                    t.ShowDialog();
                    value = int.Parse(t.RespawnTime.Text);

                    foreach (MapLife life in selected)
                    {
                        if (life is MapMob)
                        {
                            life.Object.SetInt("mobTime", value);
                        }
                    }
                }

            }
            else if (EditReactor.Checked && selected.Count > 0)
            {
                MapReactor reactor = (MapReactor)Map.Instance.layers[(int)Layer.Value].GetItemAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                if (reactor != null)
                {
                    GetReactorInfo t = new GetReactorInfo(reactor.Object.GetString("name"), reactor.Object.GetInt("reactorTime"));
                    t.ShowDialog();

                    reactor.Object.SetString("name", t.RName.Text);

                    reactor.Object.SetInt("reactorTime", int.Parse(t.RespawnTime.Text));
                }
            }
            else if (EditLR.Checked && !FootholdsCreator.Checked)
            {
                MapLR lr = null;
                if (selected.Count > 0)
                {
                    lr = ((MapLRSide)selected[0]).LR;
                }
                else
                {
                    lr = Map.Instance.layers[(int)Layer.Value].GetLRAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                }
                if (lr != null)
                {
                    GetLRType t = new GetLRType(lr.Object.GetInt("l"));
                    t.ShowDialog();
                    lr.Object.SetInt("l", t.Type.SelectedIndex);
                }

            }
            else if (EditPortal.Checked)
            {
                MapPortal portal = Map.Instance.GetItemAt((int)(e.X / Zoom), (int)(e.Y / Zoom)) as MapPortal;
                if (portal != null)
                {
                    GetPortalInfo t = new GetPortalInfo(portal.Object.GetInt("pt"), portal.Object.GetString("pn"), portal.Object.GetInt("tm"), portal.Object.GetString("tn"));
                    t.ShowDialog();
                    int type = t.PortalType.SelectedIndex;
                    switch (type)
                    {
                        case 4: type = 10; break;
                    }
                    portal.Object.SetInt("pt", type);
                    if (type == 0)
                    {
                        portal.Object.SetString("pn", "sp");
                        portal.Object.SetInt("tm", 999999999);
                        portal.Object.SetString("tn", "");
                    }
                    else if (type == 1)
                    {
                        portal.Object.SetString("pn", t.PortalName.Text);
                        if (t.IsTeleport.Checked)
                        {
                            if (t.ThisMap.Checked)
                            {
                                portal.Object.SetInt("tm", int.Parse(MapID));
                            }
                            else
                            {
                                portal.Object.SetInt("tm", int.Parse(t.ToMap.Text));
                            }
                            portal.Object.SetString("tn", t.ToName.Text);
                        }
                        else
                        {
                            portal.Object.SetInt("tm", 999999999);
                            portal.Object.SetString("tn", "");
                        }
                    }
                    else if (type == 2)
                    {
                        portal.Object.SetString("pn", t.PortalName.Text);
                        portal.Object.SetInt("tm", int.Parse(t.ToMap.Text));
                        portal.Object.SetString("tn", t.ToName.Text);
                    }
                    else if (type == 10 || type == 3)
                    {
                        portal.Object.SetString("pn", t.PortalName.Text);
                        if (t.ThisMap.Checked)
                        {
                            portal.Object.SetInt("tm", int.Parse(MapID));
                        }
                        else
                        {
                            portal.Object.SetInt("tm", int.Parse(t.ToMap.Text));
                        }
                        portal.Object.SetString("tn", t.ToName.Text);
                    }
                }

            }
            else if (EditToolTip.Checked)
            {
                MapItem item = Map.Instance.GetItemAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                if (item != null)
                {
                    MapToolTip tt;
                    if (item is MapToolTipCorner)
                    {
                        tt = (item as MapToolTipCorner).ToolTip;
                    }
                    else
                    {
                        tt = item as MapToolTip;
                    }
                    GetToolTipInfo info = new GetToolTipInfo(tt.Image.GetString("Title"), tt.Image.GetString("Desc"));
                    info.ShowDialog();
                    tt.Image.SetString("Title", info.Title.Text);
                    if (info.Desc.Text == "")
                    {
                        tt.Image.childs.Remove("Desc");
                    }
                    else
                    {
                        tt.Image.SetString("Desc", info.Desc.Text);
                    }
                    tt.Image.parent.parent.parent.ToSave = true;
                }
            }
        }
        private void GraphicPanel_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (scrolling)
            {
                if (wheelClicked)
                {
                    scrollingDragged = true;
                }
                
                int xMove = (hScrollBar1.Visible) ? e.X - ShiftX - scrollPosition.X : 0;
                int yMove = (vScrollBar1.Visible) ? e.Y - ShiftY - scrollPosition.Y : 0;

                if (Math.Abs(xMove) < 15) xMove = 0;
                if (Math.Abs(yMove) < 15) yMove = 0;

                if (xMove > 0)
                {
                    if(yMove > 0) Cursor = Cursors.PanSE;
                    else if(yMove < 0) Cursor = Cursors.PanNE;
                    else Cursor = Cursors.PanEast;
                }
                else if (xMove < 0)
                {
                    if(yMove > 0) Cursor = Cursors.PanSW;
                    else if(yMove < 0) Cursor = Cursors.PanNW;
                    else Cursor = Cursors.PanWest;
                }
                else
                {
                    if(yMove > 0) Cursor = Cursors.PanSouth;
                    else if(yMove < 0) Cursor = Cursors.PanNorth;
                    else
                    {
                        if (vScrollBar1.Visible && hScrollBar1.Visible) Cursor = Cursors.NoMove2D;
                        else if (vScrollBar1.Visible) Cursor = Cursors.NoMoveVert;
                        else if (hScrollBar1.Visible) Cursor = Cursors.NoMoveHoriz;
                    }
                }

                Point position = new Point(ShiftX, ShiftY); ;
                int x = xMove / 10 + position.X;
                int y = yMove / 10 + position.Y;

                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x > hScrollBar1.Maximum - hScrollBar1.LargeChange) x = hScrollBar1.Maximum - hScrollBar1.LargeChange;
                if (y > vScrollBar1.Maximum - vScrollBar1.LargeChange) y = vScrollBar1.Maximum - vScrollBar1.LargeChange;
                
                if (x != position.X || y != position.Y)
                {
                    if (vScrollBar1.Visible)
                    {
                        vScrollBar1.Value = y;
                    }
                    if (hScrollBar1.Visible)
                    {
                        hScrollBar1.Value = x;
                    }
                    GraphicPanel.Render();
                    GraphicPanel.OnMouseMoveEventCreate();
                }
            }
            if (draggingSelection || dragged)
            {
                Point position = new Point(ShiftX, ShiftY); ;
                int ScreenMaxX = position.X + splitContainer1.Panel1.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
                int ScreenMaxY = position.Y + splitContainer1.Panel1.Height - System.Windows.Forms.SystemInformation.HorizontalScrollBarHeight;
                int ScreenMinX = position.X;
                int ScreenMinY = position.Y;

                int x = position.X;
                int y = position.Y;

                if (e.X > ScreenMaxX)
                {
                    x += (e.X - ScreenMaxX) / 10;
                }
                else if (e.X < ScreenMinX)
                {
                    x += (e.X - ScreenMinX) / 10;
                }
                if (e.Y > ScreenMaxY)
                {
                    y += (e.Y - ScreenMaxY) / 10;
                }
                else if (e.Y < ScreenMinY)
                {
                    y += (e.Y - ScreenMinY) / 10;
                }

                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x > hScrollBar1.Maximum - hScrollBar1.LargeChange) x = hScrollBar1.Maximum - hScrollBar1.LargeChange;
                if (y > vScrollBar1.Maximum - vScrollBar1.LargeChange) y = vScrollBar1.Maximum - vScrollBar1.LargeChange;

                if (x != position.X || y != position.Y)
                {
                    if (vScrollBar1.Visible)
                    {
                        vScrollBar1.Value = y;
                    }
                    if (hScrollBar1.Visible)
                    {
                        hScrollBar1.Value = x;
                    }
                    GraphicPanel.Render();
                    GraphicPanel.OnMouseMoveEventCreate();
                }

            }
            if (DrawMode.Checked)
            {
                if (createdCurve != null)
                {
                    createdCurve.AddPoint(e.Location);
                }
            }
            else
            {
                //toolStripStatusLabel1.Text = "X: " + e.X + " Y: " + e.Y;
                if (draggingSelection)
                {
                    if (selectingX != e.X && selectingY != e.Y)
                    {
                        int x1 = (int)(selectingX / Zoom), x2 = (int)(e.X / Zoom);
                        int y1 = (int)(selectingY / Zoom), y2 = (int)(e.Y / Zoom);
                        if (x1 > x2)
                        {
                            x1 = (int)(e.X / Zoom);
                            x2 = (int)(selectingX / Zoom);
                        }
                        if (y1 > y2)
                        {
                            y1 = (int)(e.Y / Zoom);
                            y2 = (int)(selectingY / Zoom);
                        }
                        List<MapItem> items;
                        if (EditFH.Checked)
                        {
                            items = Map.Instance.layers[(int)Layer.Value].GetConnectionsAt(new Rectangle(x1, y1, x2 - x1, y2 - y1));
                        }
                        else
                        {
                            items = Map.Instance.layers[(int)Layer.Value].GetItemsAt(new Rectangle(x1, y1, x2 - x1, y2 - y1));
                        }

                        foreach (MapItem item in TempChanged)
                        {
                            if (!items.Contains(item))
                            {
                                if (selected.Contains(item))
                                {
                                    item.Selected = true;
                                }
                                else
                                {
                                    item.Selected = false;
                                }
                            }
                        }
                        TempChanged.Clear();
                        foreach (MapItem item in items)
                        {
                            if (selected.Contains(item) && CtrlPressed)
                            {
                                item.Selected = false;
                            }
                            else
                            {
                                item.Selected = true;
                            }
                            TempChanged.Add(item);
                        }
                    }
                    if (EditTile.Checked) Map.Instance.layers[(int)Layer.Value].OrderTiles();
                }
                else if (dragged)
                {
                    foreach (MapItem s in selected)
                    {
                        s.Move((int)((e.X - lastX) / Zoom), (int)((e.Y - lastY) / Zoom));
                    }
                    if (MagnetMode.Checked && selected.Count == 1)
                    {
                        if (EditTile.Checked)
                        {
                            Point p = Map.Instance.layers[(int)Layer.Value].GetTileDesignPosition((MapTile)selected[0]);
                            if (p != new Point(0xffff, 0xffff))
                            {
                                ((MapTile)selected[0]).MagnetX = p.X;
                                ((MapTile)selected[0]).MagnetY = p.Y;
                                ((MapTile)selected[0]).Magnet = true;
                            }
                            else
                            {
                                ((MapTile)selected[0]).Magnet = false;
                            }
                        }
                        else if (EditLife.Checked)
                        {
                            Point p = Map.Instance.GetLifeDesignPosition((MapLife)selected[0]);
                            if (p != Point.Empty)
                            {
                                ((MapLife)selected[0]).MagnetY = p.X;
                                ((MapLife)selected[0]).Object.SetInt("fh", p.Y);
                                ((MapLife)selected[0]).Magnet = true;
                            }
                            else
                            {
                                ((MapLife)selected[0]).Object.SetInt("fh", 0);
                                ((MapLife)selected[0]).Magnet = false;
                            }
                        }
                        /*else if (EditObj.Checked)
                        {
                            Point p = Map.Instance.GetObjectDesignPosition((MapObject)selected[0]);
                            if (p != new Point(0, -1))
                            {
                                ((MapObject)selected[0]).MagnetY = p.X;
                                ((MapObject)selected[0]).Object.SetInt("zM", p.Y);
                                ((MapObject)selected[0]).Magnet = true;
                            }
                            else
                            {
                                ((MapObject)selected[0]).Magnet = false;
                            }
                        }*/
                    }
                }
            }
            lastX = e.X;
            lastY = e.Y;
        }
        private void GraphicPanel_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (DrawMode.Checked)
                {
                    createdCurve = new MapCurve();
                    createdCurve.AddPoint(e.Location);
                    curves.Add(createdCurve);
                }
                else
                {
                    if (LinkToFH.Checked)
                    {
                        if (EditTile.Checked || EditObj.Checked)
                        {
                            MapFootholds group = Map.Instance.layers[(int)Layer.Value].GetFootholdGroupAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                            if (group != null)
                            {
                                foreach (MapItem s in selected)
                                {
                                    s.Object.SetInt("zM", group.ID);
                                    if (s is ICached) (s as ICached).Cache();
                                }
                                if (selectedGroup != null)
                                {
                                    selectedGroup.Selected = false;
                                }
                                selectedGroup = group;
                                selectedGroup.Selected = true;
                            }
                        }
                        return;
                    }
                    else if (FootholdsCreator.Checked)
                    {
                        if (EditFH.Checked)
                        {
                            MapFootholdDesign fh = Map.Instance.layers[(int)Layer.Value].GetFootholdDesignAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                            if (fh != null)
                            {
                                if (!FHCreating.Contains(fh))
                                {
                                    FHCreating.Add(fh);
                                }
                            }
                            else
                            {
                                if (FHCreating.Count > 0)
                                {
                                    CreateFootholds(FHCreating);
                                    FHCreating.Clear();
                                }
                            }
                        }
                        else if (EditLR.Checked)
                        {
                            MapLRDesign lr = Map.Instance.layers[(int)Layer.Value].GetLRDesignAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                            if (lr != null && !LRCreating.Contains(lr))
                            {
                                if (LRCreating.Count > 0)
                                {
                                    if (LRCreating[0].GetX() == lr.GetX() && LRCreating[0].ladder == lr.ladder)
                                    {
                                        LRCreating.Add(lr);
                                        CreateLRs(LRCreating);
                                        LRCreating.Clear();
                                    }
                                }
                                else
                                {
                                    LRCreating.Add(lr);
                                }
                            }
                            else
                            {
                                LRCreating.Clear();
                            }
                        }
                        return;
                    }
                    //GraphicPanel.Focus();
                    if (EditMode.Checked && Map.Instance != null && !dragged)
                    {
                        if (EditTile.Checked || EditObj.Checked || EditLife.Checked || EditReactor.Checked || EditBack.Checked)
                        {
                            if (Map.Instance.layers[(int)Layer.Value].GetItemAt((int)(e.X / Zoom), (int)(e.Y / Zoom)) == null)
                            {
                                draggingSelection = true;
                                selectingX = e.X;
                                selectingY = e.Y;
                                if (!CtrlPressed && !ShiftPressed)
                                {
                                    Deselect();
                                }
                            }
                            else
                            {
                                if (CtrlPressed && selected.Count > 0)
                                {
                                    MapItem at = Map.Instance.layers[(int)Layer.Value].GetItemAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                                    if (at != null)
                                    {
                                        if (selected.Contains(at))
                                        {
                                            selected.Remove(at);
                                            at.Selected = false;
                                            if (selected.Count == 0) Deselect();
                                            else if (selected.Count == 1 && EditObj.Checked) toolStripStatusLabel1.Text = "Z: " + selected[0].Object.GetInt("z");

                                        }
                                        else
                                        {
                                            selected.Add(at);
                                            at.Selected = true;
                                            toolStripStatusLabel1.Text = "";
                                        }
                                    }
                                }
                                else
                                {
                                    if (EditTile.Checked && selected.Count == 1 && selected[0].IsPointInArea((int)(e.X / Zoom), (int)(e.Y / Zoom)))
                                    {
                                        dragged = true;
                                        selectedDragged = selected[0];
                                        sX = e.X;
                                        sY = e.Y;
                                    }
                                    else
                                    {
                                        MapItem at = Map.Instance.layers[(int)Layer.Value].GetItemAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                                        if (selected.Count > 1 && at != null && (selected.Contains(at)))
                                        {
                                            dragged = true;
                                            selectedDragged = at;
                                            sX = e.X;
                                            sY = e.Y;
                                        }
                                        else
                                        {
                                            Deselect();
                                            if (at != null)
                                            {
                                                selected.Add(at);
                                                at.Selected = true;
                                                if (EditObj.Checked)
                                                    toolStripStatusLabel1.Text = "Z: " + at.Object.GetInt("z");
                                            }
                                        }
                                    }
                                }
                            }
                            if (EditTile.Checked) Map.Instance.layers[(int)Layer.Value].OrderTiles();
                        }
                        else if (EditFH.Checked)
                        {
                            if (Map.Instance.layers[(int)Layer.Value].GetConnectionAt((int)(e.X / Zoom), (int)(e.Y / Zoom)).Count == 0)
                            {
                                draggingSelection = true;
                                selectingX = e.X;
                                selectingY = e.Y;
                                if (!CtrlPressed && !ShiftPressed)
                                {
                                    Deselect();
                                }
                            }
                            else
                            {
                                if (CtrlPressed && selected.Count > 0)
                                {
                                    List<MapFootholdSide> list = Map.Instance.layers[(int)Layer.Value].GetConnectionAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                                    if (list.Count > 0)
                                    {
                                        if (selected.Contains(list[0]))
                                        {
                                            foreach (MapFootholdSide f in list)
                                            {
                                                selected.Remove(f);
                                                f.Selected = false;
                                            }
                                            if (selected.Count == 0) Deselect();

                                        }
                                        else
                                        {
                                            foreach (MapFootholdSide f in list)
                                            {
                                                selected.Add(f);
                                                f.Selected = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    List<MapFootholdSide> list = Map.Instance.layers[(int)Layer.Value].GetConnectionAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                                    if (selected.Count > 2 && list.Count > 0 && selected.Contains(list[0]))
                                    {
                                        dragged = true;
                                        selectedDragged = list[0];
                                        if (list.Count > 1)
                                            selectedDragged2 = list[1];
                                        sX = e.X;
                                        sY = e.Y;
                                    }
                                    else
                                    {
                                        Deselect();
                                        if (list.Count > 0)
                                        {
                                            foreach (MapFootholdSide f in list)
                                            {
                                                selected.Add(f);
                                                f.Selected = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (EditLR.Checked)
                        {
                            MapLRSide lr = Map.Instance.layers[(int)Layer.Value].GetLRSideAt((int)(e.X / Zoom), (int)(e.Y / Zoom));
                            if (lr != null && selected.Contains(lr))
                            {
                                dragged = true;
                                selectedDragged = lr;
                                sX = e.X;
                                sY = e.Y;
                            }
                            else
                            {
                                Deselect();
                                if (lr != null)
                                {
                                    selected.Add(lr);
                                    lr.Selected = true;
                                }
                            }
                        }
                        else if (EditSeat.Checked || EditPortal.Checked || EditToolTip.Checked || EditClock.Checked)
                        {
                            MapItem item = null;

                            item = Map.Instance.GetItemAt((int)(e.X / Zoom), (int)(e.Y / Zoom));

                            if (item != null && selected.Contains(item))
                            {
                                dragged = true;
                                selectedDragged = item;
                                sX = e.X;
                                sY = e.Y;
                            }
                            else
                            {
                                Deselect();
                                if (item != null)
                                {
                                    selected.Add(item);
                                    item.Selected = true;
                                }
                            }
                        }
                    }
                    if (selected.Count > 0)
                    {
                        bool clickedIn = false;
                        foreach (MapItem s in selected)
                        {
                            if (s.IsPointInArea((int)(e.X / Zoom), (int)(e.Y / Zoom)))
                            {
                                clickedIn = true;
                            }
                        }
                        if (clickedIn)
                        {
                            dragged = true;
                            sX = e.X;
                            sY = e.Y;
                            lastX = e.X;
                            lastY = e.Y;
                        }
                    }
                    if (selected.Count == 1)
                    {
                        toolStripStatusLabel1.Text = selected[0].ToString();
                    }
                }

                if (scrolling)
                {
                    scrolling = false;
                    Cursor = Cursors.Default;
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                wheelClicked = true;
                scrolling = true;
                scrollingDragged = false;
                scrollPosition = new Point(e.X - ShiftX, e.Y - ShiftY);
                if (vScrollBar1.Visible && hScrollBar1.Visible)
                {
                    Cursor = Cursors.NoMove2D;
                }
                else if (vScrollBar1.Visible)
                {
                    Cursor = Cursors.NoMoveVert;
                }
                else if (hScrollBar1.Visible)
                {
                    Cursor = Cursors.NoMoveHoriz;
                }
                else
                {
                    scrolling = false;
                }
            }
        }
        private void GraphicPanel_OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (DrawMode.Checked)
                {
                    if (!createdCurve.Close()) curves.Remove(createdCurve);
                    else
                    {
                        try
                        {
                            createdCurve.Simplify();
                        }
                        catch
                        {
                            curves.Remove(createdCurve);
                        }
                    }
                    createdCurve = null;
                }
                else
                {
                    MagnetDisable();
                    if (draggingSelection)
                    {
                        if (selectingX != e.X && selectingY != e.Y)
                        {

                            int x1 = (int)(selectingX / Zoom), x2 = (int)(e.X / Zoom);
                            int y1 = (int)(selectingY / Zoom), y2 = (int)(e.Y / Zoom);
                            if (x1 > x2)
                            {
                                x1 = (int)(e.X / Zoom);
                                x2 = (int)(selectingX / Zoom);
                            }
                            if (y1 > y2)
                            {
                                y1 = (int)(e.Y / Zoom);
                                y2 = (int)(selectingY / Zoom);
                            }
                            List<MapItem> items;
                            if (EditFH.Checked)
                            {
                                items = Map.Instance.layers[(int)Layer.Value].GetConnectionsAt(new Rectangle(x1, y1, x2 - x1, y2 - y1));
                            }
                            else
                            {
                                items = Map.Instance.layers[(int)Layer.Value].GetItemsAt(new Rectangle(x1, y1, x2 - x1, y2 - y1));
                            }

                            foreach (MapItem item in items)
                            {
                                if (selected.Contains(item) && CtrlPressed)
                                {
                                    selected.Remove(item);
                                    item.Selected = false;
                                }
                                else if (!selected.Contains(item))
                                {
                                    selected.Add(item);
                                    item.Selected = true;
                                }
                            }

                            if (selected.Count == 1 && EditObj.Checked) toolStripStatusLabel1.Text = "Z: " + selected[0].Object.GetInt("z");
                        }
                        draggingSelection = false;
                        TempChanged.Clear();
                    }
                    else
                    {
                        if (dragged && (sX != e.X || sY != e.Y))
                        {
                            undo.Push(new ActionMove(new List<MapItem>(selected), e.X - sX, e.Y - sY));
                            redo.Clear();
                        }
                        dragged = false;
                        if (selectedDragged != null)
                        {
                            if (e.X == sX && e.Y == sY)
                            {
                                Deselect();
                                selected.Add(selectedDragged);
                                selectedDragged.Selected = true;
                                if (selectedDragged2 != null)
                                {
                                    selected.Add(selectedDragged2);
                                    selectedDragged2.Selected = true;
                                }
                                if (EditObj.Checked)
                                    toolStripStatusLabel1.Text = "Z: " + selectedDragged.Object.GetInt("z");
                            }
                            selectedDragged = null;
                            selectedDragged2 = null;
                        }
                    }
                    if (EditTile.Checked) Map.Instance.layers[(int)Layer.Value].OrderTiles();
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                wheelClicked = false;
                if (scrollingDragged)
                {
                    scrolling = false;
                    Cursor = Cursors.Default;
                }
            }
        }

        private void GraphicPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (CtrlPressed)
            {
                int change = e.Delta / 120;
                ZoomLevel += change;
                if (ZoomLevel > 3) ZoomLevel = 3;
                if (ZoomLevel < -5) ZoomLevel = -5;

                switch (ZoomLevel)
                {
                    case 3: Zoom = 2; break;
                    case 2: Zoom = 3 / 2.0; break;
                    case 1: Zoom = 5 / 4.0; break;
                    case 0: Zoom = 1; break;
                    case -1: Zoom = 2 / 3.0; break;
                    case -2: Zoom = 1 / 2.0; break;
                    case -3: Zoom = 1 / 3.0; break;
                    case -4: Zoom = 1 / 4.0; break;
                    case -5: Zoom = 1 / 8.0; break;
                }

                //double maxZoomX = 8000.0 / Map.Instance.Width, maxZoomY = 8000.0 / Map.Instance.Height;
                //Zoom = Math.Min(Zoom, Math.Min(maxZoomX, maxZoomY));
                SetMapSize((int)(Map.Instance.Width * Zoom), (int)(Map.Instance.Height * Zoom));
            }
            else
            {
                if(vScrollBar1.Visible)
                {
                    int y = vScrollBar1.Value - e.Delta;
                    if (y < 0) y = 0;
                    if (y > vScrollBar1.Maximum - vScrollBar1.LargeChange) y = vScrollBar1.Maximum - vScrollBar1.LargeChange;
                    vScrollBar1.Value = y;
                }

            }
        }

        private void LoadNames(WZDirectory dir, IMGFile strings, Hashtable names)
        {
            foreach (IMGFile life in dir.IMGs.Values)
            {
                string id = int.Parse(life.Name.Substring(0, 7)).ToString();
                names.Add(id, strings.GetString(id + "/name"));
            }
        }
        private bool load()
        {
            if (file == null)
            {
                OpenFileDialog ofdOpen = new OpenFileDialog();
                ofdOpen.Filter = "Map.wz|Map.wz";
                if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                {
                    MapFileName = ofdOpen.FileName;
                    string MobFileName = ofdOpen.FileName.ToLower().Replace("map.wz", "mob.wz");
                    string NpcFileName = ofdOpen.FileName.ToLower().Replace("map.wz", "npc.wz");
                    string SoundFileName = ofdOpen.FileName.ToLower().Replace("map.wz", "sound.wz");
                    StringFileName = ofdOpen.FileName.ToLower().Replace("map.wz", "string.wz");
                    string ReactorFileName = ofdOpen.FileName.ToLower().Replace("map.wz", "reactor.wz");
                    UIFileName = ofdOpen.FileName.ToLower().Replace("map.wz", "ui.wz");
                    if (!System.IO.File.Exists(MobFileName))
                    {
                        ofdOpen.Filter = "Mob.wz|Mob.wz";
                        if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                        {
                            MobFileName = ofdOpen.FileName;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (!System.IO.File.Exists(NpcFileName))
                    {
                        NpcFileName = System.IO.Path.GetDirectoryName(ofdOpen.FileName) + "\\npc.wz";

                        if (!System.IO.File.Exists(NpcFileName))
                        {
                            ofdOpen.Filter = "Npc.wz|Npc.wz";
                            if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                            {
                                NpcFileName = ofdOpen.FileName;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    if (!System.IO.File.Exists(SoundFileName))
                    {
                        SoundFileName = System.IO.Path.GetDirectoryName(ofdOpen.FileName) + "\\sound.wz";

                        if (!System.IO.File.Exists(SoundFileName))
                        {
                            ofdOpen.Filter = "Sound.wz|Sound.wz";
                            if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                            {
                                SoundFileName = ofdOpen.FileName;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    if (!System.IO.File.Exists(StringFileName))
                    {
                        StringFileName = System.IO.Path.GetDirectoryName(ofdOpen.FileName) + "\\string.wz";

                        if (!System.IO.File.Exists(StringFileName))
                        {
                            ofdOpen.Filter = "String.wz|String.wz";
                            if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                            {
                                StringFileName = ofdOpen.FileName;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    if (!System.IO.File.Exists(ReactorFileName))
                    {
                        ReactorFileName = System.IO.Path.GetDirectoryName(ofdOpen.FileName) + "\\reactor.wz";

                        if (!System.IO.File.Exists(ReactorFileName))
                        {
                            ofdOpen.Filter = "Reactor.wz|Reactor.wz";
                            if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                            {
                                ReactorFileName = ofdOpen.FileName;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    if (!System.IO.File.Exists(UIFileName))
                    {
                        UIFileName = System.IO.Path.GetDirectoryName(ofdOpen.FileName) + "\\ui.wz";

                        if (!System.IO.File.Exists(UIFileName))
                        {
                            ofdOpen.Filter = "UI.wz|UI.wz";
                            if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                            {
                                UIFileName = ofdOpen.FileName;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    file = new WZFile(MapFileName);
                    if (MessageBox.Show("Version 0." + file.DetectVersion() + " detected, is it right?", "Version", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        GetVersion version = new GetVersion();
                        version.ShowDialog();
                        file.Version = byte.Parse(version.Version.Text);
                    }
                    file.Open();
                    mob = new WZFile(MobFileName, file.Version);
                    mob.Open();
                    npc = new WZFile(NpcFileName, file.Version);
                    npc.Open();
                    sound = new WZFile(SoundFileName, file.Version);
                    sound.Open();
                    stringf = new WZFile(StringFileName, file.Version);
                    stringf.Open();
                    reactor = new WZFile(ReactorFileName, file.Version);
                    reactor.Open();
                    ui = new WZFile(UIFileName, file.Version);
                    ui.Open();
                    LoadNames(mob.Directory, stringf.Directory.GetIMG("Mob.img"), mobNames);
                    LoadNames(npc.Directory, stringf.Directory.GetIMG("Npc.img"), npcNames);
                    //MapCurve.ProcessObjects(file.Directory.GetDirectory("Obj"));
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void openMapLoginimgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (load())
            {
                if (Map.Instance != null)
                {
                    OnMapUnload();

                }
                Map map;
                lock(UILock)
                    map = new Map(ui.Directory.GetIMG("MapLogin.img"));

                map.map.ToSave = true;

                UpdateIMGsList(true);

                MapID = "MapLogin.img";

                EditBack.Enabled = true;

                ZoomLevel = 0;
                Zoom = 1;

                SetMapSize((int)(map.Width * Zoom), (int)(map.Height * Zoom));

                GraphicPanel.Render();
            }
        }

        private void OnMapUnload()
        {
            if (Map.Instance.map.GetChild("miniMap") != null)
            {
                Bitmap bitmap = new Bitmap(Map.Instance.Width, Map.Instance.Height);
                using (Graphics g = Graphics.FromImage(bitmap))
                    Map.Instance.Draw(g);
                bitmap = ResizeBitmap(bitmap, new Size(bitmap.Width / 16, bitmap.Height / 16));

                Map.Instance.map.GetChild("miniMap").GetCanvas("canvas").SetBitmap(bitmap);
                Map.Instance.map.GetChild("miniMap").GetCanvas("canvas").format = WZCanvas.ImageFormat.FORMAT_4444;
            }
            /*
            if (Map.Instance.clock != null)
            {
                MapEditor.file.Directory.GetIMG("Obj/etc.img").Close();
            }
            foreach (MapBack back in Map.Instance.backs)
            {
                IMGFile img = back.Image.parent.parent as IMGFile;
                if (img.IsLoaded()) img.Close();
            }
            foreach (MapLife life in Map.Instance.lifes)
            {
                IMGFile img = life.Image.parent.parent as IMGFile;
                if (img.IsLoaded()) img.Close();
            }
            foreach (MapReactor reactor in Map.Instance.reactors)
            {
                IMGFile img = reactor.Image.parent.parent as IMGFile;
                if (img.IsLoaded()) img.Close();
            }
            foreach (MapToolTip tt in Map.Instance.tooltips)
            {
                IMGFile img = tt.Image.parent.parent.parent as IMGFile;
                if (img.IsLoaded()) img.Close();
            }
            for (int i = 0; i < 8; i++)
            {
                foreach (MapObject obj in Map.Instance.layers[i].objects)
                {
                    IMGFile img = obj.Image.parent.parent.parent.parent as IMGFile;
                    if (img.IsLoaded()) img.Close();
                }
                string tileStyle = Map.Instance.layers[i].info.GetString("tS");
                if (tileStyle != "")
                {
                    IMGFile img = MapEditor.file.Directory.GetIMG("Tile/" + tileStyle + ".img");
                    if (img.IsLoaded()) img.Close();
                }
            }*/
        }

        private void Open_Click(object sender, EventArgs e)
        {
            if (load())
            {
                MapSelect select = new MapSelect();
                select.ShowDialog();

                if (select.Result == "")
                {
                    return;
                }
                if (Map.Instance != null)
                {
                    OnMapUnload();
                }

                MapID = select.Result.Substring(9, 9);
                Map map;
                lock(MapLock)
                    map = new Map(file.Directory.GetIMG(select.Result));

                map.map.ToSave = true;

                UpdateIMGsList(true);

                ZoomLevel = 0;
                Zoom = 1;

                SetMapSize((int)(map.Width * Zoom), (int)(map.Height * Zoom));

                GraphicPanel.Render();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            splitContainer1.Size = new Size(Size.Width - 40, Size.Height - 116);
            //splitContainer1.SplitterDistance = Math.Max(splitContainer1.Width - 259, 0);

            panel1.Height = splitContainer1.Height - 8;
            TilesPanel.Height = splitContainer1.Height - 8;
            ObjectsPanel.Height = splitContainer1.Height - 8;
            panel1.Width = splitContainer1.Panel2.Width - 6;
            TilesPanel.Width = splitContainer1.Panel2.Width - 6;
            ObjectsPanel.Width = splitContainer1.Panel2.Width - 6;

            RandomTiles.Location = new Point(RandomTiles.Location.X, TilesPanel.Height - 20);
            panel2.Location = new Point(RandomTiles.Location.X, panel1.Height - 221);

            TilesList.Height = TilesPanel.Height - 93;
            ObjectsList.Height = ObjectsPanel.Height - 143;
            ItemsList.Height = panel1.Height - 293;
            TilesList.Width = TilesPanel.Width - 6;
            ObjectsList.Width = ObjectsPanel.Width - 6;

            vScrollBar1.Visible = false;
            hScrollBar1.Visible = false;
            if (hScrollBar1.Maximum > splitContainer1.SplitterDistance - 2) hScrollBar1.Visible = true;
            if (vScrollBar1.Maximum > splitContainer1.Height - 2) vScrollBar1.Visible = true;
            if (hScrollBar1.Maximum > splitContainer1.SplitterDistance - (vScrollBar1.Visible ? 19 : 0)) hScrollBar1.Visible = true;
            if (vScrollBar1.Maximum > splitContainer1.Height - (hScrollBar1.Visible ? 18 : 0)) vScrollBar1.Visible = true;

            if (vScrollBar1.Visible)
            {
                vScrollBar1.Location = new Point(splitContainer1.SplitterDistance - 19, 0);
                vScrollBar1.Height = splitContainer1.Height - (hScrollBar1.Visible ? 18 : 2);
                vScrollBar1.LargeChange = vScrollBar1.Height;
                ScreenHeight = vScrollBar1.Height;
            }
            else
            {
                ScreenHeight = GraphicPanel.Height;
                ShiftY = 0;
            }
            if (hScrollBar1.Visible)
            {
                hScrollBar1.Location = new Point(0, splitContainer1.Height - 18);
                hScrollBar1.Width = splitContainer1.SplitterDistance - (vScrollBar1.Visible ? 19 : 2);
                hScrollBar1.LargeChange = hScrollBar1.Width;
                ScreenWidth = vScrollBar1.Width;
            }
            else
            {
                ScreenWidth = GraphicPanel.Width;
                ShiftX = 0;
            }
            if (hScrollBar1.Visible && vScrollBar1.Visible)
            {
                panel3.Visible = true;
                panel3.Location = new Point(hScrollBar1.Width, vScrollBar1.Height);
            }
            else panel3.Visible = false;
        }
        

        private void SetMapSize(int width, int height)
        {
            vScrollBar1.Maximum = height;
            hScrollBar1.Maximum = width;

            GraphicPanel.Width = Math.Min(width, SystemInformation.PrimaryMonitorSize.Width);
            GraphicPanel.Height = Math.Min(height, SystemInformation.PrimaryMonitorSize.Height);

            Form1_Resize(null, null);

            hScrollBar1.Value = Math.Max(0, Math.Min(hScrollBar1.Value, hScrollBar1.Maximum - hScrollBar1.LargeChange));
            vScrollBar1.Value = Math.Max(0, Math.Min(vScrollBar1.Value, vScrollBar1.Maximum - vScrollBar1.LargeChange));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open_Click(sender, e);
        }

        public void OnResetDevice(object sender, DeviceEventArgs e)
        {
            Device device = e.Device;
        }

        private void GraphicPanel_OnRender(object sender, DeviceEventArgs e)
        {
            if (Map.Instance != null)
            {
                Map.Instance.Draw(GraphicPanel);
            }
            if (scrolling)
            {
                if (vScrollBar1.Visible && hScrollBar1.Visible) GraphicPanel.Draw2DCursor(scrollPosition.X, scrollPosition.Y);
                else if (vScrollBar1.Visible) GraphicPanel.DrawVertCursor(scrollPosition.X, scrollPosition.Y);
                else if (hScrollBar1.Visible) GraphicPanel.DrawHorizCursor(scrollPosition.X, scrollPosition.Y);
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GraphicPanel.Init(this);
        }


        private void ShowBack_Click(object sender, EventArgs e)
        {
            if (ShowBack.Checked)
            {
                ShowBack.Checked = false;
                ShowBack.ToolTipText = "Show Background";
            }
            else
            {
                ShowBack.Checked = true;
                ShowBack.ToolTipText = "Hide Background";
            }
        }

        private void ShowTile_Click(object sender, EventArgs e)
        {
            if (ShowTile.Checked)
            {
                ShowTile.Checked = false;
                ShowTile.ToolTipText = "Show Tiles";
            }
            else
            {
                ShowTile.Checked = true;
                ShowTile.ToolTipText = "Hide Tiles";
            }
        }
        private void ShowObj_Click(object sender, EventArgs e)
        {
            if (ShowObj.Checked)
            {
                ShowObj.Checked = false;
                ShowObj.ToolTipText = "Show Tiles";
            }
            else
            {
                ShowObj.Checked = true;
                ShowObj.ToolTipText = "Hide Tiles";
            }
        }
        private void ShowLife_Click(object sender, EventArgs e)
        {
            if (ShowLife.Checked)
            {
                ShowLife.Checked = false;
                ShowLife.ToolTipText = "Show Lifes";
            }
            else
            {
                ShowLife.Checked = true;
                ShowLife.ToolTipText = "Hide Lifes";
            }
        }
        private void ShowFH_Click(object sender, EventArgs e)
        {
            if (ShowFH.Checked)
            {
                ShowFH.Checked = false;
                ShowFH.ToolTipText = "Show Footholds";
            }
            else
            {
                ShowFH.Checked = true;
                ShowFH.ToolTipText = "Hide Footholds";
            }
        }
        private void ShowLR_Click(object sender, EventArgs e)
        {
            if (ShowLR.Checked)
            {
                ShowLR.Checked = false;
                ShowLR.ToolTipText = "Show Ladders/Ropes";
            }
            else
            {
                ShowLR.Checked = true;
                ShowLR.ToolTipText = "Hide Ladders/Ropes";
            }
        }
        private void ShowSeat_Click(object sender, EventArgs e)
        {
            if (ShowSeat.Checked)
            {
                ShowSeat.Checked = false;
                ShowSeat.ToolTipText = "Show Seats";
            }
            else
            {
                ShowSeat.Checked = true;
                ShowSeat.ToolTipText = "Hide Seats";
            }
        }
        private void ShowPortal_Click(object sender, EventArgs e)
        {
            if (ShowPortal.Checked)
            {
                ShowPortal.Checked = false;
                ShowPortal.ToolTipText = "Show Portals";
            }
            else
            {
                ShowPortal.Checked = true;
                ShowPortal.ToolTipText = "Hide Portals";
            }
        }
        private void ShowReactor_Click(object sender, EventArgs e)
        {
            if (ShowReactor.Checked)
            {
                ShowReactor.Checked = false;
                ShowReactor.ToolTipText = "Show Reactors";
            }
            else
            {
                ShowReactor.Checked = true;
                ShowReactor.ToolTipText = "Hide Reactors";
            }
        }

        private void ShowToolTip_Click(object sender, EventArgs e)
        {
            if (ShowToolTip.Checked)
            {
                ShowToolTip.Checked = false;
                ShowToolTip.ToolTipText = "Show ToolTips";
            }
            else
            {
                ShowToolTip.Checked = true;
                ShowToolTip.ToolTipText = "Hide ToolTips";
            }
        }


        private void ShowClock_Click(object sender, EventArgs e)
        {
            if (ShowClock.Checked)
            {
                ShowClock.Checked = false;
                ShowClock.ToolTipText = "Show Clock";
            }
            else
            {
                ShowClock.Checked = true;
                ShowClock.ToolTipText = "Hide Clock";
            }
        }

        public void Deselect()
        {
            foreach (MapItem s in selected)
            {
                s.Selected = false;
            }
            MagnetDisable();
            selected.Clear();
            toolStripStatusLabel1.Text = "";
            if (EditTile.Checked) Map.Instance.layers[(int)Layer.Value].OrderTiles();
        }

        private void EditMode_Click(object sender, EventArgs e)
        {
            if (EditMode.Checked)
            {
                Deselect();
                EditMode.Checked = false;

                EditTile.Enabled = false;
                EditObj.Enabled = false;
                EditLife.Enabled = false;
                EditFH.Enabled = false;
                EditLR.Enabled = false;
                EditSeat.Enabled = false;
                EditPortal.Enabled = false;
                EditToolTip.Enabled = false;
                EditClock.Enabled = false;

                Layer.Enabled = false;
                EditReactor.Enabled = false;
            }
            else
            {
                EditMode.Checked = true;

                EditTile.Enabled = true;
                EditObj.Enabled = true;
                EditLife.Enabled = true;
                EditFH.Enabled = true;
                EditLR.Enabled = true;
                EditSeat.Enabled = true;
                EditPortal.Enabled = true;
                EditReactor.Enabled = true;
                EditToolTip.Enabled = true;
                EditClock.Enabled = true;

                Layer.Enabled = true;
            }
            UpdateIMGsList(true);
        }


        private void DrawMode_Click(object sender, EventArgs e)
        {
            /*if (EditMode.Checked)
            {
                EditMode_Click(null, null);
            }*/
            DrawMode.Checked = true;
        }

        private List<String> GetObjectsIMGs()
        {
            if (ObjectsIMGs != null) return ObjectsIMGs;

            ObjectsIMGs = new List<String>();

            WZDirectory Objects = MapEditor.file.Directory.GetDirectory("Obj");
            foreach (IMGFile img in Objects.IMGs.Values)
            {
                ObjectsIMGs.Add(img.Name);
            }

            return ObjectsIMGs;
        }

        private void UpdateIMGsList(bool ChangeMode)
        {
            UpdateIMGsList(ChangeMode, false);
        }
        private void UpdateIMGsList(bool ChangeMode, bool DontUpdate)
        {
            if (EditMode.Checked)
            {
                if (EditObj.Checked)
                {
                    if (ChangeMode)
                    {
                        ObjectsPanel.Visible = true;
                        panel1.Visible = false;
                        TilesPanel.Visible = false;
                        ObjectsFolders.Items.Clear();
                        ObjectsFolders.SelectedIndex = -1;
                        SubObjectsList.Items.Clear();
                        SubObjectsList.SelectedIndex = -1;
                        GraphicPanel.Focus();
                        ObjectsFolders.Items.AddRange(GetObjectsIMGs().ToArray());
                        UpdateObjectsList();
                        /*
                        IMGsList.Items.Clear();
                        IMGsList.SelectedIndex = -1;
                        IMGsList.Visible = true;
                        IMGsList.Items.AddRange(GetObjectsIMGs().ToArray());
                        IMGsList.Text = "";
                        IMGsList.Enabled = true;
                        ItemsList.Items.Clear();
                        ItemsList.SelectedIndex = -1;
                        ItemsList.Visible = true;
                        ItemPreview.Image = null;*/
                    }
                }
                else if (EditTile.Checked)
                {
                    if (ChangeMode)
                    {
                        panel1.Visible = false;
                        TilesPanel.Visible = true;
                        ObjectsPanel.Visible = false;
                        GraphicPanel.Focus();
                    }
                    string tS = Map.Instance.layers[(int)Layer.Value].info.GetString("tS");
                    if (tS == "")
                    {
                        TileView.Image = null;
                        TilesList.Controls.Clear();
                    }
                    else
                    {
                        lock (MapLock)
                        {
                            if (!DontUpdate) UpdateTilesList();
                            TileView.Image = file.Directory.GetIMG("Tile/" + tS + ".img").GetCanvas("enH0/0").GetBitmap();
                        }
                    }
                }
                else if (EditLife.Checked)
                {
                    if (ChangeMode)
                    {
                        panel1.Visible = true;
                        TilesPanel.Visible = false;
                        ObjectsPanel.Visible = false;
                        GraphicPanel.Focus();
                        IMGsList.Items.Clear();
                        IMGsList.SelectedIndex = -1;
                        IMGsList.Visible = true;
                        IMGsList.Items.Add("Mob.wz");
                        IMGsList.Items.Add("Npc.wz");
                        IMGsList.Text = "";
                        IMGsList.Enabled = true;
                        ItemsList.Items.Clear();
                        ItemsList.SelectedIndex = -1;
                        ItemsList.Visible = true;
                        Search.Visible = true;
                        Search.Text = "";
                        label1.Visible = true;
                        ItemPreview.Image = null;
                    }
                }
                else if (EditReactor.Checked)
                {
                    if (ChangeMode)
                    {
                        panel1.Visible = true;
                        TilesPanel.Visible = false;
                        ObjectsPanel.Visible = false;
                        GraphicPanel.Focus();
                        IMGsList.Items.Clear();
                        IMGsList.SelectedIndex = -1;
                        IMGsList.Visible = true;
                        IMGsList.Items.Add("Reactor.wz");
                        IMGsList.Text = "";
                        IMGsList.Enabled = true;
                        ItemsList.Items.Clear();
                        ItemsList.SelectedIndex = -1;
                        ItemsList.Visible = true;
                        Search.Visible = true;
                        Search.Text = "";
                        label1.Visible = true;
                        ItemPreview.Image = null;
                    }
                }
                else
                {
                    ObjectsPanel.Visible = false;
                    panel1.Visible = false;
                    TilesPanel.Visible = false;
                }
                if ((EditObj.Checked || EditTile.Checked) && selected.Count > 0)
                {
                    LinkToFH.Enabled = true;
                }
                else
                {
                    LinkToFH.Enabled = false;
                    if (LinkToFH.Checked) LinkToFH_Click(null, null);
                }
                if (EditFH.Checked || EditSeat.Checked)
                {
                    AutoConnect.Enabled = true;
                }
                else
                {
                    AutoConnect.Enabled = false;
                }
                if (EditFH.Checked || EditLR.Checked || EditSeat.Checked || EditPortal.Checked || EditToolTip.Checked || EditClock.Checked)
                {
                    AddLine.Enabled = true;
                }
                else
                {
                    AddLine.Enabled = false;
                }
                if (EditFH.Checked || EditLR.Checked)
                {
                    FootholdsCreator.Enabled = true;
                    if (FootholdsCreator.Checked) FootholdsCreator_Click(null, null);

                }
                else
                {
                    FootholdsCreator.Enabled = false;
                }
            }
            else
            {
                panel1.Visible = false;
                TilesPanel.Visible = false;
                ObjectsPanel.Visible = false;
                AddLine.Enabled = false;
                AutoConnect.Enabled = false;
                LinkToFH.Enabled = false;
                if (LinkToFH.Checked) LinkToFH_Click(null, null);
            }
        }

        private void UpdateObjectsList()
        {
            ObjectsList.Controls.Clear();
            IMGFile objects = file.Directory.GetIMG("Obj/" + (string)ObjectsFolders.SelectedItem);
            SelectedIMG = objects;
            if (objects != null)
            {
                IMGEntry e1 = objects.GetChild((string)SubObjectsList.SelectedItem);
                if(e1 != null)
                {
                    string n1 = e1.Name;
                    foreach (IMGEntry e2 in e1.childs.Values)
                    {
                        string n2 = e2.Name;
                        foreach (IMGEntry e3 in e2.childs.Values)
                        {
                            string name = n1 + "/" + n2 + "/" + e3.Name;
                            if (e3.GetCanvas("0") != null)
                            {
                                ImageViewer imageViewer = ObjectsList.Add(e3.GetCanvas("0").GetBitmap(), name, true);
                                imageViewer.MouseClick += new MouseEventHandler(ImageViewer_MouseClick);
                                imageViewer.MouseDoubleClick += new MouseEventHandler(ImageViewer_MouseDoubleClick);
                                imageViewer.MaxWidth = 100;
                                imageViewer.MaxHeight = 100;
                                float mp = 1;
                                float dx = 0, dy = 0;
                                if (imageViewer.Width - 8 > 100)
                                {
                                    dx = 100f / (imageViewer.Width - 8);
                                }
                                if (imageViewer.Height - 20 > 100)
                                {
                                    dy = 100f / (imageViewer.Height - 20);
                                }

                                if (dx != 0 && dy == 0) mp = dx;
                                if (dy != 0 && dx == 0) mp = dy;
                                if (dx != 0 && dx != 0) mp = Math.Min(dx, dy);
                                imageViewer.Width = (int)((imageViewer.Width - 8) * mp + 8);
                                imageViewer.Height = (int)((imageViewer.Height - 20) * mp + 20);
                            }
                        }
                    }
                }
            }
        }
        private void UpdateTilesList()
        {
            TilesList.Controls.Clear();
            IMGFile tiles = file.Directory.GetIMG("Tile/" + Map.Instance.layers[(int)Layer.Value].info.GetString("tS") + ".img");
            if (RandomTiles.Checked)
            {
                for (int i=0; i<tiles.childs.Count; i++)
                {
                    if (tiles.childs[i].Name != "info")
                    {
                        ImageViewer imageViewer = TilesList.Add(tiles.childs[i].GetCanvas("0").GetBitmap(), tiles.childs[i].Name, false);
                        imageViewer.MouseClick += new MouseEventHandler(ImageViewer_MouseClick);
                        imageViewer.MouseDoubleClick += new MouseEventHandler(ImageViewer_MouseDoubleClick);
                    }
                }
            }
            else
            {
                for (int i = 0; i < tiles.childs.Count; i++)
                {
                    if (tiles.childs[i].Name != "info")
                    {
                        for (int j = 0; j < tiles.childs[i].childs.Count; j++)
                        {
                            ImageViewer imageViewer = TilesList.Add(tiles.childs[i].childs[j].GetCanvas().GetBitmap(), tiles.childs[i].Name + "/" + tiles.childs[i].childs[j].Name, false);
                            imageViewer.MouseClick += new MouseEventHandler(ImageViewer_MouseClick);
                            imageViewer.MouseDoubleClick += new MouseEventHandler(ImageViewer_MouseDoubleClick);
                        }
                    }
                }
            }
        }

        private void ImageViewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (m_ActiveImageViewer != null)
            {
                m_ActiveImageViewer.IsActive = false;
            }

            m_ActiveImageViewer = (ImageViewer)sender;
            m_ActiveImageViewer.IsActive = true;
        }
        private void ImageViewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            lock (MapLock)
            {
                if (EditMode.Checked)
                {
                    Point pos = new Point((int)(ShiftX / Zoom), (int)(ShiftY / Zoom));
                    pos.X += Math.Min(splitContainer1.SplitterDistance, GraphicPanel.Width) / 2 - Map.Instance.CenterX;
                    pos.Y += Math.Min(splitContainer1.Height, GraphicPanel.Height) / 2 - Map.Instance.CenterY;
                    if (EditTile.Checked)
                    {
                        IMGEntry entry = new IMGEntry();
                        string tile = m_ActiveImageViewer.Name;
                        IMGFile img = MapEditor.file.Directory.GetIMG("Tile/" + Map.Instance.layers[(int)Layer.Value].info.GetString("tS") + ".img");
                        if (RandomTiles.Checked)
                        {
                            tile += "/" + random.Next(img.GetChild(tile).childs.Count).ToString();
                        }
                        entry.SetInt("x", pos.X);
                        entry.SetInt("y", pos.Y);
                        entry.SetString("u", tile.Substring(0, tile.IndexOf("/")));
                        entry.SetInt("no", int.Parse(tile.Substring(tile.IndexOf("/") + 1)));
                        entry.SetInt("zM", 0);
                        MapTile t = new MapTile();
                        t.Object = entry;
                        t.Image = img.GetChild(tile);
                        t.CreateFootholdDesignList();
                        t.SetDesign(entry.GetString("u"));
                        Map.Instance.layers[(int)Layer.Value].Add(t);
                        undo.Push(new ActionAdd(t, (int)Layer.Value));
                        redo.Clear();
                        Deselect();
                        selected.Add(t);
                        t.Selected = true;
                        Map.Instance.layers[(int)Layer.Value].OrderTiles();
                    }
                    else if (EditObj.Checked)
                    {
                        IMGEntry entry = new IMGEntry();
                        string img = (string)ObjectsFolders.SelectedItem;
                        string obj = m_ActiveImageViewer.Name;
                        entry.SetString("oS", img.Substring(0, img.IndexOf(".")));
                        entry.SetString("l0", obj.Substring(0, obj.IndexOf("/")));
                        int f1 = obj.IndexOf("/") + 1;
                        entry.SetString("l1", obj.Substring(f1, obj.IndexOf("/", f1) - f1));
                        int f2 = obj.IndexOf("/", f1) + 1;
                        entry.SetString("l2", obj.Substring(f2));
                        entry.SetInt("x", pos.X);
                        entry.SetInt("y", pos.Y);
                        entry.SetInt("z", 0);
                        entry.SetInt("f", 0);
                        entry.SetInt("zM", 0);
                        MapObject o = new MapObject();
                        o.Object = entry;
                        o.Image = Map.GetRealImage(MapEditor.file.Directory.GetIMG("Obj/" + img).GetChild(obj + "/0"));
                        o.CreateFootholdDesignList();
                        Map.Instance.layers[(int)Layer.Value].Add(o);
                        undo.Push(new ActionAdd(o, (int)Layer.Value));
                        redo.Clear();
                        Deselect();
                        selected.Add(o);
                        o.Selected = true;
                    }
                    GraphicPanel.Focus();
                }
            }
        }

        private void EditTile_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditTile.Checked)
            {
                EditTile.Checked = false;
            }
            else
            {
                EditTile.Checked = true;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }
        private void EditObj_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditObj.Checked)
            {
                EditObj.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = true;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }
        private void EditLife_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditLife.Checked)
            {
                EditLife.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = true;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }
        private void EditFH_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditFH.Checked)
            {
                EditFH.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = true;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }
        private void EditLR_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditLR.Checked)
            {
                EditLR.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = true;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }
        private void EditSeat_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditSeat.Checked)
            {
                EditSeat.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = true;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }
        private void EditPortal_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditPortal.Checked)
            {
                EditPortal.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = true;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }

        private void EditBack_Click(object sender, EventArgs e)
        {
            if (EditMode.Checked)
            {
                Deselect();
                if (EditBack.Checked)
                {
                    EditBack.Checked = false;
                }
                else
                {
                    EditTile.Checked = false;
                    EditObj.Checked = false;
                    EditLife.Checked = false;
                    EditFH.Checked = false;
                    EditLR.Checked = false;
                    EditSeat.Checked = false;
                    EditPortal.Checked = false;
                    EditReactor.Checked = false;
                    EditToolTip.Checked = false;
                    EditClock.Checked = false;
                    EditBack.Checked = true;
                }
                UpdateIMGsList(true);
            }
        }
        private void EditReactor_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditReactor.Checked)
            {
                EditReactor.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = true;
                EditToolTip.Checked = false;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }

        private void EditToolTip_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditToolTip.Checked)
            {
                EditToolTip.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = true;
                EditClock.Checked = false;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }

        private void EditClock_Click(object sender, EventArgs e)
        {
            Deselect();
            if (EditClock.Checked)
            {
                EditClock.Checked = false;
            }
            else
            {
                EditTile.Checked = false;
                EditObj.Checked = false;
                EditLife.Checked = false;
                EditFH.Checked = false;
                EditLR.Checked = false;
                EditSeat.Checked = false;
                EditPortal.Checked = false;
                EditReactor.Checked = false;
                EditToolTip.Checked = false;
                EditClock.Checked = true;
                EditBack.Checked = false;
            }
            UpdateIMGsList(true);
        }

        private void LinkToFH_Click(object sender, EventArgs e)
        {
            if (LinkToFH.Checked)
            {
                LinkToFH.Checked = false;
                Layer.Enabled = true;
                EditMode.Enabled = true;
                EditTile.Enabled = true;
                EditObj.Enabled = true;
                EditLife.Enabled = true;
                EditFH.Enabled = true;
                EditLR.Enabled = true;
                EditSeat.Enabled = true;
                EditPortal.Enabled = true;
                EditReactor.Enabled = true;
                EditToolTip.Enabled = true;
                EditClock.Enabled = true;
                Open.Enabled = true;
                if (selectedGroup != null)
                {
                    selectedGroup.Selected = false;
                    selectedGroup = null;
                }
            }
            else
            {
                if(selected.Count > 0){
                    Layer.Enabled = false;
                    LinkToFH.Checked = true;
                    EditMode.Enabled = false;
                    EditTile.Enabled = false;
                    EditObj.Enabled = false;
                    EditLife.Enabled = false;
                    EditFH.Enabled = false;
                    EditLR.Enabled = false;
                    EditSeat.Enabled = false;
                    EditPortal.Enabled = false;
                    EditReactor.Enabled = false;
                    EditToolTip.Enabled = false;
                    EditClock.Enabled = false;
                    Open.Enabled = false;
                    if (EditObj.Checked || EditTile.Checked)
                    {
                        int zM = -1;
                        foreach (MapItem s in selected)
                        {
                            int nzM = s.Object.GetInt("zM");
                            if (zM != -1 && zM != nzM)
                            {
                                zM = -1;
                                break;
                            }
                            if (zM == -1)
                            {
                                zM = nzM;
                            }
                        }
                        if (zM != -1 && Map.Instance.layers[(int)Layer.Value].footholdGroups.Contains(zM))
                        {
                            selectedGroup = (MapFootholds)Map.Instance.layers[(int)Layer.Value].footholdGroups[zM];
                            selectedGroup.Selected = true;
                        }
                    }
                }
            }
        }

        private void FootholdsCreator_Click(object sender, EventArgs e)
        {
            if (FootholdsCreator.Checked)
            {
                FootholdsCreator.Checked = false;
                Layer.Enabled = true;
                EditMode.Enabled = true;
                EditTile.Enabled = true;
                EditObj.Enabled = true;
                EditLife.Enabled = true;
                EditFH.Enabled = true;
                EditLR.Enabled = true;
                EditSeat.Enabled = true;
                EditPortal.Enabled = true;
                EditReactor.Enabled = true;
                EditToolTip.Enabled = true;
                EditClock.Enabled = true;
                Open.Enabled = true;
            }
            else
            {
                Layer.Enabled = false;
                FootholdsCreator.Checked = true;
                EditMode.Enabled = false;
                EditTile.Enabled = false;
                EditObj.Enabled = false;
                EditLife.Enabled = false;
                EditFH.Enabled = false;
                EditLR.Enabled = false;
                EditSeat.Enabled = false;
                EditPortal.Enabled = false;
                EditReactor.Enabled = false;
                EditToolTip.Enabled = false;
                EditClock.Enabled = false;
                Open.Enabled = false;
            }
        }

        private void Layer_ValueChanged(object sender, EventArgs e)
        {
            if (selected != null)
            {
                if (EditObj.Checked)
                {
                    foreach (MapItem s in selected)
                    {
                        Map.Instance.layers[layerLastValue].Delete(s);
                        Map.Instance.layers[(int)Layer.Value].Add(s);
                    }
                    if (selected.Count > 0)
                    {
                        undo.Push(new ActionChangeLayer(new List<MapItem>(selected), layerLastValue, (int)Layer.Value));
                        redo.Clear();
                    }
                }
                else if (EditTile.Checked)
                {
                    Deselect();
                    UpdateIMGsList(true);
                }
            }
            layerLastValue = (int)Layer.Value;
            if (sender != null)
            {
                GraphicPanel.Focus();
            }
            UpdateIMGsList(false);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (Map.Instance == null) return;

            //Map.Instance.map.GetChild("info").SetInt("fieldLimit", 0);

            if (Map.Instance.map.GetChild("miniMap") != null)
            {
                Bitmap bitmap = new Bitmap(Map.Instance.Width, Map.Instance.Height);
                using (Graphics g = Graphics.FromImage(bitmap))
                    Map.Instance.Draw(g);
                bitmap = ResizeBitmap(bitmap, new Size(bitmap.Width / 16, bitmap.Height / 16));

                Map.Instance.map.GetChild("miniMap").GetCanvas("canvas").SetBitmap(bitmap);
                Map.Instance.map.GetChild("miniMap").GetCanvas("canvas").format = WZCanvas.ImageFormat.FORMAT_4444;
            }

            if (MapID == "MapLogin.img")
            {
                ui.Save();
                ui.Close();
                ui = new WZFile(UIFileName, file.Version);
                ui.Open();

                new Map(ui.Directory.GetIMG("MapLogin.img")).map.ToSave = true;
            }
            else
            {
                string IMG = ((IMGFile)Map.Instance.map).GetFullName();

                file.Save();
                stringf.Save();
                file.Close();
                file = new WZFile(MapFileName, file.Version);
                file.Open();
                stringf.Close();
                stringf = new WZFile(StringFileName, file.Version);
                stringf.Open();

                lock(MapLock)
                    new Map(file.Directory.GetIMG(IMG.Substring(4))).map.ToSave = true;
            }
        }

        private static Bitmap ResizeBitmap(Bitmap imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            /*float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);*/
            int destWidth = size.Width;
            int destHeight = size.Height;

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            imgToResize.Dispose();
            return b;
        }

        private void RenderMinimap_Click(object sender, EventArgs e)
        {
            if (RenderMinimap.Checked)
            {
                RenderMinimap.Checked = false;
            }
            else
            {
                RenderMinimap.Checked = true;
            }
        }

        private void ShowLimits_Click(object sender, EventArgs e)
        {
            if (ShowLimits.Checked)
            {
                ShowLimits.Checked = false;
            }
            else
            {
                ShowLimits.Checked = true;
            }
        }
        private void MagnetMode_Click(object sender, EventArgs e)
        {
            if (MagnetMode.Checked)
            {
                MagnetMode.Checked = false;
                if (EditTile.Checked && selected.Count == 1)
                {
                    ((MapTile)selected[0]).Magnet = false;
                }
                else if (EditLife.Checked && selected.Count == 1)
                {
                    ((MapLife)selected[0]).Magnet = false;
                }
                /*else if (EditObj.Checked && selected.Count == 1)
                {
                    ((MapObject)selected[0]).Magnet = false;
                }*/
            }
            else
            {
                MagnetMode.Checked = true;
            }
        }

        private void ShowAll()
        {
            ItemsList.Items.Clear();
            if (IMGsList.SelectedIndex == -1) return;
            if (EditMode.Checked)
            {
                if (EditLife.Checked)
                {
                    ItemsList.ClearSelected();
                    ItemsList.Items.Clear();

                    string type = (string)IMGsList.SelectedItem;
                    List<string> names = new List<string>();
                    WZDirectory Lifes = null;
                    if (type == "Mob.wz")
                    {
                        Lifes = mob.Directory;
                    }
                    else if (type == "Npc.wz")
                    {
                        Lifes = npc.Directory;
                    }

                    foreach (IMGFile img in Lifes.IMGs.Values)
                    {
                        string id = int.Parse(img.Name.Substring(0, 7)).ToString();
                        names.Add(img.Name + " - " + ((type == "Mob.wz") ? (string)mobNames[id] : (string)npcNames[id]));
                    }

                    names.Sort();

                    ItemsList.Items.AddRange(names.ToArray());
                }
                else if (EditReactor.Checked)
                {
                    ItemsList.ClearSelected();
                    ItemsList.Items.Clear();

                    string type = (string)IMGsList.SelectedItem;
                    List<string> names = new List<string>();
                    WZDirectory Reactors = reactor.Directory;

                    foreach (IMGFile img in Reactors.IMGs.Values)
                    {
                        names.Add(img.Name);
                    }

                    names.Sort();

                    ItemsList.Items.AddRange(names.ToArray());
                }
            }
        }

        private List<string> GetListByName(string sName, Hashtable names)
        {
            List<string> list1 = new List<string>();
            Hashtable list2 = new Hashtable();
            foreach (DictionaryEntry name in names)
            {
                if (((string)name.Value).ToLower() == sName)
                {
                    list1.Add(((string)name.Key).PadLeft(7, '0') + ".img - " + (string)name.Value);
                }
                else if (((string)name.Value).ToLower().IndexOf(sName) != -1)
                {
                    //list2.Add((string)name.Key, (((string)name.Value).ToLower().IndexOf(sName) == 0) ? 0 : 1);
                    list2.Add((string)name.Key, ((string)name.Value).ToLower().IndexOf(sName) + ((string)name.Value).ToLower().Replace(sName, "").Length);
                }
            }

            list1.Sort();
            List<string> list3 = new List<string>(list2.Keys.Cast<string>());
            list3.Sort();
            list3 = list3.OrderBy(s => names[s]).ToList<string>();
            list3 = list3.OrderBy(s => list2[s]).ToList<string>();
            List<string> list4 = new List<string>();
            foreach (string id in list3)
            {
                list4.Add(id.PadLeft(7, '0') + ".img - " + names[id]);
            }
            list1.AddRange(list4);
            return list1;
        }

        private void ShowByID(string ID)
        {
            ItemsList.Items.Clear();
            if (IMGsList.SelectedIndex == -1) return;
            if (EditMode.Checked)
            {
                if (EditLife.Checked)
                {
                    ItemsList.ClearSelected();
                    ItemsList.Items.Clear();

                    string type = (string)IMGsList.SelectedItem;
                    List<string> names = new List<string>();
                    WZDirectory Lifes = null;
                    if (type == "Mob.wz")
                    {
                        Lifes = mob.Directory;
                    }
                    else if (type == "Npc.wz")
                    {
                        Lifes = npc.Directory;
                    }

                    foreach (IMGFile img in Lifes.IMGs.Values)
                    {
                        string id = int.Parse(img.Name.Substring(0, 7)).ToString();
                        if (MapSelect.IDMatch(id, ID))
                        {
                            names.Add(img.Name + " - " + ((type == "Mob.wz") ? (string)mobNames[id] : (string)npcNames[id]));
                        }
                    }

                    names.Sort();

                    ItemsList.Items.AddRange(names.ToArray());
                }
                else if (EditReactor.Checked)
                {
                    ItemsList.ClearSelected();
                    ItemsList.Items.Clear();

                    string type = (string)IMGsList.SelectedItem;
                    List<string> names = new List<string>();
                    WZDirectory Reactors = reactor.Directory;

                    foreach (IMGFile img in Reactors.IMGs.Values)
                    {
                        if (MapSelect.IDMatch(int.Parse(img.Name.Substring(0, 7)).ToString(), ID))
                        {
                            names.Add(img.Name);
                        }
                    }

                    names.Sort();

                    ItemsList.Items.AddRange(names.ToArray());
                }
            }
        }

        private void ShowByName(string sName)
        {
            ItemsList.Items.Clear();
            if (IMGsList.SelectedIndex == -1) return;
            if (EditMode.Checked)
            {
                if (EditLife.Checked)
                {
                    ItemsList.ClearSelected();
                    ItemsList.Items.Clear();

                    string type = (string)IMGsList.SelectedItem;

                    Hashtable names = null;
                    if (type == "Mob.wz")
                    {
                        names = mobNames;
                    }
                    else if (type == "Npc.wz")
                    {
                        names = npcNames;
                    }

                    ItemsList.Items.AddRange(GetListByName(sName, names).ToArray());
                }
            }
        }

        private void IMGsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowAll();
        }

        private void ItemsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemsList.SelectedItem == null)
            {
                ItemPreview.Image = null;
            }
            else if (EditMode.Checked)
            {
                if (EditLife.Checked)
                {
                    IMGEntry preview = null;
                    string type = (string)IMGsList.SelectedItem;
                    lock ((type == "Npc.wz") ? NpcLock : MobLock)
                    {
                        if (type == "Npc.wz")
                        {
                            IMGFile npc = MapEditor.npc.Directory.GetIMG(((String)ItemsList.SelectedItem).Substring(0, 11));
                            if (npc.GetChild("info/link") != null)
                            {
                                npc = MapEditor.npc.Directory.GetIMG(npc.GetString("info/link") + ".img");
                            }
                            preview = npc.GetChild("stand/0");

                        }
                        else if (type == "Mob.wz")
                        {
                            IMGFile mob = MapEditor.mob.Directory.GetIMG(((String)ItemsList.SelectedItem).Substring(0, 11));
                            if (mob.GetChild("info/link") != null)
                            {
                                mob = MapEditor.mob.Directory.GetIMG(mob.GetString("info/link") + ".img");
                            }
                            preview = mob.GetChild("stand/0");
                            if (preview == null) preview = mob.GetChild("fly/0");
                        }
                        ItemPreview.Image = Map.GetRealImage(preview).GetCanvas().GetBitmap();
                    }
                }
                else if (EditReactor.Checked)
                {
                    lock (ReactorLock)
                    {
                        IMGEntry preview = null;
                        string type = (string)IMGsList.SelectedItem;
                        IMGFile reactor = MapEditor.reactor.Directory.GetIMG((String)ItemsList.SelectedItem);
                        if (reactor.GetChild("info/link") != null)
                        {
                            reactor = MapEditor.reactor.Directory.GetIMG(reactor.GetString("info/link") + ".img");
                        }
                        preview = reactor.GetChild("0/0");
                        ItemPreview.Image = Map.GetRealImage(preview).GetCanvas().GetBitmap();
                    }
                }
            }

        }

        private void ItemsList_DoubleClick(object sender, EventArgs e)
        {
            if (EditMode.Checked && !LinkToFH.Checked)
            {
                Point pos = new Point((int)(ShiftX / Zoom), (int)(ShiftY / Zoom));
                pos.X += Math.Min(splitContainer1.SplitterDistance, GraphicPanel.Width)/2 - Map.Instance.CenterX;
                pos.Y += Math.Min(splitContainer1.Height, GraphicPanel.Height) / 2 - Map.Instance.CenterY;
                if (EditLife.Checked)
                {
                    string type = (string)IMGsList.SelectedItem;
                    lock ((type == "Npc.wz") ? NpcLock : MobLock)
                    {
                        string id = ((string)ItemsList.SelectedItem).Substring(0, 11);
                        IMGEntry entry = new IMGEntry();
                        entry.SetString("type", type == "Mob.wz" ? "m" : "n");
                        entry.SetString("id", id.Substring(0, id.IndexOf(".")).PadLeft(7, '0'));
                        entry.SetInt("x", pos.X);
                        entry.SetInt("y", pos.Y);
                        if (type == "Mob.wz") entry.SetInt("mobTime", 0);
                        entry.SetInt("f", 0);
                        entry.SetInt("hide", 0);
                        entry.SetInt("fh", 0);
                        entry.SetInt("cy", pos.Y);
                        entry.SetInt("rx0", pos.X - 50);
                        entry.SetInt("rx1", pos.X + 50);
                        MapLife l;
                        if (type == "Npc.wz")
                        {
                            l = new MapNPC();
                        }
                        else
                        {
                            l = new MapMob();
                        }
                        l.Object = entry;
                        if (l is MapNPC)
                        {
                            IMGFile npc = MapEditor.npc.Directory.GetIMG(l.Object.GetString("id") + ".img");
                            if (npc.GetChild("info/link") != null)
                            {
                                npc = MapEditor.npc.Directory.GetIMG(npc.GetString("info/link") + ".img");
                            }
                            l.Image = npc.GetChild("stand/0");

                        }
                        else
                        {
                            IMGFile mob = MapEditor.mob.Directory.GetIMG(l.Object.GetString("id") + ".img");
                            if (mob.GetChild("info/link") != null)
                            {
                                mob = MapEditor.mob.Directory.GetIMG(mob.GetString("info/link") + ".img");
                            }
                            l.Image = mob.GetChild("stand/0");
                            if (l.Image == null) l.Image = mob.GetChild("fly/0");
                        }
                        l.Image = Map.GetRealImage(l.Image);
                        Map.Instance.Add(l);
                        undo.Push(new ActionAdd(l));
                        redo.Clear();
                    }
                }
                else if (EditReactor.Checked)
                {
                    lock (ReactorLock)
                    {
                        string type = (string)IMGsList.SelectedItem;
                        string id = ((string)ItemsList.SelectedItem).Substring(0, 11);
                        IMGEntry entry = new IMGEntry();
                        entry.SetString("id", id.Substring(0, id.IndexOf(".")).PadLeft(7, '0'));
                        entry.SetInt("x", pos.X);
                        entry.SetInt("y", pos.Y);
                        entry.SetInt("reactorTime", 0);
                        entry.SetInt("f", 0);
                        entry.SetString("name", "");
                        MapReactor r = new MapReactor();
                        r.Object = entry;
                        IMGFile reactor = MapEditor.reactor.Directory.GetIMG(r.Object.GetString("id") + ".img");
                        if (reactor.GetChild("info/link") != null)
                        {
                            reactor = MapEditor.reactor.Directory.GetIMG(reactor.GetString("info/link") + ".img");
                        }
                        r.Image = reactor.GetChild("0/0");
                        r.Image = Map.GetRealImage(r.Image);
                        Map.Instance.Add(r);
                        undo.Push(new ActionAdd(r));
                        redo.Clear();
                    }
                }
                GraphicPanel.Focus();
            }

        }

        private void AddLine_Click(object sender, EventArgs e)
        {
            Point pos = new Point((int)(ShiftX / Zoom), (int)(ShiftY / Zoom));
            pos.X += Math.Min(splitContainer1.SplitterDistance, GraphicPanel.Width)/2 - Map.Instance.CenterX;
            pos.Y += Math.Min(splitContainer1.Height, GraphicPanel.Height) / 2 - Map.Instance.CenterY;
            if (EditFH.Checked)
            {
                IMGEntry gentry = new IMGEntry();
                gentry.Name = Map.Instance.GenerateFootholdsGroupID().ToString();
                IMGEntry entry = new IMGEntry();
                entry.Name = Map.Instance.GenerateFootholdID().ToString();
                entry.SetInt("x1", pos.X);
                entry.SetInt("y1", pos.Y);
                entry.SetInt("x2", pos.X + 20);
                entry.SetInt("y2", pos.Y);
                entry.SetInt("prev", 0);
                entry.SetInt("next", 0);
                gentry.Add(entry);
                IMGEntry layer = Map.Instance.map.GetChild("foothold/" + (int)Layer.Value);
                if(layer == null)
                {
                    layer = new IMGEntry();
                    layer.Name = ((int)Layer.Value).ToString();
                    Map.Instance.map.GetChild("foothold").Add(layer);
                }
                layer.Add(gentry);
                MapFootholds group = new MapFootholds(int.Parse(gentry.Name));
                group.Object = gentry;
                MapFoothold fh = new MapFoothold(int.Parse(entry.Name));
                fh.Object = entry;
                fh.s1 = new MapFootholdSide();
                fh.s2 = new MapFootholdSide();
                fh.s1.ID = 1;
                fh.s2.ID = 2;
                fh.s1.Object = entry;
                fh.s2.Object = entry;
                fh.s1.Foothold = fh;
                fh.s2.Foothold = fh;
                fh.Group = group;
                group.footholds.Add(fh.ID, fh);
                group.ToFix = true;
                Map.Instance.layers[(int)Layer.Value].footholdGroups.Add(group.ID, group);
            }
            if (EditLR.Checked)
            {
                IMGEntry entry = new IMGEntry();
                entry.SetInt("l", 1);
                entry.SetInt("uf", 1);
                entry.SetInt("x", pos.X);
                entry.SetInt("y1", pos.Y);
                entry.SetInt("y2", pos.Y + 20);
                entry.SetInt("page", (int)Layer.Value);
                MapLR lr = new MapLR();
                lr.Object = entry;
                lr.s1 = new MapLRSide();
                lr.s2 = new MapLRSide();
                lr.s1.ID = 1;
                lr.s2.ID = 2;
                lr.s1.Object = entry;
                lr.s2.Object = entry;
                lr.s1.LR = lr;
                lr.s2.LR = lr;
                Map.Instance.Add(lr);
            }
            else if (EditSeat.Checked)
            {
                IMGEntry entry = new IMGEntry();
                entry.SetVector(new WZVector(pos.X, pos.Y));

                MapSeat seat = new MapSeat();
                seat.Object = entry;

                Map.Instance.Add(seat);
                undo.Push(new ActionAdd(seat));
                redo.Clear();
            }
            else if (EditPortal.Checked)
            {
                IMGEntry entry = new IMGEntry();

                entry.SetString("pn", "sp");
                entry.SetInt("pt", 0);
                entry.SetInt("x", pos.X);
                entry.SetInt("y", pos.Y);
                entry.SetInt("tm", 999999999);
                entry.SetString("tn", "");

                MapPortal portal = new MapPortal();
                portal.Object = entry;

                Map.Instance.Add(portal);
                undo.Push(new ActionAdd(portal));
                redo.Clear();
            }
            else if (EditToolTip.Checked)
            {
                IMGEntry entry = new IMGEntry();
                entry.SetInt("x1", pos.X);
                entry.SetInt("y1", pos.Y);
                entry.SetInt("x2", pos.X + 40);
                entry.SetInt("y2", pos.Y + 20);
                MapToolTip tt = new MapToolTip();

                tt.Object = entry;
                tt.c1 = new MapToolTipCorner();
                tt.c1.Object = entry;
                tt.c1.type = MapToolTipCornerType.TopLeft;
                tt.c1.ToolTip = tt;
                tt.c2 = new MapToolTipCorner();
                tt.c2.Object = entry;
                tt.c2.type = MapToolTipCornerType.TopRight;
                tt.c2.ToolTip = tt;
                tt.c3 = new MapToolTipCorner();
                tt.c3.Object = entry;
                tt.c3.type = MapToolTipCornerType.BottomLeft;
                tt.c3.ToolTip = tt;
                tt.c4 = new MapToolTipCorner();
                tt.c4.Object = entry;
                tt.c4.type = MapToolTipCornerType.BottomRight;
                tt.c4.ToolTip = tt;
                Map.Instance.Add(tt);
                lock (StringLock)
                {
                    IMGEntry ToolTipsStrings = stringf.Directory.GetIMG("ToolTipHelp.img").GetChild("Mapobject").GetChild(int.Parse(MapEditor.Instance.MapID).ToString());
                    if (ToolTipsStrings == null)
                    {
                        ToolTipsStrings = new IMGEntry();
                        ToolTipsStrings.Name = int.Parse(MapID).ToString();
                        stringf.Directory.GetIMG("ToolTipHelp.img").GetChild("Mapobject").Add(ToolTipsStrings);
                    }
                    IMGEntry strings = new IMGEntry(tt.Object.Name);
                    strings.SetString("Title", "Title");
                    strings.SetString("Desc", "Desc");
                    ToolTipsStrings.Add(strings);
                    stringf.Directory.GetIMG("ToolTipHelp.img").ToSave = true;
                    tt.Image = strings;
                }
            }
            else if (EditClock.Checked)
            {
                if (Map.Instance.clock == null)
                {
                    IMGEntry entry = new IMGEntry();

                    entry.SetInt("x", pos.X - 100);
                    entry.SetInt("y", pos.Y - 100);
                    entry.SetInt("width", 200);
                    entry.SetInt("height", 200);

                    MapClock clock = new MapClock();
                    clock.Object = entry;

                    lock(MapLock)
                        Map.Instance.Add(clock);
                    undo.Push(new ActionAdd(clock));
                    redo.Clear();
                }
            }
        }

        private void AutoConnect_Click(object sender, EventArgs e)
        {
            if (EditFH.Checked)
            {
                foreach(MapLayer layer in Map.Instance.layers)
                {
                    foreach (MapObject obj in layer.objects)
                    {
                        if (obj.Footholds.Count > 0)
                        {
                            List<MapFootholdSide> l = layer.GetConnectionAt(obj.Footholds[0][0].GetX(), obj.Footholds[0][0].GetY());
                            if (l.Count > 0 && l[0].GetX() == obj.Footholds[0][0].GetX() && l[0].GetY() == obj.Footholds[0][0].GetY())
                            {
                                continue;
                            }
                            MapFootholds g = CreateFootholds(obj.Footholds);
                            obj.Object.SetInt("zM", g.ID);
                        }
                    }
                }
            }
            else if (EditSeat.Checked)
            {
                foreach (MapLayer layer in Map.Instance.layers)
                {
                    foreach (MapObject obj in layer.objects)
                    {
                        foreach (MapSeatDesign seat in obj.Seats)
                        {
                            MapSeat t = Map.Instance.GetItemAt(seat.GetX(), seat.GetY()) as MapSeat;
                            
                            if (t != null && t.GetX() == seat.GetX() && t.GetY() == seat.GetY())
                            {
                                continue;
                            }
                            CreateSeat(seat);
                        }
                    }

                }
            }
        }

        private void New_Click(object sender, EventArgs e)
        {
            if (load())
            {
                NewMapWizard wizard = new NewMapWizard();
                wizard.ShowDialog();
                if (!wizard.Cancel)
                {
                    int MapID = int.Parse(wizard.MapID.Text);
                    int width = int.Parse(wizard.MapWidth.Text);
                    int height = int.Parse(wizard.MapHeight.Text);
                    int cy = Math.Min(height / 2, 300);
                    int cx = width / 2;
                    IMGFile img = new IMGFile(MapID.ToString().PadLeft(9, '0') + ".img");
                    IMGEntry info = new IMGEntry("info");
                    info.SetInt("version", 10);
                    info.SetInt("cloud", 0);
                    info.SetInt("town", wizard.IsTown.Checked ? 1 : 0);
                    info.SetInt("version", wizard.IsTown.Checked ? 1 : 0);
                    info.SetInt("returnMap", wizard.IsReturnMap.Checked ? int.Parse(wizard.ReturnMap.Text) : 999999999);
                    info.SetFloat("mobRate", 1);
                    info.SetString("bgm", ((string)wizard.BGMsList.SelectedItem).Replace(".img", ""));
                    info.SetString("mapDesc", "");
                    info.SetInt("hideMinimap", 0);
                    info.SetInt("forcedReturn", 999999999);
                    info.SetInt("moveLimit", 0);
                    info.SetString("mapMark", wizard.selectedMark);
                    info.SetInt("fieldLimit", 0);
                    info.SetInt("VRTop", cy - height);
                    info.SetInt("VRLeft", cx - width);
                    info.SetInt("VRBottom", cy);
                    info.SetInt("VRRight", cx);
                    info.SetInt("swim", wizard.IsSwim.Checked ? 1 : 0);
                    img.Add(info);
                    img.Add(MapBackground.Object.GetChild("back").Clone() as IMGEntry);
                    img.Add(new IMGEntry("life"));
                    for (int i = 0; i < 8; i++)
                    {
                        IMGEntry entry = new IMGEntry(i.ToString());
                        entry.Add(new IMGEntry("info"));
                        entry.Add(new IMGEntry("tile"));
                        entry.Add(new IMGEntry("obj"));
                        img.Add(entry);
                    }
                    img.Add(new IMGEntry("reactor"));
                    img.Add(new IMGEntry("foothold"));
                    if (wizard.IsMiniMap.Checked)
                    {
                        IMGEntry minimap = new IMGEntry("miniMap");
                        minimap.SetCanvas("canvas", new WZCanvas());
                        minimap.SetInt("width", width + 100);
                        minimap.SetInt("height", height + 100);
                        minimap.SetInt("centerX", (width - cx) + 50);
                        minimap.SetInt("centerY", (height - cy) + 50);
                        minimap.SetInt("mag", 4);
                        img.Add(minimap);
                    }
                    img.Add(new IMGEntry("portal"));
                    WZDirectory dir = file.Directory.GetDirectory("Map/Map" + img.Name[0]);
                    dir.IMGs.Add(img.Name, img);
                    img.Directory = dir;
                    IMGEntry sname = new IMGEntry(MapID.ToString());
                    sname.SetString("streetName", wizard.StreetName.Text);
                    sname.SetString("mapName", wizard.MapName.Text);
                    lock (StringLock)
                    {
                        MapEditor.stringf.Directory.GetIMG("Map.img").ToSave = true;
                        MapEditor.stringf.Directory.GetIMG("Map.img").GetChild((string)wizard.MapGroup.SelectedItem).Add(sname);
                    }

                    if (Map.Instance != null)
                    {
                        OnMapUnload();
                    }
                    Map map;

                    lock(MapLock)
                        map = new Map(img);

                    img.ToSave = true;

                    UpdateIMGsList(true);

                    this.MapID = img.Name.Substring(0, 9);

                    ZoomLevel = 0;
                    Zoom = 1;

                    SetMapSize((int)(map.Width * Zoom), (int)(map.Height * Zoom));

                    GraphicPanel.Render();
                }
            }
        }

        private void SelectTile_Click(object sender, EventArgs e)
        {
            lock (MapLock)
            {
                MapTileSelect select = new MapTileSelect();
                select.ShowDialog();
                if (select.ActiveImageViewer != null)
                {
                    if (select.ActiveImageViewer.Name.Replace(".img", "") != Map.Instance.layers[(int)Layer.Value].info.GetString("tS"))
                    {
                        Map.Instance.layers[(int)Layer.Value].ChangeTileStyle(select.ActiveImageViewer.Name.Replace(".img", ""));
                        UpdateIMGsList(true);
                    }
                }
            }
        }

        private void RandomTiles_CheckedChanged(object sender, EventArgs e)
        {
            UpdateIMGsList(true);
        }

        private void ObjectsFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            SubObjectsList.Items.Clear();
            ObjectsList.Controls.Clear();
            List<string> list = new List<string>();
            lock (MapLock)
            {
                IMGFile objects = file.Directory.GetIMG("Obj/" + (string)ObjectsFolders.SelectedItem);
                if (objects != null)
                {
                    foreach (IMGEntry sub in objects.childs.Values)
                    {
                        list.Add(sub.Name);
                    }
                }
            }
            list.Sort();
            SubObjectsList.Items.AddRange(list.ToArray());
        }
        private void SubObjectsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SubObjectsList.SelectedItem != null && EditMode.Checked && EditObj.Checked)
            {
                lock (MapLock)
                    UpdateObjectsList();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            New_Click(sender, e);
        }

        private void sToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save_Click(sender, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveAsimgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Map.Instance != null && MapID != "MapLogin.img")
            {
                SaveFileDialog save = new SaveFileDialog();
                save.FileName = MapID.PadLeft(9, '0');
                save.Filter = ".img File|*.img";
                if (save.ShowDialog() != DialogResult.Cancel)
                {
                    if (Map.Instance.map.GetChild("miniMap") != null)
                    {
                        Bitmap bitmap = new Bitmap(Map.Instance.Width, Map.Instance.Height);
                        using (Graphics g = Graphics.FromImage(bitmap))
                            Map.Instance.Draw(g);
                        bitmap = ResizeBitmap(bitmap, new Size(bitmap.Width / 16, bitmap.Height / 16));

                        Map.Instance.map.GetChild("miniMap").GetCanvas("canvas").SetBitmap(bitmap);
                        Map.Instance.map.GetChild("miniMap").GetCanvas("canvas").format = WZCanvas.ImageFormat.FORMAT_4444;
                    }
                    ((IMGFile)Map.Instance.map).Save(save.FileName);
                }
            }
        }

        private void mportFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (load())
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = ".img File|*.img";
                if (open.ShowDialog() != DialogResult.Cancel)
                {
                    ImportMap imp = new ImportMap(System.IO.Path.GetFileNameWithoutExtension(open.FileName));

                    imp.ShowDialog();
                    if (!imp.Canceled)
                    {
                        File f = new File(open.FileName);
                        IMGFile img = new IMGFile(imp.MapID.Text.PadLeft(9, '0') + ".img", f);

                        WZDirectory dir = file.Directory.GetDirectory("Map/Map" + img.Name[0]);
                        dir.IMGs.Add(img.Name, img);
                        img.Directory = dir;
                        IMGEntry sname = new IMGEntry(int.Parse(imp.MapID.Text).ToString());
                        sname.SetString("streetName", imp.StreetName.Text);
                        sname.SetString("mapName", imp.MapName.Text);
                        lock (StringLock)
                        {
                            MapEditor.stringf.Directory.GetIMG("Map.img").ToSave = true;
                            MapEditor.stringf.Directory.GetIMG("Map.img").GetChild((string)imp.MapGroup.SelectedItem).Add(sname);
                        }

                        if (Map.Instance != null)
                        {
                            OnMapUnload();
                        }
                        Map map;

                        lock (MapLock)
                            map = new Map(img);

                        map.map.ToSave = true;

                        UpdateIMGsList(true);

                        this.MapID = img.Name.Substring(0, 9);

                        ZoomLevel = 0;
                        Zoom = 1;

                        SetMapSize((int)(map.Width * Zoom), (int)(map.Height * Zoom));

                        GraphicPanel.Render();
                    }

                }

            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        private void buttonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Controls().ShowDialog();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("You can't export this file to xml.\n\nTo prevent abusing of this tool, WZ files created by this tool are protected and are not accessible by any tool. (except this tool and the game of course)");
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {

            if (Search.Text == "")
            {
                ShowAll();
            }
            else if (MapSelect.IsNumber(Search.Text))
            {
                ShowByID(Search.Text);
            }
            else
            {
                ShowByName(Search.Text.ToLower());
            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Map.Instance != null)
            {
                GetMapInfo wizard = new GetMapInfo(Map.Instance.map.GetChild("info"));
                wizard.ShowDialog();

                IMGEntry info = Map.Instance.map.GetChild("info");
                info.SetInt("town", wizard.IsTown.Checked ? 1 : 0);
                info.SetInt("returnMap", wizard.IsReturnMap.Checked ? int.Parse(wizard.ReturnMap.Text) : 999999999);
                info.SetString("bgm", ((string)wizard.BGMsList.SelectedItem).Replace(".img", ""));
                info.SetString("mapMark", wizard.selectedMark);
                info.SetInt("swim", wizard.IsSwim.Checked ? 1 : 0);

                if (wizard.selectedBG != null)
                {
                    Map.Instance.map.childs.Remove("back");
                    Map.Instance.map.Add(MapBackground.Object.GetChild("back").Clone() as IMGEntry);

                    IMGEntry back = Map.Instance.map.GetChild("back");
                    foreach (IMGEntry b in back.childs.Values)
                    {
                        if (b.GetInt("ani") != 1 && b.GetString("bS") != "")
                        {
                            MapBack mb = new MapBack();

                            mb.Object = b;
                            mb.Image = MapEditor.file.Directory.GetIMG("Back/" + b.GetString("bS") + ".img").GetChild("back/" + b.GetInt("no").ToString());
                            mb.ID = int.Parse(b.Name);

                            Map.Instance.backs.Add(mb);
                        }
                    }
                }
            }
        }

        private void saveAspngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Map.Instance != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.FileName = MapID.PadLeft(9, '0') + ".png";
                if (MapID == "MapLogin.img") save.FileName = "MapLogin.png";
                save.Filter = ".png File|*.png";
                if (save.ShowDialog() != DialogResult.Cancel)
                {
                    using (Bitmap bitmap = new Bitmap(Map.Instance.Width, Map.Instance.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                            Map.Instance.DrawAll(g);
                        bitmap.Save(save.FileName);
                    }
                }
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Form1_Resize(null, null);
        }

        private void createPatchFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Map.Instance != null && MapID != "MapLogin.img")
            {
                OpenFileDialog ofdOpen = new OpenFileDialog();
                ofdOpen.Filter = "Map.wz|Map.wz";
                ofdOpen.Title = "Select the original files";
                if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                {
                    string fileName = ofdOpen.FileName;
                    ofdOpen.Filter = "String.wz|String.wz";
                    if (ofdOpen.ShowDialog() != DialogResult.Cancel)
                    {
                        SaveFileDialog save = new SaveFileDialog();
                        save.Filter = "*.patch|*.patch";
                        if (save.ShowDialog() != DialogResult.Cancel)
                        {
                            //Save_Click(null, null);
                            WZFile map = new WZFile(fileName, file.Version);
                            WZFile str = new WZFile(ofdOpen.FileName, file.Version);
                            WZPatch patch = new WZPatch();
                            patch.files.Add(WZPatchFile.FromWZ(map, file));
                            patch.files.Add(WZPatchFile.FromWZ(str, stringf));
                            patch.Save(save.FileName);
                            str.Close();
                            map.Close();
                        }
                    }
                }
            }
        }

        private void splitContainer1_Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            // DELETE
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            //ShiftY = e.NewValue;
            //GraphicPanel.Render();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            //ShiftX = e.NewValue;
            //GraphicPanel.Render();
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            ShiftY = (sender as VScrollBar).Value;
            if(!(draggingSelection || dragged || scrolling)) GraphicPanel.Render();

            /*if (draggingSelection)
            {
                GraphicPanel.OnMouseMoveEventCreate();
            }*/
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            ShiftX = hScrollBar1.Value;
            if (!(draggingSelection || dragged || scrolling)) GraphicPanel.Render();
        }

        private void simulate_Click(object sender, EventArgs e)
        {
            if (EditMode.Checked) EditMode_Click(null, null);
            this.splitContainer1.Panel1.Controls.Remove(this.GraphicPanel);

            this.GraphicPanel.OnRender -= new WZMapEditor.RenderEventHandler(this.GraphicPanel_OnRender);
            this.GraphicPanel.MouseMove -= new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseMove);
            this.GraphicPanel.MouseWheel -= new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_MouseWheel);
            this.GraphicPanel.MouseDoubleClick -= new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseDoubleClick);
            this.GraphicPanel.MouseDown -= new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseDown);
            this.GraphicPanel.OnCreateDevice -= new WZMapEditor.CreateDeviceEventHandler(this.OnResetDevice);
            this.GraphicPanel.MouseUp -= new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseUp);
            this.GraphicPanel.KeyDown -= new System.Windows.Forms.KeyEventHandler(GraphicPanel_OnKeyDown);
            this.GraphicPanel.KeyUp -= new System.Windows.Forms.KeyEventHandler(GraphicPanel_OnKeyUp);

            //Enabled = false;
            Visible = false;

            new Simulator(GraphicPanel).Show();
        }

        public void simulate_End()
        {
            this.GraphicPanel.OnRender += new WZMapEditor.RenderEventHandler(this.GraphicPanel_OnRender);
            this.GraphicPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseMove);
            this.GraphicPanel.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_MouseWheel);
            this.GraphicPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseDoubleClick);
            this.GraphicPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseDown);
            this.GraphicPanel.OnCreateDevice += new WZMapEditor.CreateDeviceEventHandler(this.OnResetDevice);
            this.GraphicPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GraphicPanel_OnMouseUp);
            this.GraphicPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(GraphicPanel_OnKeyDown);
            this.GraphicPanel.KeyUp += new System.Windows.Forms.KeyEventHandler(GraphicPanel_OnKeyUp);

            this.splitContainer1.Panel1.Controls.Add(this.GraphicPanel);

            //Enabled = true;
            Visible = true;

            GraphicPanel.Focus();
            Form1_Resize(null, null);
        }

        /*private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Bitmap p = new Bitmap(800, 600);
            using (Graphics g = Graphics.FromImage(p))
            {
                foreach (MapBack b in Map.Instance.backs)
                {
                    b.Draw(g);
                }
            }
            p.Save("test.png", System.Drawing.Imaging.ImageFormat.Png);
        }*/
    }
}
