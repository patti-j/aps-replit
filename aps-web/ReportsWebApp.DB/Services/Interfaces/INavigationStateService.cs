using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface INavigationStateService
{
    event Action OnNavigationChanged;
    List<NavigationItem> GetNavigationItems(bool blockMenu, bool UserCompanyActive, User User, List<Category> categories, List<Role> roles);
    void SetNavigationPath(List<BreadcrumbItem> newBreadcrumbs);
    List<BreadcrumbItem> GetNavigationPath();
    
    public delegate bool NavigationPreemptionCallback(string url, bool forceLoad);
    
    /// <summary>
    /// Used for preempting the normal navigation of pages. Implemented originally to allow for navigation cancelling when unsaved changes exist in the report editor.
    /// </summary>
    /// <param name="setter">The object calling this method i.e. <c>this</c>. Alternatively <c>typeof(SomeType)</c> if called from a static method.
    /// This is used to determine whether ClearPreemption should do anything, as you wouldn't want to clear someone else's preemption!</param>
    /// <param name="preemptionCallback">The function to be called before navigation occurs, takes 2 parameters, first is the navigation url and 2nd is whether a force load is to occur.
    /// Returns a bool to indicate whether the navigation should be cancelled. e.g. true to cancel false to continue.</param>
    /// <remarks>
    /// Initially I considered implementing a sort of "preemption-chain" whereby you could have an ordered list of callbacks that could be called after a navigation occurs.
    /// However, this could be difficult to use in places where lifetime tracking of objects isn't super clear. For instance, a temporary object may be created which
    /// adds itself to the preemption chain, this object is then discarded shortly thereafter, but it never removes itself from the preemption chain.
    /// This would both leak the object and cause potential state bugs. The current implementation of only having one preemptor at a time doesn't fix this,
    /// but it does reduce the scope of the problem. In the future it may be useful to revisit this idea for use in cases where the lifetime of objects is known
    /// and well handled.
    /// </remarks>
    public void PreemptNavigation(object setter, NavigationPreemptionCallback preemptionCallback);

    public void ClearPreemption(object setter);
    
    public void ExecutePreemption(string url, bool forceLoad, Action navFunction);

    //because navigation is stateful the NavigationStateService needs to handle it unless you are doing a force reload
    public void NavigateTo(string url, bool forceLoad = false);
}