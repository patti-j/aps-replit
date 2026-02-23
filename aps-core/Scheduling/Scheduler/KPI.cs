using System.Collections;

using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Database;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Each KPI Value indicates the value of a specific KPI after the Transmission indicated by the LastTransmissionNbr.
/// This allows for the user interface to graph the KPI values over time as the schedule is changed.and then undo back
/// to a specific set of values.
/// </summary>
public class KpiValue : IPTSerializable, IComparable
{
    public const int UNIQUE_ID = 407;

    #region IPTSerializable Members
    public KpiValue(IReader reader)
    {
        if (reader.VersionNumber >= 678)
        {
            reader.Read(out m_calculation);
            reader.Read(out int val);
            m_snapshotType = (KpiOptions.ESnapshotType)val;
            reader.Read(out m_description);
            reader.Read(out m_transmissionNbr);
            reader.Read(out m_transmissionDateTime);
        }
        else if (reader.VersionNumber >= 363)
        {
            reader.Read(out m_calculation);
            reader.Read(out int val);
            m_snapshotType = KpiOptions.ESnapshotType.Other;
            reader.Read(out m_description);
            reader.Read(out m_transmissionNbr);
            reader.Read(out m_transmissionDateTime);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_calculation);
            reader.Read(out int val);
            m_snapshotType = KpiOptions.ESnapshotType.Other;
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
#if DEBUG
        writer.DuplicateErrorCheck(this);
#endif
        writer.Write(m_calculation);
        writer.Write((int)m_snapshotType);
        writer.Write(m_description);
        writer.Write(m_transmissionNbr);
        writer.Write(m_transmissionDateTime);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public KpiValue(decimal a_calculation, KpiOptions.ESnapshotType a_snapshotType, string a_description, ulong a_transmissionNbr, DateTimeOffset a_transmissionDateTime)
    {
        m_calculation = a_calculation;
        m_snapshotType = a_snapshotType;
        m_description = a_description;
        m_transmissionNbr = a_transmissionNbr;
        m_transmissionDateTime = a_transmissionDateTime.Ticks;
    }

    private readonly decimal m_calculation;

    public decimal Calculation => m_calculation;

    public string GetScaledCalculationDisplayString(IBaseKpiCalculatorElement a_calculator)
    {
        return (Calculation * a_calculator.ChartScaleMultiplier).ToString(a_calculator.FormatString);
    }

    private readonly KpiOptions.ESnapshotType m_snapshotType;

    public KpiOptions.ESnapshotType SnapshotType => m_snapshotType;

    private readonly string m_description;

    public string Description => m_description;

    private readonly ulong m_transmissionNbr;

    public ulong TransmissionNbr => m_transmissionNbr;

    private readonly long m_transmissionDateTime;
    public DateTimeOffset TransmissionDateTime => new (m_transmissionDateTime, TimeSpan.Zero);
    public int CompareTo(object a_otherKpi)
    {
        if (a_otherKpi is KpiValue)
        {
            return TransmissionDateTime.CompareTo(((KpiValue)a_otherKpi).TransmissionDateTime);
        }

        return 1;
    }
}
/// <summary>
/// There is one Kpi for each type of value to be generated.
/// This object stores the calculator of the values and a list of previously calculated values.
/// </summary>
public class KPI : IPTSerializable, ICloneable
{
    public const int UNIQUE_ID = 399;

    #region IPTSerializable Members
    public KPI(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 648)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out calculatorName);
            a_reader.Read(out m_overrideColor);
            if (m_overrideColor)
            {
                a_reader.Read(out m_drawingColor);
            }

            m_kpiValues = new KpiValuesList(a_reader);
        }
        else if (a_reader.VersionNumber >= 504)
        {
            a_reader.Read(out calculatorName);
            bool obsoleteBool;
            a_reader.Read(out obsoleteBool);
            Hidden = obsoleteBool;
            a_reader.Read(out obsoleteBool);
            DoNotCalculate = obsoleteBool;
            a_reader.Read(out m_overrideColor);
            if (m_overrideColor)
            {
                a_reader.Read(out m_drawingColor);
            }

            m_kpiValues = new KpiValuesList(a_reader);
        }

        // The UI does not allow changing this value, so default all hidden ones to visible so the user can hide them from the UI again if needed.
        if (a_reader.VersionNumber < 12500)
        {
            Hidden = false;
        }
    }

    public void Serialize(IWriter a_writer)
    {
#if DEBUG
        a_writer.DuplicateErrorCheck(this);
#endif
        m_bools.Serialize(a_writer);
        a_writer.Write(CalculatorName);
        a_writer.Write(m_overrideColor);
        if (m_overrideColor)
        {
            a_writer.Write(m_drawingColor);
        }

        m_kpiValues.Serialize(a_writer);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    private KPI(KPI a_original)
    {
        m_bools = new BoolVector32(a_original.m_bools);
        Calculator = a_original.Calculator;
        m_overrideColor = a_original.m_overrideColor;
        m_drawingColor = a_original.m_drawingColor;
        m_kpiValues = a_original.KpiValues.CopyInMemory();
    }

    private BoolVector32 m_bools;

    private const short c_hiddenIdx = 0;
    private const short c_doNotCalculateIdx = 1;
    private const short c_alertIdx = 2;

    //		const int maxNumberOfValues=20; //*JMC* Get this from UNDO somewhere?

    private readonly string calculatorName = "";

    /// <summary>
    /// Used to determine which Calculator to attach to after deserializing.
    /// </summary>
    public string CalculatorName
    {
        get
        {
            if (Calculator != null)
            {
                return Calculator.Name;
            }

            return calculatorName;
        }
    }

    public bool Hidden
    {
        get => m_bools[c_hiddenIdx] || DoNotCalculate;
        set => m_bools[c_hiddenIdx] = value;
    }

    public bool DoNotCalculate
    {
        get => m_bools[c_doNotCalculateIdx];
        set => m_bools[c_doNotCalculateIdx] = value;
    }

    public bool Alert
    {
        get => m_bools[c_alertIdx];
        set => m_bools[c_alertIdx] = value;
    }

    private bool m_overrideColor;
    private System.Drawing.Color m_drawingColor;

    public System.Drawing.Color DrawingColor
    {
        get
        {
            if (m_overrideColor)
            {
                return m_drawingColor;
            }

            return Calculator.PlotColor;
        }
        set
        {
            if (value == Calculator.PlotColor)
            {
                m_overrideColor = false;
            }
            else if (value != m_drawingColor)
            {
                m_drawingColor = value;
                m_overrideColor = true;
            }
        }
    }

    private IKpiCalculatorElement m_calculator;

    public KPI(IKpiCalculatorElement calculator)
    {
        m_calculator = calculator;
    }

    /// <summary>
    /// The Calculator used to calculate the kpi values.
    /// </summary>
    public IKpiCalculatorElement Calculator
    {
        get => m_calculator;
        set => m_calculator = value;
    }

    public void Calculate(ScenarioDetail sd, KpiOptions.ESnapshotType a_snapshotType, string aDescription, ulong aTransmissionNbr, DateTimeOffset aTransmissionDateTime)
    {
        if (DoNotCalculate)
        {
            return;
        }

        KpiValue newValue = new (m_calculator.Calculate(sd), a_snapshotType, aDescription, aTransmissionNbr, aTransmissionDateTime);
        m_kpiValues.Add(newValue);
    }

    internal KpiValue Calculate(ScenarioDetail sd, KpiOptions.ESnapshotType a_snapshotType, string aDescription)
    {
        return DoNotCalculate ? null : new KpiValue(m_calculator.Calculate(sd), a_snapshotType, aDescription, 0, DateTime.Now);
    }

    /// <summary>
    /// The most recent calculated value.
    /// There many not be a value so check the KPIValues.Count property or make sure this property is not null.
    /// </summary>
    /// <returns></returns>
    public KpiValue GetCurrentValue()
    {
        if (m_kpiValues.Count == 0)
        {
            return null;
        }

        return m_kpiValues.SortedKpIsList[m_kpiValues.Count - 1];
    }

    /// <summary>
    /// The the amount of change from the previous value to the current value.
    /// </summary>
    /// <returns></returns>
    public decimal GetChange()
    {
        if (m_kpiValues.Count < 2)
        {
            return 0;
        }

        KpiValue[] sortedKpIsList = m_kpiValues.SortedKpIsList;

        return (sortedKpIsList[m_kpiValues.Count - 1]).Calculation -
               (sortedKpIsList[m_kpiValues.Count - 2]).Calculation;
    }

    /// <summary>
    /// The the percent of change from the previous value to the current value.
    /// </summary>
    /// <returns></returns>
    public decimal GetChangePercent()
    {
        if (m_kpiValues.Count < 2)
        {
            return 0;
        }

        decimal currentValue = m_kpiValues[m_kpiValues.Count - 2].Calculation;
        if (currentValue != 0)
        {
            return GetChange() / currentValue;
        }

        return 0;
    }

    private readonly KpiValuesList m_kpiValues = new ();
    /// <summary>
    /// Returns a sorted collection of KPIValues, sorted by Transmission DateTime in asc
    /// </summary>
    public KpiValue[] SortedKpiValuesList => m_kpiValues.SortedKpIsList;
    
    public KpiValuesList KpiValues => m_kpiValues;

    public enum EValueDisplayTypes { Double, Integer, Percentage, Currency }

    /// <summary>
    /// Retrives the Calculator name and value for the KPI and adds it to a new row in the dataset
    /// </summary>
    public void PtDbPopulate(ref PtDbDataSet a_dataSet, PtDbDataSet.SchedulesRow a_publishRow)
    {
        KpiValue tempCurrentValue = GetCurrentValue();
        if (tempCurrentValue != null && CalculatorName != null)
        {
            a_dataSet.KPIs.AddKPIsRow(a_publishRow, a_publishRow.InstanceId, CalculatorName, GetCurrentValue().Calculation, m_calculator.Id);
        }
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public KPI Clone()
    {
        return new KPI(this);
    }
}

/// <summary>
/// Stores an ArrayList of KPI values.
/// </summary>
public class KpiValuesList : IPTDeserializable
{
    public const int UNIQUE_ID = 406;

    #region IPTSerializable Members
    public KpiValuesList(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                KpiValue kpi = new (reader);
                Add(kpi);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
#if DEBUG
        writer.DuplicateErrorCheck(this);
#endif
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public KpiValuesList() { }

    private ArrayList m_kpiValues = new ();

    public KpiValue Add(KpiValue kpiValue)
    {
        m_kpiValues.Add(kpiValue);
        return kpiValue;
    }

    public void RemoveAt(int index)
    {
        m_kpiValues.RemoveAt(index);
    }

    public KpiValue this[int index] => (KpiValue)m_kpiValues[index];
    public int Count => m_kpiValues.Count;
    /// <summary>
    /// Returns a sorted collection of KPIValues, sorted by Transmission DateTime in asc
    /// </summary>
    internal KpiValue[] SortedKpIsList
    {
        get
        {
            KpiValue[] sortedList = new KpiValue[m_kpiValues.Count];
            m_kpiValues.ToArray().CopyTo(sortedList, 0);
            Array.Sort(sortedList, new KPIValuesCollectionComparer());

            return sortedList;
        }
    }
    private class KPIValuesCollectionComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null || y == null)
            {
                return 1;
            }
            return ((KpiValue)x).CompareTo((KpiValue)y);
        }
    }
}
/// <summary>
/// Controls the calculation of KPI values and stores these values.
/// </summary>
public class KpiController : IPTSerializable
{
    public const int UNIQUE_ID = 349;

    #region IPTSerializable Members
    public KpiController(IReader reader)
    {
        if (reader.VersionNumber >= 363)
        {
            snapshotList = new SnapshotList(reader);
            kpis = new KPIList(reader);
            m_kpiOptions = new KpiOptions(reader);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            snapshotList = new SnapshotList(reader);

            kpis = new KPIList(reader);
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
#if DEBUG
        writer.DuplicateErrorCheck(this);
#endif

        snapshotList.Serialize(writer);
        kpis.Serialize(writer);
        m_kpiOptions.Serialize(writer);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public KpiController() { }

    /// <summary>
    /// Set the list of calculators to use when creating KPI values.
    /// </summary>
    internal void InitCalculatorList(List<IKpiCalculatorModule> a_kpiModules)
    {
        //Get list of KPI calculators from packages
        m_kpiCalculatorList = new List<IKpiCalculatorElement>();
        foreach (IKpiCalculatorModule kpiModule in a_kpiModules)
        {
            List<IBaseKpiCalculatorElement> elements = kpiModule.GetKpiCalculatorElements();
            foreach (IBaseKpiCalculatorElement element in elements)
            {
                if (element is IKpiCalculatorElement kpiElement)
                {
                    m_kpiCalculatorList.Add(kpiElement);
                }
            }
        }

        //Make sure that the stored KPIs match the current Calculator list.
        CheckThatKPIsMatchCalculators();
    }

    private KpiOptions m_kpiOptions = new ();
    public KpiOptions Options => m_kpiOptions;

    internal void UpdateOptions(ScenarioKpiOptionsUpdateT t)
    {
        if (m_kpiOptions.ImpactComparisonType != t.Options.ImpactComparisonType)
        {
            //Clear cache, the impact comparison snapshot could have changed.
            m_cachedSnapshot = null;
        }

        m_kpiOptions = t.Options.Clone();
    }

    /// <summary>
    /// Updates the visibility settings for KPIs
    /// </summary>
    /// <param name="a_t"></param>
    internal void UpdateKpiVisibility(ScenarioKpiVisibilityT a_t)
    {
        foreach (ScenarioKpiVisibilityT.KpiUpdateValues update in a_t.KpiUpdateList)
        {
            for (int i = 0; i < KpiList.UnsortedKpiList.Count; i++)
            {
                if (KpiList.UnsortedKpiList[i].Calculator.Id == update.ID)
                {
                    KpiList.UnsortedKpiList[i].Hidden = update.Hidden;
                    KpiList.UnsortedKpiList[i].DoNotCalculate = update.DoNotCalculate;
                    KpiList.UnsortedKpiList[i].DrawingColor = update.DrawingColor;
                    KpiList.UnsortedKpiList[i].Alert = update.Alert;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Each KPI value must have a matching KPI Calculator and vice versa.
    /// If they don't match then the KPIs and Snapshots are reinitialized.
    /// </summary>
    private void CheckThatKPIsMatchCalculators()
    {
        //Loop through the KpiCalculatorList, set the calculator if the KpiList contains the KPI, if not, add the calculator to the KpiList.
        if (KpiCalculatorList.Count > 0)
        {
            for (int i = 0; i < KpiCalculatorList.Count; i++)
            {
                IKpiCalculatorElement kpiCalculator = KpiCalculatorList[i];

                if (KpiList.Contains(kpiCalculator.Name))
                {
                    KPI kpi = KpiList.Find(kpiCalculator.Name);
                    kpi.Calculator = kpiCalculator;
                }
                else
                {
                    IKpiCalculatorElement calculator = KpiCalculatorList[i];
                    KpiList.Add(new KPI(calculator));
                }
            }
        }

        //Loop through all of the KpiList and check if any do not exist in the KpiCalculatorList, if so, remove them from the KpiList.
        for (int i = KpiList.Count - 1; i >= 0; i--)
        {
            KPI kpi = KpiList[i];
            if (!KpiCalculatorList.Exists(c => c.Name.ToLower() == kpi.CalculatorName.ToLower()))
            {
                KpiList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Clear any existing KPI and Snapshot data and create KPIs based on the list of Calculators.
    /// </summary>
    private void InitializeAllKpiValuesAndSnapshots()
    {
        kpis.Clear();
        SnapshotList.Clear();

        for (int i = 0; i < KpiCalculatorList.Count; i++)
        {
            IKpiCalculatorElement calculator = KpiCalculatorList[i];
            KpiList.Add(new KPI(calculator));
        }
    }

    private List<IKpiCalculatorElement> m_kpiCalculatorList;

    /// <summary>
    /// This current list of KPI calculators.
    /// </summary>
    private List<IKpiCalculatorElement> KpiCalculatorList => m_kpiCalculatorList;

    private readonly SnapshotList snapshotList = new ();
    public SnapshotList SnapshotList => snapshotList;

    //This is not serialized.
    private CachedSnapshotComparison m_cachedSnapshot;

    public CachedSnapshotComparison CachedSnapshotComparison
    {
        get
        {
            if (m_cachedSnapshot == null && snapshotList.Count > 1)
            {
                Snapshot impactComparisonSnapshot = FindImpactComparisonSnapshot();
                if (impactComparisonSnapshot == null)
                {
                    return null;
                }

                JobInfoList before = impactComparisonSnapshot.JobInfoList;
                JobInfoList after = snapshotList[^1].JobInfoList;
                m_cachedSnapshot = new CachedSnapshotComparison(before, after, impactComparisonSnapshot.GetDescription());
            }

            return m_cachedSnapshot;
        }
    }

    /// <summary>
    /// Returns the snapshot to use for impact comparison. This is determined by KpiOptions settings.
    /// </summary>
    private Snapshot FindImpactComparisonSnapshot()
    {
        if (snapshotList.Count < 2)
        {
            return null;
        }

        //Return latest snapshot
        if (Options.ImpactComparisonType == KpiOptions.ESnapshotType.Latest)
        {
            return snapshotList[^2];
        }

        Snapshot snapshot;

        //Check for a specific snapshot
        if (Options.ImpactComparisonType == KpiOptions.ESnapshotType.Specific)
        {
            for (int i = 0; i < snapshotList.Count - 1; i++)
            {
                snapshot = snapshotList[i];
                if (snapshot.TransmissionNbr == Options.ImpactComparisonId)
                {
                    return snapshot;
                }
            }

            return null;
        }

        //Check for a snapshot from date
        if (Options.ImpactComparisonType == KpiOptions.ESnapshotType.Today)
        {
            DateTimeOffset nowDate = PTDateTime.UserDateTimeNow.ToDateNoTime();
            for (int i = 0; i < snapshotList.Count - 2; i++)
            {
                snapshot = snapshotList[i];
                if (snapshot.CreationDate.Date >= nowDate.ToDateTime())
                {
                    return snapshot;
                }
            }

            return null;
        }

        //Find latest snapshot of type
        for (int i = snapshotList.Count - 2; i >= 0; i--)
        {
            snapshot = snapshotList[i];
            if (snapshot.Type == Options.ImpactComparisonType)
            {
                return snapshot;
            }
        }

        return null;
    }

    private readonly KPIList kpis = new ();

    public KPIList KpiList => kpis;

    public class KPIException : PTException
    {
        public KPIException(KPI a_kpi, Exception a_innerException)
            : base("4072", a_innerException, new object[] { a_kpi.CalculatorName }) { }
    }

    internal void CalculateKPIs(ScenarioDetail a_sd, ulong a_transmissionNbr, string a_description, DateTimeOffset a_transmissionDateTime, KpiOptions.ESnapshotType a_snapshotType, ISystemLogger a_errorReporter)
    {
        m_cachedSnapshot = null;
        if (!PTSystem.LicenseKey.IncludeKPIs)
        {
            ScenarioExceptionInfo sei = new ();
            sei.Create(a_sd);
            APSCommon.AuthorizationException err = new ("CalculateKPIs", APSCommon.AuthorizationType.LicenseKey, "IncludeKPIs", false.ToString());
            a_errorReporter.LogException(err, sei);
            return;
        }

        CreateSnapshot(a_sd, a_transmissionNbr, a_snapshotType, a_transmissionDateTime);
        TrimBelowMaxLength();
        for (int i = 0; i < kpis.Count; i++)
        {
            KPI kpi = kpis.UnsortedKpiList[i];
            try
            {
                kpi.Calculate(a_sd, a_snapshotType, a_description, a_transmissionNbr, a_transmissionDateTime);
            }
            catch (Exception ex)
            {
                throw new KPIException(kpi, ex);
            }
        }
    }

    public KpiValue CalculateKPIByName(ScenarioDetail a_sd, string a_KpiName)
    {
        KPI kpi = null;
        foreach (KPI k in kpis.UnsortedKpiList)
        {
            if (k.CalculatorName == a_KpiName)
            {
                kpi = k;
                break;
            }
        }

        if (kpi != null && !kpi.DoNotCalculate)
        {
            return kpi.Calculate(a_sd, KpiOptions.ESnapshotType.Other, "");
        }

        return null;
    }

    /// <summary>
    /// if true, KPI calculations are skipped. For example, when performing MRP
    /// only the last optimize should cause a KPI calculation.
    /// </summary>
    private bool m_suppressKPICalculation;

    public void SetSuppressKPICalculation(bool a_suppressKPICalc)
    {
        m_suppressKPICalculation = a_suppressKPICalc;
    }

    /// <summary>
    /// Creates a new set of KPI values for the last simulation event, if the KPI Options require it and LicenseKey Allows it.
    /// </summary>
    internal void CalculateKPIs(Scenario a_scenario, ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simulationType, PTTransmission a_t, ISystemLogger a_errorReporter)
    {
        if ((PTSystem.LicenseKey.IncludeKPIs && Options.CalculateKpiAfterEveryScheduleChange && !m_suppressKPICalculation) || PTSystem.RunningMassRecordings)
        {
            string description = "";
            KpiOptions.ESnapshotType type = GetSnapshotTypeFromTransmission(a_t);

            CalculateKPIs(a_sd, a_t.TransmissionNbr, description, a_t.TimeStamp, type, a_errorReporter);
            //TODO: Why would we fire this event every time KPIs are calculated. This causes duplicate events for simulations since simulations cause KPIs to be recalculated.
            FireKpiChangedEvent(a_scenario, a_sd, a_simulationType, a_t);
        }
    }

    private KpiOptions.ESnapshotType GetSnapshotTypeFromTransmission(PTTransmission a_t)
    {
        if (a_t is UserLogOnT or ScenarioTouchT)
        {
            return KpiOptions.ESnapshotType.TimeAdjustment;
        }

        if (a_t is ScenarioDetailOptimizeT)
        {
            return KpiOptions.ESnapshotType.Optimize;
        }

        if (a_t is ScenarioDetailMoveT)
        {
            return KpiOptions.ESnapshotType.Move;
        }

        if (a_t is ScenarioDetailExpediteBaseT)
        {
            return KpiOptions.ESnapshotType.Expedite;
        }

        if (a_t is ScenarioDetailCompressT)
        {
            return KpiOptions.ESnapshotType.Compress;
        }

        if (a_t is ScenarioDetailJitCompressT)
        {
            return KpiOptions.ESnapshotType.JitCompress;
        }

        if (a_t is ScenarioClockAdvanceT)
        {
            return KpiOptions.ESnapshotType.ClockAdvance;
        }

        return KpiOptions.ESnapshotType.Other;
    }

    private void FireKpiChangedEvent(Scenario a_scenario, ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simulationType, PTTransmission a_t)
    {
        ScenarioDataChanges dataChanges = new ();
        dataChanges.KPIChanges = true;
        using (a_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            se.FireKPIChangedEvent(this, a_sd, a_simulationType, a_t);
        }
    }

    /// <summary>
    /// Calculate KPIs for the Publish, if the KPI Options require it.
    /// </summary>
    /// <param name="t"></param>
    internal void CalculateKPIsForPublish(Scenario a_scenario, ScenarioDetail a_sd, ScenarioDetailExportT a_t, ISystemLogger a_errorReporter)
    {
        if (PTSystem.LicenseKey.IncludeKPIs && Options.CalculateKpiAfterPublish)
        {
            CalculateKPIs(a_sd, a_t.TransmissionNbr, "Publish", a_t.TimeStamp, KpiOptions.ESnapshotType.Publish, a_errorReporter);
            FireKpiChangedEvent(a_scenario, a_sd, ScenarioDetail.SimulationType.None, a_t);
        }
    }

    /// <summary>
    /// Calculate KPIs after the import.
    /// </summary>
    /// <param name="t"></param>
    internal void CalculateKPIsForImport(Scenario a_scenario, ScenarioDetail a_sd, ImportT a_t, ISystemLogger a_errorReporter)
    {
        if (PTSystem.LicenseKey.IncludeKPIs && Options.CalculateKpiAfterImport)
        {
            CalculateKPIs(a_sd, a_t.TransmissionNbr, "Import", a_t.TimeStamp, KpiOptions.ESnapshotType.Import, a_errorReporter);
            FireKpiChangedEvent(a_scenario, a_sd, ScenarioDetail.SimulationType.None, a_t);
        }
    }

    internal void CalculateKPIsForSchedulingAgent(Scenario a_scenario, ScenarioDetail a_sd, KpiSnapshotOfLiveScenarioT a_t, ISystemLogger a_errorReporter)
    {
        if (PTSystem.LicenseKey.IncludeKPIs)
        {
            CalculateKPIs(a_sd, a_t.TransmissionNbr, "Automatic", a_t.TimeStamp, KpiOptions.ESnapshotType.Automatic, a_errorReporter);
            FireKpiChangedEvent(a_scenario, a_sd, ScenarioDetail.SimulationType.None, a_t);
        }
    }

    internal void CalculateKPIsAtUserRequest(Scenario a_scenario, ScenarioDetail a_sd, ScenarioKpiSnapshotT a_t, ISystemLogger a_errorReporter)
    {
        if (PTSystem.LicenseKey.IncludeKPIs)
        {
            CalculateKPIs(a_sd, a_t.TransmissionNbr, a_t.Description, a_t.TimeStamp, KpiOptions.ESnapshotType.User, a_errorReporter);
            FireKpiChangedEvent(a_scenario, a_sd, ScenarioDetail.SimulationType.None, a_t);
        }
    }

    /// <summary>
    /// Accesses scenario detail to get the necessary data to create a snapshot of the scenario at this point in time.
    /// </summary>
    /// <param name="a_sd"></param>
    private void CreateSnapshot(ScenarioDetail a_sd, ulong a_lastTransmissionNbr, KpiOptions.ESnapshotType a_type, DateTimeOffset a_creationDateTime)
    {
        if (a_creationDateTime.ToDateTime() <= PTDateTime.MinDateTime)
        {
            return; //this is not a valid snapshot.
        }

        if (a_type == KpiOptions.ESnapshotType.TimeAdjustment && SnapshotList.Count > 0)
        {
            return; //we don't need to store a time adjustment snapshot since the data doesn't change on time adjustments
        }

        Snapshot s = new (a_lastTransmissionNbr, a_type, a_creationDateTime);
        //Add a new JobInfo for each Job in the ScenarioDetail.
        //for (int i = 0; i < a_sd.JobManager.Count; i++)
        //{
        //    Job j = a_sd.JobManager[i];
        //    if (!j.Template)
        //    {
        //        s.JobInfoList.Add(new JobInfo(j, a_sd));
        //    }
        //}

        SnapshotList.Add(s);
    }

    /// <summary>
    /// Compares two snapshots, the earlier one against the later one and returns a SnapshotComparison.
    /// </summary>
    public SnapshotComparison CompareSnapshots(ulong trans1, ulong trans2)
    {
        ulong beforeTrans = Math.Min(trans1, trans2);
        ulong afterTrans = Math.Max(trans1, trans2);

        Snapshot beforeSnapshot = SnapshotList.Find(beforeTrans);
        Snapshot afterSnapshot = SnapshotList.Find(afterTrans);

        if (beforeSnapshot == null)
        {
            throw new PTHandleableException("2325", new object[] { trans1.ToString() });
        }

        if (afterSnapshot == null)
        {
            throw new PTHandleableException("2326", new object[] { trans2.ToString() });
        }

        return afterSnapshot.CompareTo(beforeSnapshot);
    }

    /// <summary>
    /// Compares two snapshots, the one for the specified transmission and the one right before it.
    /// If there are fewer than two snapshots a PTHandleableException is thrown.
    /// </summary>
    public SnapshotComparison CompareSnapshots(ScenarioBaseT a_afterT)
    {
        if (a_afterT is ScenarioUndoT) //Compare the two newest Snapshots
        {
            return CompareLatestSnapshots();
        }

        if (SnapshotList.Count < 2)
        {
            throw new PTHandleableException("2327", new object[] { SnapshotList.Count });
        }

        ulong afterTransNbr = a_afterT.TransmissionNbr;
        Snapshot beforeSnapshot = SnapshotList.FindBefore(afterTransNbr);
        Snapshot afterSnapshot = SnapshotList.Find(afterTransNbr);

        if (beforeSnapshot != null && afterSnapshot != null)
        {
            return afterSnapshot.CompareTo(beforeSnapshot);
        }

        return null;
    }

    /// <summary>
    /// Compares the two most recent snapshots.
    /// If there are fewer than two snapshots a PTHandleableException is thrown.
    /// </summary>
    public SnapshotComparison CompareLatestSnapshots()
    {
        if (SnapshotList.Count < 2)
        {
            throw new PTHandleableException("2327", new object[] { SnapshotList.Count });
        }

        Snapshot beforeSnapshot = SnapshotList[SnapshotList.Count - 2];
        Snapshot afterSnapshot = SnapshotList[SnapshotList.Count - 1];

        if (beforeSnapshot != null && afterSnapshot != null)
        {
            return afterSnapshot.CompareTo(beforeSnapshot);
        }

        return null;
    }

    /// <summary>
    /// Removes the oldest impact and kpi values if the max count has been reached.
    /// </summary>
    private void TrimBelowMaxLength()
    {
        while (SnapshotList.Count >= Options.MaxKpiValuesToStore)
        {
            SnapshotList.RemoveAt(0);
            for (int i = 0; i < KpiList.Count; i++)
            {
                //New KPI Calculators might not have the same number of values
                while (KpiList.UnsortedKpiList[i].KpiValues.Count >= Options.MaxKpiValuesToStore)
                {
                    KpiList.UnsortedKpiList[i].KpiValues.RemoveAt(0);
                }
            }
        }
    }

    /// <summary>
    /// Get the row data for each KPI value
    /// </summary>
    /// <param name="dataSet">Full PT Dataset by reference</param>
    public void PtDbPopulate(ref PtDbDataSet a_dataSet, PtDbDataSet.SchedulesRow a_publishDate)
    {
        //The KPIs list can be null if KPIs have never been calculated.
        if (kpis != null)
        {
            for (int i = 0; i < kpis.Count; i++)
            {
                KPI kpiToAdd = kpis.SortedKpiList.Values[i];
                kpiToAdd.PtDbPopulate(ref a_dataSet, a_publishDate);
            }
        }
    }

    /// <summary>
    /// Sets KPI visibility settings to match the KPIs from the specified list.
    /// </summary>
    /// <param name="a_kpiList"></param>
    public void CopyKpiVisibility(List<KPI> a_kpiList)
    {
        foreach (KPI kpi in a_kpiList)
        {
            try
            {
                KPI matchingKPI = KpiList.Find(kpi.CalculatorName);
                if (matchingKPI != null)
                {
                    matchingKPI.Hidden = kpi.Hidden;
                    matchingKPI.DoNotCalculate = kpi.DoNotCalculate;
                    matchingKPI.DrawingColor = kpi.DrawingColor;
                }
            }
            catch
            {
                //Can't copy this KPI settings.
            }
        }
    }
}

/// <summary>
/// Stores a list of the KPIs.
/// </summary>
public class KPIList : IPTSerializable
{
    public const int UNIQUE_ID = 398;

    #region IPTSerializable Members
    public KPIList(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 262)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                KPI kpi = new (a_reader);
                Add(kpi);
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                KPI kpi = new (a_reader);
                //this.Add(kpi); Don't add because prior to version 262 the names weren't stored so we need to clear them out on upgrade at this  point.
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
#if DEBUG
        a_writer.DuplicateErrorCheck(this);
#endif

        a_writer.Write(UnsortedKpiList.Count);
        for (int i = 0; i < UnsortedKpiList.Count; i++)
        {
            UnsortedKpiList[i].Serialize(a_writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public KPIList() { }

    private SortedList<string, KPI> m_kpis = new ();
    private List<KPI> kpiNonSortedList = new ();

    /// <summary>
    /// List of KPIs that preserves the original sort when created which is used in the display of the KPIs.
    /// </summary>
    public List<KPI> UnsortedKpiList => kpiNonSortedList;

    public SortedList<string, KPI> SortedKpiList => m_kpis;

    //This list is built dynamically when referenced.
    //This can be converted to be updated like the other 2 lists without having
    // to change any of the external references to this list.
    public List<KPI> FilteredKpiList => FilterKPIList();

    public KPI Add(KPI a_kpi)
    {
        m_kpis.Add(a_kpi.CalculatorName, a_kpi);
        kpiNonSortedList.Add(a_kpi);
        return a_kpi;
    }

    public void RemoveAt(int a_index)
    {
        KPI kpiToRemove = m_kpis.Values[a_index];
        kpiNonSortedList.Remove(kpiToRemove);

        m_kpis.RemoveAt(a_index);
    }

    public KPI this[int a_index] => m_kpis.Values[a_index];

    public int Count => m_kpis.Count;

    public bool Contains(string a_kpiCalculatorName)
    {
        return m_kpis.ContainsKey(a_kpiCalculatorName);
    }

    public KPI Find(string a_kpiCalculatorName)
    {
        return m_kpis[a_kpiCalculatorName];
    }

    public void Clear()
    {
        m_kpis.Clear();
        kpiNonSortedList.Clear();
    }

    /// <summary>
    /// Returns a KPI list with only KPIs that are not hidden
    /// </summary>
    /// <returns></returns>
    private List<KPI> FilterKPIList()
    {
        List<KPI> filteredList = new ();
        for (int i = 0; i < SortedKpiList.Count; i++)
        {
            KPI kpi = SortedKpiList.Values[i];
            if (!kpi.Hidden)
            {
                filteredList.Add(kpi);
            }
        }

        return filteredList;
    }
}