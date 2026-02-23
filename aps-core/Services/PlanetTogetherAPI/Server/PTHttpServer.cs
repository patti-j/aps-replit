using System.Globalization;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#if DEBUG
using Microsoft.OpenApi; //For swagger
#endif

using PT.APIDefinitions;
using PT.APSCommon;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PlanetTogetherAPI.BinaryFormatters;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler.PackageDefs;

namespace PT.PlanetTogetherAPI;

public class PTHttpServer : IPTWebService, IDisposable
{
    private readonly UriBuilder m_serviceUrl;
    private readonly bool m_apiDiagnosticsOn;
    private readonly string m_thumbprint;
    private readonly IAuthorizer m_authorizer;
    private readonly bool m_isConfigMode;
    private readonly ICommonLogger m_logger;
    private IPackageManager m_packageManager;
    private IWebHost m_webHost;

    public PTHttpServer(UriBuilder a_serviceUrl, bool a_apiDiagnosticsOn, string a_thumbprint, IAuthorizer a_authorizer, bool a_isConfigMode, ICommonLogger a_logger)
    {
        m_serviceUrl = a_serviceUrl;
        m_apiDiagnosticsOn = a_apiDiagnosticsOn;
        m_thumbprint = a_thumbprint;
        m_authorizer = a_authorizer;
        m_isConfigMode = a_isConfigMode;
        //TODO: Make m_logger required. One needs to be created on Service startup
        m_logger = a_logger;
    }

    /// <summary>
    /// The system package manager can't be accessed until after construction, so this must be called to fully initialize before
    /// </summary>
    /// <param name="a_packageManager"></param>
    public void LoadPackageManager(IPackageManager a_packageManager)
    {
        m_packageManager = a_packageManager;
    }

    public void Start()
    {
        ControllerProperties.SetControllerProperties(m_apiDiagnosticsOn);
        //TODO: Do we really need to bind on an endpoint name? This shouldn't be needed any more.
        //(int port, EndPoint hostname, EndPoint cert, EndPoint cert2) = BuildHostNames(certificate);
        
        m_webHost = new WebHostBuilder()
                    .UseKestrel(k =>
                    {
                        if (m_serviceUrl.Scheme == "https")
                        {
                            k.ListenAnyIP(m_serviceUrl.Port, o => o.UseHttps(GetHostCert()));
                        }
                        else
                        {
                            k.ListenAnyIP(m_serviceUrl.Port);
                        }
                        //k.ListenLocalhost(port, o => o.UseHttps(certificate));
                        //k.ListenNamedPipe("PT-CavanDev2024", o => o.UseHttps(certificate));
                        //k.Listen(new NamedPipeEndPoint(), o => o.UseHttps(certificate));
                        //if (cert2 != null)
                        //{
                        //    k.Listen(cert2, o => o.UseHttps(certificate));
                        //}
                    })
                    .ConfigureLogging(logBuilder =>
                    {
                        logBuilder.SetMinimumLevel(LogLevel.Error);
                        logBuilder.AddEventLog();
                        logBuilder.AddConsole();
                    })
                    .UseStartup(_ => new APIStartup(m_authorizer, m_packageManager, m_isConfigMode)) //Pass in the object that can validate users
                    .Build();
        
        m_webHost.RunAsync();
    }

    /// <summary>
    /// Lookup and return the certificate based on the configured certificate thumbprint
    /// </summary>
    /// <returns></returns>
    private X509Certificate2 GetHostCert()
    {
        X509Store store = new (StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);

        if (string.IsNullOrEmpty(m_thumbprint))
        {
            PTValidationException noThumbprintException = new PTValidationException("No Certificate Thumbprint is defined");
            m_logger?.LogException(noThumbprintException, null, false, ELogClassification.PtService);
            if (m_logger == null)
            {
                throw noThumbprintException;
            }
        }

        X509Certificate2[] certs = store.Certificates.OfType<X509Certificate2>().ToArray();

        if (certs.Length == 0)
        {
            PTValidationException noThumbprintException = new ("No Certificates found.");
            m_logger?.LogException(noThumbprintException, null, false, ELogClassification.PtService);
            if (m_logger == null)
            {
                throw noThumbprintException;
            }
        }

        X509Certificate2 certificate = certs.FirstOrDefault(c => string.Compare(c.Thumbprint, m_thumbprint, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0);

        if (certificate == null)
        {
            PTValidationException noThumbprintException = new($"No Certificate found using configured thumbprint: {m_thumbprint}.");
            m_logger?.LogException(noThumbprintException, null, false, ELogClassification.PtService);
            if (m_logger == null)
            {
                throw noThumbprintException;
            }
        }

        return certificate;
    }

    public void Stop()
    {
        m_webHost?.StopAsync().Wait();
    }

    public void Dispose()
    {
        Stop();
    }
}

public class APIStartup
{
    /// <summary>
    /// The object that can authorize users
    /// </summary>
    private readonly IAuthorizer m_authorizer;

    private readonly bool m_isConfigMode;
    private readonly IPackageManager m_packageManager;

    public APIStartup(IAuthorizer a_authorizer, IPackageManager a_packageManager, bool a_isConfigMode = false)
    {
        m_authorizer = a_authorizer;
        m_packageManager = a_packageManager;
        m_isConfigMode = a_isConfigMode;
    }

    // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(options =>
                {
                    //options.DefaultScheme = "SessionTokenScheme";
                })
                //Add the session token scheme
                .AddScheme<PTSessionAuthSchemeOptions, PTSessionAuthHandler>(PTSessionAuthSchemeOptions.SchemeName, o => { o.Authorizer = m_authorizer; })
                .AddScheme<PTServerAuthSchemeOptions, PTServerAuthHandler>(PTServerAuthSchemeOptions.SchemeName, o => { o.Authorizer = m_authorizer; });
        IMvcBuilder controllersBuilder = services.AddControllers(opt =>
        {
            // Enable direct sending of byte arrays without JSON serialization.
            opt.InputFormatters.Insert(0, new BinaryInputFormatter());
            opt.OutputFormatters.Insert(0, new BinaryOutputFormatter());
            opt.Filters.Add<PTHttpExceptionHandlingFilter>();
            opt.RespectBrowserAcceptHeader = true;
            if (m_isConfigMode)
            {
                opt.Conventions.Add(new ConfigModeControllerFilterConvention());
            }
        });

        // Handles cacheing across API calls in a way that improves handling of concurrent requests to load the cache vs standard System.IMemoryCache
        services.AddLazyCache();

        LoadPackageControllers(controllersBuilder);

        services.AddTransient<IStartupFilter, PTAuthEnforcementFilter>();

#if DEBUG
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "SystemServiceAPI", Version = "v1" }); });
#endif
    }

    /// <summary>
    /// Loads in controllers from Packages assemblies which define them, so that they can be part of the HttpServer's API surface.
    /// </summary>
    /// <param name="a_controllersBuilder"></param>
    private void LoadPackageControllers(IMvcBuilder a_controllersBuilder)
    {
        if (m_packageManager == null)
        {
            throw new Exception("The web server must be initialized by calling LoadPackageManager prior to starting.");
        }

        List<IApiModule> apiModules = m_packageManager.GetApiModules();

        // Each package implementing IApiPackage may contain one or more API-specific modules,
        // with one or more controller parts to add.
        foreach (IApiModule apiModule in apiModules)
        {
            try
            {
                foreach (IApplicationPartElement servicePart in apiModule.GetServiceParts())
                {
                    a_controllersBuilder.PartManager.ApplicationParts.Add(servicePart.GetApplicationPart());
                }
            }
            catch (Exception e)
            {
                //TODO: Ideally log here instead of throw
                throw new PTPackageException($"Failed to load ApplicationParts from module {apiModule.GetType()}", e);
            }
        }

        a_controllersBuilder.AddControllersAsServices();
    }

    public void Configure(IApplicationBuilder app)
    {
#if DEBUG
        if (ControllerProperties.ApiDiagnosticsOn)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SystemServiceAPI v1"));
        }
#endif

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}