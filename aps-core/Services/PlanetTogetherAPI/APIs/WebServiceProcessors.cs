using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.Extensions;
using PT.PackageDefinitions.Settings;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using PT.Transmissions.CTP;

namespace PT.PlanetTogetherAPI.APIs;

public class WebServiceProcessors
{
    /// <summary>
    /// Process a CtpRequest and return CtpResponse
    /// </summary>
    internal class CtpProcessor : ProcessorBase
    {
        public CtpProcessor(BaseId a_instigator) : base(a_instigator) { }

        private CtpRequest m_ctpRequest;
        private CtpResponse m_ctpResp;

        internal CtpResponse ProcessRequest(CtpRequest a_ctpRequest)
        {
            BaseId scenarioId = new BaseId(a_ctpRequest.ScenarioId);
            DateTime start = DateTime.Now;
            m_ctpRequest = a_ctpRequest;

            if (a_ctpRequest.WaitForResponse)
            {
               scenarioId = ListenToCtpEvent(start, a_ctpRequest.TimeoutMinutes, a_ctpRequest.ScenarioId);
            }

            PTTransmission t;
            if (a_ctpRequest.ReserveMaterialsAndCapacity)
            {
                t = GetScenarioCtpT(a_ctpRequest, scenarioId);
                m_ctpRequest.RequestId = ((ScenarioCtpT)t).ctp.RequestId;
            }
            else
            {
                t = GetCtpT(a_ctpRequest, scenarioId);
                m_ctpRequest.RequestId = ((CtpT)t).ctp.RequestId;
            }

            t.Instigator = m_instigator;
            SystemController.ClientSession.SendClientAction(t);

            if (a_ctpRequest.WaitForResponse)
            {
                // wait until CTP is done or timeout expires.
                while (m_ctpResp == null)
                {
                    if (a_ctpRequest.TimeoutMinutes == 0 || DateTime.Now.Subtract(start).TotalMinutes <= a_ctpRequest.TimeoutMinutes)
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        m_ctpResp = new CtpResponse { ReturnCode = EApsWebServicesResponseCodes.ProcessingTimeout };
                    }
                }

                Task.Run(() => UnListenToCtpEvent()); // call this with a task so we can wait until the job is done without holding up the response.
            }
            
            return m_ctpResp;
        }

        /// <summary>
        /// subscribes to CTP event so we know when CTP has been completed and get the results.
        /// </summary>
        /// <returns></returns>
        private BaseId ListenToCtpEvent(DateTime a_start, double a_timeoutMins, long a_requestedScenarioId)
        {
            while (a_timeoutMins == 0 || DateTime.Now.Subtract(a_start).TotalMinutes <= a_timeoutMins)
            {
                BaseId scenarioId;
                try
                {
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
                    {
                        // Historically, this method always ran CTP on the production scenario.
                        // That seems like a bug since ScenarioId is passed in, but to avoid breaking existing api functionality, I'm leaving it in as a default
                        Scenario requestedScenario = sm.Find(new BaseId(a_requestedScenarioId));

                        if (requestedScenario == null)
                        {
                            requestedScenario = sm.GetFirstProductionScenario();
                        }

                        scenarioId = requestedScenario.Id;

                        using (requestedScenario.ScenarioEventsLock.TryEnterRead(out ScenarioEvents se, 500))
                        {
                            se.CTPEvent += ScenarioEvents_CtpEvent;
                        }

                        return scenarioId;
                    }
                }
                catch (AutoTryEnterException)
                {
                    Thread.Sleep(500);
                }
            }

            // timeout expired, throw processing timeout exception
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.ProcessingTimeout);
        }

        /// <summary>
        /// Stop listening to CTP event so no references are stored and garbage collector can
        /// clean this object after it's done.
        /// </summary>
        private void UnListenToCtpEvent()
        {
            while (true)
            {
                try
                {
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
                    {
                        Scenario s = sm.GetFirstProductionScenario();
                        using (s.ScenarioEventsLock.TryEnterRead(out ScenarioEvents se, 500))
                        {
                            se.CTPEvent -= ScenarioEvents_CtpEvent;
                            return;
                        }
                    }
                }
                catch (AutoTryEnterException)
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// process results of the ctp.
        /// </summary>
        private void ScenarioEvents_CtpEvent(ScenarioBaseT a_t, Ctp a_ctp, Job a_job, Exception a_ctpException)
        {
            SetCtpResponse(a_job, a_ctpException);
        }

        /// <summary>
        /// Processes Job created by CTP (if any) into a CtpResponse and sets the m_ctpResp variable
        /// that indicates the CTP is done and result is ready.
        /// </summary>
        /// <param name="a_job"></param>
        private void SetCtpResponse(Job a_job, Exception a_ctpException)
        {
            if (a_ctpException != null)
            {
                //Something went wrong
                CtpResponse resp = new ();
                resp.ReturnCode = EApsWebServicesResponseCodes.Failure;
                resp.ErrorMessage = a_ctpException.GetExceptionFullMessage();
                resp.CtpResponseLines = new List<CtpResponseLine>();
                m_ctpResp = resp;
                return;
            }

            //No processing exceptions. Try to build the response
            try
            {
                List<CtpResponseLine> respLines = new ();
                List<CtpRequestLine> orderedCtpReqLines = m_ctpRequest.CtpRequestLines.OrderBy(l => l.LineId).ToList();
                if (a_job == null) // no jobs were created, possibly because there's enough available on hand.
                {
                    foreach (CtpRequestLine reqLine in orderedCtpReqLines)
                    {
                        respLines.Add(new CtpResponseLine { LineId = reqLine.LineId });
                    }
                }
                else
                {
                    for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
                    {
                        ManufacturingOrder mo = a_job.ManufacturingOrders[i];
                        //Not sure exactly how MOs map to response lines. This may need updating
                        if (orderedCtpReqLines.Count > i)
                        {
                            int lineId = orderedCtpReqLines[i].LineId;
                            respLines.Add(new CtpResponseLine { LineId = lineId, ScheduledStartDate = mo.ScheduledStartDate, ScheduledEndDate = mo.ScheduledEndDate });
                        }
                        else
                        {
                            //TODO: MOs created for sub jobs within this CTP don't map to multiple CTPRequestLines.
                            break;
                        }
                    }
                }

                CtpResponse resp = new ();
                resp.ReturnCode = EApsWebServicesResponseCodes.Success;
                resp.CtpResponseLines = respLines;
                m_ctpResp = resp;
            }
            catch (Exception e)
            {
                //error building the response
                CtpResponse resp = new ();
                resp.ReturnCode = EApsWebServicesResponseCodes.Failure;
                resp.ErrorMessage = e.GetExceptionFullMessage();
                resp.CtpResponseLines = new List<CtpResponseLine>();
                m_ctpResp = resp;
            }
        }

        private ScenarioCtpT GetScenarioCtpT(CtpRequest a_ctpRequest, BaseId a_scenarioId)
        {
            ScenarioCtpT scenarioCtpT = new (a_scenarioId, null);
            scenarioCtpT.ctp = GetCtpTCtp(a_ctpRequest);
            return scenarioCtpT;
        }

        /// <summary>
        /// for running CTP in what-if
        /// </summary>
        /// <param name="a_ctpRequest"></param>
        /// <param name="a_scenarioId"></param>
        /// <returns></returns>
        private CtpT GetCtpT(CtpRequest a_ctpRequest, BaseId a_scenarioId)
        {
            CtpT ctpT = new (a_scenarioId, null);
            ctpT.ctp = GetCtpTCtp(a_ctpRequest);
            return ctpT;
        }

        private Ctp GetCtpTCtp(CtpRequest a_ctpRequest)
        {
            Ctp ctp = new ();
            ctp.Priority = a_ctpRequest.Priority;
            ctp.Reserve = a_ctpRequest.ReserveMaterialsAndCapacity;
            ctp.SchedulingType = Enum.Parse<CtpDefs.ESchedulingType>(a_ctpRequest.SchedulingType);
            foreach (CtpRequestLine line in a_ctpRequest.CtpRequestLines.OrderBy(cl => cl.LineId)) // order them so we can get the right LineId later by the index
            {
                ctp.CtpLineList.Add(new CtpLine(line.WarehouseExternalId, line.ItemExternalId, line.RequiredQty, line.NeedDate));
            }

            ctp.RequestId = a_ctpRequest.RequestId;
            

            return ctp;
        }
    }

    /// <summary>
    /// Base class used by all processors to acquire the latest transmission number.
    /// </summary>
    internal class ProcessorBase
    {
        protected readonly BaseId m_instigator;

        internal ProcessorBase(BaseId a_instigator)
        {
            m_instigator = a_instigator;
        }
    }

    /// <summary>
    /// Process a CopyScenarioRequest to initiate a scenario copy and return CopyScenarioResponse confirmation at creation ScenarioNewEvent trigger
    /// </summary>
    internal class CopyScenarioProcessor : ProcessorBase
    {
        private CopyScenarioResponse m_copyScenarioResp;
        private EApsWebServicesResponseCodes m_copyResponseCode = EApsWebServicesResponseCodes.SuccessWithoutValidation;

        public CopyScenarioProcessor(BaseId a_instigator) : base(a_instigator) { }

        internal CopyScenarioResponse ProcessRequest(CopyScenarioRequest a_copyScenarioRequest)
        {
            DateTime start = DateTime.Now;
            BaseId liveScenarioId = ListenToScenarioNewEvent(start, a_copyScenarioRequest.TimeoutMinutes);
            ScenarioCopyT t = GetScenarioCopyT(a_copyScenarioRequest, liveScenarioId);
            if (t != null)
            {
                t.Instigator = m_instigator;
                SystemController.ClientSession.SendClientAction(t);

                // wait until scenario creation is done or timeout expires.
                while (m_copyScenarioResp == null)
                {
                    if (a_copyScenarioRequest.TimeoutMinutes == 0 || DateTime.Now.Subtract(start).TotalMinutes <= a_copyScenarioRequest.TimeoutMinutes)
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        m_copyScenarioResp = new CopyScenarioResponse { ResponseCode = EApsWebServicesResponseCodes.ProcessingTimeout };
                    }
                }
            }

            Task.Run(() => UnListenToScenarioNewEvent()); // call this with a task so we can wait without holding up the response.

            return m_copyScenarioResp;
        }

        private ScenarioCopyT GetScenarioCopyT(CopyScenarioRequest a_request, BaseId a_liveScenarioId)
        {
            //Initialize copy target variables
            ScenarioCopyT copyT = null;

            //Scenario validation variables
            bool createIfNew = a_request.CreateScenarioIfNew;
            BaseId importScenarioId = new (a_request.ScenarioId);
            string importedIdValidatedName = "";
            bool validImportScenarioId = false;
            string importScenarioName = a_request.ScenarioName;
            BaseId importedNameValidatedId = BaseId.NULL_ID;
            bool validImportName = false;

            //Validate Scenario
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 5000))
            {
                //Target Scenario input validation
                for (int i = 0; i < sm.LoadedScenarioCount; i++)
                {
                    Scenario scenario = sm.GetByIndex(i);
                    using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        //Verify by Id
                        if (importScenarioId != BaseId.NULL_ID && ss.Id == importScenarioId)
                        {
                            validImportScenarioId = true;
                            importedIdValidatedName = ss.Name;
                        }

                        //Verify by name
                        if (!string.IsNullOrEmpty(importScenarioName) && ss.Name == importScenarioName)
                        {
                            validImportName = true;
                            importedNameValidatedId = ss.Id;
                        }
                    }
                }
            }

            //Target Scenario assignment
            string targetScenarioName;
            BaseId targetScenarioId;
            if (validImportName && validImportScenarioId) //name and id args provided
            {
                //Both values valid, Id precedence
                targetScenarioId = importScenarioId;
                targetScenarioName = importedIdValidatedName;
                m_copyResponseCode = EApsWebServicesResponseCodes.Success;
            }
            else if (validImportName)
            {
                targetScenarioId = importedNameValidatedId;
                targetScenarioName = importScenarioName;

                m_copyResponseCode = importScenarioId != BaseId.NULL_ID ? EApsWebServicesResponseCodes.InvalidScenarioId : EApsWebServicesResponseCodes.Success;
            }
            else if (validImportScenarioId)
            {
                targetScenarioId = importScenarioId;
                targetScenarioName = importedIdValidatedName;

                m_copyResponseCode = !string.IsNullOrEmpty(importScenarioName) ? EApsWebServicesResponseCodes.InvalidScenarioName : EApsWebServicesResponseCodes.Success;
            }
            else //name and id invalid (no matching scenario exists)
            {
                //validate user defined naming
                targetScenarioName = createIfNew ? !string.IsNullOrEmpty(importScenarioName) ? importScenarioName : "" : "";
                targetScenarioId = a_liveScenarioId;

                m_copyResponseCode = createIfNew ? EApsWebServicesResponseCodes.CreatedScenario : EApsWebServicesResponseCodes.InvalidScenarioIdAndName;
            }

            copyT = new ScenarioCopyT(targetScenarioId, ScenarioTypes.Whatif, targetScenarioName);
            copyT.Instigator = m_instigator;
            copyT.SetTimeStamp(DateTime.Now);
            copyT.IsBlackBoxScenario = a_request.IsBlackBoxScenario;

            return copyT;
        }

        private BaseId ListenToScenarioNewEvent(DateTime a_start, double a_timeoutMinutes)
        {
            BaseId scenarioId;

            //Attach listner
            try
            {
                ScenarioManager sm;
                using (SystemController.Sys.ScenariosLock.EnterRead(out sm))
                {
                    sm.ScenarioNewEvent += new ScenarioManager.ScenarioNewDelegate(sm_ScenarioNewEvent);
                }
            }
            catch (AutoTryEnterException)
            {
                Thread.Sleep(500);
            }

            while (a_timeoutMinutes == 0 || DateTime.Now.Subtract(a_start).TotalMinutes <= a_timeoutMinutes)
            {
                try
                {
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
                    {
                        Scenario s = sm.GetFirstProductionScenario();
                        return s.Id;
                    }
                }
                catch (AutoTryEnterException)
                {
                    Thread.Sleep(500);
                }
            }

            // timeout expired, throw processing timeout exception
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.ProcessingTimeout);
        }

        private void sm_ScenarioNewEvent(BaseId a_scenarioId, ScenarioBaseT t)
        {
            Scenario s;
            using (SystemController.Sys.AutoEnterRead(a_scenarioId, out s))
            {
                try
                {
                    ScenarioConfirmation conf = new () { ScenarioId = s.Id.Value, ScenarioName = s.Name, ScenarioType = s.Type.ToString() };
                    m_copyScenarioResp = new CopyScenarioResponse { Confirmation = conf };
                    m_copyScenarioResp.ResponseCode = EApsWebServicesResponseCodes.Success;

                    if (t is ScenarioCopyT or ScenarioLoadT)
                    {
                        if (m_copyResponseCode != EApsWebServicesResponseCodes.Success && m_copyResponseCode != EApsWebServicesResponseCodes.CreatedScenario)
                        {
                            m_copyScenarioResp.Exception = true;
                            m_copyScenarioResp.ErrorMessage = m_copyResponseCode.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    //error building the response
                    CopyScenarioResponse resp = new () { Confirmation = new ScenarioConfirmation() };
                    resp.ResponseCode = EApsWebServicesResponseCodes.Failure;
                    resp.ErrorMessage = e.GetExceptionFullMessage();
                    m_copyScenarioResp = resp;
                }
            }
        }

        private void UnListenToScenarioNewEvent()
        {
            while (true)
            {
                try
                {
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
                    {
                        sm.ScenarioNewEvent -= new ScenarioManager.ScenarioNewDelegate(sm_ScenarioNewEvent);
                        return;
                    }
                }
                catch (AutoTryEnterException)
                {
                    Thread.Sleep(500);
                }
            }
        }
    }

    /// <summary>
    /// Process a GetScenariosRequest and return a list of scenarios of specified type (or all types)
    /// </summary>
    internal class GetScenariosProcessor : ProcessorBase
    {
        private GetScenariosResponse m_getScenariosResp;

        public GetScenariosProcessor(BaseId a_instigator) : base(a_instigator) { }

        internal GetScenariosResponse ProcessRequest(GetScenariosRequest a_getScenariosRequest)
        {
            List<ScenarioConfirmation> confs = new();
            DateTime start = DateTime.Now;

            while (m_getScenariosResp == null)
            {
                if (DateTime.Now.Subtract(start).TotalMinutes >= a_getScenariosRequest.TimeoutMinutes)
                {
                    m_getScenariosResp = new GetScenariosResponse { ResponseCode = EApsWebServicesResponseCodes.ProcessingTimeout };
                }
                else
                {
                    try
                    {
                        using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 5000))
                        {
                            //Target Scenario input validation
                            for (int i = 0; i < sm.LoadedScenarioCount; i++)
                            {
                                Scenario scenario = sm.GetByIndex(i);
                                using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                                {
                                    if (a_getScenariosRequest.GetBlackBoxScenario && !ss.IsBlackBoxScenario)
                                    {
                                        continue;
                                    }

                                    if (!string.IsNullOrEmpty(a_getScenariosRequest.ScenarioType))
                                    {
                                        // RR Integration has historically used this API to get the "Live" scenario - this was since replaced with the Production Flag
                                        if (a_getScenariosRequest.ScenarioType.Equals("Live", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (IsProductionScenario(ss))
                                            {
                                                confs.Add(new()
                                                {
                                                    ScenarioId = scenario.Id.Value,
                                                    ScenarioName = scenario.Name,
                                                    ScenarioType = "Live" // RR integration expects this value to match what it requested
                                                }); 
                                            }
                                        }
                                        else if (a_getScenariosRequest.ScenarioType == scenario.Type.ToString())
                                        {
                                            confs.Add(new() { ScenarioId = scenario.Id.Value, ScenarioName = scenario.Name, ScenarioType = scenario.Type.ToString() });
                                        }
                                    }
                                    else
                                    {
                                        confs.Add(new() { ScenarioId = scenario.Id.Value, ScenarioName = scenario.Name, ScenarioType = scenario.Type.ToString() });
                                    }
                                }
                            }
                        }
                    }
                    catch (AutoTryEnterException e)
                    {
                        //Try again
                        Thread.Sleep(100);
                    }

                    if (confs.Count > 0)
                    {

                        m_getScenariosResp = new GetScenariosResponse { Confirmations = confs };
                        m_getScenariosResp.ResponseCode = EApsWebServicesResponseCodes.SuccessWithoutValidation;
                    }
                }
            }

            return m_getScenariosResp;
        }

        private static bool IsProductionScenario(ScenarioSummary ss)
        {
            ScenarioPlanningSettings scenarioPlanningSettings = ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
            return scenarioPlanningSettings.Production;

        }
    }

    /// <summary>
    /// Process a DeleteScenarioRequest to initiate a scenario delete and return DeleteScenarioResponse confirmation at deletion ScenarioDeleteEvent trigger
    /// </summary>
    internal class DeleteScenarioProcessor : ProcessorBase
    {
        private DeleteScenarioResponse m_deleteScenarioResp;
        private EApsWebServicesResponseCodes m_deleteResponseCode = EApsWebServicesResponseCodes.SuccessWithoutValidation;
        private BaseId m_deletedScenarioId;

        public DeleteScenarioProcessor(BaseId a_instigator) : base(a_instigator) { }

        internal DeleteScenarioResponse ProcessRequest(DeleteScenarioRequest a_deleteScenarioRequest)
        {
            DateTime start = DateTime.Now;
            PTTransmission t = GetScenarioDeleteT(a_deleteScenarioRequest, start);
            if (t != null)
            {
                SystemController.ClientSession.SendClientAction(t, true);
                Task awaitTTask = Task.Run(() => SystemController.ClientSession.WaitForTransmissionReceive(t.TransmissionId));
                awaitTTask.Wait(TimeSpan.FromMinutes(a_deleteScenarioRequest.TimeoutMinutes));

                using (SystemController.Sys.ScenariosLock.EnterWrite(out ScenarioManager sm))
                {
                    Scenario scenario = sm.Find(m_deletedScenarioId);
                    if (scenario == null)
                    {
                        m_deleteScenarioResp = new DeleteScenarioResponse { ResponseCode = EApsWebServicesResponseCodes.Success };
                    }
                    else
                    {
                        m_deleteScenarioResp = new DeleteScenarioResponse
                        {
                            ResponseCode = EApsWebServicesResponseCodes.Failure,
                            Exception = true
                        };
                    }
                }
            }

            return m_deleteScenarioResp;
        }

        private ScenarioDeleteT GetScenarioDeleteT(DeleteScenarioRequest a_request, DateTime a_date)
        {
            BaseId liveScenarioId = BaseId.NULL_ID;
            ScenarioDeleteT deleteT = null;
            BaseId targetScenarioId = BaseId.NULL_ID;

            while (a_request.TimeoutMinutes == 0 || DateTime.Now.Subtract(a_date).TotalMinutes <= a_request.TimeoutMinutes)
            {
                try
                {
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
                    {
                        liveScenarioId = sm.GetFirstProductionScenario().Id;
                        break;
                    }
                }
                catch (AutoTryEnterException)
                {
                    Thread.Sleep(100);
                }
            }

            if (string.IsNullOrEmpty(a_request.ScenarioName) && a_request.ScenarioId == BaseId.NULL_ID.Value)
            {
                m_deleteResponseCode = EApsWebServicesResponseCodes.InvalidScenarioIdAndName;
                throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidScenarioIdAndName);
            }

            //Validate Scenario
            if (!string.IsNullOrEmpty(a_request.ScenarioName))
            {
                targetScenarioId = GetScenarioByName(a_request.ScenarioName);
            }

            if (targetScenarioId == BaseId.NULL_ID.Value && a_request.ScenarioId != BaseId.NULL_ID.Value)
            {
                targetScenarioId = GetScenarioById(a_request.ScenarioId);
            }

            if (targetScenarioId == BaseId.NULL_ID.Value)
            {
                m_deleteResponseCode = EApsWebServicesResponseCodes.InvalidScenarioIdAndName;
                throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidScenarioIdAndName);
            }

            if (targetScenarioId == liveScenarioId)
            {
                m_deleteResponseCode = EApsWebServicesResponseCodes.InvalidLiveScenario;
                throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidLiveScenario);
            }

            m_deletedScenarioId = targetScenarioId;
            m_deleteResponseCode = EApsWebServicesResponseCodes.Success;
            deleteT = new ScenarioDeleteT(targetScenarioId);
            deleteT.Instigator = m_instigator;
            deleteT.SetTimeStamp(DateTime.Now);

            return deleteT;
        }

        private static BaseId GetScenarioByName(string a_scenarioName)
        {
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 5000))
            {
                //Target Scenario input validation
                for (int i = 0; i < sm.LoadedScenarioCount; i++)
                {
                    Scenario scenario = sm.GetByIndex(i);
                    using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        //Verify by name
                        if (ss.Name == a_scenarioName)
                        {
                            return ss.Id;
                        }
                    }
                }
            }

            return BaseId.NULL_ID;
        }

        private static BaseId GetScenarioById(long a_scenarioId)
        {
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 5000))
            {
                //Target Scenario input validation
                for (int i = 0; i < sm.LoadedScenarioCount; i++)
                {
                    Scenario scenario = sm.GetByIndex(i);
                    using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        //Verify by Id
                        if (ss.Id == new BaseId(a_scenarioId))
                        {
                            return ss.Id;
                        }
                    }
                }
            }

            return BaseId.NULL_ID;
        }
    }

    internal class GetScenarioInformationProcessor
    {
        internal GetScenarioLastActionInfoResponse BuildLastActionInfo(BaseId a_scenarioId)
        {
            GetScenarioLastActionInfoResponse response = new GetScenarioLastActionInfoResponse();
            try
            {
                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
                {
                    Scenario s = sm.Find(new BaseId(a_scenarioId));
                    if (s == null)
                    {
                        //If true return early to prevent possible race condition where the deletion was already proceed on the server but not yet on the client.
                        //Therefore, an attempt to retrieve the last processed action may result in a null reference exception since the scenario wouldn't be in the collection of scenarios.
                        return response;
                    }

                    using (s.UndoSetsLock.TryEnterRead(out Scenario.UndoSets undoSets, AutoExiter.THREAD_TRY_WAIT_MS))
                    {
                        (string lastActionDescription, DateTimeOffset lastActionTimestamp, Guid lastTransmissionId) = sm.BuildLastActionInfo(undoSets);

                        response.LastActionInfo = lastActionDescription;
                        response.LastActionTicks = lastActionTimestamp.Ticks;
                        response.HasLastActions = lastActionTimestamp.Ticks != PTDateTime.MinDateTimeTicks;
                        response.LastActionTransmissionGuid = lastTransmissionId;
                    }
                }
            }
            catch (AutoTryEnterException)
            {
                Thread.Sleep(500);
            }

            return response;
        }
    }
}