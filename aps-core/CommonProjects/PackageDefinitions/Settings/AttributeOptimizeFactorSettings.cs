namespace PT.PackageDefinitions.Settings;

public class AttributeOptimizeFactorSettings : ISettingData, ICloneable
{
    private Dictionary<string, decimal> m_attributeExternalIdsToMultiplier = new ();

    /*
     * We want the default multiplier for any PTAttribute to be 1.0 (which means it's just in use like normally).
     * The values in this dictionary might not be synchronized with the PTAttributes being added/removed
     * because it is stored as a setting on the SequenceFactor. The AttributeOptimizeFactorSettingsControl,
     * which allows the user to modify this dictionary, has an extra bool call UsedInSequencing
     * that we do not store explicitly. If UsedInSequencing is checked, then the row's multiplier value is
     * added to the dictionary. If UsedInSequencing is not checked, then 0 is always added to the dictionary.
     * Since UsedInSequencing is not explicitly stored, if the user tries to set it to true and set the multiplier
     * to 0, it'll get loaded back into the control with UsedInSequencing as false. Functionally though,
     * with how these multipliers are used, UsedInSequencing's value has no impact on the schedule
     * if the multiplier is 0.
     */

    //Attribute here refers to PTAttribute, not OperationAttribute
    public Dictionary<string, decimal> AttributeExternalIdsToMultiplier
    {
        get => m_attributeExternalIdsToMultiplier;
        set => m_attributeExternalIdsToMultiplier = value;
    }

    private bool m_subtractScore;

    public bool SubtractScore
    {
        get => m_subtractScore;
        set => m_subtractScore = value;
    }

    public AttributeOptimizeFactorSettings(string a_settingKey)
    {
        SettingKey = a_settingKey;
    }

    public AttributeOptimizeFactorSettings() { }

    public AttributeOptimizeFactorSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12303)
        {
            a_reader.Read(out int attributeCount);
            for (int i = 0; i < attributeCount; i++)
            {
                a_reader.Read(out string attributeExternalId);
                a_reader.Read(out decimal multiplier);
                m_attributeExternalIdsToMultiplier.Add(attributeExternalId, multiplier);
            }

            a_reader.Read(out m_subtractScore);
        }
        else if (a_reader.VersionNumber >= 12059)
        {
            a_reader.Read(out int attributeCount);
            for (int i = 0; i < attributeCount; i++)
            {
                a_reader.Read(out string attributeName);
                a_reader.Read(out decimal multiplier);
                // We are not maintaining backwards compatibility for OperationAttributes
                // so just read in all the values to ensure the other serialization stuff doesn't get messed up,
                // but we can just discard the values afterwards.
            }

            a_reader.Read(out m_subtractScore);
        }
        else
        {
            /*
             * For #21690, we needed to add functionality so that each of the Attributes
             * sequence factors could assign different weights to each attribute when
             * scheduling.
             * This had to go into 12.0 though, and we cannot change serialization like
             * we normally do due to the split between 12.1 and 12.0. This is why
             * we're doing this weird string into KeyValuePair format. The goal
             * is to maintain backwards compatibility between the two branches.
             */
            // There aren't really any customers out there on 12.0 so we can probably remove this,
            // but the explanation above is why the de-serialization below is complicated. 
            a_reader.Read(out int attributeCount);
            for (int i = 0; i < attributeCount; i++)
            {
                a_reader.Read(out string attributeInfo);
                if (attributeInfo.Contains('^'))
                {
                    string[] attributeInfoTokens = attributeInfo.Split('^');
                    string attributeName = attributeInfoTokens[0];
                    decimal attributeWeight = Convert.ToDecimal(attributeInfoTokens[1]);
                    m_attributeExternalIdsToMultiplier.Add(attributeName, attributeWeight);
                }
                else
                {
                    m_attributeExternalIdsToMultiplier.Add(attributeInfo, 100m);
                }
            }

            // We are not maintaining backwards compatibility for OperationAttributes
            // so just read in all the values to ensure the other serialization stuff doesn't get messed up,
            // but we can just discard the values afterwards.
            m_attributeExternalIdsToMultiplier.Clear();

            a_reader.Read(out m_subtractScore);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_attributeExternalIdsToMultiplier.Count);
        foreach (KeyValuePair<string, decimal> attributeWeights in m_attributeExternalIdsToMultiplier)
        {
            a_writer.Write(attributeWeights.Key);
            a_writer.Write(attributeWeights.Value);
        }

        a_writer.Write(m_subtractScore);
    }

    public int UniqueId => 1039;
    public string SettingKey { get; set; }
    public string SettingCaption { get; set; }
    public string Description => "Settings data for attribute optimize factors";
    public string SettingsGroup => "Sequence Planning";
    public string SettingsGroupCategory => "Sequence Planning";

    object ICloneable.Clone()
    {
        return Clone();
    }

    public AttributeOptimizeFactorSettings Clone()
    {
        return (AttributeOptimizeFactorSettings)MemberwiseClone();
    }
}