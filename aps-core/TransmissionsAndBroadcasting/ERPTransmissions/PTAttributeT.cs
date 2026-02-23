using System.Data;
using System.Drawing;

using PT.ERPTransmissions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new PTAttribute in the specified Scenario using default values.
/// </summary>
public class PTAttributeT : ERPMaintenanceTransmission<PTAttributeT.PTAttribute>
{
    public override string Description => "PTAttribute updated";

    public new const int UNIQUE_ID = 991;

    #region IPTSerializable Members
    public PTAttributeT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int ptAttributeCount = 0; ptAttributeCount < count; ptAttributeCount++)
            {
                Add(new PTAttribute(a_reader));
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PTAttributeT() { }

    public new PTAttribute this[int a_i] => Nodes[a_i];

    public void Fill(IDbCommand a_ptAttributeCmd)
    {
        PtImportDataSet ds = new ();
        FillTable(ds.PTAttributes, a_ptAttributeCmd);

        Fill(ds);
    }

    private void Fill(PtImportDataSet a_ds)
    {
        foreach (PtImportDataSet.PTAttributesRow ptAttributeRow in a_ds.PTAttributes)
        {
            Add(new PTAttribute(ptAttributeRow));
        }
    }

    public class PTAttribute : PTObjectIdBase, IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 992;

        public PTAttribute(IReader a_reader) : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12302)
            {
                m_bools = new BoolVector32(a_reader);
                m_setBools = new BoolVector32(a_reader);
                a_reader.Read(out m_name);
                a_reader.Read(out m_description);
                a_reader.Read(out m_colorCode);
                a_reader.Read(out m_cost);
                a_reader.Read(out m_duration);
                a_reader.Read(out int incurWhenVal);
                m_attributeTrigger = (PTAttributeDefs.EAttributeTriggerOptions)incurWhenVal;
                a_reader.Read(out int attributeTypeVal);
                m_attributeType = (PTAttributeDefs.EIncurAttributeType)attributeTypeVal;
                a_reader.Read(out m_cleanoutGrade);
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_bools.Serialize(a_writer);
            m_setBools.Serialize(a_writer);
            a_writer.Write(m_name);
            a_writer.Write(m_description);
            a_writer.Write(m_colorCode);
            a_writer.Write(m_cost);
            a_writer.Write(m_duration);
            a_writer.Write((int)m_attributeTrigger);
            a_writer.Write((int)m_attributeType);
            a_writer.Write(m_cleanoutGrade);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public PTAttribute() { }

        public PTAttribute(PtImportDataSet.PTAttributesRow a_ptAttributeRow) : base(a_ptAttributeRow.ExternalId)
        {
            if (!a_ptAttributeRow.IsNameNull())
            {
                Name = a_ptAttributeRow.Name;
            }

            if (!a_ptAttributeRow.IsDescriptionNull())
            {
                Description = a_ptAttributeRow.Description;
            }

            if (!a_ptAttributeRow.IsColorCodeNull())
            {
                ColorCode = ColorUtils.GetColorFromHexString(a_ptAttributeRow.ColorCode);
            }

            if (!a_ptAttributeRow.IsDefaultCostNull())
            {
                Cost = a_ptAttributeRow.DefaultCost;
            }

            if (!a_ptAttributeRow.IsDefaultDurationHrsNull())
            {
                Duration = TimeSpan.FromHours(a_ptAttributeRow.DefaultDurationHrs);
            }

            if (!a_ptAttributeRow.IsAttributeTriggerNull())
            {
                AttributeTrigger = (PTAttributeDefs.EAttributeTriggerOptions)Enum.Parse(typeof(PTAttributeDefs.EAttributeTriggerOptions), a_ptAttributeRow.AttributeTrigger);
            }

            if (!a_ptAttributeRow.IsAttributeTypeNull())
            {
                AttributeType = (PTAttributeDefs.EIncurAttributeType)Enum.Parse(typeof(PTAttributeDefs.EIncurAttributeType), a_ptAttributeRow.AttributeType);
            }

            if (!a_ptAttributeRow.IsCleanoutGradeNull())
            {
                CleanoutGrade = a_ptAttributeRow.CleanoutGrade;
            }

            if (!a_ptAttributeRow.IsShowInGanttNull())
            {
                ShowInGantt = a_ptAttributeRow.ShowInGantt;
            }

            if (!a_ptAttributeRow.IsHideInGridsNull())
            {
                HideInGrids = a_ptAttributeRow.HideInGrids;
            }

            if (!a_ptAttributeRow.IsConsecutiveSetupNull())
            {
                ConsecutiveSetup = a_ptAttributeRow.ConsecutiveSetup;
            }

            if (!a_ptAttributeRow.IsUseInSequencingNull())
            {
                UseInSequencing = a_ptAttributeRow.UseInSequencing;
            }

            if (!a_ptAttributeRow.IsIncurResourceSetupNull())
            {
                IncurResourceSetup = a_ptAttributeRow.IncurResourceSetup;
            }
        }

        public PTAttribute(string a_externalId, string a_name, string a_description) : base(a_externalId)
        {
            m_name = a_name;
            m_description = a_description;
        }

        private BoolVector32 m_bools;
        private const short c_showInGanttIdx = 0;
        private const short c_consecutiveSetupIdx = 1;
        private const short c_hideInGridsIdx = 2;
        private const short c_manualUpdateIdx = 3;
        private const short c_useInSequencing = 4;
        private const short c_incurResourceSetupIdx = 5;

        private BoolVector32 m_setBools;
        private const short c_nameIsSetIdx = 0;
        private const short c_descriptionIsSetIdx = 1;
        private const short c_colorCodeIsSetIdx = 2;
        private const short c_costIsSetIdx = 3;
        private const short c_durationIsSetIdx = 4;
        private const short c_incurWhenIsSetIdx = 5;
        private const short c_attributeTypeIsSetIdx = 6;
        private const short c_cleanoutGradeIsSetIdx = 7;
        private const short c_showInGanttIsSetIdx = 8;
        private const short c_hideInGridsIsSetIdx = 9;
        private const short c_consecutiveSetupIsSetIdx = 10;
        private const short c_manualUpdateOnlyIsSetIdx = 11;
        private const short c_useInSequencingIsSetIdx = 12;
        private const short c_incurResourceSetupIsSetIdx = 13;

        #region Shared Properties
        private string m_name;

        /// <summary>
        /// Identifier for the Attribute.
        /// </summary>
        [System.ComponentModel.ParenthesizePropertyName(true)]
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                m_setBools[c_nameIsSetIdx] = true;
            }
        }

        public bool NameIsSet => m_setBools[c_nameIsSetIdx];

        private string m_description;

        /// <summary>
        /// Text for describing the Attribute.
        /// </summary>
        public string Description
        {
            get => m_description;
            set
            {
                m_description = value;
                m_setBools[c_descriptionIsSetIdx] = true;
            }
        }

        public bool DescriptionIsSet => m_setBools[c_descriptionIsSetIdx];

        private PTAttributeDefs.EAttributeTriggerOptions m_attributeTrigger;

        /// <summary>
        /// Specifies when to include a setup time and cost for the Attribute.
        /// </summary>
        public PTAttributeDefs.EAttributeTriggerOptions AttributeTrigger
        {
            get => m_attributeTrigger;
            set
            {
                m_attributeTrigger = value;
                m_setBools[c_incurWhenIsSetIdx] = true;
            }
        }

        public bool IncurWhenIsSet => m_setBools[c_incurWhenIsSetIdx];

        private PTAttributeDefs.EIncurAttributeType m_attributeType;

        /// <summary>
        /// Whether to incur this attribute changeover as Clean or Setup
        /// </summary>
        public PTAttributeDefs.EIncurAttributeType AttributeType
        {
            get => m_attributeType;
            set
            {
                m_attributeType = value;
                m_setBools[c_attributeTypeIsSetIdx] = true;
            }
        }

        public bool AttributeTypeIsSet => m_setBools[c_attributeTypeIsSetIdx];

        private Color m_colorCode = Color.Empty;

        /// <summary>
        /// A Color that can be used to represent the value of the Attribute in the Gantt.  For example, if the Attribute represents paint color then the ColorCode can be the color of the paint.
        /// </summary>
        public Color ColorCode
        {
            get => m_colorCode;
            set
            {
                m_colorCode = value;
                m_setBools[c_colorCodeIsSetIdx] = true;
            }
        }

        public bool ColorCodeIsSet => m_setBools[c_colorCodeIsSetIdx];

        /// <summary>
        /// The total setup time for an Operation is calculated as the sum of the maximum non-Consecutive setup time
        /// and the maximum of the various Consecutive setup times.
        /// </summary>
        public bool ConsecutiveSetup
        {
            get => m_bools[c_consecutiveSetupIdx];
            set
            {
                m_bools[c_consecutiveSetupIdx] = value;
                m_setBools[c_consecutiveSetupIsSetIdx] = true;
            }
        }

        public bool ConsecutiveSetupIsSet => m_setBools[c_consecutiveSetupIsSetIdx];

        private decimal m_cost;

        /// <summary>
        /// The cost to perform the setup related to this Attribute.
        /// This is only used if not using Lookup tables.
        /// </summary>
        public decimal Cost
        {
            get => m_cost;
            set
            {
                m_cost = value;
                m_setBools[c_costIsSetIdx] = true;
            }
        }

        public bool CostIsSet => m_setBools[c_costIsSetIdx];

        private TimeSpan m_duration;

        /// <summary>
        /// The time to perform the setup related to this Attribute.
        /// This is only used if not using Lookup tables.
        /// </summary>
        public TimeSpan Duration
        {
            get => m_duration;
            set
            {
                m_duration = value;
                m_setBools[c_durationIsSetIdx] = true;
            }
        }

        public bool DurationIsSet => m_setBools[c_durationIsSetIdx];

        /// <summary>
        /// Set to true to show the Attribute in the Attributes Segment in the Gantt.
        /// </summary>
        public bool ShowInGantt
        {
            get => m_bools[c_showInGanttIdx];
            set
            {
                m_bools[c_showInGanttIdx] = value;
                m_setBools[c_showInGanttIsSetIdx] = true;
            }
        }

        public bool ShowInGanttIsSet => m_setBools[c_showInGanttIsSetIdx];

        /// <summary>
        /// Set to true to hide the Attribute in the Grids.
        /// </summary>
        public bool HideInGrids
        {
            get => m_bools[c_hideInGridsIdx];
            set
            {
                m_bools[c_hideInGridsIdx] = value;
                m_setBools[c_hideInGridsIsSetIdx] = true;
            }
        }

        public bool HideInGridsIsSet => m_setBools[c_hideInGridsIsSetIdx];

        private int m_cleanoutGrade;

        /// <summary>
        /// Whether to incur this attribute changeover as Clean or Setup
        /// </summary>
        public int CleanoutGrade
        {
            get => m_cleanoutGrade;
            set
            {
                m_cleanoutGrade = value;
                m_setBools[c_cleanoutGradeIsSetIdx] = true;
            }
        }

        public bool CleanoutGradeIsSet => m_setBools[c_cleanoutGradeIsSetIdx];

        public bool ManualUpdateOnly
        {
            get => m_bools[c_manualUpdateIdx];
            set
            {
                m_bools[c_manualUpdateIdx] = value;
                m_setBools[c_manualUpdateOnlyIsSetIdx] = true;
            }
        }

        public bool ManualUpdateOnlyIsSet => m_setBools[c_manualUpdateOnlyIsSetIdx];

        public bool UseInSequencing
        {
            get => m_bools[c_useInSequencing];
            set
            {
                m_bools[c_useInSequencing] = value;
                m_setBools[c_useInSequencingIsSetIdx] = true;
            }
        }

        public bool UseInSequencingIsSet => m_setBools[c_useInSequencingIsSetIdx];

        public bool IncurResourceSetup
        {
            get => m_bools[c_incurResourceSetupIdx];
            set
            {
                m_bools[c_incurResourceSetupIdx] = value;
                m_setBools[c_incurResourceSetupIsSetIdx] = true;
            }
        }

        public bool IncurResourceSetupIsSet => m_setBools[c_incurResourceSetupIsSetIdx];
        #endregion
    }
}