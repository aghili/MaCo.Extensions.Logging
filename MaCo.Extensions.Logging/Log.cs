using Aghili.Logging.Classes;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;


namespace Aghili.Logging;

public partial class Log
{
    private static Log LogInstance = new();
    public List<ILogWrite> writeAdapter = [];
    private string ExecPath;

    private Log()
    {
        InitialVariables();
        LoadSettings();
        CreateLogAdapter();
    }

    protected static void CreateAndSetPermissions(string path)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
#if !NETSTANDARD            
            FileSystemAccessRule rule1 = new FileSystemAccessRule((IdentityReference)new SecurityIdentifier(WellKnownSidType.WorldSid, (SecurityIdentifier)null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            DirectorySecurity accessControl1 = fileInfo.Directory.GetAccessControl();
            accessControl1.AddAccessRule(rule1);
            fileInfo.Directory.SetAccessControl(accessControl1);
            if (!fileInfo.Exists)
                return;
            FileSystemAccessRule rule2 = new FileSystemAccessRule((IdentityReference)new SecurityIdentifier(WellKnownSidType.WorldSid, (SecurityIdentifier)null), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
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

    private void CreateLogAdapter()
    {
        bool flag = false;
        if (Settings.LogType.HasFlag(LogType.WindowsLogEvent))
        {
            try
            {
                writeAdapter.Add(new LogWindowsEventAdapter());
            }
            catch (SecurityException ex)
            {
                writeAdapter.Add(new LogFileAdapter());
                flag = true;
                WriteNew(ex,"Can not set log writer to Widnows Event Log!");
            }
        }
        if (Settings.LogType.HasFlag(LogType.File) && !flag)
            writeAdapter.Add(new LogFileAdapter());
        foreach (ILogWrite logWrite in writeAdapter)
        {
            logWrite.WriteOptions.LogKeepDataOnLimitRichedPercent = Settings.LogKeepDataOnLimitRichedPercent;
            logWrite.WriteOptions.LogRowLimitPerContainer = Settings.LogRowLimitPerContainer;
        }
    }

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

    private void LoadSettings()
    {
        try
        {
            Settings = JsonSerializer.Deserialize<LogSettings>(ReadSettingContent());
        }
        catch
        {
            try
            {
                Directory.CreateDirectory(ExecPath + "\\Log");
                File.WriteAllText(ExecPath + "\\Log\\Settings.json", JsonSerializer.Serialize(Settings));
            }
            catch
            {
            }
        }
    }

    public string ReadSettingContent()
    {
        CreateAndSetPermissions(ExecPath + "\\Log\\Settings.json");
        return File.ReadAllText(ExecPath + "\\Log\\Settings.json");
    }

    public static Log Instance
    {
        get
        {
            LogInstance ??= new();
            return LogInstance;
        }
    }

    public static void Dispose()
    {
        if (Log.LogInstance?.writeAdapter != null)
        {
            foreach (ILogWrite logWrite in LogInstance?.writeAdapter)
                logWrite?.Dispose();
        }
        LogInstance = null;
    }

    ~Log() => Dispose();

    public LogSettings Settings { set; get; } = new LogSettings();

    public void WriteNew(LogMesssageType type, params object[] msg)
    {
        if (!Settings.Enabled || !Settings.MesssageTypes.HasFlag(type))
            return;
        try
        {
            StackFrame caller = new StackTrace().GetFrame(1);
            bool CallerIsValid = caller != null;
            string ClassFullName = string.Empty;
            string MethodName = string.Empty;
            string FileThatContainMethod = "Unknown";
            string ClassName = string.Empty;
            int LineNumber = 0;
            if (CallerIsValid)
            {
                MethodBase method = caller.GetMethod();
                LineNumber = caller.GetFileLineNumber();
                if (method != null)
                {
                    ClassFullName = Utilites.RemoveIligalPathChars(method.DeclaringType?.FullName);
                    MethodName = Utilites.RemoveIligalPathChars(method.Name);
                    FileThatContainMethod = Utilites.RemoveIligalPathChars(method.Module?.ToString());
                    ClassName = Utilites.RemoveIligalPathChars(method.DeclaringType?.Name);
                }
            }
            string path = Path.Combine(FileThatContainMethod, ClassFullName, MethodName);
            string message = "";
            foreach (var item in msg)
            {
                if (item != null)
                {
                    if (typeof(IEnumerable<object>).IsAssignableFrom(item.GetType()))
                    {
                        foreach (object message_node in (object[])item)
                            message = message_node.ToString() + "=>";
                    }
                    else
                        message = message + item.ToString() + "=>";
                }
            }

            string HeaderDate = $"[{DateTime.Now:yyyy-MM-dd hh:mm:ss}][L:{LineNumber}]";
            string HeaderType = $"[{type}]";

            try
            {
                writeAdapterWrite(type, Path.Combine(path, $"{type}.log"), HeaderDate + message);
                writeAdapterWrite(type, Path.Combine(path, "AllMessages.log"), HeaderDate + HeaderType + message);
                writeAdapterWrite(type, "AllMessages.log", $"{HeaderDate}{HeaderType}{FileThatContainMethod}\\{ClassName}-{message}");
            }
            catch (Exception ex)
            {
                try
                {
                    writeAdapterWrite(LogMesssageType.Exception, "LogError.log",
                        $"ErrMgs  = {ex.Message}{Environment.NewLine}" +
                        $"Date    = {HeaderDate}{Environment.NewLine}" +
                        $"type    = {HeaderType}{Environment.NewLine}" +
                        $"path    = {path}{Environment.NewLine}" +
                        $"message = {message}{Environment.NewLine}" +
                        $"module  = {FileThatContainMethod}{Environment.NewLine}" +
                        $"DeclaringType.Name= {ClassName}{Environment.NewLine}" +
                        $"StackTrace = {ex.StackTrace}{Environment.NewLine}" +
                        $"----------------------------------------------------------------------------{Environment.NewLine}");
                }
                catch
                {
                }
            }
            finally
            {
            }
        }
        catch (StackOverflowException ex)
        {
        }
        catch (Exception ex)
        {
            try
            {
                writeAdapterWrite(LogMesssageType.Exception, "LogError.log",
                    $"Date      = {DateTime.Now:yyyy-MM-dd hh:mm:ss}+{Environment.NewLine}" +
                    $"Message   = {ex}{Environment.NewLine}" +
                    $"----------------------------------------------------------------------{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }

    private void writeAdapterWrite(LogMesssageType messageType, string path, string message)
    {
        foreach (ILogWrite logWrite in LogInstance?.writeAdapter)
            logWrite.Write(messageType, path, message);
    }

    public void WriteNew(Exception exIn, params object[] msg)
    {
        LogMesssageType logMesssageType = LogMesssageType.Exception;
        if (!Settings.Enabled || !Settings.MesssageTypes.HasFlag(logMesssageType))
            return;
        try
        {
            string message = "";
        
            StackFrame caller = new StackTrace(exIn, true).GetFrame(0);
            if (caller == null)
            {
                caller = new StackTrace().GetFrame(1);
                message = "[WARNING Exception object did not Throwed and just Created!]";
            }
            bool CallerIsValid = caller != null;
            string ClassFullName = string.Empty;
            string MethodName = string.Empty;
            string FileThatContainMethod = "Unknown";
            string ClassName = string.Empty;
            int LineNumber = 0;
            if (CallerIsValid)
            {
                MethodBase method = caller.GetMethod();
                LineNumber = caller.GetFileLineNumber();
                if (method != null)
                {
                    ClassFullName = Utilites.RemoveIligalPathChars(method.DeclaringType?.FullName);
                    MethodName = Utilites.RemoveIligalPathChars(method.Name);
                    FileThatContainMethod = Utilites.RemoveIligalPathChars(method.Module?.ToString());
                    ClassName = Utilites.RemoveIligalPathChars(method.DeclaringType?.Name);
                }
            }
            string path = Path.Combine(FileThatContainMethod, ClassFullName, MethodName);
            string pathException = Path.Combine("[All Exceptions]",path);
            message = exIn.ToString();
            if (exIn.InnerException != null)
                message += $"{Environment.NewLine} InnerException = {exIn.InnerException}";
            
            foreach (var item in msg)
            {
                if (item != null)
                {
                    if (typeof(IEnumerable<object>).IsAssignableFrom(item.GetType()))
                    {
                        foreach (object message_node in (object[])item)
                            message = message_node.ToString() + "=>";
                    }
                    else
                        message = message + item.ToString() + "=>";
                }
            }

            string HeaderDate = $"[{DateTime.Now:yyyy-MM-dd hh:mm:ss}][L:{LineNumber}]";
            string HeaderType = $"[{logMesssageType}]";

            try
            {
                writeAdapterWrite(logMesssageType, Path.Combine(pathException, $"{logMesssageType}.log"), HeaderDate + message);
                writeAdapterWrite(logMesssageType, Path.Combine(path, $"{logMesssageType}.log"), HeaderDate + message);
                writeAdapterWrite(logMesssageType, Path.Combine(path, "AllMessages.log"), HeaderDate + HeaderType + message);
                writeAdapterWrite(logMesssageType, "AllMessages.log", $"{HeaderDate}{HeaderType}{FileThatContainMethod}\\{ClassName}-{message}");
            }
            catch (Exception ex)
            {
                try
                {
                    writeAdapterWrite(LogMesssageType.Exception, "LogError.log",
                        $"ErrMgs  = {ex.Message}{Environment.NewLine}" +
                        $"Date    = {HeaderDate}{Environment.NewLine}" +
                        $"type    = {HeaderType}{Environment.NewLine}" +
                        $"path    = {path}{Environment.NewLine}" +
                        $"message = {message}{Environment.NewLine}" +
                        $"module  = {FileThatContainMethod}{Environment.NewLine}" +
                        $"DeclaringType.Name= {ClassName}{Environment.NewLine}" +
                        $"StackTrace = {ex.StackTrace}{Environment.NewLine}" +
                        $"----------------------------------------------------------------------------{Environment.NewLine}");
                }
                catch
                {
                }
            }
            finally
            {
            }
        }
        catch (StackOverflowException ex)
        {
        }
        catch (Exception ex)
        {
            try
            {
                writeAdapterWrite(LogMesssageType.Exception, "LogError.log",
                    $"Date      = {DateTime.Now:yyyy-MM-dd hh:mm:ss}+{Environment.NewLine}" +
                    $"Message   = {ex}{Environment.NewLine}" +
                    $"----------------------------------------------------------------------{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }

    public void Warrning(params object[] msg)
    {
        WriteNew(LogMesssageType.Warrning, msg);
    }
    public void Information(params object[] msg)
    {
        WriteNew(LogMesssageType.Information, msg);
    }
    public void Exception(params object[] msg)
    {
        WriteNew(LogMesssageType.Exception, msg);
    }
    public void DataLog(params object[] msg)
    {
        WriteNew(LogMesssageType.DataLog, msg);
    }
    public void Exception(Exception exIn, params object[] msg)
    {
        WriteNew(exIn, msg);
    }
}

public sealed class MaCoLogger(
    string name,
    Func<ColorConsoleLoggerConfiguration> getCurrentConfig) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) =>
        getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

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

        ColorConsoleLoggerConfiguration config = getCurrentConfig();
        if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

            Console.ForegroundColor = originalColor;
            Console.Write($"     {name} - ");

            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            Console.Write($"{formatter(state, exception)}");

            Console.ForegroundColor = originalColor;
            Console.WriteLine();
        }
    }
}