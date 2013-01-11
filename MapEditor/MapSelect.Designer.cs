namespace WZMapEditor
{
    partial class MapSelect
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
            this.label1 = new System.Windows.Forms.Label();
            this.select = new System.Windows.Forms.Button();
            this.MapList = new System.Windows.Forms.ListBox();
            this.Search = new System.Windows.Forms.TextBox();
            this.label = new System.Windows.Forms.Label();
            this.MapName = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Preview = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Preview)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(261, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Map Preview:";
            // 
            // select
            // 
            this.select.Enabled = false;
            this.select.Location = new System.Drawing.Point(297, 231);
            this.select.Name = "select";
            this.select.Size = new System.Drawing.Size(105, 28);
            this.select.TabIndex = 3;
            this.select.Text = "Select";
            this.select.UseVisualStyleBackColor = true;
            this.select.Click += new System.EventHandler(this.select_Click);
            // 
            // MapList
            // 
            this.MapList.FormattingEnabled = true;
            this.MapList.Location = new System.Drawing.Point(12, 38);
            this.MapList.Name = "MapList";
            this.MapList.Size = new System.Drawing.Size(243, 225);
            this.MapList.TabIndex = 4;
            this.MapList.SelectedIndexChanged += new System.EventHandler(this.MapList_SelectedIndexChanged);
            this.MapList.DoubleClick += new System.EventHandler(this.MapList_DoubleClick);
            // 
            // Search
            // 
            this.Search.Location = new System.Drawing.Point(130, 12);
            this.Search.Name = "Search";
            this.Search.Size = new System.Drawing.Size(125, 20);
            this.Search.TabIndex = 4;
            this.Search.TextChanged += new System.EventHandler(this.Search_TextChanged);
            this.Search.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Search_KeyDown);
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(12, 15);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(112, 13);
            this.label.TabIndex = 6;
            this.label.Text = "Search: (By ID/Name)";
            // 
            // MapName
            // 
            this.MapName.AutoSize = true;
            this.MapName.Location = new System.Drawing.Point(261, 22);
            this.MapName.Name = "MapName";
            this.MapName.Size = new System.Drawing.Size(0, 13);
            this.MapName.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.Preview);
            this.panel1.Location = new System.Drawing.Point(264, 38);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(179, 187);
            this.panel1.TabIndex = 5;
            // 
            // Preview
            // 
            this.Preview.Location = new System.Drawing.Point(3, 3);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(172, 180);
            this.Preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.Preview.TabIndex = 3;
            this.Preview.TabStop = false;
            // 
            // MapSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 272);
            this.Controls.Add(this.MapName);
            this.Controls.Add(this.label);
            this.Controls.Add(this.Search);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.MapList);
            this.Controls.Add(this.select);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapSelect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Select Map";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Preview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button select;
        private System.Windows.Forms.ListBox MapList;
        private System.Windows.Forms.TextBox Search;
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Label MapName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox Preview;
    }
}