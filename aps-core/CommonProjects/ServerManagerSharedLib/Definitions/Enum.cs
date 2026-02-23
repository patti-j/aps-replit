using System;

namespace PT.ServerManagerSharedLib.Definitions
{
    public enum ESchedulingType
    {
        /// <summary>
        /// Optimize at the of CTP
        /// </summary>
        Optimize,

        /// <summary>
        /// Expedite the new Job to Clock (do ASAP)
        /// </summary>
        ExpediteToClock,

        /// <summary>
        /// Expedite the new Job to it's JIT Start Date.
        /// </summary>
        ExpediteToJIT
    }
    //Set by the Client. Used by the server to determine which user validation
    // should be performed
    public enum LoginType
    {
        User,
        JWT
    }

    public enum EStartType
    {
        NotSet = 0,
        //
        // Summary:
        //     Load the scenario file in the scenario folder. If the scenario file doesn't
        //     exist then a new blank one is created.
        Normal = 1,
        //
        // Summary:
        //     Create a new system at startup.
        Fresh = 2,
        //
        // Summary:
        //     Loads the scenario and play the transmissions in a specified folder.
        Recording = 4,
        //
        // Summary:
        //     This gives clients the opportunity to start before the recording begins.
        //     The service starts but playback is delayed until the BeginReplay message
        //     is sent by from the sample data generator.
        RecordingClientDelayed = 5,
        //
        // Summary:
        //     Run a recording. After each optimization has completed store the schedule
        //     and inventory allocations in a file in the unit test base folder.  The ToString()
        //     value of this enumeration element is used as the directory name for this
        //     type of unit test.
        UnitTestBase = 6,
        //
        // Summary:
        //     Run a recording. After each optimization has completed store the schedule
        //     and inventory allocations in a file in the unit test folder. You can compare
        //     these files against the files in the unit test base folder to determine whether
        //     any differences exist (suggesting a bug).  The ToString() value of this enumeration
        //     element is used as the directory name for this type of unit test.
        UnitTest = 7,
        //
        // Summary:
        //     Attempt to reproduce an exception in scenario detail. If an exception does occur, attempt to remove data that is 
        //     not related to the error to simplify the scenario.
        Prune = 8,
        // Summary:
        //     This is used to run a scenario that has been saved in the scenario folder.
        Scenario = 9,
    }

    public enum ScenarioAccessLevels
    {
        /// <summary>
        /// Can only view the Published Scenario only (if there is one).  No changes are allowed.
        /// </summary>
        ViewPublished = 0,
        /// <summary>
        /// Can view all Scenarios but can make no changes.
        /// </summary>
        ViewAll,
        /// <summary>
        /// Can view all Scenarios and can make changes to and create What-If Scenarios.  No changes can be made to the Published or Live Scenarios.
        /// </summary>
        ViewAllEditWhatifs,
        /// <summary>
        /// Can view and edit all Scenarios including the Live, Published, and What-If Scenarios.
        /// </summary>
        MasterSchedulerDepricated,
        /// <summary>
        /// Can only view the Live Scenario only.  No changes are allowed.
        /// </summary>
        ViewLive,
        /// <summary>
        /// View published and what if scenarios
        /// </summary>
        ViewWhatIfs,
        /// <summary>
        /// View live and update status
        /// </summary>
        ShopFloor,
        /// <summary>
        /// View live and run CTP
        /// </summary>
        CustomerService,
        /// <summary>
        /// View live and maintain resources and capacity
        /// </summary>
        Operations,
        /// <summary>
        /// View all scenarios and edit what-ifs
        /// </summary>
        Planner,
        /// <summary>
        /// Schedule on the live and and what if scenarios
        /// </summary>
        Scheduler,
        /// <summary>
        /// Schedule and maintain data on all scenarios
        /// </summary>
        MaintainData,
        /// <summary>
        /// MaintainData and run import and publish
        /// </summary>
        MasterScheduler,
        /// <summary>
        /// Full system access including managing users
        /// </summary>
        SystemAdministrator,
        /// <summary>
        /// Currently unused
        /// </summary>
        Super,
    };
    
    public enum ELicenseStatus
    {
        Unknown,
        Active,
        ReadOnly,
        Error
    }
}
