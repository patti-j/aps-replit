namespace PT.ComponentLibrary.Features
{
    partial class FeatureControlFrame
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
            this.panelControl_Title = new DevExpress.XtraEditors.PanelControl();
            this.labelControl_Title = new DevExpress.XtraEditors.LabelControl();
            this.accordionControl = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.accordionGroupElement = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.featuresNavFrame = new DevExpress.XtraBars.Navigation.NavigationFrame();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Title)).BeginInit();
            this.panelControl_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.featuresNavFrame)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl_Title
            // 
            this.panelControl_Title.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_Title.Controls.Add(this.labelControl_Title);
            this.panelControl_Title.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl_Title.Location = new System.Drawing.Point(0, 0);
            this.panelControl_Title.Name = "panelControl_Title";
            this.panelControl_Title.Size = new System.Drawing.Size(487, 35);
            this.panelControl_Title.TabIndex = 1;
            // 
            // labelControl_Title
            // 
            this.labelControl_Title.Appearance.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelControl_Title.Appearance.Options.UseFont = true;
            this.labelControl_Title.Location = new System.Drawing.Point(6, 6);
            this.labelControl_Title.Name = "labelControl_Title";
            this.labelControl_Title.Size = new System.Drawing.Size(81, 23);
            this.labelControl_Title.TabIndex = 0;
            this.labelControl_Title.Text = "Features";
            // 
            // accordionControl
            // 
            this.accordionControl.AllowItemSelection = true;
            this.accordionControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.accordionControl.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.accordionGroupElement});
            this.accordionControl.Location = new System.Drawing.Point(0, 35);
            this.accordionControl.Name = "accordionControl";
            this.accordionControl.Size = new System.Drawing.Size(155, 501);
            this.accordionControl.TabIndex = 2;
            this.accordionControl.ElementClick += new DevExpress.XtraBars.Navigation.ElementClickEventHandler(this.accordionControl_ElementClick);
            // 
            // accordionGroupElement
            // 
            this.accordionGroupElement.Expanded = true;
            this.accordionGroupElement.HeaderVisible = false;
            this.accordionGroupElement.Name = "accordionGroupElement";
            this.accordionGroupElement.Text = "Element1";
            // 
            // featuresNavFrame
            // 
            this.featuresNavFrame.AllowTransitionAnimation = DevExpress.Utils.DefaultBoolean.False;
            this.featuresNavFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.featuresNavFrame.Location = new System.Drawing.Point(155, 35);
            this.featuresNavFrame.Name = "featuresNavFrame";
            this.featuresNavFrame.SelectedPage = null;
            this.featuresNavFrame.Size = new System.Drawing.Size(332, 501);
            this.featuresNavFrame.TabIndex = 3;
            this.featuresNavFrame.Text = "featuresNavFrame";
            // 
            // FeatureControlFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.featuresNavFrame);
            this.Controls.Add(this.accordionControl);
            this.Controls.Add(this.panelControl_Title);
            this.Name = "FeatureControlFrame";
            this.Size = new System.Drawing.Size(487, 536);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Title)).EndInit();
            this.panelControl_Title.ResumeLayout(false);
            this.panelControl_Title.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.featuresNavFrame)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.PanelControl panelControl_Title;
        private DevExpress.XtraEditors.LabelControl labelControl_Title;
        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl;
        private DevExpress.XtraBars.Navigation.NavigationFrame featuresNavFrame;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionGroupElement;
    }
}
