using DoorNotifier.Sensor;
using DoorNotifier.Notify;
using DoorNotifier;

var builder = Host.CreateApplicationBuilder(args);

builder.AddNotifyClient();
builder.AddSensorClient();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();