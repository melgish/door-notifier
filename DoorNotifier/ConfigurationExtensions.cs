using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Memory;

internal static class ConfigurationExtensions
{
    /// <summary>
    /// Dumps the configuration sources pecking order to the supplied action.
    /// </summary>
    public static IList<IConfigurationSource> Dump(
        this IList<IConfigurationSource> sources,
        Action<(string Name, string Parameter)> logAction
    )
    {
        sources
            .Select(s => (
                s.GetType().Name,
                Parameter: s switch
                {
                    // For known types display a relevant parameter.
                    CommandLineConfigurationSource c => $"Args={string.Join(',', c.Args)}",
                    EnvironmentVariablesConfigurationSource e => $"Prefix={e.Prefix}",
                    FileConfigurationSource f => $"Path={f.Path}",
                    MemoryConfigurationSource m => $"Keys={string.Join(',', m.InitialData?.Select(s => s.Key) ?? [])}",
                    _ => "Unknown"
                }))
            .ToList()
            .ForEach(logAction);
        return sources;
    }
}