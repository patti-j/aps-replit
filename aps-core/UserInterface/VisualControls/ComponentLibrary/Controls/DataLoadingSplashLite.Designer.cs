using System.Drawing;

using DevExpress.XtraEditors;

namespace PT.ComponentLibrary.Controls
{
    partial class DataLoadingSplashLite
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

            if (pictureBox_LoadingAnimation.Image != null)
                ((Bitmap)pictureBox_LoadingAnimation.Image).Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox_LoadingAnimation = new DevExpress.XtraEditors.PictureEdit();
            this.labelControl_LoadingText = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LoadingAnimation.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox_LoadingAnimation
            // 
            this.pictureBox_LoadingAnimation.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox_LoadingAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_LoadingAnimation.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_LoadingAnimation.Name = "pictureBox_LoadingAnimation";
            this.pictureBox_LoadingAnimation.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_LoadingAnimation.Properties.Appearance.Options.UseBackColor = true;
            this.pictureBox_LoadingAnimation.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictureBox_LoadingAnimation.Properties.ReadOnly = true;
            this.pictureBox_LoadingAnimation.Properties.ShowMenu = false;
            this.pictureBox_LoadingAnimation.Properties.ShowZoomSubMenu = DevExpress.Utils.DefaultBoolean.False;
            this.pictureBox_LoadingAnimation.Properties.ZoomPercent = 70D;
            this.pictureBox_LoadingAnimation.Size = new System.Drawing.Size(73, 61);
            this.pictureBox_LoadingAnimation.TabIndex = 5;
            // 
            // labelControl_LoadingText
            // 
            this.labelControl_LoadingText.Appearance.Options.UseTextOptions = true;
            this.labelControl_LoadingText.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl_LoadingText.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl_LoadingText.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelControl_LoadingText.Location = new System.Drawing.Point(0, 61);
            this.labelControl_LoadingText.Name = "labelControl_LoadingText";
            this.labelControl_LoadingText.Size = new System.Drawing.Size(73, 13);
            this.labelControl_LoadingText.TabIndex = 6;
            this.labelControl_LoadingText.Text = "Loading...";
            // 
            // DataLoadingSplashLite
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox_LoadingAnimation);
            this.Controls.Add(this.labelControl_LoadingText);
            this.Name = "DataLoadingSplashLite";
            this.Size = new System.Drawing.Size(73, 74);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LoadingAnimation.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private PictureEdit pictureBox_LoadingAnimation;
        private LabelControl labelControl_LoadingText;
    }
}
