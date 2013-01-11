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
using System.Collections;

namespace WZMapEditor
{
    public partial class MapBackgroundSelect : Form
    {
        Hashtable maps = new Hashtable();
        public MapBackgroundSelect(string selected)
        {
            InitializeComponent();
            List<string> MapNames = new List<string>();

            WZDirectory mapsF = MapEditor.file.Directory.GetDirectory("Map");
            foreach (WZDirectory maps in mapsF.Directories.Values)
            {
                foreach (IMGFile map in maps.IMGs.Values)
                {
                    MapNames.Add(maps.Name + "/" + map.Name);
                }
            }

            MapNames.Sort();

            MapsList.Items.AddRange(MapNames.ToArray());

            if (selected != null)
            {
                MapsList.SelectedItem = selected;
                MapsList_SelectedIndexChanged(null, null);
            }
        }

        public MapBackground GetMapBackground()
        {
            if (maps.Contains((String)MapsList.SelectedItem))
            {
                return (MapBackground)maps[(String)MapsList.SelectedItem];
            }
            else
            {
                IMGEntry entry = MapEditor.file.Directory.GetIMG("Map/" + (String)MapsList.SelectedItem);
                if (entry == null)
                {
                    maps.Add((String)MapsList.SelectedItem, null);
                    return null;
                }
                else
                {
                    MapBackground bg = new MapBackground();
                    MapBackground.Object = entry;
                    lock(MapEditor.MapLock)
                        bg.Load();
                    maps.Add((String)MapsList.SelectedItem, bg);
                    return bg;
                }
            }
        }
        private void MapsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapBackground bg = GetMapBackground();
            if (bg != null)
            {
                if (!select.Enabled) select.Enabled = true;
                BackgroundPreview.Image = bg.Bitmap;
            }
            else
            {
                select.Enabled = false;
                BackgroundPreview.Image = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
