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

namespace WZMapEditor
{
    public partial class GetPortalInfo : Form
    {
        public GetPortalInfo(int type, string name, int id, string pn)
        {
            InitializeComponent();
            PortalType.Items.Add("Spawn Point");
            PortalType.Items.Add("Teleport Point/Hidden Teleport");
            PortalType.Items.Add("Teleport (To another map)");
            PortalType.Items.Add("Auto Teleport (force teleport)");
            PortalType.Items.Add("Hidden Teleport (With hint)");

            PortalName.Text = name;
            ToMap.Text = id.ToString();
            ToName.Text = pn;

            int i = 0;

            switch (type)
            {
                case 0: i = 0; break;
                case 1: i = 1; break;
                case 2: i = 2; break;
                case 3: i = 3; break;
                case 10: i = 4; break;
            }

            if (i == 1 && pn != "")
            {
                IsTeleport.Checked = true;
            }
            if ((i == 4 || i == 3 || (i == 1 && IsTeleport.Checked)) && id == int.Parse(MapEditor.Instance.MapID))
            {
                ThisMap.Checked = true;
            }
            PortalType.SelectedIndex = i;
            InitializeType();
        }

        private void InitializeType()
        {
            if (PortalType.SelectedIndex == 0)
            {
                ThisMap.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                ToMap.Visible = false;
                ToName.Visible = false;
                IsTeleport.Visible = false;
                PortalName.Visible = false;
            }
            else if (PortalType.SelectedIndex == 1)
            {
                ThisMap.Visible = true;
                label1.Visible = true;
                IsTeleport.Visible = true;
                PortalName.Visible = true;

                if (IsTeleport.Checked)
                {
                    label2.Visible = true;
                    ToMap.Visible = true;
                    label3.Visible = true;
                    ToName.Visible = true;
                    if (ThisMap.Checked)
                    {
                        ToMap.Enabled = false;
                    }
                    else
                    {
                        ToMap.Enabled = true;
                    }
                }
                else
                {
                    ToMap.Visible = false;
                    label2.Visible = false;
                    label3.Visible = false;
                    ToName.Visible = false;
                }
            }
            else if (PortalType.SelectedIndex == 2)
            {
                ThisMap.Visible = false;
                ToMap.Enabled = true;
                label1.Visible = true;
                IsTeleport.Visible = false;
                PortalName.Visible = true;
                label2.Visible = true;
                ToMap.Visible = true;
                label3.Visible = true;
                ToName.Visible = true;
            }
            else if (PortalType.SelectedIndex == 3 || PortalType.SelectedIndex == 4)
            {
                label1.Visible = true;
                IsTeleport.Visible = false;
                ThisMap.Visible = true;
                PortalName.Visible = true;
                label2.Visible = true;
                ToMap.Visible = true;
                label3.Visible = true;
                ToName.Visible = true;

                if (ThisMap.Checked)
                {
                    ToMap.Enabled = false;
                }
                else
                {
                    ToMap.Enabled = true;
                }
            }
        }

        private void ThisMap_CheckedChanged(object sender, EventArgs e)
        {
            InitializeType();
        }

        private void IsTeleport_CheckedChanged(object sender, EventArgs e)
        {
            InitializeType();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PortalType_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitializeType();
        }

        private void GetPortalInfo_Load(object sender, EventArgs e)
        {

        }
    }
}
