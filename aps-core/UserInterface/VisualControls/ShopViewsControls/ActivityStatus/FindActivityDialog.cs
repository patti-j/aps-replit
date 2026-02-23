//using System;

//using PT.ComponentLibrary;
//using PT.ComponentLibrary.Forms;

//namespace ShopViewsControls.ActivityStatus
//{
//    /// <summary>
//    /// Summary description for FindActivityDialog.
//    /// </summary>
//    public class FindActivityDialog : PTStyledForm
//    {
//        #region Declarations
//        private System.Windows.Forms.ImageList imageList1;
//        private System.Windows.Forms.TextBox JobTB;
//        private System.Windows.Forms.Label JobLabel;
//        private System.Windows.Forms.Label label4;
//        private System.Windows.Forms.Label label5;
//        private System.Windows.Forms.Label MoLabel;
//        private System.Windows.Forms.TextBox MoTB;
//        private System.Windows.Forms.Label OpLabel;
//        private System.Windows.Forms.TextBox OpTB;
//        private System.Windows.Forms.Label FoundLabel;
//        private DevExpress.XtraEditors.SimpleButton FindFirstBTN;
//        private DevExpress.XtraEditors.SimpleButton FindNextBTN;
//        private DevExpress.XtraEditors.SimpleButton CloseBTN;
//        private System.ComponentModel.IContainer components;
//        #endregion

//        /// <summary>
//        /// //FOR DESIGNER ONLY, DO NOT USE THIS CONSTRUCTOR
//        /// </summary>
//        public FindActivityDialog()
//        {
//            //
//            // Required for Windows Form Designer support
//            //
//            InitializeComponent();

//            //FOR DESIGNER ONLY, DO NOT USE THIS CONSTRUCTOR
//        }

//        private readonly ActivityFinder finder;

//        public FindActivityDialog(ActivityFinder finder)
//        {
//            //
//            // Required for Windows Form Designer support
//            //
//            InitializeComponent();

//            this.finder = finder;
//        }

//        public override void Localize()
//        {
//            UILocalizationHelper.LocalizeControlsRecursively(Controls);
//        }

//        #region Disposal
//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (components != null)
//                {
//                    components.Dispose();
//                }
//            }
//            base.Dispose(disposing);
//        }
//        #endregion

//        #region Windows Form Designer generated code
//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            this.components = new System.ComponentModel.Container();
//            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof (FindActivityDialog));
//            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
//            this.JobTB = new System.Windows.Forms.TextBox();
//            this.JobLabel = new System.Windows.Forms.Label();
//            this.MoLabel = new System.Windows.Forms.Label();
//            this.MoTB = new System.Windows.Forms.TextBox();
//            this.OpLabel = new System.Windows.Forms.Label();
//            this.OpTB = new System.Windows.Forms.TextBox();
//            this.label4 = new System.Windows.Forms.Label();
//            this.label5 = new System.Windows.Forms.Label();
//            this.FoundLabel = new System.Windows.Forms.Label();
//            this.FindFirstBTN = new DevExpress.XtraEditors.SimpleButton();
//            this.FindNextBTN = new DevExpress.XtraEditors.SimpleButton();
//            this.CloseBTN = new DevExpress.XtraEditors.SimpleButton();
//            this.SuspendLayout();
//            // 
//            // imageList1
//            // 
//            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
//            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
//            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
//            // 
//            // JobTB
//            // 
//            this.JobTB.Location = new System.Drawing.Point(8, 24);
//            this.JobTB.Name = "JobTB";
//            this.JobTB.Size = new System.Drawing.Size(168, 22);
//            this.JobTB.TabIndex = 0;
//            this.JobTB.Text = "";
//            this.JobTB.TextChanged += new System.EventHandler(this.JobTB_TextChanged);
//            // 
//            // JobLabel
//            // 
//            this.JobLabel.Location = new System.Drawing.Point(8, 8);
//            this.JobLabel.Name = "JobLabel";
//            this.JobLabel.Size = new System.Drawing.Size(184, 16);
//            this.JobLabel.TabIndex = 2;
//            this.JobLabel.Text = "Job Name";
//            // 
//            // MoLabel
//            // 
//            this.MoLabel.Location = new System.Drawing.Point(8, 56);
//            this.MoLabel.Name = "MoLabel";
//            this.MoLabel.Size = new System.Drawing.Size(184, 16);
//            this.MoLabel.TabIndex = 4;
//            this.MoLabel.Text = "Manufacturing Order Name";
//            // 
//            // MoTB
//            // 
//            this.MoTB.Location = new System.Drawing.Point(8, 72);
//            this.MoTB.Name = "MoTB";
//            this.MoTB.Size = new System.Drawing.Size(168, 22);
//            this.MoTB.TabIndex = 1;
//            this.MoTB.Text = "";
//            // 
//            // OpLabel
//            // 
//            this.OpLabel.Location = new System.Drawing.Point(8, 104);
//            this.OpLabel.Name = "OpLabel";
//            this.OpLabel.Size = new System.Drawing.Size(184, 16);
//            this.OpLabel.TabIndex = 6;
//            this.OpLabel.Text = "Operation Name";
//            // 
//            // OpTB
//            // 
//            this.OpTB.Location = new System.Drawing.Point(8, 120);
//            this.OpTB.Name = "OpTB";
//            this.OpTB.Size = new System.Drawing.Size(168, 22);
//            this.OpTB.TabIndex = 2;
//            this.OpTB.Text = "";
//            // 
//            // label4
//            // 
//            this.label4.Location = new System.Drawing.Point(184, 72);
//            this.label4.Name = "label4";
//            this.label4.Size = new System.Drawing.Size(64, 16);
//            this.label4.TabIndex = 4;
//            this.label4.Text = "optional";
//            // 
//            // label5
//            // 
//            this.label5.Location = new System.Drawing.Point(184, 120);
//            this.label5.Name = "label5";
//            this.label5.Size = new System.Drawing.Size(64, 16);
//            this.label5.TabIndex = 4;
//            this.label5.Text = "optional";
//            // 
//            // FoundLabel
//            // 
//            this.FoundLabel.ForeColor = System.Drawing.Color.Blue;
//            this.FoundLabel.Location = new System.Drawing.Point(8, 144);
//            this.FoundLabel.Name = "FoundLabel";
//            this.FoundLabel.Size = new System.Drawing.Size(160, 16);
//            this.FoundLabel.TabIndex = 12;
//            // 
//            // FindFirstBTN
//            // 
//            this.FindFirstBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
//            this.FindFirstBTN.DialogResult = System.Windows.Forms.DialogResult.OK;
//            this.FindFirstBTN.Enabled = false;
//            this.FindFirstBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            this.FindFirstBTN.ImageIndex = 0;
//            this.FindFirstBTN.Location = new System.Drawing.Point(4, 168);
//            this.FindFirstBTN.Name = "FindFirstBTN";
//            this.FindFirstBTN.Size = new System.Drawing.Size(80, 24);
//            this.FindFirstBTN.TabIndex = 11;
//            this.FindFirstBTN.Text = "Find First";
//            this.FindFirstBTN.Click += new System.EventHandler(this.FindFirstBTN_Click);
//            // 
//            // FindNextBTN
//            // 
//            this.FindNextBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
//            this.FindNextBTN.DialogResult = System.Windows.Forms.DialogResult.OK;
//            this.FindNextBTN.Enabled = false;
//            this.FindNextBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            this.FindNextBTN.ImageIndex = 0;
//            this.FindNextBTN.Location = new System.Drawing.Point(88, 168);
//            this.FindNextBTN.Name = "FindNextBTN";
//            this.FindNextBTN.Size = new System.Drawing.Size(72, 24);
//            this.FindNextBTN.TabIndex = 9;
//            this.FindNextBTN.Text = "Find Next";
//            this.FindNextBTN.Click += new System.EventHandler(this.FindNextBTN_Click);
//            // 
//            // CloseBTN
//            // 
//            this.CloseBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
//            this.CloseBTN.DialogResult = System.Windows.Forms.DialogResult.Cancel;
//            this.CloseBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            this.CloseBTN.ImageIndex = 0;
//            this.CloseBTN.Location = new System.Drawing.Point(164, 168);
//            this.CloseBTN.Name = "CloseBTN";
//            this.CloseBTN.Size = new System.Drawing.Size(68, 24);
//            this.CloseBTN.TabIndex = 10;
//            this.CloseBTN.Text = "Close";
//            this.CloseBTN.Click += new System.EventHandler(this.CloseBTN_Click);
//            // 
//            // FindActivityDialog
//            // 
//            this.AcceptButton = this.FindNextBTN;
//            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
//            this.CancelButton = this.CloseBTN;
//            this.ClientSize = new System.Drawing.Size(234, 193);
//            this.Controls.Add(this.FoundLabel);
//            this.Controls.Add(this.FindFirstBTN);
//            this.Controls.Add(this.FindNextBTN);
//            this.Controls.Add(this.CloseBTN);
//            this.Controls.Add(this.OpLabel);
//            this.Controls.Add(this.OpTB);
//            this.Controls.Add(this.MoTB);
//            this.Controls.Add(this.JobTB);
//            this.Controls.Add(this.MoLabel);
//            this.Controls.Add(this.JobLabel);
//            this.Controls.Add(this.label4);
//            this.Controls.Add(this.label5);
//            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
//            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
//            this.Name = "FindActivityDialog";
//            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
//            this.Text = "Find an Activity";
//            this.ResumeLayout(false);
//        }
//        #endregion

//        private void JobTB_TextChanged(object sender, System.EventArgs e)
//        {
//            CheckEnabling();
//        }

//        private void CheckEnabling()
//        {
//            if (this.JobTB.Text.Trim().Length == 0)
//            {
//                this.FindFirstBTN.Enabled = false;
//                this.FindNextBTN.Enabled = false;
//            }
//            else
//            {
//                this.FindFirstBTN.Enabled = true;
//                this.FindNextBTN.Enabled = true;
//            }
//        }

//        private void FindFirstBTN_Click(object sender, System.EventArgs e)
//        {
//            PT.UIDefinitions.MultiLevelHourglass.TurnOn();

//            int foundCount = this.finder.FindFirst(this.JobTB.Text.Trim(), this.MoTB.Text.Trim(), this.OpTB.Text.Trim());

//            if (foundCount == 0)
//            {
//                NotFound();
//            }
//            else
//            {
//                this.FoundLabel.Text = String.Format("{0} found", foundCount.ToString());
//            }

//            PT.UIDefinitions.MultiLevelHourglass.TurnOff(false);
//        }

//        private void FindNextBTN_Click(object sender, System.EventArgs e)
//        {
//            PT.UIDefinitions.MultiLevelHourglass.TurnOn();

//            int remainingToFind = this.finder.FindNext(this.JobTB.Text.Trim(), this.MoTB.Text.Trim(), this.OpTB.Text.Trim());
//            if (remainingToFind < 0)
//            {
//                NotFound();
//            }
//            else
//            {
//                if (remainingToFind == 1)
//                {
//                    this.FoundLabel.Text = String.Format("{0} remains", remainingToFind);
//                }
//                else
//                {
//                    this.FoundLabel.Text = String.Format("{0} remain", remainingToFind);
//                }
//            }

//            PT.UIDefinitions.MultiLevelHourglass.TurnOff(false);
//        }

//        private void NotFound()
//        {
//            this.FoundLabel.Text = String.Format("not found");
//            this.JobTB.Focus();
//            this.JobTB.SelectAll();
//        }

//        private void CloseBTN_Click(object sender, System.EventArgs e)
//        {
//            this.Close();
//        }
//    }
//}

