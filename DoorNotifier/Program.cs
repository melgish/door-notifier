using System.Reflection;

using DoorNotifier;
using DoorNotifier.Notify;
using DoorNotifier.Sensor;

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

    builder.AddNotifyClient();
    builder.AddSensorClient();

    builder.Services.AddHostedService<WorkerService>();

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

