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

namespace WZMapEditor
{
    public partial class NewMapWizard : Form
    {

        public MapBackground selectedBG;
        string selectedBGName;
        public string selectedMark = "None";

        public bool Cancel = true;

        public NewMapWizard()
        {
            InitializeComponent();
            List<string> MusicNames = new List<string>();

            lock (MapEditor.SoundLock)
            {
                foreach (IMGFile bgms in MapEditor.sound.Directory.IMGs.Values)
                {
                    if (bgms.Name.Substring(0, 3) == "Bgm")
                    {
                        foreach (IMGEntry sound in bgms.childs.Values)
                        {
                            MusicNames.Add(bgms.Name + "/" + sound.Name);
                        }
                    }
                }
            }

            MusicNames.Sort();

            BGMsList.Items.AddRange(MusicNames.ToArray());

            List<string> GroupNames = new List<string>();

            IMGFile maps = MapEditor.stringf.Directory.GetIMG("Map.img");

            string[] array = new string[maps.childs.Keys.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = maps.childs[i].Name;
            }

            MapGroup.Items.AddRange(array);
        }

        private void SelectBackground_Click(object sender, EventArgs e)
        {
            MapBackgroundSelect select = new MapBackgroundSelect(selectedBGName);

            select.ShowDialog();

            selectedBG = select.GetMapBackground();
            BackgroundPreview.Image = selectedBG.GetThumb();
            selectedBGName = (string)select.MapsList.SelectedItem;
        }

        private void NewMapWizard_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MapMarkSelect select = new MapMarkSelect();
            select.ShowDialog();

            if (select.ActiveImageViewer != null)
            {
                selectedMark = select.ActiveImageViewer.Name;
                MarkPreview.Image = select.ActiveImageViewer.Image;
            }
        }

        private void IsReturnMap_CheckedChanged(object sender, EventArgs e)
        {
            if (IsReturnMap.Checked)
            {
                ReturnMap.Enabled = true;
            }
            else
            {
                ReturnMap.Enabled = false;
            }
        }

        private void Create_Click(object sender, EventArgs e)
        {
            Cancel = false;
            Close();
        }
    }
}
