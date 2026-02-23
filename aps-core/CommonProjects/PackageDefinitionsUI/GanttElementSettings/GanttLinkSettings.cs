using PT.Common.Utility;
using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class GanttLinkSettings : ISettingData, ICloneable
{
    public GanttLinkSettings()
    {
        ShowJobLinks = true;
        ShowPredecessorMoLinks = true;
        ShowSuccessorMoLinks = true;
        ShowOperationLinks = true;
        ShowHelperLinks = true;
        ShowActivityLinks = true;
        ShowInventoryProductLinks = true;
        ShowConfirmedProductSupplies = true;
        ShowPossibleProductSupplies = true;
        ShowInventoryMaterialLinks = true;
        ShowConfirmedMaterialSupplies = true;
        ShowPossibleMaterialSupplies = true;
        ShowEligibleMaterialSupplies = true;
        m_widthScalar = 2;
    }

    public GanttLinkSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_widthScalar);
        }
        else
        {
            m_bools = new BoolVector32(a_reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_widthScalar);
    }

    private int m_widthScalar;

    public int WidthScalar
    {
        set => m_widthScalar = MathUtils.ConstrainRange(1, value, 10);
        get => m_widthScalar;
    }

    private BoolVector32 m_bools;

    private const short c_opLinksIdx = 0;
    private const short c_predecessorMOLinksIdx = 1;
    private const short c_jobLinksIdx = 2;
    private const short c_actLinksIdx = 3;
    private const short c_invProductLinksIdx = 4;
    private const short c_successorMOLinksIdx = 5;
    private const short c_invMaterialLinksIdx = 6;
    private const short c_confirmedMaterialLinksIdx = 7;
    private const short c_possibleMaterialLinksIdx = 8;
    private const short c_eligibleMaterialLinksIdx = 9;
    private const short c_showConfirmedProductSuppliesLinksIdx = 10;
    private const short c_showPossibleProductSuppliesLinksIdx = 11;


    public bool ShowJobLinks
    {
        get => m_bools[c_jobLinksIdx];
        set => m_bools[c_jobLinksIdx] = value;
    }

    public bool ShowPredecessorMoLinks
    {
        get => m_bools[c_predecessorMOLinksIdx];
        set => m_bools[c_predecessorMOLinksIdx] = value;
    }

    public bool ShowSuccessorMoLinks
    {
        get => m_bools[c_successorMOLinksIdx];
        set => m_bools[c_successorMOLinksIdx] = value;
    }

    public bool ShowOperationLinks
    {
        get => m_bools[c_opLinksIdx];
        set => m_bools[c_opLinksIdx] = value;
    }

    public bool ShowHelperLinks
    {
        get => m_bools[c_opLinksIdx];
        set => m_bools[c_opLinksIdx] = value;
    }

    public bool ShowActivityLinks
    {
        get => m_bools[c_actLinksIdx];
        set => m_bools[c_actLinksIdx] = value;
    }

    public bool ShowInventoryProductLinks
    {
        get => m_bools[c_invProductLinksIdx];
        set => m_bools[c_invProductLinksIdx] = value;
    }

    public bool ShowConfirmedProductSupplies
    {
        get => m_bools[c_showConfirmedProductSuppliesLinksIdx];
        set => m_bools[c_showConfirmedProductSuppliesLinksIdx] = value;
    }

    public bool ShowPossibleProductSupplies
    {
        get => m_bools[c_showPossibleProductSuppliesLinksIdx];
        set => m_bools[c_showPossibleProductSuppliesLinksIdx] = value;
    }

    public bool ShowInventoryMaterialLinks
    {
        get => m_bools[c_invMaterialLinksIdx];
        set => m_bools[c_invMaterialLinksIdx] = value;
    }

    public bool ShowConfirmedMaterialSupplies
    {
        get => m_bools[c_confirmedMaterialLinksIdx];
        set => m_bools[c_confirmedMaterialLinksIdx] = value;
    }
    
    public bool ShowPossibleMaterialSupplies
    {
        get => m_bools[c_possibleMaterialLinksIdx];
        set => m_bools[c_possibleMaterialLinksIdx] = value;
    }
    
    public bool ShowEligibleMaterialSupplies
    {
        get => m_bools[c_eligibleMaterialLinksIdx];
        set => m_bools[c_eligibleMaterialLinksIdx] = value;
    }

    public int UniqueId => 928;
    public string SettingKey => "userSetting_GanttLinks";
    public string Description => "TODO:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.GanttViewSettings;
    public string SettingCaption => "Link settings";

    object ICloneable.Clone()
    {
        return Clone();
    }

    public GanttLinkSettings Clone()
    {
        return (GanttLinkSettings)MemberwiseClone();
    }
}