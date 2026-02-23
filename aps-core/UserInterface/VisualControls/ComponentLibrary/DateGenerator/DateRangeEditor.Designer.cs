namespace PT.ComponentLibrary.DateGenerator
{
    partial class DateRangeEditor
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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.dateTimeEditor_end = new DevExpress.XtraEditors.DateEdit();
            this.label_end = new DevExpress.XtraEditors.LabelControl();
            this.label_start = new DevExpress.XtraEditors.LabelControl();
            this.dateTimeEditor_start = new DevExpress.XtraEditors.DateEdit();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Controls.Add(this.dateTimeEditor_end, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.label_end, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.label_start, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.dateTimeEditor_start, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(300, 55);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // dateTimeEditor_end
            // 
            this.dateTimeEditor_end.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimeEditor_end.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.dateTimeEditor_end.Location = new System.Drawing.Point(153, 30);
            this.dateTimeEditor_end.Properties.EditMask = "{LOC}mm/dd/yyyy";
            this.dateTimeEditor_end.Properties.MinValue = new System.DateTime(1800, 1, 1, 0, 0, 0, 0);
            this.dateTimeEditor_end.Name = "dateTimeEditor_end";
            this.dateTimeEditor_end.Size = new System.Drawing.Size(144, 24);
            this.dateTimeEditor_end.TabIndex = 3;
            this.dateTimeEditor_end.EditValueChanged += new System.EventHandler(this.dateTimeEditor_end_ValueChanged);
            // 
            // label_end
            // 
            this.label_end.AutoSize = true;
            this.label_end.BackColor = System.Drawing.SystemColors.Control;
            this.label_end.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_end.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label_end.Location = new System.Drawing.Point(153, 3);
            this.label_end.Margin = new System.Windows.Forms.Padding(3);
            this.label_end.Name = "label_end";
            this.label_end.Size = new System.Drawing.Size(144, 21);
            this.label_end.TabIndex = 1;
            this.label_end.Text = "End Date";
            // 
            // label_start
            // 
            this.label_start.AutoSize = true;
            this.label_start.BackColor = System.Drawing.SystemColors.Control;
            this.label_start.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_start.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label_start.Location = new System.Drawing.Point(3, 3);
            this.label_start.Margin = new System.Windows.Forms.Padding(3);
            this.label_start.Name = "label_start";
            this.label_start.Size = new System.Drawing.Size(144, 21);
            this.label_start.TabIndex = 0;
            this.label_start.Text = "Start Date";
            // 
            // dateTimeEditor_start
            // 
            this.dateTimeEditor_start.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimeEditor_start.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.dateTimeEditor_start.Location = new System.Drawing.Point(3, 30);
            this.dateTimeEditor_start.Properties.EditMask = "{LOC}mm/dd/yyyy";
            this.dateTimeEditor_start.Properties.MinValue = new System.DateTime(1800, 1, 1, 0, 0, 0, 0);
            this.dateTimeEditor_start.Name = "dateTimeEditor_start";
            this.dateTimeEditor_start.Size = new System.Drawing.Size(144, 24);
            this.dateTimeEditor_start.TabIndex = 2;
            this.dateTimeEditor_start.EditValueChanged += new System.EventHandler(this.dateTimeEditor_start_ValueChanged);
            // 
            // DateRangeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "DateRangeEditor";
            this.Size = new System.Drawing.Size(300, 55);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private DevExpress.XtraEditors.LabelControl label_end;
        private DevExpress.XtraEditors.LabelControl label_start;
        private DevExpress.XtraEditors.DateEdit dateTimeEditor_start;
        private DevExpress.XtraEditors.DateEdit dateTimeEditor_end;
    }
}
