using System.Windows.Forms;

using PT.APSCommon.Windows;

namespace PT.UIDefinitions.Interfaces;

public interface IMessageProvider
{
    DialogResult ShowMessageBox(string a_msg, string a_title, bool a_dialogMode, bool a_allowResize = true, Form a_owner = null, string a_details = null);
    DialogResult ShowMessageBoxError(Exception a_e, bool a_dialogMode, string a_notice = null, Form a_owner = null);
    DialogResult ShowMessageBox(PTMessage a_message, bool a_dialogMode = false, bool a_allowResize = true, Form a_owner = null);
    void ShowMessagePrompt(PTMessage a_message, Action<DialogResult> a_closingAction);
    void ShowMessageBoxSimple(string a_msg, string a_title = null);
    void ShowMessage(string a_message, bool a_error = false);
    void ShowBusyMessage();
    string GetLastMessage();
}