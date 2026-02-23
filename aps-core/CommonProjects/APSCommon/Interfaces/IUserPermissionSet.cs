namespace PT.APSCommon.Interfaces;

public interface IUserPermissionSet
{
    /// <summary>
    /// Returns the permission keys in this group
    /// </summary>
    /// <param name="a_allowed">Whether to return the list of allowed permissions or the denied permissions</param>
    /// <returns></returns>
    public HashSet<string> GetPermissions(bool a_allowed);
}
