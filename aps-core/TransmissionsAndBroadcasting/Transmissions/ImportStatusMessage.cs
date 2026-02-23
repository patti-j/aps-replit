using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class ImportStatusMessage : IPTSerializable
{
    public ImportStatusMessage(BaseId a_instigator)
    {
        m_originalInstigator = a_instigator;
        m_uniqueIdentifier = Guid.NewGuid();
        m_progressStep = ImportStatuses.EImportProgressStep.Started;
        m_progressPercent = 0;
        m_startTimeTicks = DateTime.UtcNow.Ticks;
    }

    #region IPTSerializable Members
    public ImportStatusMessage(IReader reader)
    {
        if (reader.VersionNumber >= 12424)
        {
            short step;
            reader.Read(out step);
            m_progressStep = (ImportStatuses.EImportProgressStep)step;
            reader.Read(out m_progressPercent);
            reader.Read(out m_completedTimeTicks);
            reader.Read(out m_startTimeTicks);
            reader.Read(out m_uniqueIdentifier);
            m_scenarioId = new BaseId(reader);
            m_originalInstigator = new BaseId(reader);
            m_exceptions = new ApplicationExceptionList(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write((short)m_progressStep);
        writer.Write(m_progressPercent);
        writer.Write(m_completedTimeTicks);
        writer.Write(m_startTimeTicks);
        writer.Write(m_uniqueIdentifier);
        m_scenarioId.Serialize(writer);
        m_originalInstigator.Serialize(writer);
        m_exceptions.Serialize(writer);
    }

    public const int UNIQUE_ID = 1108;

    public int UniqueId => UNIQUE_ID;
    #endregion

    private double m_progressPercent;


    private Guid m_uniqueIdentifier;

    /// <summary>
    /// A unique identifier usable to track progress of a particular import
    /// </summary>
    public Guid UniqueIdentifier
    {
        get => m_uniqueIdentifier;
        set => m_uniqueIdentifier = value;
    }

    private BaseId m_scenarioId;

    /// <summary>
    /// The scenario this import is performed on
    /// </summary>
    public BaseId ScenarioId
    {
        get => m_scenarioId;
        set => m_scenarioId = value;
    }

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
    /// User id for the user who initiated the Import
    /// </summary>
    public BaseId OriginalInstigator
    {
        get => m_originalInstigator;
        set => m_originalInstigator = value;
    }

    private ImportStatuses.EImportProgressStep m_progressStep;

    /// <summary>
    /// Current progress status of the Import
    /// </summary>
    public ImportStatuses.EImportProgressStep ProgressStep
    {
        get => m_progressStep;
        set => m_progressStep = value;
    }

    private ApplicationExceptionList m_exceptions = new();

    /// <summary>
    /// Exceptions encountered during Import
    /// </summary>
    public ApplicationExceptionList Exceptions
    {
        get => m_exceptions;
        set
        {
            m_exceptions = value;
            if (value.Count > 0)
            {
                m_progressStep = ImportStatuses.EImportProgressStep.Error;
            }
        }
    }

    private long m_startTimeTicks = PTDateTime.MinValue.Ticks;

    /// <summary>
    /// The time the Import was start.
    /// </summary>
    public DateTime StartTime
    {
        get => new(m_startTimeTicks);

        set => m_startTimeTicks = value.Ticks;
    }

    private long m_completedTimeTicks = PTDateTime.MinValue.Ticks;

    /// <summary>
    /// The time the Import was completed.
    /// </summary>
    public DateTime CompletedTime
    {
        get => new(m_completedTimeTicks);

        set => m_completedTimeTicks = value.Ticks;
    }

    public bool IsCompleted => m_progressStep is ImportStatuses.EImportProgressStep.Complete
        or ImportStatuses.EImportProgressStep.Error
        or ImportStatuses.EImportProgressStep.Cancelled;
}