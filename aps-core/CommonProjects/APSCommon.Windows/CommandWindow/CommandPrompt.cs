using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PT.APSCommon.Windows.CommandWindow;

public partial class CommandPrompt : TextBox
{
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case 0x000C: // WM_SETTEXT
            case 0x0300: // WM_CUT
            case 0x0302: // WM_PASTE
                if (BeforeNewLinePrompt())
                {
                    MovePromptToEndOfLine();
                }

                break;

            case 0x0303: // WM_CLEAR
                return;
        }

        base.WndProc(ref m);
    }

    public delegate void CommandEnteredEvent(string a_command);

    public event CommandEnteredEvent m_commandEvent;

    private void FireCommandEnteredEvent(string a_command)
    {
        if (m_commandEvent != null)
        {
            m_commandEvent(a_command);
            m_enteredCommandsList.Add(a_command);
            m_nextCommand = m_enteredCommandsList.Count - 1;
        }
    }

    private readonly List<string> m_enteredCommandsList = new ();
    private int m_nextCommand;

    private bool GetCommand(bool a_prev, out string a_s)
    {
        if (m_nextCommand >= 0 && m_nextCommand < m_enteredCommandsList.Count)
        {
            a_s = m_enteredCommandsList[m_nextCommand];
            if (a_prev)
            {
                --m_nextCommand;
                if (m_nextCommand < 0)
                {
                    m_nextCommand = 0;
                }
            }
            else
            {
                ++m_nextCommand;
                if (m_nextCommand > m_enteredCommandsList.Count - 1)
                {
                    m_nextCommand = m_enteredCommandsList.Count - 1;
                }
            }

            return true;
        }

        a_s = null;
        return false;
    }

    public CommandPrompt()
    {
        InitializeComponent();
        WritePrompt();
    }

    public CommandPrompt(IContainer container)
    {
        container.Add(this);
        InitializeComponent();
    }

    private const string PROMPT = ">";

    public void WritePrompt()
    {
        if (!AtStartOfNewEmptyLine())
        {
            Text += Environment.NewLine;
        }

        Text += PROMPT;
        MovePromptToEndOfLine();
    }

    private void MovePromptToEndOfLine()
    {
        SelectionStart = Text.Length;
        ScrollToCaret();
    }

    public void Write(string a_text)
    {
        Text += a_text;
    }

    /// <summary>
    /// If restartLine, overwrites last line with message text
    /// </summary>
    /// <param name="a_messageText"></param>
    /// <param name="a_restartLine"></param>
    public void Write(string a_messageText, bool a_restartLine)
    {
        if (a_restartLine)
        {
            string s = Text.Substring(Text.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            if (s != "\n>")
            {
                int index = Text.LastIndexOf('>');
                Text = Text.Substring(0, index + 1);
            }
        }

        Text += a_messageText;
    }

    private bool AtStartOfNewEmptyLine()
    {
        if (Text.Length > Environment.NewLine.Length)
        {
            string s = Text.Substring(Text.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            return s == Environment.NewLine;
        }

        return Text.Length == 0;
    }

    private bool BeforeNewLinePrompt()
    {
        if (Lines.Length > 0)
        {
            string s = Lines[Lines.Length - 1];
            int len = s.Length;
            return SelectionStart < Text.Length - len + PROMPT.Length;
        }

        return false;
    }

    private bool AtNewLinePrompt()
    {
        if (Lines.Length > 0)
        {
            string s = Lines[Lines.Length - 1];
            int len = s.Length;
            return SelectionStart <= Text.Length - len + PROMPT.Length;
        }

        return false;
    }

    private bool m_clearExcessEnters;

    private void CommandPrompt_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            string s;
            if (GetCommand(true, out s))
            {
                ReplaceCommand(s);
            }

            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Down)
        {
            string s;
            if (GetCommand(false, out s))
            {
                ReplaceCommand(s);
            }

            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Left)
        {
            if (AtNewLinePrompt())
            {
                e.Handled = true;
            }
            else if (BeforeNewLinePrompt())
            {
                MovePromptToEndOfLine();
                e.Handled = true;
            }
        }
        else if (e.KeyCode == Keys.Right)
        {
            if (BeforeNewLinePrompt())
            {
                MovePromptToEndOfLine();
                e.Handled = true;
            }
        }
        else if (e.KeyCode == Keys.Home)
        {
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Back)
        {
            if (AtNewLinePrompt() || BeforeNewLinePrompt())
            {
                e.Handled = true;
            }
        }
        else
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                MovePromptToEndOfLine();
                m_clearExcessEnters = true;
            }

            if (BeforeNewLinePrompt())
            {
                if (!e.Control)
                {
                    MovePromptToEndOfLine();
                }
            }
        }
    }

    private void CommandPrompt_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == 13)
        {
            ExecuteCommand();
            e.Handled = true;
        }
        else if (e.KeyChar == 8)
        {
            if (AtNewLinePrompt())
            {
                e.Handled = true;
            }
            else if (BeforeNewLinePrompt())
            {
                MovePromptToEndOfLine();
                e.Handled = true;
            }
        }
    }

    public void ReplaceAndExecuteCommand(string a_command)
    {
        ReplaceCommand(a_command);
        ExecuteCommand();
    }

    private void ExecuteCommand()
    {
        string command = GetCommand();
        Write(Environment.NewLine);
        FireCommandEnteredEvent(command);
        WritePrompt();
    }

    private string GetCommand()
    {
        if (Lines.Length > 0)
        {
            string s = Lines[Lines.Length - 1];
            if (s != null && s.Length > 0)
            {
                return s.Substring(1, s.Length - 1);
            }

            return null;
        }

        return null;
    }

    private void ReplaceCommand(string a_s)
    {
        string command = GetCommand();
        if (command != null)
        {
            string sub = Text.Substring(0, Text.Length - command.Length);
            string newText = sub + a_s;
            Text = newText;
            MovePromptToEndOfLine();
        }
    }

    private void CommandPrompt_TextChanged(object sender, EventArgs e)
    {
        if (m_clearExcessEnters)
        {
            m_clearExcessEnters = false;
            string s = Text;
            if (s.EndsWith(Environment.NewLine + PROMPT + Environment.NewLine))
            {
                Text = s.Substring(0, Text.Length - Environment.NewLine.Length);
                MovePromptToEndOfLine();
            }
        }

        // Limit the number of lines shown.
        // Once a certain number of characters are contained within the textbox it will no longer accept input.
        // I don't know the exact limit and simply picked this as the limit.
        // Tried 500, but it didn't work.
        const int MaxLines = 100;
        string[] lines = Lines;
        string[] linesMax = new string[MaxLines];

        if (lines.Length > MaxLines)
        {
            int linesMaxIdx = MaxLines - 1;
            int linesIdx = lines.Length - 1;
            while (linesMaxIdx >= 0)
            {
                linesMax[linesMaxIdx] = lines[linesIdx];
                --linesMaxIdx;
                --linesIdx;
            }

            Lines = linesMax;
        }
    }

    // Code below taken from Internet.
    // Attempted to use the code to prevent flashing
    // and typing lock up in the control (when it contains too much text).

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

    private const int WM_SETREDRAW = 11;

    private int suspendCounter;

    /// <summary>
    /// Suspend drawing the control.
    /// </summary>
    private void SuspendDrawing()
    {
        if (suspendCounter == 0)
        {
            SendMessage(Handle, WM_SETREDRAW, false, 0);
        }

        suspendCounter++;
    }

    /// <summary>
    /// Resume redrawing the control. Redraw will resume only after as many ResumeRedrawing calls matches the number of SuspenDrawings.
    /// </summary>
    private void ResumeDrawing()
    {
        suspendCounter--;
        if (suspendCounter == 0)
        {
            SendMessage(Handle, WM_SETREDRAW, true, 0);
            Refresh();
        }
    }
}