namespace PT.ComponentLibrary.DateGenerator
{
    partial class DateGeneratorControl
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
            this.recurrenceIntervalEditor = new PT.ComponentLibrary.DateGenerator.RecurrenceIntervalEditor();
            this.dateRangeEditor = new PT.ComponentLibrary.DateGenerator.DateRangeEditor();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 377F));
            this.tableLayoutPanel1.Controls.Add(this.recurrenceIntervalEditor, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dateRangeEditor, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(377, 158);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // recurrenceIntervalEditor
            // 
            this.recurrenceIntervalEditor.BackColor = System.Drawing.SystemColors.Control;
            this.recurrenceIntervalEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recurrenceIntervalEditor.Location = new System.Drawing.Point(3, 82);
            this.recurrenceIntervalEditor.Name = "recurrenceIntervalEditor";
            this.recurrenceIntervalEditor.Size = new System.Drawing.Size(371, 73);
            this.recurrenceIntervalEditor.TabIndex = 1;
            // 
            // dateRangeEditor
            // 
            this.dateRangeEditor.BackColor = System.Drawing.SystemColors.Control;
            this.dateRangeEditor.Location = new System.Drawing.Point(3, 3);
            this.dateRangeEditor.Name = "dateRangeEditor";
            this.dateRangeEditor.Size = new System.Drawing.Size(371, 73);
            this.dateRangeEditor.TabIndex = 2;
            // 
            // DateGeneratorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DateGeneratorControl";
            this.Size = new System.Drawing.Size(377, 158);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private RecurrenceIntervalEditor recurrenceIntervalEditor;
        private DateRangeEditor dateRangeEditor;
    }
}
