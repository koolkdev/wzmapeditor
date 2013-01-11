namespace WZMapEditor
{
    partial class MapBackgroundSelect
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
            this.MapsList = new System.Windows.Forms.ComboBox();
            this.BackgroundPreview = new System.Windows.Forms.PictureBox();
            this.select = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.BackgroundPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // MapsList
            // 
            this.MapsList.FormattingEnabled = true;
            this.MapsList.Location = new System.Drawing.Point(12, 12);
            this.MapsList.Name = "MapsList";
            this.MapsList.Size = new System.Drawing.Size(800, 21);
            this.MapsList.TabIndex = 0;
            this.MapsList.SelectedIndexChanged += new System.EventHandler(this.MapsList_SelectedIndexChanged);
            // 
            // BackgroundPreview
            // 
            this.BackgroundPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BackgroundPreview.Location = new System.Drawing.Point(12, 39);
            this.BackgroundPreview.Name = "BackgroundPreview";
            this.BackgroundPreview.Size = new System.Drawing.Size(800, 600);
            this.BackgroundPreview.TabIndex = 1;
            this.BackgroundPreview.TabStop = false;
            // 
            // select
            // 
            this.select.Enabled = false;
            this.select.Location = new System.Drawing.Point(380, 645);
            this.select.Name = "select";
            this.select.Size = new System.Drawing.Size(75, 23);
            this.select.TabIndex = 2;
            this.select.Text = "Select";
            this.select.UseVisualStyleBackColor = true;
            this.select.Click += new System.EventHandler(this.button1_Click);
            // 
            // MapBackgroundSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 676);
            this.Controls.Add(this.select);
            this.Controls.Add(this.BackgroundPreview);
            this.Controls.Add(this.MapsList);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapBackgroundSelect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Select Background";
            ((System.ComponentModel.ISupportInitialize)(this.BackgroundPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ComboBox MapsList;
        private System.Windows.Forms.PictureBox BackgroundPreview;
        private System.Windows.Forms.Button select;
    }
}