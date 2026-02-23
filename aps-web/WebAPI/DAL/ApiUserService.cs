using System.Transactions;

using Microsoft.EntityFrameworkCore;

using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.Shared;

namespace WebAPI.DAL
{
    public class ApiUserService
    {
        private readonly CompanyDBContext dbContext;
        private readonly IDbContextFactory<CompanyDBContext> _dbContextFactory;

        public ApiUserService(IDbContextFactory<CompanyDBContext> dbContextFactory, CompanyDBContext companyDBContext)
        {
            _dbContextFactory = dbContextFactory;
            dbContext = companyDBContext;
        }

        public async Task<bool> ValidateWebUserAsync(WebAPI.Models.User a_userModel)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            //Check if user exists
            var existingUser = dbContext.Users
                                        .Include(u => u.Groups)
                                        .AsNoTracking()
                                        .FirstOrDefault(u => u.Email.Equals(a_userModel.Email));
            if (existingUser != null) throw new Exception("User with this email " + existingUser.Email+ " already exists.");

            //Check if the email domain is in the allowed domains or company email domain.
            string email = a_userModel.Email;
            string userDomain = string.Empty;
            string companyDomain = string.Empty;

            if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
            {
                userDomain = email.Split('@')[1].Trim().ToLower();
            }
            else
            {
                // Handle invalid email format as needed
                throw new InvalidDataException("Invalid User Email.");
            }

            var company = dbContext.Companies.AsNoTracking()
                                   .Where(c => c.Id == a_userModel.CompanyId)
                                   .FirstOrDefault();
            companyDomain = company?.Email.Split('@')[1].Trim().ToLower() ?? string.Empty;
            var allowedDomains = company.AllowedDomains + "," + companyDomain;

            bool isAllowed = allowedDomains
                             .Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(d => d.Trim())
                             .Any(d => d.Equals(userDomain, StringComparison.OrdinalIgnoreCase));
            if (!isAllowed)
            {
                throw new InvalidDataException($"The domain {userDomain} is not allowed. Please contact a PlanetTogether Administrator if you believe this is incorrect.");
            }

            //Check if the groups are valid
            var companyGroups = dbContext.Roles.AsNoTracking()
                                         .Where(g => g.CompanyId == a_userModel.CompanyId)
                                         .ToList();

            bool allGroupsExistInCompany = true;
            var matchingGroups = new List<WebAPI.Models.Role>();
            var groupNotFound = string.Empty;
            foreach (var userGroup in a_userModel.Groups)
            {
                var matchingCompanyGroup = companyGroups.FirstOrDefault(cg => cg.Name == userGroup.Name);
                if (matchingCompanyGroup != null)
                {
                    matchingGroups.Add(matchingCompanyGroup);
                }
                else
                {
                    allGroupsExistInCompany = false;
                    groupNotFound = userGroup.Name;
                    break;
                }
            }

            if (!allGroupsExistInCompany)
            {
                throw new InvalidDataException("Group: "+ groupNotFound+ " does not exist in the company.");
            }

            a_userModel.Groups = matchingGroups;
            return true;
        }

        public async Task<bool> AddWebUserAsync(WebAPI.Models.User a_userModel)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            {
                if (a_userModel.Version != null)
                {
                    //user got deleted while editing
                    throw new DbUpdateConcurrencyException("User was deleted while you were editing. Please retry");
                }

                a_userModel.ExternalId = Guid.NewGuid().ToString();
                a_userModel.CreationDate = DateTime.UtcNow;
                a_userModel.CreatedBy = "Web API";

                //Adding groups again, since EF is thinking we are creating new groups
                var groups = await dbContext.Roles
                                            .Where(g => g.CompanyId == a_userModel.CompanyId && a_userModel
                                                                                                .Groups.Select(ug => ug.Name).Contains(g.Name))
                                            .ToListAsync();
                a_userModel.Groups = groups;
                dbContext.Users.Add(a_userModel);

                // Use the below code to verify the entity
                //var entries = dbContext.ChangeTracker.Entries()
                //                       .Select(e => new { Type = e.Entity.GetType().Name, e.State })
                //                       .ToList();
                await dbContext.SaveChangesAsync();
                a_userModel = null;
            }

            return true;
        }
    }
}
