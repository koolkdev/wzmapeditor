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
using System.Collections;
using System.Text.RegularExpressions;

namespace WZMapEditor
{
    public partial class MapSelect : Form
    {
        SortedList maps  = new SortedList();
        Hashtable mapNames = new Hashtable();
        string [] fullList;

        public String Result = "";

        public MapSelect()
        {
            InitializeComponent();

            lock (MapEditor.MapLock)
            {
                WZDirectory mapsF = MapEditor.file.Directory.GetDirectory("Map");
                foreach (WZDirectory mapsE in mapsF.Directories.Values)
                {
                    foreach (IMGFile map in mapsE.IMGs.Values)
                    {
                        maps.Add(int.Parse(map.Name.Substring(0, 9)).ToString(), map.Name + " - " + GetMapName(int.Parse(map.Name.Substring(0, 9)).ToString()));
                    }
                }
            }

            foreach (string map in maps.Keys)
            {
                mapNames.Add(map, GetFullMapName(map).ToLower());
            }

            List<String> list = new List<string>(maps.Values.Cast<string>());
            list.Sort();
            fullList = list.ToArray();

            ShowAll();
        }

        private string GetFullMapName(string mapID)
        {
            lock (MapEditor.StringLock)
            {
                IMGFile strings = MapEditor.stringf.Directory.GetIMG("Map.img");

                foreach (IMGEntry maps in strings.childs.Values)
                {
                    if (maps.GetChild(mapID) != null)
                    {
                        return maps.GetChild(mapID).GetString("streetName") + " : " + maps.GetChild(mapID).GetString("mapName");
                    }
                }
            }
            return "";
        }

        private string GetMapName(string mapID)
        {
            IMGFile strings = MapEditor.stringf.Directory.GetIMG("Map.img");

            foreach (IMGEntry maps in strings.childs.Values)
            {
                if (maps.GetChild(mapID) != null)
                {
                    return maps.GetChild(mapID).GetString("mapName");
                }
            }
            return "";
        }

        private void ShowAll()
        {
            MapList.Items.Clear();

            MapList.Items.AddRange(fullList.ToArray());
        }

        int Length(int num)
        {
            return num.ToString().Length;
        }

        public static bool IDMatch(string oID, string sID)
        {
            if(sID.Length > oID.Length) return false;
            for (int i = 0; i < sID.Length; i++)
            {
                if (sID[i] != oID[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void ShowByID(string id)
        {
            List<string> MapNames = new List<string>();
            foreach(string mID in maps.Keys)
            {
                if(IDMatch(mID, id))
                {
                    MapNames.Add((string)maps[mID]);
                }
            }

            MapNames.Sort();

            MapList.Items.Clear();            
            MapList.Items.AddRange(MapNames.ToArray());
        }

        private void ShowByName(string sName)
        {
            List<string> MapNames1 = new List<string>();
            Hashtable MapNames2 = new Hashtable();
            Hashtable MapNames3 = new Hashtable();

            foreach (DictionaryEntry entry in mapNames)
            {
                if (((string)entry.Value).IndexOf(sName) != -1)
                {
                    if (((string)entry.Value) == sName || ((string)entry.Value).Substring(((string)entry.Value).IndexOf(":") + 2) == sName)
                    {
                        MapNames1.Add((string)maps[(string)entry.Key]);
                    }
                    else if (((string)entry.Value).Substring(((string)entry.Value).IndexOf(":") + 2).IndexOf(sName) != -1)
                    {
                        MapNames2.Add((string)maps[(string)entry.Key], ((string)entry.Value).Substring(((string)entry.Value).IndexOf(":") + 2).IndexOf(sName) + ((string)entry.Value).Replace(sName, "").Length + ((int.Parse((string)entry.Key) % 10 == 0) ? 0 : 2) + ((int.Parse((string)entry.Key) / 10 % 10 == 0) ? 0 : 3) + ((int.Parse((string)entry.Key) / 100 % 10 == 0) ? 0 : 4) + ((int.Parse((string)entry.Key) / 1000 % 10 == 0) ? 0 : 1) + ((int.Parse((string)entry.Key) / 10000 % 10 == 0) ? 0 : 1) + ((int.Parse((string)entry.Key) / 10000 % 10 == 0) ? 0 : 1) + ((int.Parse((string)entry.Key) / 100000 % 10 == 0) ? 0 : 1));
                    }
                    else
                    {
                        MapNames3.Add((string)maps[(string)entry.Key], ((string)entry.Value).IndexOf(sName) + ((string)entry.Value).Replace(sName, "").Length + ((int.Parse((string)entry.Key) % 10 == 0) ? 0 : 2) + ((int.Parse((string)entry.Key) / 10 % 10 == 0) ? 0 : 3) + ((int.Parse((string)entry.Key) / 100 % 10 == 0) ? 0 : 4) + ((int.Parse((string)entry.Key) / 1000 % 10 == 0) ? 0 : 1) + ((int.Parse((string)entry.Key) / 10000 % 10 == 0) ? 0 : 1) + ((int.Parse((string)entry.Key) / 10000 % 10 == 0) ? 0 : 1) + ((int.Parse((string)entry.Key) / 100000 % 10 == 0) ? 0 : 1));
                    }
                }
            }

            MapNames1.Sort();
            
            List<string> MapNames4 = new List<string>(MapNames2.Keys.Cast<string>());
            List<string> MapNames5 = new List<string>(MapNames3.Keys.Cast<string>());

            MapNames4.Sort();
            MapNames5.Sort();
            MapNames4 = MapNames4.OrderBy(s => MapNames2[s]).ToList<string>();
            MapNames5 = MapNames5.OrderBy(s => MapNames2[s]).ToList<string>();

            MapList.Items.Clear();

            MapList.Items.AddRange(MapNames1.ToArray());
            MapList.Items.AddRange(MapNames4.ToArray());
            MapList.Items.AddRange(MapNames5.ToArray());
        }

        private void MapList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MapList.SelectedItem != null)
            {
                lock (MapEditor.MapLock)
                {
                    WZCanvas preview = MapEditor.file.Directory.GetIMG(GetSelectedMap()).GetCanvas("miniMap/canvas");
                    if (!select.Enabled) select.Enabled = true;
                    if (preview != null)
                    {
                        Preview.Image = preview.GetBitmap();
                    }
                    else
                    {
                        Preview.Image = null;
                    }
                    MapName.Text = GetFullMapName(int.Parse(((string)MapList.SelectedItem).Substring(0, 9)).ToString());
                }
            }
            else
            {
                MapName.Text = "";
                Preview.Image = null;
            }
        }

        private string GetSelectedMap()
        {
            string map = ((string)MapList.SelectedItem).Substring(0, 13);
            return "Map/Map" + map[0] + "/" + map;

        }
        private void MapList_DoubleClick(object sender, EventArgs e)
        {
            if (select.Enabled)
            {
                select_Click(sender, e);
            }
        }

        private void select_Click(object sender, EventArgs e)
        {
            Result = GetSelectedMap();
            Close();
        }

        public static bool IsNumber(string text)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(text);
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {
            if (Search.Text == "")
            {
                ShowAll();
            }
            else if (IsNumber(Search.Text))
            {
                ShowByID(Search.Text);
            }
            else
            {
                ShowByName(Search.Text.ToLower());
            }
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (MapList.SelectedItem == null)
                {
                    if (MapList.Items.Count > 0)
                    {
                        MapList.SelectedIndex = 0;
                        select_Click(null, null);
                    }
                }
                else
                {
                    select_Click(null, null);
                }
            }
        }
    }
}
