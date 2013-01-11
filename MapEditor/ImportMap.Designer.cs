namespace WZMapEditor
{
    partial class ImportMap
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
            this.label10 = new System.Windows.Forms.Label();
            this.MapGroup = new System.Windows.Forms.ComboBox();
            this.MapName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.StreetName = new System.Windows.Forms.TextBox();
            this.MapID = new WZMapEditor.NumericTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Cancel = new System.Windows.Forms.Button();
            this.Import = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 94);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(63, 13);
            this.label10.TabIndex = 32;
            this.label10.Text = "Map Group:";
            // 
            // MapGroup
            // 
            this.MapGroup.FormattingEnabled = true;
            this.MapGroup.Location = new System.Drawing.Point(85, 91);
            this.MapGroup.Name = "MapGroup";
            this.MapGroup.Size = new System.Drawing.Size(106, 21);
            this.MapGroup.TabIndex = 31;
            // 
            // MapName
            // 
            this.MapName.Location = new System.Drawing.Point(84, 65);
            this.MapName.Name = "MapName";
            this.MapName.Size = new System.Drawing.Size(100, 20);
            this.MapName.TabIndex = 30;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Map Name:";
            // 
            // StreetName
            // 
            this.StreetName.Location = new System.Drawing.Point(91, 39);
            this.StreetName.Name = "StreetName";
            this.StreetName.Size = new System.Drawing.Size(100, 20);
            this.StreetName.TabIndex = 28;
            // 
            // MapID
            // 
            this.MapID.Location = new System.Drawing.Point(67, 13);
            this.MapID.MaxLength = 9;
            this.MapID.Name = "MapID";
            this.MapID.Size = new System.Drawing.Size(60, 20);
            this.MapID.TabIndex = 27;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Street Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Map ID:";
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(12, 118);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 33;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // Import
            // 
            this.Import.Location = new System.Drawing.Point(126, 118);
            this.Import.Name = "Import";
            this.Import.Size = new System.Drawing.Size(75, 23);
            this.Import.TabIndex = 34;
            this.Import.Text = "Import";
            this.Import.UseVisualStyleBackColor = true;
            this.Import.Click += new System.EventHandler(this.Import_Click);
            // 
            // ImportMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(213, 153);
            this.Controls.Add(this.Import);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.MapGroup);
            this.Controls.Add(this.MapName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.StreetName);
            this.Controls.Add(this.MapID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ImportMap";
            this.Text = "ImportMap";
            this.Load += new System.EventHandler(this.ImportMap_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.ComboBox MapGroup;
        public System.Windows.Forms.TextBox MapName;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox StreetName;
        public NumericTextBox MapID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Import;
    }
}