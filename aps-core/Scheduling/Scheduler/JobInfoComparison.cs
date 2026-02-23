using System.Collections;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Summarizes the differences between any two JobInfo objects.
/// </summary>
public class JobInfoComparison
{
    public JobInfoComparison(JobInfo jobInfoBefore, JobInfo jobInfoAfter)
    {
        this.jobInfoBefore = jobInfoBefore;
        this.jobInfoAfter = jobInfoAfter;

        AddNewMaterialShortages(jobInfoBefore.MaterialShortages.GetNewShortages(jobInfoAfter.MaterialShortages));
        AddFilledMaterialShortages(jobInfoBefore.MaterialShortages.GetDeletedShortages(jobInfoAfter.MaterialShortages));
        AddChangedMaterialShortage(jobInfoBefore.MaterialShortages.GetShortagesDiffs(jobInfoBefore.MaterialShortages, jobInfoAfter.MaterialShortages));
    }

    private readonly JobInfo jobInfoBefore;

    internal JobInfo JobInfoBefore => jobInfoBefore;

    private readonly JobInfo jobInfoAfter;

    internal JobInfo JobInfoAfter => jobInfoAfter;

    public enum latenessChangeTypes
    {
        NoChange = 0,
        Later,
        LessLate,
        OnTimeToLate,
        LateToOnTime,
        Earlier,
        LessEarly,
        Scheduled,
        UnScheduled
    }

    /// <summary>
    /// Describes the impact on the Lateness of the Job.
    /// </summary>
    public latenessChangeTypes LatenessChangeType
    {
        get
        {
            if (jobInfoBefore.Scheduled != jobInfoAfter.Scheduled)
            {
                if (jobInfoAfter.Scheduled)
                {
                    return latenessChangeTypes.Scheduled;
                }

                return latenessChangeTypes.UnScheduled;
            }

            if (jobInfoAfter.Late)
            {
                if (jobInfoBefore.Late)
                {
                    if (jobInfoAfter.Lateness > jobInfoBefore.Lateness)
                    {
                        return latenessChangeTypes.Later;
                    }

                    if (jobInfoAfter.Lateness < jobInfoBefore.Lateness)
                    {
                        return latenessChangeTypes.LessLate;
                    }

                    return latenessChangeTypes.NoChange;
                }

                return latenessChangeTypes.OnTimeToLate;
            }

            //this Job is ontime or early
            if (jobInfoBefore.Late)
            {
                return latenessChangeTypes.LateToOnTime;
            }

            if (jobInfoAfter.Lateness > jobInfoBefore.Lateness)
            {
                return latenessChangeTypes.Earlier;
            }

            if (jobInfoAfter.Lateness < jobInfoBefore.Lateness)
            {
                return latenessChangeTypes.LessEarly;
            }

            return latenessChangeTypes.NoChange;
        }
    }

    /// <summary>
    /// If the job was scheduled in the frozen span and now is not
    /// </summary>
    public bool MovedOutOfFrozenSpan => jobInfoBefore.ScheduledInFrozenSpan && !jobInfoAfter.ScheduledInFrozenSpan;

    /// <summary>
    /// If the job was scheduled outside the frozen span and now is in the frozen span
    /// </summary>
    public bool MovedIntoFrozenSpan => !jobInfoBefore.ScheduledInFrozenSpan && jobInfoAfter.ScheduledInFrozenSpan;

    /// <summary>
    /// The Job's current Lateness minus its previous Lateness.  Positive numbers indicate an increase in Lateness.
    /// </summary>
    public TimeSpan LatenessChange => jobInfoAfter.Lateness.Subtract(jobInfoBefore.Lateness);

    //Material Shortages
    private readonly List<JobInfo.MaterialShortage> _newMaterialShortages = new ();

    public List<JobInfo.MaterialShortage> NewMaterialShortages => _newMaterialShortages;

    private readonly List<JobInfo.MaterialShortage> _filledMaterialShortages = new ();

    public List<JobInfo.MaterialShortage> FilledMaterialShortages => _filledMaterialShortages;

    private readonly List<JobInfo.MaterialShortageDiff> _changedMaterialShortages = new ();

    public List<JobInfo.MaterialShortageDiff> ChangedMaterialShortages => _changedMaterialShortages;

    private void AddNewMaterialShortages(List<JobInfo.MaterialShortage> aShortages)
    {
        for (int i = 0; i < aShortages.Count; i++)
        {
            _newMaterialShortages.Add(aShortages[i]);
        }
    }

    private void AddFilledMaterialShortages(List<JobInfo.MaterialShortage> aShortages)
    {
        for (int i = 0; i < aShortages.Count; i++)
        {
            _filledMaterialShortages.Add(aShortages[i]);
        }
    }

    private void AddChangedMaterialShortage(List<JobInfo.MaterialShortageDiff> aMaterialShortageDiffs)
    {
        for (int i = 0; i < aMaterialShortageDiffs.Count; i++)
        {
            _changedMaterialShortages.Add(aMaterialShortageDiffs[i]);
        }
    }

    public int MaterialShortageIncreasesCount => _newMaterialShortages.Count;

    public int MaterialShortageDecreasesCount => _filledMaterialShortages.Count;

    public int MaterialShortageChangesCount => _changedMaterialShortages.Count;

    #region Job Info fields
    /// <summary>
    /// Job Id
    /// </summary>
    public BaseId Id => jobInfoAfter.Id;

    /// <summary>
    /// True if the Job ends after its NeedDateTime.
    /// </summary>
    public bool Late => jobInfoAfter.Late;

    /// <summary>
    /// The ScheduledEndDateTime minus the NeedDateTime.
    /// </summary>
    public TimeSpan Lateness => jobInfoAfter.Lateness;
    #endregion
}

/// <summary>
/// Stores an ArrayList of JobInfoComparisons.
/// </summary>
public class JobInfoComparisonList : ICopyTable
{
    private readonly ArrayList jobInfoComparisons = new ();

    public JobInfoComparison Add(JobInfoComparison j)
    {
        jobInfoComparisons.Add(j);
        return j;
    }

    public void RemoveAt(int index)
    {
        jobInfoComparisons.RemoveAt(index);
    }

    public JobInfoComparison this[int index] => (JobInfoComparison)jobInfoComparisons[index];

    #region ICopyTable
    public Type ElementType => typeof(JobInfoComparison);

    public object GetRow(int index)
    {
        return jobInfoComparisons[index];
    }

    public int Count => jobInfoComparisons.Count;
    #endregion ICopyTable
}

/// <summary>
/// Stores the comparison between two Snapshots.
/// </summary>
public class SnapshotComparison
{
    #region Lists
    private readonly JobInfoComparisonList jobinfoComparisonList = new ();

    public JobInfoComparisonList JobinfoComparisonList => jobinfoComparisonList;

    private readonly JobInfoList addedJobInfos = new ();

    public JobInfoList AddedJobsList => addedJobInfos;

    private readonly JobInfoList deletedJobInfos = new ();

    public JobInfoList DeletedJobsList => deletedJobInfos;

    public void AddJobInfoComparison(JobInfoComparison jobInfoComparison)
    {
        JobinfoComparisonList.Add(jobInfoComparison);
        //store the material shortage differences for the Job
        AddNewMaterialShortages(jobInfoComparison.NewMaterialShortages);
        AddFilledMaterialShortages(jobInfoComparison.FilledMaterialShortages);
        AddChangedMaterialShortage(jobInfoComparison.ChangedMaterialShortages);
    }

    public void AddAddedJobInfo(JobInfo addedJobInfo)
    {
        addedJobInfos.Add(addedJobInfo);
        //store the material shortages for the ADDED Job
        AddNewMaterialShortages(addedJobInfo.MaterialShortages);
    }

    public void AddDeletedJobInfo(JobInfo deletedJobInfo)
    {
        deletedJobInfos.Add(deletedJobInfo);
    }

    //Material Shortages
    private readonly List<JobInfo.MaterialShortage> _newMaterialShortages = new ();

    public List<JobInfo.MaterialShortage> NewMaterialShortages => _newMaterialShortages;

    private readonly List<JobInfo.MaterialShortage> _filledMaterialShortages = new ();

    public List<JobInfo.MaterialShortage> FilledMaterialShortages => _filledMaterialShortages;

    private readonly List<JobInfo.MaterialShortageDiff> _changedMaterialShortages = new ();

    public List<JobInfo.MaterialShortageDiff> ChangedMaterialShortages => _changedMaterialShortages;

    public void AddNewMaterialShortages(List<JobInfo.MaterialShortage> aShortages)
    {
        for (int i = 0; i < aShortages.Count; i++)
        {
            _newMaterialShortages.Add(aShortages[i]);
        }
    }

    public void AddNewMaterialShortages(JobInfo.MaterialShortageList aShortages)
    {
        Dictionary<BaseId, JobInfo.MaterialShortage>.Enumerator shortagesEnumerator = aShortages.GetEnumerator();
        while (shortagesEnumerator.MoveNext())
        {
            JobInfo.MaterialShortage shortage = shortagesEnumerator.Current.Value;
            _newMaterialShortages.Add(shortage);
        }
    }

    public void AddFilledMaterialShortages(List<JobInfo.MaterialShortage> aShortages)
    {
        for (int i = 0; i < aShortages.Count; i++)
        {
            _filledMaterialShortages.Add(aShortages[i]);
        }
    }

    public void AddChangedMaterialShortage(List<JobInfo.MaterialShortageDiff> aMaterialShortageDiffs)
    {
        for (int i = 0; i < aMaterialShortageDiffs.Count; i++)
        {
            _changedMaterialShortages.Add(aMaterialShortageDiffs[i]);
        }
    }
    #endregion Lists

    #region Analysis
    public bool HaveNewLateJob()
    {
        for (int i = 0; i < JobinfoComparisonList.Count; i++)
        {
            JobInfoComparison jobInfoComparison = JobinfoComparisonList[i];

            if (jobInfoComparison.LatenessChangeType == JobInfoComparison.latenessChangeTypes.OnTimeToLate)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if a Job went from OnTime to Late and is now more than the Threshold late or became later by more than the Threshold.
    /// </summary>
    public bool HaveNewlyLateOrLaterJobPassedThreshold(TimeSpan aThreshold)
    {
        for (int i = 0; i < JobinfoComparisonList.Count; i++)
        {
            JobInfoComparison jobInfoComparison = JobinfoComparisonList[i];

            if (jobInfoComparison.LatenessChangeType == JobInfoComparison.latenessChangeTypes.OnTimeToLate && jobInfoComparison.Lateness.Ticks > aThreshold.Ticks)
            {
                return true;
            }

            if (jobInfoComparison.LatenessChangeType == JobInfoComparison.latenessChangeTypes.Later && jobInfoComparison.LatenessChange.Ticks > aThreshold.Ticks)
            {
                return true;
            }
        }

        return false;
    }

    public bool HaveNewOnTimeJob()
    {
        for (int i = 0; i < JobinfoComparisonList.Count; i++)
        {
            JobInfoComparison jobInfoComparison = JobinfoComparisonList[i];

            if (jobInfoComparison.LatenessChangeType == JobInfoComparison.latenessChangeTypes.LateToOnTime)
            {
                return true;
            }
        }

        return false;
    }
    #endregion Analysis
}