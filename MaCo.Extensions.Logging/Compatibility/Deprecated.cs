using System.ComponentModel;
using MaCo.Extensions.Logging.Classes;

namespace MaCo.Extensions.Logging;

/// <summary>
/// Deprecated. Use <see cref="LogMessageType"/> instead.
/// This type will be removed in v2.0.
/// </summary>
[Obsolete("Use LogMessageType instead. This type will be removed in v2.0.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public enum LogMesssageType
{
    Exception = 1,
    Warrning = 2,
    Information = 4,
    DataLog = 8,
}

/// <summary>
/// Deprecated. Use <see cref="ShrinkEventArgs"/> instead.
/// This type will be removed in v2.0.
/// </summary>
[Obsolete("Use ShrinkEventArgs instead. This type will be removed in v2.0.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ShirinkEventArgs : ShrinkEventArgs { }

/// <summary>
/// Deprecated. Use <see cref="ShrinkType"/> instead.
/// This type will be removed in v2.0.
/// </summary>
[Obsolete("Use ShrinkType instead. This type will be removed in v2.0.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public enum ShirinkType
{
    Backup = 0,
    Resize = 1,
}

/// <summary>
/// Extension methods providing backward-compatible deprecated API surface.
/// These will be removed in v2.0.
/// </summary>
[Obsolete("This compatibility layer will be removed in v2.0. Migrate to the new API.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DeprecatedExtensions
{
    /// <summary>
    /// Deprecated. Use <see cref="Log.Warning(object[])"/> instead.
    /// </summary>
    [Obsolete("Use Log.Warning() instead. This method will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Warrning(this Log log, params object[] msg)
        => log.Warning(msg);

    /// <summary>
    /// Deprecated. Use <see cref="Log.MessageTypes"/> instead.
    /// </summary>
    [Obsolete("Use Settings.MessageTypes instead. This property will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static LogMesssageType GetMesssageTypes(this Log.LogSettings settings)
        => (LogMesssageType)(int)settings.MessageTypes;

    /// <summary>
    /// Deprecated. Use <see cref="Log.MessageTypes"/> instead.
    /// </summary>
    [Obsolete("Use Settings.MessageTypes instead. This property will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetMesssageTypes(this Log.LogSettings settings, LogMesssageType value)
        => settings.MessageTypes = (LogMessageType)(int)value;
}
