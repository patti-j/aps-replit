using System.Diagnostics;
using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.Common.Localization;

namespace PT.APSCommon.Windows;

// TODO: Consider extracting MessageBox error handling and move this to core APSCommon
public class WebUtility
{
    public static async Task OpenWebPage(string a_url)
    {
        Exception openWebPageAsync = await OpenWebPageAsync(a_url);
        if (openWebPageAsync != null)
        {
            string msg = Localizer.GetErrorString("2705", new object[] { openWebPageAsync, a_url });
            MessageBox.Show(msg, "Web Browse Error".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>
    /// Attempts to open a webpage. Returns whether it was successful.
    /// </summary>
    public static async Task<Exception> OpenWebPageAsync(string a_url)
    {
        Exception e = await Task.Run(() => OpenWebPageProcess(a_url)).ConfigureAwait(false);
        return e;
    }

    private static Exception OpenWebPageProcess(string a_url)
    {
        try
        {
            ProcessStartInfo startInfo = new ()
            {
                UseShellExecute = true,
                FileName = a_url
            };
            Process.Start(startInfo);
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}