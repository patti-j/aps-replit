using PT.Common.Exceptions;

namespace PT.Scheduler.Simulation.Customizations;

public class ChangeableOperationValues
{
    public class ChangableResourceRequirementValue
    {
        public ChangableResourceRequirementValue(ResourceRequirement a_resourceRequirement, BaseResource a_defaultResource)
        {
            if (a_resourceRequirement == null)
            {
                throw new PTException("4087");
            }

            ResourceRequirement = a_resourceRequirement;
            DefaultResource = a_defaultResource;
        }

        public ResourceRequirement ResourceRequirement { get; private set; }

        public BaseResource DefaultResource { get; private set; }
    }

    public List<ChangableResourceRequirementValue> ResourceRequirementChanges { get; set; }
}