using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

namespace BankService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = GetConfiguration(env);
            
            SetupLogger(env, configuration);
            try
            {
                Log.Information("Starting web host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();

        private static void SetupLogger(string env, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Environment", env)
                .ReadFrom.Configuration(configuration)
                //.WriteTo.Console()
                //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://192.168.250.135:30330/"))
                //{
                //    FailureCallback = e => Log.Information("Unable to submit event " + e.MessageTemplate),
                //    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                //                       EmitEventFailureHandling.WriteToFailureSink |
                //                       EmitEventFailureHandling.RaiseCallback,
                //    FailureSink = new LoggerConfiguration().WriteTo
                //        .File(new JsonFormatter(), "./fails.txt").CreateLogger()
                //})

                .CreateLogger();
            Log.Information("Logger setup");
        }

        private static IConfigurationRoot GetConfiguration(string env)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json")
                .Build();
        }
    }
}
