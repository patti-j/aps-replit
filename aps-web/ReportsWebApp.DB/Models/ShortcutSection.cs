using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReportsWebApp.DB.Models
{
    public class ShortcutSection
    {
        public string Title { get; set; }
        public List<Shortcut> Shortcuts { get; set; }
    }

    public class Shortcut
    {
        public string Key { get; set; }
        public ToolbarAction Action { get; set; }
    }

    public enum ToolbarAction
    {
        [DisplayText("N/A"), Show(false), Icon("")]
        Zero,

        [DisplayText("Full Screen"), Show(true), Icon("fas fa-arrows-alt")]
        FullScreen,

        [DisplayText("Resize Rows to Fit"), Show(true), Icon("fas fa-list-ul")]
        ResizeRowsToFit,

        [DisplayText("Zoom In"), Show(true), Icon("fas fa-search-plus")]
        ZoomIn,

        [DisplayText("Zoom Out"), Show(true), Icon("fas fa-search-minus")]
        ZoomOut,

        [DisplayText("Increase Block Height"), Show(true), Icon("fas fa-arrow-up")]
        IncreaseBlockHeight,

        [DisplayText("Decrease Block Height"), Show(true), Icon("fas fa-arrow-down")]
        DecreaseBlockHeight,

        [DisplayText("Activity Display Options"), Show(true), Icon("fas fa-cog")]
        ActivityDisplayOptions,

        [DisplayText("Scenario Filter"), Show(true), Icon("fas fa-filter")]
        HierarchyFilter,

        [DisplayText("Manage Resources"), Show(true), Icon("fas fa-tasks")]
        ManageResources,

        [DisplayText("Save to Favorites"), Show(true), Icon("fas fa-star")]
        SaveToFavorites,

        [DisplayText("Add a CTP Request"), Show(false), Icon("fa-solid fa-calendar-circle-plus")]
        AddCTPRequest,

        [DisplayText("Manage CTP Requests"), Show(false), Icon("fa-solid fa-calendar-clock")]
        CTPRequests,

        [DisplayText("Keyboard Shortcuts"), Show(true), Icon("fa-regular fa-keyboard")]
        KeyboardShortcuts,

        [DisplayText("Dev Mode On"), Show(false), Icon("")]
        DevModeOn
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayTextAttribute : Attribute
    {
        public string Text { get; }

        public DisplayTextAttribute(string text)
        {
            Text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ShowAttribute : Attribute
    {
        public bool Value { get; }

        public ShowAttribute(bool value)
        {
            Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class IconAttribute : Attribute
    {
        public string Value { get; }

        public IconAttribute(string value)
        {
            Value = value;
        }
    }

    public static class ToolbarActionExtensions
    {
        public static string GetDisplayText(this ToolbarAction action)
        {
            return action.GetAttribute<DisplayTextAttribute>()?.Text ?? "Unknown";
        }

        public static bool GetShowValue(this ToolbarAction action)
        {
            return action.GetAttribute<ShowAttribute>()?.Value ?? false;
        }

        public static string GetIconValue(this ToolbarAction action)
        {
            return action.GetAttribute<IconAttribute>()?.Value ?? string.Empty;
        }

        private static T GetAttribute<T>(this ToolbarAction action) where T : Attribute
        {
            return action.GetType()
                         .GetField(action.ToString())
                         .GetCustomAttribute<T>();
        }
    }
}
