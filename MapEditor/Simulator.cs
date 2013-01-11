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
using System.Threading;

namespace WZMapEditor
{
    public partial class Simulator : Form
    {
        DevicePanel Graphics;
        MaplePyhsics physics;

        public Simulator(DevicePanel Graphics)
        {
            InitializeComponent();

            Controls.Add(Graphics);

            this.Graphics = Graphics;

            Graphics.OnRender += new RenderEventHandler(Graphics_OnRender);
            Graphics.MouseMove += new MouseEventHandler(Graphics_MouseMove);
            Graphics.MouseWheel += new MouseEventHandler(Graphics_MouseWheel);
            Graphics.MouseDoubleClick += new MouseEventHandler(Graphics_MouseDoubleClick);
            Graphics.MouseDown += new MouseEventHandler(Graphics_MouseDown);
            Graphics.OnCreateDevice += new CreateDeviceEventHandler(Graphics_OnCreateDevice);
            Graphics.MouseUp += new MouseEventHandler(Graphics_MouseUp);
            Graphics.KeyDown += new KeyEventHandler(Graphics_KeyDown);
            Graphics.KeyUp += new KeyEventHandler(Graphics_KeyUp);

            //Graphics.Size = new Size(800, 600);
            MapEditor.Instance.Width = 800;
            MapEditor.Instance.Height = 600;
            Graphics.Focus();

            if (MapEditor.Instance.ShiftX < Map.Instance.VRLeft + Map.Instance.CenterX) MapEditor.Instance.ShiftX = Map.Instance.VRLeft + Map.Instance.CenterX;
            if (MapEditor.Instance.ShiftX + 800 > Map.Instance.VRRight + Map.Instance.CenterX) MapEditor.Instance.ShiftX = Map.Instance.VRRight + Map.Instance.CenterX - 800;
            if (MapEditor.Instance.ShiftY < Map.Instance.VRTop + Map.Instance.CenterY) MapEditor.Instance.ShiftY = Map.Instance.VRTop + Map.Instance.CenterY;
            if (MapEditor.Instance.ShiftY + 600 > Map.Instance.VRBottom + Map.Instance.CenterY) MapEditor.Instance.ShiftY = Map.Instance.VRBottom + Map.Instance.CenterY - 600;

            physics = new MaplePyhsics(MapEditor.file.Directory.GetIMG("Physics.img"), 0, 0);
        }

        void Graphics_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) physics.left = true;
            else if (e.KeyCode == Keys.Right) physics.right = true;
            else if (e.KeyCode == Keys.Up) physics.up = true;
            else if (e.KeyCode == Keys.Down) physics.down = true;
            else if (e.KeyCode == Keys.ControlKey) physics.alt = true;
            //int.if (e.KeyCode == Keys.Down)
            //{
            //    if (MapEditor.Instance.ShiftY + 600 < Map.Instance.VRBottom + Map.Instance.CenterY) MapEditor.Instance.ShiftY++;
            //}
            //else if (e.KeyCode == Keys.Up)
            //{
            //    if (MapEditor.Instance.ShiftY > Map.Instance.VRTop + Map.Instance.CenterY) MapEditor.Instance.ShiftY--;
            //}
            //else if (e.KeyCode == Keys.Left)
            //{
            //    if (MapEditor.Instance.ShiftX > Map.Instance.VRLeft + Map.Instance.CenterX) MapEditor.Instance.ShiftX--;
            //}
            //else if (e.KeyCode == Keys.Right)
            ////{
            //    if (MapEditor.Instance.ShiftX + 800 < Map.Instance.VRRight + Map.Instance.CenterX) MapEditor.Instance.ShiftX++;
            //}
        }

        void Graphics_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) physics.left = false;
            else if (e.KeyCode == Keys.Right) physics.right = false;
            else if (e.KeyCode == Keys.Up) physics.up = false;
            else if (e.KeyCode == Keys.Down) physics.down = false;
            else if (e.KeyCode == Keys.ControlKey) physics.alt = false;
        }

        void Graphics_MouseUp(object sender, MouseEventArgs e)
        {

        }

        void Graphics_OnCreateDevice(object sender, DeviceEventArgs e)
        {

        }

        void Graphics_MouseDown(object sender, MouseEventArgs e)
        {

        }

        void Graphics_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        void Graphics_MouseWheel(object sender, MouseEventArgs e)
        {

        }

        void Graphics_MouseMove(object sender, MouseEventArgs e)
        {

        }

        void Graphics_OnRender(object sender, DeviceEventArgs e)
        {
            int x = physics.x;
            int y = physics.y;
            MapEditor.Instance.ShiftX = x + Map.Instance.CenterX - 400;
            MapEditor.Instance.ShiftY = y + Map.Instance.CenterY - 300;
            if (Map.Instance != null)
            {
                Map.Instance.DrawAnimation(Graphics);

            }
            Graphics.DrawCircle(x + Map.Instance.CenterX, y + Map.Instance.CenterY, Color.Black);
        }

        private void Simulator_FormClosed(object sender, FormClosedEventArgs e)
        {
            Graphics.OnRender -= new RenderEventHandler(Graphics_OnRender);
            Graphics.MouseMove -= new MouseEventHandler(Graphics_MouseMove);
            Graphics.MouseWheel -= new MouseEventHandler(Graphics_MouseWheel);
            Graphics.MouseDoubleClick -= new MouseEventHandler(Graphics_MouseDoubleClick);
            Graphics.MouseDown -= new MouseEventHandler(Graphics_MouseDown);
            Graphics.OnCreateDevice -= new CreateDeviceEventHandler(Graphics_OnCreateDevice);
            Graphics.MouseUp -= new MouseEventHandler(Graphics_MouseUp);
            Graphics.KeyDown -= new KeyEventHandler(Graphics_KeyDown);
            Graphics.KeyUp -= new KeyEventHandler(Graphics_KeyUp);

            Controls.Remove(Graphics);

            MapEditor.Instance.simulate_End();
        }
    }
}
