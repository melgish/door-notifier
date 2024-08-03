using System.Net;

using DoorNotifier.Notify;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

namespace DoorNotifier.Tests.Notify;

public class NotifyClientTests
{
    private readonly FakeLogger<NotifyClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<NotifyOptions> _options;
    private readonly Mock<HttpMessageHandler> _mockHandler;


    public NotifyClientTests()
    {
        _mockHandler = new();
        _httpClient = new(_mockHandler.Object);
        _logger = new();
        _options = Options.Create(new NotifyOptions
        {
            Uri = new("http://127.0.0.1/test-topic")
        });
    }

    private NotifyClient CreateNotifyClient()
    {
        return new NotifyClient(_options, _logger, _httpClient);
    }

    [Fact]
    public async Task PostAsync_SuccessfulNotification_DoesNotLogFailure()
    {
        _mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
          )
          .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var client = CreateNotifyClient();
        await client.PostAsync("OPEN");

        Assert.Equal(0, _logger.Collector.Count);
    }

    [Fact]
    public async Task PostAsync_FailedStatusCode_LogsWarning()
    {
        var statusCode = HttpStatusCode.BadGateway;

        _mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
          )
          .ReturnsAsync(new HttpResponseMessage
          {
              StatusCode = statusCode,
          });

        var client = CreateNotifyClient();
        await client.PostAsync("CLOSED");

        Assert.Equal(LogLevel.Warning, _logger.LatestRecord.Level);
    }

    [Fact]
    public async Task PostAsync_Exception_LogsWarning()
    {
        _mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
          )
          .ThrowsAsync(new Exception("Network Error"));

        var client = CreateNotifyClient();
        await client.PostAsync("UNKNOWN");

        Assert.Equal(LogLevel.Warning, _logger.LatestRecord.Level);
        Assert.Equal(LogEvent.SendStatusFailed, _logger.LatestRecord.Id);
    }

}