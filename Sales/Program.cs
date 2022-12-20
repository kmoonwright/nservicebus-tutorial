using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Sales
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Sales";
            await CreateHostBuilder(args).RunConsoleAsync();
        }

        // Custom ActivitySource to export via OpenTelemetry
        static class CustomActivitySources
        {
            public const string Name = "Example.SalesProcess";
            public static ActivitySource Main = new ActivitySource(Name);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .UseNServiceBus(context =>
                    {
                        var endpointConfiguration = new EndpointConfiguration("Sales");

                        endpointConfiguration.UseTransport<LearningTransport>();

                        endpointConfiguration.SendFailedMessagesTo("error");
                        endpointConfiguration.AuditProcessedMessagesTo("audit");
                        endpointConfiguration.SendHeartbeatTo("Particular.ServiceControl");

                        // So that when we test recoverability, we don't have to wait so long
                        // for the failed message to be sent to the error queue
                        var recoverablility = endpointConfiguration.Recoverability();
                        recoverablility.Delayed(
                            delayed =>
                            {
                                delayed.TimeIncrease(TimeSpan.FromSeconds(2));
                            }
                        );

                        var metrics = endpointConfiguration.EnableMetrics();
                        metrics.SendMetricDataToServiceControl("Particular.Monitoring", TimeSpan.FromMilliseconds(500));

                        // Open Telemetry Configurations for Sales Service
                        endpointConfiguration.EnableOpenTelemetry();
                        var serviceName = "Sales";
                        var serviceVersion = "1.0.0";
                        var APIKey = System.Environment.GetEnvironmentVariable("HONEYCOMB_API_KEY");
                        var tracerProvider = Sdk.CreateTracerProviderBuilder()
                            .AddOtlpExporter(option =>
                            {
                                option.Endpoint = new Uri("https://api.honeycomb.io/v1/traces");
                                option.Headers = $"x-honeycomb-team={APIKey}";
                                option.Protocol = OtlpExportProtocol.HttpProtobuf;
                            })
                            .SetResourceBuilder(
                                ResourceBuilder
                                    .CreateDefault()
                                    .AddService(
                                        serviceName: serviceName,
                                        serviceVersion: serviceVersion
                                    )
                            )
                            .AddSource("NServiceBus.core")
                            .AddSource(CustomActivitySources.Name)
                            .Build();

                        return endpointConfiguration;
                    });
        }
    }
}
