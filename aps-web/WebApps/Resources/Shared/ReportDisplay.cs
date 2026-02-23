
using Microsoft.JSInterop;

namespace ReportsWebApp.Resources.Shared
{
    public class ReportDisplay
    {
        public List<string> PageDisplayNames { get; set; }
        public List<string> PageInternalNames { get; set; }
        public string CurrentPage { get; set; }
        public int CurrentIndex {  get; set; }
        public string ReportId { get; set; }

        public event Action OnPageChanged;

        public ReportDisplay() {
            PageDisplayNames = new List<string>();
            PageInternalNames = new List<string>();
            CurrentPage = string.Empty;
        }

        [JSInvokable("SetPageNames")]
        public void SetPageNames(List<string> displayNames, List<string> internalNames)
        {
            PageInternalNames = internalNames;
            PageDisplayNames = displayNames;
            if (CurrentPage != string.Empty)
            {
                CurrentIndex = PageInternalNames.IndexOf(CurrentPage);
                OnPageChanged();
            }
        }

        [JSInvokable("SetCurrentPage")]
        public void SetCurrentPage(string name)
        {
            CurrentPage = name;
            if (PageInternalNames.Any())
            {
                CurrentIndex = PageInternalNames.IndexOf(CurrentPage);
                OnPageChanged();
            }
        }

        public void SetIndex(int index)
        {
            if (CurrentIndex != index)
            {
                CurrentIndex = index;
                CurrentPage = PageInternalNames[index];
                OnPageChanged();
            }
        }
    }
}
