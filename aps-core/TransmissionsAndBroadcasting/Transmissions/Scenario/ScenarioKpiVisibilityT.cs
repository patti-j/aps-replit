using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Stores the updated KPI visibility settings.
/// </summary>
public class ScenarioKpiVisibilityT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 782;

    #region IPTSerializable Members
    public ScenarioKpiVisibilityT(IReader reader)
        : base(reader)
    {
        KpiUpdateList = new List<KpiUpdateValues>();
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                KpiUpdateList.Add(new KpiUpdateValues(reader));
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(KpiUpdateList.Count);
        for (int i = 0; i < KpiUpdateList.Count; i++)
        {
            KpiUpdateList[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;

    public override string Description => "KPI Visibility Options saved";
    #endregion

    public ScenarioKpiVisibilityT() { }

    public ScenarioKpiVisibilityT(BaseId a_scenarioId) : base(a_scenarioId)
    {
        KpiUpdateList = new List<KpiUpdateValues>();
    }

    public List<KpiUpdateValues> KpiUpdateList;

    /// <summary>
    /// Holds the settings that can be updated for a KPI
    /// </summary>
    public class KpiUpdateValues : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 783;

        public KpiUpdateValues(IReader reader)
        {
            if (reader.VersionNumber >= 648)
            {
                m_bools = new BoolVector32(reader);
                reader.Read(out m_id);
                int argb;
                reader.Read(out argb);
                m_color = System.Drawing.Color.FromArgb(argb);
            }
            else if (reader.VersionNumber >= 504)
            {
                bool obsoleteBool;
                reader.Read(out m_id);
                reader.Read(out obsoleteBool);
                Hidden = obsoleteBool;
                reader.Read(out obsoleteBool);
                DoNotCalculate = obsoleteBool;
                int argb;
                reader.Read(out argb);
                m_color = System.Drawing.Color.FromArgb(argb);
            }
            else
            {
                bool obsoleteBool;
                reader.Read(out m_id);
                reader.Read(out obsoleteBool);
                Hidden = obsoleteBool;
                int argb;
                reader.Read(out argb);
                m_color = System.Drawing.Color.FromArgb(argb);
            }
        }

        public void Serialize(IWriter writer)
        {
            m_bools.Serialize(writer);
            writer.Write(m_id);
            writer.Write(m_color.ToArgb());
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public KpiUpdateValues(int a_id, bool a_hidden, bool a_doNotCalculate, System.Drawing.Color a_drawingColor, bool a_alert)
        {
            m_id = a_id;
            Hidden = a_hidden;
            DoNotCalculate = a_doNotCalculate;
            Alert = a_alert;
            m_color = a_drawingColor;
        }

        private readonly int m_id;
        private readonly System.Drawing.Color m_color;

        /// <summary>
        /// KPI Calculator ID
        /// </summary>
        public int ID => m_id;

        private BoolVector32 m_bools;

        private const short c_hiddenIdx = 0;
        private const short c_doNotCalculateIdx = 1;
        private const short c_alertIdx = 2;

        /// <summary>
        /// Bool for wheter to show in the UI
        /// </summary>
        public bool Hidden
        {
            get => m_bools[c_hiddenIdx];
            private set => m_bools[c_hiddenIdx] = value;
        }

        /// <summary>
        /// Bool for wheter to show in the UI
        /// </summary>
        public bool DoNotCalculate
        {
            get => m_bools[c_doNotCalculateIdx];
            private set => m_bools[c_doNotCalculateIdx] = value;
        }

        /// <summary>
        /// Bool for wheter to show an alert for the KPI value
        /// </summary>
        public bool Alert
        {
            get => m_bools[c_alertIdx];
            private set => m_bools[c_alertIdx] = value;
        }

        /// <summary>
        /// Color used to draw the KPI
        /// </summary>
        public System.Drawing.Color DrawingColor => m_color;
    }
}