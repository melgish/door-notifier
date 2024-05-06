using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace DoorNotifier.Sensor;

internal static class StartupExtensions
{
    /// <summary>
    /// Sets up required services to add sensor client to DI container
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static void AddSensorClient(this IHostApplicationBuilder builder)
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
    }
}
