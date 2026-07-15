using System.ComponentModel;
using MaCo.Extensions.Logging.Classes;

// =============================================================================
// DEPRECATED COMPATIBILITY LAYER
// These types preserve the original Aghili.Logging namespace API surface.
// They will be removed in v2.0. Migrate to MaCo.Extensions.Logging.
// =============================================================================

namespace Aghili.Logging
{
    /// <summary>
    /// Deprecated. Use <see cref="MaCo.Extensions.Logging.LogMessageType"/> instead.
    /// Original namespace: Aghili.Logging
    /// </summary>
    [Obsolete("Use MaCo.Extensions.Logging.LogMessageType instead. This type will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Flags]
    public enum LogMesssageType
    {
        Exception = 1,
        Warrning = 2,
        Information = 4,
        DataLog = 8,
    }

    /// <summary>
    /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Log"/> instead.
    /// Original namespace: Aghili.Logging
    /// </summary>
    [Obsolete("Use MaCo.Extensions.Logging.Log instead. This type will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Log
    {
        /// <summary>
        /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Log.Instance"/> instead.
        /// </summary>
        [Obsolete("Use MaCo.Extensions.Logging.Log.Instance instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static MaCo.Extensions.Logging.Log Instance => MaCo.Extensions.Logging.Log.Instance;

        /// <summary>
        /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Log.Dispose"/> instead.
        /// </summary>
        [Obsolete("Use MaCo.Extensions.Logging.Log.Dispose() instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Dispose() => MaCo.Extensions.Logging.Log.Dispose();

        /// <summary>
        /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Log.Configure"/> instead.
        /// </summary>
        [Obsolete("Use MaCo.Extensions.Logging.Log.Configure() instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Configure(Microsoft.Extensions.Configuration.IConfiguration configuration, string sectionName = "Logging:MaCo")
            => MaCo.Extensions.Logging.Log.Configure(configuration, sectionName);

        /// <summary>
        /// Deprecated. Use the new API with LogMessageType instead.
        /// </summary>
        [Obsolete("Use MaCo.Extensions.Logging.Log.Instance.Warning() instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Warrning(this MaCo.Extensions.Logging.Log log, params object[] msg)
            => log.Warning(msg);
    }

    /// <summary>
    /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Log.LogSettings"/> instead.
    /// Original namespace: Aghili.Logging
    /// </summary>
    [Obsolete("Use MaCo.Extensions.Logging.Log.LogSettings instead. This type will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class LogSettingsExtensions
    {
        /// <summary>
        /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Log.LogSettings.MessageTypes"/> instead.
        /// </summary>
        [Obsolete("Use Settings.MessageTypes instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogMesssageType GetMesssageTypes(this MaCo.Extensions.Logging.Log.LogSettings settings)
            => (LogMesssageType)(int)settings.MessageTypes;

        /// <summary>
        /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Log.LogSettings.MessageTypes"/> instead.
        /// </summary>
        [Obsolete("Use Settings.MessageTypes instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetMesssageTypes(this MaCo.Extensions.Logging.Log.LogSettings settings, LogMesssageType value)
            => settings.MessageTypes = (MaCo.Extensions.Logging.LogMessageType)(int)value;
    }
}

namespace Aghili.Logging.Classes
{
    /// <summary>
    /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Classes.ShrinkEventArgs"/> instead.
    /// Original namespace: Aghili.Logging.Classes
    /// </summary>
    [Obsolete("Use MaCo.Extensions.Logging.Classes.ShrinkEventArgs instead. This type will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ShirinkEventArgs : MaCo.Extensions.Logging.Classes.ShrinkEventArgs { }

    /// <summary>
    /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Classes.ShrinkType"/> instead.
    /// Original namespace: Aghili.Logging.Classes
    /// </summary>
    [Obsolete("Use MaCo.Extensions.Logging.Classes.ShrinkType instead. This type will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum ShirinkType
    {
        Backup = 0,
        Resize = 1,
    }

    /// <summary>
    /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Classes.ILogWrite"/> instead.
    /// Original namespace: Aghili.Logging.Classes
    /// </summary>
    [Obsolete("Use MaCo.Extensions.Logging.Classes.ILogWrite instead. This interface will be removed in v2.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILogWrite : MaCo.Extensions.Logging.Classes.ILogWrite
    {
        /// <summary>
        /// Deprecated. Use <see cref="MaCo.Extensions.Logging.Classes.ILogWrite.OnShrinkRise"/> instead.
        /// </summary>
        [Obsolete("Use OnShrinkRise instead.")]
        new event EventHandler<ShirinkEventArgs>? OnShiringRise;
    }
}
