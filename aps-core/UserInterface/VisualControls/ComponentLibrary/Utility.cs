using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;

namespace PT.ComponentLibrary;

/// <summary>
/// Contains various static methods needed by different UI classes.
/// </summary>
public class Utility
{
    #region Declaration
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern short GetSystemMetrics(int vKey);

    private const int HIT = short.MinValue + 1;

    //Here's the list of keys: http://msdn2.microsoft.com/en-us/library/ms645540(VS.85).aspx

    private const int VK_ESCAPE = 0xA;

    public static bool EscapeKeyPressed()
    {
        int keyState = GetAsyncKeyState(VK_ESCAPE);
        return keyState == HIT;
    }

    public static bool ControlKeyPressed()
    {
        bool keyDown = Control.ModifierKeys.HasFlag(Keys.Control);
        return keyDown;
    }

    public static bool ShiftKeyPressed()
    {
        bool keyDown = Control.ModifierKeys.HasFlag(Keys.Shift);
        return keyDown;
    }

    public static bool AltKeyPressed()
    {
        bool keyDown = Control.ModifierKeys.HasFlag(Keys.Alt);
        return keyDown;
    }

    public static bool KeyPressed(char c)
    {
        int keyState = GetAsyncKeyState(c);
        return keyState == HIT;
    }

    public static bool KeyPressed(EKeyBoardSpecialKeys a_specialKey)
    {
        if (a_specialKey == EKeyBoardSpecialKeys.LeftArrowKeyDown)
        {
            bool keyPressed = GetAsyncKeyState(c_leftArrowButton) == HIT;
            return keyPressed;
        }

        if (a_specialKey == EKeyBoardSpecialKeys.RightArrowKeyDown)
        {
            bool keyPressed = GetAsyncKeyState(c_rightArrowButton) == HIT;
            return keyPressed;
        }

        return false;
    }

    [Flags]
    public enum EKeyBoardSpecialKeys
    {
        None = 0,
        AltKeyDown = 1,
        CtrlKeyDown = 2,
        ShiftKeyDown = 4,
        LeftArrowKeyDown = 8,
        RightArrowKeyDown = 16
    }

    public enum EKeyPressOptions { MoveTrailingSplitsFromSameResourceToSameResource, DragDropBetweenGantts, ExpediteDragDrop }

    public static bool KeyPressed(EKeyPressOptions a_option)
    {
        if (a_option == EKeyPressOptions.MoveTrailingSplitsFromSameResourceToSameResource)
        {
            return KeyPressed('G');
        }

        if (a_option == EKeyPressOptions.DragDropBetweenGantts)
        {
            return KeyPressed('B') || KeyPressed('b');
        }

        if (a_option == EKeyPressOptions.ExpediteDragDrop)
        {
            return KeyPressed('W') || KeyPressed('w');
        }

        return false;
    }

    public static bool KeyboardSpecialKeysPressed(EKeyBoardSpecialKeys a_state)
    {
        if (!KeyboardExclusiveStateHelper((int)a_state, EKeyBoardSpecialKeys.AltKeyDown, AltKeyPressed()))
        {
            return false;
        }

        if (!KeyboardExclusiveStateHelper((int)a_state, EKeyBoardSpecialKeys.CtrlKeyDown, ControlKeyPressed()))
        {
            return false;
        }

        if (!KeyboardExclusiveStateHelper((int)a_state, EKeyBoardSpecialKeys.ShiftKeyDown, ShiftKeyPressed()))
        {
            return false;
        }

        return true;
    }

    private static bool KeyboardExclusiveStateHelper(int a_state, EKeyBoardSpecialKeys a_keyValue, bool a_isKeyDepressed)
    {
        bool wantKeyDepressed = (a_state & (int)a_keyValue) > 0;

        if (wantKeyDepressed)
        {
            if (!a_isKeyDepressed)
            {
                return false;
            }
        }
        else
        {
            if (a_isKeyDepressed)
            {
                return false;
            }
        }

        return true;
    }

    #region VK values FOR REFERENCE
    //VK_LBUTTON (01)
    //Left mouse button

    //VK_RBUTTON (02)
    //Right mouse button

    //VK_CANCEL (03)
    //Control-break processing

    //VK_MBUTTON (04)
    //Middle mouse button (three-button mouse)

    //VK_XBUTTON1 (05)
    //Windows 2000/XP: X1 mouse button

    //VK_XBUTTON2 (06)
    //Windows 2000/XP: X2 mouse button

    //- (07)
    //Undefined

    //VK_BACK (08)
    //BACKSPACE key

    //VK_TAB (09)
    //TAB key

    //- (0A-0B)
    //Reserved

    //VK_CLEAR (0C)
    //CLEAR key

    //VK_RETURN (0D)
    //ENTER key

    //- (0E-0F)
    //Undefined

    //VK_SHIFT (10)
    //SHIFT key

    //VK_CONTROL (11)
    //CTRL key

    //VK_MENU (12)
    //ALT key

    //VK_PAUSE (13)
    //PAUSE key

    //VK_CAPITAL (14)
    //CAPS LOCK key

    //VK_KANA (15)
    //Input Method Editor (IME) Kana mode

    //VK_HANGUEL (15)
    //IME Hanguel mode (maintained for compatibility; use VK_HANGUL)

    //VK_HANGUL (15)
    //IME Hangul mode

    //- (16)
    //Undefined

    //VK_JUNJA (17)
    //IME Junja mode

    //VK_FINAL (18)
    //IME final mode

    //VK_HANJA (19)
    //IME Hanja mode

    //VK_KANJI (19)
    //IME Kanji mode

    //- (1A)
    //Undefined

    //VK_ESCAPE (1B)
    //ESC key

    //VK_CONVERT (1C)
    //IME convert

    //VK_NONCONVERT (1D)
    //IME nonconvert

    //VK_ACCEPT (1E)
    //IME accept

    //VK_MODECHANGE (1F)
    //IME mode change request

    //VK_SPACE (20)
    //SPACEBAR

    //VK_PRIOR (21)
    //PAGE UP key

    //VK_NEXT (22)
    //PAGE DOWN key

    //VK_END (23)
    //END key

    //VK_HOME (24)
    //HOME key

    //VK_LEFT (25)
    //LEFT ARROW key

    //VK_UP (26)
    //UP ARROW key

    //VK_RIGHT (27)
    //RIGHT ARROW key

    //VK_DOWN (28)
    //DOWN ARROW key

    //VK_SELECT (29)
    //SELECT key

    //VK_PRINT (2A)
    //PRINT key

    //VK_EXECUTE (2B)
    //EXECUTE key

    //VK_SNAPSHOT (2C)
    //PRINT SCREEN key

    //VK_INSERT (2D)
    //INS key

    //VK_DELETE (2E)
    //DEL key

    //VK_HELP (2F)
    //HELP key
    #endregion

    private const int c_mouseHit = short.MinValue;
    private const int c_vkLbutton = 0x1;
    private const int c_vkRbutton = 0x2;
    private const int c_smSwapbutton = 23;
    private const int c_leftArrowButton = 37;
    private const int c_rightArrowButton = 39;

    public static bool LeftMouseButtonPressed()
    {
        int keyState;
        bool mouseButtonsSwapped = GetSystemMetrics(c_smSwapbutton) != 0; //if Windows has swapped use of left and right buttons
        if (!mouseButtonsSwapped)
        {
            keyState = GetAsyncKeyState(c_vkLbutton); //This returns whether the PHYSICAL left mouse button is pressed
        }
        else
        {
            keyState = GetAsyncKeyState(c_vkRbutton); //This returns whether the PHYSICAL right mouse button is pressed
        }

        return keyState == c_mouseHit;
    }

    public static bool RightMouseButtonPressed()
    {
        int keyState;
        bool mouseButtonsSwapped = GetSystemMetrics(c_smSwapbutton) != 0; //if Windows has swapped use of left and right buttons
        if (mouseButtonsSwapped)
        {
            keyState = GetAsyncKeyState(c_vkLbutton); //This returns whether the PHYSICAL left mouse button is pressed
        }
        else
        {
            keyState = GetAsyncKeyState(c_vkRbutton); //This returns whether the PHYSICAL right mouse button is pressed
        }

        return keyState == c_mouseHit;
    }
    #endregion

    /// <summary>
    /// Get the Reflection Attributes Collection for the specified property of the specified object.
    /// </summary>
    /// <returns></returns>
    public static AttributeCollection GetObjectAttributes(Type a_type, string a_propertyName)
    {
        PropertyDescriptor pd = TypeDescriptor.GetProperties(a_type)[a_propertyName];
        return pd?.Attributes;
    }

    /// <summary>
    /// Make text boxes readonly and other objects disabled.
    /// </summary>
    /// <param name="a_control"></param>
    public static void DisableChildControls(Control a_control)
    {
        if (a_control is XtraTabControl)
        {
            XtraTabControl tabControl = (XtraTabControl)a_control;
            //Disable the tab pages
            for (int pageI = 0; pageI < tabControl.TabPages.Count; pageI++)
            {
                XtraTabPage page = tabControl.TabPages[pageI];
                foreach (Control control in page.Controls)
                {
                    DisableChildControls(control);
                }
            }
        }
        else if (a_control is TabPane)
        {
            TabPane tabControl = (TabPane)a_control;
            //Disable the tab pages
            for (int pageI = 0; pageI < tabControl.Pages.Count; pageI++)
            {
                NavigationPageBase page = tabControl.Pages[pageI];
                DisableChildControls(page);
            }
        }
        else
        {
            for (int i = 0; i < a_control.Controls.Count; i++)
            {
                Control childControl = a_control.Controls[i];
                if (childControl is TextBoxBase)
                {
                    ((TextBoxBase)childControl).ReadOnly = true;
                }
                else if (childControl is BaseEdit)
                {
                    ((BaseEdit)childControl).ReadOnly = true;
                }
                else if (childControl is Label || childControl is Button)
                {
                    //leave alone
                }
                //else if (childControl is PT..DatePickerControl)
                //{
                //    ((PT.ComponentLibrary.Controls.DatePickerControl())childControl).ReadOnly = true;
                //}
                else if (childControl is Panel)
                {
                    DisableChildControls(childControl);
                }
                else if (childControl is PanelControl)
                {
                    DisableChildControls(childControl);
                }
                else if (childControl is GroupBox)
                {
                    DisableChildControls(childControl);
                }
                else
                {
                    childControl.Enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Make text boxes readonly and other objects disabled for each page in the TabControl.
    /// </summary>
    public static void DisableTabControlChildControls(TabControl a_tabControl)
    {
        for (int i = 0; i < a_tabControl.TabCount; i++)
        {
            DisableChildControls(a_tabControl.TabPages[i]);
        }
    }
}