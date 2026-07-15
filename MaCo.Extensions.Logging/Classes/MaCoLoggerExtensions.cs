using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MaCo.Extensions.Logging;

/// <summary>
/// Extension methods on ILogger that capture caller information
/// and forward it to Log.Instance for accurate file/line reporting.
///
/// When the logger is a MaCoLogger, caller info (file, method, line) is captured
/// via compiler attributes and forwarded directly — no StackTrace needed.
///
/// When the logger is any other ILogger, falls back to the built-in Log() method.
/// </summary>
public static class MaCoLoggerExtensions
{
    public static void LogTrace(
        this ILogger logger,
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (logger is MaCoLogger maCo)
            maCo.LogWithCallerInfo(LogLevel.Trace, message, member, file, line);
        else
            logger.Log(LogLevel.Trace, 0, message, null, (m, e) => m);
    }

    public static void LogDebug(
        this ILogger logger,
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (logger is MaCoLogger maCo)
            maCo.LogWithCallerInfo(LogLevel.Debug, message, member, file, line);
        else
            logger.Log(LogLevel.Debug, 0, message, null, (m, e) => m);
    }

    public static void LogInformation(
        this ILogger logger,
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (logger is MaCoLogger maCo)
            maCo.LogWithCallerInfo(LogLevel.Information, message, member, file, line);
        else
            logger.Log(LogLevel.Information, 0, message, null, (m, e) => m);
    }

    public static void LogWarning(
        this ILogger logger,
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (logger is MaCoLogger maCo)
            maCo.LogWithCallerInfo(LogLevel.Warning, message, member, file, line);
        else
            logger.Log(LogLevel.Warning, 0, message, null, (m, e) => m);
    }

    public static void LogError(
        this ILogger logger,
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (logger is MaCoLogger maCo)
            maCo.LogWithCallerInfo(LogLevel.Error, message, member, file, line);
        else
            logger.Log(LogLevel.Error, 0, message, null, (m, e) => m);
    }

    public static void LogError(
        this ILogger logger,
        Exception ex,
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (logger is MaCoLogger maCo)
            maCo.LogWithCallerInfo(LogLevel.Error, $"{message} | {ex.Message}", member, file, line);
        else
            logger.Log(LogLevel.Error, 0, message, ex, (m, e) => m);
    }

    public static void LogCritical(
        this ILogger logger,
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (logger is MaCoLogger maCo)
            maCo.LogWithCallerInfo(LogLevel.Critical, message, member, file, line);
        else
            logger.Log(LogLevel.Critical, 0, message, null, (m, e) => m);
    }
}
