using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PT.Common;

static class Program
{
	[STAThread]
	static void Main(string[] args)
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new MainForm(args ?? Array.Empty<string>()));
	}
}
