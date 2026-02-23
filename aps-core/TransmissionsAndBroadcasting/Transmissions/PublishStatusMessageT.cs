using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class PublishStatusMessageT : ScenarioIdBaseT, IPTSerializable
{
    public PublishStatusMessageT() { }

    public PublishStatusMessageT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    #region IPTSerializable Members
    public PublishStatusMessageT(IReader reader)
        : base(reader)
    {
        #region 627
        if (reader.VersionNumber >= 627)
        {
            short step;
            reader.Read(out step);
            m_progressStep = (PublishStatuses.EPublishProgressStep)step;
            reader.Read(out m_progressPercent);
            m_originalInstigator = new BaseId(reader);
            m_exceptions = new ApplicationExceptionList(reader);
        }
        #endregion

        #region 600
        else if (reader.VersionNumber >= 600)
        {
            short step;
            reader.Read(out step);
            m_progressStep = (PublishStatuses.EPublishProgressStep)step;
            reader.Read(out m_progressPercent);
            m_exceptions = new ApplicationExceptionList(reader);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((short)m_progressStep);
        writer.Write(m_progressPercent);
        m_originalInstigator.Serialize(writer);
        m_exceptions.Serialize(writer);
    }

    public const int UNIQUE_ID = 819;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private double m_progressPercent;

    /// <summary>
    /// A sub progress percent that can be interpreted by the UI
    /// </summary>
    public double ProgressPercent
    {
        get => m_progressPercent;
        set => m_progressPercent = value;
    }

    private BaseId m_originalInstigator;

    /// <summary>
    /// User id for the user who initiated the publish
    /// </summary>
    public BaseId OriginalInstigator
    {
        get => m_originalInstigator;
        set => m_originalInstigator = value;
    }

    private PublishStatuses.EPublishProgressStep m_progressStep;

    /// <summary>
    /// Current progress status of the publish
    /// </summary>
    public PublishStatuses.EPublishProgressStep ProgressStep
    {
        get => m_progressStep;
        set => m_progressStep = value;
    }

    private ApplicationExceptionList m_exceptions = new ();

    /// <summary>
    /// Exceptions encountered during publish
    /// </summary>
    public ApplicationExceptionList Exceptions
    {
        get => m_exceptions;
        set
        {
            m_exceptions = value;
            if (value.Count > 0)
            {
                m_progressStep = PublishStatuses.EPublishProgressStep.Error;
            }
        }
    }

    //These transmission descriptions should be displayed to the user. No need to store strings in undoset memory.
    public override string Description => "";
}