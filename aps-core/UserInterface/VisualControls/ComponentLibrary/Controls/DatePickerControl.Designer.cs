namespace PT.ComponentLibrary.Controls
{
    public partial class DatePickerControl
    {
        #region Declarations
        private DevExpress.XtraEditors.TimeEdit m_timeEditor;
        private DevExpress.XtraEditors.DateEdit m_dateEditor;
        private DevExpress.XtraLayout.LayoutControl layoutControl_DatePicker;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraLayout.LayoutControlItem lci_timeEditor;
        private DevExpress.XtraLayout.SimpleLabelItem simpleLabelItem_Text;
        private DevExpress.XtraLayout.LayoutControlItem lci_dateEditor;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.Container components = null;
        #endregion

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions1 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject2 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject3 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject4 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions2 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject5 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject6 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject7 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject8 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions3 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject9 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject10 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject11 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject12 = new DevExpress.Utils.SerializableAppearanceObject();
            this.layoutControl_DatePicker = new DevExpress.XtraLayout.LayoutControl();
            this.m_dateEditor = new DevExpress.XtraEditors.DateEdit();
            this.m_timeEditor = new DevExpress.XtraEditors.TimeEdit();
            this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
            this.lci_timeEditor = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleLabelItem_Text = new DevExpress.XtraLayout.SimpleLabelItem();
            this.lci_dateEditor = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl_DatePicker)).BeginInit();
            this.layoutControl_DatePicker.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dateEditor.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dateEditor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_timeEditor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lci_timeEditor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem_Text)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lci_dateEditor)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl_DatePicker
            // 
            this.layoutControl_DatePicker.AutoSize = true;
            this.layoutControl_DatePicker.AutoScroll = false;
            this.layoutControl_DatePicker.Controls.Add(this.m_dateEditor);
            this.layoutControl_DatePicker.Controls.Add(this.m_timeEditor);
            this.layoutControl_DatePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl_DatePicker.Location = new System.Drawing.Point(0, 0);
            this.layoutControl_DatePicker.Margin = new System.Windows.Forms.Padding(0);
            this.layoutControl_DatePicker.Name = "layoutControl_DatePicker";
            this.layoutControl_DatePicker.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(-2886, 208, 650, 400);
            this.layoutControl_DatePicker.Root = this.Root;
            this.layoutControl_DatePicker.Size = new System.Drawing.Size(283, 20);
            this.layoutControl_DatePicker.TabIndex = 10;
            this.layoutControl_DatePicker.Text = "layoutControl1";
            // 
            // m_dateEditor
            // 
            this.m_dateEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_dateEditor.EditValue = null;
            this.m_dateEditor.Location = new System.Drawing.Point(33, 0);
            this.m_dateEditor.Margin = new System.Windows.Forms.Padding(0);
            this.m_dateEditor.MaximumSize = new System.Drawing.Size(120, 20);
            this.m_dateEditor.MinimumSize = new System.Drawing.Size(120, 20);
            this.m_dateEditor.Name = "m_dateEditor";
            this.m_dateEditor.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Today", -1, true, true, false, editorButtonImageOptions1, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, serializableAppearanceObject2, serializableAppearanceObject3, serializableAppearanceObject4, "", "Today", null, DevExpress.Utils.ToolTipAnchor.Default)});
            this.m_dateEditor.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.m_dateEditor.Properties.DrawCellLines = true;
            this.m_dateEditor.Properties.NullText = "Not Set";
            this.m_dateEditor.Size = new System.Drawing.Size(120, 20);
            this.m_dateEditor.StyleController = this.layoutControl_DatePicker;
            this.m_dateEditor.TabIndex = 7;
            this.m_dateEditor.EditValueChanged += new System.EventHandler(this.m_timeEditor_EditValueChanged);
            this.m_dateEditor.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.m_dateEditor_EditValueChanging);
            // 
            // m_timeEditor
            // 
            this.m_timeEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_timeEditor.EditValue = new System.DateTime(2016, 4, 18, 0, 0, 0, 0);
            this.m_timeEditor.Location = new System.Drawing.Point(153, 0);
            this.m_timeEditor.Margin = new System.Windows.Forms.Padding(0);
            this.m_timeEditor.MaximumSize = new System.Drawing.Size(130, 20);
            this.m_timeEditor.MinimumSize = new System.Drawing.Size(130, 20);
            this.m_timeEditor.Name = "m_timeEditor";
            this.m_timeEditor.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Now", -1, true, true, false, editorButtonImageOptions2, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject5, serializableAppearanceObject6, serializableAppearanceObject7, serializableAppearanceObject8, "", "Now", null, DevExpress.Utils.ToolTipAnchor.Default),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Delete, "Clear", -1, true, true, false, editorButtonImageOptions3, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject9, serializableAppearanceObject10, serializableAppearanceObject11, serializableAppearanceObject12, "", "Clear", null, DevExpress.Utils.ToolTipAnchor.Default)});
            this.m_timeEditor.Properties.TimeEditStyle = DevExpress.XtraEditors.Repository.TimeEditStyle.TouchUI;
            this.m_timeEditor.Size = new System.Drawing.Size(130, 20);
            this.m_timeEditor.StyleController = this.layoutControl_DatePicker;
            this.m_timeEditor.TabIndex = 8;
            this.m_timeEditor.EditValueChanged += new System.EventHandler(this.m_timeEditor_EditValueChanged);
            this.m_timeEditor.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.m_timeEditor_EditValueChanging);
            // 
            // Root
            // 
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.lci_timeEditor,
            this.simpleLabelItem_Text,
            this.lci_dateEditor});
            this.Root.Name = "Root";
            this.Root.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.Root.Size = new System.Drawing.Size(283, 20);
            this.Root.TextVisible = false;
            // 
            // lci_timeEditor
            // 
            this.lci_timeEditor.Control = this.m_timeEditor;
            this.lci_timeEditor.Location = new System.Drawing.Point(153, 0);
            this.lci_timeEditor.Name = "lci_timeEditor";
            this.lci_timeEditor.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.lci_timeEditor.Size = new System.Drawing.Size(130, 20);
            this.lci_timeEditor.TextSize = new System.Drawing.Size(0, 0);
            this.lci_timeEditor.TextVisible = false;
            // 
            // simpleLabelItem_Text
            // 
            this.simpleLabelItem_Text.AllowHotTrack = false;
            this.simpleLabelItem_Text.Location = new System.Drawing.Point(0, 0);
            this.simpleLabelItem_Text.Name = "simpleLabelItem_Text";
            this.simpleLabelItem_Text.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 0, 0);
            this.simpleLabelItem_Text.Size = new System.Drawing.Size(33, 20);
            this.simpleLabelItem_Text.Text = "Label:";
            this.simpleLabelItem_Text.TextSize = new System.Drawing.Size(29, 13);
            // 
            // lci_dateEditor
            // 
            this.lci_dateEditor.Control = this.m_dateEditor;
            this.lci_dateEditor.Location = new System.Drawing.Point(33, 0);
            this.lci_dateEditor.Name = "lci_dateEditor";
            this.lci_dateEditor.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.lci_dateEditor.Size = new System.Drawing.Size(120, 20);
            this.lci_dateEditor.TextSize = new System.Drawing.Size(0, 0);
            this.lci_dateEditor.TextVisible = false;
            // 
            // DatePickerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.layoutControl_DatePicker);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DatePickerControl";
            this.Size = new System.Drawing.Size(283, 20);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl_DatePicker)).EndInit();
            this.layoutControl_DatePicker.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_dateEditor.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dateEditor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_timeEditor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lci_timeEditor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem_Text)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lci_dateEditor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}