namespace PT.ScenarioControls.Tiles
{
    sealed partial class TileBoardLayoutControl<T>
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
            DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
            DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem2 = new DevExpress.Utils.ToolTipTitleItem();
            DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
            DevExpress.XtraBars.Navigation.AccordionCheckContextButton accordionCheckContextButton1 = new DevExpress.XtraBars.Navigation.AccordionCheckContextButton();
            DevExpress.XtraBars.Navigation.AccordionCheckContextButton accordionCheckContextButton2 = new DevExpress.XtraBars.Navigation.AccordionCheckContextButton();
            this.panelControl_Main = new DevExpress.XtraEditors.PanelControl();
            this.tileDocumentManager = new TileDocumentManager();
            this.accordionControl1 = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.accordionControlElement_Tiles = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionControlElement_Settings = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionControlElement_ShowMode = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionControlElement_CloseAll = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.accordionControlElement_InlineMenuMode = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.navButton2 = new DevExpress.XtraBars.Navigation.NavButton();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Main)).BeginInit();
            this.panelControl_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl_Main
            // 
            this.panelControl_Main.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_Main.Controls.Add(this.tileDocumentManager);
            this.panelControl_Main.Controls.Add(this.accordionControl1);
            this.panelControl_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl_Main.Location = new System.Drawing.Point(0, 0);
            this.panelControl_Main.Name = "panelControl_Main";
            this.panelControl_Main.Size = new System.Drawing.Size(816, 568);
            this.panelControl_Main.TabIndex = 0;
            // 
            // tileDocumentManager
            // 
            this.tileDocumentManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tileDocumentManager.Location = new System.Drawing.Point(250, 0);
            this.tileDocumentManager.Name = "tileDocumentManager";
            this.tileDocumentManager.Size = new System.Drawing.Size(566, 568);
            this.tileDocumentManager.TabIndex = 0;
            // 
            // accordionControl1
            // 
            this.accordionControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.accordionControl1.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
                this.accordionControlElement_Tiles,
                this.accordionControlElement_Settings,
                this.accordionControlElement_CloseAll});
            this.accordionControl1.ExpandGroupOnHeaderClick = false;
            this.accordionControl1.ExpandItemOnHeaderClick = false;
            this.accordionControl1.Location = new System.Drawing.Point(0, 0);
            this.accordionControl1.Name = "accordionControl1";
            this.accordionControl1.OptionsHamburgerMenu.DisplayMode = DevExpress.XtraBars.Navigation.AccordionControlDisplayMode.Overlay;
            this.accordionControl1.OptionsMinimizing.AllowMinimizeMode = DevExpress.Utils.DefaultBoolean.True;
            this.accordionControl1.RootDisplayMode = DevExpress.XtraBars.Navigation.AccordionControlRootDisplayMode.Footer;
            this.accordionControl1.ScrollBarMode = DevExpress.XtraBars.Navigation.ScrollBarMode.Fluent;
            this.accordionControl1.ShowGroupExpandButtons = false;
            this.accordionControl1.Size = new System.Drawing.Size(250, 568);
            this.accordionControl1.TabIndex = 1;
            this.accordionControl1.UseDirectXPaint = DevExpress.Utils.DefaultBoolean.False;
            this.accordionControl1.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu;
            this.accordionControl1.ElementClick += new DevExpress.XtraBars.Navigation.ElementClickEventHandler(this.accordionControl1_ElementClick);
            // 
            // accordionControlElement_Tiles
            // 
            this.accordionControlElement_Tiles.Expanded = true;
            this.accordionControlElement_Tiles.Name = "accordionControlElement_Tiles";
            toolTipTitleItem1.Text = "Tiles";
            toolTipItem1.LeftIndent = 6;
            toolTipItem1.Text = "Tiles contain tools for managing data in this board.  Tiles can be repositioned by dragging their title bar. They can also be undocked to view on another screen.";
            superToolTip1.Items.Add(toolTipTitleItem1);
            superToolTip1.Items.Add(toolTipItem1);
            this.accordionControlElement_Tiles.SuperTip = superToolTip1;
            this.accordionControlElement_Tiles.Text = "Tiles";
            // 
            // accordionControlElement_Settings
            // 
            this.accordionControlElement_Settings.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
                this.accordionControlElement_ShowMode,
                this.accordionControlElement_InlineMenuMode}); // Removed accordionControlElement_CloseAll
            this.accordionControlElement_Settings.Expanded = true;
            this.accordionControlElement_Settings.Name = "accordionControlElement_Settings";
            toolTipTitleItem2.Text = "Tile Layout Settings";
            toolTipItem2.LeftIndent = 6;
            toolTipItem2.Text = "Settings for managing tile layouts on this board.";
            superToolTip2.Items.Add(toolTipTitleItem2);
            superToolTip2.Items.Add(toolTipItem2);
            this.accordionControlElement_Settings.SuperTip = superToolTip2;
            this.accordionControlElement_Settings.Text = "Settings";
            // 
            // accordionControlElement_ShowMode
            // 
            accordionCheckContextButton1.Id = new System.Guid("40c698e2-fa35-4e38-bc22-f5bcc8066209");
            accordionCheckContextButton1.Name = "accordionCheckContextButton_ShowMode";
            accordionCheckContextButton1.Visibility = DevExpress.Utils.ContextItemVisibility.Visible;
            this.accordionControlElement_ShowMode.ContextButtons.Add(accordionCheckContextButton1);
            this.accordionControlElement_ShowMode.Name = "accordionControlElement_ShowMode";
            this.accordionControlElement_ShowMode.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionControlElement_ShowMode.Text = "Open tiles undocked as movable windows";
            // 
            // accordionControlElement_CloseAll
            // 
            this.accordionControlElement_CloseAll.Name = "accordionControlElement_CloseAll";
            this.accordionControlElement_CloseAll.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionControlElement_CloseAll.Text = "Close open tiles";
            // 
            // accordionControlElement_InlineMenuMode
            // 
            accordionCheckContextButton2.Id = new System.Guid("99324620-3eec-413c-8296-9cb0d7b045c2");
            accordionCheckContextButton2.Name = "accordionContextButton1";
            accordionCheckContextButton2.Visibility = DevExpress.Utils.ContextItemVisibility.Visible;
            this.accordionControlElement_InlineMenuMode.ContextButtons.Add(accordionCheckContextButton2);
            this.accordionControlElement_InlineMenuMode.Name = "accordionControlElement_InlineMenuMode";
            this.accordionControlElement_InlineMenuMode.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.accordionControlElement_InlineMenuMode.Text = "Toggle Inline Menu";
            // 
            // navButton2
            // 
            this.navButton2.Caption = "Main Menu";
            this.navButton2.IsMain = true;
            this.navButton2.Name = "navButton2";
            // 
            // TileBoardLayoutControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelControl_Main);
            this.Name = "TileBoardLayoutControl";
            this.Size = new System.Drawing.Size(816, 568);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Main)).EndInit();
            this.panelControl_Main.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl_Main;
        private DevExpress.XtraBars.Navigation.NavButton navButton2;
        private TileDocumentManager tileDocumentManager;
        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl1;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionControlElement_Tiles;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionControlElement_Settings;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionControlElement_ShowMode;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionControlElement_CloseAll;
        private DevExpress.XtraBars.Navigation.AccordionControlElement accordionControlElement_InlineMenuMode;
    }
}
