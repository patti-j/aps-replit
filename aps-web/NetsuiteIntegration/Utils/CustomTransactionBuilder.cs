using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NetsuiteIntegration.ResponseObjects;

namespace NetsuiteIntegration.Utils
{
    public class CustomTransactionBuilder
    {        
        private static string FmtIso(DateTime dt) =>
            dt.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        
        public async Task<List<WoUpdate>> BuildWoEndOnlyAsync(List<UpdatedJobs> a_jobs)
        {
            List<WoUpdate> list = a_jobs
                .GroupBy(j => j.PT_WorkOrderID)
                .Select(g =>
                {
                    UpdatedJobs row = g.OrderByDescending(x => x.PT_WorkOrderEndDate).First();
                    return new WoUpdate
                    {
                        internalId = row.PT_WorkOrderID,
                        jobEnd = FmtIso(row.PT_WorkOrderEndDate),
                        tranId = row.PT_WorkOrderName
                    };
                })
                .ToList();

            return await Task.FromResult(list);
        }

        
        public class WoUpdate
        {
            public int internalId { get; set; }
            public string jobEnd { get; set; }   
            public string tranId { get; set; }
        }
    }
}
