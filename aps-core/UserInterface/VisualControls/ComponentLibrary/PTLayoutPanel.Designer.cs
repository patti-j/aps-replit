namespace PT.ComponentLibrary
{
    partial class PTLayoutPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.stackPanel_Layout = new DevExpress.Utils.Layout.StackPanel();
            ((System.ComponentModel.ISupportInitialize)(this.stackPanel_Layout)).BeginInit();
            this.SuspendLayout();
            // 
            // stackPanel_Layout
            // 
            this.stackPanel_Layout.AutoSize = true;
            this.stackPanel_Layout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.stackPanel_Layout.Location = new System.Drawing.Point(0, 0);
            this.stackPanel_Layout.Name = "stackPanel_Layout";
            this.stackPanel_Layout.Size = new System.Drawing.Size(0, 0);
            this.stackPanel_Layout.TabIndex = 0;
            // 
            // PTLayoutPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.stackPanel_Layout);
            this.Name = "PTLayoutPanel";
            this.Size = new System.Drawing.Size(3, 3);
            ((System.ComponentModel.ISupportInitialize)(this.stackPanel_Layout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.Utils.Layout.StackPanel stackPanel_Layout;
    }
}
