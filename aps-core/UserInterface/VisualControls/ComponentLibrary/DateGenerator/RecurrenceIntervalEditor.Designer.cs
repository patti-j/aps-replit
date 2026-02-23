namespace PT.ComponentLibrary.DateGenerator
{
    partial class RecurrenceIntervalEditor
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
            this.radioButton_quarter = new System.Windows.Forms.RadioButton();
            this.radioButton_month = new System.Windows.Forms.RadioButton();
            this.radioButton_week = new System.Windows.Forms.RadioButton();
            this.radioButton_day = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Controls.Add(this.radioButton_quarter, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.radioButton_month, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.radioButton_week, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.radioButton_day, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(300, 55);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // radioButton_quarter
            // 
            this.radioButton_quarter.AutoSize = true;
            this.radioButton_quarter.Cursor = System.Windows.Forms.Cursors.Default;
            this.radioButton_quarter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton_quarter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.radioButton_quarter.Location = new System.Drawing.Point(153, 30);
            this.radioButton_quarter.Name = "radioButton_quarter";
            this.radioButton_quarter.Size = new System.Drawing.Size(144, 22);
            this.radioButton_quarter.TabIndex = 3;
            this.radioButton_quarter.Tag = "Quarter ({0})";
            this.radioButton_quarter.Text = "Quarter";
            this.radioButton_quarter.UseVisualStyleBackColor = true;
            this.radioButton_quarter.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButton_month
            // 
            this.radioButton_month.AutoSize = true;
            this.radioButton_month.Cursor = System.Windows.Forms.Cursors.Default;
            this.radioButton_month.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton_month.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.radioButton_month.Location = new System.Drawing.Point(3, 30);
            this.radioButton_month.Name = "radioButton_month";
            this.radioButton_month.Size = new System.Drawing.Size(144, 22);
            this.radioButton_month.TabIndex = 2;
            this.radioButton_month.Tag = "Month ({0})";
            this.radioButton_month.Text = "Month";
            this.radioButton_month.UseVisualStyleBackColor = true;
            this.radioButton_month.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButton_week
            // 
            this.radioButton_week.AutoSize = true;
            this.radioButton_week.Cursor = System.Windows.Forms.Cursors.Default;
            this.radioButton_week.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton_week.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.radioButton_week.Location = new System.Drawing.Point(153, 3);
            this.radioButton_week.Name = "radioButton_week";
            this.radioButton_week.Size = new System.Drawing.Size(144, 21);
            this.radioButton_week.TabIndex = 1;
            this.radioButton_week.Tag = "Week ({0})";
            this.radioButton_week.Text = "Week";
            this.radioButton_week.UseVisualStyleBackColor = true;
            this.radioButton_week.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButton_day
            // 
            this.radioButton_day.AutoSize = true;
            this.radioButton_day.Checked = true;
            this.radioButton_day.Cursor = System.Windows.Forms.Cursors.Default;
            this.radioButton_day.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton_day.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.radioButton_day.Location = new System.Drawing.Point(3, 3);
            this.radioButton_day.Name = "radioButton_day";
            this.radioButton_day.Size = new System.Drawing.Size(144, 21);
            this.radioButton_day.TabIndex = 0;
            this.radioButton_day.TabStop = true;
            this.radioButton_day.Tag = "Day ({0})";
            this.radioButton_day.Text = "Day";
            this.radioButton_day.UseVisualStyleBackColor = true;
            this.radioButton_day.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // RecurrenceIntervalEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "RecurrenceIntervalEditor";
            this.Size = new System.Drawing.Size(300, 55);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.RadioButton radioButton_day;
        private System.Windows.Forms.RadioButton radioButton_quarter;
        private System.Windows.Forms.RadioButton radioButton_month;
        private System.Windows.Forms.RadioButton radioButton_week;
    }
}
