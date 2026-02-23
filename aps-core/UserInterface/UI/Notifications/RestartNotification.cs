namespace PT.UI.Notifications;

//TODO: Restore
//internal class RestartNotification : IRibbonNotificationsElement
//{
//    private readonly MainForm m_mainForm;
//    private readonly string m_message;
//    public event Action<IBarButtonElement> DisplayRefresh;
//    internal RestartNotification(MainForm a_mainForm, string a_message)
//    {
//        m_mainForm = a_mainForm;
//        m_message = a_message;
//    }

//    public string PackageObjectId => "UI_MainForm_RestartPendingNotification";
//    public string ElementKey => "GeneralNotifications";
//    public uint PositionPriority => 50;
//    public void Dispose()
//    {
//        ButtonImage?.Dispose();
//    }

//    public string GroupKey => "Notifications";
//    public string Caption => "Restart Pending";
//    public string SuperTipTitle => "";
//    public string SuperTipContent => "";
//    public Image ButtonImage => null;
//    public BarButtonStyle ButtonStyle => BarButtonStyle.Default;
//    public int NumberToShow => 1;
//    public void ItemClicked()
//    {
//        DialogResult result = m_mainForm.MessageProvider.ShowMessageBox(new PTMessage(m_message, "Restart Required".Localize()) { Classification = PTMessage.EMessageClassification.Question }, true);
//        if (result == DialogResult.Yes)
//        {
//            m_mainForm.RestartImmediately();
//        }
//    }

//    /// <summary>
//    /// Keep this button visible until the user restarts
//    /// </summary>
//    public bool Persistant => true;
//    public bool Dynamic => false;
//}