namespace PT.ComponentLibrary.Controls
{
    partial class ColorPickerBoxControl
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
            DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
            this.colorPickEdit_Color = new DevExpress.XtraEditors.ColorPickEdit();
            this.labelControl_Main = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.colorPickEdit_Color.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // colorPickEdit_Color
            // 
            this.colorPickEdit_Color.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.colorPickEdit_Color.EditValue = System.Drawing.Color.Empty;
            this.colorPickEdit_Color.Location = new System.Drawing.Point(18, 16);
            this.colorPickEdit_Color.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.colorPickEdit_Color.Name = "colorPickEdit_Color";
            this.colorPickEdit_Color.Properties.AutomaticColor = System.Drawing.Color.Black;
            this.colorPickEdit_Color.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.colorPickEdit_Color.Size = new System.Drawing.Size(57, 20);
            this.colorPickEdit_Color.TabIndex = 182;
            this.colorPickEdit_Color.ColorChanged += new System.EventHandler(this.ColorPickEdit_Color_ColorChanged);
            // 
            // labelControl_Main
            // 
            this.labelControl_Main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl_Main.Appearance.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl_Main.Appearance.Options.UseFont = true;
            this.labelControl_Main.Appearance.Options.UseTextOptions = true;
            this.labelControl_Main.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl_Main.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl_Main.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.labelControl_Main.Location = new System.Drawing.Point(0, 0);
            this.labelControl_Main.Name = "labelControl_Main";
            this.labelControl_Main.Size = new System.Drawing.Size(90, 50);
            toolTipItem2.Text = "Scheduled more than the Job\'s MaxEarlySpan before the Activity\'s JIT Start Date.";
            superToolTip2.Items.Add(toolTipItem2);
            this.labelControl_Main.SuperTip = superToolTip2;
            this.labelControl_Main.TabIndex = 183;
            this.labelControl_Main.Text = "Too Early";
            this.labelControl_Main.Click += new System.EventHandler(this.LabelControl_Main_Click);
            // 
            // ColorPickerBoxControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelControl_Main);
            this.Controls.Add(this.colorPickEdit_Color);
            this.Name = "ColorPickerBoxControl";
            this.Size = new System.Drawing.Size(90, 50);
            ((System.ComponentModel.ISupportInitialize)(this.colorPickEdit_Color.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.ColorPickEdit colorPickEdit_Color;
        private DevExpress.XtraEditors.LabelControl labelControl_Main;
    }
}
