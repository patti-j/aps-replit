using ReportsWebApp.DB.Data;
using Microsoft.EntityFrameworkCore;
using Auth0.AspNetCore.Authentication;
using ReportsWebApp.DB.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ReportsWebApp.Hubs;
using Microsoft.Azure.Cosmos;
using ReportsWebApp.DB.Models;
using ReportsWebApp.Shared;
using ReportsWebApp.Common;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using ReportsWebApp.DB.Services.SchedulerHelpers;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using DevExpress.AIIntegration;
using Microsoft.ApplicationInsights;
using ReportsWebApp.DB.Services.Interfaces;
using System.ClientModel;
using System.Collections.Concurrent;

using ReportsWebApp;
using Azure;
using Azure.Messaging.EventHubs.Producer;

using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using ReportsWebApp.Helpers;
using Sentry;

var builder = WebApplication.CreateBuilder(args);

var cosmosDbConfig = builder.Configuration.GetSection("CosmosDb");
var cosmosDbEndpoint = cosmosDbConfig["EndpointUri"];
var cosmosDbKey = cosmosDbConfig["PrimaryKey"];
var cosmosDbDatabaseId = cosmosDbConfig["DatabaseId"];
var cosmosDbContainerId = cosmosDbConfig["ContainerId"];

//// Azure OpenAI Configuration
//var azureOpenAIConfig = builder.Configuration.GetSection("AzureOpenAI");
//string azureOpenAIEndpoint = azureOpenAIConfig["AZURE_OPENAI_ENDPOINT"];
//string azureOpenAIKey = azureOpenAIConfig["AZURE_OPENAI_API_KEY"];
//string deploymentName = azureOpenAIConfig["AZURE_OPENAI_DEPLOYMENT_NAME"];
//// Create an ApiKeyCredential
//var apiKeyCredential = new ApiKeyCredential(azureOpenAIKey);

//if (string.IsNullOrEmpty(deploymentName))
//{
//    throw new InvalidOperationException("Azure OpenAI deployment name is missing.");
//}

//if (string.IsNullOrEmpty(azureOpenAIEndpoint) || string.IsNullOrEmpty(azureOpenAIKey))
//{
//    throw new InvalidOperationException("Azure OpenAI configuration variables are missing.");
//}

// Add services to the container.
builder.Services.AddSingleton<CosmosClient>(new CosmosClient(cosmosDbEndpoint, cosmosDbKey));
builder.Services.AddSingleton<ICosmosDbService<GanttFavoriteData>, CosmosDbService<GanttFavoriteData>>(sp =>
{
    try
    {
        var client = sp.GetRequiredService<CosmosClient>();
        var databaseResponse = client.CreateDatabaseIfNotExistsAsync(cosmosDbDatabaseId).Result;
        var containerResponse = databaseResponse.Database.CreateContainerIfNotExistsAsync(cosmosDbContainerId, "/id").Result;

        return new CosmosDbService<GanttFavoriteData>(client, databaseResponse.Database, containerResponse.Container);
    }
    catch (Exception ex)
    {
        return new CosmosDbService<GanttFavoriteData>();
    }
});

// Add EventHub client
var hubConfig = builder.Configuration.GetSection("EventHub");
var hubConnectionString = hubConfig["ConnectionString"];
var hubName = hubConfig["HubName"];

if (hubConnectionString.IsNullOrEmpty())
{
    builder.Services.AddSingleton<EventHubProducerClient>(new DummyEventHupProducerClient());
}
else
{
    builder.Services.AddSingleton<EventHubProducerClient>(new EventHubProducerClient(hubConnectionString));
}

//// Add Azure OpenAI as a chat client
//var azureClient = new AzureOpenAIClient(
//    new Uri(azureOpenAIEndpoint),
//    new ApiKeyCredential(azureOpenAIKey));

//builder.Services.AddDevExpressAI((config) => {
//    config.RegisterOpenAIAssistants(azureClient, deploymentName);
//});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDevExpressBlazor();
//builder.Services.AddChatClient(cfg =>
//    cfg.Use(azureClient.AsChatClient(deploymentName))
//);
builder.Services.AddSignalR();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddDbContextFactory<DbReportsContext>(options =>
{
    options.EnableSensitiveDataLogging();
    ConfigureDBAndMigrationSource(builder, options);
});

// Add Sentry
if (!builder.Environment.IsDevelopment())
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = "https://08c2e75d37119c8776ddbb16ded91d17@o348563.ingest.us.sentry.io/4509159686340608";
        options.TracesSampleRate = 1.0;
        options.Debug = false;
        options.DiagnosticLevel = SentryLevel.Error;
        options.TracesSampleRate = 0.0;
    });

builder.Services.AddScoped<IDataConnectorService, DataConnectorService>();
builder.Services.AddScoped<IExternalIntegrationService, ExternalIntegrationService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPlanningAreaAssignmentPropagationService, PlanningAreaAssignmentPropagationService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICompanyDbService, CompanyDbService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlanningAreaLoginService, PlanningAreaLoginService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IServerManagerService, ServerManagerService>();
builder.Services.AddScoped<IDarkModeService, DarkModeService>();
builder.Services.AddScoped<IDBIntegrationService, DBIntegrationService>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<ServerActionsService>();
builder.Services.AddScoped<SchedulerInitializer>();
builder.Services.AddScoped<ISchedulerFavoritesService, SchedulerFavoritesService>();
builder.Services.AddScoped<IJobRetrievalService, JobRetrievalService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<HierarchyFilterService>();
builder.Services.AddScoped<PopupManagerService>();
builder.Services.AddScoped<INavigationStateService, NavigationStateService>();
builder.Services.AddScoped<ServiceContainer>();
builder.Services.AddScoped<ShortcutService>(); 
builder.Services.AddScoped<IAppInsightsLogger, AppInsightsLogger>();
builder.Services.AddScoped<IPlanningAreaDataService, PlanningAreaDataService>();
builder.Services.AddScoped<PowerBIService>();
builder.Services.AddScoped<ICTPRequestService, CTPRequestService>();
builder.Services.AddScoped<IIntegrationConfigService, IntegrationConfigService>();
builder.Services.AddScoped<NavMenuUpdateService>();
builder.Services.AddScoped<IGanttService, GanttService>();
builder.Services.AddScoped<IGanttDataService, GanttDataService>();
builder.Services.AddScoped<ICustomFieldService, CustomFieldService>();
builder.Services.AddScoped<ScopedAppStateService>();
builder.Services.AddScoped<IServerActionsService, ServerActionsService>();
builder.Services.AddScoped<AzureBlobService>();
builder.Services.AddScoped<AzureTableService>();
builder.Services.AddScoped<Auth0Service>();
builder.Services.AddScoped<Cookie>();
builder.Services.AddScoped<ScopeService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<AuditService>();


// Power BI Settings
PowerBISettings PowerBISettings = builder.Configuration.GetSection("PowerBI").Get<PowerBISettings>();
builder.Services.AddSingleton(PowerBISettings);
builder.Services.AddApplicationInsightsTelemetry();

// Set to allow 600MiB files for device software uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 600 * 1024 * 1024;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    // Set the maximum request body size (in bytes)
    options.Limits.MaxRequestBodySize = 600 * 1024 * 1024; // Example: 600 MB
});

var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultSignInScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
});
authenticationBuilder.AddAuth0WebAppAuthentication(Auth0Constants.AuthenticationScheme, options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.Scope = "openid profile email";
    options.OpenIdConnectEvents = new OpenIdConnectEvents()
    {
        OnTokenValidated = context => OnTokenValidatedFuncAsync(context)
    };
});

builder.Services.Configure<DevExpress.Blazor.Configuration.GlobalOptions>(options =>
{
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
});
builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

var telemetryClient = app.Services.GetRequiredService<TelemetryClient>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

var supportedCultures = new[] { "en-US", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
var signalRConnection = new HubConnectionBuilder().WithUrl($"{((IConfiguration)app.Services.GetService(typeof(IConfiguration)))["ApiUrl"].TrimEnd('/')}/realtime", options =>
                                                  {
                                                      options.Headers.Add("AzureAPIKey", "Pl@n3t10geth3r");
                                                  })
                                                  .WithAutomaticReconnect()
                                                  .Build();
Action startRealtimeConnection = null;
startRealtimeConnection = async () =>
{
    try
    {
        await signalRConnection.StartAsync();
    }
    catch (Exception e)
    {
       await OnWsError(e);
    }
};

signalRConnection.Closed += OnWsError;
await Task.Run(startRealtimeConnection);

async Task OnWsError(Exception? ex)
{
    await Task.Delay(5000);
    await Task.Run(startRealtimeConnection);
}

ConcurrentQueue<ActionStatus> statusQueue = new ConcurrentQueue<ActionStatus>();
var netMessage = new NetMessage(new SignalRClientMessageProvider(signalRConnection), MessageHandler);

var listener = new EventBusListener((ev) =>
{
    if (ev is UpdatePaApiKeyEvent updateEvent)
        netMessage.SendMessage("UpdatePaApiKey", updateEvent);
}, typeof(UpdatePaApiKeyEvent));

Task.Run(() =>
{
    while (true)
    {
        List<ActionStatus> statusList = new ();
        while (statusQueue.TryDequeue(out var status))
        {
            statusList.Add(status);
        }

        if (statusList.Count > 0)
        {
            EventBus.Main.PostEventSync(new ActionStatusUpdateEvent(new ActionStatusList(statusList)));
        }

        Task.Delay(3000).Wait();
    }
});
void MessageHandler(string a_messageTypeKey, string a_messageDataJson)
{
    switch (a_messageTypeKey)
    {
        case "PAStatusUpdate":
        {
            var statusList = JsonConvert.DeserializeObject<PlanningAreaStatusList>(a_messageDataJson);
            EventBus.Main.PostEventSync(new PAStatusUpdateEvent(statusList!));
            break;
        }
        case "ActionStatusUpdate":
        {
            var status = JsonConvert.DeserializeObject<ActionStatus>(a_messageDataJson);
            statusQueue.Enqueue(status!); //there might be a lot of updates coming through so we buffer them to only fire to listeners once every 3s
            break;
        }
        case "ServerAgentShutdown":
        {
            var status = JsonConvert.DeserializeObject<string>(a_messageDataJson);
            EventBus.Main.PostEventSync(new PlanningAreaShutdownEvent(status!));

            break;
        }
    }
}
app.MapHub<NotificationHub>("/notificationhub");

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var userIdentity = context?.User?.Claims?.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
    var props = new Dictionary<string, string>();
    await next.Invoke();
    if (userIdentity != null && (!context.Request.Path.ToString().StartsWith("/css") && !context.Request.Path.ToString().StartsWith("/_bla")))
    {
        props.Add("User", userIdentity + " Request Path: " + context.Request.Path);
        telemetryClient.TrackEvent("Login", props);
    }
});

app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
}

var NotificationService = app.Services.CreateScope().ServiceProvider.GetService<INotificationService>();
await NotificationService.SeedInitialData();

var paService = app.Services.CreateScope().ServiceProvider.GetService<IPlanningAreaLoginService>();
await paService.CreatePermissionGroupRecords(PermissionsGroupConstants.DefaultPermissions);

telemetryClient.TrackEvent("ApplicationStarted",
    new Dictionary<string, string> { { "Environment", builder.Environment.EnvironmentName } },
    new Dictionary<string, double> { { "StartupTime", DateTime.UtcNow.ToOADate() } });

app.Run();

async Task OnTokenValidatedFuncAsync(TokenValidatedContext context)
{
    // Save token for Planning Area Login
    var token = context.SecurityToken.RawData;
    if (!token.IsNullOrEmpty() &&
        context?.Principal?.Identity is ClaimsIdentity claimsIdentity)
    {
        claimsIdentity.AddClaim(new Claim("jwtToken", token));
    }
    await Task.CompletedTask.ConfigureAwait(false);
}

/// <summary>
/// Sets database options for EF Core.
/// Notably, determines what source of Migrations to use for Migration-related actions (including updating database).
/// </summary>

void ConfigureDBAndMigrationSource(WebApplicationBuilder webApplicationBuilder, DbContextOptionsBuilder dbContextOptionsBuilder)
{
    // Look for (optional) MigrationSource argument - this will only matter when doing actions from the Package Manager Console,
    // so it should be added using the -Args command there, not in appsettings or other config sources.
    // The Migration Assembly shouldn't matter for normal running of the application, so no need to set it then.
    var configuration = webApplicationBuilder.Configuration;
    var migrationSource = configuration.GetValue("MigrationAssembly", string.Empty);

    // Always use SQLServer, but use migration source based on above arg
    dbContextOptionsBuilder.UseSqlServer(webApplicationBuilder.Configuration.GetConnectionString("WebAppDatabase")
        , sqlOpts => _ = migrationSource switch
        {
            "Prod" => sqlOpts.MigrationsAssembly("ReportsWebApp.DB.Prod"),
            _ => sqlOpts.MigrationsAssembly("ReportsWebApp.DB"), // default
        });
}