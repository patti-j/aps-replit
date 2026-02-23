namespace PT.SchedulerExtensions.User;

public static class UserExtensions
{
    public static string GetReadableName(this Scheduler.User a_user)
    {
        if (a_user == null)
        {
            return "Unknown user";
        }

        if (string.IsNullOrEmpty(a_user.FirstName) && string.IsNullOrEmpty(a_user.LastName))
        {
            return a_user.Name;
        }

        string firstName = a_user.FirstName ?? string.Empty;
        string lastName = a_user.LastName ?? string.Empty;

        return string.Format("{0} {1}", firstName, lastName).Trim();
    }

    public static string GetSimplifiedName(this Scheduler.User a_user)
    {
        if (a_user == null)
        {
            return "Unknown user";
        }

        if (!string.IsNullOrEmpty(a_user.FirstName))
        {
            return a_user.FirstName;
        }

        if (!string.IsNullOrEmpty(a_user.LastName))
        {
            return a_user.LastName;
        }

        return a_user.Name;
    }
}