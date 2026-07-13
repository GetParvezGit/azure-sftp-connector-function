using AzureSftpHelper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

IConfiguration config = BuildConfiguration(builder.Environment.ContentRootPath);

static IConfiguration BuildConfiguration(string contentRootPath)
{
    var config =
                  new ConfigurationBuilder()
                      .SetBasePath(contentRootPath)
                      .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                      .AddEnvironmentVariables()
                      .Build();

    return config;
}

builder.ConfigureFunctionsWebApplication();

builder.Services.AzureSftpHelperService();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
