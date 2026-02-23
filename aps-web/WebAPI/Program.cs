using Microsoft.EntityFrameworkCore;

using WebAPI;
using WebAPI.DAL;
using WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

if (!builder.Environment.IsDevelopment())
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = "https://3525dd1a077c8bda282830ba69e00af3@o348563.ingest.us.sentry.io/4509159703707648";
        options.TracesSampleRate = 1.0;
        options.Debug = false;
        options.DiagnosticLevel = SentryLevel.Error;
        options.TracesSampleRate = 0.0;
    });
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<CompanyDBContext>(options =>
{
    options.EnableSensitiveDataLogging();
    options.UseSqlServer(builder.Configuration.GetConnectionString("APICompanyDatabase"), (b) => b.EnableRetryOnFailure());
});
builder.Services.AddScoped<CompanyDBService>();
builder.Services.AddScoped<WebAPI.DAL.PlanningAreaLoginService>();
builder.Services.AddScoped<ServerManagerActionDbService>();
builder.Services.AddScoped<WebAPI.DAL.AzureTableService>();
builder.Services.AddScoped<WebAPI.DAL.AzureBlobService>();
builder.Services.AddScoped<WebAppApiService>();

builder.Services.AddScoped<ApiUserService>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IMessagePublisher, SignalRMessagePublisher>();
var app = builder.Build();
app.MapHub<RealTimeHub>("/realtime");
SignalRMessagePublisher.SendToWebAppEv += SignalRMessagePublisherOnSendToWebAppEv;

void SignalRMessagePublisherOnSendToWebAppEv(string messageKey, object messageData)
{
    app.Services.GetService<IMessagePublisher>()?.Publish(messageKey, messageData);
}

app.UseSwagger();
app.UseSwaggerUI();

if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
