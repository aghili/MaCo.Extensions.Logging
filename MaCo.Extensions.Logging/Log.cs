using MaCo.Extensions.Logging.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text.Json;


namespace MaCo.Extensions.Logging;

public partial class Log
{
    private static Log LogInstance = new();
    public List<ILogWrite> writeAdapter = [];
    private readonly object _adapterLock = new();
    private string ExecPath;

    private Log()
    {
        InitialVariables();
        LoadSettings();
        CreateLogAdapter();
    }

#if !NETSTANDARD
    [SupportedOSPlatform("windows")]
#endif
    protected static void CreateAndSetPermissions(string path) =>
        PermissionsHelper.EnsurePermissions(path);

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
        if (Settings.LogType.HasFlag(LogType.Online) && Settings.Online.Enabled)
            writeAdapter.Add(new LogOnlineAdapter());
        foreach (ILogWrite logWrite in writeAdapter)
        {
            logWrite.WriteOptions.LogKeepDataOnLimitRichedPercent = Settings.LogKeepDataOnLimitRichedPercent;
            logWrite.WriteOptions.LogRowLimitPerContainer = Settings.LogRowLimitPerContainer;
        }
    }

    private void InitialVariables()
    {
        ExecPath = PathHelper.ResolveExecPath();
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
                Directory.CreateDirectory(Path.Combine(ExecPath, "Log"));
                File.WriteAllText(Path.Combine(ExecPath, "Log", "Settings.json"), JsonSerializer.Serialize(Settings));
            }
            catch
            {
            }
        }
    }

    private string ReadSettingContent()
    {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateAndSetPermissions(Path.Combine(ExecPath, "Log", "Settings.json"));
        return File.ReadAllText(Path.Combine(ExecPath, "Log", "Settings.json"));
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
        Log? instance;
        lock (Instance._adapterLock)
        {
            instance = LogInstance;
            LogInstance = null;
        }
        if (instance?.writeAdapter != null)
        {
            foreach (ILogWrite logWrite in instance.writeAdapter)
                logWrite?.Dispose();
        }
    }

    public static void Configure(IConfiguration configuration, string sectionName = "Logging:MaCo")
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        IConfigurationSection section = configuration.GetSection(sectionName);
        if (section.Exists())
        {
            section.Bind(Instance.Settings);
            Instance.RebuildAdapters();
        }
    }

    private void RebuildAdapters()
    {
        lock (_adapterLock)
        {
            foreach (ILogWrite logWrite in writeAdapter)
                logWrite?.Dispose();
            writeAdapter.Clear();
            CreateLogAdapter();
        }
    }

    ~Log() => Dispose();

    public LogSettings Settings { set; get; } = new LogSettings();

    public void WriteNew(LogMesssageType type, params object[] msg)
    {
        if (!Settings.Enabled || !Settings.MesssageTypes.HasFlag(type))
            return;
        CallerContext ctx = ResolveCaller(new StackTrace().GetFrame(1));
        string path = Path.Combine(ctx.FileThatContainMethod, ctx.ClassFullName, ctx.MethodName);
        string message = BuildMessage(msg);
        WriteNewCore(type, path, ctx, message);
    }

    public void WriteNew(LogMesssageType type, object[] msg,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (!Settings.Enabled || !Settings.MesssageTypes.HasFlag(type))
            return;
        CallerContext ctx = BuildCallerContext(member, file, line);
        string path = Path.Combine(ctx.FileThatContainMethod, ctx.ClassFullName, ctx.MethodName);
        string message = BuildMessage(msg);
        WriteNewCore(type, path, ctx, message);
    }

    public void WriteNew(LogLevel type, params object[] msg)
    {
        LogMesssageType mapped = MapLogLevel(type);
        if (!Settings.Enabled || !Settings.MesssageTypes.HasFlag(mapped))
            return;
        CallerContext ctx = ResolveCaller(new StackTrace().GetFrame(1));
        string path = Path.Combine(ctx.FileThatContainMethod, ctx.ClassFullName, ctx.MethodName);
        string message = BuildMessage(msg);
        WriteNewCore(mapped, path, ctx, message);
    }

    public void WriteNew(LogLevel type, object[] msg,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        LogMesssageType mapped = MapLogLevel(type);
        if (!Settings.Enabled || !Settings.MesssageTypes.HasFlag(mapped))
            return;
        CallerContext ctx = BuildCallerContext(member, file, line);
        string path = Path.Combine(ctx.FileThatContainMethod, ctx.ClassFullName, ctx.MethodName);
        string message = BuildMessage(msg);
        WriteNewCore(mapped, path, ctx, message);
    }

    private void WriteNewCore(LogMesssageType type, string path, CallerContext ctx, string message, string extraPath = null)
    {
        string headerDate = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][L:{ctx.LineNumber}]";
        string headerType = $"[{type}]";
        try
        {
            writeAdapterWrite(type, Path.Combine(path, $"{type}.log"), headerDate + message);
            writeAdapterWrite(type, Path.Combine(path, "AllMessages.log"), headerDate + headerType + message);
            writeAdapterWrite(type, "AllMessages.log", $"{headerDate}{headerType}{ctx.FileThatContainMethod}{Path.DirectorySeparatorChar}{ctx.ClassName}-{message}");
            if (extraPath != null)
                writeAdapterWrite(type, Path.Combine(extraPath, $"{type}.log"), headerDate + message);
        }
        catch (Exception ex)
        {
            WriteErrorLog(headerDate, headerType, path, message, ctx, ex);
        }
    }

    private static LogMesssageType MapLogLevel(LogLevel level) => level switch
    {
        LogLevel.Warning => LogMesssageType.Warrning,
        LogLevel.Error or LogLevel.Critical => LogMesssageType.Exception,
        _ => LogMesssageType.Information
    };

    public void WriteNew<TState>(
        LogLevel level,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!Settings.Enabled)
            return;
        LogMesssageType mapped = MapLogLevel(level);
        if (!Settings.MesssageTypes.HasFlag(mapped))
            return;
        CallerContext ctx = ResolveCaller(new StackTrace().GetFrame(2));
        string path = Path.Combine(ctx.FileThatContainMethod, ctx.ClassFullName, ctx.MethodName);
        string message = formatter(state, exception);
        WriteNewCore(mapped, path, ctx, message);
    }

    public void WriteNew<TState>(
        LogLevel level,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (!Settings.Enabled)
            return;
        LogMesssageType mapped = MapLogLevel(level);
        if (!Settings.MesssageTypes.HasFlag(mapped))
            return;
        CallerContext ctx = BuildCallerContext(member, file, line);
        string path = Path.Combine(ctx.FileThatContainMethod, ctx.ClassFullName, ctx.MethodName);
        string message = formatter(state, exception);
        WriteNewCore(mapped, path, ctx, message);
    }

    private void WriteErrorLog(string headerDate, string headerType, string path, string message, CallerContext ctx, Exception ex)
    {
        try
        {
            writeAdapterWrite(LogMesssageType.Exception, "LogError.log",
                $"ErrMgs  = {ex.Message}{Environment.NewLine}" +
                $"Date    = {headerDate}{Environment.NewLine}" +
                $"type    = {headerType}{Environment.NewLine}" +
                $"path    = {path}{Environment.NewLine}" +
                $"message = {message}{Environment.NewLine}" +
                $"module  = {ctx.FileThatContainMethod}{Environment.NewLine}" +
                $"DeclaringType.Name= {ctx.ClassName}{Environment.NewLine}" +
                $"StackTrace = {ex.StackTrace}{Environment.NewLine}" +
                $"----------------------------------------------------------------------------{Environment.NewLine}");
        }
        catch
        {
        }
    }

    private struct CallerContext
    {
        public string ClassFullName;
        public string MethodName;
        public string FileThatContainMethod;
        public string ClassName;
        public int LineNumber;
    }

    private static CallerContext BuildCallerContext(string member, string file, int line)
    {
        return new CallerContext
        {
            LineNumber = line,
            ClassFullName = Utilites.RemoveIligalPathChars(Path.GetFileNameWithoutExtension(file)),
            MethodName = Utilites.RemoveIligalPathChars(member),
            FileThatContainMethod = Utilites.RemoveIligalPathChars(file),
            ClassName = Utilites.RemoveIligalPathChars(Path.GetFileNameWithoutExtension(file))
        };
    }

    private static CallerContext ResolveCaller(StackFrame caller)
    {
        CallerContext ctx = new CallerContext { FileThatContainMethod = "Unknown" };
        if (caller?.GetMethod() is { } method)
        {
            ctx.LineNumber = caller.GetFileLineNumber();
            ctx.ClassFullName = Utilites.RemoveIligalPathChars(method.DeclaringType?.FullName);
            ctx.MethodName = Utilites.RemoveIligalPathChars(method.Name);
            ctx.FileThatContainMethod = Utilites.RemoveIligalPathChars(method.Module?.ToString());
            ctx.ClassName = Utilites.RemoveIligalPathChars(method.DeclaringType?.Name);
        }
        return ctx;
    }

    private void writeAdapterWrite(LogMesssageType messageType, string path, string message)
    {
        List<ILogWrite>? adapters = null;
        lock (_adapterLock)
            adapters = LogInstance?.writeAdapter.ToList();
        if (adapters == null)
            return;
        foreach (ILogWrite logWrite in adapters)
            logWrite.Write(messageType, path, message);
    }

    private static string BuildMessage(object[] msg, string prefix = "")
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(prefix))
            parts.Add(prefix);
        if (msg != null)
        {
            foreach (var item in msg)
            {
                if (item == null)
                    continue;
                if (item is IEnumerable<object> enumerable)
                {
                    foreach (object node in enumerable)
                    {
                        if (node?.ToString() is string s)
                            parts.Add(s);
                    }
                }
                else if (item.ToString() is string s)
                {
                    parts.Add(s);
                }
            }
        }
        return string.Join("=>", parts);
    }

    public void WriteNew(Exception exIn, params object[] msg)
    {
        LogMesssageType logMesssageType = LogMesssageType.Exception;
        if (!Settings.Enabled || !Settings.MesssageTypes.HasFlag(logMesssageType))
            return;
        StackFrame? caller = new StackTrace(exIn, true).GetFrame(0);
        string? warning = null;
        if (caller == null)
        {
            caller = new StackTrace().GetFrame(1);
            warning = "[WARNING Exception object did not Throwed and just Created!]";
        }
        if (caller == null)
        {
            // Last resort — no stack trace available at all
            string fallbackMessage = $"{exIn}{Environment.NewLine}{BuildMessage(msg, "")}";
            writeAdapterWrite(logMesssageType, "LogError.log", fallbackMessage);
            return;
        }
        CallerContext ctx = ResolveCaller(caller);
        string path = Path.Combine(ctx.FileThatContainMethod, ctx.ClassFullName, ctx.MethodName);
        string pathException = Path.Combine("[All Exceptions]", path);
        string messageBase = warning ?? exIn.ToString();
        if (exIn.InnerException != null)
            messageBase += $"{Environment.NewLine} InnerException = {exIn.InnerException}";
        string message = BuildMessage(msg, messageBase);
        WriteNewCore(logMesssageType, path, ctx, message, pathException);
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
