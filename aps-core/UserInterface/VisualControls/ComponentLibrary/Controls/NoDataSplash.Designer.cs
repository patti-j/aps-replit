
namespace PT.ComponentLibrary.Controls
{
    partial class NoDataSplash
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
            panelControl1 = new DevExpress.XtraEditors.PanelControl();
            label_Text = new DevExpress.XtraEditors.LabelControl();
            pictureBox_NoDataImage = new DevExpress.XtraEditors.PictureEdit();
            ((System.ComponentModel.ISupportInitialize)panelControl1).BeginInit();
            panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_NoDataImage.Properties).BeginInit();
            SuspendLayout();
            // 
            // panelControl1
            // 
            panelControl1.Controls.Add(label_Text);
            panelControl1.Controls.Add(pictureBox_NoDataImage);
            panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            panelControl1.Location = new System.Drawing.Point(0, 0);
            panelControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelControl1.Name = "panelControl1";
            panelControl1.Size = new System.Drawing.Size(272, 187);
            panelControl1.TabIndex = 0;
            // 
            // label_Text
            // 
            label_Text.Anchor = System.Windows.Forms.AnchorStyles.None;
            label_Text.Appearance.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label_Text.Appearance.Options.UseFont = true;
            label_Text.Appearance.Options.UseTextOptions = true;
            label_Text.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            label_Text.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            label_Text.Location = new System.Drawing.Point(15, 100);
            label_Text.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            label_Text.Name = "label_Text";
            label_Text.Size = new System.Drawing.Size(240, 28);
            label_Text.TabIndex = 7;
            label_Text.Text = "This Control Requires Data In Order To Function";
            // 
            // pictureBox_NoDataImage
            // 
            pictureBox_NoDataImage.Anchor = System.Windows.Forms.AnchorStyles.None;
            pictureBox_NoDataImage.Location = new System.Drawing.Point(40, 20);
            pictureBox_NoDataImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox_NoDataImage.Name = "pictureBox_NoDataImage";
            pictureBox_NoDataImage.Properties.AllowFocused = false;
            pictureBox_NoDataImage.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            pictureBox_NoDataImage.Properties.Appearance.Options.UseBackColor = true;
            pictureBox_NoDataImage.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pictureBox_NoDataImage.Properties.ReadOnly = true;
            pictureBox_NoDataImage.Properties.ShowMenu = false;
            pictureBox_NoDataImage.Properties.ShowZoomSubMenu = DevExpress.Utils.DefaultBoolean.False;
            pictureBox_NoDataImage.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            pictureBox_NoDataImage.Size = new System.Drawing.Size(189, 58);
            pictureBox_NoDataImage.TabIndex = 6;
            // 
            // NoDataSplash
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panelControl1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "NoDataSplash";
            Size = new System.Drawing.Size(272, 187);
            ((System.ComponentModel.ISupportInitialize)panelControl1).EndInit();
            panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox_NoDataImage.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PictureEdit pictureBox_NoDataImage;
        private DevExpress.XtraEditors.LabelControl label_Text;
    }
}
