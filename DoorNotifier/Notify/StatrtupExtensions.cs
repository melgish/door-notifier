using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace DoorNotifier.Notify;

internal static class StartupExtensions
{
    public static void AddNotifyClient(this IHostApplicationBuilder builder)
    {
        builder
            .Services
            .AddOptions<NotifyOptions>()
            .Bind(builder.Configuration.GetSection(NotifyOptions.Notify))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddHttpClient<INotifyClient, NotifyClient>((sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<NotifyOptions>>().Value;
            httpClient.BaseAddress = options.Uri;
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.Token);
        });
    }
}
