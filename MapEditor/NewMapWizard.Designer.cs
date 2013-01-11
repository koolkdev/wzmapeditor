namespace WZMapEditor
{
    partial class NewMapWizard
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
            this.label2 = new System.Windows.Forms.Label();
            this.StreetName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.MapName = new System.Windows.Forms.TextBox();
            this.MarkPreview = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BGMsList = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.BackgroundPreview = new System.Windows.Forms.PictureBox();
            this.SelectBackground = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.MapWidth = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.Create = new System.Windows.Forms.Button();
            this.IsReturnMap = new System.Windows.Forms.CheckBox();
            this.MapHeight = new System.Windows.Forms.TextBox();
            this.MapGroup = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.IsTown = new System.Windows.Forms.CheckBox();
            this.IsSwim = new System.Windows.Forms.CheckBox();
            this.ReturnMap = new WZMapEditor.NumericTextBox();
            this.MapID = new WZMapEditor.NumericTextBox();
            this.IsMiniMap = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.MarkPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackgroundPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Map ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Street Name:";
            // 
            // StreetName
            // 
            this.StreetName.Location = new System.Drawing.Point(87, 38);
            this.StreetName.Name = "StreetName";
            this.StreetName.Size = new System.Drawing.Size(100, 20);
            this.StreetName.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Map Name:";
            // 
            // MapName
            // 
            this.MapName.Location = new System.Drawing.Point(80, 64);
            this.MapName.Name = "MapName";
            this.MapName.Size = new System.Drawing.Size(100, 20);
            this.MapName.TabIndex = 5;
            // 
            // MarkPreview
            // 
            this.MarkPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MarkPreview.Location = new System.Drawing.Point(76, 90);
            this.MarkPreview.Name = "MarkPreview";
            this.MarkPreview.Size = new System.Drawing.Size(40, 40);
            this.MarkPreview.TabIndex = 6;
            this.MarkPreview.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Map Mark:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 137);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Return Map:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 163);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Background Music:";
            // 
            // BGMsList
            // 
            this.BGMsList.FormattingEnabled = true;
            this.BGMsList.Location = new System.Drawing.Point(117, 160);
            this.BGMsList.Name = "BGMsList";
            this.BGMsList.Size = new System.Drawing.Size(161, 21);
            this.BGMsList.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(281, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Background:";
            // 
            // BackgroundPreview
            // 
            this.BackgroundPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BackgroundPreview.Location = new System.Drawing.Point(284, 41);
            this.BackgroundPreview.Name = "BackgroundPreview";
            this.BackgroundPreview.Size = new System.Drawing.Size(162, 122);
            this.BackgroundPreview.TabIndex = 13;
            this.BackgroundPreview.TabStop = false;
            // 
            // SelectBackground
            // 
            this.SelectBackground.Location = new System.Drawing.Point(394, 10);
            this.SelectBackground.Name = "SelectBackground";
            this.SelectBackground.Size = new System.Drawing.Size(52, 23);
            this.SelectBackground.TabIndex = 14;
            this.SelectBackground.Text = "Select";
            this.SelectBackground.UseVisualStyleBackColor = true;
            this.SelectBackground.Click += new System.EventHandler(this.SelectBackground_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 194);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Width:";
            // 
            // MapWidth
            // 
            this.MapWidth.Location = new System.Drawing.Point(56, 187);
            this.MapWidth.MaxLength = 5;
            this.MapWidth.Name = "MapWidth";
            this.MapWidth.Size = new System.Drawing.Size(42, 20);
            this.MapWidth.TabIndex = 16;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(104, 190);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Height:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(120, 90);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(52, 23);
            this.button1.TabIndex = 19;
            this.button1.Text = "Select";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Create
            // 
            this.Create.Location = new System.Drawing.Point(394, 185);
            this.Create.Name = "Create";
            this.Create.Size = new System.Drawing.Size(52, 23);
            this.Create.TabIndex = 20;
            this.Create.Text = "Create";
            this.Create.UseVisualStyleBackColor = true;
            this.Create.Click += new System.EventHandler(this.Create_Click);
            // 
            // IsReturnMap
            // 
            this.IsReturnMap.AutoSize = true;
            this.IsReturnMap.Location = new System.Drawing.Point(150, 137);
            this.IsReturnMap.Name = "IsReturnMap";
            this.IsReturnMap.Size = new System.Drawing.Size(15, 14);
            this.IsReturnMap.TabIndex = 21;
            this.IsReturnMap.UseVisualStyleBackColor = true;
            this.IsReturnMap.CheckedChanged += new System.EventHandler(this.IsReturnMap_CheckedChanged);
            // 
            // MapHeight
            // 
            this.MapHeight.Location = new System.Drawing.Point(151, 187);
            this.MapHeight.MaxLength = 5;
            this.MapHeight.Name = "MapHeight";
            this.MapHeight.Size = new System.Drawing.Size(42, 20);
            this.MapHeight.TabIndex = 22;
            // 
            // MapGroup
            // 
            this.MapGroup.FormattingEnabled = true;
            this.MapGroup.Location = new System.Drawing.Point(268, 187);
            this.MapGroup.Name = "MapGroup";
            this.MapGroup.Size = new System.Drawing.Size(106, 21);
            this.MapGroup.TabIndex = 23;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(199, 190);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(63, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "Map Group:";
            // 
            // IsTown
            // 
            this.IsTown.AutoSize = true;
            this.IsTown.Location = new System.Drawing.Point(209, 12);
            this.IsTown.Name = "IsTown";
            this.IsTown.Size = new System.Drawing.Size(53, 17);
            this.IsTown.TabIndex = 25;
            this.IsTown.Text = "Town";
            this.IsTown.UseVisualStyleBackColor = true;
            // 
            // IsSwim
            // 
            this.IsSwim.AutoSize = true;
            this.IsSwim.Location = new System.Drawing.Point(208, 35);
            this.IsSwim.Name = "IsSwim";
            this.IsSwim.Size = new System.Drawing.Size(51, 17);
            this.IsSwim.TabIndex = 26;
            this.IsSwim.Text = "Swim";
            this.IsSwim.UseVisualStyleBackColor = true;
            // 
            // ReturnMap
            // 
            this.ReturnMap.Enabled = false;
            this.ReturnMap.Location = new System.Drawing.Point(84, 134);
            this.ReturnMap.MaxLength = 9;
            this.ReturnMap.Name = "ReturnMap";
            this.ReturnMap.Size = new System.Drawing.Size(60, 20);
            this.ReturnMap.TabIndex = 27;
            this.ReturnMap.Text = "999999999";
            // 
            // MapID
            // 
            this.MapID.Location = new System.Drawing.Point(63, 12);
            this.MapID.MaxLength = 9;
            this.MapID.Name = "MapID";
            this.MapID.Size = new System.Drawing.Size(60, 20);
            this.MapID.TabIndex = 2;
            // 
            // IsMiniMap
            // 
            this.IsMiniMap.AutoSize = true;
            this.IsMiniMap.Checked = true;
            this.IsMiniMap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IsMiniMap.Location = new System.Drawing.Point(208, 58);
            this.IsMiniMap.Name = "IsMiniMap";
            this.IsMiniMap.Size = new System.Drawing.Size(66, 17);
            this.IsMiniMap.TabIndex = 43;
            this.IsMiniMap.Text = "MiniMap";
            this.IsMiniMap.UseVisualStyleBackColor = true;
            // 
            // NewMapWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 225);
            this.Controls.Add(this.IsMiniMap);
            this.Controls.Add(this.ReturnMap);
            this.Controls.Add(this.IsSwim);
            this.Controls.Add(this.IsTown);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.MapGroup);
            this.Controls.Add(this.MapHeight);
            this.Controls.Add(this.IsReturnMap);
            this.Controls.Add(this.Create);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.MapWidth);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.SelectBackground);
            this.Controls.Add(this.BackgroundPreview);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.BGMsList);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.MarkPreview);
            this.Controls.Add(this.MapName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.StreetName);
            this.Controls.Add(this.MapID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewMapWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "NewMapWizard";
            this.Load += new System.EventHandler(this.NewMapWizard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.MarkPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackgroundPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox MarkPreview;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox BackgroundPreview;
        private System.Windows.Forms.Button SelectBackground;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button Create;
        private System.Windows.Forms.Label label10;
        public NumericTextBox MapID;
        public System.Windows.Forms.TextBox StreetName;
        public System.Windows.Forms.TextBox MapName;
        public System.Windows.Forms.ComboBox BGMsList;
        public System.Windows.Forms.TextBox MapWidth;
        public System.Windows.Forms.CheckBox IsReturnMap;
        public System.Windows.Forms.TextBox MapHeight;
        public System.Windows.Forms.ComboBox MapGroup;
        public System.Windows.Forms.CheckBox IsTown;
        public System.Windows.Forms.CheckBox IsSwim;
        public NumericTextBox ReturnMap;
        public System.Windows.Forms.CheckBox IsMiniMap;
    }
}