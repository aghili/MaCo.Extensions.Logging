using Aghili.Logging;
using Microsoft.Extensions.Logging;

namespace MaCo.Extensions.Logging
{
    public sealed class MaCoLogger(
     string name,
     Func<MaCoLoggerConfiguration> getCurrentConfig) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) =>
            getCurrentConfig().LogLevels.Contains(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            MaCoLoggerConfiguration config = getCurrentConfig();
            Aghili.Logging.Log.Instance.WriteNew(
                logLevel,
                eventId,
                state,
                exception,
                formatter);
        }
    }
}
