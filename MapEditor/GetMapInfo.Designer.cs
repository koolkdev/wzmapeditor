namespace WZMapEditor
{
    partial class GetMapInfo
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
            this.IsSwim = new System.Windows.Forms.CheckBox();
            this.IsTown = new System.Windows.Forms.CheckBox();
            this.IsReturnMap = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SelectBackground = new System.Windows.Forms.Button();
            this.BackgroundPreview = new System.Windows.Forms.PictureBox();
            this.label7 = new System.Windows.Forms.Label();
            this.BGMsList = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.MarkPreview = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.IsMiniMap = new System.Windows.Forms.CheckBox();
            this.ReturnMap = new WZMapEditor.NumericTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BackgroundPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MarkPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // IsSwim
            // 
            this.IsSwim.AutoSize = true;
            this.IsSwim.Location = new System.Drawing.Point(212, 34);
            this.IsSwim.Name = "IsSwim";
            this.IsSwim.Size = new System.Drawing.Size(51, 17);
            this.IsSwim.TabIndex = 39;
            this.IsSwim.Text = "Swim";
            this.IsSwim.UseVisualStyleBackColor = true;
            // 
            // IsTown
            // 
            this.IsTown.AutoSize = true;
            this.IsTown.Location = new System.Drawing.Point(212, 11);
            this.IsTown.Name = "IsTown";
            this.IsTown.Size = new System.Drawing.Size(53, 17);
            this.IsTown.TabIndex = 38;
            this.IsTown.Text = "Town";
            this.IsTown.UseVisualStyleBackColor = true;
            // 
            // IsReturnMap
            // 
            this.IsReturnMap.AutoSize = true;
            this.IsReturnMap.Location = new System.Drawing.Point(150, 61);
            this.IsReturnMap.Name = "IsReturnMap";
            this.IsReturnMap.Size = new System.Drawing.Size(15, 14);
            this.IsReturnMap.TabIndex = 37;
            this.IsReturnMap.UseVisualStyleBackColor = true;
            this.IsReturnMap.CheckedChanged += new System.EventHandler(this.IsReturnMap_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(122, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(52, 23);
            this.button1.TabIndex = 36;
            this.button1.Text = "Select";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SelectBackground
            // 
            this.SelectBackground.Location = new System.Drawing.Point(193, 135);
            this.SelectBackground.Name = "SelectBackground";
            this.SelectBackground.Size = new System.Drawing.Size(85, 106);
            this.SelectBackground.TabIndex = 35;
            this.SelectBackground.Text = "Select";
            this.SelectBackground.UseVisualStyleBackColor = true;
            this.SelectBackground.Click += new System.EventHandler(this.SelectBackground_Click);
            // 
            // BackgroundPreview
            // 
            this.BackgroundPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BackgroundPreview.Location = new System.Drawing.Point(15, 119);
            this.BackgroundPreview.Name = "BackgroundPreview";
            this.BackgroundPreview.Size = new System.Drawing.Size(162, 122);
            this.BackgroundPreview.TabIndex = 34;
            this.BackgroundPreview.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(190, 119);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 33;
            this.label7.Text = "Background:";
            // 
            // BGMsList
            // 
            this.BGMsList.FormattingEnabled = true;
            this.BGMsList.Location = new System.Drawing.Point(117, 84);
            this.BGMsList.Name = "BGMsList";
            this.BGMsList.Size = new System.Drawing.Size(161, 21);
            this.BGMsList.TabIndex = 32;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 87);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 31;
            this.label6.Text = "Background Music:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Return Map:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "Map Mark:";
            // 
            // MarkPreview
            // 
            this.MarkPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MarkPreview.Location = new System.Drawing.Point(76, 12);
            this.MarkPreview.Name = "MarkPreview";
            this.MarkPreview.Size = new System.Drawing.Size(40, 40);
            this.MarkPreview.TabIndex = 28;
            this.MarkPreview.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(15, 247);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(263, 23);
            this.button2.TabIndex = 41;
            this.button2.Text = "Close";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // IsMiniMap
            // 
            this.IsMiniMap.AutoSize = true;
            this.IsMiniMap.Enabled = false;
            this.IsMiniMap.Location = new System.Drawing.Point(212, 57);
            this.IsMiniMap.Name = "IsMiniMap";
            this.IsMiniMap.Size = new System.Drawing.Size(66, 17);
            this.IsMiniMap.TabIndex = 42;
            this.IsMiniMap.Text = "MiniMap";
            this.IsMiniMap.UseVisualStyleBackColor = true;
            // 
            // ReturnMap
            // 
            this.ReturnMap.Location = new System.Drawing.Point(84, 58);
            this.ReturnMap.MaxLength = 9;
            this.ReturnMap.Name = "ReturnMap";
            this.ReturnMap.Size = new System.Drawing.Size(60, 20);
            this.ReturnMap.TabIndex = 40;
            // 
            // GetMapInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 282);
            this.ControlBox = false;
            this.Controls.Add(this.IsMiniMap);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.ReturnMap);
            this.Controls.Add(this.IsSwim);
            this.Controls.Add(this.IsTown);
            this.Controls.Add(this.IsReturnMap);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.SelectBackground);
            this.Controls.Add(this.BackgroundPreview);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.BGMsList);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.MarkPreview);
            this.Name = "GetMapInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "GetMapInfo";
            ((System.ComponentModel.ISupportInitialize)(this.BackgroundPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MarkPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public NumericTextBox ReturnMap;
        public System.Windows.Forms.CheckBox IsSwim;
        public System.Windows.Forms.CheckBox IsTown;
        public System.Windows.Forms.CheckBox IsReturnMap;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button SelectBackground;
        private System.Windows.Forms.PictureBox BackgroundPreview;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.ComboBox BGMsList;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox MarkPreview;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.CheckBox IsMiniMap;

    }
}