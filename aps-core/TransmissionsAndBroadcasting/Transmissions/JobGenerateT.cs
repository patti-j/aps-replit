using PT.APSCommon;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Used to generate copies of Jobs.
/// </summary>
public class JobGenerateT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 597;

    #region IPTSerializable Members
    public JobGenerateT(IReader a_reader)
        : base(a_reader)
    {
        JobsToCopy = new ExternalIdList();

        if (a_reader.VersionNumber >= 12557)
        {
            a_reader.Read(out int jobCount);
            for (int i = 0; i < jobCount; i++)
            {
                a_reader.Read(out string jobExternalId);
                JobsToCopy.Add(jobExternalId);
            }

            a_reader.Read(out randomizeNeedDate);
            a_reader.Read(out MinNeedDate);
            a_reader.Read(out MaxNeedDate);

            a_reader.Read(out randomizeRequiredQty);
            a_reader.Read(out MinReqQty);
            a_reader.Read(out MaxReqQty);

            a_reader.Read(out randomizeRevenue);
            a_reader.Read(out MinRevenue);
            a_reader.Read(out MaxRevenue);

            a_reader.Read(out randomizePriority);
            a_reader.Read(out MinPriority);
            a_reader.Read(out MaxPriority);

            a_reader.Read(out randomizeMinutesPerCycle);
            a_reader.Read(out MinMinutesPerCycle);
            a_reader.Read(out MaxMinutesPerCycle);

            a_reader.Read(out randomizeSetupHours);
            a_reader.Read(out MinSetupHours);
            a_reader.Read(out MaxSetupHours);

            a_reader.Read(out randomizeSetupNumber);
            a_reader.Read(out MinSetupNumber);
            a_reader.Read(out MaxSetupNumber);

            a_reader.Read(out NbrOfCopiesPerJob);
            a_reader.Read(out randomizeCommitment);

            a_reader.Read(out randomizeCustomer);
            a_reader.Read(out randomizeProductNamesAndColors);
            a_reader.Read(out JobStartValue);
            a_reader.Read(out JobPrefixName);
            a_reader.Read(out randomizeColor);

            int productCount;
            a_reader.Read(out productCount);
            productNames = new string[productCount];
            for (int i = 0; i < productCount; i++)
            {
                string prodName;
                a_reader.Read(out prodName);
                productNames.SetValue(prodName, i);
            }

            int colorCount;
            a_reader.Read(out colorCount);
            colors = new System.Drawing.Color[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                System.Drawing.Color nxtColor;
                a_reader.Read(out nxtColor);
                colors.SetValue(nxtColor, i);
            }

            int customerCount;
            a_reader.Read(out customerCount);
            customers = new string[customerCount];
            for (int i = 0; i < customerCount; i++)
            {
                string nxtCustomer;
                a_reader.Read(out nxtCustomer);
                customers.SetValue(nxtCustomer, i);
            }

            bools = new BoolVector32(a_reader);
        }

        #region 12303

        else if (a_reader.VersionNumber >= 12303)
        {
            _ = new BaseIdList(a_reader);

            a_reader.Read(out randomizeNeedDate);
            a_reader.Read(out MinNeedDate);
            a_reader.Read(out MaxNeedDate);

            a_reader.Read(out randomizeRequiredQty);
            a_reader.Read(out MinReqQty);
            a_reader.Read(out MaxReqQty);

            a_reader.Read(out randomizeRevenue);
            a_reader.Read(out MinRevenue);
            a_reader.Read(out MaxRevenue);

            a_reader.Read(out randomizePriority);
            a_reader.Read(out MinPriority);
            a_reader.Read(out MaxPriority);

            a_reader.Read(out randomizeMinutesPerCycle);
            a_reader.Read(out MinMinutesPerCycle);
            a_reader.Read(out MaxMinutesPerCycle);

            a_reader.Read(out randomizeSetupHours);
            a_reader.Read(out MinSetupHours);
            a_reader.Read(out MaxSetupHours);

            a_reader.Read(out randomizeSetupNumber);
            a_reader.Read(out MinSetupNumber);
            a_reader.Read(out MaxSetupNumber);

            a_reader.Read(out NbrOfCopiesPerJob);
            a_reader.Read(out randomizeCommitment);

            a_reader.Read(out randomizeCustomer);
            a_reader.Read(out randomizeProductNamesAndColors);
            a_reader.Read(out JobStartValue);
            a_reader.Read(out JobPrefixName);
            a_reader.Read(out randomizeColor);

            int productCount;
            a_reader.Read(out productCount);
            productNames = new string[productCount];
            for (int i = 0; i < productCount; i++)
            {
                string prodName;
                a_reader.Read(out prodName);
                productNames.SetValue(prodName, i);
            }

            int colorCount;
            a_reader.Read(out colorCount);
            colors = new System.Drawing.Color[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                System.Drawing.Color nxtColor;
                a_reader.Read(out nxtColor);
                colors.SetValue(nxtColor, i);
            }

            int customerCount;
            a_reader.Read(out customerCount);
            customers = new string[customerCount];
            for (int i = 0; i < customerCount; i++)
            {
                string nxtCustomer;
                a_reader.Read(out nxtCustomer);
                customers.SetValue(nxtCustomer, i);
            }

            bools = new BoolVector32(a_reader);
        }

        #endregion
        #region version 625
        else if (a_reader.VersionNumber >= 625)
        {
            _ = new BaseIdList(a_reader);

            a_reader.Read(out randomizeNeedDate);
            a_reader.Read(out MinNeedDate);
            a_reader.Read(out MaxNeedDate);

            a_reader.Read(out randomizeRequiredQty);
            a_reader.Read(out MinReqQty);
            a_reader.Read(out MaxReqQty);

            a_reader.Read(out randomizeRevenue);
            a_reader.Read(out MinRevenue);
            a_reader.Read(out MaxRevenue);

            a_reader.Read(out randomizePriority);
            a_reader.Read(out MinPriority);
            a_reader.Read(out MaxPriority);

            a_reader.Read(out randomizeMinutesPerCycle);
            a_reader.Read(out MinMinutesPerCycle);
            a_reader.Read(out MaxMinutesPerCycle);

            a_reader.Read(out randomizeSetupHours);
            a_reader.Read(out MinSetupHours);
            a_reader.Read(out MaxSetupHours);

            a_reader.Read(out randomizeSetupNumber);
            a_reader.Read(out MinSetupNumber);
            a_reader.Read(out MaxSetupNumber);

            a_reader.Read(out NbrOfCopiesPerJob);
            a_reader.Read(out randomizeCommitment);

            a_reader.Read(out randomizeCustomer);
            a_reader.Read(out randomizeProductNamesAndColors);
            a_reader.Read(out bool useProductColorAsOperationAttribute);
            a_reader.Read(out JobStartValue);
            a_reader.Read(out JobPrefixName);
            a_reader.Read(out randomizeColor);

            int productCount;
            a_reader.Read(out productCount);
            productNames = new string[productCount];
            for (int i = 0; i < productCount; i++)
            {
                string prodName;
                a_reader.Read(out prodName);
                productNames.SetValue(prodName, i);
            }

            int colorCount;
            a_reader.Read(out colorCount);
            colors = new System.Drawing.Color[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                System.Drawing.Color nxtColor;
                a_reader.Read(out nxtColor);
                colors.SetValue(nxtColor, i);
            }

            int customerCount;
            a_reader.Read(out customerCount);
            customers = new string[customerCount];
            for (int i = 0; i < customerCount; i++)
            {
                string nxtCustomer;
                a_reader.Read(out nxtCustomer);
                customers.SetValue(nxtCustomer, i);
            }

            bools = new BoolVector32(a_reader);
        }
        #endregion 625
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(JobsToCopy.Count);
        for (int i = 0; i < JobsToCopy.Count; i++)
        {
            string jobExternalId = JobsToCopy[i];
            writer.Write(jobExternalId);
        }

        writer.Write(randomizeNeedDate);
        writer.Write(MinNeedDate);
        writer.Write(MaxNeedDate);

        writer.Write(randomizeRequiredQty);
        writer.Write(MinReqQty);
        writer.Write(MaxReqQty);

        writer.Write(randomizeRevenue);
        writer.Write(MinRevenue);
        writer.Write(MaxRevenue);

        writer.Write(randomizePriority);
        writer.Write(MinPriority);
        writer.Write(MaxPriority);

        writer.Write(randomizeMinutesPerCycle);
        writer.Write(MinMinutesPerCycle);
        writer.Write(MaxMinutesPerCycle);

        writer.Write(randomizeSetupHours);
        writer.Write(MinSetupHours);
        writer.Write(MaxSetupHours);

        writer.Write(randomizeSetupNumber);
        writer.Write(MinSetupNumber);
        writer.Write(MaxSetupNumber);

        writer.Write(NbrOfCopiesPerJob);
        writer.Write(randomizeCommitment);

        writer.Write(randomizeCustomer);
        writer.Write(randomizeProductNamesAndColors);
        writer.Write(JobStartValue);
        writer.Write(JobPrefixName);
        writer.Write(randomizeColor);

        writer.Write(productNames.Length);
        for (int i = 0; i < productNames.Length; i++)
        {
            writer.Write(productNames.GetValue(i).ToString());
        }

        writer.Write(colors.Length);
        for (int i = 0; i < colors.Length; i++)
        {
            writer.Write((System.Drawing.Color)colors.GetValue(i));
        }

        writer.Write(customers.Length);
        for (int i = 0; i < customers.Length; i++)
        {
            writer.Write(customers.GetValue(i).ToString());
        }

        bools.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public JobGenerateT() { }

    public JobGenerateT(BaseId scenarioId, ExternalIdList a_jobsToCopy)
        : base(scenarioId)
    {
        JobsToCopy = a_jobsToCopy;
    }

    public ExternalIdList JobsToCopy;

    public int NbrOfCopiesPerJob;

    public bool randomizeNeedDate;
    public DateTime MinNeedDate;
    public DateTime MaxNeedDate;

    public bool randomizeRequiredQty;
    public decimal MinReqQty;
    public decimal MaxReqQty;

    public bool randomizeRevenue;
    public decimal MinRevenue;
    public decimal MaxRevenue;

    public bool randomizePriority;
    public int MinPriority;
    public int MaxPriority;

    public bool randomizeMinutesPerCycle;
    public decimal MinMinutesPerCycle;
    public decimal MaxMinutesPerCycle;

    public bool randomizeSetupHours;
    public decimal MinSetupHours;
    public decimal MaxSetupHours;

    public bool randomizeSetupNumber;
    public decimal MinSetupNumber;
    public decimal MaxSetupNumber;

    public int JobStartValue;
    public string JobPrefixName;
    public bool randomizeCommitment;
    public bool randomizeColor;

    public bool randomizeProductNamesAndColors;
    public string[] productNames = new string[0]; //so don't need to check for it being null in serialization if not set in UI.
    public System.Drawing.Color[] colors = new System.Drawing.Color[0]; //so don't need to check for it being null in serialization if not set in UI.
    public bool randomizeCustomer;
    public string[] customers = new string[0]; //so don't need to check for it being null in serialization if not set in UI.

    private BoolVector32 bools;
    private const int ConvertTemplatesToJobsIdx = 0;

    /// <summary>
    /// If true then any Job created from a Template has its Template flag set to false.
    /// </summary>
    public bool ConvertTemplatesToJobs
    {
        get => bools[ConvertTemplatesToJobsIdx];
        set => bools[ConvertTemplatesToJobsIdx] = value;
    }

    public override string Description => "Job Copies generated";
}