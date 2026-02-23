namespace PT.Transmissions;

public class SetupRangeAttributeUpdate
{
    #region IPTSerializable Members
    public SetupRangeAttributeUpdate(IReader reader)
    {
        if (reader.VersionNumber >= 258)
        {
            reader.Read(out m_ptAttributeExternalId);
            reader.Read(out attributeDescription);
            reader.Read(out eligibilityConstraint);

            int count;
            reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                fromRanges.Add(new SetupRangeUpdate(reader));
            }
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_ptAttributeExternalId);
            reader.Read(out attributeDescription);

            int count;
            reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                fromRanges.Add(new SetupRangeUpdate(reader));
            }
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_ptAttributeExternalId);
        writer.Write(attributeDescription);
        writer.Write(eligibilityConstraint);

        writer.Write(fromRanges.Count);
        for (int i = 0; i < fromRanges.Count; ++i)
        {
            fromRanges[i].Serialize(writer);
        }
    }
    #endregion

    public SetupRangeAttributeUpdate(string a_aPTAttributeExternalId, string aAttributeDescription, bool aEligibilityConstraint)
    {
        m_ptAttributeExternalId = a_aPTAttributeExternalId;
        attributeDescription = aAttributeDescription;
        eligibilityConstraint = aEligibilityConstraint;
    }

    public string m_ptAttributeExternalId;
    public string attributeDescription;
    public bool eligibilityConstraint;

    public List<SetupRangeUpdate> fromRanges = new ();
}