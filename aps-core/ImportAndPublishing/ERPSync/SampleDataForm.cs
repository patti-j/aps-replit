using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Runtime.Remoting;
using System.Windows.Forms;
using Infragistics.Win.UltraWinEditors;

using PT.Broadcasting;
using PT.ERPTransmissions;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.Transmissions;
namespace UI
{
	public class SampleDataForm : System.Windows.Forms.Form
	{
		#region Declarations

		private System.Windows.Forms.TabControl tabControl1;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor PlantBox;
		private System.Windows.Forms.Label PlantLabel;
		private System.Windows.Forms.Label label1;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor jobsNE;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor seedNE;
		private System.Windows.Forms.CheckBox cbJobMax;
		private System.Windows.Forms.Label label9;

		private Infragistics.Win.UltraWinEditors.UltraNumericEditor UsersBox;
		private System.Windows.Forms.Label UsersLabel;
		private System.Windows.Forms.GroupBox ResourcesGroupBox;
		private System.Windows.Forms.GroupBox groupBox5;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor MachineBox;
		private System.Windows.Forms.Label DepartmentLabel;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor DepartmentBox;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor CapabilityBox;
		private System.Windows.Forms.Label CapabilityLabel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.DateTimePicker maxDueDateDTP;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label4;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor maxCycleTimeNE;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor minCycleTimeNE;
		private System.Windows.Forms.Label label5;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor maxCapabilityIdNE;
		private System.Windows.Forms.Label label3;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor maxOpsNE;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor maxRequiredQtyNE;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label2;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor maxMOsNE;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox OnTop;
		private System.Windows.Forms.Button generateButton;
		private System.Windows.Forms.TabPage SummaryTabPage;
		private System.Windows.Forms.TabPage PlantsTabPage;
		private System.Windows.Forms.TabPage JobsTabPage;
		private System.Windows.Forms.TabPage OtherTabPage;
		private System.Windows.Forms.CheckBox CapacityIntervalsCB;
		private System.Windows.Forms.ComboBox DatasetComboBox;
		private System.Windows.Forms.CheckBox testCB;
		private System.Windows.Forms.CheckBox IncludeFinishesCB;
		private System.Windows.Forms.Label ResourceLabel;
		private System.Windows.Forms.CheckBox RecurringCapacityIntervalsCB;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.RadioButton rbBeginReplay;
		private System.Windows.Forms.Button btnOptimize;
		private System.Windows.Forms.RadioButton rbExport;
		private System.Windows.Forms.RadioButton rbMove;
		private System.Windows.Forms.RadioButton rbExpedite;
		private System.Windows.Forms.RadioButton rbSimulate;
		private System.Windows.Forms.RadioButton rbCompress;
		private System.Windows.Forms.RadioButton rbAdvanceClock;
		private System.Windows.Forms.TabPage MaterialsTabPage;
		private System.Windows.Forms.Label label10;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor ItemCountEditor;
		private System.Windows.Forms.Label label11;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor WarehouseCountEditor;
		private System.Windows.Forms.CheckBox IncludeMaterialRequirementsCB;
		private System.Windows.Forms.CheckBox IncludeProductsCB;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private Infragistics.Win.UltraWinEditors.UltraNumericEditor PurchaseToStockCountEditor;
        private System.Windows.Forms.RadioButton rbPerformImport;
		private System.ComponentModel.IContainer components = null;

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new SampleDataForm());
		}

		const int MaxDaysDueFromNow=180;
		DateTime simulationStartTime;
		DateTime sundayOfWeek;
		bool fixedReleaseDate=false;
		DateTime fixedReleaseDateTime;

		PTBroadcaster ptBroadcaster;
		int connectionNbr;
		long creationTicks;

		void CreateBroadcasterConnection()
		{
			ptBroadcaster=new PTBroadcaster();
			connectionNbr=-1;
			creationTicks=-1;
			ptBroadcaster.CreateConnection(Connection.CreateDescription(), out connectionNbr, out creationTicks);
		}

		#region Construction
		public SampleDataForm()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			string config_file_name=(System.Environment.GetCommandLineArgs())[0]+".config";
			RemotingConfiguration.Configure(config_file_name);

			// Set the working directory.
			Hashtable PTSystemConfig=(Hashtable) ConfigurationSettings.GetConfig("PTSystem");
			string workingDirectory=(string)PTSystemConfig["workingDirectory"];
			PT.Broadcasting.WorkingDirectory.Directory=workingDirectory;

			//Initialize labels
			//Resources
			this.PlantLabel.Text="Plants";
			this.DepartmentLabel.Text="Departments";
			this.ResourceLabel.Text="Resources";
			this.CapabilityLabel.Text="Of each type of Capability";
			//Users
			this.UsersLabel.Text="Users";

			//Set default values
			//Resources
			this.Plants=1;
			this.Departments=1;

			this.Capabilities=10;
			
			//Users
			this.Users=100;

			//Set default Dataset
			/**/		this.DatasetComboBox.SelectedIndex=1;

			/**/		this.Machines=2;
			/**/		this.Jobs=100;
			/**/		this.MaxMOs=2;
			/**/		this.MaxOps=5;
			/**/		fixedReleaseDate=true;
			/**/		fixedReleaseDateTime=DateTime.Now;

			// Always default this to the Sunday of this week.
			DateTime now=DateTime.Now;
			DateTime dt=new DateTime(now.Year, now.Month, now.Day);
			int fallBack=(int)now.DayOfWeek;
			sundayOfWeek=dt-(new TimeSpan(fallBack, 0, 0, 0));

			// Start the simulation 1 week after the Sunday of this week.
			simulationStartTime=sundayOfWeek+new TimeSpan(7, 0, 0, 0);
			MaxDueDate=sundayOfWeek+new TimeSpan(MaxDaysDueFromNow, 0, 0, 0);

			this.MaxRequiredQty=100;
			this.MaxCapabilityId=10;
			this.MinCycleTime=1;
			this.MaxCycleTimeNE=10;

			//			Random r=new Random();
			//			Seed=r.Next(1, 1000000);
			Seed=23424;

			//Put form in lower right corner out of the way
			int x=Screen.PrimaryScreen.Bounds.Width-this.Width;
			int y=Screen.PrimaryScreen.WorkingArea.Height-this.Height;

			this.Location=new Point(x,y);
			CapacityIntervalsCB.Checked=false;
			RecurringCapacityIntervalsCB.Checked=true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SampleDataForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.SummaryTabPage = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.rbPerformImport = new System.Windows.Forms.RadioButton();
            this.rbBeginReplay = new System.Windows.Forms.RadioButton();
            this.btnOptimize = new System.Windows.Forms.Button();
            this.rbExport = new System.Windows.Forms.RadioButton();
            this.rbMove = new System.Windows.Forms.RadioButton();
            this.rbExpedite = new System.Windows.Forms.RadioButton();
            this.rbSimulate = new System.Windows.Forms.RadioButton();
            this.rbCompress = new System.Windows.Forms.RadioButton();
            this.rbAdvanceClock = new System.Windows.Forms.RadioButton();
            this.testCB = new System.Windows.Forms.CheckBox();
            this.DatasetComboBox = new System.Windows.Forms.ComboBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.OnTop = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.seedNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.cbJobMax = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.jobsNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.PlantBox = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.PlantLabel = new System.Windows.Forms.Label();
            this.PlantsTabPage = new System.Windows.Forms.TabPage();
            this.RecurringCapacityIntervalsCB = new System.Windows.Forms.CheckBox();
            this.CapacityIntervalsCB = new System.Windows.Forms.CheckBox();
            this.CapabilityBox = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.CapabilityLabel = new System.Windows.Forms.Label();
            this.ResourcesGroupBox = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.MachineBox = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.ResourceLabel = new System.Windows.Forms.Label();
            this.DepartmentLabel = new System.Windows.Forms.Label();
            this.DepartmentBox = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.JobsTabPage = new System.Windows.Forms.TabPage();
            this.IncludeFinishesCB = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.maxDueDateDTP = new System.Windows.Forms.DateTimePicker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.maxCycleTimeNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.minCycleTimeNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.label5 = new System.Windows.Forms.Label();
            this.maxCapabilityIdNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.label3 = new System.Windows.Forms.Label();
            this.maxOpsNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.maxRequiredQtyNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.maxMOsNE = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.label7 = new System.Windows.Forms.Label();
            this.MaterialsTabPage = new System.Windows.Forms.TabPage();
            this.label13 = new System.Windows.Forms.Label();
            this.PurchaseToStockCountEditor = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.label12 = new System.Windows.Forms.Label();
            this.IncludeMaterialRequirementsCB = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.WarehouseCountEditor = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.label10 = new System.Windows.Forms.Label();
            this.ItemCountEditor = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.IncludeProductsCB = new System.Windows.Forms.CheckBox();
            this.OtherTabPage = new System.Windows.Forms.TabPage();
            this.UsersBox = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.UsersLabel = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.SummaryTabPage.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.seedNE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.jobsNE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlantBox)).BeginInit();
            this.PlantsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CapabilityBox)).BeginInit();
            this.ResourcesGroupBox.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MachineBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DepartmentBox)).BeginInit();
            this.JobsTabPage.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxCycleTimeNE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minCycleTimeNE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxCapabilityIdNE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxOpsNE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxRequiredQtyNE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxMOsNE)).BeginInit();
            this.MaterialsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PurchaseToStockCountEditor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WarehouseCountEditor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemCountEditor)).BeginInit();
            this.OtherTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UsersBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.SummaryTabPage);
            this.tabControl1.Controls.Add(this.PlantsTabPage);
            this.tabControl1.Controls.Add(this.JobsTabPage);
            this.tabControl1.Controls.Add(this.MaterialsTabPage);
            this.tabControl1.Controls.Add(this.OtherTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(246, 419);
            this.tabControl1.TabIndex = 21;
            // 
            // SummaryTabPage
            // 
            this.SummaryTabPage.Controls.Add(this.groupBox6);
            this.SummaryTabPage.Controls.Add(this.testCB);
            this.SummaryTabPage.Controls.Add(this.DatasetComboBox);
            this.SummaryTabPage.Controls.Add(this.generateButton);
            this.SummaryTabPage.Controls.Add(this.OnTop);
            this.SummaryTabPage.Controls.Add(this.label9);
            this.SummaryTabPage.Controls.Add(this.seedNE);
            this.SummaryTabPage.Controls.Add(this.cbJobMax);
            this.SummaryTabPage.Controls.Add(this.label1);
            this.SummaryTabPage.Controls.Add(this.jobsNE);
            this.SummaryTabPage.Controls.Add(this.PlantBox);
            this.SummaryTabPage.Controls.Add(this.PlantLabel);
            this.SummaryTabPage.Location = new System.Drawing.Point(4, 22);
            this.SummaryTabPage.Name = "SummaryTabPage";
            this.SummaryTabPage.Size = new System.Drawing.Size(238, 393);
            this.SummaryTabPage.TabIndex = 0;
            this.SummaryTabPage.Text = "Summary";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.rbPerformImport);
            this.groupBox6.Controls.Add(this.rbBeginReplay);
            this.groupBox6.Controls.Add(this.btnOptimize);
            this.groupBox6.Controls.Add(this.rbExport);
            this.groupBox6.Controls.Add(this.rbMove);
            this.groupBox6.Controls.Add(this.rbExpedite);
            this.groupBox6.Controls.Add(this.rbSimulate);
            this.groupBox6.Controls.Add(this.rbCompress);
            this.groupBox6.Controls.Add(this.rbAdvanceClock);
            this.groupBox6.Location = new System.Drawing.Point(7, 173);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(206, 179);
            this.groupBox6.TabIndex = 39;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Perform Actions";
            // 
            // rbPerformImport
            // 
            this.rbPerformImport.Location = new System.Drawing.Point(112, 88);
            this.rbPerformImport.Name = "rbPerformImport";
            this.rbPerformImport.Size = new System.Drawing.Size(87, 21);
            this.rbPerformImport.TabIndex = 54;
            this.rbPerformImport.Text = "Perform Import";
            // 
            // rbBeginReplay
            // 
            this.rbBeginReplay.Location = new System.Drawing.Point(113, 64);
            this.rbBeginReplay.Name = "rbBeginReplay";
            this.rbBeginReplay.Size = new System.Drawing.Size(87, 21);
            this.rbBeginReplay.TabIndex = 53;
            this.rbBeginReplay.Text = "BeginReplay";
            // 
            // btnOptimize
            // 
            this.btnOptimize.Location = new System.Drawing.Point(40, 145);
            this.btnOptimize.Name = "btnOptimize";
            this.btnOptimize.Size = new System.Drawing.Size(107, 28);
            this.btnOptimize.TabIndex = 46;
            this.btnOptimize.Text = "Perform Action";
            this.btnOptimize.Click += new System.EventHandler(this.btnOptimize_Click);
            // 
            // rbExport
            // 
            this.rbExport.Location = new System.Drawing.Point(13, 88);
            this.rbExport.Name = "rbExport";
            this.rbExport.Size = new System.Drawing.Size(87, 24);
            this.rbExport.TabIndex = 52;
            this.rbExport.Text = "Export to DB";
            // 
            // rbMove
            // 
            this.rbMove.Location = new System.Drawing.Point(113, 40);
            this.rbMove.Name = "rbMove";
            this.rbMove.Size = new System.Drawing.Size(67, 24);
            this.rbMove.TabIndex = 51;
            this.rbMove.Text = "Move";
            // 
            // rbExpedite
            // 
            this.rbExpedite.Location = new System.Drawing.Point(113, 16);
            this.rbExpedite.Name = "rbExpedite";
            this.rbExpedite.Size = new System.Drawing.Size(87, 24);
            this.rbExpedite.TabIndex = 50;
            this.rbExpedite.Text = "Expedite";
            // 
            // rbSimulate
            // 
            this.rbSimulate.Checked = true;
            this.rbSimulate.Location = new System.Drawing.Point(13, 16);
            this.rbSimulate.Name = "rbSimulate";
            this.rbSimulate.Size = new System.Drawing.Size(94, 24);
            this.rbSimulate.TabIndex = 47;
            this.rbSimulate.TabStop = true;
            this.rbSimulate.Text = "Simulate";
            // 
            // rbCompress
            // 
            this.rbCompress.Location = new System.Drawing.Point(13, 40);
            this.rbCompress.Name = "rbCompress";
            this.rbCompress.Size = new System.Drawing.Size(94, 23);
            this.rbCompress.TabIndex = 48;
            this.rbCompress.Text = "Compress";
            // 
            // rbAdvanceClock
            // 
            this.rbAdvanceClock.Location = new System.Drawing.Point(13, 64);
            this.rbAdvanceClock.Name = "rbAdvanceClock";
            this.rbAdvanceClock.Size = new System.Drawing.Size(94, 25);
            this.rbAdvanceClock.TabIndex = 49;
            this.rbAdvanceClock.Text = "Advance Clock";
            // 
            // testCB
            // 
            this.testCB.Location = new System.Drawing.Point(8, 104);
            this.testCB.Name = "testCB";
            this.testCB.Size = new System.Drawing.Size(167, 24);
            this.testCB.TabIndex = 38;
            this.testCB.Text = "China Cabinet Product Only";
            // 
            // DatasetComboBox
            // 
            this.DatasetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DatasetComboBox.Items.AddRange(new object[] {
            "Random Data",
            "Furniture Factory"});
            this.DatasetComboBox.Location = new System.Drawing.Point(8, 8);
            this.DatasetComboBox.Name = "DatasetComboBox";
            this.DatasetComboBox.Size = new System.Drawing.Size(127, 21);
            this.DatasetComboBox.TabIndex = 37;
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(47, 132);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(106, 27);
            this.generateButton.TabIndex = 35;
            this.generateButton.Text = "&Generate Data";
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // OnTop
            // 
            this.OnTop.Checked = true;
            this.OnTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OnTop.Location = new System.Drawing.Point(9, 358);
            this.OnTop.Name = "OnTop";
            this.OnTop.Size = new System.Drawing.Size(166, 27);
            this.OnTop.TabIndex = 34;
            this.OnTop.Text = "Keep this window on top";
            this.OnTop.CheckedChanged += new System.EventHandler(this.OnTop_CheckedChanged);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(80, 83);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(31, 15);
            this.label9.TabIndex = 33;
            this.label9.Text = "Seed:";
            // 
            // seedNE
            // 
            this.seedNE.AlwaysInEditMode = true;
            this.seedNE.Location = new System.Drawing.Point(127, 83);
            this.seedNE.Name = "seedNE";
            this.seedNE.PromptChar = ' ';
            this.seedNE.Size = new System.Drawing.Size(87, 21);
            this.seedNE.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.seedNE.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.seedNE.TabIndex = 32;
            // 
            // cbJobMax
            // 
            this.cbJobMax.Location = new System.Drawing.Point(8, 80);
            this.cbJobMax.Name = "cbJobMax";
            this.cbJobMax.Size = new System.Drawing.Size(71, 23);
            this.cbJobMax.TabIndex = 31;
            this.cbJobMax.Text = "Random";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(104, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 23);
            this.label1.TabIndex = 30;
            this.label1.Text = "Jobs";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // jobsNE
            // 
            this.jobsNE.AlwaysInEditMode = true;
            this.jobsNE.Location = new System.Drawing.Point(8, 32);
            this.jobsNE.MaxValue = 10000;
            this.jobsNE.MinValue = 0;
            this.jobsNE.Name = "jobsNE";
            this.jobsNE.PromptChar = ' ';
            this.jobsNE.Size = new System.Drawing.Size(88, 21);
            this.jobsNE.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.jobsNE.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.jobsNE.TabIndex = 29;
            // 
            // PlantBox
            // 
            this.PlantBox.AlwaysInEditMode = true;
            this.PlantBox.Location = new System.Drawing.Point(8, 56);
            this.PlantBox.MaxValue = 10000;
            this.PlantBox.MinValue = 0;
            this.PlantBox.Name = "PlantBox";
            this.PlantBox.PromptChar = ' ';
            this.PlantBox.Size = new System.Drawing.Size(88, 21);
            this.PlantBox.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.PlantBox.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.PlantBox.TabIndex = 27;
            // 
            // PlantLabel
            // 
            this.PlantLabel.Location = new System.Drawing.Point(104, 56);
            this.PlantLabel.Name = "PlantLabel";
            this.PlantLabel.Size = new System.Drawing.Size(40, 16);
            this.PlantLabel.TabIndex = 28;
            this.PlantLabel.Text = "Plants";
            // 
            // PlantsTabPage
            // 
            this.PlantsTabPage.Controls.Add(this.RecurringCapacityIntervalsCB);
            this.PlantsTabPage.Controls.Add(this.CapacityIntervalsCB);
            this.PlantsTabPage.Controls.Add(this.CapabilityBox);
            this.PlantsTabPage.Controls.Add(this.CapabilityLabel);
            this.PlantsTabPage.Controls.Add(this.ResourcesGroupBox);
            this.PlantsTabPage.Location = new System.Drawing.Point(4, 22);
            this.PlantsTabPage.Name = "PlantsTabPage";
            this.PlantsTabPage.Size = new System.Drawing.Size(238, 393);
            this.PlantsTabPage.TabIndex = 1;
            this.PlantsTabPage.Text = "Plants";
            // 
            // RecurringCapacityIntervalsCB
            // 
            this.RecurringCapacityIntervalsCB.Location = new System.Drawing.Point(20, 201);
            this.RecurringCapacityIntervalsCB.Name = "RecurringCapacityIntervalsCB";
            this.RecurringCapacityIntervalsCB.Size = new System.Drawing.Size(152, 28);
            this.RecurringCapacityIntervalsCB.TabIndex = 36;
            this.RecurringCapacityIntervalsCB.Text = "Create Recurring Capacity Intervals";
            // 
            // CapacityIntervalsCB
            // 
            this.CapacityIntervalsCB.Location = new System.Drawing.Point(20, 176);
            this.CapacityIntervalsCB.Name = "CapacityIntervalsCB";
            this.CapacityIntervalsCB.Size = new System.Drawing.Size(152, 24);
            this.CapacityIntervalsCB.TabIndex = 35;
            this.CapacityIntervalsCB.Text = "Create Capacity Intervals";
            // 
            // CapabilityBox
            // 
            this.CapabilityBox.AlwaysInEditMode = true;
            this.CapabilityBox.Location = new System.Drawing.Point(16, 125);
            this.CapabilityBox.MaxValue = 10000;
            this.CapabilityBox.MinValue = 0;
            this.CapabilityBox.Name = "CapabilityBox";
            this.CapabilityBox.PromptChar = ' ';
            this.CapabilityBox.Size = new System.Drawing.Size(88, 21);
            this.CapabilityBox.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.CapabilityBox.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.CapabilityBox.TabIndex = 20;
            // 
            // CapabilityLabel
            // 
            this.CapabilityLabel.Location = new System.Drawing.Point(120, 125);
            this.CapabilityLabel.Name = "CapabilityLabel";
            this.CapabilityLabel.Size = new System.Drawing.Size(64, 24);
            this.CapabilityLabel.TabIndex = 21;
            this.CapabilityLabel.Text = "Capabilities";
            // 
            // ResourcesGroupBox
            // 
            this.ResourcesGroupBox.Controls.Add(this.groupBox5);
            this.ResourcesGroupBox.Controls.Add(this.DepartmentLabel);
            this.ResourcesGroupBox.Controls.Add(this.DepartmentBox);
            this.ResourcesGroupBox.Location = new System.Drawing.Point(8, 8);
            this.ResourcesGroupBox.Name = "ResourcesGroupBox";
            this.ResourcesGroupBox.Size = new System.Drawing.Size(184, 110);
            this.ResourcesGroupBox.TabIndex = 2;
            this.ResourcesGroupBox.TabStop = false;
            this.ResourcesGroupBox.Text = "Per Plant";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.MachineBox);
            this.groupBox5.Controls.Add(this.ResourceLabel);
            this.groupBox5.Location = new System.Drawing.Point(8, 40);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(168, 64);
            this.groupBox5.TabIndex = 1;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Per Department";
            // 
            // MachineBox
            // 
            this.MachineBox.AlwaysInEditMode = true;
            this.MachineBox.Location = new System.Drawing.Point(8, 16);
            this.MachineBox.MaxValue = 10000;
            this.MachineBox.MinValue = 0;
            this.MachineBox.Name = "MachineBox";
            this.MachineBox.PromptChar = ' ';
            this.MachineBox.Size = new System.Drawing.Size(88, 21);
            this.MachineBox.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.MachineBox.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.MachineBox.TabIndex = 0;
            // 
            // ResourceLabel
            // 
            this.ResourceLabel.Location = new System.Drawing.Point(100, 18);
            this.ResourceLabel.Name = "ResourceLabel";
            this.ResourceLabel.Size = new System.Drawing.Size(53, 20);
            this.ResourceLabel.TabIndex = 1;
            // 
            // DepartmentLabel
            // 
            this.DepartmentLabel.Location = new System.Drawing.Point(104, 18);
            this.DepartmentLabel.Name = "DepartmentLabel";
            this.DepartmentLabel.Size = new System.Drawing.Size(72, 16);
            this.DepartmentLabel.TabIndex = 18;
            this.DepartmentLabel.Text = "Departments";
            // 
            // DepartmentBox
            // 
            this.DepartmentBox.AlwaysInEditMode = true;
            this.DepartmentBox.Location = new System.Drawing.Point(8, 16);
            this.DepartmentBox.MaxValue = 10000;
            this.DepartmentBox.MinValue = 0;
            this.DepartmentBox.Name = "DepartmentBox";
            this.DepartmentBox.PromptChar = ' ';
            this.DepartmentBox.Size = new System.Drawing.Size(88, 21);
            this.DepartmentBox.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.DepartmentBox.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.DepartmentBox.TabIndex = 0;
            // 
            // JobsTabPage
            // 
            this.JobsTabPage.Controls.Add(this.IncludeFinishesCB);
            this.JobsTabPage.Controls.Add(this.groupBox1);
            this.JobsTabPage.Location = new System.Drawing.Point(4, 22);
            this.JobsTabPage.Name = "JobsTabPage";
            this.JobsTabPage.Size = new System.Drawing.Size(238, 393);
            this.JobsTabPage.TabIndex = 2;
            this.JobsTabPage.Text = "Jobs";
            // 
            // IncludeFinishesCB
            // 
            this.IncludeFinishesCB.Checked = true;
            this.IncludeFinishesCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IncludeFinishesCB.Location = new System.Drawing.Point(13, 270);
            this.IncludeFinishesCB.Name = "IncludeFinishesCB";
            this.IncludeFinishesCB.Size = new System.Drawing.Size(167, 20);
            this.IncludeFinishesCB.TabIndex = 6;
            this.IncludeFinishesCB.Text = "Activity Finishes (random)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.maxDueDateDTP);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(8, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(208, 257);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Job";
            // 
            // maxDueDateDTP
            // 
            this.maxDueDateDTP.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.maxDueDateDTP.Location = new System.Drawing.Point(8, 16);
            this.maxDueDateDTP.MaxDate = new System.DateTime(2099, 12, 31, 0, 0, 0, 0);
            this.maxDueDateDTP.MinDate = new System.DateTime(2003, 12, 14, 0, 0, 0, 0);
            this.maxDueDateDTP.Name = "maxDueDateDTP";
            this.maxDueDateDTP.Size = new System.Drawing.Size(112, 20);
            this.maxDueDateDTP.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.maxMOsNE);
            this.groupBox2.Location = new System.Drawing.Point(8, 40);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(192, 210);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Per Job";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.maxOpsNE);
            this.groupBox3.Controls.Add(this.maxRequiredQtyNE);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Location = new System.Drawing.Point(8, 40);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(176, 161);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Per Manufacturing Order";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.maxCycleTimeNE);
            this.groupBox4.Controls.Add(this.minCycleTimeNE);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.maxCapabilityIdNE);
            this.groupBox4.Location = new System.Drawing.Point(8, 64);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(160, 88);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Operation";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(96, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 23);
            this.label6.TabIndex = 12;
            this.label6.Text = "Cycle Time";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(96, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 23);
            this.label4.TabIndex = 11;
            this.label4.Text = "Cycle Time";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // maxCycleTimeNE
            // 
            this.maxCycleTimeNE.Location = new System.Drawing.Point(8, 64);
            this.maxCycleTimeNE.MinValue = 0;
            this.maxCycleTimeNE.Name = "maxCycleTimeNE";
            this.maxCycleTimeNE.Size = new System.Drawing.Size(88, 21);
            this.maxCycleTimeNE.TabIndex = 2;
            // 
            // minCycleTimeNE
            // 
            this.minCycleTimeNE.Location = new System.Drawing.Point(8, 40);
            this.minCycleTimeNE.MinValue = 0;
            this.minCycleTimeNE.Name = "minCycleTimeNE";
            this.minCycleTimeNE.Size = new System.Drawing.Size(88, 21);
            this.minCycleTimeNE.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(96, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 23);
            this.label5.TabIndex = 8;
            this.label5.Text = "Capability Id";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // maxCapabilityIdNE
            // 
            this.maxCapabilityIdNE.AlwaysInEditMode = true;
            this.maxCapabilityIdNE.Location = new System.Drawing.Point(8, 16);
            this.maxCapabilityIdNE.MaxValue = 1000;
            this.maxCapabilityIdNE.MinValue = 1;
            this.maxCapabilityIdNE.Name = "maxCapabilityIdNE";
            this.maxCapabilityIdNE.PromptChar = ' ';
            this.maxCapabilityIdNE.Size = new System.Drawing.Size(88, 21);
            this.maxCapabilityIdNE.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.maxCapabilityIdNE.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.maxCapabilityIdNE.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(96, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 23);
            this.label3.TabIndex = 5;
            this.label3.Text = "Operations";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // maxOpsNE
            // 
            this.maxOpsNE.AlwaysInEditMode = true;
            this.maxOpsNE.Location = new System.Drawing.Point(8, 16);
            this.maxOpsNE.MaxValue = 10000;
            this.maxOpsNE.MinValue = 1;
            this.maxOpsNE.Name = "maxOpsNE";
            this.maxOpsNE.PromptChar = ' ';
            this.maxOpsNE.Size = new System.Drawing.Size(88, 21);
            this.maxOpsNE.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.maxOpsNE.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.maxOpsNE.TabIndex = 0;
            // 
            // maxRequiredQtyNE
            // 
            this.maxRequiredQtyNE.Location = new System.Drawing.Point(8, 40);
            this.maxRequiredQtyNE.MaxValue = 10000;
            this.maxRequiredQtyNE.MinValue = 0;
            this.maxRequiredQtyNE.Name = "maxRequiredQtyNE";
            this.maxRequiredQtyNE.Size = new System.Drawing.Size(88, 21);
            this.maxRequiredQtyNE.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(96, 40);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 23);
            this.label8.TabIndex = 5;
            this.label8.Text = "Required Qty";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(96, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 23);
            this.label2.TabIndex = 4;
            this.label2.Text = "Manufacturing Orders";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // maxMOsNE
            // 
            this.maxMOsNE.AlwaysInEditMode = true;
            this.maxMOsNE.Location = new System.Drawing.Point(8, 16);
            this.maxMOsNE.MaxValue = 10000;
            this.maxMOsNE.MinValue = 1;
            this.maxMOsNE.Name = "maxMOsNE";
            this.maxMOsNE.PromptChar = ' ';
            this.maxMOsNE.Size = new System.Drawing.Size(88, 21);
            this.maxMOsNE.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.maxMOsNE.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.maxMOsNE.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(120, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 23);
            this.label7.TabIndex = 7;
            this.label7.Text = "Due Date";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MaterialsTabPage
            // 
            this.MaterialsTabPage.Controls.Add(this.label13);
            this.MaterialsTabPage.Controls.Add(this.PurchaseToStockCountEditor);
            this.MaterialsTabPage.Controls.Add(this.label12);
            this.MaterialsTabPage.Controls.Add(this.IncludeMaterialRequirementsCB);
            this.MaterialsTabPage.Controls.Add(this.label11);
            this.MaterialsTabPage.Controls.Add(this.WarehouseCountEditor);
            this.MaterialsTabPage.Controls.Add(this.label10);
            this.MaterialsTabPage.Controls.Add(this.ItemCountEditor);
            this.MaterialsTabPage.Controls.Add(this.IncludeProductsCB);
            this.MaterialsTabPage.Location = new System.Drawing.Point(4, 22);
            this.MaterialsTabPage.Name = "MaterialsTabPage";
            this.MaterialsTabPage.Size = new System.Drawing.Size(238, 393);
            this.MaterialsTabPage.TabIndex = 4;
            this.MaterialsTabPage.Text = "Materials";
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(113, 125);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(114, 22);
            this.label13.TabIndex = 38;
            this.label13.Text = "Purchases to stock";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PurchaseToStockCountEditor
            // 
            this.PurchaseToStockCountEditor.AlwaysInEditMode = true;
            this.PurchaseToStockCountEditor.Location = new System.Drawing.Point(20, 125);
            this.PurchaseToStockCountEditor.MaxValue = 100000;
            this.PurchaseToStockCountEditor.MinValue = 0;
            this.PurchaseToStockCountEditor.Name = "PurchaseToStockCountEditor";
            this.PurchaseToStockCountEditor.PromptChar = ' ';
            this.PurchaseToStockCountEditor.Size = new System.Drawing.Size(87, 21);
            this.PurchaseToStockCountEditor.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.PurchaseToStockCountEditor.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.PurchaseToStockCountEditor.TabIndex = 37;
            this.PurchaseToStockCountEditor.Value = 300;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.Location = new System.Drawing.Point(13, 14);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(199, 35);
            this.label12.TabIndex = 36;
            this.label12.Text = "These options only affect Random Data, not Furniture Factory.";
            // 
            // IncludeMaterialRequirementsCB
            // 
            this.IncludeMaterialRequirementsCB.Checked = true;
            this.IncludeMaterialRequirementsCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IncludeMaterialRequirementsCB.Location = new System.Drawing.Point(20, 166);
            this.IncludeMaterialRequirementsCB.Name = "IncludeMaterialRequirementsCB";
            this.IncludeMaterialRequirementsCB.Size = new System.Drawing.Size(207, 21);
            this.IncludeMaterialRequirementsCB.TabIndex = 35;
            this.IncludeMaterialRequirementsCB.Text = "Include Material Requirments in Jobs";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(113, 90);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(80, 23);
            this.label11.TabIndex = 34;
            this.label11.Text = "Warehouses";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WarehouseCountEditor
            // 
            this.WarehouseCountEditor.AlwaysInEditMode = true;
            this.WarehouseCountEditor.Location = new System.Drawing.Point(20, 90);
            this.WarehouseCountEditor.MaxValue = 100000;
            this.WarehouseCountEditor.MinValue = 0;
            this.WarehouseCountEditor.Name = "WarehouseCountEditor";
            this.WarehouseCountEditor.PromptChar = ' ';
            this.WarehouseCountEditor.Size = new System.Drawing.Size(87, 21);
            this.WarehouseCountEditor.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.WarehouseCountEditor.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.WarehouseCountEditor.TabIndex = 33;
            this.WarehouseCountEditor.Value = 25;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(113, 55);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(32, 23);
            this.label10.TabIndex = 32;
            this.label10.Text = "Items";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ItemCountEditor
            // 
            this.ItemCountEditor.AlwaysInEditMode = true;
            this.ItemCountEditor.Location = new System.Drawing.Point(20, 55);
            this.ItemCountEditor.MaxValue = 100000;
            this.ItemCountEditor.MinValue = 0;
            this.ItemCountEditor.Name = "ItemCountEditor";
            this.ItemCountEditor.PromptChar = ' ';
            this.ItemCountEditor.Size = new System.Drawing.Size(87, 21);
            this.ItemCountEditor.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.ItemCountEditor.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.ItemCountEditor.TabIndex = 31;
            this.ItemCountEditor.Value = 500;
            // 
            // IncludeProductsCB
            // 
            this.IncludeProductsCB.Checked = true;
            this.IncludeProductsCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IncludeProductsCB.Location = new System.Drawing.Point(20, 201);
            this.IncludeProductsCB.Name = "IncludeProductsCB";
            this.IncludeProductsCB.Size = new System.Drawing.Size(180, 21);
            this.IncludeProductsCB.TabIndex = 35;
            this.IncludeProductsCB.Text = "Include Products in Jobs";
            // 
            // OtherTabPage
            // 
            this.OtherTabPage.Controls.Add(this.UsersBox);
            this.OtherTabPage.Controls.Add(this.UsersLabel);
            this.OtherTabPage.Location = new System.Drawing.Point(4, 22);
            this.OtherTabPage.Name = "OtherTabPage";
            this.OtherTabPage.Size = new System.Drawing.Size(238, 393);
            this.OtherTabPage.TabIndex = 3;
            this.OtherTabPage.Text = "Other";
            // 
            // UsersBox
            // 
            this.UsersBox.AlwaysInEditMode = true;
            this.UsersBox.Location = new System.Drawing.Point(16, 16);
            this.UsersBox.MaxValue = 10000;
            this.UsersBox.MinValue = 0;
            this.UsersBox.Name = "UsersBox";
            this.UsersBox.PromptChar = ' ';
            this.UsersBox.Size = new System.Drawing.Size(88, 21);
            this.UsersBox.SpinButtonAlignment = Infragistics.Win.ButtonAlignment.Left;
            this.UsersBox.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            this.UsersBox.TabIndex = 19;
            // 
            // UsersLabel
            // 
            this.UsersLabel.Location = new System.Drawing.Point(112, 18);
            this.UsersLabel.Name = "UsersLabel";
            this.UsersLabel.Size = new System.Drawing.Size(48, 16);
            this.UsersLabel.TabIndex = 20;
            this.UsersLabel.Text = "Users";
            // 
            // SampleDataForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(246, 419);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SampleDataForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "PlanetTogether - Sample Data Generator";
            this.TopMost = true;
            this.tabControl1.ResumeLayout(false);
            this.SummaryTabPage.ResumeLayout(false);
            this.SummaryTabPage.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.seedNE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.jobsNE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlantBox)).EndInit();
            this.PlantsTabPage.ResumeLayout(false);
            this.PlantsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CapabilityBox)).EndInit();
            this.ResourcesGroupBox.ResumeLayout(false);
            this.ResourcesGroupBox.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MachineBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DepartmentBox)).EndInit();
            this.JobsTabPage.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxCycleTimeNE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minCycleTimeNE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxCapabilityIdNE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxOpsNE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxRequiredQtyNE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxMOsNE)).EndInit();
            this.MaterialsTabPage.ResumeLayout(false);
            this.MaterialsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PurchaseToStockCountEditor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WarehouseCountEditor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemCountEditor)).EndInit();
            this.OtherTabPage.ResumeLayout(false);
            this.OtherTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UsersBox)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion
		#endregion

		#region Form Events
		private void generateButton_Click(object sender, System.EventArgs e)
		{
			if(this.DatasetComboBox.SelectedIndex==0)
				this.GenerateRandomData();
			else if(this.DatasetComboBox.SelectedIndex==1)
				this.GenerateFurnitureFactoryData();
		}

		private void DepartmentBox_Click(object sender, System.EventArgs e)
		{
		
		}

		private void DepartmentLabel_Click(object sender, System.EventArgs e)
		{
		
		}

		private void CapabilityBox_Click(object sender, System.EventArgs e)
		{
		
		}

		private void CapabilityLabel_Click(object sender, System.EventArgs e)
		{
		
		}

		private void OnTop_CheckedChanged(object sender, System.EventArgs e)
		{
			this.TopMost=this.OnTop.Checked;
			//			if(this.TopMost)
			//				this.Opacity=.8;
			//			else
			//				this.Opacity=1;
		}

		#endregion Form Events

		#region Properties
		#region Users
		internal int Users
		{
			get{return (int)this.UsersBox.Value;}
			set{this.UsersBox.Value=value;}
		}
		#endregion
		#region Resources
		internal int Plants
		{
			get{return (int)this.PlantBox.Value;}
			set{this.PlantBox.Value=value;}
		}
		internal int Departments
		{
			get{return (int)this.DepartmentBox.Value;}
			set{this.DepartmentBox.Value=value;}
		}
		internal int Machines
		{
			get{return (int)this.MachineBox.Value;}
			set{this.MachineBox.Value=value;}
		}
		internal int Capabilities
		{
			get{return (int)this.CapabilityBox.Value;}
			set{this.CapabilityBox.Value=value;}
		}
		internal int Items
		{
			get{return (int)this.ItemCountEditor.Value;}
		}
		internal int Warehouses
		{
			get{return (int)this.WarehouseCountEditor.Value;}
		}
		internal int PurchasesToStock
		{
			get{return (int)this.PurchaseToStockCountEditor.Value;}
		}
		#endregion

		#region jobs

		int Jobs
		{
			get{return (int)jobsNE.Value;}
			set{jobsNE.Value=value;}
		}

		DateTime MaxDueDate
		{
			get{return (DateTime)maxDueDateDTP.Value;}
			set{maxDueDateDTP.Value=value;}
		}

		int MaxMOs
		{
			get{return (int)maxMOsNE.Value;}
			set{maxMOsNE.Value=value;}
		}

		int MaxOps
		{
			get{return (int)maxOpsNE.Value;}
			set{maxOpsNE.Value=value;}
		}

		int MaxMaterials
		{
			get{return 3;} //need to hookup to UI but right now I can't change the Tabs!!
		}

		int MaxRequiredQty
		{
			get{return (int)maxRequiredQtyNE.Value;}
			set{maxRequiredQtyNE.Value=value;}
		}

		int MaxCapabilityId
		{
			get{return (int)maxCapabilityIdNE.Value;}
			set{maxCapabilityIdNE.Value=value;}
		}

		int MinCycleTime
		{
			get{return (int)minCycleTimeNE.Value;}
			set{minCycleTimeNE.Value=value;}
		}

		int MaxCycleTimeNE
		{
			get{return (int)maxCycleTimeNE.Value;}
			set{maxCycleTimeNE.Value=value;}
		}

		int Seed
		{
			get
			{
				return (int)seedNE.Value;
			}

			set
			{
				seedNE.Value=value;
			}
		}

		bool UseItemsInMaterialRequirements
		{
			get{return this.IncludeMaterialRequirementsCB.Checked;}
		}
		bool IncludeProducts
		{
			get{return this.IncludeProductsCB.Checked;}
		}
		

		#endregion
		#endregion

		#region Random Data Generation
		void GenerateRandomData()
		{
			Random randomizer=new Random(Seed);

			CreateBroadcasterConnection();

			Cursor.Current=Cursors.WaitCursor;

			//Capabilities - same number of each type 
			PT.ERPTransmissions.CapabilityT machineCapabilityT=new CapabilityT();
			for(int i=1;i<=Capabilities;i++)
			{
				string externalId=String.Format("Capability {0}",i.ToString());
				CapabilityT.Capability machineCapability=new CapabilityT.Capability(externalId,externalId);
				machineCapabilityT.Add(machineCapability);
			}
			machineCapabilityT.AutoDeleteMode=true;

			//Items
			if(this.Items>0)
			{
				PT.ERPTransmissions.ItemT itemT=new ItemT();
				itemT.AutoDeleteMode=true;
				for(int i=1;i<=this.Items;i++)
				{
					string externalId=String.Format("Item {0}",i);
					ItemT.Item item=new ItemT.Item(externalId,externalId);
					itemT.Add(item);
				}
				byte[] b=Transmission.Compress(itemT);
				ptBroadcaster.Broadcast(b, connectionNbr);
			}



			if(Capabilities>0)
			{
				byte[] b=Transmission.Compress(machineCapabilityT);
				ptBroadcaster.Broadcast(b, connectionNbr);
			}

			#region Plant Related Transmissions
			//Plants
			string plantExternalId;
			string departmentExternalId;
			PT.ERPTransmissions.PlantT plantT=new PlantT();
			for(int p=1;p<=Plants;p++)
			{
				plantExternalId=String.Format("Plant {0}",p.ToString());
				PlantT.Plant plant=new PlantT.Plant(plantExternalId,plantExternalId);
				plantT.Add(plant);
			}
			plantT.AutoDeleteMode=true;


			//Departments
			PT.ERPTransmissions.DepartmentT wct=new DepartmentT();
			for(int p=1;p<=Plants;p++)
			{
				plantExternalId=String.Format("Plant {0}",p.ToString());
				for(int w=1;w<=Departments;w++)
				{
					departmentExternalId=String.Format("Department {0} ",w.ToString()) + plantExternalId;
					DepartmentT.Department dept=new DepartmentT.Department(departmentExternalId, departmentExternalId, plantExternalId);
					dept.Notes="Notes Test";
					wct.Add(dept);	
				}
			}
			wct.AutoDeleteMode=true;



			//Resources
			PT.ERPTransmissions.ResourceT mt=new ResourceT();
			//Add a "plant shutdown" CapacityInterval
			PT.ERPTransmissions.CapacityIntervalT cinT=new CapacityIntervalT();
			string capacityIntervalName="Maintenance Shutdown";
			PT.ERPTransmissions.IdList plantIdList=new IdList();
			PT.ERPTransmissions.IdList deptIdList=new IdList();
			PT.ERPTransmissions.IdList resIdList=new IdList();
			PT.Transmissions.CapacityInterval ci=new PT.Transmissions.CapacityInterval(capacityIntervalName,capacityIntervalName);
			ci.IntervalType=CapacityIntervalDefs.capacityIntervalTypes.Offline;
			PT.ERPTransmissions.CapacityIntervalT.CapacityIntervalDef capIntervalDef=new PT.ERPTransmissions.CapacityIntervalT.CapacityIntervalDef(capacityIntervalName,capacityIntervalName,ci,plantIdList,deptIdList,resIdList);
			cinT.Add(capIntervalDef);
			for(int p=1;p<=Plants;p++)
			{
				plantExternalId=String.Format("Plant {0}",p.ToString());
				for(int w=1;w<=Departments;w++)
				{
					departmentExternalId=String.Format("Department {0} ",w.ToString()) + plantExternalId;
					//Machines
					for(int m=1;m<=Machines;m++)
					{
						string resourceExternalId=String.Format("Resource {0} ",m.ToString()) + departmentExternalId;
						ResourceT.Resource mc=new ResourceT.Resource(resourceExternalId,resourceExternalId,plantExternalId,departmentExternalId);
						mc.CapacityType=PT.SchedulerDefinitions.InternalResourceDefs.capacityTypes.SingleTasking;

						mc.Notes="Notes Test";
						//Add all Capabilties to each Resource
						for(int i=0;i<machineCapabilityT.Count;i++) 
							mc.Capabilities.Add(((CapabilityT.Capability)machineCapabilityT[i]).ExternalId);
						mt.Add(mc);	


						if(this.CapacityIntervalsCB.Checked)
						{
							capIntervalDef.plantExternalIds.Add(plantExternalId);
							capIntervalDef.departmentExternalIds.Add(departmentExternalId);
							capIntervalDef.resourceExternalIds.Add(resourceExternalId);
						}
					}					
				}
			}
			mt.AutoDeleteMode=true;

			//Send all Plant related transmissions
			if(Plants>0)
			{
				ptBroadcaster.Broadcast(Transmission.Compress(plantT), connectionNbr);
				if(Departments>0)
				{
					ptBroadcaster.Broadcast(Transmission.Compress(wct), connectionNbr);
					if(Machines>0)	
					{
						ptBroadcaster.Broadcast(Transmission.Compress(mt), connectionNbr);

					}
				}

				//Warehouses
				if(this.Warehouses>0)
				{
					PT.ERPTransmissions.WarehouseT warehouseT=new WarehouseT();
					warehouseT.AutoDeleteMode=true;
					for(int w=1;w<=this.Warehouses;w++)
					{
						string externalId=String.Format("Warehouse {0}",w);
						WarehouseT.Warehouse warehouse=new PT.ERPTransmissions.WarehouseT.Warehouse(externalId, externalId);
						//Add every item to every warehouse
						for(int i=1;i<=this.Items;i++)
						{
							string itemExternalId=String.Format("Item {0}",i);
							WarehouseT.Inventory inventory=new PT.ERPTransmissions.WarehouseT.Inventory(itemExternalId);
							inventory.OnHandQty=randomizer.Next(10000);
							inventory.LeadTime=TimeSpan.FromDays(randomizer.Next(90));
							warehouse.Add(inventory);
						}

						//Add every Plant to every warehouse
						for(int p=1;p<=this.Plants;p++)
						{
							string pExtId=String.Format("Plant {0}",p);
							warehouse.AddSuppliedPlant(pExtId);
						}
						warehouseT.Add(warehouse);
					}
					byte[] wT=Transmission.Compress(warehouseT);
					ptBroadcaster.Broadcast(wT, connectionNbr);				
				}

				//Purhchases to stock
				if(this.PurchasesToStock>0)
				{
					PurchaseToStockT purchaseToStockT=new PurchaseToStockT();
					purchaseToStockT.AutoDeleteMode=true;
					for(int purchaseI=1;purchaseI<=this.PurchasesToStock;purchaseI++)
					{
						string externalId=String.Format("Purchase {0}",purchaseI);
						double qtyOrdered=randomizer.Next(1,5000);
						DateTime receiptDate=DateTime.Now.AddDays(randomizer.Next(90));
						string itemExternalId=String.Format("Item {0}",randomizer.Next(1,this.Items));
						string warehouseExternalId=String.Format("Warehouse {0}",randomizer.Next(1,this.Warehouses));
					
						PurchaseToStockT.PurchaseToStock purchase=new PurchaseToStockT.PurchaseToStock(externalId,
							qtyOrdered,receiptDate,itemExternalId,warehouseExternalId,true,false,"","");
						purchaseToStockT.Add(purchase);
					}
					byte[] pT=Transmission.Compress(purchaseToStockT);
					ptBroadcaster.Broadcast(pT, connectionNbr);
				}
			}
			
			if(this.CapacityIntervalsCB.Checked && Plants>=1 && Departments>=1 && Machines>=1)
			{
				cinT.AutoDeleteMode=true;
				ptBroadcaster.Broadcast(Transmission.Compress(cinT), connectionNbr);
			}

			#endregion Plant related transmissions

			//Users
			for(int i=1;i<=Users;i++)
			{
			}

			// Insert Jobs.
			if(Jobs>0)
			{
				InsertJobs();
			}

			Cursor.Current=Cursors.Default;
			ptBroadcaster.LogOff(connectionNbr, creationTicks);
		}



		void InsertJobs()
		{
			JobT jobT=new JobT();
			jobT.AutoDeleteMode=true;

			Random randomizer=new Random(Seed);

			// Create the jobs.
			for(int jobNbr=1; jobNbr<=Jobs; ++jobNbr)
			{
				JobT.Job job=new JobT.Job(jobNbr.ToString(), string.Format("Job {0}", jobNbr));
				job.CustomerExternalId="WalMart";
				TimeSpan ts=MaxDueDate-simulationStartTime;
				int maxValue= ((int)ts.TotalMinutes)+1;
				int minutes=0;
				if(maxValue>0)
					minutes=randomizer.Next(0,maxValue);
				///**/			minutes=1;
				DateTime needDate=simulationStartTime;
				needDate=needDate.AddMinutes(minutes);
				///**/            needDate=new DateTime(2004, 5, 13, 12, 0, 0); // Weird scheduling result. The scheduled start time is during a period when there are no on/offline intervals and the amount of space the block takes up is huge. This release date is after all the on/offline intervals created by this sample data generateor.
				///**/            needDate=new DateTime(2004, 5, 12, 12, 0, 0); // Weird scheduling result. The block doesn't schedule enough time. The block is scheduled to start right before there are no more on/offline intervals.
				job.NeedDateTime=needDate;
				job.Revenue=(decimal)randomizer.Next(1000,50000);
				// Create the Manufacturing orders.
				int nbrOfMos;

				if(cbJobMax.Checked)
				{
					nbrOfMos=randomizer.Next(1, MaxMOs+1);
				}
				else
				{
					nbrOfMos=MaxMOs;
				}

				for(int moNbr=1; moNbr<=nbrOfMos; moNbr++)
				{
					double requiredQty=randomizer.Next(1, MaxRequiredQty+1);


					JobT.ManufacturingOrder mo=new PT.ERPTransmissions.JobT.ManufacturingOrder(moNbr.ToString(), string.Format("Manufacturing Order {0}", moNbr), requiredQty);
					TimeSpan diffBetweenSundayAndJobDueDate=job.NeedDateTime-sundayOfWeek;
					//					int releaseDateMinutesToSubtract;
					//					releaseDateMinutesToSubtract=randomizer.Next(0, (int)diffBetweenSundayAndJobDueDate.TotalMinutes);
					mo.ReleaseDateTime=job.NeedDateTime.Subtract(TimeSpan.FromDays(7));

					if(fixedReleaseDate)
					{
						mo.ReleaseDateTime=fixedReleaseDateTime;
					}
					mo.IsReleased=mo.ReleaseDateTime<DateTime.Now.AddDays(14);
					int a=randomizer.Next(0,255);
					int r=randomizer.Next(0,255);
					int g=randomizer.Next(0,255);
					int blue=randomizer.Next(0,255);
					mo.ProductColor=Color.FromArgb(a,r,g,blue);
					///**/				mo.ReleaseDateTime=needDate;
					
					// Create a linear routing.
					JobT.AlternatePath path=new PT.ERPTransmissions.JobT.AlternatePath("1", "Alternate Path 1", 0);

					int nbrOfOps;
					if(cbJobMax.Checked)
					{
						nbrOfOps=randomizer.Next(1, MaxOps+1);
					}
					else
					{
						nbrOfOps=MaxOps;
					}

					for(int opNbr=1; opNbr<=nbrOfOps; opNbr++)
					{
						// Create the operation's cycletime.
						int opNbrX10=opNbr*10;
						int cycleTimeInMinutes=randomizer.Next(MinCycleTime, MaxCycleTimeNE+1);
						cycleTimeInMinutes=1;
						TimeSpan cycleTime=new TimeSpan(0, 0, cycleTimeInMinutes, 0, 0);
						
						JobT.ResourceOperation operation=new PT.ERPTransmissions.JobT.ResourceOperation(opNbrX10.ToString(), string.Format("Operation {0}", opNbrX10), requiredQty, cycleTime);

						if(jobNbr==1 && opNbr<nbrOfOps)
						{
							operation.SuccessorProcessing=PT.SchedulerDefinitions.OperationDefs.successorProcessingEnumeration.KeepSuccessor;
							operation.KeepSuccessorsTimeLimit=new TimeSpan(48, 0, 0);
						}

						// Create an activity for the operation.
						JobT.InternalActivity activity=new PT.ERPTransmissions.JobT.InternalActivity("1");

						
						operation.SetupSpan=new TimeSpan(1,0,0);
						operation.SetupNumber=Math.Round(randomizer.NextDouble()*100,1);
						operation.PostProcessingSpan=new TimeSpan(0,0,0);
						operation.RequiredFinishQty=requiredQty;

						if(opNbr==1)
							activity.ProductionStatus=PT.SchedulerDefinitions.InternalActivityDefs.productionStatuses.Ready;
						else
							activity.ProductionStatus=PT.SchedulerDefinitions.InternalActivityDefs.productionStatuses.Waiting;

						//						if(jobNbr==1 && moNbr==1 && opNbr==1)
						//						{
						//							if(testCB.Checked)
						//							{
						//								activity.ProductionStatus=PT.SchedulerDefinitions.InternalActivityDefs.productionStatuses.Finished;
						//							}
						////							operation.Omitted=PT.SchedulerDefinitions.BaseOperationDefs.omitStatuses.OmittedAutomatically;
						//						}

					
						//Setup info
						int setupType=randomizer.Next(1,6);
						if(setupType==1)
							operation.SetupColor=Color.Orange;
						else if(setupType==2)
							operation.SetupColor=Color.Yellow;
						else if(setupType==3)
							operation.SetupColor=Color.SeaGreen;
						else if(setupType==4)
							operation.SetupColor=Color.SteelBlue;
						else 
							operation.SetupColor=Color.Silver;

						operation.SetupCode=operation.SetupColor.Name;
						operation.SetupNumber=setupType;



						activity.RequiredFinishQty=requiredQty;
						bool DoSplitting=false; //toggle to enable
						if(DoSplitting && opNbr==2)
						{
							double splitQty=activity.RequiredFinishQty/2;
							activity.RequiredFinishQty=activity.RequiredFinishQty-splitQty;
							operation.AddInternalActivity(activity); 
							JobT.InternalActivity activityCopy=new PT.ERPTransmissions.JobT.InternalActivity("2");
							activityCopy.ProductionStatus=activity.ProductionStatus; 
							activityCopy.RequiredFinishQty=activity.RequiredFinishQty;
							operation.AddInternalActivity(activityCopy);
						}
						else
							operation.AddInternalActivity(activity);

						// Create the required capability.
						int capabilityNbr=randomizer.Next(1, MaxCapabilityId+1);
						string capabilityExternalId=string.Format("Capability {0}", capabilityNbr.ToString());
						//Create one ResourceRequirement with one Capability
						JobT.InternalOperation.ResourceRequirement requirement=new JobT.InternalOperation.ResourceRequirement("RR1","ResReqt 1",PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,capabilityExternalId);
						operation.AddResourceRequirement(requirement);

						//Add Materials
						if(opNbr==1)
						{
							for(int materialI=0;materialI<this.MaxOps;materialI++)
							{
								string materialId=String.Format("Material{0}",materialI);
								DateTime availDate=DateTime.Today.AddDays(materialI+1);
								TimeSpan stdLeadTime=new TimeSpan(materialI,0,0,0);
								double matlRequiredQty=randomizer.Next(1,5000);
								JobT.MaterialRequirement material=new JobT.MaterialRequirement(materialId,materialId,availDate,stdLeadTime,matlRequiredQty);
								if(materialI==2)
									material.ConstraintType=PT.SchedulerDefinitions.MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate;
								else if(materialI==3)
									material.ConstraintType=PT.SchedulerDefinitions.MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate;
							
								if(this.UseItemsInMaterialRequirements && this.Items>0)
								{
								
									int itemNbr=randomizer.Next(1,this.Items);
									material.ItemExternalId=String.Format("Item {0}",itemNbr);
									material.RequirementType=MaterialRequirementDefs.requirementTypes.FromStock;

									int warehouseNbr=randomizer.Next(1,this.Warehouses);
									string warehouseExternalId=String.Format("Warehouse {0}",warehouseNbr);
									material.WarehouseExternalId=warehouseExternalId;
								}
							
								material.ConstraintType=PT.SchedulerDefinitions.MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate;
								operation.Add(material);
							}
						}

						//Products
						if(this.IncludeProducts && this.Items>0 && this.Warehouses>0
							&&jobNbr==2)
						{
							//Add one random Product to each op
							int itemNbr=randomizer.Next(1,this.Items);
							string itemExternalId=String.Format("Item {0}",itemNbr);

							int warehouseNbr=randomizer.Next(1,this.Warehouses);
							string warehouseExternalId=String.Format("Warehouse {0}",warehouseNbr);

							double outputQty=4000;

							PT.ERPTransmissions.JobT.Product product=new PT.ERPTransmissions.JobT.Product("Product 1",itemExternalId, outputQty, warehouseExternalId);
							operation.Add(product);
						}


						// Create the predecessor operation attributes.
						if(opNbr>1)
						{
							int predOpNbrX10=(opNbr-1)*10;
                            JobT.PredecessorOperationAttributes predAtts = new PT.ERPTransmissions.JobT.PredecessorOperationAttributes(opNbrX10.ToString());
                            //operation.Add(predAtts);

							// Add this operation as a successor of the predecessor operation.
							JobT.AlternateNode sucessorPathNode=new PT.ERPTransmissions.JobT.AlternateNode(predOpNbrX10.ToString());
                            sucessorPathNode.AddSuccessor(predAtts);
							path.Add(sucessorPathNode);
						}

						if(nbrOfOps==1)
						{
							// Add this operation as the only operation in the path.
							JobT.AlternateNode node=new PT.ERPTransmissions.JobT.AlternateNode(opNbrX10.ToString());
							path.Add(node);
						}

						mo.AddOperation(operation);
					}
                    
					mo.AddPath(path);
					job.Add(mo);
				}

				jobT.Add(job);
			}

			jobT.Validate();

			ptBroadcaster.Broadcast(Transmission.Compress(jobT), connectionNbr);
		}


		#endregion Random Data Generation

		#region Furniture Factory Data Generation
		string[] machines=new string[] {
																																		"Table Saw LG",			//	 0
																																		"Table Saw SM",			//	 1
																																		"Planer LG",						//	 2
																																		"Planer SM",						//	 3
																																		"Joiner",									//	 4
																																		"Router",									//	 5
																																		"Sander",									//	 6
																																		"Coater",									//	 7
																																		"Finisher (Sub)",	//	 8
																																		"Assembly1",						//	 9
																																		"Assembly2",						//	10
																																		"Inspection",					//	11
																																		"Shipping",							//	12
																																		"Band Saw",							//	13
																																		"Richard",								//	14
																																		"Sam",												//	15
																																		"Lathe1",									// 16
																																		"Lathe2", 								// 17
																																		"OrbitalBuffer1",      // 18
																																		"OrbitalBuffer2"};     // 19

		/// <summary>
		/// You need to create a corresponding entry for every machine.
		/// </summary>
		string[] workcenters=new string[] {
																																					"Cutting",								//	 0						
																																					"Cutting",								//	 1
																																					"Planing",								//	 2
																																					"Planing",								//	 3
																																					"Joiner",									//	 4
																																					"Router",									//	 5
																																					"Sander",									//	 6
																																					"Coater",									//	 7
																																					"Finisher (Sub)",	//	 8
																																					"Assembly",							//	 9
																																					"Assembly",							//	10
																																					"Inspection",					//	11
																																					"Shipping",							//	12
																																					"Cutting",								//	13
																																					"Operators",						//	14
																																					"Operators",						//	15
																																					"Turning",								//	16
																																					"Turning", 							//	17
																																					"Buffing",        // 18
																																					"Buffing"};       // 19

		void GenerateFurnitureFactoryData()
		{
			Cursor.Current=Cursors.WaitCursor;

			//Create Connection
			CreateBroadcasterConnection();

			//Capabilities
			ArrayList capabilityDefs=new ArrayList();
			string[] capabilityNames=new string[]
			{
				"48” Cross cut",			//	0
				"24” Cross cut",			//	1
				"48” Planing",					//	2
				"36” Planing",					//	3
				"Joining",									//	4
				"Routing",									//	5
				"Sanding",									//	6
				"Lacquer coating",	//	7
				"Polishing",							//	8
				"Assembly",								//	9
				"Inspecting",						//	10
				"Shipping",								//	11
				"Curved cutting",		//	12
				"Painting",								//	13
				"Lathing"										// 14
			};

			for(int i=0;i<capabilityNames.Length;i++)
				capabilityDefs.Add(new CapabilityDef(capabilityNames[i],capabilityNames[i]));

			CreateCapabilities(capabilityDefs);

			//Plants
			ArrayList plantDefs=new ArrayList();
			plantDefs.Add(new PlantDef("San Diego Plant","San Diego Plant"));
			plantDefs.Add(new PlantDef("L.A. Plant","L.A. Plant"));
			CreatePlants(plantDefs);

			//Departments
			ArrayList departmentDefs=new ArrayList();
			string[] sdDepartments=new string[] {"Machining","Finishing"};
			string[] laDepartments=new string[] {"Final Assembly","Shipping"};
			for(int i=0;i<sdDepartments.Length;i++)
				departmentDefs.Add(new DepartmentDef(((PlantDef)plantDefs[0]).externalId,sdDepartments[i],sdDepartments[i]));
			for(int i=0;i<laDepartments.Length;i++)
				departmentDefs.Add(new DepartmentDef(((PlantDef)plantDefs[1]).externalId,laDepartments[i],laDepartments[i]));

			CreateDepartments(departmentDefs);

			//Machines
			ArrayList machineDefs=new ArrayList();
			//Note, can't have machines with the same name since it's used for external id too.

			IdList capabilities;

			//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			//Machining Department (San Diego Plant)
			//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			capabilities=new IdList(new string[] {capabilityNames[0],capabilityNames[1]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[0],machines[0],capabilities,workcenters[0]));

			capabilities=new IdList(new string[] {capabilityNames[1]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[1],machines[1],capabilities,workcenters[1]));

			capabilities=new IdList(new string[] {capabilityNames[2]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[2],machines[2],capabilities,workcenters[2]));

			capabilities=new IdList(new string[] {capabilityNames[3]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[3],machines[3],capabilities,workcenters[3]));

			capabilities=new IdList(new string[] {capabilityNames[4]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[4],machines[4],capabilities,workcenters[4]));

			capabilities=new IdList(new string[] {capabilityNames[5]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[5],machines[5],capabilities,workcenters[5]));

			capabilities=new IdList(new string[] {capabilityNames[12]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[13],machines[13],capabilities,workcenters[13]));

			capabilities=new IdList(new string[] {capabilityNames[14]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[16],machines[16],capabilities,workcenters[16]));

			capabilities=new IdList(new string[] {capabilityNames[14]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[17],machines[17],capabilities,workcenters[17]));

			capabilities=new IdList(new string[] {capabilityNames[8]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[0]).externalId,machines[18],machines[18],capabilities,workcenters[18]));

			//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			//Finishing Department (San Diego Plant)
			//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			capabilities=new IdList(new string[] {capabilityNames[6]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[1]).externalId,machines[6],machines[6],capabilities,workcenters[6]));

			capabilities=new IdList(new string[] {capabilityNames[7]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[1]).externalId,machines[7],machines[7],capabilities,workcenters[7]));

			capabilities=new IdList(new string[] {capabilityNames[6],capabilityNames[8]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[1]).externalId,machines[8],machines[8],capabilities,workcenters[8]));

			capabilities=new IdList(new string[] {capabilityNames[13]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[1]).externalId,machines[14],machines[14],capabilities,workcenters[14]));

			capabilities=new IdList(new string[] {capabilityNames[13]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[0]).externalId,((DepartmentDef)departmentDefs[1]).externalId,machines[15],machines[15],capabilities,workcenters[15]));

			//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			//Final Assembly Department (L.A. Plant)
			//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			capabilities=new IdList(new string[] {capabilityNames[9]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[1]).externalId,((DepartmentDef)departmentDefs[2]).externalId,machines[9],machines[9],capabilities,workcenters[9]));

			capabilities=new IdList(new string[] {capabilityNames[9]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[1]).externalId,((DepartmentDef)departmentDefs[2]).externalId,machines[10],machines[10],capabilities,workcenters[10]));

			capabilities=new IdList(new string[] {capabilityNames[10]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[1]).externalId,((DepartmentDef)departmentDefs[3]).externalId,machines[11],machines[11],capabilities,workcenters[11]));

			capabilities=new IdList(new string[] {capabilityNames[11]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[1]).externalId,((DepartmentDef)departmentDefs[3]).externalId,machines[12],machines[12],capabilities,workcenters[12]));

			capabilities=new IdList(new string[] {capabilityNames[8]});
			machineDefs.Add(new MachineDef(((PlantDef)plantDefs[1]).externalId,((DepartmentDef)departmentDefs[3]).externalId,machines[19],machines[19],capabilities,workcenters[19]));

			CreateMachines(machineDefs);

			if(this.CapacityIntervalsCB.Checked)
				CreateCapacityIntervals(machineDefs);
			if(this.RecurringCapacityIntervalsCB.Checked)
				CreateRecurringCapacityIntervals(machineDefs);

			CreateJobs();	
		
			Cursor.Current=Cursors.Default;
		}
		void CreateCapabilities(ArrayList capabilityDefs)
		{
			PT.ERPTransmissions.CapabilityT machineCapabilityT=new CapabilityT();
			for(int i=0;i<capabilityDefs.Count;i++)
			{
				CapabilityDef capabilityDef=(CapabilityDef)capabilityDefs[i];
				CapabilityT.Capability machineCapability=new CapabilityT.Capability(capabilityDef.externalId,capabilityDef.name);
				machineCapabilityT.Add(machineCapability);
			}
			machineCapabilityT.AutoDeleteMode=true;

			if(machineCapabilityT.Count>0)
			{
				ptBroadcaster.Broadcast(Transmission.Compress(machineCapabilityT), connectionNbr);
			}
		}
		void CreatePlants(ArrayList plantDefs)
		{
			PT.ERPTransmissions.PlantT plantT=new PlantT();
			for(int i=0;i<plantDefs.Count;i++)
			{
				PlantDef plantDef=(PlantDef)plantDefs[i];
				PlantT.Plant plant=new PlantT.Plant(plantDef.externalId,plantDef.name);
				plantT.Add(plant);
			}
			plantT.AutoDeleteMode=true;

			if(plantT.Count>0)
				ptBroadcaster.Broadcast(Transmission.Compress(plantT), connectionNbr);
		}


		void CreateDepartments(ArrayList departmentDefs)
		{
			PT.ERPTransmissions.DepartmentT wct=new DepartmentT();
			for(int i=0;i<departmentDefs.Count;i++)
			{
				DepartmentDef departmentDef=(DepartmentDef)departmentDefs[i];
				DepartmentT.Department dept=new DepartmentT.Department(departmentDef.externalId,departmentDef.name,departmentDef.plantExternalId);
				wct.Add(dept);	
			}
			wct.AutoDeleteMode=true;

			if(wct.Count>0)
				ptBroadcaster.Broadcast(Transmission.Compress(wct), connectionNbr);
		}

		void CreateMachines(ArrayList machineDefs)
		{
			PT.ERPTransmissions.ResourceT machineT=new ResourceT();

			for(int i=0;i<machineDefs.Count;i++)
			{
				MachineDef machineDef=(MachineDef)machineDefs[i];
				ResourceT.Resource machine=new ResourceT.Resource(machineDef.externalId,machineDef.name,machineDef.plantExternalId,machineDef.departmentExternalId);
				machine.Workcenter=machineDef.workcenter;
				//Add Capabilities
				machine.Capabilities=machineDef.capabilities;
				machine.CapacityType=PT.SchedulerDefinitions.InternalResourceDefs.capacityTypes.SingleTasking;

				if(machine.ExternalId==this.machines[8])
				{
					machine.CapacityType=PT.SchedulerDefinitions.InternalResourceDefs.capacityTypes.Infinite;
				}

				machineT.Add(machine);
			}
			machineT.AutoDeleteMode=true;

			if(machineT.Count>0)
				ptBroadcaster.Broadcast(Transmission.Compress(machineT), connectionNbr);
		}

		void CreateCapacityIntervals(ArrayList machineDefs)
		{
			CapacityIntervalT capacityIntervalT=new CapacityIntervalT();

			//Add a plant shutdown
			string name="Plant Shutdown";
			PT.Transmissions.CapacityInterval ci=new PT.Transmissions.CapacityInterval(name,name);
			ci.IntervalType=CapacityIntervalDefs.capacityIntervalTypes.Offline;
			ci.StartDateTime=DateTime.Now.AddDays(3);
			ci.EndDateTime=DateTime.Now.AddDays(10);
			PT.ERPTransmissions.IdList plantList=new IdList();
			PT.ERPTransmissions.IdList deptList=new IdList();
			PT.ERPTransmissions.IdList resList=new IdList();
			PT.ERPTransmissions.CapacityIntervalT.CapacityIntervalDef def=new PT.ERPTransmissions.CapacityIntervalT.CapacityIntervalDef(name,name,ci,plantList,deptList,resList);
			//now add all reasources
			for(int i=0;i<machineDefs.Count;i++)
			{
				MachineDef m=(MachineDef)machineDefs[i];
				def.plantExternalIds.Add(m.plantExternalId);
				def.departmentExternalIds.Add(m.departmentExternalId);
				def.resourceExternalIds.Add(m.externalId);
			}
			capacityIntervalT.Add(def);

			capacityIntervalT.AutoDeleteMode=true;
			ptBroadcaster.Broadcast(Transmission.Compress(capacityIntervalT), connectionNbr);
		}

		void CreateRecurringCapacityIntervals(ArrayList machineDefs)
		{
			RecurringCapacityIntervalT rciT=new RecurringCapacityIntervalT();

			//Setup the RCI definition
			PT.Transmissions.RecurringCapacityInterval rci=new PT.Transmissions.RecurringCapacityInterval("Standard","Standard");
			rci.Recurrence=PT.SchedulerDefinitions.CapacityIntervalDefs.recurrences.Weekly;
			rci.SkipFrequency=0;
			rci.Monday=true;
			rci.Tuesday=true;
			rci.Wednesday=true;
			rci.Thursday=true;
			rci.Friday=true;
			rci.Saturday=false;
			rci.Sunday=false;

			DateTime date=DateTime.Now;
			DateTime startDate=new DateTime(date.Year, date.Month, date.Day);
			rci.StartDateTime=PT.Common.TimeZoneAdjuster.ConvertToServerTime(DateTime.Today.AddHours(8));
			rci.EndDateTime=PT.Common.TimeZoneAdjuster.ConvertToServerTime(DateTime.Today.AddHours(16).ToUniversalTime());

			//Assign the RCI to all of the resources
			PT.ERPTransmissions.IdList plantExternalIds=new IdList();
			PT.ERPTransmissions.IdList departmentExternalIds=new IdList();
			PT.ERPTransmissions.IdList resourceExternalIds=new IdList();

			for(int i=0; i<machineDefs.Count; ++i)
			{
				MachineDef machineDef=(MachineDef)machineDefs[i];
				plantExternalIds.Add(machineDef.plantExternalId);
				departmentExternalIds.Add(machineDef.departmentExternalId);
				resourceExternalIds.Add(machineDef.externalId);
			}


			RecurringCapacityIntervalT.RecurringCapacityIntervalDef rciDef=new PT.ERPTransmissions.RecurringCapacityIntervalT.RecurringCapacityIntervalDef("Standard","Standard",rci,plantExternalIds,departmentExternalIds,resourceExternalIds);
			rciT.Add(rciDef);
			rciT.AutoDeleteMode=true;
			ptBroadcaster.Broadcast(Transmission.Compress(rciT), connectionNbr);
		}

		Random random;
		void CreateJobs()
		{
			random=new Random(Seed);


			int needDateMin=14;
			int needDateMax=(int)(maxDueDateDTP.Value.Date.Subtract(DateTime.Today)).TotalDays;
			needDateMax=Math.Max(needDateMin+1,needDateMax);
			
			string[] customers=new string[]{"Sterling Furniture Inc.","WalMart","Bassett Furniture", "Broyhill", "JC Penney"};


			JobT jobT=new JobT();
			jobT.AutoDeleteMode=true;
			for(int i=100;i<100+(int)this.jobsNE.Value;i++)
			{
				int needDateOffset=random.Next(needDateMin,needDateMax);
				int requiredQuantity=random.Next(1,5);
				string customerExternalId=customers[random.Next(3)];
				jobT.Add(GeneratePortofinoDiningRoomJob(String.Format("Job-{0}",i.ToString()),new TimeSpan(needDateOffset,0,0,0),1000,requiredQuantity,customerExternalId));
			}

			if(jobT.Count>0)
				ptBroadcaster.Broadcast(Transmission.Compress(jobT), connectionNbr);

		}


		int finishMoUpTo=0;
		int addDays2;

		JobT.Job GeneratePortofinoDiningRoomJob(string jobName, TimeSpan needDateOffset, decimal revenue, double requiredQty, string customerExternalId)
		{

			JobT.Job job=new JobT.Job(jobName, jobName);
			job.Description="Portofino Dining Room Job";
			job.CustomerExternalId=customerExternalId;
			job.NeedDateTime=DateTime.Today.Add(needDateOffset);
			job.Revenue=revenue;
			job.MaxEarlyDeliverySpan=new TimeSpan(14,0,0,0,0);

			//Job Color
			int r=random.Next(0,255);
			int g=random.Next(0,255);
			int b=random.Next(0,255);
			job.ColorCode=Color.FromArgb(r,g,b);

			if(job.NeedDateTime<DateTime.Now.AddDays(30))
				job.Commitment=JobDefs.commitmentTypes.Firm;
			else
				job.Commitment=JobDefs.commitmentTypes.Planned;

			// Create the Manufacturing orders.
			string materialId;
			string materialName;
			DateTime availDate;
			TimeSpan stdLeadTime;
			JobT.MaterialRequirement material;
				
			const string SetupCodeWide="Wide";
			const double SetupNbrWide=10;
			const string SetupCodeStandard="Standard";
			const double SetupNbrStandard=5;
			const string SetupCodeNarrow="Narrow";
			const double SetupNbrNarrow=5;
			const string SetupCodeHeavy="Heavy";
			const double SetupNbrHeavy=100;
			const string SetupCodeLight="Light";
			const double SetupNbrLight=20;
			const string SetupCodeClear="Clear Coat";
			const double SetupNbrClear=1;

			finishMoUpTo=random.Next(100); //Operations with opn number less than or equal to this are marked as finished
			double deviation = random.NextDouble();//for activity finishes

			JobT.ManufacturingOrder mo;
			JobT.AlternatePath path;
			TimeSpan setupSpan;
			JobT.ResourceOperation operation;

			if(!this.testCB.Checked)
			{
				#region Table MO
				mo=new PT.ERPTransmissions.JobT.ManufacturingOrder("MO1", string.Format("M{0}", 1), requiredQty);
				mo.ProductName="5866-93 Glass Top Table";
				mo.ProductColor=Color.BurlyWood;
				mo.ReleaseDateTime=job.NeedDateTime.Subtract(new TimeSpan(21,0,0,0));
				mo.IsReleased=mo.ReleaseDateTime<DateTime.Now.AddDays(14);

				// Create a linear routing.
				path=new PT.ERPTransmissions.JobT.AlternatePath("1", "Alternate Path 1", 0);

				// Operation 10
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"10",requiredQty,new TimeSpan(0,12,0),SetupCodeWide,Color.Yellow,SetupNbrWide,setupSpan,deviation,"Cut");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR1",
					"ResReqt 1",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"48” Cross cut"));

				//Add Materials
				materialId="Posts";
				materialName="4x4 x 6' Maple Post";
				availDate=DateTime.Today.AddDays(15); //(addDays++%10);
				stdLeadTime=new TimeSpan(14,0,0,0);
				double matlTotalReqdQty=requiredQty*4;
				material=new JobT.MaterialRequirement(materialId,materialName,availDate,stdLeadTime, matlTotalReqdQty);
				material.ConstraintType=PT.SchedulerDefinitions.MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate; // (PT.SchedulerDefinitions.MaterialRequirementDefs.constraintTypes)(constraintType++%3);
				material.MaterialName=materialId;
				material.MaterialDescription=materialName;
				operation.Add(material);

				//Add an Attribute for the material
				PtAttribute attrib=new PtAttribute(materialName);
				attrib.Description="material";
				attrib.ColorCode=System.Drawing.Color.Gold;			
				attrib.Number=1;
				operation.PtAttributes.Add(attrib);

				// Operation 20
				setupSpan=new TimeSpan(0,30,0);
				operation=AddOperation(mo,"20",requiredQty,new TimeSpan(0,20,0),SetupCodeWide,Color.PeachPuff,SetupNbrWide,setupSpan,deviation,"Plane");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR2",
					"ResReqt 2",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"48” Planing"));

				LinkSuccessor("10","20",operation,path);
				//Add Materials
				materialId="Board";
				materialName="4' x 6' x 3/4\" Particle Board";
				availDate=DateTime.Today.AddDays(3);
				stdLeadTime=new TimeSpan(1,0,0,0);
				material=new JobT.MaterialRequirement(materialId,materialName,availDate,stdLeadTime,requiredQty);
				material.ConstraintType=PT.SchedulerDefinitions.MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate;
				material.MaterialName=materialId;
				material.MaterialDescription=materialName;
				operation.Add(material);

				// Operation 30
				setupSpan=new TimeSpan(2,0,0);
				operation=AddOperation(mo,"30",requiredQty,new TimeSpan(0,5,0),SetupCodeStandard,Color.Purple,SetupNbrStandard,setupSpan,deviation,"Join");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR3",
					"ResReqt 3",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Joining"));
				LinkSuccessor("20","30",operation,path);

				// Operation 40
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"40",requiredQty,new TimeSpan(0,30,0),SetupCodeStandard,Color.RosyBrown,SetupNbrStandard,setupSpan,deviation,"Route");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR4",
					"ResReqt 4",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Routing"));
				LinkSuccessor("30","40",operation,path);

				// Operation 50
				setupSpan=new TimeSpan(0,45,0);
				operation=AddOperation(mo,"50",requiredQty,new TimeSpan(0,20,0),SetupCodeHeavy,Color.DarkBlue,SetupNbrHeavy,setupSpan,deviation,"Sand");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR5",
					"ResReqt 5",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Sanding"));
				LinkSuccessor("40","50",operation,path);

				// Operation 60
				setupSpan=new TimeSpan(2,0,0);
				operation=AddOperation(mo,"60",requiredQty,new TimeSpan(0,10,0),SetupCodeClear,Color.White,SetupNbrClear,setupSpan,deviation,"Coat");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR6",
					"ResReqt 6",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Lacquer coating"));
				LinkSuccessor("50","60",operation,path);

				// Operation 70
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"70",requiredQty,new TimeSpan(0,10,0),SetupCodeHeavy,Color.DarkBlue,SetupNbrHeavy,setupSpan,deviation,"Polish");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR7",
					"ResReqt 7",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Sanding", "Polishing"));
				LinkSuccessor("60","70",operation,path);

				// Operation 80
				setupSpan=new TimeSpan(4,0,0);
				operation=AddOperation(mo,"80",requiredQty,new TimeSpan(0,20,0),SetupCodeStandard,Color.Teal,SetupNbrStandard,setupSpan,deviation,"Assemble");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR8",
					"ResReqt 8",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Assembly"));
				LinkSuccessor("70","80",operation,path);

				// Operation 81
				setupSpan=new TimeSpan(4,0,0);
				operation=AddOperation(mo,"81",requiredQty,new TimeSpan(0,20,0),SetupCodeStandard,Color.Teal,SetupNbrStandard,setupSpan,deviation,"Surface Mount");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR8",
					"ResReqt 8",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Assembly"));
				LinkSuccessor("80","81",operation,path);

				// Operation 90
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"90",requiredQty,new TimeSpan(0,2,0),SetupCodeStandard,Color.Gray,SetupNbrStandard,setupSpan,deviation,"Inspect");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR9",
					"ResReqt 9",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Inspecting"));
				LinkSuccessor("81","90",operation,path);

				// Operation 100
				setupSpan=new TimeSpan(2,0,0);
				operation=AddOperation(mo,"100",requiredQty,new TimeSpan(0,1,0),SetupCodeStandard,Color.Pink,SetupNbrStandard,setupSpan,deviation,"Ship");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR10",
					"ResReqt 10",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Shipping"));
				LinkSuccessor("90","100",operation,path);

				mo.AddPath(path);
				job.Add(mo);
				#endregion Table

				#region Chair MO
				mo=new PT.ERPTransmissions.JobT.ManufacturingOrder("MO2", string.Format("M{0}", 2), requiredQty * 8);
				mo.ProductName="6295-58 Arm Chair";
				mo.ProductColor=Color.Purple;
				mo.ReleaseDateTime=job.NeedDateTime.Subtract(new TimeSpan(21,0,0,0));
				mo.IsReleased=mo.ReleaseDateTime<DateTime.Now.AddDays(14);

				finishMoUpTo=random.Next(80); //Operations with opn number less than or equal to this are marked as finished

				// Create a linear routing.
				path=new PT.ERPTransmissions.JobT.AlternatePath("1", "Alternate Path 1", 0);

				// Operation 10
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"10",requiredQty * 8,new TimeSpan(0,13,0),SetupCodeStandard,Color.SteelBlue,SetupNbrStandard,setupSpan,deviation,"Cut");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR10",
					"ResReqt 10",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Curved cutting"));
				//Add Materials
				materialId="Maple Plank";
				materialName="24x24 x 3/4 Maple Plank";
				availDate=DateTime.Today.AddDays(addDays2++%15);
				stdLeadTime=new TimeSpan(28,0,0,0);
				material=new JobT.MaterialRequirement(materialId,materialName,availDate,stdLeadTime,requiredQty);
				material.ConstraintType=(PT.SchedulerDefinitions.MaterialRequirementDefs.constraintTypes)(addDays2++%2+1);
				material.MaterialName=materialId;
				material.MaterialDescription=materialName;
				operation.Add(material);

				// Operation 20
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"20",requiredQty * 8,new TimeSpan(0,10,0),SetupCodeNarrow,Color.Orange,SetupNbrNarrow,setupSpan,deviation,"Route");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR20",
					"ResReqt 20",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Routing"));
				LinkSuccessor("10","20",operation,path);

				// Operation 30
				setupSpan=new TimeSpan(3,0,0);
				operation=AddOperation(mo,"30",requiredQty * 8,new TimeSpan(0,10,0),SetupCodeHeavy,Color.DarkBlue,SetupNbrHeavy,setupSpan,deviation,"Sand");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR30",
					"ResReqt 30",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Sanding"));
				LinkSuccessor("20","30",operation,path);

				// Operation 40
				setupSpan=new TimeSpan(3,0,0);
				operation=AddOperation(mo,"40",requiredQty * 8,new TimeSpan(0,5,0),SetupCodeClear,Color.White,SetupNbrClear,setupSpan,deviation,"Coat");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR40",
					"ResReqt 40",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Lacquer coating"));
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR40-2",
					"ResReqt 40-2",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Painting"));
				LinkSuccessor("30","40",operation,path);

				// Operation 50
				setupSpan=new TimeSpan(5,0,0); //Large setup and cycle time so this is the bottleneck.
				operation=AddOperation(mo,"50",requiredQty * 8,new TimeSpan(0,15,0),SetupCodeLight,Color.LightBlue,SetupNbrLight,setupSpan,deviation,"Polish");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR50",
					"ResReqt 50",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Sanding", "Polishing"));
				LinkSuccessor("40","50",operation,path);

				// Operation 60
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"60",requiredQty * 8,new TimeSpan(0,20,0),SetupCodeStandard,Color.Tan,SetupNbrStandard,setupSpan,deviation,"Assemble");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR60",
					"ResReqt 60",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Assembly"));
				LinkSuccessor("50","60",operation,path);

				// Operation 70
				setupSpan=new TimeSpan(2,0,0);
				operation=AddOperation(mo,"70",requiredQty * 8,new TimeSpan(0,2,0),SetupCodeStandard,Color.Gold,SetupNbrStandard,setupSpan,deviation,"Inspect");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR70",
					"ResReqt 70",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Inspecting"));
				LinkSuccessor("60","70",operation,path);

				// Operation 80
				setupSpan=new TimeSpan(2,0,0);
				operation=AddOperation(mo,"80",requiredQty * 8,new TimeSpan(0,1,0),SetupCodeStandard,Color.IndianRed,SetupNbrStandard,setupSpan,deviation,"Ship");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR80",
					"ResReqt 80",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Shipping"));
				LinkSuccessor("70","80",operation,path);

				mo.AddPath(path);
				job.Add(mo);
				#endregion Table
			}
			#region China Cabinet & Sideboard Credenza
		{
			DateTime commonReleaseDate=job.NeedDateTime.Subtract(new TimeSpan(21,0,0,0));
		{
			//--------------------------------------------------------------------------
			// China Cabinet
			//--------------------------------------------------------------------------
			double moQty=requiredQty;
			mo=new PT.ERPTransmissions.JobT.ManufacturingOrder("MO3", string.Format("M{0}", 3), moQty);
			mo.ProductName="1115-06 China Cabinet";
			mo.ProductColor=Color.White;
			mo.ReleaseDateTime=commonReleaseDate;
			mo.IsReleased=mo.ReleaseDateTime<DateTime.Now.AddDays(14);

			finishMoUpTo=random.Next(40); //Operations with opn number less than or equal to this are marked as finished

			// Create a linear routing.
			path=new PT.ERPTransmissions.JobT.AlternatePath("1", "Alternate Path 1", 0);

			// Operation 10
			setupSpan=new TimeSpan(1,0,0);
			operation=AddOperation(mo,"10",moQty,new TimeSpan(4,0,0),SetupCodeStandard,Color.AntiqueWhite,SetupNbrStandard,setupSpan,deviation,"Cut");
			operation.AddResourceRequirement
				(new JobT.InternalOperation.ResourceRequirement(
				"RR10",
				"ResReqt 10",
				PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
				"24” Cross cut"));

			// Operation 20
			setupSpan=new TimeSpan(1,0,0);
			operation=AddOperation(mo,"20",moQty,new TimeSpan(1,0,0),SetupCodeStandard,Color.AntiqueWhite,SetupNbrStandard,setupSpan,deviation,"Turn");
			operation.AddResourceRequirement
				(new JobT.InternalOperation.ResourceRequirement(
				"RR20",
				"ResReqt 20",
				PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
				"Lathing"));

			LinkSuccessor("10", "20", operation, path);

			// Operation 30
			setupSpan=new TimeSpan(1,0,0);
			operation=AddOperation(mo,"30",moQty,new TimeSpan(1,0,0),SetupCodeStandard,Color.AntiqueWhite,SetupNbrStandard,setupSpan,deviation,"Coat");
			operation.AddResourceRequirement
				(new JobT.InternalOperation.ResourceRequirement(
				"RR30",
				"ResReqt 30",
				PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
				"Lathing"));

			LinkSuccessor("10", "30", operation, path);

			// Operation 40
			setupSpan=new TimeSpan(1,0,0);
			operation=AddOperation(mo,"40",moQty,new TimeSpan(1,0,0),SetupCodeStandard,Color.AntiqueWhite,SetupNbrStandard,setupSpan,deviation,"Assemble");
			operation.AddResourceRequirement
				(new JobT.InternalOperation.ResourceRequirement(
				"RR40",
				"ResReqt 40",
				PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
				"Assembly"));

			LinkSuccessor("20", "40", operation, path);
			LinkSuccessor("30", "40", operation, path);


			mo.AddPath(path);
			job.Add(mo);
		}

			if(!this.testCB.Checked)
			{
			{
				//--------------------------------------------------------------------------
				// Sideboard Credenza
				//--------------------------------------------------------------------------
				double moQty=requiredQty;
				mo=new PT.ERPTransmissions.JobT.ManufacturingOrder("SBC", string.Format("M{0}", "SBC"), moQty);
				mo.ProductName="9797-54 Sideboard Credenza";
				mo.ProductColor=Color.Olive;
				mo.ReleaseDateTime=commonReleaseDate;
				mo.IsReleased=mo.ReleaseDateTime<DateTime.Now.AddDays(14);

				finishMoUpTo=random.Next(10); //Operations with opn number less than or equal to this are marked as finished

				// Create a linear routing.
				path=new PT.ERPTransmissions.JobT.AlternatePath("1", "Alternate Path 1", 0);

				// Operation 10
				setupSpan=new TimeSpan(0,30,0);
				operation=AddOperation(mo,"10",moQty,new TimeSpan(3,30,0),SetupCodeStandard,Color.RosyBrown,SetupNbrStandard,setupSpan,deviation,"Assemble");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR10",
					"ResReqt 10",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Assembly"));

				JobT.AlternateNode node=new PT.ERPTransmissions.JobT.AlternateNode(operation.ExternalId);
				path.Add(node);

				mo.AddPath(path);
				job.Add(mo);
			}

			{
				//--------------------------------------------------------------------------
				// Predecessor task - Cherry wood
				//--------------------------------------------------------------------------
				double moQty=requiredQty*16;
				mo=new PT.ERPTransmissions.JobT.ManufacturingOrder("MO4", string.Format("M{0}", 4), moQty);
				mo.ProductName="2413-54 Cherry Wood";
				mo.ProductColor=Color.Maroon;
				mo.ReleaseDateTime=commonReleaseDate;
				mo.IsReleased=mo.ReleaseDateTime<DateTime.Now.AddDays(14);

				finishMoUpTo=random.Next(10); //Operations with opn number less than or equal to this are marked as finished

				// Create a linear routing.
				path=new PT.ERPTransmissions.JobT.AlternatePath("1", "Alternate Path 1", 0);

				// Operation 10
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"10",moQty,new TimeSpan(0,10,0),SetupCodeStandard,Color.Cornsilk,SetupNbrStandard,setupSpan,deviation,"Coat");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR10",
					"ResReqt 10",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Lacquer coating"));
				//					// Create an activity for the operation.
				//					JobT.InternalActivity activity=new PT.ERPTransmissions.JobT.InternalActivity("1");
				//					activity.RequiredFinishQty=moQty;
				//
				//					if(this.IncludeFinishesCB.Checked && finishMoUpTo==1)
				//						activity.ProductionStatus=PT.SchedulerDefinitions.InternalActivityDefs.productionStatuses.Finished;
				//					operation.AddInternalActivity(activity);

				JobT.AlternateNode node=new PT.ERPTransmissions.JobT.AlternateNode(operation.ExternalId);
				path.Add(node);

				mo.AddPath(path);

				// Setup successor task constraint. This task constrains MO3 operation 10.
				JobT.SuccessorMO successorMO1=new PT.ERPTransmissions.JobT.SuccessorMO("1", job.ExternalId, "MO3");
				successorMO1.AlternatePathExternalId="1";
				successorMO1.OperationExternalId="10";
				mo.SuccessorMOs.Add(successorMO1);

				// Setup successor task constraint. This task constrains SBC operation 10.
				JobT.SuccessorMO successorMO2=new PT.ERPTransmissions.JobT.SuccessorMO("2", job.ExternalId, "SBC");
				successorMO2.AlternatePathExternalId="1";
				successorMO2.OperationExternalId="10";
				mo.SuccessorMOs.Add(successorMO2);

				job.Add(mo);
			}
					
			{
				//--------------------------------------------------------------------------
				// Predecessor task - Glass Shelves
				//--------------------------------------------------------------------------
				double moQty=requiredQty*4;
				mo=new PT.ERPTransmissions.JobT.ManufacturingOrder("MO5", string.Format("M{0}", 5), moQty);
				mo.ProductName="9571-00 Glass Shelf";
				mo.ProductColor=Color.Silver;
				mo.ReleaseDateTime=commonReleaseDate;
				mo.IsReleased=mo.ReleaseDateTime<DateTime.Now.AddDays(14);

				finishMoUpTo=random.Next(10); //Operations with opn number less than or equal to this are marked as finished

				// Create a linear routing.
				path=new PT.ERPTransmissions.JobT.AlternatePath("1", "Alternate Path 1", 0);

				// Operation 10
				setupSpan=new TimeSpan(1,0,0);
				operation=AddOperation(mo,"10",moQty,new TimeSpan(0,10,0),SetupCodeStandard,Color.Brown,SetupNbrStandard,setupSpan,deviation,"Polish");
				operation.AddResourceRequirement
					(new JobT.InternalOperation.ResourceRequirement(
					"RR10",
					"ResReqt 10",
					PT.SchedulerDefinitions.MainResourceDefs.usedDuringEnum.SetupAndRun,
					"Polishing"));

				JobT.AlternateNode node=new PT.ERPTransmissions.JobT.AlternateNode(operation.ExternalId);
				path.Add(node);

				mo.AddPath(path);

				// Setup the successor task constraint. This task constrains MO3 operation 10.
				JobT.SuccessorMO successorMO=new PT.ERPTransmissions.JobT.SuccessorMO("1", job.ExternalId, "MO3");
				successorMO.AlternatePathExternalId="1";
				successorMO.OperationExternalId="10";
				mo.SuccessorMOs.Add(successorMO);

				job.Add(mo);
			}
			}
		}
			#endregion

			return job;
		}

		JobT.ResourceOperation AddOperation(JobT.ManufacturingOrder mo, string externalId, double requiredQty, TimeSpan cycleSpan, string setupCode, Color setupColor, double setupNumber, TimeSpan setupSpan, double deviation, string description)
		{
			JobT.ResourceOperation operation=new PT.ERPTransmissions.JobT.ResourceOperation(externalId, string.Format("{0}", externalId), requiredQty, cycleSpan);
			// Create an activity for the operation.
			JobT.InternalActivity activity=new PT.ERPTransmissions.JobT.InternalActivity("1");
			operation.Description=description;
			operation.SetupCode=setupCode;
			operation.SetupColor=setupColor;
			operation.SetupNumber=setupNumber;
			operation.SetupSpan=setupSpan;
			operation.RequiredFinishQty=requiredQty;
			activity.RequiredFinishQty=requiredQty;

			if(this.IncludeFinishesCB.Checked)
			{
				try
				{
					int id=Convert.ToInt32(externalId);
					if(id<=this.finishMoUpTo) 
					{
						activity.ProductionStatus=PT.SchedulerDefinitions.InternalActivityDefs.productionStatuses.Finished;
						activity.ReportedRunSpan=new TimeSpan((long)(operation.CycleSpan.Ticks * activity.RequiredFinishQty * 2f * deviation));//From zero time to double the planned
						activity.ReportedSetupSpan=new TimeSpan((long)(operation.SetupSpan.Ticks * 2f * deviation));
						double qtyGood=((long)(activity.RequiredFinishQty * deviation));
						activity.ReportedGoodQty=qtyGood;
						activity.ReportedScrapQty=Math.Max(0,activity.RequiredFinishQty - activity.ReportedGoodQty);
					}
					else if(id<=(this.finishMoUpTo+10)) //Assumes operation numbers increment by 10!
					{
						activity.ProductionStatus=PT.SchedulerDefinitions.InternalActivityDefs.productionStatuses.Ready;
						if(id==20)
							activity.ReportedGoodQty=1; //so we can see some started
					}
					else
						activity.ProductionStatus=PT.SchedulerDefinitions.InternalActivityDefs.productionStatuses.Waiting;
				}
				catch
				{
				}
			}
			operation.AddInternalActivity(activity);

			mo.AddOperation(operation);

			return operation;
		}
	

		void LinkSuccessor(string predOpnExternalId, string sucOpnExternalId, JobT.ResourceOperation operation,JobT.AlternatePath path)
		{
			// Create the predecessor operation attributes (for Operations with predecessors only)
            JobT.PredecessorOperationAttributes predAtts = new PT.ERPTransmissions.JobT.PredecessorOperationAttributes(sucOpnExternalId);

			// Add this operation as a successor of the predecessor operation.
			JobT.AlternateNode successorPathNode=null;
			for(int pathNodeI=0; pathNodeI<path.Count; ++pathNodeI)
			{
				JobT.AlternateNode tempNode=path[pathNodeI];
				if(tempNode.OperationExternalId==predOpnExternalId)
				{
					successorPathNode=tempNode;
					break;
				}
			}

			if(successorPathNode==null)
			{
				successorPathNode=new PT.ERPTransmissions.JobT.AlternateNode(predOpnExternalId);
                successorPathNode.AddSuccessor(predAtts);
				path.Add(successorPathNode);
			}
			else
			{
                successorPathNode.AddSuccessor(predAtts);
			}
		}


		#endregion Furniture Factory Data Generation
		
		#region Simulation Tests
		int countTest=0;

		private void btnOptimize_Click(object sender, System.EventArgs e)
		{
			if(rbSimulate.Checked)
			{
				Simulate();
			}
			else if(this.rbCompress.Checked)
			{
				Compress();
			}
			else if(this.rbAdvanceClock.Checked)
			{
				ClockAdvance();
			}
			else if(this.rbExpedite.Checked)
			{
				Expedite();
			}
			else if(this.rbMove.Checked)
			{
				SimMove();
			}
			else if(this.rbExport.Checked)
			{
				ExportToDb();
			}
			else if(this.rbBeginReplay.Checked)
			{
				BeginReplay();
			}
			else if(this.rbPerformImport.Checked)
			{
				PerformImport();
			}
		}

		void Simulate()
		{
			Cursor.Current=Cursors.WaitCursor;
			OptimizeSettings optimizeSettings=new OptimizeSettings();
			optimizeSettings.SpecificStartTime=simulationStartTime;
			PT.Transmissions.ScenarioDetailOptimizeT optimizeT=new PT.Transmissions.ScenarioDetailOptimizeT(new BaseId(1), optimizeSettings);
			optimizeT.sequenced=false;
			PTBroadcaster.BroadcastStatic(optimizeT);
		}

		void Compress()
		{
			Cursor.Current=Cursors.WaitCursor;
			OptimizeSettings optimizeSettings=new OptimizeSettings();
			optimizeSettings.SpecificStartTime=simulationStartTime;
			PT.Transmissions.ScenarioDetailCompressT optimizeT=new PT.Transmissions.ScenarioDetailCompressT(new BaseId(1), optimizeSettings);
			optimizeT.sequenced=false;
			PTBroadcaster.BroadcastStatic(optimizeT);
		}

		void ClockAdvance()
		{
			Cursor.Current=Cursors.WaitCursor;

			DateTime dateTime=new DateTime(2004, 4, 12, 9, 0, 0);
			PT.Transmissions.ScenarioClockAdvanceT t=new PT.Transmissions.ScenarioClockAdvanceT(dateTime);

			t.sequenced=false;
			PTBroadcaster.BroadcastStatic(t);
		}

		void Expedite()
		{
			Cursor.Current=Cursors.WaitCursor;
			PT.Scheduler.MOKeyList mos=new PT.Scheduler.MOKeyList();
			PT.SchedulerDefinitions.ManufacturingOrderKey orderKey;

			if(countTest%2==0)
			{
				orderKey=new PT.SchedulerDefinitions.ManufacturingOrderKey(new BaseId(2), new BaseId(1));
			}
			else
			{
				orderKey=new PT.SchedulerDefinitions.ManufacturingOrderKey(new BaseId(1), new BaseId(1));
			}

			countTest++;

			mos.Add(orderKey, null);

			DateTime now=DateTime.Now;
			now=now.AddDays(1);
			DateTime expediteDate=new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
			
			PT.Transmissions.ScenarioDetailExpediteMOsT expediteT=new PT.Transmissions.ScenarioDetailExpediteMOsT(new BaseId(1), mos, expediteDate.Ticks,true,true);
			expediteT.sequenced=false;
			PTBroadcaster.BroadcastStatic(expediteT);
		}

		void SimMove()
		{
			Cursor.Current=Cursors.WaitCursor;
			PT.Scheduler.MOKeyList mos=new PT.Scheduler.MOKeyList();

			BaseId jobId=new BaseId(1);
			BaseId moId=new BaseId(1);
			BaseId operationId=new BaseId(1);
			BaseId activityId=new BaseId(1);
			BaseId blockId=new BaseId(1);
			BaseId resReqtId=new BaseId(1);
			PT.SchedulerDefinitions.BlockKey blockKey=new PT.SchedulerDefinitions.BlockKey(jobId, moId, operationId, activityId, blockId);
			DateTime now=DateTime.Now;
			now=now.AddDays(1);
			DateTime moveDate=new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
			PT.SchedulerDefinitions.ResourceKey machineKey=new PT.SchedulerDefinitions.ResourceKey(new BaseId(1), new BaseId(1), new BaseId(1));

            PT.Transmissions.ScenarioDetailMoveT moveT = new PT.Transmissions.ScenarioDetailMoveT(new BaseId(1), blockKey, machineKey, machineKey, moveDate.Ticks, true, true, true, true, true, true);
			moveT.sequenced=false; // This is just for testing. In the UI set it to true.
			PTBroadcaster.BroadcastStatic(moveT);
		}

		void ExportToDb()
		{
			Cursor.Current=Cursors.WaitCursor;
			//Write to database
			ScenarioDetailExportT exportT=new ScenarioDetailExportT(new BaseId(1),ScenarioDetailExportT.exportDestinations.ToDatabase);
			exportT.sequenced=false; // This is just for testing. In the UI set it to true.
			PTBroadcaster.BroadcastStatic(exportT);

			//Now write to xml
			exportT=new ScenarioDetailExportT(new BaseId(1),ScenarioDetailExportT.exportDestinations.ToXML);
			exportT.sequenced=false; // This is just for testing. In the UI set it to true.
			PTBroadcaster.BroadcastStatic(exportT);
		}

		void BeginReplay()
		{
			Cursor.Current=Cursors.WaitCursor;
			TriggerRecordingPlaybackT t=new TriggerRecordingPlaybackT();
			PTBroadcaster.BroadcastStatic(t);
		}

		void PerformImport()
		{
			PTInterface ptInterface=new PTInterface();
			ptInterface.RunImport("usadmin", BaseId.SCHEDULING_AGENT_ID, connectionNbr);
		}

		#endregion Simulation Tests
	
		#region Object Def Classes

		public class PlantDef
		{
			public string externalId;
			public string name;
			public PlantDef(string externalId, string name)
			{
				this.externalId=externalId;
				this.name=name;
			}
		}
		public class CapabilityDef
		{
			public string externalId;
			public string name;
			public CapabilityDef(string externalId, string name)
			{
				this.externalId=externalId;
				this.name=name;
			}
		}
		public class DepartmentDef
		{
			public string plantExternalId;
			public string externalId;
			public string name;
			public DepartmentDef(string plantExternalId, string externalId, string name)
			{
				this.plantExternalId=plantExternalId;
				this.externalId=externalId;
				this.name=name;
			}
		}
		public class MachineDef
		{
			public string plantExternalId;
			public string departmentExternalId;
			public string workcenter;
			public string externalId;
			public string name;
			public IdList capabilities;
			public MachineDef(string plantExternalId, string departmentExternalId, string externalId, string name, IdList capabilities, string workcenter)
			{
				this.plantExternalId=plantExternalId;
				this.departmentExternalId=departmentExternalId;
				this.externalId=externalId;
				this.name=name;
				this.capabilities=capabilities;
				this.workcenter=workcenter;
			}
		}
		#endregion
	}
}

