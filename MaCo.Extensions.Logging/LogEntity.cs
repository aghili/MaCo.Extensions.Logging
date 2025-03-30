using System.Collections.Concurrent;

namespace Aghili.Logging;

internal class LogEntity
{
    public ConcurrentQueue<string> Messages { set; get; } = new ConcurrentQueue<string>();

    public int WriteTriedNumber { set; get; }
}
