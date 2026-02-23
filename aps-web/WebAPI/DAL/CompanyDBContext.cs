using Microsoft.EntityFrameworkCore;

using ReportsWebApp.DB.Models;

using WebAPI.Models;
using WebAPI.Models.CTP;

using Company = WebAPI.Models.Company;
using CompanyDb = WebAPI.Models.CompanyDb;
using CompanyServer = WebAPI.Models.CompanyServer;
using DBIntegration = WebAPI.Models.Integration.DBIntegration;
using DBIntegrationObject = WebAPI.Models.Integration.DBIntegrationObject;
using Feature = WebAPI.Models.Integration.Feature;
using InstallCode = WebAPI.Models.InstallCode;
using IntegrationConfig = WebAPI.Models.Integration.IntegrationConfig;
using PADetails = WebAPI.Models.Integration.PADetails;
using PAPermissionGroup = WebAPI.Models.PAPermissionGroup;
using PAPermissionGroupPAUserPermission = WebAPI.Models.PAPermissionGroupPAUserPermission;
using PAUserPermission = WebAPI.Models.PAUserPermission;
using PlanningAreaAccess = WebAPI.Models.PlanningAreaAccess;
using PlanningAreaIntegrationConfig = WebAPI.Models.Integration.PlanningAreaIntegrationConfig;
using PlanningAreaPATag = WebAPI.Models.PlanningAreaPATag;
using PlanningAreaTag = WebAPI.Models.PlanningAreaTag;
using Property = WebAPI.Models.Integration.Property;
using Role = WebAPI.Models.Role;
using ServerCertificate = WebAPI.Models.ServerCertificate;
using ServerManagerActionRequest = WebAPI.Models.ServerManagerActionRequest;
using ServerUsingCompany = WebAPI.Models.ServerUsingCompany;
using User = WebAPI.Models.User;
using UserRole = WebAPI.Models.UserRole;

namespace WebAPI.DAL
{
    public class CompanyDBContext : DbContext
    {
        public DbSet<PADetails> PlanningAreas { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyDb> CompanyDbs { get; set; }
        public DbSet<CompanyServer> CompanyServers { get; set; }
        public DbSet<ServerCertificate> ServerCertificates { get; set; }
        public DbSet<ServerUsingCompany> ServerUsingCompanies => Set<ServerUsingCompany>();
        public DbSet<PlanningAreaTag> PlanningAreaTags => Set<PlanningAreaTag>();
        public DbSet<PlanningAreaAccess> PlanningAreaAccesses => Set<PlanningAreaAccess>();

        public DbSet<Ctp> Ctps { get; set; }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<PAPermissionGroup> PlanningAreaPermissionGroups => Set<PAPermissionGroup>();
        public DbSet<PAUserPermission> PAUserPermissions => Set<PAUserPermission>();
        public DbSet<PAPermissionGroupPAUserPermission> PAPermissionGroupPAUserPermission => Set<PAPermissionGroupPAUserPermission>();
        public DbSet<InstallCode> InstallCodes => Set<InstallCode>();
        public DbSet<ServerManagerActionRequest> ServerManagerActionRequests => Set<ServerManagerActionRequest>();

        // Integration Config
        public DbSet<IntegrationConfig> IntegrationConfigs => Set<IntegrationConfig>();
        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<Property> Properties => Set<Property>();
        public DbSet<DBIntegration> DBIntegrations => Set<DBIntegration>();
        public DbSet<DBIntegrationObject> DBIntegrationObjects => Set<DBIntegrationObject>();
        public DbSet<DataConnector> DataConnectors => Set<DataConnector>();

        public CompanyDBContext(DbContextOptions<CompanyDBContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<CompanyDb>()
            //    .HasOne(c => c.Company)
            //    //.WithMany(i => i.Instances)
            //    .HasForeignKey(k => k.CompanyId);

            modelBuilder.Entity<ServerUsingCompany>()
                .HasKey(sc => new { sc.CompanyServerId, sc.CompanyId });

            modelBuilder.Entity<CompanyServer>()
                .HasMany(x => x.ServerCertificates)
                .WithOne(x => x.CompanyServer)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<PlanningAreaLogin>()
               .HasOne(u => u.User).WithMany().OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PADetails>()
                .HasOne(e => e.CurrentIntegration).WithMany().HasForeignKey(e => e.DBIntegrationId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<IntegrationConfig>()
                .HasMany(e => e.Properties).WithOne(e => e.IntegrationConfig).HasForeignKey(e => e.IntegrationConfigId);

            modelBuilder.Entity<IntegrationConfig>()
                .HasMany(e => e.Features).WithOne(e => e.IntegrationConfig).HasForeignKey(e => e.IntegrationConfigId);
           
            modelBuilder.Entity<Role>()
                .HasMany(e => e.Users)
                .WithMany(e => e.Groups)
                .UsingEntity<UserRole>(
                    "UserRole",
                    ug => ug.HasOne(u => u.User).WithMany().HasForeignKey(e => e.UsersId).HasPrincipalKey(nameof(User.Id))
                        .OnDelete(DeleteBehavior.NoAction),
                    ug => ug.HasOne(u => u.Role).WithMany().HasForeignKey(e => e.RoleId).HasPrincipalKey(nameof(Role.Id))
                        .OnDelete(DeleteBehavior.NoAction)
                );
            modelBuilder.Entity<Role>()
                        .PrimitiveCollection(g => g.Permissions);
            
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
            
            modelBuilder.Entity<DBIntegration>()
                .HasMany(e => e.IntegrationDBObjects).WithOne();
            
            modelBuilder.Entity<Company>()
                .HasMany(e => e.Integrations).WithOne(e => e.Company);
            
            modelBuilder.Entity<PADetails>()
                .HasOne(e => e.UsedByCompany).WithMany().HasForeignKey(e => e.UsedByCompanyId).OnDelete(DeleteBehavior.NoAction);
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
        }
    }
}
