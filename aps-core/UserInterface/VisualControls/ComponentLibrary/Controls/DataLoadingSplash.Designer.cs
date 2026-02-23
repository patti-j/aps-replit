using System.Drawing;

using DevExpress.XtraEditors;

namespace PT.ComponentLibrary.Controls
{
    partial class DataLoadingSplash
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
            this.ultraPanel_Main = new DevExpress.XtraEditors.PanelControl();
            this.pictureBox_LoadingAnimation = new DevExpress.XtraEditors.PictureEdit();
            this.ultraButton_Cancel = new DevExpress.XtraEditors.SimpleButton();
            this.ultraLabel_LoadingText = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.ultraPanel_Main)).BeginInit();
            this.ultraPanel_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LoadingAnimation.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanel_Main
            // 
            this.ultraPanel_Main.Controls.Add(this.pictureBox_LoadingAnimation);
            this.ultraPanel_Main.Controls.Add(this.ultraButton_Cancel);
            this.ultraPanel_Main.Controls.Add(this.ultraLabel_LoadingText);
            this.ultraPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel_Main.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel_Main.Name = "ultraPanel_Main";
            this.ultraPanel_Main.Size = new System.Drawing.Size(233, 162);
            this.ultraPanel_Main.TabIndex = 0;
            // 
            // pictureBox_LoadingAnimation
            // 
            this.pictureBox_LoadingAnimation.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox_LoadingAnimation.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox_LoadingAnimation.Location = new System.Drawing.Point(13, 6);
            this.pictureBox_LoadingAnimation.Name = "pictureBox_LoadingAnimation";
            this.pictureBox_LoadingAnimation.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_LoadingAnimation.Properties.Appearance.Options.UseBackColor = true;
            this.pictureBox_LoadingAnimation.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictureBox_LoadingAnimation.Properties.InitialImage = null;
            this.pictureBox_LoadingAnimation.Properties.ReadOnly = true;
            this.pictureBox_LoadingAnimation.Properties.ShowMenu = false;
            this.pictureBox_LoadingAnimation.Properties.ShowZoomSubMenu = DevExpress.Utils.DefaultBoolean.False;
            this.pictureBox_LoadingAnimation.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.pictureBox_LoadingAnimation.Properties.ZoomAccelerationFactor = 1D;
            this.pictureBox_LoadingAnimation.Size = new System.Drawing.Size(206, 75);
            this.pictureBox_LoadingAnimation.TabIndex = 5;
            // 
            // ultraButton_Cancel
            // 
            this.ultraButton_Cancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ultraButton_Cancel.Location = new System.Drawing.Point(57, 87);
            this.ultraButton_Cancel.Name = "ultraButton_Cancel";
            this.ultraButton_Cancel.Size = new System.Drawing.Size(120, 23);
            this.ultraButton_Cancel.TabIndex = 4;
            this.ultraButton_Cancel.Text = "Cancel";
            this.ultraButton_Cancel.Click += new System.EventHandler(this.ultraButton_Cancel_Click);
            // 
            // ultraLabel_LoadingText
            // 
            this.ultraLabel_LoadingText.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ultraLabel_LoadingText.Appearance.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabel_LoadingText.Appearance.Options.UseFont = true;
            this.ultraLabel_LoadingText.Appearance.Options.UseTextOptions = true;
            this.ultraLabel_LoadingText.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ultraLabel_LoadingText.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.ultraLabel_LoadingText.Location = new System.Drawing.Point(13, 116);
            this.ultraLabel_LoadingText.Name = "ultraLabel_LoadingText";
            this.ultraLabel_LoadingText.Size = new System.Drawing.Size(206, 14);
            this.ultraLabel_LoadingText.TabIndex = 1;
            this.ultraLabel_LoadingText.Text = "Loading Data...";
            // 
            // DataLoadingSplash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ultraPanel_Main);
            this.Name = "DataLoadingSplash";
            this.Size = new System.Drawing.Size(233, 162);
            ((System.ComponentModel.ISupportInitialize)(this.ultraPanel_Main)).EndInit();
            this.ultraPanel_Main.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LoadingAnimation.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PanelControl ultraPanel_Main;
        private LabelControl ultraLabel_LoadingText;
        private SimpleButton ultraButton_Cancel;
        private PictureEdit pictureBox_LoadingAnimation;
    }
}
