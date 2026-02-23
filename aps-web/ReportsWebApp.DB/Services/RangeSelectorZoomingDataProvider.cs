using System;
using System.Collections.Generic;
using System.Linq;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services
{
    /// <summary>
    /// Provides time-based zooming data derived from SchedulerDiskImage content, grouped monthly.
    /// Inserts a zeroed ZoomingData at the start of each month to reset visible metrics.
    /// </summary>
    public class RangeSelectorZoomingDataProvider
    {
        private IGanttService _ganttService;
        
        public RangeSelectorZoomingDataProvider(IGanttService ganttService)
        {
            _ganttService = ganttService;
        }

        /// <summary>
        /// Extracts zoomable time-based metrics from Events, creating visible spikes for each month and inserting monthly reset points.
        /// </summary>
        /// <returns>List of ZoomingData representing zoom metrics</returns>
        public List<ZoomingData> GetData()
        {
            if (_ganttService?.ActiveProject == null)
            {
                return new List<ZoomingData>();
            }
            var events = _ganttService.ActiveProject.Events
                                      .Where(e => e.StartDate < e.EndDate)
                                      .OrderBy(e => e.StartDate)
                                      .ToList();

            if (!events.Any())
                return new List<ZoomingData>();

            var result = new List<ZoomingData>();
            var grouped = events.GroupBy(e => new { e.StartDate.Year, e.StartDate.Month });

            foreach (var group in grouped)
            {
                var monthStart = new DateTime(group.Key.Year, group.Key.Month, 1);

                // Insert reset point at beginning of the month
                result.Add(new ZoomingData
                {
                    Arg = monthStart,
                    Y1 = 0,
                    Y2 = 0,
                    Y3 = 0
                });

                int index = 0;
                foreach (var e in group.OrderBy(e => e.StartDate))
                {
                    result.Add(new ZoomingData
                    {
                        Arg = e.StartDate,
                        Y1 = (e.EndDate - e.StartDate).TotalHours,
                        Y2 = index++,
                        Y3 = e.BlockLocked ? 1 : 0
                    });
                }
            }

            return result;
        }
    }
}
