namespace PT.ComponentLibrary.Forms
{
    partial class AutoFadeForm
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
            this.m_autoCloseTimer = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // m_autoCloseTimer
            // 
            this.m_autoCloseTimer.Tick += new System.EventHandler(this.m_autoCloseTimer_Tick);
            // 
            // AutoFadeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.None;
            this.Name = "AutoFadeForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "AutoFadeForm";
            this.MouseEnter += new System.EventHandler(this.AutoFadeForm_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.AutoFadeForm_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AutoFadeForm_MouseMove);
            this.Move += new System.EventHandler(this.AutoFadeForm_Move);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer m_autoCloseTimer;
    }
}