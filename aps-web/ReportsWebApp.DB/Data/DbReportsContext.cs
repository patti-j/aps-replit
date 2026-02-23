using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerBI.Api;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsWebApp.DB.Data
{
    public class DbReportsContext : DbContext
    {
        // Other DbSet properties go here...
        public DbSet<PADetails> PlanningAreas { get; set; }
        public DbSet<CtpRequest> CtpRequests { get; set; }
        public DbSet<JobTemplate> JobTemplates { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Report> Reports { get; set; } 
        public DbSet<PBIWorkspace> PBIWorkspace { get; set; }
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<PAPermissionGroup> PlanningAreaPermissionGroups => Set<PAPermissionGroup>();
        public DbSet<PlanningAreaAccess> PlanningAreaAccesses => Set<PlanningAreaAccess>();
        public DbSet<PAUserPermission> PAUserPermissions => Set<PAUserPermission>();
        public DbSet<PAPermissionGroupPAUserPermission> PAPermissionGroupPAUserPermission => Set<PAPermissionGroupPAUserPermission>();
        public DbSet<SchedulerFavorite> GanttFavorites => Set<SchedulerFavorite>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyDb> CompanyDbs { get; set; }
        public DbSet<CompanyServer> CompanyServers { get; set; }
        public DbSet<ServerCertificate> ServerCertificates { get; set; }
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RoleCategory> RoleCategories => Set<RoleCategory>();
        public DbSet<ServerUsingCompany> ServerUsingCompanies => Set<ServerUsingCompany>();
        public DbSet<ReportCategory> ReportCategories => Set<ReportCategory>();
        public DbSet<CustomField> CustomFields => Set<CustomField>();
        public DbSet<CFGroup> CFGroups => Set<CFGroup>();
        public DbSet<CustomFieldCFGroup> CustomFieldCFGroup => Set<CustomFieldCFGroup>();
        public DbSet<PlanningAreaTag> PlanningAreaTags => Set<PlanningAreaTag>();
        public DbSet<PlanningAreaPATag> PlanningAreaPATags => Set<PlanningAreaPATag>();
        public DbSet<PlanningAreaLocation> PlanningAreaLocations => Set<PlanningAreaLocation>();
        public DbSet<ExternalIntegration> ExternalIntegrations => Set<ExternalIntegration>();
        public DbSet<DataConnector> DataConnectors => Set<DataConnector>();
        public DbSet<GanttNote> GanttNotes => Set<GanttNote>();
        
        // Integration Config
        public DbSet<IntegrationConfig> IntegrationConfigs => Set<IntegrationConfig>();
        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<Property> Properties => Set<Property>();
        public DbSet<DBIntegration> DBIntegrations => Set<DBIntegration>();
        public DbSet<DBIntegrationObject> DBIntegrationObjects => Set<DBIntegrationObject>();
        public DbSet<InstallCode> InstallCodes => Set<InstallCode>();
        public DbSet<ServerManagerActionRequest> ServerManagerActionRequests => Set<ServerManagerActionRequest>();
        public DbSet<UserInviteLink> UserInviteLinks => Set<UserInviteLink>();
        public DbSet<SavedGridLayout> SavedGridLayouts => Set<SavedGridLayout>();
        public DbSet<PlanningAreaScope> PlanningAreaScopes => Set<PlanningAreaScope>();
        public DbSet<PlanningAreaScopeAssociationKey> PlanningAreaScopeAssociationKeys => Set<PlanningAreaScopeAssociationKey>();
        public DbSet<PAPlanningAreaScope> PAPlanningAreaScopes => Set<PAPlanningAreaScope>();
        public DbSet<CompanySubscriptionInfo> CompanySubscriptionInfo => Set<CompanySubscriptionInfo>();
        public DbSet<Team> Teams => Set<Team>();

        public DbReportsContext(DbContextOptions<DbReportsContext> options)
            : base(options)
        {
            //Recreate tables and seed them when debugging
#if DEBUG
            // To Recreate or Update the Database, run the command
            // "EntityFrameworkCore\update-database -Context DbReportsContext -Project ReportsWebApp.DB -StartupProject ReportsWebApp"
            // Make sure that Seed() and EnsureDeleted() are both commented out first!
            // Database.EnsureDeleted();
            // Seed();

            // To Add a migration to the Database, first make the necessary changes to the data models, then
            // run the command "EntityFrameworkCore\Add-Migration "MigrationName" -Context DbReportsContext -Project ReportsWebApp.DB -StartupProject ReportsWebApp"
            // Then, run update-database as above to apply the changes to the DB.
            //For generating migration scrip for production
            //Script-Migration -From <FromMigrationName> -To <ToMigrationName>

            // We maintain two different sets of Migrations: one which follows the history of schema changes in the dev branch, and one for prod.
            // These are expected to differ, and your database should apply migrations from just one of them (make a copy db if you have to!).
            // If doing any action related to migrations (creating/deleting them, updating database based on them, creating scripts) related to the PROD environment,
            // two changes are needed to your commands:
            // 1. Use '-Project ReportsWebApp.Db.Prod' (always add this, even if you normally don't provide the -Project arg)
            // 2. Add '-Args "--MigrationAssembly Prod"'
            // Devs merging migration changes into prod_staging must create a new prod migration as part of their PR in *that* branch (not dev).
            // To do this, you can generally run:
            //EntityFrameworkCore\Add-Migration "SameMigrationNameAsDevMigration" -Context DbReportsContext -Project ReportsWebApp.DB.Prod -StartupProject ReportsWebApp -Args "--MigrationAssembly Prod"
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerUsingCompany>()
                .HasKey(sc => new { sc.CompanyServerId, sc.CompanyId });

            modelBuilder.Entity<CompanyServer>()
                        .HasMany(x => x.ServerCertificates)
                        .WithOne(x => x.CompanyServer)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyServer>()
                .HasMany(sc => sc.UsingCompanies)
                .WithOne(c => c.CompanyServer)
                .HasForeignKey(sc => sc.CompanyServerId);

            modelBuilder.Entity<ServerUsingCompany>()
                .HasOne(sc => sc.Company)
                .WithMany()
                .HasForeignKey(sc => sc.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServerUsingCompany>()
                .HasOne(sc => sc.CompanyServer)
                .WithMany(s => s.UsingCompanies)
                .HasForeignKey(sc => sc.CompanyServerId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Role>()
                .HasMany(e => e.Users)
                .WithMany(e => e.Roles)
                .UsingEntity<UserRole>(
                    "UserRole",
                    ug => ug.HasOne(u => u.User).WithMany().HasForeignKey(e => e.UsersId).HasPrincipalKey(nameof(User.Id))
                        .OnDelete(DeleteBehavior.NoAction),
                    ug => ug.HasOne(u => u.Role).WithMany().HasForeignKey(e => e.RoleId).HasPrincipalKey(nameof(Role.Id))
                        .OnDelete(DeleteBehavior.NoAction)
                    );

            modelBuilder.Entity<PADetails>()
                        .HasMany(e => e.Scopes)
                        .WithMany(e => e.PlanningAreas)
                        .UsingEntity<PAPlanningAreaScope>(
                            "PAPlanningAreaScope",
                            ug => ug.HasOne(u => u.PlanningAreaScope).WithMany().HasForeignKey(e => e.PlanningAreaScopeId).HasPrincipalKey(nameof(PlanningAreaScope.Id))
                                    .OnDelete(DeleteBehavior.NoAction),
                            ug => ug.HasOne(u => u.PlanningArea).WithMany().HasForeignKey(e => e.PlanningAreaId).HasPrincipalKey(nameof(PADetails.Id))
                                    .OnDelete(DeleteBehavior.NoAction)
                        );

            modelBuilder.Entity<PADetails>()
                        .HasMany(e => e.Scopes)
                        .WithMany(e => e.PlanningAreas)
                        .UsingEntity<PAPlanningAreaScope>(
                            "PAPlanningAreaScope",
                            ug => ug.HasOne(u => u.PlanningAreaScope).WithMany().HasForeignKey(e => e.PlanningAreaScopeId).HasPrincipalKey(nameof(PlanningAreaScope.Id))
                                    .OnDelete(DeleteBehavior.NoAction),
                            ug => ug.HasOne(u => u.PlanningArea).WithMany().HasForeignKey(e => e.PlanningAreaId).HasPrincipalKey(nameof(PADetails.Id))
                                    .OnDelete(DeleteBehavior.NoAction)
                        );

            modelBuilder.Entity<PlanningAreaScopeAssociationKey>()
                        .HasOne(e => e.Scope)
                        .WithMany(e => e.PlanningAreaScopeAssociationKeys)
                        .HasForeignKey(e => e.ScopeId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
               .HasMany(e => e.Categories)
               .WithMany(e => e.Reports)
               .UsingEntity<ReportCategory>(
                   "CategoryReport",
                   l => l.HasOne(x => x.Category).WithMany().HasForeignKey(e => e.CategoriesId).HasPrincipalKey(nameof(Category.Id))
                         .OnDelete(DeleteBehavior.NoAction), // Specify OnDelete behavior
                   r => r.HasOne(x => x.Report).WithMany().HasForeignKey(e => e.ReportsId).HasPrincipalKey(nameof(Report.Id))
                         .OnDelete(DeleteBehavior.NoAction) // Specify OnDelete behavior
                   );

            modelBuilder.Entity<Report>()
               .HasOne(e => e.PBIWorkspace)
               .WithMany(e => e.Reports)
               .HasForeignKey(x => x.PBIWorkspaceId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                        .PrimitiveCollection(u => u.SubscribedNotifications);

            modelBuilder.Entity<IntegrationConfig>()
                .HasMany(e => e.Properties).WithOne(e => e.IntegrationConfig).HasForeignKey(e => e.IntegrationConfigId);

            modelBuilder.Entity<IntegrationConfig>()
                .HasMany(e => e.Features).WithOne(e => e.IntegrationConfig).HasForeignKey(e => e.IntegrationConfigId);

            modelBuilder.Entity<PADetails>()
                .HasOne(u => u.Company).WithMany();

            modelBuilder.Entity<JobTemplate>()
                .HasOne(u => u.Company).WithMany();

            modelBuilder.Entity<CtpRequest>()
                .HasOne(u => u.PADetails).WithMany();

            modelBuilder.Entity<CtpRequest>()
                .HasOne(u => u.JobTemplate).WithMany();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Company).WithMany();

            modelBuilder.Entity<PAPermissionGroup>()
                .HasMany(e => e.Permissions)
                .WithMany(e => e.Groups)
                .UsingEntity<PAPermissionGroupPAUserPermission>(
                   "PAPermissionGroupPAUserPermission",
                   r => r.HasOne(x => x.PAUserPermission).WithMany().HasForeignKey(e => e.PAUserPermissionId).HasPrincipalKey(nameof(PAUserPermission.Id))
                         .OnDelete(DeleteBehavior.NoAction), // Specify OnDelete behavior
                   l => l.HasOne(x => x.PAPermissionGroup).WithMany().HasForeignKey(e => e.PAPermissionGroupId).HasPrincipalKey(nameof(PAPermissionGroup.Id))
                         .OnDelete(DeleteBehavior.NoAction) // Specify OnDelete behavior
                   );

            modelBuilder.Entity<PlanningAreaTag>()
                .HasMany(e => e.PlanningAreas)
                .WithMany(e => e.Tags)
                .UsingEntity<PlanningAreaPATag>(
                   "PlanningAreaPATag",
                   r => r.HasOne(x => x.PlanningArea).WithMany().HasForeignKey(e => e.PlanningAreaId).HasPrincipalKey(nameof(PADetails.Id))
                         .OnDelete(DeleteBehavior.NoAction), // Specify OnDelete behavior
                   l => l.HasOne(x => x.PAGroup).WithMany().HasForeignKey(e => e.PAGroupId).HasPrincipalKey(nameof(PlanningAreaTag.Id))
                         .OnDelete(DeleteBehavior.NoAction) // Specify OnDelete behavior
                   );
            
            modelBuilder.Entity<PlanningAreaAccess>()
                        .HasOne(x => x.User)
                        .WithMany()
                        .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PlanningAreaAccess>()
                        .HasOne(x => x.PlanningArea)
                        .WithMany()
                        .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PlanningAreaAccess>()
                        .HasOne(x => x.PermissionGroup)
                        .WithMany()
                        .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<IntegrationConfig>()
                .HasMany(e => e.PlanningAreas)
                .WithMany(e => e.IntegrationConfigs)
                .UsingEntity<PlanningAreaIntegrationConfig>(
                   "PlanningAreaIntegrationConfig",
                   r => r.HasOne(x => x.PlanningArea).WithMany().HasForeignKey(e => e.PlanningAreaId).HasPrincipalKey(nameof(PADetails.Id))
                         .OnDelete(DeleteBehavior.NoAction), // Specify OnDelete behavior
                   l => l.HasOne(x => x.IntegrationConfig).WithMany().HasForeignKey(e => e.IntegrationConfigId).HasPrincipalKey(nameof(IntegrationConfig.Id))
                         .OnDelete(DeleteBehavior.NoAction) // Specify OnDelete behavior
                   );
            
            modelBuilder.Entity<Company>()
                        .HasMany<ExternalIntegration>().WithOne(e => e.Company).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ExternalIntegration>()
                        .HasMany<PADetails>().WithOne(e => e.ExternalIntegration).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Company>()
                        .HasMany<DataConnector>().WithOne(e => e.Company).OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Company>()
                .HasMany(e => e.Integrations).WithOne(e => e.Company);

            modelBuilder.Entity<PADetails>()
                .HasOne(e => e.UsedByCompany).WithMany().HasForeignKey(e => e.UsedByCompanyId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PADetails>()
                .HasOne(e => e.Server).WithMany().HasForeignKey(e => e.ServerId).OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<PADetails>()
                .HasOne(e => e.CurrentIntegration).WithMany().HasForeignKey(e => e.DBIntegrationId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CFGroup>()
                .HasMany(e => e.CustomFields)
                .WithMany(e => e.Groups)
                .UsingEntity<CustomFieldCFGroup>(
                   "CustomFieldCFGroup",
                   r => r.HasOne(x => x.CustomField).WithMany().HasForeignKey(e => e.CustomFieldId).HasPrincipalKey(nameof(CustomField.Id))
                         .OnDelete(DeleteBehavior.NoAction), // Specify OnDelete behavior
                   l => l.HasOne(x => x.CFGroup).WithMany().HasForeignKey(e => e.CFGroupId).HasPrincipalKey(nameof(CFGroup.Id))
                         .OnDelete(DeleteBehavior.NoAction) // Specify OnDelete behavior
                   );

            modelBuilder.Entity<Company>()
                .HasMany<User>().WithOne(u => u.Company);

            modelBuilder.Entity<PBIWorkspace>()
                   .HasMany(w => w.Reports)
                   .WithOne(r => r.PBIWorkspace)
                   .OnDelete(DeleteBehavior.Cascade);

            // Configure the GanttFavorite entity
            modelBuilder.Entity<SchedulerFavorite>()
                .HasOne(f => f.User) // GanttFavorite has a single User
                .WithMany(u => u.GanttFavorites) // User has many GanttFavorites
                .HasForeignKey(f => f.UserId) // The foreign key is UserId in GanttFavorite
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<SchedulerFavorite>()
                .HasOne(f => f.Company) // GanttFavorite has a single Company
                .WithMany(c => c.GanttFavorites) // Company has many GanttFavorites
                .HasForeignKey(f => f.CompanyId) // The foreign key is CompanyId in GanttFavorite
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<SchedulerFavorite>()
                .HasMany(f => f.Roles) 
                .WithMany(g => g.GanttFavorites)
                .UsingEntity("SchedulerFavorite");

            modelBuilder.Entity<Category>()
               .HasMany(e => e.Roles)
               .WithMany(e => e.Categories)
               .UsingEntity<RoleCategory>(
                   "RoleCategory",
                   l => l.HasOne(x => x.Role).WithMany().HasForeignKey(e => e.RolesId).HasPrincipalKey(nameof(Role.Id))
                         .OnDelete(DeleteBehavior.NoAction), // Specify OnDelete behavior
                   r => r.HasOne(x => x.Category).WithMany().HasForeignKey(e => e.CategoriesId).HasPrincipalKey(nameof(Category.Id))
                         .OnDelete(DeleteBehavior.NoAction) // Specify OnDelete behavior
                   );
            
            modelBuilder.Entity<GanttNote>()
                        .HasOne(x => x.Company)
                        .WithMany()
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Role>()
                        .PrimitiveCollection(g => g.Permissions);
            
            modelBuilder.Entity<Role>()
                        .PrimitiveCollection(g => g.DesktopPermissions);
            
            modelBuilder.Entity<Team>()
                        .HasMany(e => e.Users)
                        .WithMany(e => e.Teams)
                        .UsingEntity<UserTeam>(
                            "UserTeam",
                            ug => ug.HasOne(u => u.User).WithMany().HasForeignKey(e => e.UsersId).HasPrincipalKey(nameof(User.Id))
                                    .OnDelete(DeleteBehavior.NoAction),
                            ug => ug.HasOne(u => u.Team).WithMany().HasForeignKey(e => e.TeamId).HasPrincipalKey(nameof(Team.Id))
                                    .OnDelete(DeleteBehavior.NoAction)
                        );
            modelBuilder.Entity<Team>()
                        .OwnsMany(e => e.ComputedRolesAndScopes).ToJson();
            modelBuilder.Entity<ScopedRole>(entity =>
            {
                entity.HasKey(sr => new { sr.TeamId, sr.ScopeId, sr.RoleId });

                entity.HasOne(sr => sr.Team)
                      .WithMany(t => t.DBRolesAndScopes)
                      .HasForeignKey(sr => sr.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.Scope)
                      .WithMany()
                      .HasForeignKey(sr => sr.ScopeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Role)
                      .WithMany()
                      .HasForeignKey(sr => sr.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CompanySubscriptionInfo relationship  
            modelBuilder.Entity<CompanySubscriptionInfo>()
                        .HasOne(s => s.Company)
                        .WithMany() // No navigation property needed on Company
                        .HasForeignKey(s => s.CompanyId)
                        .OnDelete(DeleteBehavior.Cascade);
        }

        // Define your Seed method here
        public void Seed()
        {
            //Customer2: de6911d3-4f26-44e2-80ef-ec626e7b1c87
            //PowerBi: 
            //var PBIWorkspaceA = new PBIWorkspace() { PBIWorkspaceId = "07602c9e-4c2f-4edf-8625-2df87408d9a9", Name = "PowerBi" };
            //var PBIWorkspaceB = new PBIWorkspace() { PBIWorkspaceId = "de6911d3-4f26-44e2-80ef-ec626e7b1c87", Name = "Customer2" };
            var PBIWorkspaceTest = new PBIWorkspace() { CompanyId = 2, Name = "Customer1", PBIWorkspaceId = "514295ce-dcea-46c2-b17b-cac2fe51b47d" };

            if (!Companies.Any())
            {
                var companies = new List<Company>
                {
                    new Company { Name = "PlanetTogether Company", Email="bernard.uriza@planettogether.com", Active  = true},
                    new Company { Name = "Company1", Email="admin@c1.com",Active = true, Workspaces  = new List<PBIWorkspace>() {PBIWorkspaceTest}},
                    //new Company { Name = "Company 1 Pfizer", Email="bernard@gmail.com" },
                    //new Company { Name = "Company 2 AT&T", Email="byjutp@gmail.com"  }
                };

                Companies.AddRange(companies);
                SaveChanges();
            }

            if (!Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Plant1", CompanyId=2  },
                    new Category { Name = "Plant2", CompanyId=2  },
                    new Category { Name = "PlantA", CompanyId=1  },
                    new Category { Name = "PlantB", CompanyId=1  },
                     //Add more categories as needed
                };

                Categories.AddRange(categories);
                SaveChanges();
            }

            if (PlanningAreas.Any())
            {
                var requests = new List<CtpRequest>
                {
                    new CtpRequest
                    {
                        RequestId = 1,
                        ItemExternalId = "Item001",
                        WarehouseExternalId = "WH001",
                        RequiredQty = 100,
                        NeedDate = DateTime.UtcNow.AddDays(7),
                        Status = "On Time",
                        Priority = 1,
                        ScheduledStart = DateTime.UtcNow,
                        ScheduledFinish = DateTime.UtcNow.AddDays(5)
                    },
                    new CtpRequest
                    {
                        RequestId = 2,
                        ItemExternalId = "Item002",
                        WarehouseExternalId = "WH002",
                        RequiredQty = 200,
                        NeedDate = DateTime.UtcNow.AddDays(10),
                        Status = "Late",
                        Priority = 2,
                        ScheduledStart = DateTime.UtcNow.AddDays(1),
                        ScheduledFinish = DateTime.UtcNow.AddDays(6)
                    },
                    new CtpRequest
                    {
                        RequestId = 3,
                        ItemExternalId = "Item003",
                        WarehouseExternalId = "WH003",
                        RequiredQty = 150,
                        NeedDate = DateTime.UtcNow.AddDays(15),
                        Status = "On Time",
                        Priority = 3,
                        ScheduledStart = DateTime.UtcNow.AddDays(2),
                        ScheduledFinish = DateTime.UtcNow.AddDays(7)
                    },
                    new CtpRequest
                    {
                        RequestId = 4,
                        ItemExternalId = "Item004",
                        WarehouseExternalId = "WH004",
                        RequiredQty = 50,
                        NeedDate = DateTime.UtcNow.AddDays(20),
                        Status = "On Time",
                        Priority = 4,
                        ScheduledStart = DateTime.UtcNow.AddDays(3),
                        ScheduledFinish = DateTime.UtcNow.AddDays(8)
                    },
                    new CtpRequest
                    {
                        RequestId = 5,
                        ItemExternalId = "Item005",
                        WarehouseExternalId = "WH005",
                        RequiredQty = 300,
                        NeedDate = DateTime.UtcNow.AddDays(25),
                        Status = "Late",
                        Priority = 5,
                        ScheduledStart = DateTime.UtcNow.AddDays(4),
                        ScheduledFinish = DateTime.UtcNow.AddDays(9)
                    }
                };

                CtpRequests.AddRange(requests);
                SaveChanges();
            }

            if (!Reports.Any())
            {
                var reports = new List<Report>
                {
                    new Report { Name = "Report1", PBIWorkspace = PBIWorkspaceTest, PBIReportId = "", PBIReportName = "" },
                    new Report { Name = "Report2", PBIWorkspace = PBIWorkspaceTest, PBIReportId = "", PBIReportName = "" },
                };

                Reports.AddRange(reports);
                SaveChanges();
            }

            if (!Roles.Any())
            {
                var groups = new List<Role>
                {
                    new Role { Name = "Managers", CompanyId=2 },
                    new Role { Name = "Employee", CompanyId=2 },
                    new Role { Name = "Handlers", CompanyId=1 },
                    new Role { Name = "Member", CompanyId=1 },
                    // Add more reports as needed with valid PBIWorkspaceId values
                };

                Roles.AddRange(groups);
                SaveChanges();
            }
           
            if (!Users.Any())
            {
                //var group1 = Groups.First(c => c.Name == "Group1");
                //var group2 = Groups.First(c => c.Name == "Group2");
                var users = new List<User>
                {
                    new User { Name = "Byju", CompanyId=1, Email="byju.philip@planettogether.com"  },
                    new User { Name = "Admin", CompanyId=1, Email="admin@c1.com" },
                    //new User { Name = "User5", CompanyId=2, Role=UserType.CompanyAdmin, Email="byjutp@gmail.com" },
                    //new User { Name = "User6", CompanyId=2, Role=UserType.UserDefault, Email="byjup@yahoo.com" }
                    // Add more reports as needed with valid PBIWorkspaceId values
                };
                //var user = users.First(c => c.Role == UserType.UserDefault);
                //user.Groups.Add(group1);
                //user.Groups.Add(group2);

                Users.AddRange(users);
                SaveChanges();
            }
        }
    }
}