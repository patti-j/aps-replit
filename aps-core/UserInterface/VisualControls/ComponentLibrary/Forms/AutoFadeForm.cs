namespace PT.ComponentLibrary.Forms;

public partial class AutoFadeForm : BaseResizableForm
{
    public AutoFadeForm() { }

    public AutoFadeForm(bool a_autoClose, string a_layoutId, bool a_allowResize = true) : base(a_layoutId, a_allowResize)
    {
        InitializeComponent();
        m_fadeMode = a_autoClose ? EFadeMode.FadeToClose : EFadeMode.NoFadeClose;
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeFormIncludingCaption(this);
    }

    private const double c_startOpacity = 1;
    private const int c_tickInterval = 50; //milliseconds		
    protected double m_fadeDurationMs = 2000;
    protected double m_fadeDelayMs = 3000;
    protected double m_showDelayMs = 3000;
    private double m_decrementAmount; //calculated

    private DateTime m_start;

    protected bool m_pinned;

    protected enum EFadeMode
    {
        FadeToClose,
        FadeToHide,
        Close,
        Hide,
        NoFadeClose,
        NoFadeHide
    }

    protected EFadeMode m_fadeMode = EFadeMode.FadeToClose;

    protected EFadeMode FadeMode
    {
        get => m_fadeMode;
        set
        {
            m_fadeMode = value;
            switch (m_fadeMode)
            {
                case EFadeMode.FadeToClose:
                case EFadeMode.FadeToHide:
                case EFadeMode.Close:
                case EFadeMode.Hide:
                    StartAutoHide();
                    break;
                case EFadeMode.NoFadeClose:
                case EFadeMode.NoFadeHide:
                    StopAutoHide();
                    break;
            }
        }
    }

    private double m_internalOpacity;

    private void m_autoCloseTimer_Tick(object sender, EventArgs e)
    {
        //Close slowly once it's been up for 3 seconds
        if (DateTime.Now > m_start.AddMilliseconds(m_fadeDelayMs))
        {
            UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        switch (m_fadeMode)
        {
            case EFadeMode.FadeToClose:
            case EFadeMode.FadeToHide:
                Opacity = Opacity - m_decrementAmount;
                if (Opacity <= 0)
                {
                    FadeFormClose();
                }

                break;
            case EFadeMode.Close:
            case EFadeMode.Hide:
                m_internalOpacity = m_internalOpacity - m_decrementAmount;
                if (m_internalOpacity <= 0)
                {
                    FadeFormClose();
                }

                break;
        }
    }

    private void AutoFadeForm_MouseLeave(object sender, EventArgs e)
    {
        //			this.autoCloseTimer.Enabled=true;  //If the user is investigating then don't close it.  Let the user close it.
    }

    private void AutoFadeForm_MouseEnter(object sender, EventArgs e)
    {
        StopAutoHide();
    }

    private void AutoFadeForm_MouseMove(object sender, EventArgs e)
    {
        StopAutoHide();
    }

    private void MessageDialog_Resize(object sender, EventArgs e)
    {
        StopAutoHide();
    }

    protected void StopAutoHide()
    {
        //Stop the timer and make the dialog opaque so the user can easily read the dialog
        m_autoCloseTimer.Stop();
        Opacity = c_startOpacity;
        m_internalOpacity = c_startOpacity;
    }

    protected void StartAutoHide()
    {
        if (!m_pinned && m_fadeMode != EFadeMode.NoFadeClose && m_fadeMode != EFadeMode.NoFadeHide)
        {
            m_autoCloseTimer.Start();
        }
    }

    /// <summary>
    /// In case the form is already shown and Show is called. This will process as if the Shown event was fired.
    /// </summary>
    public new void Show()
    {
        if (!Visible)
        {
            base.Show();
        }

        FormShown();
    }

    protected new void Close()
    {
        FadeFormClose();
    }

    protected new void Hide()
    {
        FadeFormClose();
    }

    private void FadeFormClose()
    {
        switch (m_fadeMode)
        {
            case EFadeMode.Close:
            case EFadeMode.FadeToClose:
            case EFadeMode.NoFadeClose:
                m_autoCloseTimer.Stop();
                base.Close();
                break;
            case EFadeMode.Hide:
            case EFadeMode.FadeToHide:
            case EFadeMode.NoFadeHide:
                m_autoCloseTimer.Stop();
                base.Hide();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FormShown()
    {
        if (DesignMode)
        {
            return;
        }

        m_autoCloseTimer.Interval = c_tickInterval;
        m_decrementAmount = c_startOpacity / m_fadeDurationMs * m_autoCloseTimer.Interval;
        m_start = DateTime.Now;
        Opacity = c_startOpacity;
        m_internalOpacity = c_startOpacity;
        StartAutoHide();
    }

    private void AutoFadeForm_Move(object sender, EventArgs e)
    {
        StopAutoHide();
    }

    protected override void FormShown(object a_sender, EventArgs a_e)
    {
        FormShown();
    }
}