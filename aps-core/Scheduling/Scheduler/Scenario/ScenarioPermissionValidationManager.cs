using PT.APSCommon;
using PT.PackageDefinitions.PackageInterfaces;

namespace PT.Scheduler;

internal class ScenarioPermissionValidationManager
{
    private readonly List<IPermissionsValidationElement> m_permissionValidationElements;

    internal ScenarioPermissionValidationManager()
    {
        m_permissionValidationElements = new List<IPermissionsValidationElement>();
    }

    /// <summary>
    /// Initialize permission validation elements
    /// </summary>
    /// <param name="a_modules"></param>
    public void InitializeValidationElements(List<IPermissionValidationModule> a_modules)
    {
        foreach (IPermissionValidationModule module in a_modules)
        {
            m_permissionValidationElements.AddRange(module.GetPermissionValidationElements());
        }
    }

    /// <summary>
    /// Verify that the instigator is able to send a transmission on the current scenario
    /// </summary>
    /// <param name="a_instigatorId"></param>
    /// <param name="a_s"></param>
    /// <returns></returns>
    public bool VerifyTransmission(BaseId a_instigatorId, Scenario a_s)
    {
        foreach (IPermissionsValidationElement elem in m_permissionValidationElements)
        {
            if (!elem.ValidatePermissions(a_instigatorId, a_s))
            {
                return false;
            }
        }

        return true;
    }
}