//using System;
//using System.Drawing;
//using System.Collections.Generic;

//using PT.Scheduler.Demand;
//using PT.Scheduler.PackageDefs;
//using PT.SchedulerDefinitions;

//namespace PT.Scheduler
//{
//    class KPICalculator
//    {
//        internal const decimal DefaultMultiplier = 1.0m;
//    }

//    // Note that an entry must be made in the contructor of KpiController() for each KPI Calculator below.

//    /// <summary>
//    /// Calculates the number of Jobs that are scheduled to finish after their Need DateTime.
//    /// </summary>
//    [Serializable]
//    public class LateJobsCalculator : IBaseKpiCalculatorElement
//    {
//        public LateJobsCalculator()
//        {
//        }
//        #region IKpiCalculator Members

//        string name = "Late Jobs";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The number of Jobs that are scheduled to finish after their Need DateTime.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 0;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Purple;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Integer;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            int tardyCnt = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (!job.Finished && job.Late && job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled) tardyCnt++;
//            }
//            return tardyCnt;
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the percent of all Jobs that are Late.
//    /// </summary>
//    [Serializable]
//    public class PercentLateJobsCalculator : IKpiCalculator
//    {
//        public const int UNIQUE_ID = 401;
//        public PercentLateJobsCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Late Jobs (%)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The percent of Jobs that are Late.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 2;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Red;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }
//        public string FormatString
//        {
//            get { return "P2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Percentage;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            if (sd.JobManager.NonTemplateJobsCount == 0)
//                return 0;

//            int tardyCnt = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.Late && !job.Finished && job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled) tardyCnt++;
//            }
//            return (decimal)tardyCnt / (decimal)sd.JobManager.NonTemplateJobsCount;
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the average number of days that Late Jobs are scheduled to finish after their Need DateTime.
//    /// </summary>
//    [Serializable]
//    public class AverageLatenessCalculator : IKpiCalculator
//    {
//        public AverageLatenessCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Average Lateness (days)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The average number of days that Late Jobs are scheduled to finish after their Need DateTime.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 3;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.DarkGray;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N1"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            return sd.JobManager.GetAverageLateness();
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the sum of revenue of all Jobs that are Late.
//    /// </summary>
//    [Serializable]
//    public class LateJobsRevenueCalculator : IKpiCalculator
//    {
//        public LateJobsRevenueCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Late Jobs Revenue";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The sum of revenue of Jobs that are Late.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 4;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.SteelBlue;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            decimal sumOfRevenue = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (!job.Finished && job.Late && job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                    sumOfRevenue = sumOfRevenue + job.Revenue;
//            }
//            return sumOfRevenue; //Showing revenue in Thousands to make values more in line with other KPI values.
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the sum of revenue of all Jobs that are finished or scheduled to End in the current quarter based on the Fiscal Year End.
//    /// </summary>
//    [Serializable]
//    public class ThisQtrRevenueCalculator : IKpiCalculator
//    {
//        public ThisQtrRevenueCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "This Qtr Revenue";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The sum of revenue of Jobs ending in this Qtr.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 10;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.LightGreen;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            decimal sumOfRevenue = 0;
//            DateTime thisQtrEnd = sd.ScenarioOptions.GetThisQtrEndDate(sd.ClockDate);
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Finished ||
//                    (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled && job.ScheduledEndDate < thisQtrEnd))
//                    sumOfRevenue = sumOfRevenue + job.Revenue;
//            }
//            return sumOfRevenue; //Showing revenue in Thousands to make values more in line with other KPI values.
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the average daily sum of revenue of all Jobs that are finished or scheduled to End in the Short Term.
//    /// </summary>
//    [Serializable]
//    public class AverageDailyRevenueCalculator : IKpiCalculator
//    {
//        public AverageDailyRevenueCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Daily Revenue";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The daily average of revenue of Jobs ending in the Short Term.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 11;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Pink;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            decimal sumOfRevenue = 0;
//            DateTime shortTermEnd = sd.GetEndOfShortTerm();
//            if (sd.ScenarioOptions.ShortTermSpan.TotalDays <= 0)
//                return -1; //avoid divide by zero.
//            else
//            {
//                for (int i = 0; i < sd.JobManager.Count; i++)
//                {
//                    Job job = sd.JobManager.GetByIndex(i);
//                    if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled && job.ScheduledEndDate <= shortTermEnd)
//                        sumOfRevenue = sumOfRevenue + job.Revenue;
//                }
//                return sumOfRevenue / (decimal)sd.ScenarioOptions.ShortTermSpan.TotalDays; //Showing revenue in Thousands to make values more in line with other KPI values.
//            }
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the daily average of Required Qty of all Jobs that are finished or scheduled to End Short Term.
//    /// </summary>
//    [Serializable]
//    public class AverageDailyMOQtyCalculator : IKpiCalculator
//    {
//        public AverageDailyMOQtyCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Daily Production (units)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The daily average of total Manufacturing Order Required Quantities of Jobs ending in the Short Term.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 12;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.LightBlue;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            decimal sumOfRequiredQty = 0;
//            DateTime shortTermEnd = sd.GetEndOfShortTerm();
//            if (sd.ScenarioOptions.ShortTermSpan.TotalDays <= 0)
//                return -1; //avoid divide by zero.
//            else
//            {
//                for (int i = 0; i < sd.JobManager.Count; i++)
//                {
//                    Job job = sd.JobManager.GetByIndex(i);
//                    if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled && job.ScheduledEndDate <= shortTermEnd)
//                        sumOfRequiredQty += job.SumOfUnfinishedMORequiredQties();
//                }
//                return sumOfRequiredQty / sd.ScenarioOptions.ShortTermSpanTotalDays;
//            }
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the average Setup Hours per day in the Short Term.
//    /// </summary>
//    public class AverageDailySetupHoursCalculator : IKpiCalculator
//    {
//        public AverageDailySetupHoursCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Daily Setup Hours";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The daily average of Setup Hours in the Short Term.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 13;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.OrangeRed;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N1"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            long sumOfSetupTicks = 0;
//            DateTime shortTermEnd = sd.GetEndOfShortTerm();
//            if (sd.ScenarioOptions.ShortTermSpan.TotalDays <= 0)
//                return -1; //avoid divide by zero.
//            else
//            {
//                for (int i = 0; i < sd.JobManager.Count; i++)
//                {
//                    Job job = sd.JobManager.GetByIndex(i);
//                    if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                        sumOfSetupTicks += job.SumOfSetupHoursTicks(shortTermEnd, false);
//                }
//                TimeSpan setupTS = new TimeSpan(sumOfSetupTicks);
//                decimal ret= (decimal)setupTS.TotalDays/(decimal)sd.ScenarioOptions.ShortTermSpan.TotalDays;
//                return ret;
//            }
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the total scheduled Setup Calendar Hours in the entire schedule.
//    /// </summary>
//    public class TotalSetupCalendarHoursCalculator : IKpiCalculator
//    {
//        public TotalSetupCalendarHoursCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Total Setup Calendar Hours";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The total scheduled Setup Hours in the entire schedule based on calendar time (independent of the number of Resources used during the setup).";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 14;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.DarkRed;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N1"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetTotalScheduledSetupHours(sd);
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the total scheduled Setup Cost for scheduled Activities.
//    /// </summary>
//    public class TotalSetupCostCalculator : IKpiCalculator
//    {
//        public TotalSetupCostCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Total Setup Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The total scheduled Setup Hours times Resource Standard Cost.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 43;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.DarkRed;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N1"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetSetupCosts(sd);
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the total scheduled Setup hours on Labor type Resources in the entire schedule.
//    /// </summary>
//    public class TotalSetupLaborHoursCalculator : IKpiCalculator
//    {
//        public TotalSetupLaborHoursCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Total Setup Labor Hours";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The total scheduled Setup Hours in the entire schedule based on the number of utilized Resources that have a Resource Type that is a type of person.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 32;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.MediumVioletRed;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N1"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            long sumOfSetupTicks = 0;

//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                    sumOfSetupTicks += job.SumOfSetupHoursTicks(DateTime.MaxValue, true);
//            }
//            TimeSpan ts= new TimeSpan(sumOfSetupTicks);
//            return (decimal)ts.TotalHours;
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the number of Hot Jobs that are Late.
//    /// </summary>
//    [Serializable]
//    public class LateHotJobsCalculator : IKpiCalculator
//    {
//        public LateHotJobsCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Late Hot Jobs";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The number of Hot Jobs that are Late.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 5;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Blue;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Integer;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            int tardyCnt = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (!job.Finished && job.Late && job.Hot && job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled) tardyCnt++;
//            }
//            return tardyCnt;
//        }

//        #endregion
//    }
//    /// <summary>
//    /// Calculates the number of Jobs that have their NeedDateTime in the past.
//    /// </summary>
//    [Serializable]
//    public class OverdueJobsCalculator : IKpiCalculator
//    {
//        public OverdueJobsCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Overdue Jobs";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The number of Jobs that have their NeedDateTime in the past.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 6;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Orange;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Integer;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            int overdueCnt = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (!job.Finished && job.Overdue) overdueCnt++;
//            }
//            return overdueCnt;
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the expected cost of carrying Finished Goods Inventory at a specific interest rate.
//    /// </summary>
//    [Serializable]
//    public class AverageLeadtimeCalculator : IKpiCalculator
//    {
//        public AverageLeadtimeCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Leadtime average (days)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The average number of days between the earliest scheduled operation start and latest scheduled operation finish.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 7;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Plum;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N1"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            int scheduledJobsCount = 0;
//            decimal leadTimeDays = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                {
//                    scheduledJobsCount++;
//                    leadTimeDays += (decimal)job.LeadTime.TotalDays;
//                }
//            }
//            if (scheduledJobsCount == 0)
//                return 0;
//            else
//                return (decimal)leadTimeDays / (decimal)scheduledJobsCount;
//        }

//        #endregion
//    }

//    /// <summary>
//    /// Calculates the expected cost of carrying Finished Goods Inventory at a specific interest rate.
//    /// </summary>
//    [Serializable]
//    public class WipCostCalculator : IKpiCalculator
//    {
//        public WipCostCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "WIP Carrying Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The sum of Operation Carrying Costs multiplied by the days between their start time and the Manufacturing Order end time.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 8;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.YellowGreen;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            decimal wipCarryingCost = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                    wipCarryingCost += job.GetWipCarryingCosts();
//            }

//            return (decimal)wipCarryingCost;
//        }

//        #endregion
//    }
//    /// <summary>
//    /// Calculates the cost of material and resources based on the days the Manufacturing Orders are finished early times the Plant APR.
//    /// </summary>
//    [Serializable]
//    public class FinishedGoodsCarryingCostCalculator : IKpiCalculator
//    {
//        public FinishedGoodsCarryingCostCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Finished Goods Carrying Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The sum of the Operation Daily Carrying Costs times the number of days early for the Manufacturing Order.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 9;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Green;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            return (decimal)KPICalculatorHelper.GetFinishedGoodsCarryingCosts(sd);
//        }

//        #endregion
//    }

//    public class ScheduleSpanCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Schedule Span (days)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The total time, expressed in calendar days, from the APS clock to the end of the final job.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 15;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Teal;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N1"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return (decimal)GetOverallSpan(sd).TotalDays;
//        }

//        public static TimeSpan GetOverallSpan(ScenarioDetail sd)
//        {
//            long latestJobEnd = DateTime.MinValue.Ticks;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager[i];
//                if (job.ScheduledStatus == PT.SchedulerDefinitions.JobDefs.scheduledStatuses.Scheduled && job.ScheduledEndDate.Ticks > latestJobEnd)
//                    latestJobEnd = job.ScheduledEndDate.Ticks;
//            }
//            if (latestJobEnd != DateTime.MinValue.Ticks)
//                return TimeSpan.FromTicks(latestJobEnd - sd.ClockDate.Ticks);
//            else
//                return new TimeSpan(0);
//        }

//        #endregion
//    }

//    public class ResourceUtilizationCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Resource Utilization (%)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The percent of available time (Online, Overtime or Potential Overtime) across the Schedule Span that is scheduled to be busy (with setup, run, etc.).";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 16;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Coral;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "P2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Percentage;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            TimeSpan scheduleSpan = ScheduleSpanCalculator.GetOverallSpan(sd);
//            if (scheduleSpan.Ticks > 0)
//            {
//                CapacityInfoBase.GroupChooser groupChooser = new CapacityInfoBase.GroupChooser(CapacityInfoBase.GroupChooser.valueTypes.UsageHours, CapacityInfoBase.GroupChooser.groupTypes.All, true, true, true, true);
//                System.Collections.Generic.Dictionary<BaseId, BaseId> resourcesToInclude = new System.Collections.Generic.Dictionary<BaseId, BaseId>();
//                System.Collections.Generic.List<Resource> resources = sd.PlantManager.GetResourceList();
//                for (int i = 0; i < resources.Count; i++)
//                {
//                    Resource resource = resources[i];
//                    resourcesToInclude.Add(resource.Id, resource.Id);
//                }

//                MultiResourceCapacityInfo capInfo = new MultiResourceCapacityInfo(sd, sd.ClockDate, sd.ClockDate.Add(scheduleSpan), scheduleSpan, resourcesToInclude, groupChooser);
//                TimeBucketList usagePercents = capInfo.GetUsagePercents();
//                if (usagePercents.Count > 0)
//                    return (decimal)usagePercents[0].TotalHours / 100;
//                else
//                    return 0;
//            }
//            else
//                return 0;
//        }

//        #endregion
//    }

//    public class DrumResourceUtilizationCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Drum Resource Utilization (%)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The percent of available time (Online, Overtime or Potential Overtime) across the Schedule Span on Resources marked as Drums that is scheduled to be busy (with setup, run, etc.).";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 17;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Crimson;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "P2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Percentage;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            TimeSpan scheduleSpan = ScheduleSpanCalculator.GetOverallSpan(sd);
//            if (scheduleSpan.Ticks > 0)
//            {
//                CapacityInfoBase.GroupChooser groupChooser = new CapacityInfoBase.GroupChooser(CapacityInfoBase.GroupChooser.valueTypes.UsageHours, CapacityInfoBase.GroupChooser.groupTypes.All, true, true, true, true);
//                System.Collections.Generic.Dictionary<BaseId, BaseId> resourcesToInclude = new System.Collections.Generic.Dictionary<BaseId, BaseId>();
//                System.Collections.Generic.List<Resource> resources = sd.PlantManager.GetResourceList();
//                for (int i = 0; i < resources.Count; i++)
//                {
//                    Resource resource = resources[i];
//                    if (resource.Drum)
//                        resourcesToInclude.Add(resource.Id, resource.Id);
//                }

//                MultiResourceCapacityInfo capInfo = new MultiResourceCapacityInfo(sd, sd.ClockDate, sd.ClockDate.Add(scheduleSpan), scheduleSpan, resourcesToInclude, groupChooser);
//                TimeBucketList usagePercents = capInfo.GetUsagePercents();
//                if (usagePercents.Count > 0)
//                    return (decimal)usagePercents[0].TotalHours / 100;
//                else
//                    return 0;
//            }
//            else
//                return 0;
//        }

//        #endregion
//    }

//    public class ThroughputCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "(T) Throughput";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Throughput for Jobs scheduled to end within the Planning Horizon.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 18;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.DarkGreen;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetJobThroughputs(sd);
//        }

//        #endregion
//    }

//    public class WIPInventoryCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "WIP Inventory Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of On-hand Inventory Cost for Items of Type Intermediate or SubAssembly.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 19;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.SaddleBrown;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetWipInventoryCost(sd);
//        }

//        #endregion
//    }

//    public class RawMaterialInventoryCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Raw Material Inventory Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of On-hand Inventory Cost for Items of Type Raw Material.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 20;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.RosyBrown;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetRawMaterialInventoryCost(sd);
//        }

//        #endregion
//    }

//    public class FinishedGoodsInventoryCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Finished Goods Inventory Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of On-hand Inventory Cost for Items of Type Finished Goods.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 21;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.RosyBrown;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetFinishedGoodsInventoryCost(sd);
//        }

//        #endregion
//    }

//    public class JobRawMaterialCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Job Raw Material Inventory Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Material costs for Material Items that have Type Raw Material for scheduled Jobs.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 22;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.SandyBrown;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetJobRawMaterialInventoryCost(sd);
//        }

//        #endregion
//    }

//    public class InventoryCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Inventory Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Job Raw Material and On-hand Raw Material, WIP and Finished Goods Inventory costs.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 23;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.GreenYellow;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetInventoryCost(sd);
//        }

//        #endregion
//    }

//    public class InvestmentCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "(I) Investment";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Plant Investments and Inventory Costs.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 24;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.LawnGreen;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetInvestment(sd);
//        }

//        #endregion
//    }

//    public class ResourceOnlineCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Resource Online Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "NonOvertime Hours times Standard Hourly Cost for Active Resources.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 25;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.BlueViolet;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return sd.PlantManager.GetResourceOnlineNonOvertimeCost();
//        }

//        #endregion
//    }

//    public class ResourceOvertimeCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Resource Overtime Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Overtime Hours times Overtime Hourly Cost for Active Resources.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 26;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.OrangeRed;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }
//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return sd.PlantManager.GetResourceOvertimeCost();
//        }

//        #endregion
//    }

//    public class SubcontractCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Subcontract Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Subcontract costs for scheduled Jobs.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 27;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.PeachPuff;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetSubcontractCosts(sd);
//        }

//        #endregion
//    }

//    public class OperatingExpenseCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "(OE) Operating Expense";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Plant operating expenses, Resource Online Cost, Overtime Cost and Subcontract Cost.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 28;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.MediumVioletRed;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetOperatingExpense(sd);
//        }

//        #endregion
//    }

//    public class ProductivityCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "(T/OE) Productivity";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Throughput divided by Operating Expense.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 29;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.MistyRose;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            decimal expense = KPICalculatorHelper.GetOperatingExpense(sd);
//            if (expense != 0)
//                return KPICalculatorHelper.GetJobThroughputs(sd) / expense;
//            else
//                return 0;
//        }

//        #endregion
//    }

//    public class InvestmentTurnsCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "(T/I) Investment Turns";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Throughput divided Investment.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 30;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Tan;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            decimal investment = KPICalculatorHelper.GetInvestment(sd);
//            if (investment != 0)
//                return KPICalculatorHelper.GetJobThroughputs(sd) / KPICalculatorHelper.GetInvestment(sd);
//            else
//                return 0;
//        }

//        #endregion
//    }

//    public class NetProfitCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "(T-OE) Net Profit";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Throughput minus Operating Expense.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 37;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Thistle;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetNetProfit(sd);
//        }

//        #endregion
//    }

//    public class ReturnOnInvestmentCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "(NP/I) Return On Investment";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Net Profit divided by Investment.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 31;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Tomato;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            decimal investment = KPICalculatorHelper.GetInvestment(sd);
//            if (investment != 0)
//                return KPICalculatorHelper.GetNetProfit(sd) / investment;
//            else
//                return 0;
//        }

//        #endregion
//    }

//    public class ThroughputPerDrumHourCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Throughput per Drum Hour";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Throughput of Drum Jobs divided by the hours of Drum capacity they use.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 39;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Aquamarine;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }
//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            ResourceArrayList resources = sd.PlantManager.GetResourceArrayList();
//            long drumTicks = 0;
//            decimal drumThroughput = 0;
//            Dictionary<BaseId, BaseId> jobIdsAlreadyCountedForThroughput = new Dictionary<BaseId, BaseId>();

//            for (int r = 0; r < resources.Count; r++)
//            {
//                Resource resource = resources[r];
//                if (resource.Drum)
//                {
//                    ResourceBlockList.Node node = resource.Blocks.First;
//                    while (node != null)
//                    {
//                        Batch batch = node.Data.Batch;
//                        IEnumerator<InternalActivity> batchActivityIterator = batch.GetEnumerator();
//                        while (batchActivityIterator.MoveNext())
//                        {
//                            InternalActivity activity = batchActivityIterator.Current;
//                            drumTicks += activity.ScheduledSetupTicks + activity.ScheduledRunTicks + batch.PostProcessingSpan.TimeSpanTicks + batch.StorageTimeSpan.Ticks + batch.StoragePostProcessingSpan.Ticks;

//                            Job job = activity.Operation.ManufacturingOrder.Job;
//                            if (!jobIdsAlreadyCountedForThroughput.ContainsKey(job.Id))
//                            {
//                                drumThroughput += job.Throughput;
//                                jobIdsAlreadyCountedForThroughput.Add(job.Id, job.Id);
//                            }
//                        }
//                        node = node.Next;
//                    }
//                }
//            }
//            if (drumTicks == 0)
//                return 0;
//            else
//                return drumThroughput / (decimal)TimeSpan.FromTicks(drumTicks).TotalHours;
//        }

//        #endregion
//    }

//    public class LateThroughputCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Throughput Late";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Throughput for open Late Jobs.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 33;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Bisque;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = false;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }
//        public string FormatString
//        {
//            get { return "C0"; }
//        }
//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            decimal sum = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager[i];
//                if (job.Late && !job.Finished && !job.Cancelled)
//                    sum += job.Throughput;
//            }
//            return sum;
//        }

//        #endregion
//    }

//    public class ScheduledMachineCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Scheduled Machine Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Machine Cost from all running operations";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 34;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.DarkSlateGray;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            decimal totalMachineCost = 0;

//            if (sd.JobManager.NonTemplateJobsCount == 0) return totalMachineCost;

//            foreach (Job j in sd.JobManager)
//            {
//                if (j.Template) continue;

//                totalMachineCost += j.MachineCost;
//            }

//            return totalMachineCost;
//        }
//        #endregion
//    }

//    public class ScheduledLaborCostCalculator : IKpiCalculator
//    {
//        #region IKpiCalculator Members
//        string name = "Scheduled Labor Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Sum of Labor Cost from all running operations";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 35;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.BlueViolet;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            decimal totalLaborCost = 0;

//            if (sd.JobManager.NonTemplateJobsCount == 0) return totalLaborCost;

//            foreach (Job j in sd.JobManager)
//            {
//                if (j.Template) continue;

//                totalLaborCost += j.LaborCost;
//            }

//            return totalLaborCost;
//        }
//        #endregion
//    }

//    public class LatePenaltyCalculator : IKpiCalculator
//    {
//        public LatePenaltyCalculator()
//        {
//        }
//        #region IKpiCalculator Members

//        string name = "Late Penalty";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Job Lateness x Job Late Penalty (for Late scheduled Jobs).";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 36;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Gold;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            return (decimal)KPICalculatorHelper.GetLatePenaltyCosts(sd);
//        }

//        #endregion
//    }

//    public class LatePenaltySetupInventoryCarryingCostCalculator : IKpiCalculator
//    {
//        public LatePenaltySetupInventoryCarryingCostCalculator()
//        {
//        }
//        #region IKpiCalculator Members

//        string name = "Late Penalty + Setup + FG Carrying Cost";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Job Lateness x Job Late Penalty plus Job Setup Cost plus finished goods Inventory Carrying Cost for scheduled Jobs.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 38;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Orange;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "C2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            return (decimal)KPICalculatorHelper.GetLatePenaltyCosts(sd) + (decimal)KPICalculatorHelper.GetSetupCosts(sd) + (decimal)KPICalculatorHelper.GetFinishedGoodsCarryingCosts(sd); ;
//        }

//        #endregion
//    }

//    public class AverageOperationBufferPenetrationPercentCalculator : IKpiCalculator
//    {
//        public AverageOperationBufferPenetrationPercentCalculator()
//        {
//        }
//        #region IKpiCalculator Members

//        string name = "Average Operation Buffer Penetration (%)";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Average Buffer Penetration for all scheduled Operations.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 40;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.OrangeRed;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "P2"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Percentage;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            return (decimal)KPICalculatorHelper.GetAverageOperationBufferPenetrationPercent(sd);
//        }

//        #endregion
//    }

//    public class BufferPenetrationAndSetupHoursCalculator : IKpiCalculator
//    {
//        public BufferPenetrationAndSetupHoursCalculator()
//        {
//        }
//        #region IKpiCalculator Members

//        string name = "Buffer Penetration and Total Setup Hours";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Weighted sum of Average Operation Buffer Penetration, Setup Hours, and Attribute Code changes. Weights are controlled by the Optimize Rule named 'KPI' if it exists, using the Buffer Penetration, Setup Hours and Setup Code sliders. If no such rule exists then one percent of Average Buffer Penetration is weighted equally to one hour of setup time and one point of Attribute Code change. Attribute Code weights are based on individual Resource Optimize Rule Attribute Code factors.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 41;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.PowderBlue;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N0"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail sd)
//        {
//            DispatcherDefinition dispatcher = sd.DispatcherDefinitionManager.GetByName("KPI");
//            decimal penetrationWeight = 1;
//            decimal setupHourWeight = 1;
//            decimal attributesWeight = 1;
//            if (dispatcher != null && dispatcher is BalancedCompositeDispatcherDefinition)
//            {
//                BalancedCompositeDispatcherDefinition balancedDispatcher = (BalancedCompositeDispatcherDefinition)dispatcher;
//                penetrationWeight = (decimal)balancedDispatcher.BufferPenetrationWeight;
//                setupHourWeight = (decimal)balancedDispatcher.SetupHoursWeight;
//                attributesWeight = (decimal)balancedDispatcher.SameSetupCodeWeight;
//            }
//            return penetrationWeight * KPICalculatorHelper.GetAverageOperationBufferPenetrationPercent(sd) + setupHourWeight * KPICalculatorHelper.GetTotalScheduledSetupHours(sd) + attributesWeight * GetAttributesWeight(sd);
//        }

//        decimal GetAttributesWeight(ScenarioDetail sd)
//        {
//            //Only include Resources using an Optimize rule that uses Attributes.
//            List<Resource> resources = sd.PlantManager.GetResourceList();
//            decimal score = 0;
//            for (int resI = 0; resI < resources.Count; resI++)
//            {
//                Resource resource = resources[resI];
//                if (resource.Dispatcher != null && resource.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
//                {
//                    BalancedCompositeDispatcherDefinition resDispatcher = (BalancedCompositeDispatcherDefinition)resource.Dispatcher.DispatcherDefinition;
//                    if (resDispatcher != null && resDispatcher.OperationAttributeInfos.Count > 0)
//                    {
//                        score += GetResourceAttributeScore(resource, resDispatcher);
//                    }
//                }
//            }
//            return score;
//        }

//        decimal GetResourceAttributeScore(Resource aResource, BalancedCompositeDispatcherDefinition aDispatcher)
//        {
//            ResourceBlockList.Node node = aResource.Blocks.First;
//            ResourceBlockList.Node prevNode = null;
//            int score = 0;

//            while (node != null)
//            {
//                ResourceBlock block = node.Data;
//                prevNode = node;
//                node = node.Next;
//                if (prevNode != null && node != null)
//                    score += GetAttributeChangeScore(prevNode.Data.Activity.Operation.Attributes, node.Data.Activity.Operation.Attributes, aDispatcher);
//            }
//            return score;
//        }

//        int GetAttributeChangeScore(AttributesCollection aPredAttributes, AttributesCollection aSucAttributes, BalancedCompositeDispatcherDefinition aDispatcher)
//        {
//            List<Transmissions.BalancedCompositeDispatcherDefinitionUpdateT.AttributeRuleInfo> attInfos = aDispatcher.OperationAttributeInfos;
//            int score = 0;
//            for (int i = 0; i < attInfos.Count; i++)
//            {
//                Transmissions.BalancedCompositeDispatcherDefinitionUpdateT.AttributeRuleInfo attInfo = attInfos[i];
//                if (attInfo.OptimizeType == PtAttributeDefs2.OptimizeType.SameAttributeCode && attInfo.Weight != 0)
//                {
//                    PTAttribute predAtt = aPredAttributes.Find(attInfo.AttributeName);
//                    PTAttribute sucAtt = aSucAttributes.Find(attInfo.AttributeName);
//                    if (predAtt != null && sucAtt != null && predAtt.Code != sucAtt.Code)
//                        score = score + (int)attInfo.Weight;
//                }
//            }
//            return score;
//        }

//        #endregion
//    }

//    public class LastAdjustmentCalculator : IKpiCalculator
//    {
//        public LastAdjustmentCalculator()
//        {
//        }
//        #region IKpiCalculator Members

//        string name = "Last Adjustment Timing";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "Time in minutes that the last schedule adjustment took to process";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 42;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.SaddleBrown;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }

//        public string FormatString
//        {
//            get { return "N3"; }
//        }

//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Double;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        public decimal Calculate(ScenarioDetail a_sd)
//        {
//            PT.Common.Collections.CircularQueue<PT.Common.Testing.Timing> timingQueue = a_sd.GetCopyOfTransmissionTimingQueue();
//            if (timingQueue.Count > 0)
//            {
//                PT.Common.Testing.Timing timing = null;
//                while (timingQueue.Count > 0)
//                {
//                    timing = timingQueue.Dequeue();
//                }
//                return (decimal)TimeSpan.FromTicks(timing.Length).TotalSeconds;
//            }
//            else
//            {
//                return 0;
//            }
//        }

//        #endregion
//    }

//    public class KPICalculatorHelper
//    {
//        public static decimal GetJobThroughputs(ScenarioDetail sd)
//        {
//            decimal t = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager[i];
//                if (job.Scheduled && job.ScheduledEndDate.Ticks < sd.EndOfPlanningHorizon)
//                    t += job.Throughput;
//            }
//            return t;
//        }

//        public static decimal GetWipInventoryCost(ScenarioDetail sd)
//        {
//            decimal wipCost = 0;
//            for (int wI = 0; wI < sd.WarehouseManager.Count; wI++)
//            {
//                Warehouse warehouse = sd.WarehouseManager.GetByIndex(wI);
//                IEnumerator<InventoryManager.ItemInventory> enumerator = warehouse.Inventories.GetEnumerator();

//                while (enumerator.MoveNext())
//                {
//                    Inventory inventory = enumerator.Current.m_inventory;
//                    if (inventory.OnHandQty > 0 && inventory.Item.Cost != 0 && (inventory.Item.ItemType == ItemDefs.itemTypes.Intermediate || inventory.Item.ItemType == ItemDefs.itemTypes.SubAssembly))
//                        wipCost += Convert.ToDecimal(inventory.OnHandQty) * inventory.Item.Cost;
//                }
//            }

//            return wipCost;
//        }

//        public static decimal GetRawMaterialInventoryCost(ScenarioDetail sd)
//        {
//            decimal invCost = 0;
//            for (int wI = 0; wI < sd.WarehouseManager.Count; wI++)
//            {
//                Warehouse warehouse = sd.WarehouseManager.GetByIndex(wI);
//                IEnumerator<InventoryManager.ItemInventory> enumerator = warehouse.Inventories.GetEnumerator();

//                while (enumerator.MoveNext())
//                {
//                    Inventory inventory = enumerator.Current.m_inventory;
//                    if (inventory.OnHandQty > 0 && inventory.Item.Cost != 0 && inventory.Item.ItemType == ItemDefs.itemTypes.RawMaterial)
//                        invCost += Convert.ToDecimal(inventory.OnHandQty) * inventory.Item.Cost;
//                }
//            }

//            return invCost;
//        }

//        public static decimal GetFinishedGoodsInventoryCost(ScenarioDetail sd)
//        {
//            decimal invCost = 0;
//            for (int wI = 0; wI < sd.WarehouseManager.Count; wI++)
//            {
//                Warehouse warehouse = sd.WarehouseManager.GetByIndex(wI);
//                IEnumerator<InventoryManager.ItemInventory> enumerator = warehouse.Inventories.GetEnumerator();

//                while (enumerator.MoveNext())
//                {
//                    Inventory inventory = enumerator.Current.m_inventory;
//                    if (inventory.OnHandQty > 0 && inventory.Item.Cost != 0 && inventory.Item.ItemType == ItemDefs.itemTypes.FinishedGood)
//                        invCost += Convert.ToDecimal(inventory.OnHandQty) * inventory.Item.Cost;
//                }
//            }

//            return invCost;
//        }

//        public static decimal GetJobRawMaterialInventoryCost(ScenarioDetail sd)
//        {
//            decimal rmCost = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager[i];
//                if (job.Scheduled)
//                    rmCost += job.RawMaterialCost;
//            }
//            return rmCost;
//        }

//        public static decimal GetInventoryCost(ScenarioDetail sd)
//        {
//            decimal rmCost = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager[i];
//                if (job.Scheduled)
//                    rmCost += job.RawMaterialCost;
//            }
//            return rmCost + GetRawMaterialInventoryCost(sd) + GetWipInventoryCost(sd) + GetFinishedGoodsInventoryCost(sd);
//        }

//        /// <summary>
//        /// Sum of Subcontract costs for scheduled Jobs.
//        /// </summary>
//        /// <param name="sd"></param>
//        /// <returns></returns>
//        public static decimal GetSubcontractCosts(ScenarioDetail sd)
//        {
//            decimal subcontractCost = 0;
//            for (int j = 0; j < sd.JobManager.Count; j++)
//            {
//                Job job = sd.JobManager[j];
//                if (job.Scheduled)
//                    subcontractCost += job.SubcontractCost;
//            }
//            return subcontractCost;
//        }

//        /// <summary>
//        /// Sum of Setup Cost across all scheduled Jobs.
//        /// </summary>
//        /// <param name="sd"></param>
//        /// <returns></returns>
//        public static decimal GetSetupCosts(ScenarioDetail sd)
//        {
//            decimal cost = 0;
//            for (int j = 0; j < sd.JobManager.Count; j++)
//            {
//                Job job = sd.JobManager[j];
//                if (job.Scheduled)
//                    cost += job.SetupCost;
//            }
//            return cost;
//        }

//        /// <summary>
//        /// Sum of carrying cost for manufacturing orders finished early.
//        /// </summary>
//        /// <param name="sd"></param>
//        /// <returns></returns>
//        public static decimal GetFinishedGoodsCarryingCosts(ScenarioDetail sd)
//        {
//            decimal fgCost = 0;
//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                    fgCost += job.GetFinishedGoodsCarryingCosts();
//            }
//            return (decimal)fgCost;
//        }

//        /// <summary>
//        /// Sum of Lateness times Late Penalty Cost across all Late Scheduled Jobs.
//        /// </summary>
//        /// <param name="sd"></param>
//        /// <returns></returns>
//        public static decimal GetLatePenaltyCosts(ScenarioDetail sd)
//        {
//            decimal cost = 0;
//            for (int j = 0; j < sd.JobManager.Count; j++)
//            {
//                Job job = sd.JobManager[j];
//                if (!job.Finished && job.Scheduled && job.Late)
//                    cost += (decimal)job.Lateness.TotalDays * job.LatePenaltyCost;
//            }
//            return cost;
//        }

//        public static decimal GetOperatingExpense(ScenarioDetail sd)
//        {
//            return sd.PlantManager.GetOperatingExpensesForOnlineDaysAcrossPlants() +
//               sd.PlantManager.GetResourceOnlineNonOvertimeCost() +
//               sd.PlantManager.GetResourceOvertimeCost() +
//               KPICalculatorHelper.GetSubcontractCosts(sd);
//        }

//        public static decimal GetInvestment(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetInventoryCost(sd) + sd.PlantManager.GetPlantsInvestedCapital();
//        }

//        public static decimal GetNetProfit(ScenarioDetail sd)
//        {
//            return KPICalculatorHelper.GetJobThroughputs(sd) - KPICalculatorHelper.GetOperatingExpense(sd);
//        }

//        public static decimal GetAverageOperationBufferPenetrationPercent(ScenarioDetail sd)
//        {
//            ResourceArrayList resources = sd.PlantManager.GetResourceArrayList();
//            decimal totalPenetration = 0;
//            int operationCount = 0;
//            for (int r = 0; r < resources.Count; r++)
//            {
//                Resource resource = resources[r];
//                ResourceBlockList.Node node = resource.Blocks.First;
//                while (node != null)
//                {
//                    Batch batch = node.Data.Batch;
//                    IEnumerator<InternalActivity> batchActivityIterator = batch.GetEnumerator();
//                    while (batchActivityIterator.MoveNext())
//                    {
//                        InternalActivity activity = batchActivityIterator.Current;
//                        totalPenetration += activity.Operation.ProjectedBufferPenetrationPercent;
//                        operationCount++;
//                    }
//                    node = node.Next;
//                }
//            }
//            if (operationCount > 0)
//                return totalPenetration / (decimal)operationCount;
//            else
//                return 0;
//        }

//        public static decimal GetTotalScheduledSetupHours(ScenarioDetail sd)
//        {
//            long sumOfSetupTicks = 0;

//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                    sumOfSetupTicks += job.SumOfSetupHoursTicks(DateTime.MaxValue, false);
//            }
//            return (decimal)new TimeSpan(sumOfSetupTicks).TotalHours;
//        }

//        public static decimal GetTotalScheduledSetupCost(ScenarioDetail sd)
//        {
//            decimal sumOfSetupCost = 0;

//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
//                    sumOfSetupCost += (decimal)job.SumOfSetupCosts(DateTime.MaxValue, false);
//            }
//            return sumOfSetupCost;
//        }

//        public static Tuple<int, List<Job>> GetLateSalesOrderJobs(ScenarioDetail a_sd)
//        {
//            List<Job> lateJobs = new List<Job>();
//            int total = 0;
//            foreach (var job in a_sd.JobManager)
//            {
//                if (!job.Finished && (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled) && job.Classification == JobDefs.classifications.SalesOrder)
//                {
//                    if (job.Late)
//                    {
//                        lateJobs.Add(job);
//                    }

//                    total++;
//                }
//            }

//            return new Tuple<int, List<Job>>(total, lateJobs);
//        }

//        public static Tuple<int, List<Job>> GetOverdueSalesOrderJobs(ScenarioDetail a_sd)
//        {
//            List<Job> lateJobs = new List<Job>();
//            int total = 0;
//            foreach (var job in a_sd.JobManager)
//            {
//                if ((job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled) && job.Classification == JobDefs.classifications.SalesOrder)
//                {
//                    if (job.Overdue)
//                    {
//                        lateJobs.Add(job);
//                    }

//                    total++;
//                }
//            }

//            return new Tuple<int, List<Job>>(total, lateJobs);
//        }

//        public static Tuple<int, List<SalesOrder>> GetLateSalesOrders(ScenarioDetail a_sd)
//        {
//            List<SalesOrder> lateSos = new List<SalesOrder>();
//            int total = 0;
//            foreach (SalesOrder salesOrder in a_sd.SalesOrderManager)
//            {
//                if (salesOrder.Cancelled)
//                {
//                    continue;
//                }
//                total++;
//                foreach (SalesOrderLine line in salesOrder.SalesOrderLines)
//                {
//                    int ontimeDistributions = 0;
//                    foreach (SalesOrderLineDistribution distribution in line.LineDistributions)
//                    {
//                        if (distribution.Closed || distribution.QtyOpenToShip == 0)
//                        {
//                            //Distribution is finished
//                            ontimeDistributions++;
//                            continue;
//                        }

//                        //Find the adjustment so we can see when the material is actually available
//                        Warehouse warehouse = distribution.MustSupplyFromWarehouse;
//                        Inventory inventory = warehouse.Inventories[line.Item.Id];
//                        if (inventory != null)
//                        {
//                            AdjustmentArray adjustmentArray = inventory.GetAdjustmentArray();
//                            for (var i = 0; i < adjustmentArray.Count; i++)
//                            {
//                                Adjustment adjustment = adjustmentArray[i];
//                                if (adjustment.Reason is SalesOrderLineDistribution dist)
//                                {
//                                    if (dist.Id == distribution.Id)
//                                    {
//                                        if (adjustment.AdjDate <= distribution.RequiredAvailableDate)
//                                        {
//                                            ontimeDistributions++;
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }

//                    //Not all distributions are on time
//                    if (ontimeDistributions != line.LineDistributions.Count)
//                    {
//                        lateSos.Add(salesOrder);
//                        break;
//                    }
//                }
//            }

//            return new Tuple<int, List<SalesOrder>>(total, lateSos);
//        }

//        public static Tuple<int, List<SalesOrder>> GetOverdueSalesOrders(ScenarioDetail a_sd)
//        {
//            DateTime limit = DateTime.UtcNow;
//            List<SalesOrder> lateSos = new List<SalesOrder>();
//            int total = 0;
//            foreach (SalesOrder salesOrder in a_sd.SalesOrderManager)
//            {
//                if (salesOrder.Cancelled)
//                {
//                    continue;
//                }
//                total++;

//                foreach (SalesOrderLine line in salesOrder.SalesOrderLines)
//                {
//                    int ontimeDistributions = 0;

//                    foreach (SalesOrderLineDistribution distribution in line.LineDistributions)
//                    {
//                        if (distribution.RequiredAvailableDate >= limit || distribution.Closed || distribution.QtyOpenToShip == 0)
//                        {
//                            //Distribution is finished
//                            ontimeDistributions++;
//                            continue;
//                        }

//                        //Find the adjustment so we can see when the material is actually available
//                        Warehouse warehouse = distribution.MustSupplyFromWarehouse;
//                        Inventory inventory = warehouse.Inventories[line.Item.Id];
//                        if (inventory != null)
//                        {
//                            AdjustmentArray adjustmentArray = inventory.GetAdjustmentArray();
//                            for (var i = 0; i < adjustmentArray.Count; i++)
//                            {
//                                Adjustment adjustment = adjustmentArray[i];
//                                if (adjustment.Reason is SalesOrderLineDistribution dist)
//                                {
//                                    if (dist.Id == distribution.Id)
//                                    {
//                                        if (adjustment.AdjDate < limit)
//                                        {
//                                            //Late
//                                            ontimeDistributions++;
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }

//                    //Not all distributions are on time
//                    if (ontimeDistributions != line.LineDistributions.Count)
//                    {
//                        lateSos.Add(salesOrder);
//                        break;
//                    }
//                }
//            }

//            return new Tuple<int, List<SalesOrder>>(total, lateSos);
//        }
//    }

//    [Serializable]
//    public class OnTimeSalesOrdersCalculator : IKpiCalculator
//    {
//        string name = "On-Time Sales Orders (%)";
//        string description = "Percent of scheduled Sales Order Jobs on-time.";
//        int id = 44;
//        Color plotColor = Color.DarkKhaki;
//        bool lowerIsBetter = false;
//        string formatString = "P2";
//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        KPI.valueDisplayTypes valueDisplayTypes = KPI.valueDisplayTypes.Percentage;

//        public OnTimeSalesOrdersCalculator() {}

//        #region IKpiCalculator Members

//        public string Name
//        {
//            get
//            {
//                return name;
//            }

//            set
//            {
//                name = value;
//            }
//        }

//        public string Description
//        {
//            get
//            {
//                return description;
//            }

//            set
//            {
//                description = value;
//            }
//        }

//        public int Id
//        {
//            get
//            {
//                return id;
//            }

//            set
//            {
//                id = value;
//            }
//        }

//        public Color PlotColor
//        {
//            get
//            {
//                return plotColor;
//            }

//            set
//            {
//                plotColor = value;
//            }
//        }

//        public bool LowerIsBetter
//        {
//            get
//            {
//                return lowerIsBetter;
//            }

//            set
//            {
//                lowerIsBetter = value;
//            }
//        }

//        public string FormatString
//        {
//            get
//            {
//                return formatString;
//            }
//        }

//        public decimal ChartScaleMultiplier
//        {
//            get
//            {
//                return chartScaleMultiplier;
//            }

//            set
//            {
//                chartScaleMultiplier = value;
//            }
//        }

//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get
//            {
//                return valueDisplayTypes;
//            }

//            set
//            {
//                valueDisplayTypes = value;
//            }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail a_sd)
//        {
//            decimal totalNumberSalesOrders = 0;
//            decimal totalNumberLate = 0;

//            Tuple<int, List<Job>> lateSalesOrderJobs = KPICalculatorHelper.GetLateSalesOrderJobs(a_sd);
//            totalNumberLate += lateSalesOrderJobs.Item2.Count;
//            totalNumberSalesOrders += lateSalesOrderJobs.Item1;

//            Tuple<int, List<SalesOrder>> salesOrders = KPICalculatorHelper.GetLateSalesOrders(a_sd);
//            totalNumberLate += salesOrders.Item2.Count;
//            totalNumberSalesOrders += salesOrders.Item1;

//            if (totalNumberSalesOrders == 0)
//            {
//                return 0m;
//            }

//            return (totalNumberSalesOrders - totalNumberLate) / totalNumberSalesOrders;
//        }
//        #endregion
//    }

//    [Serializable]
//    public class LateSalesOrderRevenueCalculator : IKpiCalculator
//    {
//        public LateSalesOrderRevenueCalculator()
//        {
//        }
//        #region IKpiCalculator Members
//        string name = "Late Sales Order Revenue";
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        string description = "The sum of Revenue from Sales Order Jobs that are scheduled late.";
//        public string Description
//        {
//            get { return description; }
//            set { description = value; }
//        }

//        int id = 45;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        Color plotColor = Color.Crimson;
//        public Color PlotColor
//        {
//            get { return plotColor; }
//            set { plotColor = value; }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get { return lowerIsBetter; }
//            set { lowerIsBetter = value; }
//        }
//        public string FormatString
//        {
//            get { return "C2"; }
//        }
//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get { return chartScaleMultiplier; }
//            set { chartScaleMultiplier = value; }
//        }

//        KPI.valueDisplayTypes valueDisplayTpe = KPI.valueDisplayTypes.Currency;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get { return valueDisplayTpe; }
//            set { valueDisplayTpe = value; }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail a_sd)
//        {
//            decimal totalLateSalesOrderRevenue = 0;
//            Tuple<int, List<Job>> lateSalesOrderJobs = KPICalculatorHelper.GetLateSalesOrderJobs(a_sd);
//            foreach (Job job in lateSalesOrderJobs.Item2)
//            {
//                totalLateSalesOrderRevenue += job.Revenue;
//            }

//            return totalLateSalesOrderRevenue;
//        }
//        #endregion
//    }

//    [Serializable]
//    public class LateSalesOrdersCalculator : IKpiCalculator
//    {
//        #region KPI Calculator
//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get
//            {
//                return chartScaleMultiplier;
//            }

//            set
//            {
//                chartScaleMultiplier = value;
//            }
//        }

//        string description = "The number of Sales Order Jobs that are late.";
//        public string Description
//        {
//            get
//            {
//                return description;
//            }

//            set
//            {
//                description = value;
//            }
//        }

//        public string FormatString
//        {
//            get
//            {
//                return "F0";
//            }
//        }

//        int id = 46;
//        public int Id
//        {
//            get
//            {
//                return id;
//            }

//            set
//            {
//                id = value;
//            }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get
//            {
//                return lowerIsBetter;
//            }

//            set
//            {
//                lowerIsBetter = value;
//            }
//        }

//        string name = "Late Sales Orders";
//        public string Name
//        {
//            get
//            {
//                return name;
//            }

//            set
//            {
//                name = value;
//            }
//        }

//        Color plotColor = Color.Magenta;
//        public Color PlotColor
//        {
//            get
//            {
//                return plotColor;
//            }

//            set
//            {
//                plotColor = value;
//            }
//        }

//        KPI.valueDisplayTypes valueDisplayType = KPI.valueDisplayTypes.Integer;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get
//            {
//                return valueDisplayType;
//            }

//            set
//            {
//                valueDisplayType = value;
//            }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail a_sd)
//        {
//            int totalNumberLateSalesOrders = 0;

//            Tuple<int, List<Job>> lateSalesOrderJobs = KPICalculatorHelper.GetLateSalesOrderJobs(a_sd);
//            totalNumberLateSalesOrders += lateSalesOrderJobs.Item2.Count;

//            Tuple<int, List<SalesOrder>> salesOrders = KPICalculatorHelper.GetLateSalesOrders(a_sd);
//            totalNumberLateSalesOrders += salesOrders.Item2.Count;

//            return totalNumberLateSalesOrders;
//        }
//        #endregion LateSalesOrderCalculator
//    }

//    [Serializable]
//    public class LateUnitsCalculator : IKpiCalculator
//    {
//        #region KPI Calculator
//        decimal chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get
//            {
//                return chartScaleMultiplier;
//            }

//            set
//            {
//                chartScaleMultiplier = value;
//            }
//        }

//        string description = "Sum of job units that are late, and open.";
//        public string Description
//        {
//            get
//            {
//                return description;
//            }

//            set
//            {
//                description = value;
//            }
//        }

//        public string FormatString
//        {
//            get
//            {
//                return "N1";
//            }
//        }

//        int id = 47;
//        public int Id
//        {
//            get
//            {
//                return id;
//            }

//            set
//            {
//                id = value;
//            }
//        }

//        bool lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get
//            {
//                return lowerIsBetter;
//            }

//            set
//            {
//                lowerIsBetter = value;
//            }
//        }

//        string name = "Late Units";
//        public string Name
//        {
//            get
//            {
//                return name;
//            }

//            set
//            {
//                name = value;
//            }
//        }

//        Color plotColor = Color.DarkTurquoise;
//        public Color PlotColor
//        {
//            get
//            {
//                return plotColor;
//            }

//            set
//            {
//                plotColor = value;
//            }
//        }

//        KPI.valueDisplayTypes valueDisplayType = KPI.valueDisplayTypes.Integer;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get
//            {
//                return valueDisplayType;
//            }

//            set
//            {
//                valueDisplayType = value;
//            }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail sd)
//        {
//            decimal totalNumberLateUnits = 0;

//            for (int i = 0; i < sd.JobManager.Count; i++)
//            {
//                Job job = sd.JobManager.GetByIndex(i);
//                if ((job.Late) && (job.Open))
//                {
//                    totalNumberLateUnits += job.Qty;
//                }
//            }
//            return totalNumberLateUnits;
//        }
//        #endregion LateUnits
//    }

//    [Serializable]
//    public class OverdueSalesOrdersCountCalculator : IKpiCalculator
//    {
//        #region KPI Calculator
//        decimal m_chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get
//            {
//                return m_chartScaleMultiplier;
//            }

//            set
//            {
//                m_chartScaleMultiplier = value;
//            }
//        }

//        string m_description = "The number of Sales Orders that are overdue.";
//        public string Description
//        {
//            get
//            {
//                return m_description;
//            }

//            set
//            {
//                m_description = value;
//            }
//        }

//        public string FormatString
//        {
//            get
//            {
//                return "F0";
//            }
//        }

//        int m_id = 48;
//        public int Id
//        {
//            get
//            {
//                return m_id;
//            }

//            set
//            {
//                m_id = value;
//            }
//        }

//        bool m_lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get
//            {
//                return m_lowerIsBetter;
//            }

//            set
//            {
//                m_lowerIsBetter = value;
//            }
//        }

//        string m_name = "Overdue Sales Orders";
//        public string Name
//        {
//            get
//            {
//                return m_name;
//            }

//            set
//            {
//                m_name = value;
//            }
//        }

//        Color m_plotColor = Color.MidnightBlue;
//        public Color PlotColor
//        {
//            get
//            {
//                return m_plotColor;
//            }

//            set
//            {
//                m_plotColor = value;
//            }
//        }

//        KPI.valueDisplayTypes m_valueDisplayType = KPI.valueDisplayTypes.Integer;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get
//            {
//                return m_valueDisplayType;
//            }

//            set
//            {
//                m_valueDisplayType = value;
//            }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail a_sd)
//        {
//            int totalNumberLateSalesOrders = 0;

//            Tuple<int, List<Job>> lateSalesOrderJobs = KPICalculatorHelper.GetOverdueSalesOrderJobs(a_sd);
//            totalNumberLateSalesOrders += lateSalesOrderJobs.Item2.Count;

//            Tuple<int, List<SalesOrder>> salesOrders = KPICalculatorHelper.GetOverdueSalesOrders(a_sd);
//            totalNumberLateSalesOrders += salesOrders.Item2.Count;

//            return totalNumberLateSalesOrders;
//        }
//        #endregion LateSalesOrderCalculator
//    }

//    [Serializable]
//    public class OverdueSalesOrdersPercentCalculator : IKpiCalculator
//    {
//        #region KPI Calculator
//        decimal m_chartScaleMultiplier = KPICalculator.DefaultMultiplier;
//        public decimal ChartScaleMultiplier
//        {
//            get
//            {
//                return m_chartScaleMultiplier;
//            }

//            set
//            {
//                m_chartScaleMultiplier = value;
//            }
//        }

//        string m_description = "The percentage of Sales Orders that are overdue.";
//        public string Description
//        {
//            get
//            {
//                return m_description;
//            }

//            set
//            {
//                m_description = value;
//            }
//        }

//        public string FormatString
//        {
//            get
//            {
//                return "P2";
//            }
//        }

//        int m_id = 49;
//        public int Id
//        {
//            get
//            {
//                return m_id;
//            }

//            set
//            {
//                m_id = value;
//            }
//        }

//        bool m_lowerIsBetter = true;
//        public bool LowerIsBetter
//        {
//            get
//            {
//                return m_lowerIsBetter;
//            }

//            set
//            {
//                m_lowerIsBetter = value;
//            }
//        }

//        string m_name = "Overdue Sales Orders (%)";
//        public string Name
//        {
//            get
//            {
//                return m_name;
//            }

//            set
//            {
//                m_name = value;
//            }
//        }

//        Color m_plotColor = Color.MediumSlateBlue;
//        public Color PlotColor
//        {
//            get
//            {
//                return m_plotColor;
//            }

//            set
//            {
//                m_plotColor = value;
//            }
//        }

//        KPI.valueDisplayTypes m_valueDisplayType = KPI.valueDisplayTypes.Integer;
//        public KPI.valueDisplayTypes ValueDisplayType
//        {
//            get
//            {
//                return m_valueDisplayType;
//            }

//            set
//            {
//                m_valueDisplayType = value;
//            }
//        }

//        decimal IKpiCalculator.Calculate(ScenarioDetail a_sd)
//        {
//            int totalOverdueOrders = 0;
//            int totalOrders = 0;

//            Tuple<int, List<Job>> lateSalesOrderJobs = KPICalculatorHelper.GetOverdueSalesOrderJobs(a_sd);
//            totalOrders += lateSalesOrderJobs.Item1;
//            totalOverdueOrders += lateSalesOrderJobs.Item2.Count;

//            Tuple<int, List<SalesOrder>> salesOrders = KPICalculatorHelper.GetOverdueSalesOrders(a_sd);
//            totalOrders += salesOrders.Item1;
//            totalOverdueOrders += salesOrders.Item2.Count;

//            if (totalOrders == 0)
//            {
//                return 0m;
//            }

//            return ((decimal)totalOverdueOrders) / (totalOrders);
//        }
//        #endregion LateSalesOrderCalculator
//    }
//}

