namespace PT.ComponentLibrary.Controls
{
    partial class SetJobsValuesGenericEditorControl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ultraPanel_EditorControl = new DevExpress.XtraEditors.PanelControl();
            this.ultraCheckEditor_Set = new DevExpress.XtraEditors.CheckEdit();
            this.Label_Warning = new DevExpress.XtraEditors.LabelControl();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraPanel_EditorControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraCheckEditor_Set.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.ultraPanel_EditorControl, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.ultraCheckEditor_Set, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Label_Warning, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(381, 41);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // ultraPanel_EditorControl
            // 
            this.ultraPanel_EditorControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.ultraPanel_EditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel_EditorControl.Location = new System.Drawing.Point(128, 3);
            this.ultraPanel_EditorControl.Name = "ultraPanel_EditorControl";
            this.ultraPanel_EditorControl.Size = new System.Drawing.Size(119, 35);
            this.ultraPanel_EditorControl.TabIndex = 2;
            // 
            // ultraCheckEditor_Set
            // 
            this.ultraCheckEditor_Set.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraCheckEditor_Set.Location = new System.Drawing.Point(3, 3);
            this.ultraCheckEditor_Set.Name = "ultraCheckEditor_Set";
            this.ultraCheckEditor_Set.Properties.Appearance.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraCheckEditor_Set.Properties.Appearance.Options.UseFont = true;
            this.ultraCheckEditor_Set.Properties.Caption = "checkEdit1";
            this.ultraCheckEditor_Set.Size = new System.Drawing.Size(119, 35);
            this.ultraCheckEditor_Set.TabIndex = 3;
            // 
            // Label_Warning
            // 
            this.Label_Warning.Appearance.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Warning.Appearance.Options.UseFont = true;
            this.Label_Warning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label_Warning.Location = new System.Drawing.Point(253, 3);
            this.Label_Warning.Name = "Label_Warning";
            this.Label_Warning.Size = new System.Drawing.Size(125, 35);
            this.Label_Warning.TabIndex = 4;
            // 
            // SetJobsValuesGenericEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SetJobsValuesGenericEditorControl";
            this.Size = new System.Drawing.Size(381, 41);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraPanel_EditorControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraCheckEditor_Set.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DevExpress.XtraEditors.PanelControl ultraPanel_EditorControl;
        private DevExpress.XtraEditors.CheckEdit ultraCheckEditor_Set;
        private DevExpress.XtraEditors.LabelControl Label_Warning;
    }
}
