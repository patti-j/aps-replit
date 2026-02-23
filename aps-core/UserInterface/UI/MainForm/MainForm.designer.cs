using System.Windows.Forms;
namespace PT.UI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            m_barAndDockingController = new DevExpress.XtraBars.BarAndDockingController(components);
            m_scenariosPanel = new DevExpress.XtraEditors.PanelControl();
            toolbarFormControl1 = new DevExpress.XtraBars.ToolbarForm.ToolbarFormControl();
            m_toolbarFormManager = new DevExpress.XtraBars.ToolbarForm.ToolbarFormManager(components);
            barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            m_barManager_Main = new DevExpress.XtraBars.BarManager(components);
            m_scenarioBar = new DevExpress.XtraBars.Bar();
            m_mainBar = new DevExpress.XtraBars.Bar();
            barDockControl1 = new DevExpress.XtraBars.BarDockControl();
            barDockControl2 = new DevExpress.XtraBars.BarDockControl();
            barDockControl3 = new DevExpress.XtraBars.BarDockControl();
            barDockControl4 = new DevExpress.XtraBars.BarDockControl();
            ((System.ComponentModel.ISupportInitialize)m_barAndDockingController).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_scenariosPanel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)toolbarFormControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_toolbarFormManager).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_barManager_Main).BeginInit();
            SuspendLayout();
            // 
            // m_barAndDockingController
            // 
            m_barAndDockingController.AppearancesBar.ItemsFont = new System.Drawing.Font("Verdana", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            m_barAndDockingController.PropertiesBar.AllowLinkLighting = false;
            m_barAndDockingController.PropertiesBar.BarItemHorzIndent = 5;
            m_barAndDockingController.PropertiesBar.BarItemVertIndent = 5;
            m_barAndDockingController.PropertiesDocking.ViewStyle = DevExpress.XtraBars.Docking2010.Views.DockingViewStyle.Classic;
            // 
            // m_scenariosPanel
            // 
            resources.ApplyResources(m_scenariosPanel, "m_scenariosPanel");
            m_scenariosPanel.Name = "m_scenariosPanel";
            // 
            // toolbarFormControl1
            // 
            resources.ApplyResources(toolbarFormControl1, "toolbarFormControl1");
            toolbarFormControl1.Manager = m_toolbarFormManager;
            toolbarFormControl1.Name = "toolbarFormControl1";
            toolbarFormControl1.TabStop = false;
            toolbarFormControl1.ToolbarForm = this;
            // 
            // m_toolbarFormManager
            // 
            m_toolbarFormManager.DockControls.Add(barDockControlTop);
            m_toolbarFormManager.DockControls.Add(barDockControlBottom);
            m_toolbarFormManager.DockControls.Add(barDockControlLeft);
            m_toolbarFormManager.DockControls.Add(barDockControlRight);
            m_toolbarFormManager.Form = this;
            // 
            // barDockControlTop
            // 
            barDockControlTop.CausesValidation = false;
            resources.ApplyResources(barDockControlTop, "barDockControlTop");
            barDockControlTop.Manager = m_toolbarFormManager;
            // 
            // barDockControlBottom
            // 
            barDockControlBottom.CausesValidation = false;
            resources.ApplyResources(barDockControlBottom, "barDockControlBottom");
            barDockControlBottom.Manager = m_toolbarFormManager;
            // 
            // barDockControlLeft
            // 
            barDockControlLeft.CausesValidation = false;
            resources.ApplyResources(barDockControlLeft, "barDockControlLeft");
            barDockControlLeft.Manager = m_toolbarFormManager;
            // 
            // barDockControlRight
            // 
            barDockControlRight.CausesValidation = false;
            resources.ApplyResources(barDockControlRight, "barDockControlRight");
            barDockControlRight.Manager = m_toolbarFormManager;
            // 
            // m_barManager_Main
            // 
            m_barManager_Main.Bars.AddRange(new DevExpress.XtraBars.Bar[] { m_scenarioBar, m_mainBar });
            m_barManager_Main.Controller = m_barAndDockingController;
            m_barManager_Main.DockControls.Add(barDockControl1);
            m_barManager_Main.DockControls.Add(barDockControl2);
            m_barManager_Main.DockControls.Add(barDockControl3);
            m_barManager_Main.DockControls.Add(barDockControl4);
            m_barManager_Main.Form = this;
            m_barManager_Main.MainMenu = m_mainBar;
            // 
            // m_scenarioBar
            // 
            m_scenarioBar.BarName = "ScenarioBar";
            m_scenarioBar.DockCol = 0;
            m_scenarioBar.DockRow = 1;
            m_scenarioBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            m_scenarioBar.OptionsBar.AllowQuickCustomization = false;
            m_scenarioBar.OptionsBar.AutoPopupMode = DevExpress.XtraBars.BarAutoPopupMode.None;
            m_scenarioBar.OptionsBar.DrawDragBorder = false;
            m_scenarioBar.OptionsBar.UseWholeRow = true;
            resources.ApplyResources(m_scenarioBar, "m_scenarioBar");
            // 
            // m_mainBar
            // 
            m_mainBar.BarName = "MainBar";
            m_mainBar.DockCol = 0;
            m_mainBar.DockRow = 0;
            m_mainBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            m_mainBar.OptionsBar.AllowQuickCustomization = false;
            m_mainBar.OptionsBar.AutoPopupMode = DevExpress.XtraBars.BarAutoPopupMode.None;
            m_mainBar.OptionsBar.DrawDragBorder = false;
            m_mainBar.OptionsBar.MultiLine = true;
            m_mainBar.OptionsBar.UseWholeRow = true;
            resources.ApplyResources(m_mainBar, "m_mainBar");
            // 
            // barDockControl1
            // 
            barDockControl1.CausesValidation = false;
            resources.ApplyResources(barDockControl1, "barDockControl1");
            barDockControl1.Manager = m_barManager_Main;
            // 
            // barDockControl2
            // 
            barDockControl2.CausesValidation = false;
            resources.ApplyResources(barDockControl2, "barDockControl2");
            barDockControl2.Manager = m_barManager_Main;
            // 
            // barDockControl3
            // 
            barDockControl3.CausesValidation = false;
            resources.ApplyResources(barDockControl3, "barDockControl3");
            barDockControl3.Manager = m_barManager_Main;
            // 
            // barDockControl4
            // 
            barDockControl4.CausesValidation = false;
            resources.ApplyResources(barDockControl4, "barDockControl4");
            barDockControl4.Manager = m_barManager_Main;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(m_scenariosPanel);
            Controls.Add(barDockControlLeft);
            Controls.Add(barDockControlRight);
            Controls.Add(barDockControlBottom);
            Controls.Add(barDockControlTop);
            Controls.Add(barDockControl3);
            Controls.Add(barDockControl4);
            Controls.Add(barDockControl2);
            Controls.Add(barDockControl1);
            Controls.Add(toolbarFormControl1);
            IconOptions.Icon = (System.Drawing.Icon)resources.GetObject("MainForm.IconOptions.Icon");
            Name = "MainForm";
            ToolbarFormControl = toolbarFormControl1;
            HelpRequested += MainForm_HelpRequested;
            ((System.ComponentModel.ISupportInitialize)m_barAndDockingController).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_scenariosPanel).EndInit();
            ((System.ComponentModel.ISupportInitialize)toolbarFormControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_toolbarFormManager).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_barManager_Main).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DevExpress.XtraEditors.PanelControl m_scenariosPanel;
        private DevExpress.XtraBars.BarAndDockingController m_barAndDockingController;
        private DevExpress.XtraBars.ToolbarForm.ToolbarFormControl toolbarFormControl1;
        private DevExpress.XtraBars.ToolbarForm.ToolbarFormManager m_toolbarFormManager;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarDockControl barDockControl3;
        private DevExpress.XtraBars.BarManager m_barManager_Main;
        private DevExpress.XtraBars.Bar m_scenarioBar;
        private DevExpress.XtraBars.Bar m_mainBar;
        private DevExpress.XtraBars.BarDockControl barDockControl1;
        private DevExpress.XtraBars.BarDockControl barDockControl2;
        private DevExpress.XtraBars.BarDockControl barDockControl4;
    }
}