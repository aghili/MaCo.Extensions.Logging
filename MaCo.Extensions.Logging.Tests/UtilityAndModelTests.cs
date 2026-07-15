using MaCo.Extensions.Logging;
using MaCo.Extensions.Logging.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaCo.Extensions.Logging.Tests;

// ── Utilites ───────────────────────────────────────────────────────────

[TestClass]
public class UtilitesTests
{
    [TestMethod]
    public void RemoveIligalPathChars_RemovesInvalidChars()
    {
        string input = "test<>:\"/\\|?*file";
        string result = Utilites.RemoveIligalPathChars(input);
        Assert.IsFalse(result.Contains("<"));
        Assert.IsFalse(result.Contains(">"));
        Assert.IsFalse(result.Contains(":"));
        Assert.IsFalse(result.Contains("\""));
    }

    [TestMethod]
    public void RemoveIligalPathChars_NullInput_ReturnsEmpty()
    {
        string result = Utilites.RemoveIligalPathChars(null!);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RemoveIligalPathChars_CleanInput_ReturnsUnchanged()
    {
        string result = Utilites.RemoveIligalPathChars("CleanName123");
        Assert.AreEqual("CleanName123", result);
    }

    [TestMethod]
    public void RemoveIligalPathChars_EmptyInput_ReturnsEmpty()
    {
        string result = Utilites.RemoveIligalPathChars("");
        Assert.AreEqual("", result);
    }
}

// ── PermissionsHelper ──────────────────────────────────────────────────

[TestClass]
public class PermissionsHelperTests
{
    [TestMethod]
    public void EnsurePermissions_CreatesDirectory()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"maco_perm_{Guid.NewGuid():N}");
        string filePath = Path.Combine(tempDir, "test.log");

        PermissionsHelper.EnsurePermissions(filePath);

        Assert.IsTrue(Directory.Exists(tempDir));

        try { Directory.Delete(tempDir, true); } catch { }
    }

    [TestMethod]
    public void EnsurePermissions_ExistingDirectory_DoesNotThrow()
    {
        string tempDir = Path.GetTempPath();
        PermissionsHelper.EnsurePermissions(Path.Combine(tempDir, "test.log"));
    }
}

// ── LogPayload ─────────────────────────────────────────────────────────

[TestClass]
public class LogPayloadTests
{
    [TestMethod]
    public void DefaultValues()
    {
        var payload = new LogPayload();
        Assert.AreEqual("event", payload.Type);
        Assert.AreEqual("event", payload.TelemetryCategory);
        Assert.AreEqual("", payload.Message);
        Assert.AreEqual("", payload.Source);
        Assert.IsTrue((DateTime.UtcNow - payload.Timestamp).TotalSeconds < 5);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        var payload = new LogPayload
        {
            Type = "error",
            TelemetryCategory = "error",
            Message = "test msg",
            Source = "test.cs",
            Timestamp = new DateTime(2024, 1, 1)
        };
        Assert.AreEqual("error", payload.Type);
        Assert.AreEqual("test msg", payload.Message);
        Assert.AreEqual("test.cs", payload.Source);
    }
}

// ── LogMessageType enum ────────────────────────────────────────────────

[TestClass]
public class LogMessageTypeTests
{
    [TestMethod]
    public void Values_AreFlags()
    {
        Assert.AreEqual(1, (int)LogMessageType.Exception);
        Assert.AreEqual(2, (int)LogMessageType.Warning);
        Assert.AreEqual(4, (int)LogMessageType.Information);
        Assert.AreEqual(8, (int)LogMessageType.DataLog);
    }

    [TestMethod]
    public void Flags_CanBeCombined()
    {
        var combined = LogMessageType.Exception | LogMessageType.Warning | LogMessageType.Information;
        Assert.IsTrue(combined.HasFlag(LogMessageType.Exception));
        Assert.IsTrue(combined.HasFlag(LogMessageType.Warning));
        Assert.IsTrue(combined.HasFlag(LogMessageType.Information));
        Assert.IsFalse(combined.HasFlag(LogMessageType.DataLog));
    }
}

// ── LogType enum ───────────────────────────────────────────────────────

[TestClass]
public class LogTypeTests
{
    [TestMethod]
    public void Values_AreFlags()
    {
        Assert.AreEqual(1, (int)LogType.File);
        Assert.AreEqual(2, (int)LogType.WindowsLogEvent);
        Assert.AreEqual(4, (int)LogType.Online);
    }

    [TestMethod]
    public void Flags_CanBeCombined()
    {
        var combined = LogType.File | LogType.Online;
        Assert.IsTrue(combined.HasFlag(LogType.File));
        Assert.IsTrue(combined.HasFlag(LogType.Online));
        Assert.IsFalse(combined.HasFlag(LogType.WindowsLogEvent));
    }
}

// ── ShrinkEventArgs ────────────────────────────────────────────────────

[TestClass]
public class ShrinkEventArgsTests
{
    [TestMethod]
    public void DefaultValues()
    {
        var args = new ShrinkEventArgs();
        Assert.AreEqual(ShrinkType.Backup, args.Type);
        Assert.AreEqual(0, args.RecordCount);
        Assert.AreEqual(0, args.NewRecordCount);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        var args = new ShrinkEventArgs
        {
            Type = ShrinkType.Resize,
            RecordCount = 100,
            NewRecordCount = 50
        };
        Assert.AreEqual(ShrinkType.Resize, args.Type);
        Assert.AreEqual(100, args.RecordCount);
        Assert.AreEqual(50, args.NewRecordCount);
    }
}

// ── ShrinkType enum ────────────────────────────────────────────────────

[TestClass]
public class ShrinkTypeTests
{
    [TestMethod]
    public void Values()
    {
        Assert.AreEqual(0, (int)ShrinkType.Backup);
        Assert.AreEqual(1, (int)ShrinkType.Resize);
    }
}

// ── WriteOption ────────────────────────────────────────────────────────

[TestClass]
public class WriteOptionTests
{
    [TestMethod]
    public void DefaultValues()
    {
        var opt = new WriteOption();
        Assert.AreEqual(80, opt.LogKeepDataOnLimitRichedPercent);
        Assert.AreEqual(20, opt.LogRowLimitPerContainer);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        var opt = new WriteOption
        {
            LogKeepDataOnLimitRichedPercent = 90,
            LogRowLimitPerContainer = 5000
        };
        Assert.AreEqual(90, opt.LogKeepDataOnLimitRichedPercent);
        Assert.AreEqual(5000, opt.LogRowLimitPerContainer);
    }
}

// ── WriteOptionEvent ───────────────────────────────────────────────────

[TestClass]
public class WriteOptionEventTests
{
    [TestMethod]
    public void DefaultValues()
    {
        var opt = new WriteOptionEvent();
        Assert.AreEqual(80, opt.LogKeepDataOnLimitRichedPercent);
        Assert.AreEqual(20, opt.LogRowLimitPerContainer);
    }

    [TestMethod]
    public void ImplementsIWriterOption()
    {
        IWriterOption opt = new WriteOptionEvent();
        Assert.AreEqual(80, opt.LogKeepDataOnLimitRichedPercent);
    }
}

// ── OnlineLoggerSettings ───────────────────────────────────────────────

[TestClass]
public class OnlineLoggerSettingsTests
{
    [TestMethod]
    public void DefaultValues()
    {
        var s = new Log.OnlineLoggerSettings();
        Assert.IsFalse(s.Enabled);
        Assert.AreEqual("", s.ApiEndpoint);
        Assert.AreEqual("", s.ApiKey);
        Assert.AreEqual(50, s.BatchSize);
        Assert.AreEqual(15, s.UploadIntervalSeconds);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        var s = new Log.OnlineLoggerSettings
        {
            Enabled = true,
            ApiEndpoint = "https://api.example.com",
            ApiKey = "key123",
            BatchSize = 100,
            UploadIntervalSeconds = 30
        };
        Assert.IsTrue(s.Enabled);
        Assert.AreEqual("https://api.example.com", s.ApiEndpoint);
        Assert.AreEqual("key123", s.ApiKey);
        Assert.AreEqual(100, s.BatchSize);
        Assert.AreEqual(30, s.UploadIntervalSeconds);
    }
}

// ── MaCoLoggerConfiguration ────────────────────────────────────────────

[TestClass]
public class MaCoLoggerConfigurationTests
{
    [TestMethod]
    public void DefaultValues()
    {
        var config = new MaCoLoggerConfiguration();
        Assert.AreEqual(0, config.EventId);
        Assert.IsTrue(config.LogLevels.Contains(LogLevel.Information));
        Assert.AreEqual(LogType.File, config.LogType);
        Assert.AreEqual(80, config.LogKeepDataOnLimitRichedPercent);
        Assert.AreEqual(10000, config.LogRowLimitPerContainer);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        var config = new MaCoLoggerConfiguration
        {
            EventId = 42,
            LogLevels = new List<LogLevel> { LogLevel.Warning, LogLevel.Error },
            LogType = LogType.Online,
            LogKeepDataOnLimitRichedPercent = 90,
            LogRowLimitPerContainer = 5000
        };
        Assert.AreEqual(42, config.EventId);
        Assert.AreEqual(2, config.LogLevels.Count);
        Assert.AreEqual(LogType.Online, config.LogType);
    }
}
