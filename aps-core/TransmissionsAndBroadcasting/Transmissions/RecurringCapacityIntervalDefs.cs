using PT.SchedulerDefinitions;

namespace PT.Transmissions
{
    public class RecurringCapacityIntervalDef : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 434;

        #region PT Serialization
        public RecurringCapacityIntervalDef(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12104)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    m_resourceKeyList.Add(new ResourceKeyExternal(a_reader));
                }

                Rci = new RecurringCapacityInterval(a_reader);
            }
            else
            {
                a_reader.ReadList(out List<string> plantExternalIds);
                a_reader.ReadList(out List<string> departmentExternalIds);
                a_reader.ReadList(out List<string> resourceExternalIds);

                for (int i = 0; i < plantExternalIds.Count; i++)
                {
                    m_resourceKeyList.Add(new ResourceKeyExternal(plantExternalIds[i], departmentExternalIds[i], resourceExternalIds[i]));
                }

                Rci = new RecurringCapacityInterval(a_reader);
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            a_writer.Write(m_resourceKeyList.Count);
            foreach (ResourceKeyExternal resourceKey in m_resourceKeyList)
            {
                resourceKey.Serialize(a_writer);
            }

            Rci.Serialize(a_writer);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        private readonly List<ResourceKeyExternal> m_resourceKeyList = new();
        public List<ResourceKeyExternal> ResourceKeyList => m_resourceKeyList;

        public RecurringCapacityInterval Rci;

        public RecurringCapacityIntervalDef()
            : base("", "") { }

        /// <summary>
        /// An intermediate class used by import and the RecurringCapacityIntervalsControl
        /// to create the transmissions necessary to manage the RecurringCapacityIntervals
        /// </summary>
        /// <param name="a_rciRow"> A recurring capacity interval row from the dataset that's on
        /// RecurringCapacityIntervalsControl.cs</param>
        /// <param name="a_convertToServerTime">Do we want this transmission to use server time.</param>
        /// <exception cref="PT.APSCommon.PTValidationException"></exception>
        public RecurringCapacityIntervalDef(RecurringCapacityIntervalTDataSet.RecurringCapacityIntervalsRow a_rciRow, bool a_convertToServerTime)
            : base(a_rciRow.ExternalId, a_rciRow.Name)
        {
            Rci = new RecurringCapacityInterval(a_rciRow.ExternalId, a_rciRow.Name);
            int serverDayAdjustment;

            if (a_convertToServerTime)
            {
                Rci.StartDateTime = a_rciRow.StartDateTime.ToServerTime().RemoveSeconds();
                Rci.EndDateTime = a_rciRow.EndDateTime.ToServerTime().RemoveSeconds();
                serverDayAdjustment = TimeZoneAdjuster.GetDayAdjustment(Rci.StartDateTime.ToDisplayTime());
            }
            else
            {
                Rci.StartDateTime = a_rciRow.StartDateTime.RemoveSeconds();
                Rci.EndDateTime = a_rciRow.EndDateTime.RemoveSeconds();
                serverDayAdjustment = 0;
            }

            Rci.Name = a_rciRow.Name;

            if (!a_rciRow.IsDescriptionNull())
            {
                Rci.Description = a_rciRow.Description;
            }

            if (!a_rciRow.IsNbrOfPeopleNull())
            {
                Rci.NbrOfPeople = a_rciRow.NbrOfPeople;
            }

            if (!a_rciRow.IsNbrOfPeopleOverrideNull())
            {
                Rci.NbrOfPeopleOverride = a_rciRow.NbrOfPeopleOverride;
            }

            if (!a_rciRow.IsIntervalPresetNull())
            {
                switch (a_rciRow.IntervalPreset)
                {
                    case "Online":
                        Rci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        Rci.Color = ColorUtils.ColorCodes.CapacityIntervalOnlineColor;
                        Rci.UsedForSetup = true;
                        Rci.UsedForRun = true;
                        Rci.UsedForPostProcessing = true;
                        Rci.UsedForStorage = true;
                        Rci.UsedForClean = true;
                        Rci.CanStartActivity = true;
                        break;
                    case "Overtime":
                        Rci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        Rci.Color = ColorUtils.ColorCodes.CapacityIntervalOvertimeColor;
                        Rci.UsedForSetup = true;
                        Rci.UsedForRun = true;
                        Rci.UsedForPostProcessing = true;
                        Rci.UsedForStorage = true;
                        Rci.UsedForClean = true;
                        Rci.Overtime = true;
                        Rci.CanStartActivity = true;
                        break;
                    case "PotentialOvertime":
                        Rci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        Rci.Color = ColorUtils.ColorCodes.CapacityIntervalPotentialOvertimeColor;
                        Rci.UsedForSetup = true;
                        Rci.UsedForRun = true;
                        Rci.UsedForPostProcessing = true;
                        Rci.UsedForStorage = true;
                        Rci.UsedForClean = true;
                        Rci.Overtime = true;
                        Rci.UseOnlyWhenLate = true;
                        Rci.CanStartActivity = true;
                        break;
                    case "Cleanout":
                        Rci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        Rci.Color = ColorUtils.ColorCodes.CapacityIntervalCleanoutColor;
                        Rci.UsedForClean = true;
                        break;
                    case "Offline":
                        Rci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Offline;
                        Rci.Color = ColorUtils.ColorCodes.CapacityIntervalOfflineColor;
                        break;
                    case "Holiday":
                        Rci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Offline;
                        Rci.Color = ColorUtils.ColorCodes.CapacityIntervalHolidayColor;
                        Rci.PreventOperationsFromSpanning = true;
                        break;
                    case "Maintenance":
                        Rci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Offline;
                        Rci.Color = ColorUtils.ColorCodes.CapacityIntervalMaintenanceColor;
                        Rci.PreventOperationsFromSpanning = true;
                        Rci.CleanOutSetups = true;
                        break;
                    default:
                        // This will catch empty strings 
                        break;
                }
            }

            if (!a_rciRow.IsIntervalTypeNull())
            {
                try
                {
                    Rci.IntervalType = (CapacityIntervalDefs.capacityIntervalTypes)Enum.Parse(typeof(CapacityIntervalDefs.capacityIntervalTypes), a_rciRow.IntervalType);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            a_rciRow.IntervalType, "RecurringCapacityInterval", "IntervalType",
                            string.Join(", ", Enum.GetNames(typeof(CapacityIntervalDefs.capacityIntervalTypes)))
                        });
                }
            }

            if (!a_rciRow.IsNotesNull())
            {
                Rci.Notes = a_rciRow.Notes;
            }

            if (!a_rciRow.IsSundayNull())
            {
                switch (serverDayAdjustment)
                {
                    case 1:
                        Rci.Sunday = a_rciRow.Saturday;
                        break;
                    case -1:
                        Rci.Sunday = a_rciRow.Monday;
                        break;
                    default:
                        Rci.Sunday = a_rciRow.Sunday;
                        break;
                }
            }

            if (!a_rciRow.IsMondayNull())
            {
                switch (serverDayAdjustment)
                {
                    case 1:
                        Rci.Monday = a_rciRow.Sunday;
                        break;
                    case -1:
                        Rci.Monday = a_rciRow.Tuesday;
                        break;
                    default:
                        Rci.Monday = a_rciRow.Monday;
                        break;
                }
            }

            if (!a_rciRow.IsTuesdayNull())
            {
                switch (serverDayAdjustment)
                {
                    case 1:
                        Rci.Tuesday = a_rciRow.Monday;
                        break;
                    case -1:
                        Rci.Tuesday = a_rciRow.Wednesday;
                        break;
                    default:
                        Rci.Tuesday = a_rciRow.Tuesday;
                        break;
                }
            }

            if (!a_rciRow.IsWednesdayNull())
            {
                switch (serverDayAdjustment)
                {
                    case 1:
                        Rci.Wednesday = a_rciRow.Tuesday;
                        break;
                    case -1:
                        Rci.Wednesday = a_rciRow.Thursday;
                        break;
                    default:
                        Rci.Wednesday = a_rciRow.Wednesday;
                        break;
                }
            }

            if (!a_rciRow.IsThursdayNull())
            {
                switch (serverDayAdjustment)
                {
                    case 1:
                        Rci.Thursday = a_rciRow.Wednesday;
                        break;
                    case -1:
                        Rci.Thursday = a_rciRow.Friday;
                        break;
                    default:
                        Rci.Thursday = a_rciRow.Thursday;
                        break;
                }
            }

            if (!a_rciRow.IsFridayNull())
            {
                switch (serverDayAdjustment)
                {
                    case 1:
                        Rci.Friday = a_rciRow.Thursday;
                        break;
                    case -1:
                        Rci.Friday = a_rciRow.Saturday;
                        break;
                    default:
                        Rci.Friday = a_rciRow.Friday;
                        break;
                }
            }

            if (!a_rciRow.IsSaturdayNull())
            {
                switch (serverDayAdjustment)
                {
                    case 1:
                        Rci.Saturday = a_rciRow.Friday;
                        break;
                    case -1:
                        Rci.Saturday = a_rciRow.Sunday;
                        break;
                    default:
                        Rci.Saturday = a_rciRow.Saturday;
                        break;
                }
            }

            if (!a_rciRow.IsMaxNbrRecurrencesNull())
            {
                Rci.MaxNbrRecurrences = a_rciRow.MaxNbrRecurrences;
            }

            if (!a_rciRow.IsMonthlyDayNumberNull())
            {
                Rci.MonthlyDayNumber = a_rciRow.MonthlyDayNumber;
            }

            if (!a_rciRow.IsNbrIntervalsToOverrideNull())
            {
                Rci.NbrIntervalsToOverride = a_rciRow.NbrIntervalsToOverride;
            }

            if (!a_rciRow.IsColorNull())
            {
                Rci.Color = ColorUtils.GetColorFromHexString(a_rciRow.Color);
            }

            if (!a_rciRow.IsResetAttributeChangeoversNull())
            {
                Rci.CleanOutSetups = a_rciRow.ResetAttributeChangeovers;
            }

            if (!a_rciRow.IsPreventOperationsFromSpanningNull())
            {
                Rci.PreventOperationsFromSpanning = a_rciRow.PreventOperationsFromSpanning;
            }

            try
            {
                Rci.Recurrence = (CapacityIntervalDefs.recurrences)Enum.Parse(typeof(CapacityIntervalDefs.recurrences), a_rciRow.Recurrence);
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2854",
                    err,
                    false,
                    new object[]
                    {
                        a_rciRow.Recurrence, "RecurringCapacityInterval", "Recurrence",
                        string.Join(", ", Enum.GetNames(typeof(CapacityIntervalDefs.recurrences)))
                    });
            }

            if (!a_rciRow.IsRecurrenceEndDateTimeNull())
            {
                Rci.RecurrenceEndDateTime = a_rciRow.RecurrenceEndDateTime;
            }

            try
            {
                Rci.RecurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)Enum.Parse(typeof(CapacityIntervalDefs.recurrenceEndTypes), a_rciRow.RecurrenceEndType);
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2854",
                    err,
                    false,
                    new object[]
                    {
                        a_rciRow.RecurrenceEndType, "RecurringCapacityInterval", "RecurrenceEndType",
                        string.Join(", ", Enum.GetNames(typeof(CapacityIntervalDefs.recurrenceEndTypes)))
                    });
            }

            if (!a_rciRow.IsSkipFrequencyNull())
            {
                Rci.SkipFrequency = a_rciRow.SkipFrequency;
            }

            if (!a_rciRow.IsCanStartActivityNull())
            {
                Rci.CanStartActivity = a_rciRow.CanStartActivity;
            }

            if (!a_rciRow.IsUsedForSetupNull())
            {
                Rci.UsedForSetup = a_rciRow.UsedForSetup;
            }

            if (!a_rciRow.IsUsedForRunNull())
            {
                Rci.UsedForRun = a_rciRow.UsedForRun;
            }

            if (!a_rciRow.IsUsedForPostProcessingNull())
            {
                Rci.UsedForPostProcessing = a_rciRow.UsedForPostProcessing;
            }

            if (!a_rciRow.IsUsedForCleanNull())
            {
                Rci.UsedForClean = a_rciRow.UsedForClean;
            }

            if (!a_rciRow.IsUsedForStoragePostProcessingNull())
            {
                Rci.UsedForStorage = a_rciRow.UsedForStoragePostProcessing;
            }

            if (!a_rciRow.IsCapacityCodeNull())
            {
                Rci.CapacityCode = a_rciRow.CapacityCode;
            }

            //Link to all Resources that should have this recurring capacity interval
            RecurringCapacityIntervalTDataSet.ResourcesRow[] resRows = a_rciRow.GetResourcesRows();
            for (int i = 0; i < resRows.Length; i++)
            {
                RecurringCapacityIntervalTDataSet.ResourcesRow resRow = (RecurringCapacityIntervalTDataSet.ResourcesRow)resRows.GetValue(i);
                AddResourceExternalId(resRow.PlantExternalId, resRow.DepartmentExternalId, resRow.ResourceExternalId);
            }
        }

        private void AddResourceExternalId(string a_plantExternalId, string a_deptExternalId, string a_resExternalId)
        {
            ResourceKeyExternal newKey = new(a_plantExternalId, a_deptExternalId, a_resExternalId);
            if (m_resourceKeyList.Contains(newKey))
            {
                throw new APSCommon.PTValidationException("2995", new object[] { ExternalId, a_plantExternalId, a_deptExternalId, a_resExternalId });
            }

            m_resourceKeyList.Add(newKey);
        }

        public override void Validate()
        {
            base.Validate();

            if (Rci.StartDateTime >= Rci.EndDateTime)
            {
                throw new APSCommon.PTValidationException("2996", new object[] { Rci.ExternalId, Rci.StartDateTime, Rci.EndDateTime });
            }

            if (Rci.RecurrenceEndType == CapacityIntervalDefs.recurrenceEndTypes.AfterMaxNbrRecurrences)
            {
                if (!Rci.MaxNbrRecurrencesSet)
                {
                    throw new APSCommon.PTValidationException("3001", new object[] { Rci.ExternalId, "RecurrenceEndType", Rci.RecurrenceEndType.ToString(), "MaxNbrRecurrences", "NULL" });
                }

                if (Rci.RecurrenceEndDateTimeSet)
                {
                    throw new APSCommon.PTValidationException("3001", new object[] { Rci.ExternalId, "RecurrenceEndType", Rci.RecurrenceEndType.ToString(), "RecurrenceEndDateTime", Rci.RecurrenceEndDateTime.ToString() });
                }
            }
            else if (Rci.RecurrenceEndType == CapacityIntervalDefs.recurrenceEndTypes.AfterRecurrenceEndDateTime)
            {
                if (!Rci.RecurrenceEndDateTimeSet)
                {
                    throw new APSCommon.PTValidationException("3001", new object[] { Rci.ExternalId, "RecurrenceEndType", Rci.RecurrenceEndType.ToString(), "RecurrenceEndDateTime", "NULL" });
                }
            }
            else if (Rci.RecurrenceEndType == CapacityIntervalDefs.recurrenceEndTypes.NoEndDate)
            {
                if (Rci.RecurrenceEndDateTimeSet || Rci.MaxNbrRecurrencesSet)
                {
                    throw new APSCommon.PTValidationException("3000", new object[] { Rci.ExternalId, Rci.RecurrenceEndType.ToString() });
                }
            }

            if ((Rci.Recurrence == CapacityIntervalDefs.recurrences.MonthlyByDayNumber || Rci.Recurrence == CapacityIntervalDefs.recurrences.YearlyByMonthDay) && !Rci.MonthlyDayNumberSet)
            {
                throw new APSCommon.PTValidationException("3001", new object[] { Rci.ExternalId, "Recurrence", Rci.Recurrence.ToString(), "MonthlyDayNumber", "NULL" });
            }

            if (Rci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Offline)
            {
                if (Rci.Overtime)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "Overtime" });
                }
                if (Rci.CanStartActivity)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "CanStartActivity" });
                }
                if (Rci.UseOnlyWhenLate)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "UseOnlyWhenLate" });
                }
                if (Rci.UsedForSetup)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "UsedForSetup" });
                }
                if (Rci.UsedForRun)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "UsedForRun" });
                }
                if (Rci.UsedForPostProcessing)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "UsedForPostProcessing" });
                }
                if (Rci.UsedForStorage)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "UsedForStoragePostProcessing" });
                }
                if (Rci.UsedForClean)
                {
                    throw new APSCommon.PTValidationException("3070", new object[] { Rci.ExternalId, "UsedForClean" });
                }
            }

            TimeSpan duration = Rci.EndDateTime - Rci.StartDateTime;
            if (duration > TimeSpan.FromHours(24))
            {
                throw new APSCommon.PTValidationException("3077", new object[] { Rci.ExternalId, Rci.StartDateTime, Rci.EndDateTime });
            }
        }
    }

}
