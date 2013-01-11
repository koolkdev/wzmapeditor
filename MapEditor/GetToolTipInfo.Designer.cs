namespace WZMapEditor
{
    partial class GetToolTipInfo
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
            this.label3 = new System.Windows.Forms.Label();
            this.Title = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Desc = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Description:";
            // 
            // Title
            // 
            this.Title.Location = new System.Drawing.Point(48, 9);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(168, 20);
            this.Title.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Title:";
            // 
            // Desc
            // 
            this.Desc.Location = new System.Drawing.Point(15, 48);
            this.Desc.Name = "Desc";
            this.Desc.Size = new System.Drawing.Size(330, 98);
            this.Desc.TabIndex = 12;
            this.Desc.Text = "";
            // 
            // GetToolTipInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(364, 162);
            this.Controls.Add(this.Desc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Title);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetToolTipInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ToolTip";
            this.Load += new System.EventHandler(this.GetToolTipInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox Title;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox Desc;
    }
}