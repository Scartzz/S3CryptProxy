namespace S3CryptProxy.Test;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

public static class Program
{
    private static void Main(string[] args)
    {
        var hostBuilder = Host.CreateApplicationBuilder(args);

        hostBuilder.Services.AddSerilog((provider, configuration) => configuration
            .Enrich.FromLogContext()
            .ReadFrom.Services(provider)
            .WriteTo.Console());

        hostBuilder.Services.Configure<ServerSettings>(settings =>
        {
            settings.AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
            settings.SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");
            settings.Endpoint = Environment.GetEnvironmentVariable("S3_ENDPOINT");
        });
        hostBuilder.Services.AddHostedService<Server>();

        hostBuilder.Build().Run();
    }
}