using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace DoorNotifier.Sensor;

internal static class StartupExtensions
{
    public static HostApplicationBuilder AddSensorClient(this HostApplicationBuilder builder)
    {
        builder
            .Services
            .AddOptions<SensorOptions>()
            .Bind(builder.Configuration.GetSection(SensorOptions.Sensor))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddHttpClient<ISensorClient, SensorClient>((sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<SensorOptions>>().Value;
            httpClient.BaseAddress = options.Uri;
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/plain")
            );
        });

        return builder;
    }
}
