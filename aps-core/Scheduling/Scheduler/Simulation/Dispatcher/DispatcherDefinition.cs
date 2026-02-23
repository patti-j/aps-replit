using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for IDispatcherDefinition.
/// </summary>
public abstract class DispatcherDefinition : ExternalBaseIdObject
{
    public new const int UNIQUE_ID = 295;

    #region IPTSerializable Members
    public DispatcherDefinition(IReader a_reader)
        : base(a_reader, a_reader.VersionNumber < 12306)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_name);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_name);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected DispatcherDefinition(BaseId a_id)
        : base(a_id)
    {
        Name = "New Sequencing Plan".Localize();
        // There's only one usage of this constructor where the name would be checked 
        // when clearing the DispatcherDefinitionManager so
        // this name should be unique within the context it's used.
    }

    protected DispatcherDefinition(BaseId a_id, string a_name)
        : base(a_id)
    {
        Name = a_name;
    }

    internal DispatcherDefinition(DispatcherDefinition a_source, BaseId a_newId)
        : base(a_newId)
    {
        Name = a_source.Name + "_copy";
    }

    private string m_name = "";

    /// <summary>
    /// Unique, changeable, text identifier.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    internal abstract ReadyActivitiesDispatcher CreateDispatcher();

    internal abstract IComparer<KeyAndActivity> Comparer { get; }

    #region Simulation
    internal virtual void SimulationInitialization(ScenarioDetail a_sd) { }
    #endregion
}