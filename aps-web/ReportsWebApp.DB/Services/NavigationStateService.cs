using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.AspNetCore.Components;

namespace ReportsWebApp.DB.Services
{
    public class NavigationStateService(NavigationManager NavigationManager) : INavigationStateService
    {
        public event Action OnNavigationChanged;
        private List<BreadcrumbItem> breadcrumbs = new List<BreadcrumbItem>();

        public List<NavigationItem> GetNavigationItems(bool blockMenu, bool UserCompanyActive, User User, List<Category> categories, List<Role> roles)
        {
            var items = new List<NavigationItem>
            {
                CreateHomeMenuItem(User)
            };
            
            items.Add(CreateSchedulingMenu(User));

            if (!blockMenu && User.IsAuthorizedFor(Permission.ViewReports) && UserCompanyActive && categories.Any())
            {
                items.Add(CreateReportsMenu(categories));
            }

            if (!blockMenu && UserCompanyActive)
            {
                AddManagementAndUserMenus(items, User, roles);
            }

            items.Add(CreateChatbotMenuItem());

            if (User.Roles.Any(r => r.Name == "AI_Analytics" && r.CompanyId == User.CompanyId))
            {
                items.Add(CreateAIAnalyticsMenuItem());
            }

            return items;
        }

        // Method for Home Menu Item
        private NavigationItem CreateHomeMenuItem(User user)
        {
            return new NavigationItem
            {
                Text = "Home", 
                NavigateUrl = "./",
                IconCssClass = "fa fa-home"
            };
        }

        // Method for Scheduling and Planning Menu
        private NavigationItem CreateSchedulingMenu(User user)
        {
            var schedulingItem = new NavigationItem
            {
                Text = "Scheduling",
                NavigateUrl = "", // No direct navigation
                IconCssClass = "fa fa-tasks", // Icon for Scheduling
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Text = "Resource Gantt",
                        NavigateUrl = "scheduler",
                        IconCssClass = "fa fa-chart-bar"
                    },
                    new NavigationItem
                    {
                        Text = "My Gantt Views",
                        NavigateUrl = "savedGanttsViewer",
                        IconCssClass = "fa fa-save"
                    },
                    new NavigationItem
                    {
                        Text = "Shared Gantt Views",
                        NavigateUrl = "sharedGanttsViewer",
                        IconCssClass = "fa fa-share-alt"
                    }
                }
            };

            // Add Planning section under Scheduling
            
            schedulingItem.Children.Add(CreatePlanningMenu());
            
            return schedulingItem;
        }

        // Method for Planning Menu under Scheduling
        private NavigationItem CreatePlanningMenu()
        {
            return new NavigationItem
            {
                Text = "Planning",
                NavigateUrl = "", // No direct navigation
                IconCssClass = "fa fa-calendar-alt", // Icon for Planning
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Text = "CTP Estimate",
                        NavigateUrl = "ctpEstimateViewer",
                        IconCssClass = "fa-sharp fa-solid fa-calendar-clock"
                    }
                }
            };
        }

        // Method for Reports Menu
        private NavigationItem CreateReportsMenu(List<Category> categories)
        {
            var reportsItem = new NavigationItem
            {
                Text = "Reports",
                NavigateUrl = "",
                IconCssClass = "fa fa-file-lines",
                Children = new List<NavigationItem>()
            };

            foreach (var category in categories)
            {
                var categoryItem = new NavigationItem
                {
                    Text = category.Name,
                    NavigateUrl = "", // Categories might not directly navigate
                    IconCssClass = "fa fa-folder",
                    Children = category.Reports.Select(report => new NavigationItem
                    {
                        Text = report.Name,
                        NavigateUrl = $"reportsViewer/{report.PBIWorkspace.PBIWorkspaceId}/{report.PBIReportId}/{category.Id}/{report.Id}",
                        IconCssClass = "fa fa-bar-chart"
                    }).ToList()
                };

                reportsItem.Children.Add(categoryItem);
            }

            return reportsItem;
        }

        // Method for Chatbot Menu Item
        private NavigationItem CreateChatbotMenuItem()
        {
            return new NavigationItem
            {
                Text = "KB AI Chatbot",
                NavigateUrl = "kbchat",
                IconCssClass = "fa fa-book"
            };
        }

        private NavigationItem CreateAIAnalyticsMenuItem()
        {
            return new NavigationItem
            {
                Text = "AI Analytics",
                NavigateUrl = "ai_analytics",
                IconCssClass = "fa fa-chart-line"
            };
        }

        // Method for Adding Management and User Menus
        private void AddManagementAndUserMenus(List<NavigationItem> items, User user, List<Role> roles)
        {
            if (user.IsAuthorizedFor(Permission.ManageReports))
            {
                items.Add(CreateReportManagementMenu());
            }

            if (user.IsAuthorizedFor(Permission.ManageIntegration))
            {
                items.Add(CreateIntegrationMenu(user));
            }

            if (user.IsAuthorizedFor(Permission.ViewServers))
            {
                items.Add(CreateServerManagerMenu(user));
            }

            
            items.Add(CreatePlanningAreaMenu(user));
            

            if (user.IsAuthorizedFor(Permission.EditUsers))
            {
                items.Add(CreateUserManagementMenu(user));
            }

            if (user.IsPTAdmin())
            {
                items.Add(CreateManagementMenu());
            }
        }

        // Method for Report Management Menu
        private NavigationItem CreateReportManagementMenu()
        {
            return new NavigationItem
            {
                Text = "Report Management",
                NavigateUrl = "",
                IconCssClass = "fa fa-cogs",
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Text = "Report Categories",
                        NavigateUrl = "categoriesViewer",
                        IconCssClass = "fa fa-tags"
                    },
                    new NavigationItem
                    {
                        Text = "Manage Reports",
                        NavigateUrl = "addReportsViewer",
                        IconCssClass = "fa fa-plus-square"
                    },
                    new NavigationItem
                    {
                        Text = "Create Reports",
                        NavigateUrl = "createReportViewer",
                        IconCssClass = "fa fa-pen-to-square"
                    }
                }
            };
        }

        // Method for Integration Menu
        private NavigationItem CreateIntegrationMenu(User user)
        {
            var integrationItem = new NavigationItem
            {
                Text = "Integration",
                NavigateUrl = "",
                IconCssClass = "fa fa-gear",
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Text = "Excel Import",
                        NavigateUrl = "uploadExcelViewer",
                        IconCssClass = "fa fa-upload"
                    },
                    new NavigationItem
                    {
                        Text = "Import History",
                        NavigateUrl = "importHistoryViewer",
                        IconCssClass = "fa fa-clock-rotate-left"
                    },
                    new NavigationItem
                    {
                        Text = "Trigger Import",
                        NavigateUrl = "triggerImportViewer",
                        IconCssClass = "fa fa-arrow-right-from-bracket"
                    },
                }
            };

            if (user.IsAuthorizedFor(Permission.AdministrateServers))
            {
                integrationItem.Children.Add(new NavigationItem
                {
                    Text = "Data Connectors",
                    NavigateUrl = "dataConnectors",
                    IconCssClass = "fa fa-file-import"
                });
            }

            if (user.IsAuthorizedFor(Permission.AdministrateServers))
            {
                integrationItem.Children.Add(new NavigationItem
                {
                    Text = "Manage Integrations",
                    NavigateUrl = "externalIntegrations",
                    IconCssClass = "fa fa-database"
                });
            }

            if (user.IsAuthorizedFor(Permission.AdministrateServers))
            {
                integrationItem.Children.Add(new NavigationItem
                {
                    Text = "Assignments",
                    NavigateUrl = "integrationsAssignments",
                    IconCssClass = "fa fa-share-nodes"
                });
            }            
            
            integrationItem.Children.Add(new NavigationItem
            {
                Text = "AI Factory Builder",
                NavigateUrl = "aibuilder",
                IconCssClass = "fa fa-microchip"
            });
            

            return integrationItem;
        }

        // Method for Planning Area Menu
        private NavigationItem CreatePlanningAreaMenu(User user)
        {
            var planningAreaItem = new NavigationItem
            {
                Text = "Planning Areas",
                NavigateUrl = "",
                IconCssClass = "fa fa-computer",
                Children = new()
            };
            
            
            planningAreaItem.Children.Add(new NavigationItem
            {
                Text = "CTP Estimate",
                NavigateUrl = "ctpEstimateViewer",
                IconCssClass = "fa-sharp fa-solid fa-calendar-clock"
            });
            

            // Deprecated in favor of Scopes/Roles
            // TODO: Remove this once access can be configured through roles
            if (user.IsAuthorizedFor(Permission.ManageUsers))
            {
                planningAreaItem.Children.Add(new NavigationItem
                {
                    Text = "Logins",
                    NavigateUrl = "paAccessViewer",
                    IconCssClass = "fa fa-user"
                });
            }

            if (user.IsAuthorizedFor(Permission.ManageIntegration))
                planningAreaItem.Children.Add(new NavigationItem
            {
                Text = "Integration Configurations",
                NavigateUrl = "servermanager/integrationconfigs",
                IconCssClass = "fa fa-cog"
            });

            if (user.IsAuthorizedFor(Permission.EditUsers))
            {
                planningAreaItem.Children.Add(new NavigationItem
                {
                    Text = "Scopes",
                    NavigateUrl = "scopesViewer",
                    IconCssClass = "fa fa-layer-group"
                });
                
                //This has also been deprecated in favor of Scopes/Roles
                //TODO: Remove when Roles/Teams/Scopes can configure Permissions/Logins
                planningAreaItem.Children.Add(new NavigationItem
                {
                    Text = "Permissions",
                    NavigateUrl = "permissionGroupViewer",
                    IconCssClass = "fa fa-users"
                });
            }

            if (user.IsAuthorizedFor(Permission.EditCustomFields))
            {
                planningAreaItem.Children.Add(new NavigationItem
                {
                    Text = "Custom Fields",
                    NavigateUrl = "customFields",
                    IconCssClass = "fa fa-database"
                });
            }

            
            planningAreaItem.Children.Add(new NavigationItem
            {
                Text = "Log In",
                NavigateUrl = "paviewer",
                IconCssClass = "fa fa-arrow-right"
            });
            

            return planningAreaItem;
        }

        // Method for User Management Menu
        private NavigationItem CreateUserManagementMenu(User user)
        {
            var userManagementItem = new NavigationItem
            {
                Text = "User Management",
                NavigateUrl = "",
                IconCssClass = "fa fa-user-plus",
                Children = new List<NavigationItem>()
            };

            if (user.IsAuthorizedFor(Permission.ManageUsers))
            {
                userManagementItem.Children.Add(new NavigationItem
                {
                    Text = "Users",
                    NavigateUrl = "usersViewer",
                    IconCssClass = "fa fa-user"
                });
            }
            
            if (user.IsAuthorizedFor(Permission.ManageUsers))
            {
                userManagementItem.Children.Add(new NavigationItem
                {
                    Text = "Import Users",
                    NavigateUrl = "bulkImportUsers",
                    IconCssClass = "fa fa-person-circle-plus"
                });
            }

            if (user.IsAuthorizedFor(Permission.ManageUsers))
            {
                userManagementItem.Children.Add(new NavigationItem
                {
                    Text = "Teams",
                    NavigateUrl = "teamsViewer",
                    IconCssClass = "fa fa-users"
                });
            }

            if (user.IsAuthorizedFor(Permission.ViewUsers))
            {
                userManagementItem.Children.Add(new NavigationItem
                {
                    Text = "Roles",
                    NavigateUrl = "rolesViewer",
                    IconCssClass = "fa fa-users"
                });
                userManagementItem.Children.Add(new NavigationItem
                {
                    Text = "Permission Viewer",
                    NavigateUrl = "permissionViewer",
                    IconCssClass = "fa fa-user"
                });
            }

            return userManagementItem;
        }
        private NavigationItem CreateServerManagerMenu(User user)
        {
            var serverManagerMenu = new NavigationItem
            {
                Text = "Server Manager",
                NavigateUrl = "",
                IconCssClass = "fa fa-server",
                Children = new List<NavigationItem>()
            };

            if (user.IsAuthorizedFor(Permission.OwnedOnlyServers))
            {
                serverManagerMenu.Children.Add(new NavigationItem
                {
                    Text = "Servers",
                    NavigateUrl = "servermanager/serverdetails",
                    IconCssClass = "fa fa-cogs"
                });
                
                serverManagerMenu.Children.Add(new NavigationItem
                {
                    Text = "Planning Areas",
                    NavigateUrl = "servermanager/planningareas",
                    IconCssClass = "fa fa-tachometer"
                });
                
                if (user.IsAuthorizedFor(Permission.EditIntegration))
                {
                    serverManagerMenu.Children.Add(new NavigationItem
                    {
                        Text = "Apply Integrations",
                        NavigateUrl = "servermanager/applyintegrations",
                        IconCssClass = "fa fa-database"
                    });
                }

                serverManagerMenu.Children.Add(new NavigationItem
                {
                    Text = "Actions",
                    NavigateUrl = "servermanager/actions",
                    IconCssClass = "fa fa-tasks"
                });
            }

            return serverManagerMenu;
        }
        // Method for Management Menu
        private NavigationItem CreateManagementMenu()
        {
            return new NavigationItem
            {
                Text = "PT Management",
                NavigateUrl = "",
                IconCssClass = "fa fa-briefcase",
                Children = new List<NavigationItem>
                {
                    new NavigationItem
                    {
                        Text = "All Users",
                        NavigateUrl = "usersViewer?adminView=true",
                        IconCssClass = "fa fa-user"
                    },
                    new NavigationItem
                    {
                        Text = "Companies",
                        NavigateUrl = "companiesViewer",
                        IconCssClass = "fa fa-building"
                    }
                }
            };
        }

        // Breadcrumb management methods
        public void SetNavigationPath(List<BreadcrumbItem> newBreadcrumbs)
        {
            if (newBreadcrumbs == null || newBreadcrumbs.Count == 0)
            {
                breadcrumbs = newBreadcrumbs;
                NotifyNavigationChanged();
                return;
            }

            for (int i = 0; i < newBreadcrumbs.Count; i++)
            {
                var breadcrumb = newBreadcrumbs[i];
                breadcrumb.HasValue = i == 0 || i == newBreadcrumbs.Count - 1;
            }

            breadcrumbs = newBreadcrumbs;
            NotifyNavigationChanged();
        }

        public List<BreadcrumbItem> GetNavigationPath() => breadcrumbs;
        
        private (object setter, INavigationStateService.NavigationPreemptionCallback preemptionCallback)? preemptionData;
        public void PreemptNavigation(object setter, INavigationStateService.NavigationPreemptionCallback preemptionCallback)
        {
            if (setter == null || preemptionCallback == null)
            {
                return;
            }

            if (preemptionData != null)
            {
                if (setter == preemptionData?.setter)
                {
                    return;
                }
            }

            preemptionData = new (setter, preemptionCallback);
        }
        
        public void ClearPreemption(object setter)
        {
            if (setter == null)
            {
                return;
            }

            if (preemptionData != null)
            {
                if (setter == preemptionData?.setter)
                {
                    preemptionData = null;
                }
            }
        }

        public void ExecutePreemption(string url, bool forceLoad, Action navFunction)
        {
            Task.Run(() => 
            {
                if (preemptionData != null)
                {
                    if (preemptionData.Value.preemptionCallback(url, forceLoad)) //if this returns true navigation is canceled
                    {
                        return;
                    }
                }
                navFunction();
            });
        }

        public void NavigateTo(string url, bool forceLoad = false)
        {
            ExecutePreemption(url, forceLoad, () =>
            {
                NavigationManager.NavigateTo(url, forceLoad);
                ClearPreemption(preemptionData?.setter); //generally objects should manage their preemption state but this is only used for the reports right now
                                                         //so this is fine for now. remove this if we ever expand on navigation preemption
            });
        }

        private void NotifyNavigationChanged() => OnNavigationChanged?.Invoke();
    }
}
