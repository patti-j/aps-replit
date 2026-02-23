using PT.APSCommon;
using PT.Common.ObjectHelpers;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct SequenceFactorElementKey
{
    public readonly BaseId DispatcherDefinitionId;
    public readonly string ElementKey;

    public SequenceFactorElementKey(BaseId a_dispatcherDefinitionId, string a_elementKey)
    {
        DispatcherDefinitionId = a_dispatcherDefinitionId;
        ElementKey = a_elementKey;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is SequenceFactorElementKey key)
        {
            return DispatcherDefinitionId.Value == key.DispatcherDefinitionId.Value && ElementKey == key.ElementKey;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCodeHelper.GetHashCode(DispatcherDefinitionId, ElementKey);
    }
}