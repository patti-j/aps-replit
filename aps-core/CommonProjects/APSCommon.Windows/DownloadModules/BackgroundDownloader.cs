using System.Net;

using PT.APSCommon.Extensions;

namespace PT.APSCommon.Windows.DownloadModules;

public class BackgroundDownloader
{
    public BackgroundDownloader(string a_downloadUri, bool a_localInstallation, string a_userName, string a_password)
    {
        Client = new WebClient();
        if (a_localInstallation)
        {
            Client.UseDefaultCredentials = true;
        }
        else
        {
            Client.Credentials = new NetworkCredential(a_userName, a_password);
        }

        m_downloadUri = a_downloadUri;
        Client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadComplete);
        Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressed);
    }

    public bool DownloadCompleted;
    private string m_errorMessage;
    protected string m_downloadUri;

    public delegate void DownloadProgressHandler(object sender, DownloadProgressChangedEventArgs a_arg);

    public event DownloadProgressHandler DownloadProgressEvent;

    public delegate void DownloadCompleteHandler(object sender, DownloadDataCompletedEventArgs a_args);

    public event DownloadCompleteHandler DownloadCompleteEvent;

    public delegate void DownloadErrorHandler(Exception e);

    public event DownloadErrorHandler DownloadErrorEvent;

    public bool HasError => m_errorMessage != null;

    protected readonly WebClient Client;

    public virtual void BeginDownload()
    {
        try
        {
            Uri uri = new (m_downloadUri);
            Client.DownloadDataAsync(uri);
        }
        catch (Exception e)
        {
            InstallationFailed("Install Failed. Error downloading: ", e);
        }
    }

    private void DownloadComplete(object sender, DownloadDataCompletedEventArgs a_args)
    {
        DownloadCompleteEvent?.Invoke(sender, a_args);
    }

    private void DownloadProgressed(object sender, DownloadProgressChangedEventArgs a_args)
    {
        DownloadProgressEvent?.Invoke(sender, a_args);
    }

    public void InstallationFailed(string a_message, Exception a_e)
    {
        m_errorMessage = a_message.Localize() + a_e.Message;
        //label_Status.ForeColor = Color.DarkRed;
        DownloadCompleted = true;
        FireError(a_e);
    }

    public virtual void InstallationComplete()
    {
        //label_Status.ForeColor = Color.DarkGreen;
        DownloadCompleted = true;
    }

    public void FireError(Exception a_e)
    {
        DownloadErrorEvent?.Invoke(a_e);
    }

    //private void button_OpenPath_Click(object sender, EventArgs e)
    //{
    //    if (m_errorMessage == null)
    //    {
    //        System.Diagnostics.Process p = new System.Diagnostics.Process();
    //        //p.StartInfo.FileName = System.IO.Directory.GetParent(m_installationPath).FullName;
    //        p.StartInfo.FileName = m_installationPath;
    //        p.Start();
    //    }
    //    else
    //    {
    //        //show error instead
    //        MessageBox.Show(m_errorMessage, "Installation failure message".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //}
}