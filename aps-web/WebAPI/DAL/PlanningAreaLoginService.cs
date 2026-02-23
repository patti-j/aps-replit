using Microsoft.EntityFrameworkCore;

using WebAPI.Models;

using UserLoginResponse = WebAPI.RequestsAndResponses.UserLoginResponse;

namespace WebAPI.DAL
{
    public class PlanningAreaLoginService
    {
        private readonly IDbContextFactory<CompanyDBContext> _dbContextFactory;


        public PlanningAreaLoginService(IDbContextFactory<CompanyDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<PAPermissionGroup>> GetPermissionGroupsAsync(Company company)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            return dbContext.PlanningAreaPermissionGroups.Where(x => x.CompanyId == company.Id).Include(x => x.Permissions).ToList();
        }

        public async Task<List<PlanningAreaLogin>> GetPAUsersForPlanningAreaAsync(string planningAreaKey)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            var pas =  await dbContext.PlanningAreaAccesses
                                        .Include(x => x.User)
                                        .Include(x => x.PlanningArea)
                                        .Include(x => x.PermissionGroup)
                                        .Where(x => 
                                            x.PlanningArea.PlanningAreaKey == planningAreaKey)
                                        .Distinct().ToListAsync();
            List<PlanningAreaLogin> planningAreaLogins = new List<PlanningAreaLogin>();
            
            foreach (PlanningAreaAccess access in pas)
            {
                planningAreaLogins.Add( new PlanningAreaLogin()
                {
                    Id = -1,
                    PAPermissionGroupId = access.PermissionGroupId,
                    PAPermissionGroup = access.PermissionGroup,
                    UserId = access.UserId,
                    User = access.User,
                    PlanningAreas = [new PlanningAreaAuthorization()
                    {
                        Name = access.PlanningArea.Name,
                        PlanningAreaKey = access.PlanningArea.PlanningAreaKey,
                        Version = access.PlanningArea.Version
                    }]
                });
            }
            
            return planningAreaLogins;
        }

        public async Task<UserLoginResponse?> AuthenticateUserAsync(string planningAreaKey, string email)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            User? user = dbContext.Users.FirstOrDefault(x => x.Email == email);

            dbContext.Dispose();
            
            if (user == null)
            {
                return null;
            }

            return await AuthenticateUserAsync(planningAreaKey, user.Id);
        }
        
        public async Task<UserLoginResponse?> AuthenticateUserAsync(string planningAreaKey, int id)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return null;
            }
            
            //TODO: Update this to read Teams/Scopes/Roles
            var maybeGroup = await dbContext.PlanningAreaAccesses
                                            .Include(x => x.PlanningArea)
                                            .Include(x => x.User)
                                            .Include(x => x.PermissionGroup)
                                            .Where(x => x.PlanningArea.PlanningAreaKey == planningAreaKey
                                                        && x.User.Id == id
                                                        && x.PermissionGroupId != null).FirstOrDefaultAsync();
            if (maybeGroup != null)
            {
                UserLoginResponse login = new ();
                login.WebAppId = id;
                login.PermissionSet = maybeGroup.PermissionGroup.Permissions.Select(x => x.PermissionKey).ToList();
                login.Email = user.Email;
                login.FirstName = user.Name;
                login.LastName = user.LastName;
                login.TaskNotes = user.TaskNotes;
                login.DisplayLanguage = user.DisplayLanguage;
                login.CompressionType = user.CompressionType;
                login.ExternalId = user.ExternalId;
                return login;
            }
            return null;
           
        }

        public async Task<List<PAUserPermission>> GetPAUserPermissionsForCompanyAsync(Company company)
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            return await dbContext.PAUserPermissions.Where(x => x.CompanyId == null || x.CompanyId == company.Id).ToListAsync();
        }
    }
}
