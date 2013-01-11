namespace WZMapEditor
{
    partial class GetPortalInfo
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
            this.PortalType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.PortalName = new System.Windows.Forms.TextBox();
            this.ToMap = new System.Windows.Forms.TextBox();
            this.ToName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.IsTeleport = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CloseButton = new System.Windows.Forms.Button();
            this.ThisMap = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // PortalType
            // 
            this.PortalType.FormattingEnabled = true;
            this.PortalType.Location = new System.Drawing.Point(12, 12);
            this.PortalType.Name = "PortalType";
            this.PortalType.Size = new System.Drawing.Size(190, 21);
            this.PortalType.TabIndex = 0;
            this.PortalType.SelectedIndexChanged += new System.EventHandler(this.PortalType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name:";
            // 
            // PortalName
            // 
            this.PortalName.Location = new System.Drawing.Point(56, 39);
            this.PortalName.Name = "PortalName";
            this.PortalName.Size = new System.Drawing.Size(100, 20);
            this.PortalName.TabIndex = 2;
            // 
            // ToMap
            // 
            this.ToMap.Location = new System.Drawing.Point(82, 88);
            this.ToMap.Name = "ToMap";
            this.ToMap.Size = new System.Drawing.Size(100, 20);
            this.ToMap.TabIndex = 3;
            // 
            // ToName
            // 
            this.ToName.Location = new System.Drawing.Point(102, 114);
            this.ToName.Name = "ToName";
            this.ToName.Size = new System.Drawing.Size(100, 20);
            this.ToName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "To Map(ID):";
            // 
            // IsTeleport
            // 
            this.IsTeleport.AutoSize = true;
            this.IsTeleport.Location = new System.Drawing.Point(12, 65);
            this.IsTeleport.Name = "IsTeleport";
            this.IsTeleport.Size = new System.Drawing.Size(65, 17);
            this.IsTeleport.TabIndex = 6;
            this.IsTeleport.Text = "Teleport";
            this.IsTeleport.UseVisualStyleBackColor = true;
            this.IsTeleport.CheckedChanged += new System.EventHandler(this.IsTeleport_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "To Portal Name:";
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(78, 140);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(63, 23);
            this.CloseButton.TabIndex = 8;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // ThisMap
            // 
            this.ThisMap.AutoSize = true;
            this.ThisMap.Location = new System.Drawing.Point(112, 65);
            this.ThisMap.Name = "ThisMap";
            this.ThisMap.Size = new System.Drawing.Size(70, 17);
            this.ThisMap.TabIndex = 9;
            this.ThisMap.Text = "This Map";
            this.ThisMap.UseVisualStyleBackColor = true;
            this.ThisMap.CheckedChanged += new System.EventHandler(this.ThisMap_CheckedChanged);
            // 
            // GetPortalInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(215, 175);
            this.ControlBox = false;
            this.Controls.Add(this.ThisMap);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.IsTeleport);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ToName);
            this.Controls.Add(this.ToMap);
            this.Controls.Add(this.PortalName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PortalType);
            this.Name = "GetPortalInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "GetPortalInfo";
            this.Load += new System.EventHandler(this.GetPortalInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button CloseButton;
        public System.Windows.Forms.ComboBox PortalType;
        public System.Windows.Forms.TextBox PortalName;
        public System.Windows.Forms.TextBox ToMap;
        public System.Windows.Forms.TextBox ToName;
        public System.Windows.Forms.CheckBox IsTeleport;
        public System.Windows.Forms.CheckBox ThisMap;
    }
}