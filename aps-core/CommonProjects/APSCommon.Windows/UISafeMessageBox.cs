using System.Windows.Forms;

namespace PT.APSCommon.Windows;

public class UISafeMessageBox
{
    /// <summary>
    /// Safe show message box that can be called from non-UI threads.
    /// </summary>
    /// <param name="a_owner">Owning parent window.</param>
    /// <param name="a_message"></param>
    /// <param name="a_caption"></param>
    public static void ShowMessageBox(Form a_owner, string a_message, string a_caption)
    {
        a_owner.Invoke(new Action(delegate { MessageBox.Show(a_owner, a_message, a_caption); }));
    }
}