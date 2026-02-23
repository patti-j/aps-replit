using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using ReportsWebApp.DB.Models;

namespace ReportsWebApp.Shared
{
    public class ApplicationInsightsComponent : ComponentBase, IDisposable
    {
        [Inject]
        private TelemetryClient _telemetryClient { get; init; }

        [Inject]
        private NavigationManager _navigationManager { get; init; }
        [Inject]
        private IUserService _userService { get; init; }
        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; init; }
        private User User { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            User = await _userService.GetCurrentUserAsync(_authenticationStateProvider);
            if (firstRender)
            {
                _navigationManager.LocationChanged += NavigationManagerOnLocationChanged;
                _authenticationStateProvider.AuthenticationStateChanged += AuthChanged;
                TrackPageView(_navigationManager.Uri, true);
            }
        }

        private async void AuthChanged(Task<AuthenticationState> task)
        {
            var state = await task;
            User = await _userService.GetCurrentUserAsync(state);
        }

        private void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            if (!e.IsNavigationIntercepted)
            {
                TrackPageView(e.Location);
            }
        }

        private void TrackPageView(string location, bool first = false)
        {
            var segments =location.Split('/');
            var name = location;
            if (segments.Length >= 4)
            {
                name = segments[3];
                if (name == "")
                {
                    name = "index";
                }
            }
            var tel = new PageViewTelemetry()
            {
                Name = name,
                Url = new Uri(location)
            };
            tel.Properties.Add("User", User.Email ?? "anonymous");
            tel.Properties.Add("Company", User.Company?.Name ?? "anonymous");

            if (first)
            {
                tel.Properties.Add("StartOfSession", "true");
            }
            _telemetryClient.TrackPageView(tel);
        }


        public void Dispose()
        {
            _navigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
        }
    }
}