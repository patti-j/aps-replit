using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using PT.Common;

namespace SystemID
{
	public class Form1 : System.Windows.Forms.Form
	{
        private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button TryStartBTN;
		private System.Windows.Forms.Button StartLaterBTN;
        private System.Windows.Forms.TextBox statusBarTB;
        private ToolTip toolTip1;
        private Button BrowseKeyFolder;
        private TextBox doneLabel;
        private Label label1;
        private IContainer components;

        public Form1()
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            InitializeComponent();
            this.TopMost = true; //Was showing behind the installer
                       
        }

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.TryStartBTN = new System.Windows.Forms.Button();
            this.StartLaterBTN = new System.Windows.Forms.Button();
            this.statusBarTB = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.BrowseKeyFolder = new System.Windows.Forms.Button();
            this.doneLabel = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(82, 9);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(365, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "Please wait.  Obtaining System Id...";
            // 
            // TryStartBTN
            // 
            this.TryStartBTN.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TryStartBTN.Location = new System.Drawing.Point(12, 143);
            this.TryStartBTN.Name = "TryStartBTN";
            this.TryStartBTN.Size = new System.Drawing.Size(447, 55);
            this.TryStartBTN.TabIndex = 0;
            this.TryStartBTN.Text = "Start the APS services using my key.";
            this.TryStartBTN.Click += new System.EventHandler(this.TryStartBTN_Click);
            // 
            // StartLaterBTN
            // 
            this.StartLaterBTN.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.StartLaterBTN.Location = new System.Drawing.Point(12, 204);
            this.StartLaterBTN.Name = "StartLaterBTN";
            this.StartLaterBTN.Size = new System.Drawing.Size(447, 34);
            this.StartLaterBTN.TabIndex = 2;
            this.StartLaterBTN.Text = "I will install a new key and start the APS services manually in Control Panel" +
                ".";
            this.toolTip1.SetToolTip(this.StartLaterBTN, "Choosing this option requires manually starting the APS Services in Control P" +
                    "anel before running the system.");
            this.StartLaterBTN.Visible = false;
            this.StartLaterBTN.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusBarTB
            // 
            this.statusBarTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.statusBarTB.Location = new System.Drawing.Point(0, 276);
            this.statusBarTB.Name = "statusBarTB";
            this.statusBarTB.ReadOnly = true;
            this.statusBarTB.Size = new System.Drawing.Size(471, 20);
            this.statusBarTB.TabIndex = 0;
            // 
            // BrowseKeyFolder
            // 
            this.BrowseKeyFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseKeyFolder.Location = new System.Drawing.Point(254, 244);
            this.BrowseKeyFolder.Name = "BrowseKeyFolder";
            this.BrowseKeyFolder.Size = new System.Drawing.Size(205, 23);
            this.BrowseKeyFolder.TabIndex = 3;
            this.BrowseKeyFolder.Text = "Browse Key Folder...";
            this.BrowseKeyFolder.UseVisualStyleBackColor = true;
            this.BrowseKeyFolder.Click += new System.EventHandler(this.BrowseKeyFolder_Click);
            // 
            // doneLabel
            // 
            this.doneLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.doneLabel.Location = new System.Drawing.Point(12, 56);
            this.doneLabel.Multiline = true;
            this.doneLabel.Name = "doneLabel";
            this.doneLabel.ReadOnly = true;
            this.doneLabel.Size = new System.Drawing.Size(447, 81);
            this.doneLabel.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "System Id";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(471, 297);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.doneLabel);
            this.Controls.Add(this.TryStartBTN);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.BrowseKeyFolder);
            this.Controls.Add(this.statusBarTB);
            this.Controls.Add(this.StartLaterBTN);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "APS System Id";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Closed += new System.EventHandler(this.Form1_Closed);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		[STAThread]
        static void Main() 
		{
			Application.Run(new Form1());
		}

		string systemId;
		delegate void DoneDelegate();

		public void GetSystemId()
		{
			//Cursor.Current=Cursors.AppStarting;

			try
			{
				systemId=MachineInfo.GetProcessorId();
			}
			catch(Exception e)
			{
				systemId="Error!";
                Cursor.Current = Cursors.WaitCursor;
                MessageBox.Show(e.Message, "SystemId Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			if(systemId.Length==0)
			{
				systemId="Error!";
			}

            Done(); //this.Invoke(new DoneDelegate(Done));
		}

		public void Done()
		{
			//RegistryEditor regEditor=new RegistryEditor();
			//this.TryStartBTN.Visible=true;
			//this.StartLaterBTN.Visible=true;
			this.textBox1.Text=systemId;
            
   //         string ptKeyFile = System.IO.Path.Combine(regEditor.PtSystemService.KeyFolder, "pt.key");

   //         if (!System.IO.File.Exists(ptKeyFile)) //no key so need to get one
   //         {
   //             try
   //             {

   //                 Clipboard.SetDataObject(systemId, true);
   //                 this.doneLabel.Text = "Please paste the above System Id into an e-mail and send it to: " + regEditor.KeyRequestEmailAddress +
   //                 "  You will receive an unlock key and further instructions. (The Id has already been Copied to the Clipboard.)" +
   //                 "  If necessary, this ID can be obtained again later by using the shortcut: \"Obtain System Id for License Key\".";
   //                 ;
   //             }
   //             catch (Exception)
   //             {
   //                 this.doneLabel.Text = "Please paste the above Id into an e-mail and send it to: " + regEditor.KeyRequestEmailAddress +
   //                 "  You will receive an unlock key and further instructions." +
   //                 "  If necessary, this ID can be obtained again later by using the shortcut: \"Obtain System Id for License Key\".";
   //                 ;
   //             }
   //         }
   //         else
   //             this.doneLabel.Text = "A system key is installed so you can start the system services now or you can choose to start them later manually after installing a different key.";
			//this.doneLabel.Refresh();  //need to force immediate draw when displayed by installer
			//Cursor.Current=Cursors.Default;
		}

        
		private void Form1_Load(object sender, System.EventArgs e)
		{
			if(this.DesignMode)return;
            Cursor.Current = Cursors.WaitCursor;

            GetSystemId();
            //System.Threading.Thread t=new System.Threading.Thread(new System.Threading.ThreadStart(GetSystemId));
            //t.Start();
            Cursor.Current = Cursors.Default;
		}

		private void Form1_Closed(object sender, System.EventArgs e)
		{
			Environment.Exit(0);
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}


		
		void StartServices()
		{
			//Cursor.Current=Cursors.WaitCursor;
			//if(StartService(PT.ConstantDefinitions.SolutionNames.SystemServiceName))
			//{
			//	Cursor.Current=Cursors.WaitCursor;
			//	if(StartService(PT.ConstantDefinitions.SolutionNames.UpdaterServiceName))
			//	{
			//		Cursor.Current=Cursors.WaitCursor;
   //                 if (StartService(PT.ConstantDefinitions.SolutionNames.InterfaceServiceName))
   //                 {
   // 					Cursor.Current=Cursors.WaitCursor;
   //                     if (StartService(PT.ConstantDefinitions.SolutionNames.ExtraServicesName))
   //                     {
   //                         Cursor.Current = Cursors.WaitCursor;
   //                         StartService(PT.ConstantDefinitions.SolutionNames.SchedulingAgentServicesName);
   //                         this.Close(); //done
   //                     }
   //                 }
			//	}
			//}
			//Cursor.Current=Cursors.Default;
		}

		bool StartService(string serviceName)
		{
			//try
			//{
			//	System.ServiceProcess.ServiceController myController=new System.ServiceProcess.ServiceController(serviceName);
			//	if(myController.Status==System.ServiceProcess.ServiceControllerStatus.Running)
			//	{
			//		myController.Stop();
			//		myController.Refresh();
			//		//wait while stopping.  This can take a while.
			//		while(myController.Status==System.ServiceProcess.ServiceControllerStatus.StopPending)
			//		{
			//			System.Threading.Thread.Sleep(100);
			//			myController.Refresh();
			//		}		
			//	}

			//	//Try to start it if it's stopped
			//	if(myController.Status==System.ServiceProcess.ServiceControllerStatus.Stopped)
			//	{
   //                 SetStatus(String.Format("Starting Services..."));
			//		myController.Start();
			//		myController.Refresh();
			//		//Make the user wait as long as it's still trying to start.  This can take a while.
			//		while(myController.Status==System.ServiceProcess.ServiceControllerStatus.StartPending)
			//		{
			//			System.Threading.Thread.Sleep(100);
			//			myController.Refresh();
			//		}
			//	}
			//	SetStatus("");
			//	Cursor.Current=Cursors.Default;

			//	if(myController.Status==System.ServiceProcess.ServiceControllerStatus.Running)
			//	{
			//		return true;
			//	}
			//	else
			//	{
			//		RegistryEditor regEditor=new RegistryEditor();
			//		string msg;
			//		string caption=String.Format("{0} Service Failed to Start",serviceName);
			//		if(serviceName==PT.ConstantDefinitions.SolutionNames.SystemServiceName)
			//		{
			//			msg=String.Format("Please verify that your key files have been saved to the folder: {1} and check the log file in the folder: {2} for more information.",PT.ConstantDefinitions.SolutionNames.SystemServiceName,regEditor.PtSystemService.KeyFolder,regEditor.PtSystemService.AlertsFolder);
			//			MessageBox.Show(msg,caption,MessageBoxButtons.OK,MessageBoxIcon.Warning);
			//			System.Diagnostics.Process.Start(regEditor.PtSystemService.KeyFolder);
			//		}
			//		else if (serviceName==PT.ConstantDefinitions.SolutionNames.UpdaterServiceName)
			//		{
			//			msg=String.Format("Please check for errors in the {0} folder for more information.",regEditor.PtClientUpdaterService.AlertsFolder);
			//			MessageBox.Show(msg,caption,MessageBoxButtons.OK,MessageBoxIcon.Warning);
			//			System.Diagnostics.Process.Start(regEditor.PtClientUpdaterService.AlertsFolder);
			//		}
			//		else
			//		{
			//			msg=String.Format("Please check for errors in the {0} folder for more information.",regEditor.PtExtraServices.AlertsFolder);
			//			MessageBox.Show(msg,caption,MessageBoxButtons.OK,MessageBoxIcon.Warning);
			//			System.Diagnostics.Process.Start(regEditor.PtExtraServices.AlertsFolder);
			//		}
					
			//		Cursor.Current=Cursors.Default;


			//		this.TryStartBTN.Enabled=true; //give them another chance

			//		return false;
			//	}
			//}
			//catch(Exception err)
			//{
			//	Cursor.Current=Cursors.Default;
			//	MessageBox.Show(err.Message,"Service Start Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
			//	return false;
			//}      
            return true;
        }

        void SetStatus(string message)
        {
            this.statusBarTB.Text = message;
            this.statusBarTB.Refresh();
        }

        private void BrowseKeyFolder_Click(object sender, EventArgs e)
        {
            try
            {
                //RegistryEditor redEditor = new RegistryEditor();
                //System.Diagnostics.Process.Start(redEditor.PtSystemService.KeyFolder);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            
        }

        private void TryStartBTN_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.AppStarting;
            this.TryStartBTN.Enabled = false;
            
            // this.TryStartBTN.Text="Please wait...attempting to start services.  This dialog will close automatically when done.";
            //System.Threading.Thread t=new System.Threading.Thread(new System.Threading.ThreadStart(StartServices));
            //t.Start();
            StartServices();
        }
	}
}
