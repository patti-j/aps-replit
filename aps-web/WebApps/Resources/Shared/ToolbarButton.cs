using ReportsWebApp.DB.Models;

namespace ReportsWebApp.Resources.Shared
{
    public class ToolbarButton
    {
        public ToolbarAction ToolbarAction { get; private set; }
        public Func<Task> Action { get; private set; }

        public ToolbarButton(ToolbarAction toolbarAction, Func<Task> action)
        {
            ToolbarAction = toolbarAction;
            Action = action;
        }
    }

}
