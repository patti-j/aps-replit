using System.Collections;
using System.Drawing;

using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public class Job : PTObjectBase, IPTSerializable
    {
        #region PT Serialization
        public Job(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 12054) // Removed travelerReport
            {
                reader.Read(out int val);
                classification = (JobDefs.classifications)val;
                reader.Read(out val);
                commitment = (JobDefs.commitmentTypes)val;
                reader.Read(out hotReason);
                reader.Read(out importance);
                reader.Read(out cancelled);
                reader.Read(out hot);
                reader.Read(out hold);
                reader.Read(out holdReason);
                reader.Read(out holdUntilDateTime);
                reader.Read(out latePenaltyCost);
                reader.Read(out priority);
                reader.Read(out revenue);
                reader.Read(out type);
                reader.Read(out canSpanPlants);
                reader.Read(out doNotDelete);
                reader.Read(out doNotSchedule);
                reader.Read(out orderNumber);
                reader.Read(out customerEmail);
                reader.Read(out agentEmail);
                reader.Read(out oldExternalId);
                reader.Read(out template);
                reader.Read(out almostLateSpan);

                //Was set flag
                reader.Read(out classificationSet);
                reader.Read(out commitmentSet);
                reader.Read(out hotReasonSet);
                reader.Read(out importanceSet);
                reader.Read(out cancelledSet);
                reader.Read(out hotSet);
                reader.Read(out holdSet);
                reader.Read(out holdReasonSet);
                reader.Read(out holdUntilSet);
                reader.Read(out latePenaltyCostSet);
                reader.Read(out prioritySet);
                reader.Read(out revenueSet);
                reader.Read(out typeSet);
                reader.Read(out doNotDeleteSet);
                reader.Read(out doNotScheduleSet);
                reader.Read(out orderNumberSet);
                reader.Read(out customerEmailSet);
                reader.Read(out agentEmailSet);
                reader.Read(out oldExternalIdSet);
                reader.Read(out templateSet);

                reader.Read(out maxEarlyDeliverySpanSet);
                reader.Read(out needDateTimeSet);
                reader.Read(out colorCodeSet);
                //End was set flag

                reader.Read(out maxEarlyDeliverySpan);
                reader.Read(out needDateTime);
                reader.Read(out colorCode);

                bools = new BoolVector32(reader);

                int moCount;
                reader.Read(out moCount);
                for (int i = 0; i < moCount; i++)
                {
                    ManufacturingOrder mo = new (reader);
                    Add(mo);
                }

                reader.Read(out val);
                shipped = (JobDefs.ShippedStatuses)val;
                reader.Read(out destination);
                reader.Read(out m_shippingCost);

                reader.ReadList(out m_customers);
            }
            else if (reader.VersionNumber >= 12025) //Removed agent and customer alerts
            {
                reader.Read(out int val);
                classification = (JobDefs.classifications)val;
                reader.Read(out val);
                commitment = (JobDefs.commitmentTypes)val;
                reader.Read(out hotReason);
                reader.Read(out importance);
                reader.Read(out cancelled);
                reader.Read(out hot);
                reader.Read(out hold);
                reader.Read(out holdReason);
                reader.Read(out holdUntilDateTime);
                reader.Read(out latePenaltyCost);
                reader.Read(out priority);
                reader.Read(out revenue);
                reader.Read(out type);
                reader.Read(out canSpanPlants);
                reader.Read(out doNotDelete);
                reader.Read(out doNotSchedule);
                reader.Read(out orderNumber);
                reader.Read(out customerEmail);
                reader.Read(out agentEmail);
                reader.Read(out oldExternalId);
                reader.Read(out template);
                reader.Read(out almostLateSpan);

                //Was set flag
                reader.Read(out classificationSet);
                reader.Read(out commitmentSet);
                reader.Read(out hotReasonSet);
                reader.Read(out importanceSet);
                reader.Read(out cancelledSet);
                reader.Read(out hotSet);
                reader.Read(out holdSet);
                reader.Read(out holdReasonSet);
                reader.Read(out holdUntilSet);
                reader.Read(out latePenaltyCostSet);
                reader.Read(out prioritySet);
                reader.Read(out revenueSet);
                reader.Read(out typeSet);
                reader.Read(out doNotDeleteSet);
                reader.Read(out doNotScheduleSet);
                reader.Read(out orderNumberSet);
                reader.Read(out customerEmailSet);
                reader.Read(out agentEmailSet);
                reader.Read(out oldExternalIdSet);
                reader.Read(out templateSet);

                reader.Read(out maxEarlyDeliverySpanSet);
                reader.Read(out needDateTimeSet);
                reader.Read(out colorCodeSet);
                //End was set flag

                reader.Read(out maxEarlyDeliverySpan);
                reader.Read(out needDateTime);
                reader.Read(out colorCode);

                bools = new BoolVector32(reader);

                int moCount;
                reader.Read(out moCount);
                for (int i = 0; i < moCount; i++)
                {
                    ManufacturingOrder mo = new (reader);
                    Add(mo);
                }

                reader.Read(out val);
                shipped = (JobDefs.ShippedStatuses)val;
                reader.Read(out destination);
                reader.Read(out string _travelerReport);
                reader.Read(out m_shippingCost);

                reader.ReadList(out m_customers);
            }
            else if (reader.VersionNumber >= 12001)
            {
                reader.Read(out int val);
                classification = (JobDefs.classifications)val;
                reader.Read(out val);
                commitment = (JobDefs.commitmentTypes)val;
                reader.Read(out hotReason);
                reader.Read(out importance);
                reader.Read(out cancelled);
                reader.Read(out hot);
                reader.Read(out hold);
                reader.Read(out holdReason);
                reader.Read(out holdUntilDateTime);
                reader.Read(out latePenaltyCost);
                reader.Read(out priority);
                reader.Read(out revenue);
                reader.Read(out type);
                reader.Read(out canSpanPlants);
                reader.Read(out doNotDelete);
                reader.Read(out doNotSchedule);
                reader.Read(out orderNumber);
                reader.Read(out customerEmail);
                reader.Read(out agentEmail);
                reader.Read(out int customerAlerts);
                reader.Read(out int agentAlerts);
                reader.Read(out oldExternalId);
                reader.Read(out template);
                reader.Read(out almostLateSpan);

                //Was set flag
                reader.Read(out classificationSet);
                reader.Read(out commitmentSet);
                reader.Read(out hotReasonSet);
                reader.Read(out importanceSet);
                reader.Read(out cancelledSet);
                reader.Read(out hotSet);
                reader.Read(out holdSet);
                reader.Read(out holdReasonSet);
                reader.Read(out holdUntilSet);
                reader.Read(out latePenaltyCostSet);
                reader.Read(out prioritySet);
                reader.Read(out revenueSet);
                reader.Read(out typeSet);
                reader.Read(out doNotDeleteSet);
                reader.Read(out doNotScheduleSet);
                reader.Read(out orderNumberSet);
                reader.Read(out customerEmailSet);
                reader.Read(out agentEmailSet);
                reader.Read(out bool customerAlertSet);
                reader.Read(out bool agentAlertSet);
                reader.Read(out oldExternalIdSet);
                reader.Read(out templateSet);

                reader.Read(out maxEarlyDeliverySpanSet);
                reader.Read(out needDateTimeSet);
                reader.Read(out colorCodeSet);
                //End was set flag

                reader.Read(out maxEarlyDeliverySpan);
                reader.Read(out needDateTime);
                reader.Read(out colorCode);

                bools = new BoolVector32(reader);

                int moCount;
                reader.Read(out moCount);
                for (int i = 0; i < moCount; i++)
                {
                    ManufacturingOrder mo = new (reader);
                    Add(mo);
                }

                reader.Read(out val);
                shipped = (JobDefs.ShippedStatuses)val;
                reader.Read(out destination);
                reader.Read(out string _travelerReport);
                reader.Read(out m_shippingCost);

                reader.ReadList(out m_customers);
            }
            else
            {
                string customerExternalId = string.Empty;
                if (reader.VersionNumber >= 411)
                {
                    reader.Read(out int val);
                    classification = (JobDefs.classifications)val;
                    reader.Read(out val);
                    commitment = (JobDefs.commitmentTypes)val;
                    reader.Read(out customerExternalId);
                    reader.Read(out hotReason);
                    reader.Read(out importance);
                    reader.Read(out cancelled);
                    reader.Read(out hot);
                    reader.Read(out hold);
                    reader.Read(out holdReason);
                    reader.Read(out holdUntilDateTime);
                    reader.Read(out latePenaltyCost);
                    reader.Read(out priority);
                    reader.Read(out revenue);
                    reader.Read(out type);
                    reader.Read(out canSpanPlants);
                    reader.Read(out doNotDelete);
                    reader.Read(out doNotSchedule);
                    reader.Read(out orderNumber);
                    reader.Read(out customerEmail);
                    reader.Read(out agentEmail);
                    reader.Read(out int customerAlerts);
                    reader.Read(out int agentAlerts);
                    reader.Read(out oldExternalId);
                    reader.Read(out template);
                    reader.Read(out almostLateSpan);

                    //Was set flag
                    reader.Read(out classificationSet);
                    reader.Read(out commitmentSet);
                    reader.Read(out bool _); //CustomerExternalIdSet deprecated
                    reader.Read(out hotReasonSet);
                    reader.Read(out importanceSet);
                    reader.Read(out cancelledSet);
                    reader.Read(out hotSet);
                    reader.Read(out holdSet);
                    reader.Read(out holdReasonSet);
                    reader.Read(out holdUntilSet);
                    reader.Read(out latePenaltyCostSet);
                    reader.Read(out prioritySet);
                    reader.Read(out revenueSet);
                    reader.Read(out typeSet);
                    reader.Read(out doNotDeleteSet);
                    reader.Read(out doNotScheduleSet);
                    reader.Read(out orderNumberSet);
                    reader.Read(out customerEmailSet);
                    reader.Read(out agentEmailSet);
                    reader.Read(out bool customerAlertSet);
                    reader.Read(out bool agentAlertSet);
                    reader.Read(out oldExternalIdSet);
                    reader.Read(out templateSet);

                    reader.Read(out maxEarlyDeliverySpanSet);
                    reader.Read(out needDateTimeSet);
                    reader.Read(out colorCodeSet);
                    //End was set flag

                    reader.Read(out maxEarlyDeliverySpan);
                    reader.Read(out needDateTime);
                    reader.Read(out colorCode);

                    bools = new BoolVector32(reader);

                    int moCount;
                    reader.Read(out moCount);
                    for (int i = 0; i < moCount; i++)
                    {
                        ManufacturingOrder mo = new (reader);
                        Add(mo);
                    }

                    reader.Read(out val);
                    shipped = (JobDefs.ShippedStatuses)val;
                    reader.Read(out destination);
                    reader.Read(out string _travelerReport);
                    reader.Read(out m_shippingCost);
                }

                //Add to customer list backwards compatibility
                if (!string.IsNullOrEmpty(customerExternalId))
                {
                    m_customers.Add(customerExternalId);
                }
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)classification);
            writer.Write((int)commitment);
            writer.Write(hotReason);
            writer.Write(importance);
            writer.Write(cancelled);
            writer.Write(hot);
            writer.Write(hold);
            writer.Write(holdReason);
            writer.Write(holdUntilDateTime);
            writer.Write(latePenaltyCost);
            writer.Write(priority);
            writer.Write(revenue);
            writer.Write(type);
            writer.Write(canSpanPlants);
            writer.Write(doNotDelete);
            writer.Write(doNotSchedule);
            writer.Write(orderNumber);
            writer.Write(customerEmail);
            writer.Write(agentEmail);
            writer.Write(oldExternalId);
            writer.Write(template);
            writer.Write(almostLateSpan);

            //Was set flag
            writer.Write(classificationSet);
            writer.Write(commitmentSet);
            writer.Write(hotReasonSet);
            writer.Write(importanceSet);
            writer.Write(cancelledSet);
            writer.Write(hotSet);
            writer.Write(holdSet);
            writer.Write(holdReasonSet);
            writer.Write(holdUntilSet);
            writer.Write(latePenaltyCostSet);
            writer.Write(prioritySet);
            writer.Write(revenueSet);
            writer.Write(typeSet);
            writer.Write(doNotDeleteSet);
            writer.Write(doNotScheduleSet);
            writer.Write(orderNumberSet);
            writer.Write(customerEmailSet);
            writer.Write(agentEmailSet);
            writer.Write(oldExternalIdSet);
            writer.Write(templateSet);

            writer.Write(maxEarlyDeliverySpanSet);
            writer.Write(needDateTimeSet);
            writer.Write(colorCodeSet);
            //End was set flag

            writer.Write(maxEarlyDeliverySpan);
            writer.Write(needDateTime);
            writer.Write(colorCode);

            bools.Serialize(writer);

            writer.Write(ManufacturingOrderCount);
            for (int i = 0; i < ManufacturingOrderCount; i++)
            {
                this[i].Serialize(writer);
            }

            writer.Write((int)shipped);
            writer.Write(destination);
            writer.Write(m_shippingCost);
            writer.WriteList(m_customers);
        }

        public new const int UNIQUE_ID = 222;

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public Job() { } // reqd. for xml serialization

        public Job(string externalId, string name)
            :
            base(externalId, name) { }

        public Job(JobDataSet.JobRow jobRow, string newJobExternalId)
            : base(newJobExternalId)
        {
            //Set base values this way in case they're null;
            Name = jobRow.Name;
            if (!jobRow.IsDescriptionNull())
            {
                Description = jobRow.Description;
            }

            if (!jobRow.IsNotesNull())
            {
                Notes = jobRow.Notes;
            }

            if (!jobRow.IsCancelledNull())
            {
                Cancelled = jobRow.Cancelled;
            }

            if (!jobRow.IsReviewedNull())
            {
                Reviewed = jobRow.Reviewed;
            }

            if (!jobRow.IsCanSpanPlantsNull())
            {
                CanSpanPlants = jobRow.CanSpanPlants;
            }

            if (!jobRow.IsClassificationNull())
            {
                try
                {
                    Classification = (JobDefs.classifications)Enum.Parse(typeof(JobDefs.classifications), jobRow.Classification);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            jobRow.Classification, "Job", "Classification",
                            string.Join(", ", Enum.GetNames(typeof(JobDefs.classifications)))
                        });
                }
            }

            if (!jobRow.IsColorCodeNull())
            {
                ColorCode = ColorUtils.GetColorFromHexString(jobRow.ColorCode);
            }

            if (!jobRow.IsCommitmentNull())
            {
                try
                {
                    Commitment = (JobDefs.commitmentTypes)Enum.Parse(typeof(JobDefs.commitmentTypes), jobRow.Commitment);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            jobRow.Commitment, "Job", "Commitment",
                            string.Join(", ", Enum.GetNames(typeof(JobDefs.commitmentTypes)))
                        });
                }
            }

            if (!jobRow.IsHotNull())
            {
                Hot = jobRow.Hot;
            }

            if (!jobRow.IsHotReasonNull())
            {
                HotReason = jobRow.HotReason;
            }

            if (!jobRow.IsImportanceNull())
            {
                Importance = jobRow.Importance;
            }

            if (!jobRow.IsLatePenaltyCostNull())
            {
                LatePenaltyCost = jobRow.LatePenaltyCost;
            }

            if (!jobRow.IsShippingCostNull())
            {
                ShippingCost = jobRow.ShippingCost;
            }

            if (!jobRow.IsMaxEarlyDeliveryDaysNull())
            {
                MaxEarlyDeliverySpan = PTDateTime.GetSafeTimeSpan(jobRow.MaxEarlyDeliveryDays * 24);
            }

            if (!jobRow.IsAlmostLateDaysNull())
            {
                AlmostLateSpan = PTDateTime.GetSafeTimeSpan(jobRow.AlmostLateDays * 24);
            }

            if (!jobRow.IsNeedDateTimeNull())
            {
                NeedDateTime = jobRow.NeedDateTime.ToServerTime();
            }

            if (!jobRow.IsOrderNumberNull())
            {
                OrderNumber = jobRow.OrderNumber;
            }

            if (!jobRow.IsCustomerEmailNull())
            {
                CustomerEmail = jobRow.CustomerEmail;
            }

            if (!jobRow.IsAgentEmailNull())
            {
                AgentEmail = jobRow.AgentEmail;
            }

            if (!jobRow.IsPriorityNull())
            {
                Priority = jobRow.Priority;
            }

            if (!jobRow.IsRevenueNull())
            {
                Revenue = jobRow.Revenue;
            }

            if (!jobRow.IsTypeNull())
            {
                Type = jobRow.Type;
            }

            if (!jobRow.IsDoNotDeleteNull())
            {
                DoNotDelete = jobRow.DoNotDelete;
            }

            if (!jobRow.IsDoNotScheduleNull())
            {
                DoNotSchedule = jobRow.DoNotSchedule;
            }

            if (!jobRow.IsTemplateNull())
            {
                Template = jobRow.Template;
            }

            //Hold
            if (!jobRow.IsHoldReasonNull())
            {
                HoldReason = jobRow.HoldReason;
            }

            if (!jobRow.IsHoldUntilDateNull())
            {
                HoldUntilDate = jobRow.HoldUntilDate.ToServerTime();
            }

            if (!jobRow.IsHoldNull()) //do after setting reason so it's cleared if unheld.
            {
                Hold = jobRow.Hold;
            }

            if (!jobRow.IsUserFieldsNull())
            {
                SetUserFields(jobRow.UserFields);
            }

            if (!jobRow.IsReviewedNull())
            {
                Reviewed = jobRow.Reviewed;
            }

            if (!jobRow.IsInvoicedNull())
            {
                Invoiced = jobRow.Invoiced;
            }

            if (!jobRow.IsPrintedNull())
            {
                Printed = jobRow.Printed;
            }

            if (!jobRow.IsShippedNull())
            {
                try
                {
                    Shipped = (JobDefs.ShippedStatuses)jobRow.Shipped;
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            jobRow.Shipped, "Job", "Shipped",
                            string.Join(", ", Enum.GetNames(typeof(JobDefs.ShippedStatuses)))
                        });
                }
            }

            if (!jobRow.IsDestinationNull())
            {
                Destination = jobRow.Destination;
            }

            if (!jobRow.IsOldExternalIdNull())
            {
                OldExternalId = jobRow.OldExternalId;
            }

            JobDataSet.CustomerRow[] customerRows = jobRow.GetCustomerRows();
            foreach (JobDataSet.CustomerRow customerRow in customerRows)
            {
                m_customers.Add(customerRow.CustomerExternalId);
            }
        }

        #region Shared Properties
        private JobDefs.classifications classification = JobDefs.classifications.ProductionOrder;

        /// <summary>
        /// Can be used to distinguish the purpose of the work request.
        /// Set to Safety Stock to use the Inventory option to ignore lateness on Safety Stock Jobs.
        /// Set to Buffer Stock Replenishment to use Buffer Stock penetration instead of DBR penetration.
        /// Default is Order.
        /// </summary>
        public JobDefs.classifications Classification
        {
            get => classification;
            set
            {
                classification = value;
                classificationSet = true;
            }
        }

        private bool classificationSet;

        public bool ClassificationSet => classificationSet;

        private bool doNotDelete;

        /// <summary>
        /// If true then the Job will not be deleted by the system.  This can be used to keep Jobs that will be used as templates for copying to new Jobs.
        /// This value can be set by the interface but not updated.  This is to preserve manual changes by the planner.
        /// </summary>
        public bool DoNotDelete
        {
            get => doNotDelete;
            set
            {
                doNotDelete = value;
                doNotDeleteSet = true;
            }
        }

        private bool doNotDeleteSet;

        public bool DoNotDeleteSet => doNotDeleteSet;

        private bool doNotSchedule;

        /// <summary>
        /// If true then the Job will not be scheduled.  This can be used to give the planner control over when quotes, etc can enter the schedule.
        /// This value can be set by the interface but not updated.  This is to preserve manual changes by the planner.
        /// </summary>
        public bool DoNotSchedule
        {
            get => doNotSchedule;
            set
            {
                doNotSchedule = value;
                doNotScheduleSet = true;
            }
        }

        private bool doNotScheduleSet;

        public bool DoNotScheduleSet => doNotScheduleSet;

        private string orderNumber = "";

        /// <summary>
        /// Can be used to specify a customer order number for reference.
        /// </summary>
        public string OrderNumber
        {
            get => orderNumber;
            set
            {
                orderNumber = value;
                orderNumberSet = true;
            }
        }

        private bool orderNumberSet;

        public bool OrderNumberSet => orderNumberSet;

        private JobDefs.commitmentTypes commitment = JobDefs.commitmentTypes.Firm;

        /// <summary>
        /// Indicates the likelihood that the work will be executed.	For display and custom Algorithms.
        /// </summary>
        public JobDefs.commitmentTypes Commitment
        {
            get => commitment;
            set
            {
                commitment = value;
                commitmentSet = true;
            }
        }

        private bool commitmentSet;

        public bool CommitmentSet => commitmentSet;

        private string hotReason = "";

        /// <summary>
        /// Explanation of why the MO is on Hot.
        /// </summary>
        public string HotReason
        {
            get => hotReason;
            set
            {
                hotReason = value;
                hotReasonSet = true;
            }
        }

        private bool hotReasonSet;

        public bool HotReasonSet => hotReasonSet;

        private int importance;

        /// <summary>
        /// /// The value of the MO relative to other MOs.
        /// </summary>
        public int Importance
        {
            get => importance;
            set
            {
                importance = value;
                importanceSet = true;
            }
        }

        private bool importanceSet;

        public bool ImportanceSet => importanceSet;

        private bool cancelled;

        /// <summary>
        /// If cancelled, the MO won't be scheduled.
        /// </summary>
        public bool Cancelled
        {
            get => cancelled;
            set
            {
                cancelled = value;
                cancelledSet = true;
            }
        }

        private bool cancelledSet;

        public bool CancelledSet => cancelledSet;

        private Color colorCode = Color.Black;

        /// <summary>
        /// A Color that can be used to distinguish the Job from other Jobs in the Gantt.
        /// When importing, this can be set to a Known Color like 'Red' or the RGB values can be specified.
        /// The Windows Known Colors are: AliceBlue, AntiqueWhite, Aqua, Aquamarine, Azure, Beige, Bisque, Black, BlanchedAlmond, Blue, BlueViolet, Brown, BurlyWood, CadetBlue, Chartreuse, Chocolate, Coral
        /// , CornflowerBlue, Cornsilk, Crimson, Cyan, DarkBlue, DarkCyan, DarkGoldenrod, DarkGray, DarkGreen, DarkKhaki, DarkMagenta, DarkOliveGreen, DarkOrange, DarkOrchid, DarkRed, DarkSalmon, DarkSeaGreen
        /// , DarkSlateBlue, DarkSlateGray, DarkTurquoise, DarkViolet, DeepPink, DeepSkyBlue, DimGray, DodgerBlue, Firebrick, FloralWhite, ForestGreen, Fuchsia, Gainsboro, GhostWhite, Gold, Goldenrod, Gray
        /// , Green, GreenYellow, Honeydew, HotPink, IndianRed, Indigo, Ivory, Khaki, Lavender, LavenderBlush, LawnGreen, LemonChiffon, LightBlue, LightCoral, LightCyan, LightGoldenrodYellow, LightGray,
        /// LightGreen
        /// , LightPink, LightSalmon, LightSeaGreen, LightSkyBlue, LightSlateGray, LightSteelBlue, LightYellow, Lime, LimeGreen, Linen, Magenta, Maroon, MediumAquamarine, MediumBlue, MediumOrchid, MediumPurple,
        /// MediumSeaGreen
        /// , MediumSlateBlue, MediumSpringGreen, MediumTurquoise, MediumVioletRed, MidnightBlue, MintCream, MistyRose, Moccasin, NavajoWhite, Navy, OldLace, Olive, OliveDrab, Orange, OrangeRed, Orchid,
        /// PaleGoldenrod, PaleGreen, PaleTurquoise
        /// , PaleVioletRed, PapayaWhip, PeachPuff, Peru, Pink, Plum, PowderBlue, Purple, Red, RosyBrown, RoyalBlue, SaddleBrown, Salmon, SandyBrown, SeaGreen, SeaShell, Sienna, Silver, SkyBlue, SlateBlue,
        /// SlateGray
        /// , Snow, SpringGreen, SteelBlue, Tan, Teal, Thistle, Tomato, Transparent, Turquoise, Violet, Wheat, White, WhiteSmoke, Yellow, YellowGreen
        /// </summary>
        public Color ColorCode
        {
            get => colorCode;
            set
            {
                colorCode = value;
                colorCodeSet = true;
            }
        }

        private bool colorCodeSet;

        public bool ColorCodeSet => colorCodeSet;

        private bool hot;

        /// <summary>
        /// Especially important.  For use in simulations, Flags, etc.
        /// </summary>
        public bool Hot
        {
            get => hot;
            set
            {
                hot = value;
                hotSet = true;
            }
        }

        private bool hotSet;

        public bool HotSet => hotSet;

        private bool hold;

        /// <summary>
        /// Whether the Job was placed On-hold and work should not be done on it.
        /// </summary>
        public bool Hold
        {
            get => hold;
            set
            {
                hold = value;
                holdSet = true;
            }
        }

        private bool holdSet;

        public bool HoldSet => holdSet;

        private string holdReason = "";

        /// <summary>
        /// The reason the Job was placed On-Hold
        /// </summary>
        public string HoldReason
        {
            get => holdReason;
            set
            {
                holdReason = value;
                holdReasonSet = true;
            }
        }

        private bool holdReasonSet;

        public bool HoldReasonSet => holdReasonSet;

        private long holdUntilDateTime;

        /// <summary>
        /// No Activities are scheduled before this date/time.
        /// This value is only set if the Job itself is placed On Hold, not if Operations, etc. only are placed On Hold.
        /// </summary>
        public DateTime HoldUntilDate
        {
            get => new (holdUntilDateTime);
            set
            {
                holdUntilDateTime = value.Ticks;
                holdUntilSet = true;
            }
        }

        private bool holdUntilSet;

        public bool HoldUntilSet => holdUntilSet;

        private decimal latePenaltyCost;

        /// <summary>
        /// Optional currency value that specifies the cost (either actual or estimated) per day of finishing late.  Can be used for comparing schedules and in custom Algorithms.
        /// </summary>
        public decimal LatePenaltyCost
        {
            get => latePenaltyCost;
            set
            {
                latePenaltyCost = value;
                latePenaltyCostSet = true;
            }
        }

        private bool latePenaltyCostSet;

        public bool LatePenaltyCostSet => latePenaltyCostSet;

        private decimal m_shippingCost;

        public decimal ShippingCost
        {
            get => m_shippingCost;
            set
            {
                m_shippingCost = value;
                bools[ShippingCostSetIdx] = true;
            }
        }

        public bool ShippingCostSet => bools[ShippingCostSetIdx];

        private TimeSpan maxEarlyDeliverySpan = new (0);

        /// <summary>
        /// The customer will accept the order the amount of time before the NeedDateTime.
        /// </summary>
        public TimeSpan MaxEarlyDeliverySpan
        {
            get => maxEarlyDeliverySpan;
            set
            {
                maxEarlyDeliverySpan = value;
                maxEarlyDeliverySpanSet = true;
            }
        }

        private bool maxEarlyDeliverySpanSet;

        public bool MaxEarlyDeliverySpanSet => maxEarlyDeliverySpanSet;

        private TimeSpan almostLateSpan;

        public TimeSpan AlmostLateSpan
        {
            get => almostLateSpan;
            set
            {
                almostLateSpan = value;
                bools[AlmostLateIdx] = true;
            }
        }

        public bool AlmostLateSpanSet => bools[AlmostLateIdx];

        private DateTime needDateTime;

        /// <summary>
        /// When to finish by to be considered on-time.
        /// </summary>
        [Required(true)]
        public DateTime NeedDateTime
        {
            get => needDateTime;
            set
            {
                needDateTime = value;
                needDateTimeSet = true;
            }
        }

        private bool needDateTimeSet;

        public bool NeedDateTimeSet => needDateTimeSet;

        private int priority;

        /// <summary>
        /// Usually used to specify a combination of importance and urgency.  Used in simulation algorithms.
        /// </summary>
        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
                prioritySet = true;
            }
        }

        private bool prioritySet;

        public bool PrioritySet => prioritySet;

        private decimal revenue;

        /// <summary>
        /// Real or estimated value.  Can be used in Custom Algorithms.
        /// </summary>
        public decimal Revenue
        {
            get => revenue;
            set
            {
                revenue = value;
                revenueSet = true;
            }
        }

        private bool revenueSet;

        public bool RevenueSet => revenueSet;

        private string type = "";

        /// <summary>
        /// Can be used to specify a free-form type for grouping.  For custom Algorithms and display.
        /// </summary>
        public string Type
        {
            get => type;
            set
            {
                type = value;
                typeSet = true;
            }
        }

        private bool typeSet;

        public bool TypeSet => typeSet;

        private decimal DEPRECATED_PROFIT;
        private bool DEPRECATED_PROFIT_SET;

        private string customerEmail = "";

        /// <summary>
        /// E-mail address of the customer to be alerted when the Job changes.
        /// Separate multiple addresses with semi-colons.
        /// </summary>
        public string CustomerEmail
        {
            get => customerEmail;
            set
            {
                customerEmail = value;
                customerEmailSet = true;
            }
        }

        private bool customerEmailSet;

        public bool CustomerEmailSet => customerEmailSet;

        private string agentEmail = "";

        /// <summary>
        /// E-mail address of the sales or customer service agents to be alerted when the Job changes.
        /// Separate multiple addresses with semi-colons.
        /// </summary>
        public string AgentEmail
        {
            get => agentEmail;
            set
            {
                agentEmail = value;
                agentEmailSet = true;
            }
        }

        private bool agentEmailSet;

        public bool AgentEmailSet => agentEmailSet;

        private string oldExternalId;

        /// <summary>
        /// If a Job with the specified ExternalId doesn't exist then a Job with this OldExternalId
        /// is searched for and updated if it exists.  If this doesn't exist either then a new Job
        /// is created with the ExternalId (not the OldExternalId).
        /// This can be useful in cases where a "What-If" Job is entered and later a real
        /// Job is entered from an ERP system.  The new Job's Old ExternalId can be set to the What-If Job's
        /// ExternalId so that the What-If Job is updated by the new Job.
        /// </summary>
        public string OldExternalId
        {
            get => oldExternalId;
            set
            {
                oldExternalId = value;
                oldExternalIdSet = true;
            }
        }

        private bool oldExternalIdSet;

        public bool OldExternalIdSet => oldExternalIdSet;

        private bool template;

        /// <summary>
        /// Indicates that the Job is only used for copying to create new Jobs.  Template Jobs are not scheduled.
        /// </summary>
        public bool Template
        {
            get => template;
            set
            {
                template = value;
                templateSet = true;
            }
        }

        private bool templateSet;

        public bool TemplateSet => templateSet;

        /// <summary>
        /// Tracks whether the Job has been reviewed by a planner.
        /// Only set during import for NEW Jobs and thereafter controlled internally.
        /// </summary>
        public bool Reviewed
        {
            get => bools[reviewedIdx];
            set => bools[reviewedIdx] = value;
        }

        /// <summary>
        /// Whether an invoice has been sent for the Job.
        /// For information only.
        /// </summary>
        public bool Invoiced
        {
            get => bools[InvoicedIdx];
            set
            {
                bools[InvoicedIdx] = value;
                bools[InvoicedSetIdx] = true;
            }
        }

        public bool InvoicedSet => bools[InvoicedSetIdx];

        /// <summary>
        /// Whether the Job's Traveller Report has been Printed.
        /// </summary>
        public bool Printed
        {
            get => bools[PrintedIdx];
            set
            {
                bools[PrintedIdx] = value;
                bools[PrintedSetIdx] = true;
            }
        }

        public bool PrintedSet => bools[PrintedSetIdx];

        private string destination;

        /// <summary>
        /// Indicates the geographical region or address where the products will be sent.
        /// For information only.
        /// </summary>
        public string Destination
        {
            get => destination;
            set
            {
                destination = value;
                bools[DestinationSetIdx] = true;
            }
        }

        public bool DestinationSet => bools[DestinationSetIdx];

        private JobDefs.ShippedStatuses shipped = JobDefs.ShippedStatuses.NotShipped;

        /// <summary>
        /// Whether the Job has been shipped to the recipient.
        /// For informatiion only.
        /// </summary>
        public JobDefs.ShippedStatuses Shipped
        {
            get => shipped;
            set
            {
                shipped = value;
                bools[ShippedSetIdx] = true;
            }
        }

        public bool ShippedSet => bools[ShippedSetIdx];

        private List<string> m_customers = new ();

        public List<string> Customers
        {
            get => m_customers;
            set => m_customers = value;
        }
        #endregion Shared Properties

        #region BaseOrder Shared Properties
        private BoolVector32 bools;
        private const int canSpanPlantsSetIdx = 0;
        private const int markForDeletionIdx = 1; // Used by the Maximus customizer. When I set this value, it indicates that I need to delete the job.
        private const int reviewedIdx = 2;
        private const int InvoicedIdx = 3;
        private const int PrintedIdx = 4;
        private const int InvoicedSetIdx = 5;
        private const int PrintedSetIdx = 6;

        private const int DestinationSetIdx = 7;

        // 8 is open, the field TravelerReport was removed
        private const int ShippedSetIdx = 9;
        private const int AlmostLateIdx = 10;
        private const int ShippingCostSetIdx = 11;

        #region CanSpanPlants
        private bool canSpanPlants = true;

        /// <summary>
        /// If true, then the Operations can schedule in more than one plant.  Otherwise, all operations must be scheduled in only one Plant.
        /// </summary>
        public virtual bool CanSpanPlants
        {
            get => canSpanPlants;
            set
            {
                canSpanPlants = value;
                CanSpanPlantsSetter = true;
            }
        }

        private bool CanSpanPlantsSetter
        {
            get => bools[canSpanPlantsSetIdx];

            set => bools[canSpanPlantsSetIdx] = value;
        }

        public bool CanSpanPlantsSet => CanSpanPlantsSetter;
        #endregion

        #region Delete Flag
        /// <summary>
        /// Can be used by the Interface Customizer to delete jobs that are batched together, etc.
        /// </summary>
        public bool MarkForDeletion
        {
            set => bools[markForDeletionIdx] = value;

            get => bools[markForDeletionIdx];
        }
        #endregion
        #endregion

        private readonly List<ManufacturingOrder> mos = new ();
        // SGS            ArrayList mos = new ArrayList();

        public void Add(ManufacturingOrder mo)
        {
            mos.Add(mo);
        }

        public int ManufacturingOrderCount => mos.Count;

        public ManufacturingOrder this[int i] => mos[i];

        private void ValidateHasProductForTemplates()
        {
            if (Template)
            {
                foreach (ManufacturingOrder mo in mos) // has to have at least one mo and mo op
                {
                    for (int i = 0; i < mo.OperationCount; i++)
                    {
                        if (mo.GetOperation(i).ProductCount > 0)
                        {
                            return;
                        }
                    }
                }

                throw new ValidationException("2862", new object[] { ExternalId });
            }
        }

        public override void Validate()
        {
            base.Validate();

            if (mos.Count == 0)
            {
                throw new ValidationException("2085", new object[] { ExternalId });
            }

            try
            {
                ValidationException.ValidateArrayList(new ArrayList(mos));
            }
            catch (ValidationException e)
            {
                throw new ValidationException("2086", new object[] { ExternalId, e.Message, e.StackTrace });
            }

            ValidateHasProductForTemplates();

            // ManufacturingOrders are validated as they're added
        }
    }
}