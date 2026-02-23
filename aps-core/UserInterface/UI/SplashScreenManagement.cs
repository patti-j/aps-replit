using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraSplashScreen;
using PT.APSCommon.Windows;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;
using PT.UIDefinitions.Interfaces;
using PT.UIDefinitions.Splash;

namespace PT.UI;

internal class SplashScreenManagement : IDisposable
{
    private readonly Form m_hostForm;
    private readonly IExceptionManager m_exceptionManager;

    internal SplashScreenManagement(Form a_hostForm, IExceptionManager a_exceptionManager, IBrand a_brand, string a_instanceName)
    {
        m_hostForm = a_hostForm;
        m_exceptionManager = a_exceptionManager;

        m_splashControl = a_brand.GetSplashControl(a_instanceName);
    }

    internal void HideExternalSplash()
    {
        try
        {
            if (SplashScreenManager.Default != null)
            {
                PTSplashScreenManager.CloseSplash();
            }

            m_hostForm.BringToFront();
            Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    internal void ShowExternalSplash()
    {
        //TODO: Set image based on version.  EA, Maintenance, etc...
        Image staticImage = PtImageCache.GetStaticImage("splash-eap");
        PTSplashScreenManager.ShowImage(staticImage, true, true, SplashImagePainter.Painter);
    }

    internal void UpdateSplashDescription(string a_localizedMessage)
    {
        try
        {
            m_splashControl.SetStatusLabel(a_localizedMessage);
        }
        catch (Exception e)
        {
            m_exceptionManager.LogSimpleException(e);
        }
    }

    internal void UpdateSplashWarning()
    {
        try
        {
            m_splashControl.CalculateWarningText();
        }
        catch (Exception e)
        {
            m_exceptionManager.LogSimpleException(e);
        }
    }

    internal void UpdateSplashLocalization()
    {
        try
        {
            SplashScreenManager.Default?.SendCommand(PackageEnums.ESplashScreenCommand.LocalizeImage, null);
        }
        catch (Exception e)
        {
            m_exceptionManager.LogSimpleException(e);
        }
    }

    internal void BringSplashToFront()
    {
        try
        {
            PTSplashScreenManager.BringToFront();
        }
        catch (Exception e)
        {
            m_exceptionManager.LogSimpleException(e);
        }
    }

    private readonly ISplashControlInterface m_splashControl;
    
    public void Dispose()
    {
        m_splashControl?.Dispose();
    }
}