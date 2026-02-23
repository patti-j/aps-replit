using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services
{
    public class ShortcutService
    {
        public List<ShortcutSection> ShortcutSections;
        public event Action DevModeTurnedOn;
        public Task TurnOnDevMode()
        {
            DevModeTurnedOn?.Invoke();
            return Task.CompletedTask; // Return a completed task
        }

        public string GetKeyByAction(ToolbarAction toolbarAction)
        {
            foreach (var section in ShortcutSections)
            {
                var shortcut = section.Shortcuts.FirstOrDefault(s => s.Action == toolbarAction);
                if (shortcut != null)
                {
                    return shortcut.Key;
                }
            }
            return null; // Return null if no matching action is found
        }
        public List<ShortcutSection> GetShortcutSections()
        {
            ShortcutSections = new List<ShortcutSection>
            {
                new ShortcutSection
                {
                    Title = "Quick access",
                    Shortcuts = new List<Shortcut>
                    {
                        new Shortcut { Key = "Ctrl + Alt + K", Action = ToolbarAction.KeyboardShortcuts },
                        new Shortcut { Key = "Ctrl + Alt + A", Action = ToolbarAction.ActivityDisplayOptions },
                        new Shortcut { Key = "Ctrl + Alt + H", Action = ToolbarAction.HierarchyFilter },
                        new Shortcut { Key = "Ctrl + Alt + M", Action = ToolbarAction.ManageResources },
                        new Shortcut { Key = "Ctrl + Alt + F", Action = ToolbarAction.FullScreen },
                        new Shortcut { Key = "Ctrl + Alt + S", Action = ToolbarAction.SaveToFavorites },
                        new Shortcut { Key = "Ctrl + Alt + C", Action = ToolbarAction.AddCTPRequest },
                        new Shortcut { Key = "Ctrl + Alt + T", Action = ToolbarAction.CTPRequests },
                    }
                },
                new ShortcutSection
                {
                    Title = "Soft actions",
                    Shortcuts = new List<Shortcut>
                    {
                        new Shortcut { Key = "Ctrl + Alt + R", Action = ToolbarAction.ResizeRowsToFit },
                        new Shortcut { Key = "Ctrl + Alt + Plus", Action = ToolbarAction.ZoomIn },
                        new Shortcut { Key = "Ctrl + Alt + Minus", Action = ToolbarAction.ZoomOut },
                        new Shortcut { Key = "Ctrl + Alt + Up", Action = ToolbarAction.IncreaseBlockHeight },
                        new Shortcut { Key = "Ctrl + Alt + Down", Action = ToolbarAction.DecreaseBlockHeight },
                        new Shortcut { Key = "Ctrl + Alt + D", Action = ToolbarAction.DevModeOn },
                    }
                }
            };
            return ShortcutSections;
        }
    }
}
