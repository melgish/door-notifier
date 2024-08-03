namespace DoorNotifier.Tests.Extensions;

using DoorNotifier.Extensions;

using Microsoft.Extensions.Configuration;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void Dump_ShouldInvokeActionForEachSource()
    {
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("key1", "value1"),
                new("key2", "value2")
            ])
            .AddInMemoryCollection(null)
            .AddJsonFile("appsettings.json", true, false)
            .AddEnvironmentVariables()
            .AddCommandLine(["--key3", "value3"])
            .AddConfiguration(new ConfigurationBuilder().Build());

        var count = 0;
        builder.Sources.Dump((source) =>
        {
            count++;
        });

        Assert.Equal(6, count);
    }
}