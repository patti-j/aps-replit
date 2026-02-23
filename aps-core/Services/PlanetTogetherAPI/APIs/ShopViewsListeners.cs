//using System;
//using System.Runtime.Serialization;
//using System.ServiceModel;

//using PT.ServerManagerSharedLib.Definitions;

//using PlanetTogetherExtraServicesDefinitions.cs;

//using PT.APIDefinitions.RequestsAndResponses;
//using PT.APIDefinitions.RequestsAndResponses.ActivityStatus;
//using PT.ServerManagerSharedLib.DTOs.Responses;
//using PT.Common;
//using PT.Common.Compression;
//using PT.Common.File;
//using PT.Scheduler;
//using PT.Scheduler.ErrorReporting;
//using PT.SchedulerDefinitions;

//namespace PT.ExtraServices.SoapAPIs
//{
//    public class ShopViewsListeners : IShopViewsContract
//    {
//        #region ShopViews
//        internal readonly byte[] m_symmetricKey;

//        //TODO: This cannot reference system proxy, because this project is referenced by the system service
//        //public ShopViewsListeners(SystemServiceProxy.ClientBroadcaster a_clientBroadcaster)
//        //{
//        //    using (EncryptionHandshake handshake = new EncryptionHandshake())
//        //    {
//        //        string publicKey = handshake.GetEncryptionKey();
//        //        byte[] encryptedData = a_clientBroadcaster.Handshake(publicKey);
//        //        m_symmetricKey = handshake.Decrypt(encryptedData);
//        //    }
//        //}

//        #region Data Retrieval
//        private static Scenario GetShopViewsScenario(ScenarioManager a_sm)
//        {
//            if (a_sm.ShopViewSystemOptions.UsePublishedSceanario && a_sm.PublishedScenario != null)
//                return a_sm.PublishedScenario;
//            else
//                return a_sm.LiveScenario;
//        }

//        /// <summary>
//        /// Returns the Resources the user has access to and access rights for each Resource.
//        /// The data is serialized and compressed.
//        /// </summary>
//        /// <returns>ViewerResourceInfos or null if data could not be locked</returns>
//        public byte[] GetCompressedViewerResourceInfos(long a_userId, TimeSpan a_timeout)
//        {
//            try
//            {
//                ViewerResourceInfos resourceInfos = new ViewerResourceInfos();
//                //Get the list of Resources

//                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, Convert.ToInt32(a_timeout.TotalMilliseconds * .5d)))
//                {
//                    Scenario s = GetShopViewsScenario(sm);

//                    try
//                    {
//                        using (s.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, Convert.ToInt32(a_timeout.TotalMilliseconds)))
//                        {
//                            for (int plantI = 0; plantI < sd.PlantManager.Count; plantI++)
//                            {
//                                Plant p = sd.PlantManager.GetByIndex(plantI);
//                                for (int deptI = 0; deptI < p.Departments.Count; deptI++)
//                                {
//                                    Department d = p.Departments.GetByIndex(deptI);

//                                    ResourceManager rm = d.Resources;
//                                    for (int i = 0; i < rm.Count; ++i)
//                                    {
//                                        Resource r = rm.GetByIndex(i);
//                                        if (r.ShopViewUsers.Count > 0) //have users assigned, so the current user must be one of them
//                                        {
//                                            BaseId baseId = new BaseId(a_userId);
//                                            if (!r.ShopViewUsers.Contains(baseId))
//                                            {
//                                                //No access granted to this user.
//                                            }
//                                            else
//                                            {
//                                                ShopViewUsers.ShopViewUser svu = r.ShopViewUsers[baseId];
//                                                ViewerResourceInfo info = new ViewerResourceInfo(r);
//                                                info.CanReassign = svu.CanReassign;
//                                                info.CanResequence = svu.CanResequence;
//                                                info.CanUpdateStatus = svu.CanUpdateStatus;
//                                                resourceInfos.Add(info);
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                    catch (AutoTryEnterException e)
//                    {
//                        //TODO: logging
//                        //using (SystemController.Sys.ErrorReporterLock.TryEnterRead(out ErrorReporter errorReporter, AutoExiter.THREAD_TRY_WAIT_MS))
//                        //{
//                        //    ScenarioExceptionInfo sei = new ScenarioExceptionInfo();
//                        //    sei.Create(s);
//                        //    errorReporter.LogException(new Exception("Shopviews timeout retrieving schedule info"), sei);
//                        //}

//                        return null;
//                    }
//                }

//                //Serialize the data into a compressed memory stream
//                using (BinaryMemoryWriter writer = new BinaryMemoryWriter(m_symmetricKey, CompressionType.Fast))
//                {
//                    resourceInfos.Serialize(writer);
//                    return writer.GetBuffer();
//                }
//            }
//            catch (AutoTryEnterException e)
//            {
//                //TODO: logging
//                //using (SystemController.Sys.ErrorReporterLock.TryEnterRead(out ErrorReporter errorReporter, AutoExiter.THREAD_TRY_WAIT_MS))
//                //{
//                //    errorReporter.LogException(new Exception("Shopviews timeout retrieving scenario info"), null);
//                //}

//                return null;
//            }
//        }

//        public APIDefinitions.RequestsAndResponses.UserPreferences GetUserPreferences(long a_userId)
//        {
//            try
//            {
//                using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, Convert.ToInt32(TimeSpan.FromSeconds(20).TotalMilliseconds)))
//                {
//                    User user = um.GetById(new BaseId(a_userId));
//                    APIDefinitions.RequestsAndResponses.UserPreferences prefs = new APIDefinitions.RequestsAndResponses.UserPreferences();
//                    if (user != null)
//                    {
//                        prefs.CanEditShopViewPreferences = true;
//                        prefs.JumpToNextOpInShopViews = user.JumpToNextOpInShopViews;
//                        prefs.PromptForShopViewsSave = user.PromptForShopViewsSave;
//                    }

//                    return prefs;
//                }
//            }
//            catch (AutoTryEnterException)
//            {
//                return null;
//            }
//        }

//        public byte[] GetHoldSettings()
//        {
//            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
//            {
//                Scenario s = GetShopViewsScenario(sm);

//                using (s.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
//                {
//                    //Serialize the data into a compressed memory stream
//                    using (BinaryMemoryWriter writer = new BinaryMemoryWriter(m_symmetricKey, CompressionType.Fast))
//                    {
//                        sd.JobManager.JobHoldSettings.Serialize(writer);
//                        return writer.GetBuffer();
//                    }
//                }
//            }
//        }
//        #endregion

//        #region Login
//        public LoginResponse Login(out long a_userId, string username, string password, out bool jumpToNextOnFinish)
//        {
//            jumpToNextOnFinish = false;
//            using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
//            {
//                //bool usingMagicWord = password == PTBroadcaster.MAGIC_WORD;

//                //if (!usingMagicWord && this.LoggedInUserCount >= PTSystem.LicenseKey.MaxNbrShopViewUsers)
//                //{
//                //    userId = BaseId.NULL_ID;
//                //    return loginResult.NoMoreLicenses;
//                //}

//                User u = um.GetUserByName(username); //case insensitive
//                if (u == null)
//                {
//                    a_userId = BaseId.NULL_ID.Value;
//                    return new LoginResponse(ELoginResult.InvalidUserOrPassword);
//                }
//                else
//                {
//                    jumpToNextOnFinish = u.JumpToNextOpInShopViews;

//                    //Verify password if in use
//                    if (UsePasswords() && u.Password.Trim() != password.Trim())
//                    {
//                        a_userId = BaseId.NULL_ID.Value;
//                        return new LoginResponse(ELoginResult.NoRights);
//                    }

//                    if (true)
//                    {
//                        a_userId = u.Id.Value;
//                        this.LoggedInUserCount++;
//                        return new LoginResponse(ELoginResult.LoggedIn);
//                    }
//                    else
//                    {
//                        a_userId = BaseId.NULL_ID.Value;
//                        return new LoginResponse(ELoginResult.NoRights);
//                    }
//                }
//            }
//        }

//        public void Logout()
//        {
//            if (LoggedInUserCount > 0)
//                this.LoggedInUserCount--;
//        }

//        int loggedInUserCount;

//        public int LoggedInUserCount
//        {
//            get { return this.loggedInUserCount; }
//            set { this.loggedInUserCount = value; }
//        }

//        public bool UsePasswords()
//        {
//            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
//            {
//                return sm.ShopViewSystemOptions.UsePasswords;
//            }
//        }

//        #endregion

//        #region AutoLogout
//        public bool AutoLogout()
//        {
//            int attempts = 5;
//            while (attempts > 0)
//            {
//                try
//                {
//                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, AutoExiter.THREAD_TRY_WAIT_MS))
//                    {
//                        return sm.ShopViewSystemOptions.AutoLogout;
//                    }
//                }
//                catch (AutoTryEnterException)
//                {
//                }

//                attempts--;
//            }

//            throw new Exception("Unable to retrieve AutoLogout value");
//        }

//        public TimeSpan AutoLogoutSpan()
//        {
//            int attempts = 5;
//            while (attempts > 0)
//            {
//                try
//                {
//                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, AutoExiter.THREAD_TRY_WAIT_MS))
//                    {
//                        return sm.ShopViewSystemOptions.AutoLogoutSpan;
//                    }
//                }
//                catch (AutoTryEnterException)
//                {
//                }

//                attempts--;
//            }

//            throw new Exception("Unable to retrieve AutoLogoutSpan value");
//        }

//        public TimeSpan AutoRefreshSpan()
//        {

//            int attempts = 5;
//            while (attempts > 0)
//            {
//                try
//                {
//                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, AutoExiter.THREAD_TRY_WAIT_MS))
//                    {
//                        return sm.ShopViewSystemOptions.AutoRefreshSpan;
//                    }
//                }
//                catch (AutoTryEnterException)
//                {
//                }

//                attempts--;
//            }

//            throw new Exception("Unable to retrieve AutoRefreshSpan value");
//        }
//        #endregion
//        #endregion
//    }
//}

