using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;


namespace Aghili.Logging.Classes;

public class LogFileAdapter : ILogWrite, IDisposable, IEquatable<LogType>
{
    private string ExecPath = "";
    public object objectLock = new object();
    private readonly ConcurrentDictionary<string, LogEntity> LogEntites = new ConcurrentDictionary<string, LogEntity>();
    private Thread? LogWriter = null;
    private bool disposedValue = false;

    public event EventHandler<ShirinkEventArgs>? OnShiringRise;

    public IWriterOption WriteOptions { get; set; } = new WriteOption();

    public bool Terminated { get; private set; } = false;

    public LogType WriterType { get; internal set; } = LogType.File;

    public LogFileAdapter() => InitialVariables();

    private void InitialVariables()
    {
        string? GetExecutingAssemblyLocation = null;
        try
        {
            GetExecutingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            GetExecutingAssemblyLocation = string.IsNullOrEmpty(GetExecutingAssemblyLocation) ? null : Path.GetDirectoryName(GetExecutingAssemblyLocation);
        }
        catch
        {
        }
        string? AppContextBaseDirectory = null;
        try
        {
            AppContextBaseDirectory = AppContext.BaseDirectory;
            AppContextBaseDirectory = string.IsNullOrEmpty(AppContextBaseDirectory) ? null : AppContextBaseDirectory;
        }
        catch
        {
        }
        string? EnvironmentCurrentDirectory = null;
        try
        {
            EnvironmentCurrentDirectory = Environment.CurrentDirectory;
            EnvironmentCurrentDirectory = string.IsNullOrEmpty(EnvironmentCurrentDirectory) ? null : EnvironmentCurrentDirectory;
        }
        catch
        {
        }
        string? TempFolder = null;
        try
        {
            TempFolder = Path.GetTempPath();
            TempFolder = string.IsNullOrEmpty(TempFolder) ? null : TempFolder;
        }
        catch 
        { 
        }

        ExecPath = AppContextBaseDirectory ?? GetExecutingAssemblyLocation ?? TempFolder ?? EnvironmentCurrentDirectory ?? "";
    }

    private void HintOnWriterEngine()
    {
        lock (objectLock)
        {
            if (LogWriter != null && LogWriter.IsAlive)
                return;
            LogWriter = null;
            Thread thread = new((() =>
            {
                int num = 2;
                while (num-- >= 0)
                {
                    try
                    {
                        if (LogEntites == null || LogEntites.IsEmpty)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            num = 2;
                            int max = LogEntites.Max(i => i.Value.Messages.Count);
                            foreach (KeyValuePair<string, LogEntity> keyValuePair in LogEntites.Where(i => i.Value.Messages.Count >= max).ToList<KeyValuePair<string, LogEntity>>())
                            {
                                string key = keyValuePair.Key;
                                LogEntity logItem = keyValuePair.Value;
                                if (logItem.WriteTriedNumber <= 100 && WriteLogEntity(key, logItem))
                                {
                                    while (!LogEntites.TryRemove(keyValuePair.Key, out var _))
                                        Thread.Sleep(100);
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (LogEntites != null && !LogEntites.IsEmpty)
                        {
                            if ((LogWriter != null ? (LogWriter.IsAlive ? 1 : 0) : 0) == 0)
                            {
                                if (LogEntites != null)
                                {
                                    lock (LogEntites)
                                    {
                                        foreach (KeyValuePair<string, LogEntity> logEntite in LogEntites)
                                            WriteLogEntity(logEntite.Key, logEntite.Value);
                                    }
                                }
                            }
                            else
                                Thread.Sleep(3000);
                        }
                    }
                }
            }))
            {
                Name = $"LogWriter {DateTime.Now}"
            };
            LogWriter = thread;
            LogWriter.Start();
        }
    }

    private void AddEntity(string File, string Message)
    {
        if (!LogEntites.ContainsKey(File))
        {
            while (!LogEntites.TryAdd(File, new LogEntity()))
                Thread.Sleep(100);
        }
        LogEntites[File].Messages.Enqueue(Message);
        HintOnWriterEngine();
    }

    protected static void CreateAndSetPermissions(string path)
    {
        try
        {
            FileInfo fileInfo = new(path);
            if (fileInfo.Directory == null)
            {
                Directory.CreateDirectory(path);
                fileInfo = new FileInfo(path);
            }
            else if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            if (fileInfo.Directory == null || !fileInfo.Exists)
                return;
#if !NETSTANDARD
            FileSystemAccessRule rule1 = new(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            DirectorySecurity accessControl1 = fileInfo.Directory.GetAccessControl();
            accessControl1.AddAccessRule(rule1);
            fileInfo.Directory.SetAccessControl(accessControl1);
            FileSystemAccessRule rule2 = new(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            FileSecurity accessControl2 = fileInfo.GetAccessControl();
            accessControl2.AddAccessRule(rule2);
            fileInfo.SetAccessControl(accessControl2);
#endif
        }
        catch (Exception ex)
        {
            Log.Instance.WriteNew(ex);
        }
    }

    private bool WriteLogEntity(string file, LogEntity logItem)
    {
        ++logItem.WriteTriedNumber;
        try
        {
            LogFileAdapter.CreateAndSetPermissions(file);
            FileStream fileStream = new(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            if (fileStream.Length > WriteOptions.LogRowLimitPerContainer * 1024)
            {
                fileStream.Close();
                Backup(file);
                fileStream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            }
            StreamWriter streamWriter = new(fileStream);
            foreach (string message in logItem.Messages)
                streamWriter.WriteLine(message);
            streamWriter.Flush();
            streamWriter.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void Backup(string file_name)
    {
        if (File.Exists(file_name + ".bak"))
        {
            try
            {
                File.Delete(file_name + ".bak");
            }
            catch
            {
            }
        }
        try
        {
            File.Move(file_name, file_name + ".bak");
        }
        catch
        {
            try
            {
                File.Delete(file_name);
            }
            catch
            {
            }
        }
        if (OnShiringRise == null)
            return;
        OnShiringRise(this, new ShirinkEventArgs()
        {
            RecordCount = 0,
            NewRecordCount = 0,
            Type = ShirinkType.Backup
        });
    }

    public void Write(LogMesssageType type, string path, string message)
    {
        path = ExecPath + "\\Log\\" + path;
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
                LogWriter?.Join(10000);
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

    public bool Equals(LogType other) => WriterType == other;
}