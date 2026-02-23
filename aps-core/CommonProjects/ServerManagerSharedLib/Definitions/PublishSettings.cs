using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class PublishSettings
    {
        // TODO: port summaries from Core's ScenarioPublishOptions.cs
        #region Publish Destinations

        public bool PublishToSQL { get; set; } = false;
        public bool PublishToCustomDll { get; set; } = false;
        public bool PublishToXML { get; set; } = false;
        public bool PublishAllActivitiesForMO { get; set; } = false;

        #endregion

        #region Publish Procedures

        public bool RunStoredProcedureAfterPublish { get; set; } = false;
        public string PostPublishStoredProcedureName { get; set; } = "APS_Publish";
        public bool RunProgramAfterPublish { get; set; } = false;
        public string RunProgramPath { get; set; } = "";
        public string RunProgramCommandLine { get; set; } = "";
        public bool PublishInLocalTime { get; set; } = false;

        #endregion

        #region Net-Change Publishing

        public bool NetChangePublishingEnabled { get; set; } = false;
        public bool RunStoredProcedureAfterNetChangePublish { get; set; } = false;
        public string NetChangeStoredProcedureName { get; set; } = "APS_NetChangePublish";

        #endregion

        #region Publish Data Limits

        public bool PublishInventory { get; set; } = true;
        public bool PublishJobs { get; set; } = true;
        public bool PublishManufacturingOrders { get; set; } = true;
        public bool PublishOperations { get; set; } = true;
        public bool PublishActivities { get; set; } = true;
        public bool PublishBlocks { get; set; } = true;
        public bool PublishBlockIntervals { get; set; } = true;
        public bool PublishProductRules { get; set; } = false;
        public bool PublishCapacityIntervals { get; set; } = false;
        public bool PublishTemplates { get; set; } = false;

        #endregion

        public PublishSettings()
        {

        }

        public PublishSettings ShallowCopy()
        {
            return this.MemberwiseClone() as PublishSettings;
        }
    }
}
