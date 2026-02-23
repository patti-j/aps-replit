namespace PT.ComponentLibrary.Controls
{
    partial class ValueSlider
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
            this.trackBarControl_Main = new DevExpress.XtraEditors.TrackBarControl();
            this.labelControl_Text = new DevExpress.XtraEditors.LabelControl();
            this.buttonEdit_Value = new DevExpress.XtraEditors.ButtonEdit();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.panelControl_Spacing = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl_Main)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl_Main.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_Value.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Spacing)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBarControl_Main
            // 
            this.trackBarControl_Main.Dock = System.Windows.Forms.DockStyle.Right;
            this.trackBarControl_Main.EditValue = null;
            this.trackBarControl_Main.Location = new System.Drawing.Point(319, 0);
            this.trackBarControl_Main.Name = "trackBarControl_Main";
            this.trackBarControl_Main.Properties.LabelAppearance.Options.UseTextOptions = true;
            this.trackBarControl_Main.Properties.LabelAppearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.trackBarControl_Main.Properties.Maximum = 1000;
            this.trackBarControl_Main.Properties.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarControl_Main.Size = new System.Drawing.Size(350, 27);
            this.trackBarControl_Main.TabIndex = 0;
            this.trackBarControl_Main.EditValueChanged += new System.EventHandler(this.trackBarControl_Main_EditValueChanged);
            // 
            // labelControl_Text
            // 
            this.labelControl_Text.Appearance.Options.UseTextOptions = true;
            this.labelControl_Text.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.labelControl_Text.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl_Text.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl_Text.Location = new System.Drawing.Point(0, 0);
            this.labelControl_Text.Name = "labelControl_Text";
            this.labelControl_Text.Size = new System.Drawing.Size(304, 27);
            this.labelControl_Text.TabIndex = 2;
            this.labelControl_Text.Text = "labelControl1";
            // 
            // buttonEdit_Value
            // 
            this.buttonEdit_Value.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit_Value.Location = new System.Drawing.Point(18, 4);
            this.buttonEdit_Value.Name = "buttonEdit_Value";
            this.buttonEdit_Value.Properties.AutoHeight = false;
            this.buttonEdit_Value.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Close)});
            this.buttonEdit_Value.Properties.Mask.EditMask = "N00";
            this.buttonEdit_Value.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.buttonEdit_Value.Size = new System.Drawing.Size(59, 20);
            this.buttonEdit_Value.TabIndex = 3;
            this.buttonEdit_Value.ButtonPressed += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.buttonEdit_Value_ButtonPressed);
            this.buttonEdit_Value.EditValueChanged += new System.EventHandler(this.buttonEdit_Value_EditValueChanged);
            this.buttonEdit_Value.Validating += new System.ComponentModel.CancelEventHandler(this.buttonEdit_Value_Validating);
            // 
            // panelControl1
            // 
            this.panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl1.Controls.Add(this.buttonEdit_Value);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelControl1.Location = new System.Drawing.Point(669, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(82, 27);
            this.panelControl1.TabIndex = 4;
            // 
            // panelControl_Spacing
            // 
            this.panelControl_Spacing.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_Spacing.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelControl_Spacing.Location = new System.Drawing.Point(304, 0);
            this.panelControl_Spacing.Name = "panelControl_Spacing";
            this.panelControl_Spacing.Size = new System.Drawing.Size(15, 27);
            this.panelControl_Spacing.TabIndex = 5;
            // 
            // ValueSlider
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelControl_Text);
            this.Controls.Add(this.panelControl_Spacing);
            this.Controls.Add(this.trackBarControl_Main);
            this.Controls.Add(this.panelControl1);
            this.Name = "ValueSlider";
            this.Size = new System.Drawing.Size(751, 27);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl_Main.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl_Main)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_Value.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Spacing)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.TrackBarControl trackBarControl_Main;
        private DevExpress.XtraEditors.LabelControl labelControl_Text;
        private DevExpress.XtraEditors.ButtonEdit buttonEdit_Value;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl_Spacing;
    }
}
