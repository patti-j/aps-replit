using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraSplashScreen;

namespace PT.UIDefinitions.Splash;

public class PTSplashScreenManager : SplashScreenManager
{
    public PTSplashScreenManager() { }

    public PTSplashScreenManager(Form a_parentForm, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut)
        : base(a_parentForm, a_splashFormType, a_useFadeIn, a_useFadeOut) { }

    public PTSplashScreenManager(Form a_parentForm, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, int a_closingDelay)
        : base(a_parentForm, a_splashFormType, a_useFadeIn, a_useFadeOut, a_closingDelay) { }

    public PTSplashScreenManager(Form a_parentForm, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, bool a_allowGlowEffect)
        : base(a_parentForm, a_splashFormType, a_useFadeIn, a_useFadeOut, a_allowGlowEffect) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, ParentType a_type)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_type) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, Type a_type)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_type) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, ParentType a_type, bool allowGlowEffect)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_type, allowGlowEffect) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, Type a_type, bool allowGlowEffect)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_type, allowGlowEffect) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, SplashFormStartPosition a_startPos, Point a_loc, ParentType a_type)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_startPos, a_loc, a_type) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, SplashFormStartPosition a_startPos, Point a_loc, Type a_type)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_startPos, a_loc, a_type) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, SplashFormStartPosition a_startPos, Point a_loc, ParentType a_type, bool a_allowGlowEffect)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_startPos, a_loc, a_type, a_allowGlowEffect) { }

    public PTSplashScreenManager(UserControl parentControl, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, SplashFormStartPosition a_startPos, Point a_loc, Type a_type, bool a_allowGlowEffect)
        : base(parentControl, a_splashFormType, a_useFadeIn, a_useFadeOut, a_startPos, a_loc, a_type, a_allowGlowEffect) { }

    public PTSplashScreenManager(Form a_parentForm, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, SplashFormStartPosition a_startPos, Point a_loc)
        : base(a_parentForm, a_splashFormType, a_useFadeIn, a_useFadeOut, a_startPos, a_loc) { }

    public PTSplashScreenManager(Form a_parentForm, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, SplashFormStartPosition a_startPos, Point a_loc, bool a_allowGlowEffect)
        : base(a_parentForm, a_splashFormType, a_useFadeIn, a_useFadeOut, a_startPos, a_loc, a_allowGlowEffect) { }

    public PTSplashScreenManager(Form a_parentForm, Type a_splashFormType, bool a_useFadeIn, bool a_useFadeOut, SplashFormStartPosition a_startPos, Point a_loc, int a_pendingTime)
        : base(a_parentForm, a_splashFormType, a_useFadeIn, a_useFadeOut, a_startPos, a_loc, a_pendingTime) { }

    public PTSplashScreenManager(Type a_splashFormType, SplashFormProperties a_info)
        : base(a_splashFormType, a_info) { }

    public PTSplashScreenManager(Type a_splashFormType, SplashFormStartPosition a_startPos, Point a_loc, SplashFormProperties a_info)
        : base(a_splashFormType, a_startPos, a_loc, a_info) { }

    public PTSplashScreenManager(Type a_splashFormType, SplashFormStartPosition a_startPos, Point a_loc, SplashFormProperties a_info, ParentFormState a_parentFormDesiredState)
        : base(a_splashFormType, a_startPos, a_loc, a_info, a_parentFormDesiredState) { }

    private class InnerSplashScreenManager : SplashScreenManager
    {
        public InnerSplashScreenManager(Type a_type, SplashFormStartPosition a_startPos, Point a_loc, SplashFormProperties a_stateInfo)
            : base(a_type, a_startPos, a_loc, a_stateInfo) { }

        protected override bool IsInnerManager => true;
    }

    private static Image s_image;
    private static bool s_useFadeOut;
    private static ICustomImagePainter s_painter;

    public new static void ShowImage(Image a_image, bool a_useFadeIn, bool a_useFadeOut, ICustomImagePainter a_painter)
    {
        if (Default != null)
        {
            throw new InvalidOperationException("Image has already been displayed");
        }

        s_image = a_image;
        s_useFadeOut = a_useFadeOut; //Store, but don't fade out on the original image because it moves position once the UI is shown
        s_painter = a_painter;
        SplashFormProperties stateInfo = new (null, a_image, a_useFadeIn, false, a_painter, 0);
        Default = new InnerSplashScreenManager(typeof(PTSplashScreenLayer), SplashFormStartPosition.Default, Point.Empty, stateInfo);
    }

    /// <summary>
    /// Closes and reshows the splash screen to bring it to the front without making it topmost
    /// </summary>
    public static void BringToFront()
    {
        if (Default != null)
        {
            CloseForm(true, 0, null, true);
            ShowImage(s_image, false, s_useFadeOut, s_painter);
        }
    }

    /// <summary>
    /// Make sure to close the splash this way instead of SplashScreenManager.CloseForm
    /// This disposes of cached resources
    /// </summary>
    public static void CloseSplash()
    {
        CloseForm();
        s_image?.Dispose();
    }
}

public class PTSplashScreenLayer : SplashScreenLayer
{
    public PTSplashScreenLayer(Image a_image, bool a_useUserLocation)
        : base(a_image, a_useUserLocation) { }

    protected override void OnShown(EventArgs a_e)
    {
        base.OnShown(a_e);
        TopMost = false;
    }
}