using Microsoft.Extensions.Logging;

namespace MaCo.Extensions.Logging
{
    public sealed class MaCoLogger(
     string name,
     Func<MaCoLoggerConfiguration> getCurrentConfig) : ILogger
    {
        private readonly string _name = name;
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

            string formattedMessage = formatter(state, exception);
            string messageWithCategory = $"[{_name}] {formattedMessage}";
            MaCo.Extensions.Logging.Log.Instance.WriteNew(
                logLevel,
                messageWithCategory);
        }

        /// <summary>
        /// Called by MaCoLoggerExtensions to forward caller info directly.
        /// Bypasses StackTrace for accurate file/line reporting.
        /// </summary>
        internal void LogWithCallerInfo(
            LogLevel logLevel,
            string message,
            string member,
            string file,
            int line)
        {
            if (!IsEnabled(logLevel))
                return;

            string messageWithCategory = $"[{_name}] {message}";
            MaCo.Extensions.Logging.Log.Instance.WriteNew(
                logLevel,
                new object[] { messageWithCategory },
                member,
                file,
                line);
        }
    }
}
