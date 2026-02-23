using PT.PackageDefinitions.PackageInterfaces;
using PT.Transmissions;

namespace PT.Scheduler;

internal class ScenarioLicenseManager
{
    public bool DataReadOnly;
    private readonly List<ILicenseValidationElement> m_validationElements;

    internal ScenarioLicenseManager()
    {
        m_validationElements = new List<ILicenseValidationElement>();
    }

    public void RegisterLicenseElements(IEnumerable<ILicenseValidationModule> a_modules)
    {
        foreach (ILicenseValidationModule module in a_modules)
        {
            List<ILicenseValidationElement> elements = module.GetLicenseValidationElements();
            foreach (ILicenseValidationElement element in elements)
            {
                m_validationElements.Add(element);
            }
        }
    }

    public void ValidateData(Scenario a_s, ScenarioDetail a_sd)
    {
        foreach (ILicenseValidationElement element in m_validationElements)
        {
            //Check for readonly
            if (!element.ValidateData(a_s, a_sd))
            {
                DataReadOnly = true;
                return;
            }
        }

        DataReadOnly = false;
    }

    public bool VerifyDataLimit(long a_dataCount, string a_dataKey)
    {
        foreach (ILicenseValidationElement licenseValidationElement in m_validationElements)
        {
            if (licenseValidationElement.DataKey == a_dataKey)
            {
                if (licenseValidationElement.VerifyDataLimit(a_dataCount))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool VerifyReadonlyTransmission(ScenarioBaseT a_scenarioBaseT)
    {
        //We need to allow TouchTs for initial login and to avoid UI errors.
        if (a_scenarioBaseT is ScenarioTouchT)
        {
            return true;
        }

        foreach (ILicenseValidationElement element in m_validationElements)
        {
            if (element.VerifyReadonlyTransmission(a_scenarioBaseT))
            {
                return true;
            }
        }

        return false;
    }

    public bool VerifyTransmission(ScenarioBaseT a_scenarioBaseT)
    {
        foreach (ILicenseValidationElement element in m_validationElements)
        {
            if (!element.VerifyTransmission(a_scenarioBaseT))
            {
                return false;
            }
        }

        return true;
    }
}