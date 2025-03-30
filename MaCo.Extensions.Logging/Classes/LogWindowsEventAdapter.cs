
namespace Aghili.Logging.Classes;

internal class LogWindowsEventAdapter : ILogWrite, IDisposable, IEquatable<LogType>
{
    private bool disposedValue = false;

    public LogType WriterType { get; internal set; } = LogType.WindowsLogEvent;

    public IWriterOption WriteOptions { get; set; } = new WriteOptionEvent();

    public event EventHandler<ShirinkEventArgs> OnShiringRise;

    public void Write(LogMesssageType type, string path, string message)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
            return;
        disposedValue = true;
    }

    public void Dispose() => Dispose(true);

    public bool Equals(LogType other) => WriterType == other;
}
