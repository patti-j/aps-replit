//using System.Windows.Forms;

//using DevExpress.Data;
//using DevExpress.Utils;

//using PT.APIDefinitions.RequestsAndResponses.ActivityStatus;
//using PT.UIDefinitions;

//namespace ShopViewsControls.ActivityStatus
//{
//    public partial class ResourceInfoControl : UserControl
//    {
//        public ResourceInfoControl()
//        {
//            InitializeComponent();

//            this.offlinesGrid.SetDataBinding(_resourceInfoDataSet.CleanoutAndOfflineIntervals, "");
//            this.offlinesGrid.GridView.OptionsBehavior.AllowAddRows = DefaultBoolean.False;
//            this.offlinesGrid.GridView.OptionsBehavior.AllowDeleteRows = DefaultBoolean.False;
//            this.offlinesGrid.GridView.OptionsBehavior.Editable = false;
//            this.offlinesGrid.GridView.Columns[_resourceInfoDataSet.CleanoutAndOfflineIntervals.StartColumn.ColumnName].DisplayFormat.FormatString = PTAppearance.DateTimeFormat;
//            this.offlinesGrid.GridView.Columns[_resourceInfoDataSet.CleanoutAndOfflineIntervals.EndColumn.ColumnName].DisplayFormat.FormatString = PTAppearance.DateTimeFormat;
//        }

//        internal void Clear()
//        {
//            _resourceInfoDataSet.CleanoutAndOfflineIntervals.Clear();
//        }

//        internal void Populate(ViewerResourceInfo resourceInfo)
//        {
//            _resourceInfoDataSet.Clear();

//            for (int i = 0; i < resourceInfo.CapacityIntervalInfoCount; i++)
//            {
//                CapacityIntervalInfo capInfo = resourceInfo.GetCapacityIntervalInfo(i);
//                _resourceInfoDataSet.CleanoutAndOfflineIntervals.AddCleanoutAndOfflineIntervalsRow(
//                    PT.APSCommon.Localization.Localizer.GetString(capInfo.IntervalType.ToString()),
//                    capInfo.Name,
//                    capInfo.Description,
//                    PT.Common.TimeZoneAdjuster.GetDisplayTime(capInfo.Start),
//                    PT.Common.TimeZoneAdjuster.GetDisplayTime(capInfo.End),
//                    capInfo.DurationHrs,
//                    PT.APSCommon.Localization.Localizer.GetString(capInfo.Recurrence.ToString()),
//                    capInfo.Notes
//                    );
//            }

//            //Sort by ascending Start DateTime
//            offlinesGrid.GridView.Columns[_resourceInfoDataSet.CleanoutAndOfflineIntervals.StartColumn.ColumnName].SortOrder = ColumnSortOrder.Ascending;

//            offlinesGrid.GridView.BestFitColumns(true);
//        }

//        private readonly ResourceInfoDataSet _resourceInfoDataSet = new ResourceInfoDataSet();
//    }
//}

