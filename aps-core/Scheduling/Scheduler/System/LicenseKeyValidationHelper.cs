using System.Security.Cryptography;
using System.Text;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Threading;
using PT.Scheduler.Sessions;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.SystemServiceDefinitions;

namespace PT.Scheduler;

internal class LicenseKeyValidationHelper
{
    private readonly ISystemLogger m_errorReporter;

    private static readonly string s_maintenanceExpired = "This version of the software is not covered by your maintenance. This build was created on {0}, but your maintenance expired on {1}. Once your maintenance has been paid you will be given a new set of key files that will allow you to upgrade to this version. If you need to get up and running immediately you can do so by reinstalling your previous version.".Localize();
    private static readonly string s_licenseExpiredMsg = "Your license expired on {0}.".Localize();
    private static readonly string s_userValidationMsg = "Number of possible users exceeded".Localize();

    internal LicenseKeyValidationHelper(ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;
    }

    internal LicenseKeyObject.ELicenseStatus InvalidReason { get; set; } = LicenseKeyObject.ELicenseStatus.GoodStatus;

    /// <summary>
    /// returns true if key passes all checks and is valid
    /// </summary>
    /// <param name="a_key"></param>
    /// <param name="a_liveSystem"></param>
    /// <returns></returns>
    internal bool LicenseKeyCheck(LicenseKey a_key, PTSystem a_liveSystem)
    {
        try
        {
            ExpirationCheck(a_key);
            MaintenanceExpirationCheck(a_key);
            KeyDataLimitsCheck(a_key, a_liveSystem);
            return true;
        }
        catch (Exception err)
        {
            //If this is running with MR, this should be a fatal error so errors are reported by MR.
            if (PTSystem.RunningMassRecordings)
            {
                LogLicensingException(err, "Error with MR key file. Make sure the key is not a license service key");
                throw;
            }

            LogLicensingException(err, "Security check Exception.");

            return false;
        }
    }

    /// <summary>
    /// Check whether key has expired.
    /// </summary>
    private void ExpirationCheck(LicenseKey a_key)
    {
        if (a_key.Expirable || a_key.TrialVersion) //make all Trial Version expirable just in case the value was entered wrong in the xml file by accident
        {
            DateTime expirationWithGrace = a_key.TrueExpiration; // this is in UTC

            if (DateTime.UtcNow > expirationWithGrace)
            {
                InvalidReason = LicenseKeyObject.ELicenseStatus.Expired;
                throw new ExpiredException(string.Format(s_licenseExpiredMsg.Localize(), expirationWithGrace.ToDisplayTime()));
            }
        }
    }

    /// <summary>
    /// Check whether Maintenance has expired.
    /// </summary>
    private void MaintenanceExpirationCheck(LicenseKey a_key)
    {
        DateTime maintenanceEnd = a_key.TrueMaintenanceExpiration;
        DateTime assemblyBuildDateUTC = PTAssemblyVersionChecker.GetServerBuildDate();

        if (assemblyBuildDateUTC > maintenanceEnd)
        {
            InvalidReason = LicenseKeyObject.ELicenseStatus.MaintenanceExpired;
            string msg = string.Format(s_maintenanceExpired, assemblyBuildDateUTC.ToShortDateString(), maintenanceEnd.ToDisplayTime().ToShortDateString());

            throw new ExpiredException(msg);
        }
    }

    /// <summary>
    /// To prevent one key being used to create the scenario and another to run it, check
    /// whether the loaded scenario violates any limitations.
    /// </summary>
    /// <returns>true if more plants exist than allowed by LicenseKey</returns>
    private void KeyDataLimitsCheck(LicenseKey a_key, PTSystem a_liveSystem)
    {
        while (true)
        {
            try
            {
                using (ObjectAccess<ScenarioManager> sm = a_liveSystem.ScenariosLock.TryEnterRead(100))
                {
                    using (sm.Instance.GetByIndex(0).ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, 10))
                    {
                        if (sd.PlantManager.Count > a_key.MaxNbrPlants)
                        {
                            InvalidReason = LicenseKeyObject.ELicenseStatus.PlantsExceeded;
                            string msg = string.Format("There are {0} Plants in Live Scenario which exceeds the limit specified in LicenseKey. Please contact support.", sd.PlantManager.Count);
                            throw new AuthorizationException(msg);
                        }

                        if (sd.ScenarioOptions.PlanningHorizon > TimeSpan.FromDays(a_key.MaxPlanningHorizonDurationInDays))
                        {
                            InvalidReason = LicenseKeyObject.ELicenseStatus.PlanningHorizonSpanExceeded;
                            string msg = string.Format("The planning horizon spans {0} months in Live Scenario which exceeds the limit specified in LicenseKey. Please contact support.", sd.ScenarioOptions.PlanningHorizon.TotalDays / 31);
                            throw new AuthorizationException(msg);
                        }

                        return;
                    }
                }
            }
            catch (AutoTryEnterException)
            {
                // Keep trying
            }
        }
    }

    internal void ValidateUser(List<BaseSession> a_sessions)
    {
        //Count the number of connections in use for each User type
        int availUserCount = PTSystem.LicenseKey.MaxNbrUsers;

        for (int i = 0; i < a_sessions.Count; i++)
        {
            BaseSession session = a_sessions[i];
            if (session is UserSession)
            {
                availUserCount--;
            }
        }

        //Make sure the system has a license available for the User
        if (availUserCount < 1)
        {
            NoMoreUserLicensesException userException = new ("2458", new object[] { s_userValidationMsg });
            InvalidReason = LicenseKeyObject.ELicenseStatus.UsersExceeded;
            LogLicensingException(userException, "User Licensing Error occurred.".Localize());

            throw userException;
        }
    }

    /// <summary>
    /// Verifies that the key hasn't been modified.
    /// </summary>
    /// <param name="a_serialCode"></param>
    /// <param name="a_sysIdTypes"></param>
    /// <param name="a_trialVersion"></param>
    /// <param name="a_expirable"></param>
    internal void VerifyKeyAuthenticity(string a_serialCode, SystemIdTypes a_sysIdTypes, LicenseKeyJson a_key, bool a_isOffline)
    {
        // verify key file before applying values
        string serialNumber;

        serialNumber = a_isOffline ? a_key.SystemId : GetSerialNumber(a_serialCode, a_sysIdTypes, a_key.TrialVersion, a_key.Expirable);
        ValidateKey(serialNumber, KeyFilePaths.KeyPath);
    }

    private void ValidateKey(string a_serialNumber, string a_keyPath)
    {
        if (a_serialNumber.Length == 0)
        {
            throw new Exception("The computer couldn't be validated, contact APS support. Details: The id was blank.".Localize());
        }

        // Read the signed data.
        byte[] signedBytes = File.ReadAllBytes(a_keyPath);

        string publicKey = "";
        // Read the public xml key string.
        using (StreamReader sr = File.OpenText(KeyFilePaths.PublicKeyPath))
        {
            publicKey = sr.ReadLine();
        }

        // Create an rsa object from the public key.
        RSACryptoServiceProvider rsa = new ();
        rsa.FromXmlString(publicKey);

        #if DEBUG
        //Loading test license
        if (File.Exists(PTSystem.WorkingDirectory.KeyFile))
        {
            return;
        }
        #endif
        Common.Security.Checksum.FileChecksum(KeyFilePaths.ConfigPath, out decimal checksum, out int lineCount, out int charCount);

        string data = string.Format("{0}CS{1}LC{2}CC{3}", a_serialNumber, checksum.ToString(), lineCount, charCount);

        //Verify that the serial number matches the signed bytes.
        if (!rsa.VerifyData(Encoding.Unicode.GetBytes(data), new SHA256CryptoServiceProvider(), signedBytes))
        {
            InvalidReason = LicenseKeyObject.ELicenseStatus.InvalidKey;
            LicenseKeyException licenseException = new ("Invalid Key! Serial Code, SystemId or another value in key doesn't match expected value. Contact APS support.".Localize());
            LogLicensingException(licenseException, "Invalid Key Error occurred.".Localize());
            
            throw licenseException;
        }
    }

    /// <summary>
    /// returns the serial
    /// </summary>
    /// <returns></returns>
    private static string GetSerialNumber(string a_serialCode, SystemIdTypes a_systemIdType, bool a_trial, bool a_expirable)
    {
        string serialNumber = "";

        int getIdAttempts = 0;

        while (true)
        {
            try
            {
                if (a_trial && a_expirable)
                {
                    serialNumber = "93c04cb7a9a3e9ca2f2b";
                }
                else
                {
                    ++getIdAttempts;
                    switch (a_systemIdType)
                    {
                        case SystemIdTypes.CpuId:
                            serialNumber = MachineInfo.GetProcessorId();
                            break;

                        case SystemIdTypes.WindowsProductId:
                            throw new PTHandleableException("WindowsProductId is not supported");

                        case SystemIdTypes.LicenseService:
                            serialNumber = a_serialCode;
                            break;
                    }
                }

                // The CPU Id was obtained. Break out of this forever loop.
                break;
            }
            catch
            {
                // Obtaining the CPU Id at computer startup was failing on some prospect and partner computers at startup. 
                if (getIdAttempts == 12)
                {
                    // Give up trying to obtain the CPU Id.
                    throw new Exception("The computer couldn't be validated, contact APS support.".Localize());
                }

                // Wait 5 seconds before trying again.
                Thread.Sleep(1000 * 5);
            }
        }

        return serialNumber;
    }

    /// <summary>
    /// Logs FaultExceptions returned by ServerManager. Since these cause system to go into read-only mode, they'll be logged to Exception folder.
    /// </summary>
    /// <param name="a_exception">Exception to log</param>
    /// <param name="a_extraText">Name of the failed Operation</param>
    internal void LogLicensingException(Exception a_exception, string a_extraText)
    {
        try
        {
            Exception exceptionToLog = a_exception;
            if (!string.IsNullOrEmpty(a_extraText))
            {
                exceptionToLog = new PTException(a_extraText, a_exception);
            }
            m_errorReporter.LogException(exceptionToLog, null, false, ELogClassification.Key);
        }
        catch
        {
            // ignored
        }
    }

    [Serializable]
    internal class ExpiredException : PTException
    {
        internal ExpiredException(string a_msg) : base(a_msg) { }

        protected ExpiredException(System.Runtime.Serialization.SerializationInfo a_info, System.Runtime.Serialization.StreamingContext a_context) { }
    }
}