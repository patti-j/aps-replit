namespace MassRecordingsUI
{
    partial class WarningsPopup
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
            this.memoEdit_WarningsDetails = new DevExpress.XtraEditors.MemoEdit();
            this.labelControl_WarningsDetails = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit_WarningsDetails.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // memoEdit_WarningsDetails
            // 
            this.memoEdit_WarningsDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.memoEdit_WarningsDetails.EditValue = "";
            this.memoEdit_WarningsDetails.Location = new System.Drawing.Point(11, 40);
            this.memoEdit_WarningsDetails.Margin = new System.Windows.Forms.Padding(2);
            this.memoEdit_WarningsDetails.Name = "memoEdit_WarningsDetails";
            this.memoEdit_WarningsDetails.Properties.AllowFocused = false;
            this.memoEdit_WarningsDetails.Properties.ReadOnly = true;
            this.memoEdit_WarningsDetails.Size = new System.Drawing.Size(948, 637);
            this.memoEdit_WarningsDetails.TabIndex = 0;
            // 
            // labelControl_WarningsDetails
            // 
            this.labelControl_WarningsDetails.Location = new System.Drawing.Point(11, 11);
            this.labelControl_WarningsDetails.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_WarningsDetails.Name = "labelControl_WarningsDetails";
            this.labelControl_WarningsDetails.Size = new System.Drawing.Size(84, 13);
            this.labelControl_WarningsDetails.TabIndex = 1;
            this.labelControl_WarningsDetails.Text = "Warnings Details:";
            // 
            // WarningsPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 688);
            this.Controls.Add(this.labelControl_WarningsDetails);
            this.Controls.Add(this.memoEdit_WarningsDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "WarningsPopup";
            this.Text = "WarningsPopup";
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit_WarningsDetails.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.MemoEdit memoEdit_WarningsDetails;
        private DevExpress.XtraEditors.LabelControl labelControl_WarningsDetails;
    }
}