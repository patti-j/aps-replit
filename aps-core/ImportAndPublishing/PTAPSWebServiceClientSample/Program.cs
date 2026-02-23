using PTAPSWebServiceClientSample.APSWebServiceRef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PTAPSWebServiceClientSample
{
    //Current Contract values
    /*
    public enum EApsWebServicesResponseCodes
    {
        Success = 0,
        SuccessWihoutValidation = 1,
        InvalidDateFormat = 2,
        InvalidDatePast = 3,
        InvalidDateFuture = 4,
        InvalidUserCredentials = 5,
        InvlaidUserPermissions = 6,
        NoServiceListening = 7,
        NoInstanceWithThatName = 8,
        ValidationTimeout = 9,
        NoServerManager = 10,
        InvalidScenarioName = 11,
        FailedToBroadcast = 12,
        ProcessingTimeout = 13,
        Failure = 14,
    }

    [ServiceContract]
    public interface IExtraServicesWebService
    {
        [OperationContract]
        EApsWebServicesResponseCodes AdvanceClock(string a_user, string a_password, string a_dateTime);

        [OperationContract]
        EApsWebServicesResponseCodes AdvanceClockTicks(string a_user, string a_password, long a_dateTimeTicks);

        [OperationContract]
        EApsWebServicesResponseCodes PublishLive(string a_user, string a_password, int a_publishType);

        [OperationContract]
        EApsWebServicesResponseCodes PublishByName(string a_user, string a_password, int a_publishType, string a_scenarioName);

        [OperationContract]
        Task<ApsWebServiceResponseBase> Import(ImportRequest a_request);

        [OperationContract]
        ApsWebServiceResponseBase Optimize(OptimizeRequest a_request);

        [OperationContract]
        Task<ApsWebServiceResponseBase> Publish(PublishRequest a_request);
        
        [OperationContract]
        CtpResponse CTP(string a_userName, string a_password, CtpRequest a_ctpRequest);
    }
    */

    class Program
    {
        private static string m_extraServicesUrl;

        /// <summary>
        /// For individual testing of service calls - uncomment related section
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            SelfSignedCertOverride();
            m_extraServicesUrl = "https://localhost:8789/APSWebService";
            
            //Setup the binding. This is done in code so the config file is not needed
            BasicHttpsBinding binding = new BasicHttpsBinding();
            binding.CloseTimeout = new TimeSpan(0, 1, 0);
            binding.OpenTimeout = new TimeSpan(0, 1, 0);
            binding.SendTimeout = new TimeSpan(0, 1, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 2, 0);
            binding.AllowCookies = false;
            binding.BypassProxyOnLocal = false;
            //binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            binding.MaxBufferPoolSize = 524288;
            binding.MaxBufferSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.TextEncoding = Encoding.UTF8;
            binding.TransferMode = TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            //binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;



            //CTP
            //TestCTP(args, binding);

            //Import
            //TestImport(args, binding);

            //CopyScenario
            //TestCopyScenario(args, binding);

            //GetScenarios
            //TestAdvanceClock(args, binding);
            //TestGetScenarios(args, binding);

            //TestOptimize(args, binding);

            //DeleteScenario
            //TestDeleteScenario(args, binding);

            //Hold
            //TestHold(binding);
            
            //Anchor
            //TestAnchor(binding);
            
            //Lock
            //TestLock(binding);
            
            //Move
            //TestMove(binding);

            //Unschedule
            //TestUnschedule(binding);

            //ActivityStatusUpdate
            //TestActivityStatusUpdate(binding);

            //ActivityQuantitiesUpdate
            //TestActivityQuantitiesUpdate(binding);

            //ActivityStatusUpdate
            //TestActivityDatesUpdate(binding);

            //Undo actions
            //TestUndoActions(binding);
            
            //Undo actions by transmission nbr
            //TestUndoByTransmissionNbr(binding);
            
            //Undo last user action
            //TestUndoByUser(binding);

            //Send chat message
            TestChat(binding);
        }

        private static void SelfSignedCertOverride()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        #region Hold
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestHold(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            HoldRequest holdRequest = new HoldRequest();
            holdRequest.UserName = "Admin";
            holdRequest.Password = "";
            holdRequest.ScenarioId = 1;
            holdRequest.HoldDate = DateTime.Now.AddDays(10);
            holdRequest.Reason = "Test Reason";
            holdRequest.Hold = true;

            List<PtObjectId> objectIds = new List<PtObjectId>();
            objectIds.Add(new PtObjectId() {JobExternalId =  "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() {JobExternalId =  "job2", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() {JobExternalId =  "job3", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });

            holdRequest.PtObjects = objectIds.ToArray();

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase holdResponse = client.Hold(holdRequest);

            if (holdResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region Anchor
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestAnchor(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            AnchorRequest anchorRequest = new AnchorRequest();
            anchorRequest.UserName = "Admin";
            anchorRequest.Password = "";
            anchorRequest.ScenarioId = 1;
            anchorRequest.Anchor = true;
            anchorRequest.Lock = true;
            anchorRequest.AnchorDate = new DateTime(2020,  9,15, 8,0,0);

            List<PtObjectId> objectIds = new List<PtObjectId>();
            objectIds.Add(new PtObjectId() { JobExternalId = "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job2", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job3", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });

            anchorRequest.PtObjects = objectIds.ToArray();

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase anchorResponse = client.Anchor(anchorRequest);

            if (anchorResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region Lock
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestLock(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            LockRequest lockRequest = new LockRequest();
            lockRequest.UserName = "Admin";
            lockRequest.Password = "";
            lockRequest.ScenarioId = 1;
            lockRequest.Lock = false;

            List<PtObjectId> objectIds = new List<PtObjectId>();
            objectIds.Add(new PtObjectId() { JobExternalId = "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job2", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job3", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });

            lockRequest.PtObjects = objectIds.ToArray();

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase lockResponse = client.Lock(lockRequest);

            if (lockResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region Move
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestMove(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            MoveRequest moveRequest = new MoveRequest();
            moveRequest.UserName = "Admin";
            moveRequest.Password = "";
            moveRequest.ScenarioId = 1;
            moveRequest.ExactMove = true;
            moveRequest.ExpediteSuccessors = true;
            moveRequest.LockMove = true;
            moveRequest.AnchorMove = true;
            moveRequest.MoveDateTime = new DateTime(2020, 9, 15, 8, 0, 0);
            moveRequest.ResourceExternalId = "PT-2147483645";

            List<PtObjectId> objectIds = new List<PtObjectId>();
            objectIds.Add(new PtObjectId() { JobExternalId = "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job2", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job3", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });

            moveRequest.PtObjects = objectIds.ToArray();

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase moveResponse = client.Move(moveRequest);

            if (moveResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region Chat
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestChat(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            ChatRequest chatRequest = new ChatRequest();
            chatRequest.UserName = "admin";
            chatRequest.Password = "";
            chatRequest.ScenarioId = 1;
            chatRequest.RecipientName = "admin";
            chatRequest.ChatMessage = "Test Message";

                ApsWebServiceResponseBase holdResponse = client.Chat(chatRequest);

            if (holdResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region Undo
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestUndoByTransmissionNbr(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            UndoByTransmissionNbrRequest undoRequest = new UndoByTransmissionNbrRequest();
            undoRequest.UserName = "Admin";
            undoRequest.Password = "";
            undoRequest.ScenarioId = 1;
            undoRequest.TransmissionNbr = 0;

            ApsWebServiceResponseBase holdResponse = client.UndoByTransmissionNbr(undoRequest);

            if (holdResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }

        private static void TestUndoActions(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            UndoActionsRequest undoRequest = new UndoActionsRequest();
            undoRequest.UserName = "Admin";
            undoRequest.Password = "";
            undoRequest.ScenarioId = 1;
            undoRequest.NbrOfActionsToUndo = 1;

            ApsWebServiceResponseBase holdResponse = client.UndoActions(undoRequest);

            if (holdResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }

        private static void TestUndoByUser(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            UndoLastUserActionRequest undoRequest = new UndoLastUserActionRequest();
            undoRequest.UserName = "Admin";
            undoRequest.Password = "";
            undoRequest.ScenarioId = 1;
            undoRequest.InstigatorName = "admin";

            ApsWebServiceResponseBase holdResponse = client.UndoLastUserAction(undoRequest);

            if (holdResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region ActivityStatus
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestActivityStatusUpdate(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            ActivityStatusUpdateRequest activityUpdateRequest = new ActivityStatusUpdateRequest();
            activityUpdateRequest.UserName = "Admin";
            activityUpdateRequest.Password = "";
            activityUpdateRequest.ScenarioId = 1;

            ActivityStatusUpdateObject updateObject = new ActivityStatusUpdateObject();
            updateObject.PtObjectId = new PtObjectId() { JobExternalId = "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" };
            updateObject.ProductionStatus = "Running";
            updateObject.Comments = "meow";
            updateObject.Paused = true;

            activityUpdateRequest.ActivityStatusUpdates = new[] { updateObject };
            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase moveResponse = client.ActivityStatusUpdate(activityUpdateRequest);

            if (moveResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region ActivityQuantities
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestActivityQuantitiesUpdate(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            ActivityQuantitiesUpdateRequest activityUpdateRequest = new ActivityQuantitiesUpdateRequest();
            activityUpdateRequest.UserName = "Admin";
            activityUpdateRequest.Password = "";
            activityUpdateRequest.ScenarioId = 1;

            ActivityQuantitiesUpdateObject updateObject = new ActivityQuantitiesUpdateObject();
            updateObject.PtObjectId = new PtObjectId() { JobExternalId = "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" };
            updateObject.ReportedGoodQty = 25;
            updateObject.ReportedScrapQty = 10;

            activityUpdateRequest.ActivityQuantitiesUpdates = new[] { updateObject };
            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase moveResponse = client.ActivityQuantitiesUpdate(activityUpdateRequest);

            if (moveResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region ActivityDates
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestActivityDatesUpdate(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            ActivityDatesUpdateRequest activityUpdateRequest = new ActivityDatesUpdateRequest();
            activityUpdateRequest.UserName = "Admin";
            activityUpdateRequest.Password = "";
            activityUpdateRequest.ScenarioId = 1;

            ActivityDatesUpdateObject updateObject = new ActivityDatesUpdateObject();
            updateObject.PtObjectId = new PtObjectId() { JobExternalId = "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" };
            updateObject.ReportedRunHrs = 1;
            updateObject.ReportedSetupHrs = 3;
            updateObject.ReportedStartDate = DateTime.Now;

            activityUpdateRequest.ActivityDatesUpdates = new[] { updateObject };
            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase moveResponse = client.ActivityDatesUpdate(activityUpdateRequest);

            if (moveResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region Unschedule
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_binding"></param>
        private static void TestUnschedule(BasicHttpsBinding a_binding)
        {
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));
            UnscheduleRequest unscheduleRequest = new UnscheduleRequest();
            unscheduleRequest.UserName = "Admin";
            unscheduleRequest.Password = "";
            unscheduleRequest.ScenarioId = 1;

            List<PtObjectId> objectIds = new List<PtObjectId>();
            objectIds.Add(new PtObjectId() { JobExternalId = "job1", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job2", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            objectIds.Add(new PtObjectId() { JobExternalId = "job3", MoExternalId = "MO010", OperationExternalId = "Op010", ActivityExternalId = "PT00001" });
            unscheduleRequest.PtObjects = objectIds.ToArray();

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase unscheduleResponse = client.Unschedule(unscheduleRequest);

            if (unscheduleResponse.ResponseCode != EApsWebServicesResponseCodes.Success)
            {
                //TODO:
            }
        }
        #endregion

        #region CTP
        /// <summary>
        /// Test ExtraServices CTP call
        /// </summary>
        /// <param name="args"></param>
        /// <param name="a_binding"></param>
        private static void TestCTP(string[] args, BasicHttpsBinding a_binding)
        {
            if (args == null || args.Length != 5)
            {
                Console.WriteLine("Need 5 parameters separated by spaces: URL username password ItemExternalId WarehouseExternalId");
                Environment.Exit(1);
            }

            string url = args[0];
            string username = args[1];
            string pwd = args[2];
            string item = args[3];
            string warehouse = args[4];
            if (string.IsNullOrEmpty(item))
            {
                item = "Item 1";
            }

            if (string.IsNullOrEmpty(warehouse))
            {
                warehouse = "Warehouse 1";
            }

            APSWebServiceRef.ExtraServicesWebServiceClient client = new APSWebServiceRef.ExtraServicesWebServiceClient(a_binding, new EndpointAddress(url));

            //Create a typed request
            APSWebServiceRef.CtpRequest ctpRequest = new APSWebServiceRef.CtpRequest();

            //Example CTP request
            List<APSWebServiceRef.CtpRequestLine> ctpRequestLines = new List<APSWebServiceRef.CtpRequestLine>();
            ctpRequestLines.Add(new APSWebServiceRef.CtpRequestLine()
            {
                LineId = 1,
                ItemExternalId = item,
                WarehouseExternalId = warehouse,
                RequiredQty = 100,
                NeedDate = DateTime.Now.Add(TimeSpan.FromDays(7))
            });

            //Connection settings
            ctpRequest.UserName = username;
            ctpRequest.Password = pwd;
            ctpRequest.TimeoutMinutes = 3;
            ctpRequest.CtpRequestLines = ctpRequestLines.ToArray();
            ctpRequest.ReserveMaterialsAndCapacity = true;
            ctpRequest.SchedulingType = APSWebServiceRef.CtpDefsESchedulingType.Optimize;
            Write(ctpRequest);
            DateTime start = DateTime.Now;
            Console.WriteLine($"Request sent at '{start}'");

            //Sending a request requires the username and password of an APS user with appropriate permissions, generally an admin.
            var resp = client.CTP(ctpRequest);
            Console.WriteLine($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(resp);
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static void Write(APSWebServiceRef.CtpResponse a_resp)
        {
            Console.WriteLine($"    Code: '{a_resp.ReturnCode}'");
            Console.WriteLine($"    ErrorMessage: '{a_resp.ErrorMessage}'");
            if (a_resp.CtpResponseLines == null)
            {
                Console.WriteLine($"    CtpResponseLines: null");
            }
            else
            {
                Console.WriteLine($"    CtpResponseLines.Length: {a_resp.CtpResponseLines.Length}");
                foreach (var respLine in a_resp.CtpResponseLines)
                {
                    Console.WriteLine($"        LineId: '{respLine.LineId}'");
                    Console.WriteLine($"        ScheduledStartDate: '{respLine.ScheduledStartDate}'");
                    Console.WriteLine($"        ScheduledStartDate: '{respLine.ScheduledEndDate}'");
                }
            }
        }

        private static void Write(APSWebServiceRef.CtpRequest a_req)
        {
            Console.WriteLine($"    Priority: '{a_req.Priority}'");
            Console.WriteLine($"    ReserveMaterialsAndCapacity: '{a_req.ReserveMaterialsAndCapacity}'");
            Console.WriteLine($"    SchedulingType: '{a_req.SchedulingType}'");
            Console.WriteLine($"    TimeoutMinutes: '{a_req.TimeoutMinutes}'");
            Console.WriteLine($"    CtpRequestLines.Length: '{a_req.CtpRequestLines.Length}'");
            foreach (var reqLine in a_req.CtpRequestLines)
            {
                Console.WriteLine($"        LineId: '{reqLine.LineId}'");
                Console.WriteLine($"        WarehouseExternalId: '{reqLine.WarehouseExternalId}'");
                Console.WriteLine($"        ItemExternalId: '{reqLine.ItemExternalId}'");
                Console.WriteLine($"        NeedDate: '{reqLine.NeedDate}'");
                Console.WriteLine($"        RequiredQty: '{reqLine.RequiredQty}'");
            }
        }
        #endregion CTP

        #region Import
        /// <summary>
        /// Test ExtraServices Import call
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestImport(string[] a_args, BasicHttpsBinding a_binding)
        {
            DateTime start = new DateTime();

            //Valiate args
            if (a_args == null || a_args.Length < 4)
            {
                Console.WriteLine("Need 4 required parameters separated by spaces: ExtraServicesURL username password IntefaceServiceURL, 3 optional parameters: ScenarioName ScenarioId CreateScenarioIfNew");
                Environment.Exit(1);
            }

            string extraServicesUrl = a_args[0];
            string username = a_args[1];
            string pwd = a_args[2];
            string interfaceUrl = a_args[3];
            string scenarioName = a_args[4];
            long scenarioId;
            if (!Int64.TryParse(a_args[5], out scenarioId) || scenarioId <= 0)
            {
                scenarioId = long.MinValue;
            }

            bool createScenarioIfNew;
            if (!Boolean.TryParse(a_args[6], out createScenarioIfNew))
            {
                createScenarioIfNew = false;
            }

            //Create client
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(extraServicesUrl));

            //Validate Scenario info provided
            List<ScenarioConfirmation> matchingScenarios = new List<ScenarioConfirmation>();
            

            //Create a Import request
            APSWebServiceRef.ImportRequest importRequest = new ImportRequest() { 
                ScenarioName = scenarioName,
                ScenarioId = scenarioId,
                CreateScenarioIfNew = createScenarioIfNew
            };
            importRequest.UserName = username;
            importRequest.Password = pwd;

            //Sending a request
            start = DateTime.Now;
            //ApsWebServiceResponseBase resp = client.Import(importRequest);
            //Console.WriteLine($"Response received after '{DateTime.Now.Subtract(start)}'");
            Task<ApsWebServiceResponseBase> importTask = client.ImportAsync(importRequest);
            ApsWebServiceResponseBase importResponse = importTask.Result;
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(importResponse);
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static void Trace(string a_message)
        {
            Console.WriteLine(a_message);

            File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "log.txt"), a_message + Environment.NewLine);
        }

        private static void Write(ApsWebServiceResponseBase a_response)
        {
            Trace($"Operation code: {a_response.ResponseCode}");
            if (!a_response.Exception)
            {
                Trace($"Completed with Zero Errors");
            }
            else
            {
                Trace($"Completed with Errors");
                Trace(a_response.FullExceptionText);
            }
        }
        #endregion Import

        #region AdvanceClock
        /// <summary>
        /// Test ExtraServices Import call
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestAdvanceClock(string[] a_args, BasicHttpsBinding a_binding)
        {
            //Valiate args
            if (a_args == null || a_args.Length < 2)
            {
                Console.WriteLine("Need 2 required parameters separated by spaces: ExtraServicesURL, username; 3 optional parameters: Password, ScenarioId, AdvanceTime (DateTime)");
                Environment.Exit(1);
            }

            string v = a_args[0];
            string username = a_args[1];
            
            string pwd = String.Empty;
            long scenarioId = Int64.MinValue;
            DateTime advanceTime = DateTime.MinValue;

            //optional parameters in a try/catch to allow the program to continue
            try
            {
                pwd = a_args[2];
                scenarioId = Convert.ToInt64(a_args[3]);
                advanceTime = Convert.ToDateTime(a_args[4]);
            }
            catch
            {
                //Didn't pass optional arguments. Just continue.
            }

            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(m_extraServicesUrl));

            //Create a Import request
            AdvanceClockRequest advanceClockRequest = new AdvanceClockRequest()
            {
                ScenarioId = scenarioId,
                //If date time not passed as a parameter, clocked advance to now
                DateTime = advanceTime != DateTime.MinValue ? advanceTime : DateTime.Now
            };
            advanceClockRequest.UserName = username;
            advanceClockRequest.Password = pwd;

            //Sending a request
            DateTime start = DateTime.Now;
            //ApsWebServiceResponseBase resp = client.Import(importRequest);
            //Console.WriteLine($"Response received after '{DateTime.Now.Subtract(start)}'");
            ApsWebServiceResponseBase importResponse =  client.AdvanceClock(advanceClockRequest);
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(importResponse);
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Test ExtraServices Import call
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestAdvanceClockStringDate(string[] a_args, BasicHttpsBinding a_binding)
        {
            //Valiate args
            if (a_args == null || a_args.Length < 2)
            {
                Console.WriteLine("Need 2 required parameters separated by spaces: ExtraServicesURL, username; 3 optional parameters: Password, ScenarioId, AdvanceTime (string)");
                Environment.Exit(1);
            }

            string extraServicesUrl = a_args[0];
            string username = a_args[1];
            
            string pwd = String.Empty;
            long scenarioId = Int64.MinValue;
            string advanceTime = String.Empty;

            //optional parameters in a try/catch to allow the program to continue
            try
            {
                pwd = a_args[2];
                scenarioId = Convert.ToInt64(a_args[3]);
                advanceTime = a_args[4];
            }
            catch
            {
                //Didn't pass optional arguments. Just continue.
            }

            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(extraServicesUrl));

            //Create a Import request
            AdvanceClockStringDateRequest advanceClockRequest = new AdvanceClockStringDateRequest()
            {
                ScenarioId = scenarioId,
                //If date time not passed as a parameter, clocked advance to now
                DateTime = advanceTime != String.Empty ? advanceTime : DateTime.Now.ToString("yyyy-MM-ddTHH:mm")
            };
            advanceClockRequest.UserName = username;
            advanceClockRequest.Password = pwd;

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase importResponse = client.AdvanceClockStringDate(advanceClockRequest);
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(importResponse);
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Test ExtraServices Import call
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestAdvanceClockTicks(string[] a_args, BasicHttpsBinding a_binding)
        {
            //Valiate args
            if (a_args == null || a_args.Length < 2)
            {
                Console.WriteLine("Need 2 required parameters separated by spaces: ExtraServicesURL, username; 3 optional parameters: Password, ScenarioId, AdvanceTime (long)");
                Environment.Exit(1);
            }

            string extraServicesUrl = a_args[0];
            string username = a_args[1];
            string pwd = String.Empty;

            long scenarioId = Int64.MinValue;
            long advanceTime = Int64.MinValue;

            //optional parameters in a try/catch to allow the program to continue
            try
            {
                pwd = a_args[2];
                scenarioId = Convert.ToInt64(a_args[3]);
                advanceTime = Convert.ToInt64(a_args[4]);
            }
            catch
            {
                //Didn't pass optional arguments. Just continue.
            }

            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(extraServicesUrl));

            //Create a Import request
            AdvanceClockTicksRequest advanceClockRequest = new AdvanceClockTicksRequest()
            {
                ScenarioId = scenarioId,
                //If date time not passed as a parameter, clocked advance to now
                DateTimeTicks = advanceTime != Int64.MinValue ? advanceTime : DateTime.Now.Ticks
            };
            advanceClockRequest.UserName = username;
            advanceClockRequest.Password = pwd;

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase importResponse = client.AdvanceClockTicks(advanceClockRequest);
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(importResponse);
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
        #endregion AdvanceClock

        #region Optimize
        /// <summary>
        /// Test ExtraServices Optimize call
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestOptimize(string[] a_args, BasicHttpsBinding a_binding)
        {
            //Valiate args
            if (a_args == null || a_args.Length < 2)
            {
                Console.WriteLine("Need 2 required parameters separated by spaces: ExtraServicesURL, username; 3 optional parameters: Password, ScenarioId, MRP (bool)");
                Environment.Exit(1);
            }

            string extraServicesUrl = a_args[0];
            string username = a_args[1];
            string pwd = String.Empty;

            long scenarioId = Int64.MinValue;
            bool mrp = false;

            //optional parameters in a try/catch to allow the program to continue
            try
            {
                pwd = a_args[2];
                scenarioId = Convert.ToInt64(a_args[3]);
                mrp = Convert.ToBoolean(a_args[4]);
            }
            catch
            {
                //Didn't pass optional arguments. Just continue.
            }

            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(extraServicesUrl));

            //Create a Import request
            OptimizeRequest optimizeRequest = new OptimizeRequest()
            {
                ScenarioId = scenarioId,
                //If date time not passed as a parameter, clocked advance to now
                MRP = mrp,
            };
            optimizeRequest.UserName = username;
            optimizeRequest.Password = pwd;

            //Sending a request
            DateTime start = DateTime.Now;
            ApsWebServiceResponseBase importResponse = client.Optimize(optimizeRequest);
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(importResponse);
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
        #endregion

        #region CopyScenario
        /// <summary>
        /// Test ExtraServices Import call
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestCopyScenario(string[] a_args, BasicHttpsBinding a_binding)
        {
            DateTime start = new DateTime();

            //Valiate args
            if (a_args == null || a_args.Length < 4)
            {
                Console.WriteLine("Need 3 required parameters separated by spaces: ExtraServicesURL username password, 3 optional parameters: ScenarioId ScenarioName CreateScenarioIfNew");
                Environment.Exit(1);
            }

            string extraServicesUrl = a_args[0];
            string username = a_args[1];
            string pwd = a_args[2];
            long scenarioId;
            if (!Int64.TryParse(a_args[3], out scenarioId) || scenarioId <= 0)
            {
                scenarioId = long.MinValue;
            }
            string scenarioName = a_args[4];

            bool createScenarioIfNew;
            if (!Boolean.TryParse(a_args[5], out createScenarioIfNew))
            {
                createScenarioIfNew = false;
            }

            APSWebServiceRef.ExtraServicesWebServiceClient client = new APSWebServiceRef.ExtraServicesWebServiceClient(a_binding, new EndpointAddress(extraServicesUrl));
            
            //Create and Send CopyScenarioRequest
            CopyScenarioResponse resp = CreateAndSendCopyScenarioRequest(scenarioId, scenarioName, createScenarioIfNew, username, pwd, client, ref start);

            //Display response stats
            Console.WriteLine($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(resp);

            //Display response data
            DisplayCopyScenarioResponse(resp);

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static void DisplayCopyScenarioResponse(CopyScenarioResponse resp)
        {
            if (resp.ResponseCode == EApsWebServicesResponseCodes.Success)
            {
                Console.WriteLine($"Success: Scenario '{resp.Confirmation.ScenarioName}' created under Id:{resp.Confirmation.ScenarioId} with Type:{resp.Confirmation.ScenarioType}");
                if (resp.Exception)
                {
                    Console.WriteLine($"Errors: Invalid scenario data entered - {resp.ErrorMessage}");
                }
            }
            else
            {
                Console.Write("Scenario Copy not completed.");
            }
        }

        private static CopyScenarioResponse CreateAndSendCopyScenarioRequest(long a_scenarioId, string a_scenarioName, bool a_createScenarioIfNew, string a_username, string a_pwd, ExtraServicesWebServiceClient a_client, ref DateTime a_start)
        {
            //Create a typed request
            CopyScenarioRequest copyRequest = new CopyScenarioRequest()
            {
                ScenarioId = a_scenarioId,
                ScenarioName = a_scenarioName,
                CreateScenarioIfNew = a_createScenarioIfNew,
                TimeoutMinutes = 3
            };

            copyRequest.UserName = a_username;
            copyRequest.Password = a_pwd;

            //Sending a request
            a_start = DateTime.Now;
            CopyScenarioResponse resp = a_client.CopyScenario(copyRequest);
            return resp;
        }
        #endregion CopyScenario

        #region GetScenarios
        /// <summary>
        /// Test ExtraServices GetScenarios call for scenarios of specified type (or default all types)
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestGetScenarios(string[] a_args, BasicHttpsBinding a_binding)
        {
            //Valiate args
            if (a_args == null || a_args.Length < 2)
            {
                Console.WriteLine("Need 2 required parameters separated by spaces: ExtraServicesURL username, 3 optional parameter: Password ScenarioType (ie. Whatif, Live, Published, RuleSeek, InsertJobs, Game, ShortTerm, Pruned), TimeoutMinutes");
                Environment.Exit(1);
            }

            string extraServicesUrl = a_args[0];
            string username = a_args[1];
            string pwd = String.Empty;

            string scenarioType = String.Empty;
            double timeout = 5;

            //optional parameters in a try/catch to allow the program to continue
            try
            {
                pwd = a_args[2];
                scenarioType = a_args[3];
                double.TryParse(a_args[4], out timeout);
            }
            catch
            {
                //Didn't pass optional arguments. Just continue.
            }
            
            ExtraServicesWebServiceClient client = new ExtraServicesWebServiceClient(a_binding, new EndpointAddress(extraServicesUrl));

            //Create and send GetScenariosRequest
            DateTime start = DateTime.Now;
            GetScenariosResponse resp = CreateAndSendGetScenariosRequest(scenarioType, timeout, username, pwd, client);

            //Display response stats
            Console.WriteLine($"Response received after '{DateTime.Now.Subtract(start)}'");
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(resp);

            Console.WriteLine($"{Environment.NewLine}Found {resp.Confirmations.Length} {(resp.Confirmations.Length > 1 ? "scenarios":"scenario")} of Type '{(string.IsNullOrEmpty(scenarioType) ? "All Types" : scenarioType)}'{Environment.NewLine}");

            //Display response data
            if (resp.Confirmations.Length > 0)
            {
                for (var i = 0; i < resp.Confirmations.Length; i++)
                {
                    ScenarioConfirmation scenarioInfo = resp.Confirmations[i];
                    Console.WriteLine($"    {i+1}. Name: {scenarioInfo.ScenarioName}, Id: {scenarioInfo.ScenarioId},  Type: {scenarioInfo.ScenarioType}{Environment.NewLine}");
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static GetScenariosResponse CreateAndSendGetScenariosRequest(string a_scenarioType, double a_timeout, string a_username, string a_pwd, ExtraServicesWebServiceClient a_client)
        {
            //Create a typed request
            APSWebServiceRef.GetScenariosRequest getScenariosRequest = new GetScenariosRequest()
            {
                ScenarioType = a_scenarioType,
                TimeoutMinutes = a_timeout
            };

            getScenariosRequest.UserName = a_username;
            getScenariosRequest.Password = a_pwd;

            //Sending request
            return a_client.GetScenarios(getScenariosRequest);
        }
        #endregion

        #region DeleteScenario
        /// <summary>
        /// Test ExtraServices Delete Scenario call
        /// </summary>
        /// <param name="a_args"></param>
        /// <param name="a_binding"></param>
        private static void TestDeleteScenario(string[] a_args, BasicHttpsBinding a_binding)
        {
            DateTime start = new DateTime();

            //Valiate args
            if (a_args == null || a_args.Length < 4)
            {
                Console.WriteLine("Need 4 total required parameters separated by spaces: ExtraServicesURL username password ScenarioId ScenarioName (Note:at least 1 Scenario identifier required, both can be submitted)");
                Environment.Exit(1);
            }

            string extraServicesUrl = a_args[0];
            string username = a_args[1];
            string pwd = a_args[2];
            long scenarioId;
            if (!Int64.TryParse(a_args[3], out scenarioId) || scenarioId <= 0)
            {
                scenarioId = long.MinValue;
            }
            string scenarioName = a_args[4];

            //Create client
            APSWebServiceRef.ExtraServicesWebServiceClient client = new APSWebServiceRef.ExtraServicesWebServiceClient(a_binding, new EndpointAddress(extraServicesUrl));
                        
            //Create and Send ScenarioDeleteRequest
            DeleteScenarioResponse resp = CreateAndSendScenarioDeleteRequest(scenarioId, scenarioName, username, pwd, client, ref start);

            //Display response stats
            Console.WriteLine($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(resp);

            //Display response data
            if (resp.ResponseCode == EApsWebServicesResponseCodes.Success)
            {
                Console.WriteLine($"Success: Scenario validated and deleted for '{resp.Confirmation.ScenarioName}' with Id '{resp.Confirmation.ScenarioId}'");

                if (resp.Exception)
                {
                    Console.WriteLine($"Input Errors: {resp.ErrorMessage}");
                }
            }
            else
            {
                Console.Write("Scenario Delete not completed - ");

                if (resp.ResponseCode == EApsWebServicesResponseCodes.InvalidLiveScenario)
                {
                    Console.WriteLine($"Attempting to delete Live Scenario - Id '{scenarioId}' received");
                }
                else if (resp.ResponseCode == EApsWebServicesResponseCodes.InvalidScenarioIdAndName)
                {
                    Console.WriteLine($"No valid scenario data entered");
                }
                else
                {
                    Console.WriteLine($"{resp.ErrorMessage}");
                }

                if (resp.Exception)
                {
                    Console.WriteLine($"Input Errors: {resp.ErrorMessage}");
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static DeleteScenarioResponse CreateAndSendScenarioDeleteRequest(long a_scenarioId, string a_scenarioName, string a_username, string a_pwd, ExtraServicesWebServiceClient a_client, ref DateTime a_start)
        {
            //Create a typed request
            DeleteScenarioRequest deleteScenarioRequest = new DeleteScenarioRequest()
            {
                ScenarioId = a_scenarioId,
                ScenarioName = a_scenarioName,
                TimeoutMinutes = 3
            };

            deleteScenarioRequest.UserName = a_username;
            deleteScenarioRequest.Password = a_pwd;

            //Sending a request
            a_start = DateTime.Now;
            DeleteScenarioResponse resp = a_client.DeleteScenario(deleteScenarioRequest);
            return resp;
        }
        #endregion DeleteScenario
    }
}
