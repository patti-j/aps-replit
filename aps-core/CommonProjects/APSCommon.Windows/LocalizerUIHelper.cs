using PT.Common.Exceptions;
using PT.Common.Localization;

namespace PT.APSCommon.Windows
{
    public class LocalizerUIHelper
    {
        public static void ShowHelp(string a_topic)
        {
            try
            {
                WebUtility.OpenWebPage(Localizer.GetUrlString(a_topic));
            }
            catch (PTHandleableException e)
            {
                #if DEBUG
                System.Windows.Forms.MessageBox.Show(e.Message, "Help Not Found", System.Windows.Forms.MessageBoxButtons.OK);
                #endif
                //help content was not found for this topic. Don't open a blank help site.
            }
        }

    }
}
