using System.IO;

namespace MaCo.Extensions.Logging.Classes;

/// <summary>
/// Ensures the directory for a log file exists. It deliberately does NOT modify
/// ACLs: granting WorldSid FullControl (the previous behaviour) made log files
/// world-readable/writable, which is a security risk. The directory is created
/// by the running process, which already owns it.
/// </summary>
public static class PermissionsHelper
{
    public static void EnsurePermissions(string path)
    {
        try
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory!);
        }
        catch
        {
            // Best-effort: if we cannot create the directory the subsequent
            // write will surface the real error to the caller.
        }
    }
}
