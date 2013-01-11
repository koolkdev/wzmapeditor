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
using System.Windows.Forms;

namespace WZMapEditor
{
    public class NumericTextBox : TextBox
    {
        int WM_KEYDOWN = 0x0100,
            WM_PASTE = 0x0302;
        public override bool PreProcessMessage(ref Message msg)
        {
            if (msg.Msg == WM_KEYDOWN)
            {
                Keys keys = (Keys)msg.WParam.ToInt32();
                bool numbers = ((keys >= Keys.D0 && keys <= Keys.D9)
                    || (keys >= Keys.NumPad0 && keys <= Keys.NumPad9)) && ModifierKeys != Keys.Shift;
                bool ctrl = keys == Keys.Control;
                bool ctrlZ = keys == Keys.Z && ModifierKeys == Keys.Control,
                    ctrlX = keys == Keys.X && ModifierKeys == Keys.Control,
                    ctrlC = keys == Keys.C && ModifierKeys == Keys.Control,
                    ctrlV = keys == Keys.V && ModifierKeys == Keys.Control,
                    del = keys == Keys.Delete,
                    bksp = keys == Keys.Back,
                    arrows = (keys == Keys.Up)
                    | (keys == Keys.Down)
                    | (keys == Keys.Left)
                    | (keys == Keys.Right);
                if (numbers | ctrl | del | bksp
                                 | arrows | ctrlC | ctrlX | ctrlZ)
                    return false;
                else if (ctrlV)
                {
                    IDataObject obj = Clipboard.GetDataObject();
                    string input = (string)obj.GetData(typeof(string));
                    foreach (char c in input)
                    {
                        if (!char.IsDigit(c)) return true;
                    }
                    return false;
                }
                else
                    return true;
            }
            else


                return base.PreProcessMessage(ref msg);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PASTE)
            {
                IDataObject obj = Clipboard.GetDataObject();
                string input = (string)obj.GetData(typeof(string));
                foreach (char c in input)
                {
                    if (!char.IsDigit(c))
                    {
                        m.Result = (IntPtr)0;
                        return;
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
