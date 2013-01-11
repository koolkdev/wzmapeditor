namespace WZMapEditor
{
    partial class MapTileSelect
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TilesList = new WZMapEditor.ThumbnailFlowLayoutPanel();
            this.SuspendLayout();
            // 
            // TilesList
            // 
            this.TilesList.AutoScroll = true;
            this.TilesList.BackColor = System.Drawing.Color.White;
            this.TilesList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TilesList.Location = new System.Drawing.Point(12, 12);
            this.TilesList.Name = "TilesList";
            this.TilesList.Size = new System.Drawing.Size(451, 305);
            this.TilesList.TabIndex = 0;
            // 
            // MapTileSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 329);
            this.Controls.Add(this.TilesList);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapTileSelect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "MapTileSelect";
            this.ResumeLayout(false);

        }

        #endregion

        private ThumbnailFlowLayoutPanel TilesList;
    }
}