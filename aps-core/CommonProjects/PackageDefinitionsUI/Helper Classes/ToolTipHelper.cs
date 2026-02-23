using DevExpress.Utils;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraEditors;

namespace PT.PackageDefinitionsUI.Helper_Classes;

public static class ToolTipHelper
{
    private static readonly ToolTipController s_toolTipController;

    static ToolTipHelper()
    {
        s_toolTipController = new ToolTipController();
        s_toolTipController.CloseOnClick = DefaultBoolean.False;
        s_toolTipController.InitialDelay = 1;
        s_toolTipController.AutoPopDelay = 30000;
    }

    public static void InitializeDefaultToolTipController(this GroupControl a_groupControl)
    {
        a_groupControl.ToolTipController = s_toolTipController;
        a_groupControl.CustomButtonClick += GroupControlOnCustomButtonClick;
    }

    private static void GroupControlOnCustomButtonClick(object a_sender, BaseButtonEventArgs a_e)
    {
        SuperToolTip tip = a_e.Button.Properties.SuperTip;

        ToolTipControlInfo info = new () { SuperTip = tip };

        s_toolTipController.HideHint();
        s_toolTipController.ShowHint(info);
    }
}