using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace MaCo.Extensions.Logging.Classes;

internal class LogOnlineAdapter : ILogWrite, IDisposable, IEquatable<LogType>
{
    private readonly string _execPath;
    private readonly string _offlineDir;
    private readonly ConcurrentQueue<LogPayload> _queue = new();
    // Shared, static HttpClient avoids socket exhaustion from per-instance clients.
    // Never dispose this — it lives for the process lifetime.
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _uploadTask;
    private bool _disposed;

    // Bound the offline store so a persistently unreachable server cannot fill the disk.
    private const int MaxOfflineFiles = 1000;

    public LogType WriterType { get; internal set; } = LogType.Online;

    public IWriterOption WriteOptions { get; set; } = new WriteOptionEvent();

    public event EventHandler<ShirinkEventArgs>? OnShiringRise;

    public LogOnlineAdapter()
    {
        _execPath = PathHelper.ResolveExecPath();
        _offlineDir = Path.Combine(_execPath, "Log", "Offline");
        RecoverOfflineFiles();
        _uploadTask = Task.Run(UploadLoop);
    }

    public void Write(LogMesssageType type, string path, string message) =>
        Enqueue(MapType(type), message, path);

    public void Write(LogLevel type, string path, string message) =>
        Enqueue(MapLogLevel(type), message, path);

    private void Enqueue(string telemetryCategory, string message, string source)
    {
        if (_disposed)
            return;
        _queue.Enqueue(new LogPayload
        {
            Type = telemetryCategory,
            TelemetryCategory = telemetryCategory,
            Message = message,
            Source = source
        });
    }

    private static string MapType(LogMesssageType type) => type switch
    {
        LogMesssageType.Exception => "error",
        LogMesssageType.DataLog => "behavior",
        LogMesssageType.Warrning => "event",
        LogMesssageType.Information => "event",
        _ => "event"
    };

    private static string MapLogLevel(LogLevel level) => level switch
    {
        LogLevel.Error or LogLevel.Critical => "error",
        _ => "event"
    };

    private async Task UploadLoop()
    {
        int interval = Log.Instance.Settings.Online.UploadIntervalSeconds;
        var delay = TimeSpan.FromSeconds(interval > 0 ? interval : 15);
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            await FlushAsync();
        }
    }

    private async Task FlushAsync()
    {
        var online = Log.Instance.Settings.Online;
        var batch = new List<LogPayload>();
        int limit = online.BatchSize > 0 ? online.BatchSize : 50;
        while (batch.Count < limit && _queue.TryDequeue(out LogPayload? item) && item != null)
            batch.Add(item);

        if (batch.Count == 0)
            return;

        if (string.IsNullOrEmpty(online.ApiEndpoint))
        {
            await PersistOfflineAsync(batch);
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, online.ApiEndpoint);
            request.Content = new StringContent(JsonSerializer.Serialize(batch), Encoding.UTF8, "application/json");
            if (!string.IsNullOrEmpty(online.ApiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", online.ApiKey);
            HttpResponseMessage response = await _httpClient.SendAsync(request, _cts.Token);
            if (!response.IsSuccessStatusCode)
                await PersistOfflineAsync(batch);
        }
        catch
        {
            await PersistOfflineAsync(batch);
        }
    }

    private async Task PersistOfflineAsync(List<LogPayload> batch)
    {
        try
        {
            Directory.CreateDirectory(_offlineDir);
            PruneOfflineStore();
            string file = Path.Combine(_offlineDir, $"{Guid.NewGuid():N}.json");
            string json = JsonSerializer.Serialize(batch);
#if NETSTANDARD2_0
            await Task.Run(() => File.WriteAllText(file, json), _cts.Token);
#else
            await File.WriteAllTextAsync(file, json, _cts.Token);
#endif
        }
        catch
        {
        }
    }

    private void PruneOfflineStore()
    {
        try
        {
            if (!Directory.Exists(_offlineDir))
                return;
            string[] files = Directory.GetFiles(_offlineDir, "*.json");
            if (files.Length < MaxOfflineFiles)
                return;
            Array.Sort(files, (a, b) => File.GetCreationTimeUtc(a).CompareTo(File.GetCreationTimeUtc(b)));
            int toDelete = files.Length - MaxOfflineFiles + 1;
            for (int i = 0; i < toDelete; i++)
                File.Delete(files[i]);
        }
        catch
        {
        }
    }

    private void RecoverOfflineFiles()
    {
        try
        {
            if (!Directory.Exists(_offlineDir))
                return;
            foreach (string file in Directory.GetFiles(_offlineDir, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    List<LogPayload>? items = JsonSerializer.Deserialize<List<LogPayload>>(json);
                    if (items != null)
                        foreach (LogPayload item in items)
                            _queue.Enqueue(item);
                    File.Delete(file);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            _cts.Cancel();
            try { _uploadTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }
            try { FlushAsync().GetAwaiter().GetResult(); } catch { }
            _cts.Dispose();
            // Do NOT dispose _httpClient — it's static and shared across instances.
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool Equals(LogType other) => WriterType == other;
}
