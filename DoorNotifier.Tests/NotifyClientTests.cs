using DoorNotifier.Notify;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace DoorNotifier.Tests;

public class NotifyClientTests
{
    private readonly FakeLogger<NotifyClient> _fakeLogger;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<NotifyOptions>> _mockOptions;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly NotifyOptions _options;

    public NotifyClientTests()
    {
        _fakeLogger = new();

        _options = new()
        {
            After = TimeSpan.FromMinutes(5),
            Uri = new Uri("http://127.0.0.1/test-topic")
        };
        _mockOptions = new();
        _mockOptions.Setup(o => o.Value).Returns(_options);
        _mockHandler = new();
        _httpClient = new(_mockHandler.Object) { BaseAddress = _options.Uri };
    }

    [Fact]
    public void After_ShouldReturnOptionsValue()
    {
        var client = CreateNotifyClient();

        Assert.Equal(_options.After, client.After);
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

        Assert.Equal(0, _fakeLogger.Collector.Count);
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

        Assert.Equal(LogLevel.Warning, _fakeLogger.LatestRecord.Level);
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

        Assert.Equal(LogLevel.Warning, _fakeLogger.LatestRecord.Level);
        Assert.Equal(LogEvent.SendStatusFailed, _fakeLogger.LatestRecord.Id);
    }

    private NotifyClient CreateNotifyClient()
    {
        return new NotifyClient(_fakeLogger, _mockOptions.Object, _httpClient);
    }
}
