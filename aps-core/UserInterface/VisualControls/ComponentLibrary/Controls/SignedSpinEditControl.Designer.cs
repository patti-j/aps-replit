
namespace PT.ComponentLibrary.Controls
{
    partial class SignedSpinEditControl
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
            DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
            DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
            stackPanel1 = new DevExpress.Utils.Layout.StackPanel();
            simpleButton_ChangeSign = new DevExpress.XtraEditors.SimpleButton();
            spinEdit1 = new ScrollableSpinEdit();
            ((System.ComponentModel.ISupportInitialize)stackPanel1).BeginInit();
            stackPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)spinEdit1.Properties).BeginInit();
            SuspendLayout();
            // 
            // stackPanel1
            // 
            stackPanel1.Controls.Add(simpleButton_ChangeSign);
            stackPanel1.Controls.Add(spinEdit1);
            stackPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            stackPanel1.Location = new System.Drawing.Point(0, 0);
            stackPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            stackPanel1.Name = "stackPanel1";
            stackPanel1.Size = new System.Drawing.Size(175, 33);
            stackPanel1.TabIndex = 1;
            // 
            // simpleButton_ChangeSign
            // 
            simpleButton_ChangeSign.AllowFocus = false;
            simpleButton_ChangeSign.Location = new System.Drawing.Point(4, 5);
            simpleButton_ChangeSign.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            simpleButton_ChangeSign.Name = "simpleButton_ChangeSign";
            simpleButton_ChangeSign.Size = new System.Drawing.Size(35, 23);
            toolTipTitleItem1.Text = "Change Sign";
            toolTipItem1.Text = "Click to change the sign of the value";
            superToolTip1.Items.Add(toolTipTitleItem1);
            superToolTip1.Items.Add(toolTipItem1);
            simpleButton_ChangeSign.SuperTip = superToolTip1;
            simpleButton_ChangeSign.TabIndex = 1;
            simpleButton_ChangeSign.Text = "+/-";
            simpleButton_ChangeSign.Click += simpleButton_ChangeSign_Click;
            // 
            // spinEdit1
            // 
            spinEdit1.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spinEdit1.Location = new System.Drawing.Point(47, 6);
            spinEdit1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            spinEdit1.Name = "spinEdit1";
            spinEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinEdit1.Size = new System.Drawing.Size(110, 20);
            spinEdit1.TabIndex = 0;
            spinEdit1.ValueChanged += spinEdit1_ValueChanged;
            // 
            // SignedSpinEditControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(stackPanel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SignedSpinEditControl";
            Size = new System.Drawing.Size(175, 33);
            ((System.ComponentModel.ISupportInitialize)stackPanel1).EndInit();
            stackPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)spinEdit1.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private DevExpress.Utils.Layout.StackPanel stackPanel1;
        private DevExpress.XtraEditors.SimpleButton simpleButton_ChangeSign;
        private ScrollableSpinEdit spinEdit1;
    }
}
