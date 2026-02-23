using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

using PT.Common;

using static System.Net.WebRequestMethods;

using User = ReportsWebApp.DB.Models.User;

namespace ReportsWebApp.DB.Services
{
    /// <summary>
    /// Connects to Auth0 for user creation and management.
    /// </summary>
    public class Auth0Service
    {
        private IConfiguration configuration;
        private IUserService userService;
        private HttpClient http;
        private string domain;
        private string url;
        private ManagementApiClient client;

        readonly char[] pwdChars = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+',
            '[', ']', '{', '}', '|', '\\', ';', ':', '\'', '"', ',', '.', '/',
            '<', '>', '?', '`', '~'
        };

        public Auth0Service(IConfiguration config, IUserService userService)
        {
            configuration = config;
            this.userService = userService;

            http = new HttpClient();

            domain = "https://" + configuration["Auth0:Domain"].Replace("https://", "");

            url = domain + "/api/v2/";

            var token = GetMgmtApiTokenAsync().Result;

            if (token == null)
            {
                throw new Exception("Failed to get Auth0 API Management Token. The appsettings file may be missing ClientSecret");
            }

            client = new ManagementApiClient(token, new Uri(url));
        }

        public async Task<string?> GetMgmtApiTokenAsync()
        {
            var request = new
            {
                client_id = configuration["Auth0:ClientId"],
                client_secret = configuration["Auth0:ClientSecret"],
                audience = url,
                grant_type = "client_credentials"
            };

            var mgmturl = domain + "/oauth/token";
            var content = JsonSerializer.Serialize(request);

            try
            {
                var task = http.PostAsync(
                    mgmturl,
                    new StringContent(content, Encoding.UTF8, "application/json")
                );

                var response = task.GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return payload.RootElement.GetProperty("access_token").GetString();
            }
            catch
            {
                return "";
            }
        }

        public async Task<User> CreateServiceUserInAuth0(User user)
        {
            var companyAdminEmail = user.Company.Email;

            string domain = companyAdminEmail.Split('@')[^1];
            
            try
            {
                var auth0user = await client.Users.CreateAsync(new UserCreateRequest()
                {
                    Email =  $"pt.svc.{user.Name}@{domain}.invalid",
                    FirstName = user.Name,
                    Connection = "Username-Password-Authentication",
                    Password = user.ServiceToken ?? throw new ArgumentNullException("user", "Users ServiceToken must be set when changing their auth0 password"),
                    UserId = user.Id.ToString(),
                    VerifyEmail = true
                });

                user.Email = auth0user.Email;
                return user;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task SetupAuth0User(User user, string baseUrl)
        {
            if (!await CheckIfUserIsValidated(user))
            {
                await CreateAuth0User(user, baseUrl);
            }
        }

        public async Task CreateAuth0User(User user, string baseUri)
        {
            var pwd = RandomNumberGenerator.GetString(pwdChars, 32);

            var auth0user = await client.Users.CreateAsync(new UserCreateRequest()
            {
                Email = user.Email,
                FirstName = user.Name,
                LastName = user.LastName,
                Connection = "Username-Password-Authentication",
                Password = pwd,
                UserId = user.Id.ToString(),
                VerifyEmail = false
            });

            await SendInviteLink(user, baseUri);
        }

        public async Task SendInviteLink(User user, string baseUri)
        {
            var token = await userService.CreateUserInviteLink(user);

            var emailer = Emailer.CreateWithPTSmtpSettings();

            emailer.SendVerificationEmailAsync(user.Email, baseUri + "UserAccountSetup/" + token);
        }

        public async Task SetAuth0PasswordForServiceUser(User user)
        {
            await client.Users.UpdateAsync("auth0|" + user.Id.ToString(), new UserUpdateRequest()
            {
                Password = user.ServiceToken ?? throw new ArgumentNullException("user", "Users ServiceToken must be set when changing their auth0 password"),
                Connection = "Username-Password-Authentication",
            });
        }

        public async Task SetAuth0PasswordAndEmailValidation(User user, string password)
        {
            try
            {
                await client.Users.UpdateAsync("auth0|" + user.Id, new UserUpdateRequest()
                {
                    EmailVerified = true,
                    Connection = "Username-Password-Authentication",
                });
                await client.Users.UpdateAsync("auth0|" + user.Id, new UserUpdateRequest()
                {
                    Password = password,
                    Connection = "Username-Password-Authentication",
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public async Task<bool> CheckIfUserIsValidated(User user)
        {
            Auth0.ManagementApi.Models.User auth0user;
            try
            {
                auth0user = await client.Users.GetAsync("auth0|" + user.Id);
            }
            catch
            {
                auth0user = null;
            }
            if (auth0user != null && auth0user.EmailVerified == true)
            {
                return true;
            }
            return false;
        }
    }
}
