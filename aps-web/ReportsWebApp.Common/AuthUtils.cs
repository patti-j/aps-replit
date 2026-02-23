using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ReportsWebApp.Common
{
    public static class AuthUtils
    {
        #region Constants

        /// <summary>
        /// All permissions that are generally available to be set by users who manage permission groups.
        /// Does not include elevated and feature-flagged permissions or Server related permissions, which are stored separately in <see cref="PTAdminOnlyPermissions"/>
        /// </summary>
        public static List<Permission> GeneralPermissions =>
            Permission.StandardPermissions;

        /// <summary>
        /// All permissions that are generally available to be set by users who manage permission groups in an On Prem company.
        /// Does not include elevated and feature-flagged permissions, which are stored separately in <see cref="PTAdminOnlyPermissions"/>
        /// </summary>
        public static List<Permission> OnPremPermissions =>
            Permission.OnPremPermissions;
        

        /// <summary>
        /// Permissions that should only be applied to PlanetTogether domain users.
        /// </summary>
        // TODO: We have logic on user/permission CRUD to do special things with the PTAdmin permission. We may want to consider if we should do that for all permissions in this group.
        public static List<Permission> PTAdminOnlyPermissions => 
        [
            Permission.PTAdmin,
            Permission.DeveloperPermissions,
        ];

        public static List<Permission> AllPermissions =>
            GeneralPermissions.Concat(OnPremPermissions).Concat(PTAdminOnlyPermissions).ToList();
        #endregion

        public static string GetUsernameFromClaim(ClaimsPrincipal currentUser)
        {
            try
            {
                return currentUser.Claims
                    .Where(claim => claim.Type == ClaimTypes.Name)
                    .Select(claim => claim.Value)
                    .FirstOrDefault()
                    ?? currentUser.Claims
                        .Where(claim => claim.Type == "name")
                        .Select(claim => claim.Value)
                        .FirstOrDefault()
                    ?? currentUser.Claims
                        .First(claim => claim.Type == ClaimTypes.NameIdentifier)
                        .Value;
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("Error: An identifier could not be found for the current user.", ex);
            }
        }

        public static string GetEmailFromClaim(ClaimsPrincipal currentUser)
        {
            try
            {
                var token = currentUser.Claims
                              .Where(c => c.Type.Equals("jwtToken"))
                              .Select(c => c.Value)
                              .FirstOrDefault()
                          ?? string.Empty;

                if (token != string.Empty)
                {
                    // Initialize the JWT token handler
                    var handler = new JwtSecurityTokenHandler();

                    // Read and decode the token
                    var jwtToken = handler.ReadJwtToken(token);

                    // Access the claims in the token
                    var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");

                    return emailClaim.Value;
                }
                return string.Empty;
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("Error: An identifier could not be found for the current user.", ex);
            }
        }

        public static List<Permission> GetAvailablePermissions(bool isUserPtAdmin, bool isPtCompany, bool isOnPremCompany = false, List<string>? keysToInclude = null)
        {
            List<Permission> roles = GeneralPermissions;

            if (isOnPremCompany)
            {
                roles.AddRange(OnPremPermissions);
            }

            if (isPtCompany && isUserPtAdmin)
            {
                roles.AddRange(PTAdminOnlyPermissions);
            }

            // If a group is assigned permissions that can't normally be added, we should still show them
            if (keysToInclude != null)
            {
                foreach (string key in keysToInclude)
                {
                    if (!roles.Any(x => x.Key == key))
                    {
                        roles.Add(AllPermissions.First(x => x.Key == key));
                    }
                }
            }

            return roles;
        }
    }

    public class Permission
    {
        /// <summary>
        /// The user-friendly name of this permission
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The internal identifier used to reference this permission
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The Category in which to display this permission
        /// </summary>
        public EPermissionCategory Category { get; }
        public string Description { get; }
        /// <summary>
        /// If warning message is present, it will display a confirmation screen when attempting to apply the permission
        /// </summary>
        public string? WarningMessage { get; }

        public enum EPermissionCategory
        {
            [Description("Reports")]
            Reports,
            [Description("Integrations")]
            Integration,
            [Description("User Management")]
            UserManagement,
            [Description("Scheduling")]
            Scheduling,
            [Description("Server Management")]
            ServerManagement,
            [Description("Planning Areas")]
            PlanningAreaManagement,
            [Description("Special Permissions")]
            Special,

        }

        public Permission(string name, string key, string description, EPermissionCategory category, string? warningMessage = null)
        {
            Name = name;
            Key = key;
            Description = description;
            Category = category;
            WarningMessage = warningMessage;
        }

        public static Permission PTAdmin => new("PT Admin", "ptAdmin", 
            "User can perform actions at the highest level of Authority across all customers. (Restricted)", EPermissionCategory.Special,
            "You are about to authorize users in this Role to perform actions at the highest level of Authority across all customers.");
        public static Permission ViewReports => new("View Reports", "viewReports",
            "User can View reports.", EPermissionCategory.Reports);
        public static Permission EditReports => new("Edit Reports", "editReports",
            "User can Add and Update reports.", EPermissionCategory.Reports);
        public static Permission ManageReports => new("Manage Reports", "adminReports",
            "User can Add, Update, and Delete reports.", EPermissionCategory.Reports);
        public static Permission ViewUsers => new("View Users", "viewUsers",
            "User can view User Management information such as Roles, Teams, and Scopes.", EPermissionCategory.UserManagement);
        public static Permission EditUsers => new("Edit Users", "editUsers",
            "User can Add and Update User Management objects.", EPermissionCategory.UserManagement);
        public static Permission ManageUsers => new("Manage Users", "adminUsers",
            "User can Add, Update, and Remove User Management objects.", EPermissionCategory.UserManagement);
        public static Permission ViewServers => new("View Servers and PAs", "viewServers",
            "User can Edit and Delete Servers for the company.", EPermissionCategory.ServerManagement);
        public static Permission OwnedOnlyServers => new("Edit Owned Servers and PAs", "ownedServers",
            "User can Edit and Delete Servers and PAs that they own.", EPermissionCategory.ServerManagement);
        public static Permission AdministrateServers => new("Administrate company", "administrateServers",
            "User can Edit and Delete Servers and PAs for the company.", EPermissionCategory.ServerManagement);
        public static Permission DeveloperPermissions => new("Developer Permissions", "DeveloperPermissions",
            "User can perform special debug operations.", EPermissionCategory.Special,
            "You are about to authorize users in this Role to be able to perform Debug operations, which may allow them to view protected information.");
        public static Permission ViewIntegration => new("View Integrations", "viewIntegrations",
            "User can view all Integration objects.", EPermissionCategory.Integration);
        public static Permission EditIntegration => new("Edit Integrations", "editIntegrations",
            "User can edit and create new Integration objects.", EPermissionCategory.Integration);
        public static Permission ManageIntegration => new("Manage Integrations", "adminIntegrations",
            "User can edit, delete, and create new Integration objects.", EPermissionCategory.Integration);
        public static Permission ManageCTPRequests => new("Manage CTP Requests", "ManageCTPRequests",
            "User can Create and Manage CTP requests for the company.", EPermissionCategory.Scheduling);
        public static Permission EditCustomFields => new("Edit Custom Fields", "EditCustomFields",
            "User can Add, Edit, and Delete custom fields from planning areas in the company.", EPermissionCategory.PlanningAreaManagement);
        public static Permission AccessAIBuilder => new("Access AI Factory Builder", "accessAIBuilder",
            "User can view the AI Factory Builder.", EPermissionCategory.Integration);

        public static List<Permission> AllPermissions => new ()
        {
            PTAdmin, ViewUsers, EditUsers, ManageUsers, ViewReports, EditReports, ManageReports, ViewServers, OwnedOnlyServers, AdministrateServers, ViewIntegration,
            EditIntegration, ManageIntegration, DeveloperPermissions
        };
        
        public static List<Permission> CompanyAdminPermissions => new ()
        {
            ViewUsers, EditUsers, ManageUsers, ViewReports, EditReports, ManageReports, ViewServers, OwnedOnlyServers, AdministrateServers, ViewIntegration,
            EditIntegration, ManageIntegration
        };
        
        public static List<Permission> WebAdminPermissions => new ()
        {
            ViewUsers, EditUsers, ManageUsers, ViewReports, EditReports, ManageReports, ViewServers, OwnedOnlyServers, AdministrateServers, ViewIntegration,
            EditIntegration, ManageIntegration
        };
        
        public static List<Permission> WebServerAdminPermissions => new ()
        {
            ViewUsers, ViewReports, ViewServers, OwnedOnlyServers, AdministrateServers, ViewIntegration,
            EditIntegration, ManageIntegration
        };
        public static List<Permission> ReportAdminPermissions => new ()
        {
            ViewUsers, ViewReports, EditReports, ManageReports, ViewServers, ViewIntegration
        };
        public static List<Permission> DesktopGlobalAdminPermissions => new ()
        {
            ViewUsers, ViewReports, ViewServers, ViewIntegration
        };
        
        public static List<Permission> DesktopMasterSchedulerPermissions => new ()
        {
            ViewReports,
        };
        
        public static List<Permission> WebViewOnlyPermissions => new ()
        {
            ViewUsers, ViewReports, ViewServers, ViewIntegration
        };
        
        public static List<Permission> StandardPermissions => new List<Permission>
        {
            ViewReports, ManageReports, ManageIntegration, ManageUsers
        };

        public static List<Permission> OnPremPermissions => new List<Permission>
        {
           AdministrateServers
        };

        public static List<Permission> DefaultAdminPermissions => StandardPermissions;

        public static List<Permission> DefaultReportAdminPermissions => new List<Permission>
        {
            ViewReports, ManageReports
        };

        public static List<Permission> DefaultUserPermissions => new List<Permission>
        {
            ViewReports
        };
    }
}
