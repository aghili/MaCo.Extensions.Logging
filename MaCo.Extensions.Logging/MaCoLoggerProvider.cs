using Microsoft.Extensions.Logging;

namespace MaCo.Extensions.Logging;

public sealed class MaCoLoggerProvider : ILoggerProvider
{
    private readonly Func<MaCoLoggerConfiguration> _getConfig;

    public MaCoLoggerProvider(Func<MaCoLoggerConfiguration> getConfig)
    {
        _getConfig = getConfig;
    }

    public ILogger CreateLogger(string categoryName) =>
        new MaCoLogger(categoryName, _getConfig);

    public void Dispose()
    {
        Log.Dispose();
    }
}
