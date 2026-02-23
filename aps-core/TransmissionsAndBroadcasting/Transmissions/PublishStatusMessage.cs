using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class PublishStatusMessage : IPTSerializable
{
    public PublishStatusMessage(BaseId a_scenarioId)
    {
        m_progressStep = PublishStatuses.EPublishProgressStep.NeverPublished;
        m_originalInstigator = BaseId.ServerId;
    }

    #region IPTSerializable Members
    public PublishStatusMessage(IReader reader)
    {
        #region 627
        if (reader.VersionNumber >= 627)
        {
            short step;
            reader.Read(out step);
            m_progressStep = (PublishStatuses.EPublishProgressStep)step;
            reader.Read(out m_progressPercent);
            reader.Read(out m_completedTimeTicks);
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

    public void Serialize(IWriter writer)
    {
        writer.Write((short)m_progressStep);
        writer.Write(m_progressPercent);
        writer.Write(m_completedTimeTicks);
        m_originalInstigator.Serialize(writer);
        m_exceptions.Serialize(writer);
    }

    public const int UNIQUE_ID = 819;

    public int UniqueId => UNIQUE_ID;
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

    private long m_completedTimeTicks = PTDateTime.MinValue.Ticks;

    /// <summary>
    /// The time the publish was completed.
    /// </summary>
    public DateTime CompletedTime
    {
        get => new (m_completedTimeTicks);

        set => m_completedTimeTicks = value.Ticks;
    }

    public bool IsCompleted => m_progressStep is PublishStatuses.EPublishProgressStep.Complete
        or PublishStatuses.EPublishProgressStep.Error
        or PublishStatuses.EPublishProgressStep.Canceled;
}