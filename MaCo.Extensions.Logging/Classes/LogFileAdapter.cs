using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;


namespace MaCo.Extensions.Logging.Classes;

public class LogFileAdapter : ILogWrite, IDisposable, IEquatable<LogType>
{
    private string ExecPath = "";
    private readonly object objectLock = new object();
    private readonly ConcurrentDictionary<string, LogEntity> LogEntites = new ConcurrentDictionary<string, LogEntity>();
    private Thread? LogWriter = null;
    private readonly AutoResetEvent _writeSignal = new(false);
    private bool disposedValue = false;

    public event EventHandler<ShrinkEventArgs>? OnShrinkRise;

    public IWriterOption WriteOptions { get; set; } = new WriteOption();

    public bool Terminated { get; private set; } = false;

    public LogType WriterType { get; internal set; } = LogType.File;

    public LogFileAdapter() => ExecPath = PathHelper.ResolveExecPath();

    private void HintOnWriterEngine()
    {
        lock (objectLock)
        {
            if (LogWriter != null && LogWriter.IsAlive)
                return;
            LogWriter = new Thread(WriterLoop)
            {
                Name = $"LogWriter {DateTime.Now}"
            };
            LogWriter.Start();
        }
    }

    private const int MaxRetries = 100;

    private void WriterLoop()
    {
        while (!Terminated)
        {
            try
            {
                _writeSignal.WaitOne(TimeSpan.FromSeconds(1));
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            if (Terminated)
                break;
            try
            {
                ProcessEntities();
            }
            catch
            {
                // Swallow and retry on the next cycle; entities remain queued.
            }
        }
        // Final drain so pending entries are not lost on shutdown.
        try { ProcessEntities(); } catch { }
    }

    private void ProcessEntities()
    {
        if (LogEntites.IsEmpty)
            return;
        foreach (KeyValuePair<string, LogEntity> keyValuePair in LogEntites.ToList())
        {
            LogEntity entity = keyValuePair.Value;
            if (entity.WriteTriedNumber > MaxRetries)
            {
                // Give up on the primary file but keep the data on disk (dead-letter).
                if (DeadLetter(keyValuePair.Key, entity))
                    while (!LogEntites.TryRemove(keyValuePair.Key, out _))
                        Thread.Sleep(0);
                continue;
            }
            if (WriteLogEntity(keyValuePair.Key, entity))
                while (!LogEntites.TryRemove(keyValuePair.Key, out _))
                    Thread.Sleep(0);
        }
    }

    private bool DeadLetter(string file, LogEntity logItem)
    {
        try
        {
            string deadFile = file + ".dead";
            File.AppendAllLines(deadFile, logItem.Messages.ToArray());
            OnShrinkRise?.Invoke(this, new ShrinkEventArgs
            {
                RecordCount = logItem.Messages.Count,
                NewRecordCount = 0,
                Type = ShrinkType.Resize
            });
            return true;
        }
        catch
        {
            // Keep the entity queued so it is retried on the next cycle.
            return false;
        }
    }

    private void AddEntity(string File, string Message)
    {
        var entity = LogEntites.GetOrAdd(File, _ => new LogEntity());
        entity.Messages.Enqueue(Message);
        HintOnWriterEngine();
        _writeSignal.Set();
    }

    private bool WriteLogEntity(string file, LogEntity logItem)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                PermissionsHelper.EnsurePermissions(file);

            FileStream? fileStream = null;
            try
            {
                fileStream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                if (fileStream.Length > WriteOptions.LogRowLimitPerContainer * 1024)
                {
                    fileStream.Dispose();
                    Backup(file, logItem.Messages.Count);
                    fileStream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                }
                using var writer = new StreamWriter(fileStream);
                foreach (string message in logItem.Messages)
                    writer.WriteLine(message);
                writer.Flush();
            }
            finally
            {
                fileStream?.Dispose();
            }
            return true;
        }
        catch
        {
            logItem.IncrementWriteTriedNumber();
            return false;
        }
    }

    private void Backup(string file_name, int recordCount)
    {
        string backupName = file_name + ".bak";
        try
        {
            if (File.Exists(backupName))
                File.Delete(backupName);
            File.Move(file_name, backupName);
        }
        catch
        {
            // Leave the original file untouched on failure to avoid data loss.
            return;
        }
        OnShrinkRise?.Invoke(this, new ShrinkEventArgs()
        {
            RecordCount = recordCount,
            NewRecordCount = 0,
            Type = ShrinkType.Backup
        });
    }

    public void Write(LogMessageType type, string path, string message)
    {
        path = Path.Combine(ExecPath, "Log", path);
        AddEntity(path, message);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
            return;
        if (disposing)
        {
            try
            {
                Terminated = true;
                _writeSignal.Set();
                LogWriter?.Join(10000);
                // Final drain — ensure all pending entities are written to disk.
                try { ProcessEntities(); } catch { }
            }
            catch
            {
            }
        }
        disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Flush()
    {
        _writeSignal.Set();
        while (!LogEntites.IsEmpty)
            Thread.Sleep(10);
        LogWriter?.Join(1000);
    }

    public bool Equals(LogType other) => WriterType == other;

    public void Write(LogLevel type, string path, string message)
    {
        path = Path.Combine(ExecPath, "Log", path);
        AddEntity(path, message);
    }
}