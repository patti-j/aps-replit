using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;


namespace ReportsWebApp.Shared
{
    public static class NavigationExtentions
    {
        /// <summary>
        /// Method <c>GetLocalRoute</c> returns the portion of the current Uri after the BaseUri
        /// </summary>
        /// <returns>The portion of the current Uri after the BaseUri</returns>
        public static string GetLocalRoute(this NavigationManager navigationManager)
        {
            return navigationManager.Uri.Substring(navigationManager.BaseUri.Length);
        }

        /// <summary>
        /// Method <c>ValidateAuth</c> checks if the user's authentication is still valid. If not, it redirects
        /// them to the login page.
        /// </summary>
        /// <param name="state">The User's Authentication State</param>
        /// <returns>True if the user's token is valid, False otherwise</returns>
        public static bool ValidateAuth(this NavigationManager navigationManager, AuthenticationState? state) 
        {
            if (state == null)
            {
                navigationManager.NavigateTo($"/login?redirectUri=/{navigationManager.GetLocalRoute()}", true);
                return false;
            }

            var token = state.User.Claims.Where(x => x.Type == "jwtToken").FirstOrDefault();

            JwtSecurityTokenHandler tokenHandler = new();
            var tokenInfo = tokenHandler.ReadToken(token.Value);
            TimeSpan remainingTime = tokenInfo.ValidTo - DateTime.UtcNow;

            // If user's token is expired or will expire in less than 10 seconds, refresh the token
            if (remainingTime.Ticks < 100000000)
            {
                navigationManager.NavigateTo($"/login?redirectUri=/{navigationManager.GetLocalRoute()}", true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method <c>IsUnregistered</c> checks if the user's account is email verified. If not, redirect them to verification
        /// </summary>
        /// <param name="state">The User's Authentication State</param>
        /// <returns>True if the user's email is registered, False otherwise</returns>
        public static bool IsUnregistered(this NavigationManager navigationManager, AuthenticationState? state)
        {
            var token = state.User.Claims.Where(x => x.Type == "email_verified").FirstOrDefault();

            if (token.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
