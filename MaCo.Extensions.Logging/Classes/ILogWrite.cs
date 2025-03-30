namespace Aghili.Logging.Classes;

public interface ILogWrite : IDisposable, IEquatable<LogType>
{
    IWriterOption WriteOptions { set; get; }

    LogType WriterType { get; }

    void Write(LogMesssageType type, string path, string message);

    event EventHandler<ShirinkEventArgs> OnShiringRise;
}