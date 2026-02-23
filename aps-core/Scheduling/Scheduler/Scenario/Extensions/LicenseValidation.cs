namespace PT.Scheduler.Extensions;

public static class LicenseValidation
{
    public static void VerifyLicenseConstraintsForOptimizeSettings(this OptimizeSettings a_optSettings)
    {
        //TODO: V12 do we need this licensing?

        //if (a_optSettings.ReleaseRule == OptimizeSettings.releaseRules.JIT && !PTSystem.LicenseKey.IncludeBottleneckScheduling)
        //{
        //    PTSystem.LogException(new APSCommon.AuthorizationException("JIT Optimize Rules require the Bottleneck Scheduling Feature. Bottleneck Scheduling is not included in your License.  Release Rule has been automatically set to Job Release", "Release Rule", OptimizeSettings.releaseRules.JobRelease.ToString()));
        //    a_optSettings.ReleaseRule = OptimizeSettings.releaseRules.JobRelease;
        //}
        //if (a_optSettings.RunMrpDuringOptimizations && !PTSystem.LicenseKey.IncludeExpressMRP)
        //{
        //    PTSystem.LogException(new APSCommon.AuthorizationException("Running MPS/MRP During Optimizations requires MPS/MRP licensing not included in your License.  Running MPS/MRP During Optimizations has been disabled", "Run MPS/MRP During Optimizations", false.ToString()));
        //    a_optSettings.RunMrpDuringOptimizations = false;
        //}
        //if (a_optSettings.ReleaseRule == OptimizeSettings.releaseRules.DBR && !PTSystem.LicenseKey.IncludeBufferManagement)
        //{
        //    PTSystem.LogException(new APSCommon.AuthorizationException("DBR Optimize Settings require the Buffer Management Feature.  Buffer Management is not included in your License.  Release Rule has been automatically set to Job Release", "Release Rule", OptimizeSettings.releaseRules.JobRelease.ToString()));
        //    a_optSettings.ReleaseRule = OptimizeSettings.releaseRules.JobRelease;
        //}
    }
}