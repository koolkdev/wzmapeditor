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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WZMapEditor
{
    public partial class DevicePanel : UserControl, ICached
    {
        #region Members

        private bool mouseMoved = false;

        Sprite sprite;
        Sprite sprite2;
        Texture tx;
        Bitmap txb;
        Texture tcircle;
        Texture tecircle;
        Texture hvcursor;
        Bitmap hvcursorb;
        Texture vcursor;
        Bitmap vcursorb;
        Texture hcursor;
        Bitmap hcursorb;
        Bitmap tcircleb;
        Bitmap tecircleb;

        private Rectangle screen;

        public bool bRender = true;

        // The DirectX device
        public Device _device = null;
        private bool deviceLost;
        private Microsoft.DirectX.Direct3D.Font font;
        private Microsoft.DirectX.Direct3D.Font fontBold;
        //public VertexBuffer vb;
        // The Form to place the DevicePanel onto
        private Form _parent = null;
        private PresentParameters presentParams;
        // On this event we can start to render our scene
        public event RenderEventHandler OnRender;

        // Now we know that the device is created
        public event CreateDeviceEventHandler OnCreateDevice;

        private MouseEventArgs lastEvent;

        #endregion

        #region ctor

        public DevicePanel()
        {
            InitializeComponent();
        }

        #endregion

        #region Init DX

        /// <summary>
        /// Init the DirectX-Stuff here
        /// </summary>
        /// <param name="parent">parent of the DevicePanel</param>
        public void Init(Form parent)
        {
            try
            {
                _parent = parent;

                // Setup our D3D stuff
                presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                Caps caps = Manager.GetDeviceCaps(Manager.Adapters.Default.Adapter, DeviceType.Hardware);

                CreateFlags createFlags;

                if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                {
                    createFlags = CreateFlags.HardwareVertexProcessing;
                }
                else
                {
                    createFlags = CreateFlags.SoftwareVertexProcessing;
                }

                if (caps.DeviceCaps.SupportsPureDevice && createFlags ==
                    CreateFlags.HardwareVertexProcessing)
                {
                    createFlags |= CreateFlags.PureDevice;
                }     

     
                _device = new Device(0, DeviceType.Hardware, this,
                createFlags, presentParams);

                if (OnCreateDevice != null)
                    OnCreateDevice(this, new DeviceEventArgs(_device));

                parent.Show();
                parent.Focus();

                //vb = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                //    4, _device, Usage.Dynamic | Usage.WriteOnly,
                //    CustomVertex.PositionTextured.Format, Pool.Default);

                sprite = new Sprite(_device);
                sprite2 = new Sprite(_device);
                //_device.SetStreamSource(0, vb, 0);

                txb = new Bitmap(1, 1);
                using (Graphics g = Graphics.FromImage(txb))
                    g.Clear(Color.White);

                tx = new Texture(_device, txb, Usage.Dynamic, Pool.Default);

                tcircleb = new Bitmap(9, 9);
                using (Graphics g = Graphics.FromImage(tcircleb))
                    g.FillEllipse(Brushes.White, new Rectangle(0, 0, 9, 9));

                tcircle = new Texture(_device, tcircleb, Usage.Dynamic, Pool.Default);

                tecircleb = new Bitmap(14, 14);
                using (Graphics g = Graphics.FromImage(tecircleb))
                    g.DrawEllipse(new Pen(Brushes.White), new Rectangle(0, 0, 13, 13));

                tecircle = new Texture(_device, tecircleb, Usage.Dynamic, Pool.Default);

                hcursorb = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(hcursorb))
                    Cursors.NoMoveHoriz.Draw(g, new Rectangle(0, 0, 32, 32));

                MakeGray(hcursorb);

                hcursor = new Texture(_device, hcursorb, Usage.Dynamic, Pool.Default);

                vcursorb = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(vcursorb))
                    Cursors.NoMoveVert.Draw(g, new Rectangle(0, 0, 32, 32));

                MakeGray(vcursorb);

                vcursor = new Texture(_device, vcursorb, Usage.Dynamic, Pool.Default);

                hvcursorb = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(hvcursorb))
                    Cursors.NoMove2D.Draw(g, new Rectangle(0, 0, 32, 32));

                MakeGray(hvcursorb);

                hvcursor = new Texture(_device, hvcursorb, Usage.Dynamic, Pool.Default); 

                
                font = new Microsoft.DirectX.Direct3D.Font(_device, new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177))));
                fontBold = new Microsoft.DirectX.Direct3D.Font(_device, new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177))));
                
                while (Created)
                {
                    if (bRender) Render();
                    if (mouseMoved)
                    {
                        OnMouseMove(lastEvent);
                        mouseMoved = false;
                    }
                    Application.DoEvents();
                }

            }
            catch (DirectXException ex)
            {
                throw new ArgumentException("Error initializing DirectX", ex);
            }
        }

        private void MakeGray(Bitmap image)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y).Name == "ffffffff")
                        image.SetPixel(x, y, Color.Transparent);
                    else if (image.GetPixel(x, y).Name == "ff000000")
                        image.SetPixel(x, y, Color.Gray);
                }
            }
        }

        /// <summary>
        /// Attempt to recover the device if it is lost.
        /// </summary>
        protected void AttemptRecovery()
        {
            if (_device == null) return;
            try
            {
                _device.TestCooperativeLevel();
            }
            catch (DeviceLostException)
            {
            }
            catch (DeviceNotResetException)
            {
                try
                {
                    _device.Reset(presentParams);
                    deviceLost = false;
                }
                catch (DeviceLostException)
                {
                    // If it's still lost or lost again, just do nothing
                }
            }
        }


        #endregion

        #region Properties

        /// <summary>
        /// Extend this list of properties if you like
        /// </summary>
        private Color _deviceBackColor = Color.Black;

        public Color DeviceBackColor
        {
            get { return _deviceBackColor; }
            set { _deviceBackColor = value; }
        }


        #endregion

        #region Rendering

        /// <summary>
        /// Rendering-method
        /// </summary>
        public void Render()
        {
            if (deviceLost) AttemptRecovery();
            if (deviceLost) return;

            if (_device == null)
                return;

            try
            {
                //Clear the backbuffer
                _device.Clear(ClearFlags.Target, _deviceBackColor, 1.0f, 0);

                //Begin the scene
                _device.BeginScene();

                sprite.Transform = Matrix.Scaling((float)MapEditor.Instance.Zoom, (float)MapEditor.Instance.Zoom, 1f); 
                
                sprite2.Begin(SpriteFlags.AlphaBlend);
                sprite.Begin(SpriteFlags.AlphaBlend);

                // Render of scene here
                if (OnRender != null)
                    OnRender(this, new DeviceEventArgs(_device));

                sprite.End();
                sprite2.End();
                //End the scene
                _device.EndScene();
                _device.Present();
            }
            catch (DeviceLostException)
            {
                deviceLost = true;
            }
        }

        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            // Render on each Paint
            this.Render();
        }

        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            // Close on escape
            if ((int)(byte)e.KeyChar == (int)System.Windows.Forms.Keys.Escape)
                _parent.Close();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up: return true;
                case Keys.Down: return true;
                case Keys.Right: return true;
                case Keys.Left: return true;
                case Keys.PageUp: return true;
                case Keys.PageDown: return true;
            }
            return base.IsInputKey(keyData);
        }

        /*protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            MapEditor.Instance.GraphicPanel_OnKeyDown(null, e);
        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            MapEditor.Instance.GraphicPanel_OnKeyUp(null, e);
        }*/

        protected override Point ScrollToControl(Control activeControl)
        {
            return this.AutoScrollPosition;
        }
        
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            lastEvent = e;
            base.OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, e.X + MapEditor.Instance.ShiftX, e.Y + MapEditor.Instance.ShiftY, e.Delta));
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(new MouseEventArgs(e.Button, e.Clicks, e.X + MapEditor.Instance.ShiftX, e.Y + MapEditor.Instance.ShiftY, e.Delta));
        }

        protected override void OnMouseDoubleClick(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDoubleClick(new MouseEventArgs(e.Button, e.Clicks, e.X + MapEditor.Instance.ShiftX, e.Y + MapEditor.Instance.ShiftY, e.Delta));
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, e.X + MapEditor.Instance.ShiftX, e.Y + MapEditor.Instance.ShiftY, e.Delta));
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, e.X + MapEditor.Instance.ShiftX, e.Y + MapEditor.Instance.ShiftY, e.Delta));
        }

        #endregion

        public void Cache()
        {
            screen = new Rectangle(MapEditor.Instance.ShiftX, MapEditor.Instance.ShiftY, MapEditor.Instance.Width, MapEditor.Instance.Height);
        }

        public Rectangle GetScreen()
        {
            if (screen == null) Cache();
            return screen;
        }

        public bool IsObjectOnScreen(int x, int y, int width, int height)
        {
            if (MapEditor.Instance.Zoom == 1.0)
                return !(x > MapEditor.Instance.ShiftX + MapEditor.Instance.Width
                || x + width < MapEditor.Instance.ShiftX
                || y > MapEditor.Instance.ShiftY + MapEditor.Instance.Height
                || y + height < MapEditor.Instance.ShiftY);
            else
                return !(x * MapEditor.Instance.Zoom > MapEditor.Instance.ShiftX + MapEditor.Instance.Width
                || (x + width) * MapEditor.Instance.Zoom < MapEditor.Instance.ShiftX
                || y * MapEditor.Instance.Zoom > MapEditor.Instance.ShiftY + MapEditor.Instance.Height
                || (y + height) * MapEditor.Instance.Zoom < MapEditor.Instance.ShiftY);
        }

        public void DrawBitmap(Texture image, int x, int y, int width, int height, bool selected, int transparency)
        {
            if (!IsObjectOnScreen(x, y, width, height)) return;

            sprite.Draw(image, new Rectangle(0, 0, width, height), Vector3.Empty, new Vector3(x - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom), y - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom), 0), (selected) ? Color.BlueViolet : Color.FromArgb(transparency, Color.White));
        }

        public void DrawBitmap(Texture image, int x, int y, Rectangle size, bool selected, int transparency)
        {
            if (!IsObjectOnScreen(x, y, size.Width, size.Height)) return;
 
            if (MapEditor.Instance.Zoom == 1.0)
                sprite.Draw(image, size, Vector3.Empty, new Vector3(x - MapEditor.Instance.ShiftX, y - MapEditor.Instance.ShiftY, 0), (selected) ? Color.BlueViolet : ColorsCaching.Get(transparency));
            else
                sprite.Draw(image, size, Vector3.Empty, new Vector3(x - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom), y - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom), 0), (selected) ? Color.BlueViolet : ColorsCaching.Get(transparency));
        }

        public void DrawCircle(int x, int y, Color color)
        {
            if (!IsObjectOnScreen(x - 4, y - 4, 9, 9)) return;
            unsafe
            {
                if ((int)tcircle.UnmanagedComPointer == 0x0)
                {
                    tcircle = new Texture(_device, tcircleb, Usage.Dynamic, Pool.Default);
                }
            }

            sprite.Draw(tcircle, new Rectangle(0, 0, 9, 9), new Vector3(0, 0, 0), new Vector3(x - 4 - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom), y - 4 - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom), 0), color);
        }

        public void DrawEmptyCircle(int x, int y, Color color)
        {
            if (!IsObjectOnScreen(x - 6, y - 6, 14, 14)) return;
            unsafe
            {
                if ((int)tecircle.UnmanagedComPointer == 0x0)
                {
                    tecircle = new Texture(_device, tecircleb, Usage.Dynamic, Pool.Default);
                }
            }

            sprite.Draw(tecircle, new Rectangle(0, 0, 14, 14), new Vector3(0, 0, 0), new Vector3(x - 6 - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom), y - 6 - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom), 0), color);
        }

        public void DrawLine(int X1, int Y1, int X2, int Y2, Color color)
        {
            int width = Math.Abs(X2 - X1);
            int height = Math.Abs(Y2 - Y1);
            int x = Math.Min(X1, X2);
            int y = Math.Min(Y1, Y2);

            if (!IsObjectOnScreen(x, y, width, height)) return;
            unsafe
            {
                if ((int)tx.UnmanagedComPointer == 0x0)
                {
                    tx = new Texture(_device, txb, Usage.Dynamic, Pool.Default);
                }
            }

            sprite.Transform = Matrix.Scaling(1f, 1f, 1f); 
            if (width == 0 || height == 0)
            {
                if (width == 0) width = 1;
                else width = (int)(width * MapEditor.Instance.Zoom);
                if (height == 0) height = 1;
                else height = (int)(height * MapEditor.Instance.Zoom);
                sprite.Draw(tx, new Rectangle(0, 0, width, height), new Vector3(0, 0, 0), new Vector3((int)((x - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), (int)((y - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), 0), color);
            }
            else
            {
                DrawLinePBP(X1, Y1, X2, Y2, color);
            }
            sprite.Transform = Matrix.Scaling((float)MapEditor.Instance.Zoom, (float)MapEditor.Instance.Zoom, 1f); 
        }

        public void DrawArrow(int x0, int y0, int x1, int y1, Color color)
        {
            int x2, y2, x3, y3;

            double angle = Math.Atan2(y1 - y0, x1 - x0) + Math.PI;

            x2 = (int)(x1 + 10 * Math.Cos(angle - Math.PI / 8));
            y2 = (int)(y1 + 10 * Math.Sin(angle - Math.PI / 8));
            x3 = (int)(x1 + 10 * Math.Cos(angle + Math.PI / 8));
            y3 = (int)(y1 + 10 * Math.Sin(angle + Math.PI / 8));

            DrawLine(x1, y1, x0, y0, color);
            DrawLine(x1, y1, x2, y2, color);
            DrawLine(x1, y1, x3, y3, color);
        }
        void DrawLinePBP(int x0, int y0, int x1, int y1, Color color)
        {
            int dx, dy, inx, iny, e;

            dx = x1 - x0;
            dy = y1 - y0;
            inx = dx > 0 ? 1 : -1;
            iny = dy > 0 ? 1 : -1;

            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            if (dx >= dy)
            {
                dy <<= 1;
                e = dy - dx;
                dx <<= 1;
                while (x0 != x1)
                {
                    sprite.Draw(tx, new Rectangle(0, 0, 1, 1), new Vector3(0, 0, 0), new Vector3((int)((x0 - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), (int)((y0 - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), 0), color);
                    if (e >= 0)
                    {
                        y0 += iny;
                        e -= dx;
                    }
                    e += dy; x0 += inx;
                }
            }
            else
            {
                dx <<= 1;
                e = dx - dy;
                dy <<= 1;
                while (y0 != y1)
                {
                    sprite.Draw(tx, new Rectangle(0, 0, 1, 1), new Vector3(0, 0, 0), new Vector3((int)((x0 - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), (int)((y0 - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), 0), color);
                    if (e >= 0)
                    {
                        x0 += inx;
                        e -= dy;
                    }
                    e += dx; y0 += iny;
                }
            }
            sprite.Draw(tx, new Rectangle(0, 0, 1, 1), new Vector3(0, 0, 0), new Vector3((int)((x0 - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), (int)((y0 - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom)) * MapEditor.Instance.Zoom), 0), color);
        }

        public void DrawRectangle(int x1, int y1, int x2, int y2, Color color)
        {
            if (!IsObjectOnScreen(x1, y1, x2 - x1, y2 - y1)) return;
            unsafe
            {
                if ((int)tx.UnmanagedComPointer == 0x0)
                {
                    tx = new Texture(_device, txb, Usage.Dynamic, Pool.Default);
                }
            }

            sprite.Draw(tx, new Rectangle(0, 0, x2 - x1, y2 - y1), new Vector3(0, 0, 0), new Vector3(x1 - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom), y1 - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom), 0), color);
        }
        public void DrawText(string text, int x, int y, int width, Color color, bool bold)
        {
            if (width >= 30)
            {
                ((bold) ? fontBold : font).DrawText(sprite, text, new Rectangle(x - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom), y - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom), width, 1000), DrawTextFormat.WordBreak, color);
            }
            else
            {
                ((bold) ? fontBold : font).DrawText(sprite, text, x - (int)(MapEditor.Instance.ShiftX / MapEditor.Instance.Zoom), y - (int)(MapEditor.Instance.ShiftY / MapEditor.Instance.Zoom), color);
            }
        }
        public void Draw2DCursor(int x, int y)
        {
            unsafe
            {
                if ((int)hvcursor.UnmanagedComPointer == 0x0)
                {
                    hvcursor = new Texture(_device, hvcursorb, Usage.Dynamic, Pool.Default);
                }
            }

            sprite2.Draw(hvcursor, new Rectangle(Point.Empty, Cursors.NoMove2D.Size), new Vector3(0, 0, 0), new Vector3(x - 16, y - 15, 0), Color.White);
        }
        public void DrawHorizCursor(int x, int y)
        {
            unsafe
            {
                if ((int)hcursor.UnmanagedComPointer == 0x0)
                {
                    hcursor = new Texture(_device, hcursorb, Usage.Dynamic, Pool.Default);
                }
            }

            sprite2.Draw(hcursor, new Rectangle(Point.Empty, Cursors.NoMove2D.Size), new Vector3(0, 0, 0), new Vector3(x - 16, y - 15, 0), Color.White);
        }

        public void DrawVertCursor(int x, int y)
        {
            unsafe
            {
                if ((int)vcursor.UnmanagedComPointer == 0x0)
                {
                    vcursor = new Texture(_device, vcursorb, Usage.Dynamic, Pool.Default);
                }
            }

            sprite2.Draw(vcursor, new Rectangle(Point.Empty, Cursors.NoMove2D.Size), new Vector3(0, 0, 0), new Vector3(x - 16, y - 15, 0), Color.White);
        }

        public void OnMouseMoveEventCreate()
        {
            Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y);
        }
    }
}
