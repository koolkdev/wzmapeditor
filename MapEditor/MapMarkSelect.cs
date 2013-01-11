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
using System.IO;
using WZ.Objects;

namespace WZMapEditor
{
    public partial class MapMarkSelect : Form
    {
        private ImageViewer m_ActiveImageViewer;
        public ImageViewer ActiveImageViewer
        {
            get
            {
                return m_ActiveImageViewer;
            }
        }
        
        public MapMarkSelect()
        {
            InitializeComponent();

            IMGEntry Marks = MapEditor.file.Directory.GetIMG("MapHelper.img").GetChild("mark");
            foreach (IMGEntry mark in Marks.childs.Values)
            {
                ImageViewer imageViewer = Panel.Add(mark.GetCanvas().GetBitmap(), mark.Name, false);
                imageViewer.MouseClick += new MouseEventHandler(ImageViewer_MouseClick);
                imageViewer.MouseDoubleClick += new MouseEventHandler(ImageViewer_MouseDoubleClick);
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
            Close();
        }


        public static Bitmap GetMark(string selectedMark)
        {
            if (selectedMark == "None") return null;
            return MapEditor.file.Directory.GetIMG("MapHelper.img").GetCanvas("mark/" + selectedMark).GetBitmap();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdOpen = new OpenFileDialog();
            ofdOpen.Filter = "Mark|*.png";
            if (ofdOpen.ShowDialog() != DialogResult.Cancel)
            {
                string FileName = ofdOpen.FileName;
                string ImageName = Path.GetFileNameWithoutExtension(FileName);
                foreach(ImageViewer iv in Panel.Controls)
                {
                    if (iv.Name == ImageName)
                    {
                        Error.Text = "Error: There is already a mark with this name.";
                        return;
                    }
                }
                Image i = Bitmap.FromFile(FileName);
                if (i.Width > 38 || i.Height > 38)
                {
                    Error.Text = "Error: The size of the image must not be larger than 38x38";
                    return;
                }
                IMGEntry marks = MapEditor.file.Directory.GetIMG("MapHelper.img").GetChild("mark");
                IMGEntry entry = new IMGEntry();

                WZCanvas c = new WZCanvas();
                c.SetBitmap((Bitmap)i);
                c.format = WZCanvas.ImageFormat.FORMAT_4444;
                entry.SetCanvas(c);
                entry.Name = ImageName;

                marks.Add(entry);

                marks.parent.ToSave = true;
                Error.Text = "";
                 ImageViewer imageViewer = new ImageViewer();
                imageViewer.Dock = DockStyle.Bottom;

                imageViewer.Image = c.GetBitmap();
                imageViewer.Width = c.GetBitmap().Width + 6;
                imageViewer.Height = c.GetBitmap().Height + 6;
                imageViewer.Name = entry.Name;
                imageViewer.MouseClick += new MouseEventHandler(ImageViewer_MouseClick);
                imageViewer.MouseDoubleClick += new MouseEventHandler(ImageViewer_MouseDoubleClick);
                imageViewer.IsThumbnail = false;

                Panel.Controls.Add(imageViewer);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
