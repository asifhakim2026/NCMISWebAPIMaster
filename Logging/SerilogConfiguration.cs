using Serilog;
using Serilog.Events;

namespace NCMISAPI.Logging;

/// <summary>
/// Central Serilog setup for console sink only (persistence goes to dbo.ErrorLogs).
/// Levels are fixed in code — no Serilog/Logging sections in appsettings.
/// </summary>
public static class SerilogConfiguration
{
    private const string ConsoleTemplate =
        "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Early logger used before the host is fully built.
    /// </summary>
    public static void CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: ConsoleTemplate)
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Registers Serilog with console sink only.
    /// </summary>
    public static WebApplicationBuilder AddNcmisSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
            ConfigureLogger(configuration, services),
            preserveStaticLogger: false,
            writeToProviders: false);

        return builder;
    }

    public static void ConfigureLogger(
        LoggerConfiguration configuration,
        IServiceProvider services)
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: ConsoleTemplate);
    }

    public static void CloseAndFlush() => Log.CloseAndFlush();
}
