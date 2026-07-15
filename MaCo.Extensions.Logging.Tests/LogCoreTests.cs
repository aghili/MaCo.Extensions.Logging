using MaCo.Extensions.Logging;
using MaCo.Extensions.Logging.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaCo.Extensions.Logging.Tests;

[TestClass]
public class LogCoreTests
{
    private InMemoryAdapter _adapter = null!;

    [TestInitialize]
    public void Init()
    {
        Log.Instance.Settings.Enabled = true;
        Log.Instance.Settings.MessageTypes =
            LogMessageType.Exception | LogMessageType.Warning |
            LogMessageType.Information | LogMessageType.DataLog;
        Log.Instance.writeAdapter.Clear();
        _adapter = new InMemoryAdapter();
        Log.Instance.writeAdapter.Add(_adapter);
    }

    [TestCleanup]
    public void Cleanup() => Log.Dispose();

    // ── Singleton ──────────────────────────────────────────────────────

    [TestMethod]
    public void Instance_ReturnsSameReference()
    {
        var a = Log.Instance;
        var b = Log.Instance;
        Assert.AreSame(a, b);
    }

    [TestMethod]
    public void Instance_IsNotNull()
    {
        Assert.IsNotNull(Log.Instance);
    }

    // ── Settings ───────────────────────────────────────────────────────

    [TestMethod]
    public void Settings_DefaultEnabled_IsTrue()
    {
        Assert.IsTrue(Log.Instance.Settings.Enabled);
    }

    [TestMethod]
    public void Settings_DefaultLogType_IsFile()
    {
        Assert.AreEqual(LogType.File, Log.Instance.Settings.LogType);
    }

    [TestMethod]
    public void Settings_DefaultKeepPercent_Is80()
    {
        Assert.AreEqual(80, Log.Instance.Settings.LogKeepDataOnLimitRichedPercent);
    }

    [TestMethod]
    public void Settings_DefaultRowLimit_Is10000()
    {
        Assert.AreEqual(10000, Log.Instance.Settings.LogRowLimitPerContainer);
    }

    [TestMethod]
    public void Settings_DefaultMessageTypes_HasExceptionAndWarning()
    {
        var s = new Log.LogSettings();
        Assert.IsTrue(s.MessageTypes.HasFlag(LogMessageType.Exception));
        Assert.IsTrue(s.MessageTypes.HasFlag(LogMessageType.Warning));
    }

    [TestMethod]
    public void Settings_Online_Defaults()
    {
        var s = new Log.LogSettings();
        Assert.IsFalse(s.Online.Enabled);
        Assert.AreEqual("", s.Online.ApiEndpoint);
        Assert.AreEqual("", s.Online.ApiKey);
        Assert.AreEqual(50, s.Online.BatchSize);
        Assert.AreEqual(15, s.Online.UploadIntervalSeconds);
    }

    // ── WriteNew(LogMessageType, params) ───────────────────────────────

    [TestMethod]
    public void WriteNew_MessageType_Information_WritesEntry()
    {
        Log.Instance.WriteNew(LogMessageType.Information, "info msg");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("info msg")));
    }

    [TestMethod]
    public void WriteNew_MessageType_Warning_WritesEntry()
    {
        Log.Instance.WriteNew(LogMessageType.Warning, "warn msg");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("warn msg")));
    }

    [TestMethod]
    public void WriteNew_MessageType_DataLog_WritesEntry()
    {
        Log.Instance.WriteNew(LogMessageType.DataLog, "data msg");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("data msg")));
    }

    [TestMethod]
    public void WriteNew_MessageType_Exception_WritesEntry()
    {
        Log.Instance.WriteNew(LogMessageType.Exception, "exc msg");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("exc msg")));
    }

    [TestMethod]
    public void WriteNew_MultipleArgs_ConcatenatedWithArrow()
    {
        Log.Instance.WriteNew(LogMessageType.Information, "a", "b", "c");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("a=>b=>c")));
    }

    [TestMethod]
    public void WriteNew_NullArgs_Skipped()
    {
        Log.Instance.WriteNew(LogMessageType.Information, "a", null!, "b");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("a=>b")));
    }

    // ── WriteNew(LogMessageType, object[], caller info) ────────────────

    [TestMethod]
    public void WriteNew_WithCallerInfo_WritesEntry()
    {
        Log.Instance.WriteNew(LogMessageType.Information, new object[] { "caller test" });
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("caller test")));
    }

    // ── WriteNew(LogLevel, params) ─────────────────────────────────────

    [TestMethod]
    public void WriteNew_LogLevel_Information_WritesEntry()
    {
        Log.Instance.WriteNew(LogLevel.Information, "level info");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("level info")));
    }

    [TestMethod]
    public void WriteNew_LogLevel_Warning_WritesEntry()
    {
        Log.Instance.WriteNew(LogLevel.Warning, "level warn");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("level warn")));
    }

    [TestMethod]
    public void WriteNew_LogLevel_Error_WritesEntry()
    {
        Log.Instance.WriteNew(LogLevel.Error, "level error");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("level error")));
    }

    [TestMethod]
    public void WriteNew_LogLevel_Critical_WritesEntry()
    {
        Log.Instance.WriteNew(LogLevel.Critical, "level critical");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("level critical")));
    }

    [TestMethod]
    public void WriteNew_LogLevel_Debug_MapsToInformation()
    {
        Log.Instance.WriteNew(LogLevel.Debug, "level debug");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("level debug")));
    }

    [TestMethod]
    public void WriteNew_LogLevel_Trace_MapsToInformation()
    {
        Log.Instance.WriteNew(LogLevel.Trace, "level trace");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("level trace")));
    }

    // ── WriteNew(LogLevel, object[], caller info) ──────────────────────

    [TestMethod]
    public void WriteNew_LogLevel_WithCallerInfo_WritesEntry()
    {
        Log.Instance.WriteNew(LogLevel.Information, new object[] { "caller level" });
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("caller level")));
    }

    // ── WriteNew<TState> (ILogger bridge) ──────────────────────────────

    [TestMethod]
    public void WriteNew_TState_FormatsMessage()
    {
        Log.Instance.WriteNew(LogLevel.Information, 0, "hello {Name}", null,
            (s, e) => $"FORMATTED{s}");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("FORMATTEDhello {Name}")));
    }

    [TestMethod]
    public void WriteNew_TState_WithException_FormatsMessage()
    {
        var ex = new InvalidOperationException("test");
        Log.Instance.WriteNew(LogLevel.Error, 0, "state", ex,
            (s, e) => $"ERR:{e!.Message}");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("ERR:test")));
    }

    [TestMethod]
    public void WriteNew_TState_WithCallerInfo_FormatsMessage()
    {
        Log.Instance.WriteNew(LogLevel.Information, 0, "state2", null,
            (s, e) => $"CAL{s}",
            "MyMethod", "/tmp/file.cs", 42);
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("CALstate2")));
    }

    // ── WriteNew(Exception, params) ────────────────────────────────────

    [TestMethod]
    public void WriteNew_Exception_WithMessage()
    {
        try { throw new Exception("boom"); }
        catch (Exception ex) { Log.Instance.WriteNew(ex, "ctx"); }

        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("boom")));
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("ctx")));
    }

    [TestMethod]
    public void WriteNew_Exception_WithoutMessage()
    {
        try { throw new Exception("solo"); }
        catch (Exception ex) { Log.Instance.WriteNew(ex); }

        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("solo")));
    }

    [TestMethod]
    public void WriteNew_Exception_WithInnerException()
    {
        try
        {
            try { throw new ArgumentException("inner"); }
            catch (Exception inner) { throw new Exception("outer", inner); }
        }
        catch (Exception ex) { Log.Instance.WriteNew(ex); }

        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("outer")));
    }

    // ── Convenience methods ────────────────────────────────────────────

    [TestMethod]
    public void Warning_WritesEntry()
    {
        Log.Instance.Warning("w1");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("w1")));
    }

    [TestMethod]
    public void Information_WritesEntry()
    {
        Log.Instance.Information("i1");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("i1")));
    }

    [TestMethod]
    public void Exception_WritesEntry()
    {
        Log.Instance.Exception("e1");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("e1")));
    }

    [TestMethod]
    public void DataLog_WritesEntry()
    {
        Log.Instance.DataLog("d1");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("d1")));
    }

    [TestMethod]
    public void Exception_Overload_WritesEntry()
    {
        try { throw new Exception("ov"); }
        catch (Exception ex) { Log.Instance.Exception(ex, "ctx"); }

        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("ov")));
    }

    // ── Disabled logging ───────────────────────────────────────────────

    [TestMethod]
    public void WriteNew_Disabled_DoesNotWrite()
    {
        Log.Instance.Settings.Enabled = false;
        Log.Instance.WriteNew(LogMessageType.Information, "should not appear");
        Assert.IsFalse(_adapter.Entries.Any(e => e.Contains("should not appear")));
    }

    [TestMethod]
    public void WriteNew_MessageTypeNotEnabled_DoesNotWrite()
    {
        Log.Instance.Settings.MessageTypes = LogMessageType.Exception;
        Log.Instance.WriteNew(LogMessageType.Information, "filtered");
        Assert.IsFalse(_adapter.Entries.Any(e => e.Contains("filtered")));
    }

    // ── Configure ──────────────────────────────────────────────────────

    [TestMethod]
    public void Configure_NullConfig_Throws()
    {
        Assert.ThrowsException<ArgumentNullException>(() => Log.Configure(null!));
    }

    [TestMethod]
    public void Configure_WithSection_BindsSettings()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:MaCo:Enabled"] = "false",
                ["Logging:MaCo:LogType"] = "File"
            })
            .Build();

        Log.Configure(config);
        Assert.IsFalse(Log.Instance.Settings.Enabled);
    }

    [TestMethod]
    public void Configure_NonExistentSection_DoesNotThrow()
    {
        var config = new ConfigurationBuilder().Build();
        Log.Configure(config, "NonExistent:Section");
        // Should not throw
    }

    // ── Dispose ────────────────────────────────────────────────────────

    [TestMethod]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        Log.Dispose();
        Log.Dispose();
        // Should not throw
    }

    // ── IEnumerable detail joining ──────────────────────────────────────

    [TestMethod]
    public void WriteNew_IEnumerableDetail_JoinedWithArrow()
    {
        var details = new object[] { "x", "y", "z" };
        Log.Instance.WriteNew(LogMessageType.Information, "pre", details);
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("pre=>x=>y=>z")));
    }

    // ── Caller context in path ─────────────────────────────────────────

    [TestMethod]
    public void WriteNew_IncludesCallerInfoInPath()
    {
        Log.Instance.WriteNew(LogMessageType.Information, "ctx test");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("LogCoreTests")));
    }
}
