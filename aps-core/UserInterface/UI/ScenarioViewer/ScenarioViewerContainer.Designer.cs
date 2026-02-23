namespace PT.UI.ScenarioViewer
{
    partial class ScenarioViewerContainer
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
            if (disposing)
            {
                components?.Dispose();
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
            DevExpress.Utils.SuperToolTip superToolTip4 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem4 = new DevExpress.Utils.ToolTipTitleItem();
            DevExpress.Utils.ToolTipItem toolTipItem4 = new DevExpress.Utils.ToolTipItem();
            DevExpress.Utils.SuperToolTip superToolTip5 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem5 = new DevExpress.Utils.ToolTipTitleItem();
            DevExpress.Utils.ToolTipItem toolTipItem5 = new DevExpress.Utils.ToolTipItem();
            DevExpress.Utils.SuperToolTip superToolTip6 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem6 = new DevExpress.Utils.ToolTipTitleItem();
            DevExpress.Utils.ToolTipItem toolTipItem6 = new DevExpress.Utils.ToolTipItem();
            this.m_scenarioViewerPanel = new DevExpress.XtraEditors.PanelControl();
            this.panel_flyoutDocking = new System.Windows.Forms.Panel();
            this.simpleButton_Show = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton_NotificationSettings = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton_Hide = new DevExpress.XtraEditors.SimpleButton();
            this.panelControl_NotificationDock = new DevExpress.XtraEditors.PanelControl();
            this.stackPanel_NotificationSlides = new DevExpress.Utils.Layout.StackPanel();
            this.stackPanel_Buttons = new DevExpress.Utils.Layout.StackPanel();
            ((System.ComponentModel.ISupportInitialize)(this.m_scenarioViewerPanel)).BeginInit();
            this.m_scenarioViewerPanel.SuspendLayout();
            this.panel_flyoutDocking.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_NotificationDock)).BeginInit();
            this.panelControl_NotificationDock.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stackPanel_NotificationSlides)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stackPanel_Buttons)).BeginInit();
            this.stackPanel_Buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_scenarioViewerPanel
            // 
            this.m_scenarioViewerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.m_scenarioViewerPanel.Controls.Add(this.panel_flyoutDocking);
            this.m_scenarioViewerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_scenarioViewerPanel.Location = new System.Drawing.Point(0, 0);
            this.m_scenarioViewerPanel.Name = "m_scenarioViewerPanel";
            this.m_scenarioViewerPanel.Size = new System.Drawing.Size(1238, 680);
            this.m_scenarioViewerPanel.TabIndex = 21;
            // 
            // panel_flyoutDocking
            // 
            this.panel_flyoutDocking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_flyoutDocking.Controls.Add(this.simpleButton_Show);
            this.panel_flyoutDocking.Location = new System.Drawing.Point(1181, 624);
            this.panel_flyoutDocking.Margin = new System.Windows.Forms.Padding(0);
            this.panel_flyoutDocking.Name = "panel_flyoutDocking";
            this.panel_flyoutDocking.Size = new System.Drawing.Size(55, 56);
            this.panel_flyoutDocking.TabIndex = 1;
            // 
            // simpleButton_Show
            // 
            this.simpleButton_Show.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleButton_Show.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButton_Show.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.simpleButton_Show.Location = new System.Drawing.Point(0, 0);
            this.simpleButton_Show.Name = "simpleButton_Show";
            this.simpleButton_Show.Size = new System.Drawing.Size(55, 56);
            toolTipTitleItem4.Text = "Open ";
            toolTipItem4.LeftIndent = 6;
            toolTipItem4.Text = "Show the notifications bar\r\n";
            superToolTip4.Items.Add(toolTipTitleItem4);
            superToolTip4.Items.Add(toolTipItem4);
            this.simpleButton_Show.SuperTip = superToolTip4;
            this.simpleButton_Show.TabIndex = 3;
            this.simpleButton_Show.Click += new System.EventHandler(this.simpleButton_Show_Click);
            // 
            // simpleButton_NotificationSettings
            // 
            this.simpleButton_NotificationSettings.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButton_NotificationSettings.ImageOptions.SvgImageSize = new System.Drawing.Size(24, 24);
            this.simpleButton_NotificationSettings.Location = new System.Drawing.Point(2, 6);
            this.simpleButton_NotificationSettings.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.simpleButton_NotificationSettings.Name = "simpleButton_NotificationSettings";
            this.simpleButton_NotificationSettings.Size = new System.Drawing.Size(32, 32);
            toolTipTitleItem5.Text = "Notification Settings";
            toolTipItem5.LeftIndent = 6;
            toolTipItem5.Text = "Open the notification settings";
            superToolTip5.Items.Add(toolTipTitleItem5);
            superToolTip5.Items.Add(toolTipItem5);
            this.simpleButton_NotificationSettings.SuperTip = superToolTip5;
            this.simpleButton_NotificationSettings.TabIndex = 4;
            this.simpleButton_NotificationSettings.Click += new System.EventHandler(this.simpleButton_NotificationSettings_Click);
            // 
            // simpleButton_Hide
            // 
            this.simpleButton_Hide.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButton_Hide.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.simpleButton_Hide.Location = new System.Drawing.Point(38, 6);
            this.simpleButton_Hide.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.simpleButton_Hide.Name = "simpleButton_Hide";
            this.simpleButton_Hide.Size = new System.Drawing.Size(32, 32);
            toolTipTitleItem6.Text = "Hide\r\n";
            toolTipItem6.LeftIndent = 6;
            toolTipItem6.Text = "Hide notifications panel\r\n";
            superToolTip6.Items.Add(toolTipTitleItem6);
            superToolTip6.Items.Add(toolTipItem6);
            this.simpleButton_Hide.SuperTip = superToolTip6;
            this.simpleButton_Hide.TabIndex = 5;
            this.simpleButton_Hide.Click += new System.EventHandler(this.simpleButton_Hide_Click);
            // 
            // panelControl_NotificationDock
            // 
            this.panelControl_NotificationDock.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.panelControl_NotificationDock.Appearance.Options.UseBackColor = true;
            this.panelControl_NotificationDock.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_NotificationDock.Controls.Add(this.stackPanel_NotificationSlides);
            this.panelControl_NotificationDock.Controls.Add(this.stackPanel_Buttons);
            this.panelControl_NotificationDock.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelControl_NotificationDock.Location = new System.Drawing.Point(0, 680);
            this.panelControl_NotificationDock.Name = "panelControl_NotificationDock";
            this.panelControl_NotificationDock.Size = new System.Drawing.Size(1238, 44);
            this.panelControl_NotificationDock.TabIndex = 22;
            // 
            // stackPanel_NotificationSlides
            // 
            this.stackPanel_NotificationSlides.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stackPanel_NotificationSlides.Location = new System.Drawing.Point(0, 0);
            this.stackPanel_NotificationSlides.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel_NotificationSlides.Name = "stackPanel_NotificationSlides";
            this.stackPanel_NotificationSlides.Size = new System.Drawing.Size(1164, 44);
            this.stackPanel_NotificationSlides.TabIndex = 0;
            // 
            // stackPanel_Buttons
            // 
            this.stackPanel_Buttons.Controls.Add(this.simpleButton_NotificationSettings);
            this.stackPanel_Buttons.Controls.Add(this.simpleButton_Hide);
            this.stackPanel_Buttons.Dock = System.Windows.Forms.DockStyle.Right;
            this.stackPanel_Buttons.Location = new System.Drawing.Point(1164, 0);
            this.stackPanel_Buttons.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.stackPanel_Buttons.Name = "stackPanel_Buttons";
            this.stackPanel_Buttons.Size = new System.Drawing.Size(74, 44);
            this.stackPanel_Buttons.TabIndex = 1;
            // 
            // ScenarioViewerContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_scenarioViewerPanel);
            this.Controls.Add(this.panelControl_NotificationDock);
            this.Name = "ScenarioViewerContainer";
            this.Size = new System.Drawing.Size(1238, 724);
            ((System.ComponentModel.ISupportInitialize)(this.m_scenarioViewerPanel)).EndInit();
            this.m_scenarioViewerPanel.ResumeLayout(false);
            this.panel_flyoutDocking.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_NotificationDock)).EndInit();
            this.panelControl_NotificationDock.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.stackPanel_NotificationSlides)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stackPanel_Buttons)).EndInit();
            this.stackPanel_Buttons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.PanelControl m_scenarioViewerPanel;
        private DevExpress.XtraEditors.PanelControl panelControl_NotificationDock;
        private DevExpress.XtraEditors.SimpleButton simpleButton_NotificationSettings;
        private System.Windows.Forms.Panel panel_flyoutDocking;
        private DevExpress.XtraEditors.SimpleButton simpleButton_Hide;
        private DevExpress.XtraEditors.SimpleButton simpleButton_Show;
        private DevExpress.Utils.Layout.StackPanel stackPanel_NotificationSlides;
        private DevExpress.Utils.Layout.StackPanel stackPanel_Buttons;
    }
}
