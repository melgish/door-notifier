using System.Reflection;

using DoorNotifier.Extensions;
using DoorNotifier.Notify;
using DoorNotifier.Sensor;
using DoorNotifier.Worker;

using Serilog;

// Use static log during startup to log any configuration warnings or errors.
Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateBootstrapLogger();

try
{
    var name = Assembly.GetExecutingAssembly().GetName();
    Log.Information("{AssemblyName} v{Version}", name.Name, name.Version);

    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.Sources.Dump(s => Log.Information("{Source}", s));

    builder.Services.AddSerilog((services, cfg) => cfg
      .ReadFrom.Configuration(builder.Configuration)
      .ReadFrom.Services(services)
    );

    // Notify
    builder.Services.AddHttpClient<INotifyClient, NotifyClient>();
    builder.Services.AddOptions<NotifyOptions>()
        .Bind(builder.Configuration.GetSection("Notify"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // Sensor
    builder.Services.AddHttpClient<ISensorClient, SensorClient>();
    builder.Services.AddOptions<SensorOptions>()
        .Bind(builder.Configuration.GetSection("Sensor"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // Worker
    builder.Services
        .AddHostedService<WorkerService>()
        .AddOptions<WorkerOptions>()
        .Bind(builder.Configuration.GetSection("Worker"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}