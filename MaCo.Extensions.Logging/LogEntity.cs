using System.Collections.Concurrent;
using System.Threading;

namespace MaCo.Extensions.Logging;

internal class LogEntity
{
    public ConcurrentQueue<string> Messages { set; get; } = new ConcurrentQueue<string>();

    private int _writeTriedNumber;
    public int WriteTriedNumber => _writeTriedNumber;
    public int IncrementWriteTriedNumber() => Interlocked.Increment(ref _writeTriedNumber);
}
