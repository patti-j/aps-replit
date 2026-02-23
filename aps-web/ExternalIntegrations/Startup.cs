using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using PlanetTogetherContext.Contexts;

namespace ExternalIntegrations
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "External Integrations", Version = "v1"});
            });

            services.AddDbContext<PTContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LocalSQLConnection"), b => b.MigrationsAssembly("PlanetTogetherContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "External Integrations v1"));
            }

            //app.Use(async (ctx, next) =>
            //{
            //    if (ctx.Request.Path.ToString().Contains("Import"))
            //    {
            //       ctx.Request.Path = "/1/67/1/api/Rest/Import";
            //    }
            //    else
            //    {
            //        ctx.Request.Path = "/1/67/1/api/Rest/Publish";
            //    }

            //    //Console.WriteLine($"[{DateTime.Now}] {ctx.Request.Method} {ctx.Request.Path}");
            //    //ctx.Request.Path = "/1/67/1/api/Rest/Import";
            //    //Console.WriteLine($"[{DateTime.Now}] {ctx.Request.Method} {ctx.Request.Path}");
            //    await next();
            //});

            app.UseRouting();            

            app.UseAuthorization();            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
