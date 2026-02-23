namespace PT.ScenarioControls.Tiles
{
    partial class TileDocumentManager
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
            components = new System.ComponentModel.Container();
            documentManager1 = new DevExpress.XtraBars.Docking2010.DocumentManager(components);
            tileViewMain = new DevExpress.XtraBars.Docking2010.Views.Widget.WidgetView(components);
            ((System.ComponentModel.ISupportInitialize)documentManager1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tileViewMain).BeginInit();
            SuspendLayout();
            // 
            // documentManager1
            // 
            documentManager1.ContainerControl = this;
            documentManager1.View = tileViewMain;
            documentManager1.ViewCollection.AddRange(new DevExpress.XtraBars.Docking2010.Views.BaseView[] { tileViewMain });
            // 
            // tileViewMain
            // 
            tileViewMain.DocumentSelectorProperties.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            tileViewMain.LayoutMode = DevExpress.XtraBars.Docking2010.Views.Widget.LayoutMode.FreeLayout;
            tileViewMain.RootContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
            tileViewMain.UseDocumentSelector = DevExpress.Utils.DefaultBoolean.False;
            tileViewMain.UseSnappingEmulation = DevExpress.Utils.DefaultBoolean.True;
            // 
            // TileDocumentManager
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Name = "TileDocumentManager";
            Size = new System.Drawing.Size(788, 627);
            ((System.ComponentModel.ISupportInitialize)documentManager1).EndInit();
            ((System.ComponentModel.ISupportInitialize)tileViewMain).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraBars.Docking2010.DocumentManager documentManager1;
        private DevExpress.XtraBars.Docking2010.Views.Widget.WidgetView tileViewMain;
    }
}
