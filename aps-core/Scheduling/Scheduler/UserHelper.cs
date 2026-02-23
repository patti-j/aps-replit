using PT.APSCommon;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class UserHelper
{
    // TODO: We intend to decouple the UserManager from the client (see ADO #20941). Rather than sending a transmission here, we should directly Add the user.
    public enum EAddedUserType { None, AdminUser, AppUser }

    private EAddedUserType m_addedUserType = EAddedUserType.None;
    private bool? m_didAddUserSucceed;

    public bool CreateUser(EAddedUserType a_userType = EAddedUserType.AdminUser, string a_userName = "Admin", string a_password = "", string a_userPermissionGroup = "", string a_plantPermissionsGroup = "")
    {
        UserT t = new ();
        UserT.User user = new ()
        {
            Name = a_userName,
            ExternalId = a_userName,
            Active = true,
            UnlockUser = true,
            AppUser = true
        };

        if (a_userType == EAddedUserType.AdminUser)
        {
            if (!string.IsNullOrEmpty(a_userPermissionGroup))
            {
                user.UserPermissionGroup = a_userPermissionGroup;
            }

            if (!string.IsNullOrEmpty(a_plantPermissionsGroup))
            {
                user.PlantPermissionGroup = a_plantPermissionsGroup;
            }
        }

        t.AutoDeleteMode = false;
        t.Add(user);

        m_addedUserType = a_userType;

        UmeEventListen(true);

        DateTime start = DateTime.Now;
        SystemController.ClientSession.SendClientAction(t);

        while (m_didAddUserSucceed == null)
        {
            if (DateTime.Now.Subtract(start) <= TimeSpan.FromSeconds(30))
            {
                Thread.Sleep(250);
            }
            else
            {
                m_didAddUserSucceed = false;
            }
        }

        UmeEventListen(false);

        m_addedUserType = EAddedUserType.None;

        return m_didAddUserSucceed.Value;
    }

    private void UmeEventListen(bool a_listen)
    {
        using (SystemController.Sys.UserManagerEventsLock.TryEnterRead(out UserManagerEvents ume, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            if (a_listen)
            {
                ume.ScenarioDataChangesEvent += UmeOnScenarioDataChangesEvent;
            }
            else
            {
                ume.ScenarioDataChangesEvent -= UmeOnScenarioDataChangesEvent;
            }
        }
    }

    private void UmeOnScenarioDataChangesEvent(IScenarioDataChanges a_dataChanges)
    {
        if (m_addedUserType == EAddedUserType.None)
        {
            return;
        }

        if (a_dataChanges.UserChanges.HasChanges && a_dataChanges.UserChanges.TotalAddedObjects > 0)
        {
            using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
            {
                foreach (BaseId userId in a_dataChanges.UserChanges.Added)
                {
                    User user = um.GetById(userId);
                    switch (m_addedUserType)
                    {
                        case EAddedUserType.AdminUser:
                            if (user.Name == "Admin")
                            {
                                m_didAddUserSucceed = true;
                                return;
                            }

                            break;
                        case EAddedUserType.AppUser:
                            if (user.AppUser)
                            {
                                m_didAddUserSucceed = true;
                                return;
                            }

                            break;
                    }
                }
            }
        }
    }
}