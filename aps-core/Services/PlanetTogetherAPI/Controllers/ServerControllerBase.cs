using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;

namespace PT.PlanetTogetherAPI.Controllers;

//Authorize using the local server token created by ServerManager
[Authorize(AuthenticationSchemes = PTServerAuthSchemeOptions.SchemeName)]
public class ServerControllerBase : ControllerBase { }