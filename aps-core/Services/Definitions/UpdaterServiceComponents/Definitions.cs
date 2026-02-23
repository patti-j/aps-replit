using System;

namespace PT.UpdaterServiceComponents
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Definitions
	{
		public static string WorkingDirectoryName
		{
			get
			{
				return "ClientUpdaterWorkingDirectory";
			}
		}

		public static string TmpUpdateExtension
		{
			get
			{
				return ".tmp_client_update_loader";
			}
		}

		public static string LatestUpdateFileName
		{
			get
			{
				return "LatestUpdateFileList.Old";
			}
		}
	}
}
