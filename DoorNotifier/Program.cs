using DoorNotifier;
using DoorNotifier.Notify;
using DoorNotifier.Sensor;

var builder = Host.CreateApplicationBuilder(args);

builder.AddNotifyClient();
builder.AddSensorClient();

builder.Services.AddHostedService<WorkerService>();

var host = builder.Build();
host.Run();