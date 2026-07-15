using Microsoft.Extensions.Logging;

namespace MaCo.Extensions.Logging.Classes;

public interface ILogWrite : IDisposable, IEquatable<LogType>
{
    IWriterOption WriteOptions { set; get; }

    LogType WriterType { get; }

    void Write(LogMessageType type, string path, string message);

    void Write(LogLevel type, string path, string message);

    event EventHandler<ShrinkEventArgs> OnShrinkRise;
}