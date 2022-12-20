using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ClientUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        static class CustomActivitySources
        {
            public const string Name = "Example.ClientUI";
            public static ActivitySource Main = new ActivitySource(Name);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var APIKey = System.Environment.GetEnvironmentVariable("HONEYCOMB_API_KEY");
            services.AddControllers();
            services.AddMvc();
            services.AddOpenTelemetryTracing(
                config =>
                    config
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ClientUI"))
                        .AddAspNetCoreInstrumentation()
                        .AddSource("NServiceBus.Core")
                        .AddSource(CustomActivitySources.Name)
                        .AddOtlpExporter(option =>
                        {
                            option.Endpoint = new Uri("https://api.honeycomb.io/v1/traces");
                            option.Headers = $"x-honeycomb-team={APIKey}";
                            option.Protocol = OtlpExportProtocol.HttpProtobuf;
                        })
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
