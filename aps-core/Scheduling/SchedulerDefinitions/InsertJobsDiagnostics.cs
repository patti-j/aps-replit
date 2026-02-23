using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Holds all diagnostic information for the InsertJobs simulation.
/// </summary>
public class InsertJobsDiagnostics : IPTSerializable
{
    public InsertJobsDiagnostics()
    {
        InsertJobsStatus = CoPilotSimulationStatus.STOPPED;
        CurrentInsertedJobsCount = 0;
        TotalJobsToInsert = 0;
    }

    public const int UNIQUE_ID = 778;

    #region IPTSerializable Members
    public InsertJobsDiagnostics(IReader reader)
    {
        if (reader.VersionNumber >= 469)
        {
            reader.Read(out m_insertJobsStatus);
            reader.Read(out m_currentInsertedJobs);
            reader.Read(out m_totalJobsToInsert);
            m_currentWorkingJob = new BaseId(reader);
            InsertJobsResultList = new InsertJobsResultsList(reader);
            reader.Read(out m_currentJobProgress);
        }

        #region 461
        else if (reader.VersionNumber >= 461)
        {
            reader.Read(out m_insertJobsStatus);
            reader.Read(out m_currentInsertedJobs);
            reader.Read(out m_totalJobsToInsert);
            m_currentWorkingJob = new BaseId(reader);
            InsertJobsResultList = new InsertJobsResultsList(reader);
        }
        #endregion

        #region 460
        else if (reader.VersionNumber >= 460)
        {
            reader.Read(out m_insertJobsStatus);
            reader.Read(out m_currentInsertedJobs);
            reader.Read(out m_totalJobsToInsert);
            InsertJobsResultList = new InsertJobsResultsList(reader);
        }
        #endregion

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            int obsoleteStatus;
            reader.Read(out obsoleteStatus);
            reader.Read(out m_insertJobsStatus);
            reader.Read(out m_currentInsertedJobs);
            reader.Read(out m_totalJobsToInsert);
            InsertJobsResultList = new InsertJobsResultsList(reader);
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_insertJobsStatus);
        writer.Write(m_currentInsertedJobs);
        writer.Write(m_totalJobsToInsert);
        m_currentWorkingJob.Serialize(writer);
        InsertJobsResultList.Serialize(writer);
        writer.Write(m_currentJobProgress);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region InsertJobs
    private int m_insertJobsStatus;

    /// <summary>
    /// Current status of the InsertJobs simulation
    /// </summary>
    public CoPilotSimulationStatus InsertJobsStatus
    {
        get => (CoPilotSimulationStatus)m_insertJobsStatus;
        set => m_insertJobsStatus = (int)value;
    }

    private BaseId m_currentWorkingJob;

    /// <summary>
    /// Current status of the InsertJobs simulation
    /// </summary>
    public BaseId CurrentWorkingJob
    {
        get => m_currentWorkingJob;
        set => m_currentWorkingJob = value;
    }

    private int m_currentInsertedJobs;

    /// <summary>
    /// Number of jobs that have been inserted so far in the InsertJobs simulation
    /// </summary>
    public int CurrentInsertedJobsCount
    {
        get => m_currentInsertedJobs;
        set => m_currentInsertedJobs = value;
    }

    private int m_totalJobsToInsert;

    /// <summary>
    /// The total number of jobs to be inserted during the InertJobs simulation
    /// </summary>
    public int TotalJobsToInsert
    {
        get => m_totalJobsToInsert;
        set => m_totalJobsToInsert = value;
    }

    private double m_currentJobProgress;

    /// <summary>
    /// The current progress of the working Job.
    /// </summary>
    public double CurrentJobProgress
    {
        get => m_currentJobProgress;
        set => m_currentJobProgress = value;
    }

    /// <summary>
    /// List that stores results from the InsertJobs simulation.
    /// Holds each job that has been scheduled and wheter it was late or not
    /// </summary>
    public InsertJobsResultsList InsertJobsResultList = new ();

    /// <summary>
    /// List that stores results from the InsertJobs simulation.
    /// Holds each job that has been scheduled and wheter it was late or not
    /// </summary>
    public class InsertJobsResultsList
    {
        public InsertJobsResultsList()
        {
            m_jobIdsList = new List<BaseId>();
            m_isLatelist = new List<bool>();
            m_errorList = new List<string>();
        }

        public const int UNIQUE_ID = 786;

        #region IPTSerializable Members
        public InsertJobsResultsList(IReader reader)
        {
            if (reader.VersionNumber >= 475)
            {
                int listLength;
                reader.Read(out listLength);
                m_jobIdsList = new List<BaseId>();
                for (int i = 0; i < listLength; i++)
                {
                    BaseId nextId = new (reader);
                    m_jobIdsList.Add(nextId);
                }

                reader.Read(out listLength);
                m_isLatelist = new List<bool>();
                for (int i = 0; i < listLength; i++)
                {
                    bool isLate;
                    reader.Read(out isLate);
                    m_isLatelist.Add(isLate);
                }

                reader.Read(out listLength);
                m_errorList = new List<string>();
                for (int i = 0; i < listLength; i++)
                {
                    string message;
                    reader.Read(out message);
                    m_errorList.Add(message);
                }
            }
            else if (reader.VersionNumber >= 1)
            {
                int listLength;
                reader.Read(out listLength);
                m_jobIdsList = new List<BaseId>();
                for (int i = 0; i < listLength; i++)
                {
                    BaseId nextId = new (reader);
                    m_jobIdsList.Add(nextId);
                }

                reader.Read(out listLength);
                m_isLatelist = new List<bool>();
                for (int i = 0; i < listLength; i++)
                {
                    bool isLate;
                    reader.Read(out isLate);
                    m_isLatelist.Add(isLate);
                }

                m_errorList = new List<string>();
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(m_jobIdsList.Count);
            for (int i = 0; i < m_jobIdsList.Count; i++)
            {
                m_jobIdsList[i].Serialize(writer);
            }

            writer.Write(m_isLatelist.Count);
            for (int i = 0; i < m_isLatelist.Count; i++)
            {
                writer.Write(m_isLatelist[i]);
            }

            writer.Write(m_errorList.Count);
            for (int i = 0; i < m_errorList.Count; i++)
            {
                writer.Write(m_errorList[i]);
            }
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        private List<BaseId> m_jobIdsList;
        private List<bool> m_isLatelist;
        private List<string> m_errorList;

        /// <summary>
        /// Add a result to the list
        /// </summary>
        /// <param name="a_baseId">Job that has been inserted</param>
        /// <param name="a_isLate">If the job is late</param>
        public void AddJobResult(BaseId a_baseId, bool a_isLate, string a_errorMessage = "")
        {
            m_jobIdsList.Add(a_baseId);
            m_isLatelist.Add(a_isLate);
            m_errorList.Add(a_errorMessage);
        }

        /// <summary>
        /// Creates a simple copy of the InsertJobsResultList
        /// </summary>
        public InsertJobsResultsList CopyList()
        {
            InsertJobsResultsList newList = new ();
            newList.CopyLists(m_jobIdsList, m_isLatelist, m_errorList);
            return newList;
        }

        /// <summary>
        /// Copys the member lists in the class. This should only be used by InsertJobsResultLists class
        /// </summary>
        /// <param name="a_jobIdsList"></param>
        /// <param name="a_isLateList"></param>
        internal void CopyLists(List<BaseId> a_jobIdsList, List<bool> a_isLateList, List<string> a_errorMessageList)
        {
            m_jobIdsList = new List<BaseId>(a_jobIdsList);
            m_isLatelist = new List<bool>(a_isLateList);
            m_errorList = new List<string>(a_errorMessageList);
        }

        /// <summary>
        /// Returns the results for the Client in a usable form.
        /// </summary>
        /// <returns></returns>
        public List<Tuple<BaseId, bool, string>> GetValuesAsTupleList()
        {
            List<Tuple<BaseId, bool, string>> tupleList = new ();
            for (int i = 0; i < m_jobIdsList.Count; i++)
            {
                tupleList.Add(new Tuple<BaseId, bool, string>(m_jobIdsList[i], m_isLatelist[i], m_errorList[i]));
            }

            return tupleList;
        }
    }
    #endregion
}