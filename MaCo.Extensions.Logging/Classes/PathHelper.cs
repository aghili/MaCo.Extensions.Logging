using System.Reflection;

namespace MaCo.Extensions.Logging.Classes;

internal static class PathHelper
{
    public static string ResolveExecPath()
    {
        string? baseDir = null;
        try { baseDir = AppContext.BaseDirectory; } catch { }
        if (string.IsNullOrEmpty(baseDir))
        {
            try { baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } catch { }
        }
        if (string.IsNullOrEmpty(baseDir))
        {
            try { baseDir = Environment.CurrentDirectory; } catch { }
        }
        if (string.IsNullOrEmpty(baseDir))
        {
            try { baseDir = Path.GetTempPath(); } catch { }
        }
        return baseDir ?? "";
    }
}
